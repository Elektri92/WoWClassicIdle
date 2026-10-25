// Debug/SimTalentOptimizer.cs – wird nur im DEBUG-Build kompiliert (siehe .csproj)
namespace DebugTools;

using Models;

/// <summary>
/// Ein investierter Talentpunkt mit gemessenem Effekt: <see cref="GainPct"/> = XP/h gegenüber
/// dem Stand ohne diesen Punkt (0.05 = +5 %). NaN = nicht gemessen (einzige Wahl ohne neue Fähigkeit).
/// </summary>
public sealed record TalentPick(int Level, string TalentId, string Name, int Rank, int MaxRanks, double GainPct);

/// <summary>
/// Verteilt Talentpunkte empirisch: Für jeden Punkt wird jedes investierbare Talent in einem Probe-Kampf getestet
/// und das mit den meisten XP pro Stunde genommen. Talente, die Fähigkeiten freischalten,
/// werden mit dem besten Loadout inklusive der neuen Fähigkeit gemessen.
/// </summary>
public static class SimTalentOptimizer
{
    /// <summary>
    /// Investiert alle verfügbaren Punkte in <paramref name="talents"/>. Die Reihenfolge der Liste entscheidet
    /// nur bei Gleichstand (z. B. Talente ohne messbaren Effekt wie Manakosten-Reduktionen).
    /// Schaltet das gewählte Talent eine Fähigkeit frei, wird der dazu gemessene Loadout übernommen.
    /// </summary>
    /// <returns>Die in diesem Aufruf investierten Punkte in Reihenfolge.</returns>
    public static List<TalentPick> InvestPoints(Player player, IReadOnlyList<TalentDefinition> talents, int specIndex, CancellationToken ct = default)
    {
        var cls   = player.ActiveClass;
        var picks = new List<TalentPick>();
        int available;
        while ((available = player.GetAvailableTalentPoints(cls)) > 0)
        {
            var candidates = talents.Where(t => player.TalentState.CanInvest(t, available)).ToList();
            if (candidates.Count == 0) break;

            TalentDefinition best        = candidates[0];
            List<string>?    bestLoadout = null;
            double           bestScore   = double.MinValue;
            // Nur eine Wahl und keine neue Fähigkeit → Messung überflüssig
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

    /// <summary>XP pro Sekunde mit einem zusätzlichen Rang in <paramref name="talent"/> und der dafür beste Loadout.</summary>
    private static (double Score, List<string> Loadout) Measure(Player player, TalentDefinition talent, int specIndex, CancellationToken ct)
    {
        var current = player.GetCurrentLoadout().EquippedAbilityIds.ToList();
        var trials  = new List<List<string>> { current };

        var granted = talent.GrantedAbilityIds
            .Select(ClassAbilityRegistry.FindById)
            .OfType<Ability>()
            .Where(a => a.LevelRequired <= player.Level)
            .ToList();
        if (granted.Count > 0)
        {
            // Neue Fähigkeit in einen freien Slot bzw. anstelle jeder bisherigen ausprobieren
            foreach (var ability in granted.Where(a => !a.IsForm))
            {
                if (current.Count < ClassAbilityRegistry.MaxSlotsPerClass)
                    trials.Add(current.Append(ability.Id).ToList());
                else
                    for (int i = 0; i < current.Count; i++)
                        trials.Add(current.Select((id, j) => j == i ? ability.Id : id).ToList());
            }

            // Heuristik-Loadout mit dem neuen Talent – deckt Gestalten ab (Bear Form + Maul)
            var unlocked = new HashSet<string>(ClassSimulator.GetUnlockedTalentAbilityIds(player, player.ActiveClass));
            unlocked.UnionWith(talent.GrantedAbilityIds);
            trials.Add(ClassAbilityRegistry.GetDefaultLoadoutIds(player.ActiveClass, player.Level, specIndex, unlocked));
        }

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
}
