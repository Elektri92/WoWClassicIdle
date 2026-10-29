// Debug/SimTalentOptimizer.cs – only compiled in the DEBUG build (see .csproj)
namespace DebugTools;

using Models;

/// <summary>
/// An invested talent point with measured effect: <see cref="GainPct"/> = XP/h compared to
/// the state without this point (0.05 = +5 %). NaN = not measured (only choice without a new ability).
/// </summary>
public sealed record TalentPick(int Level, string TalentId, string Name, int Rank, int MaxRanks, double GainPct);

/// <summary>
/// Distributes talent points empirically: for each point every investable talent is tested in a trial fight
/// and the one with the most XP per hour is taken. Talents that unlock abilities
/// are measured with the best loadout including the new ability.
/// </summary>
public static class SimTalentOptimizer
{
    /// <summary>
    /// Invests all available points into <paramref name="talents"/>. The order of the list only decides
    /// on ties (e.g. talents without measurable effect such as mana cost reductions).
    /// If the chosen talent unlocks an ability, the loadout measured for it is adopted.
    /// </summary>
    /// <returns>The points invested in this call, in order.</returns>
    public static List<TalentPick> InvestPoints(Player player, IReadOnlyList<TalentDefinition> talents, int specIndex, CancellationToken ct = default)
    {
        var cls   = player.ActiveClass;
        var picks = new List<TalentPick>();
        int available;
        while ((available = player.GetAvailableTalentPoints(cls)) > 0)
        {
            var candidates = talents.Where(t => player.TalentState.CanInvest(t, available, player.Level)).ToList();
            if (candidates.Count == 0) break;

            TalentDefinition best        = candidates[0];
            List<string>?    bestLoadout = null;
            double           bestScore   = double.MinValue;
            // Only one choice and no new ability → measuring is unnecessary
            bool measure = candidates.Count > 1 || candidates[0].GrantedAbilityIds.Any();
            double baseline = measure
                ? SimLoadoutOptimizer.Evaluate(player, player.GetCurrentLoadout().EquippedAbilityIds.ToList(), ct)
                : double.NaN;
            foreach (var talent in measure ? candidates : [])
            {

                var (score, loadout) = Measure(player, talent, specIndex, ct);
                if (score > bestScore)
                {
                    best        = talent;
                    bestLoadout = loadout;
                    bestScore   = score;
                }
            }

            player.InvestTalent(best);
            if (bestLoadout is not null && best.GrantedAbilityIds.Any())
                player.ApplyLoadout(new ClassLoadout { Class = cls, EquippedAbilityIds = bestLoadout });

            double gain = measure && baseline > 0 ? bestScore / baseline - 1.0 : double.NaN;
            picks.Add(new TalentPick(player.Level, best.Id, best.Name, player.TalentState.GetRanks(best.Id), best.MaxRanks, gain));
        }
        return picks;
    }

    /// <summary>XP per second with one additional rank in <paramref name="talent"/> and the best loadout for it.</summary>
    private static (double Score, List<string> Loadout) Measure(Player player, TalentDefinition talent, int specIndex, CancellationToken ct)
    {
        var current = player.GetCurrentLoadout().EquippedAbilityIds.ToList();
        var trials  = new List<List<string>> { current };

        // Form talents (Shadowform, Moonkin …): the form is mandatory, only which slot gives way is measured
        var formGrant = talent.GrantedAbilityIds.Select(ClassAbilityRegistry.FindById)
            .FirstOrDefault(a => a is { IsForm: true } && a.LevelRequired <= player.Level);
        if (formGrant is not null && current.Count >= ClassAbilityRegistry.MaxSlotsPerClass)
            trials.AddRange(Enumerable.Range(0, current.Count)
                .Select(i => current.Select((id, j) => j == i ? formGrant.Id : id).ToList()));

        var granted = talent.GrantedAbilityIds
            .Select(ClassAbilityRegistry.FindById)
            .OfType<Ability>()
            .Where(a => a.LevelRequired <= player.Level)
            .ToList();
        if (granted.Count > 0)
        {
            // Try the new ability in a free slot or in place of each existing one
            foreach (var ability in granted.Where(a => !a.IsForm))
            {
                if (current.Count < ClassAbilityRegistry.MaxSlotsPerClass)
                    trials.Add(current.Append(ability.Id).ToList());
                else
                    for (int i = 0; i < current.Count; i++)
                        trials.Add(current.Select((id, j) => j == i ? ability.Id : id).ToList());
            }

            // Heuristic loadout with the new talent – covers forms (Bear Form + Maul)
            var unlocked = new HashSet<string>(ClassSimulator.GetUnlockedTalentAbilityIds(player, player.ActiveClass));
            unlocked.UnionWith(talent.GrantedAbilityIds);
            trials.Add(ClassAbilityRegistry.GetDefaultLoadoutIds(player.ActiveClass, player.Level, specIndex, unlocked));
        }

        if (formGrant is not null)
            trials = trials.Select(t => WithForm(t, formGrant)).ToList();

        double       bestScore = double.MinValue;
        List<string> best      = current;
        foreach (var trial in trials.Where(t => !SimLoadoutOptimizer.HasExclusiveConflict(t)))
        {
            double score = SimLoadoutOptimizer.Evaluate(player, trial, ct, talent);
            if (score > bestScore)
            {
                bestScore = score;
                best      = trial;
            }
        }
        return (bestScore, best);
    }

    /// <summary>Ensures <paramref name="form"/> is in the loadout; abilities unusable in the form are dropped.</summary>
    private static List<string> WithForm(List<string> ids, Ability form)
    {
        var result = ids.Contains(form.Id) ? ids.ToList() : ids.Prepend(form.Id).ToList();
        return result
            .Where(id => ClassAbilityRegistry.FindById(id) is { } a && ShapeshiftForms.IsUsableIn(a, form.Form))
            .Take(ClassAbilityRegistry.MaxSlotsPerClass)
            .ToList();
    }
}
