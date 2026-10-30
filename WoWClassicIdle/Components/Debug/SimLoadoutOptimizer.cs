// Debug/SimLoadoutOptimizer.cs – only compiled in the DEBUG build (see .csproj)
namespace DebugTools;

using Models;

/// <summary>
/// Improves the simulator loadout empirically: instead of a fixed heuristic, abilities
/// are swapped in trial fights and kept if they yield more XP per hour.
/// XP/h weighs damage, survival (resurrection time, lost enemies, death XP penalty) and mana (idle time) together.
/// </summary>
public static class SimLoadoutOptimizer
{
    /// <summary>
    /// Simulated seconds per trial fight – long enough for mana shortage and deaths to show up
    /// (at 4 minutes, mana and survival talents consistently showed 0 %).
    /// </summary>
    private const double TrialSeconds = 600;

    /// <summary>A swap must be at least this much better so random noise doesn't trigger changes.</summary>
    private const double MinImprovement = 1.03;

    private const int MaxPasses = 2;

    /// <summary>
    /// Each trial runs with these fixed seeds and is averaged: same enemies and rolls for all variants,
    /// but no lucky outlier of a single fight (a single seed yielded e.g. +13.8 % for +2 % haste).
    /// </summary>
    private static readonly int[] TrialSeeds = [1337, 7331, 4242];

    /// <summary>
    /// XP rate after the death penalty. Trials run far shorter than a level, so the penalty cannot occur in them;
    /// it is derived from the measured rates instead. With a share p of the current XP lost per death,
    /// XP follows dX/dt = r − p·d·X, which gives the time to reach the XP needed for the level.
    /// Where X never gets there (x ≥ 1) the score keeps falling, so hopeless variants can still be ranked.
    /// </summary>
    internal static double NetXpRate(double xpPerSecond, double deathsPerSecond, int level)
    {
        int needed = StaticHelpers.GetXpForLevel(level);
        if (level < Player.DeathXpPenaltyMinLevel || needed <= 0 || deathsPerSecond <= 0 || xpPerSecond <= 0)
            return xpPerSecond;

        const double Limit = 0.95;
        double x = Player.DeathXpPenaltyShare * deathsPerSecond * needed / xpPerSecond;
        double factor = x < Limit
            ? x / -Math.Log(1 - x)
            : Limit / -Math.Log(1 - Limit) * (Limit / x);
        return xpPerSecond * factor;
    }

    /// <summary>
    /// Candidate pool of the player: per ability the highest available rank (incl. talent abilities),
    /// without forms – they stay fixed from <paramref name="seedIds"/>. Also serves as cache key.
    /// </summary>
    public static IReadOnlyList<Ability> CandidatePool(Player player, IReadOnlyList<string> seedIds)
    {
        var cls      = player.ActiveClass;
        var unlocked = ClassSimulator.GetUnlockedTalentAbilityIds(player, cls);
        var form     = ShapeshiftForms.FormOf(seedIds.Select(ClassAbilityRegistry.FindById).OfType<Ability>());

        return ClassAbilityRegistry.GetAbilitiesFor(cls)
            .Where(a => a.LevelRequired <= player.Level)
            .Where(a => !a.TalentUnlockOnly || unlocked.Contains(a.Id))
            .Where(a => !a.IsForm && ShapeshiftForms.IsUsableIn(a, form))
            .GroupBy(a => a.Name)
            .Select(g => g.OrderByDescending(a => a.Rank).First())
            .ToList();
    }

    /// <summary>
    /// Talent forms (Shadowform, Moonkin, Tree, Bear, Cat) are always equipped in the simulator:
    /// if the loadout lacks a form, the unlocked one is inserted at the front and the last slot is pushed out.
    /// </summary>
    public static List<string> WithTalentForm(Player player, IReadOnlyList<string> ids)
    {
        var result = ids.ToList();
        if (result.Any(id => ClassAbilityRegistry.FindById(id) is { IsForm: true })) return result;

        var form = ClassSimulator.GetUnlockedTalentAbilityIds(player, player.ActiveClass)
            .Select(ClassAbilityRegistry.FindById)
            .FirstOrDefault(a => a is { IsForm: true } && a.LevelRequired <= player.Level);
        if (form is null) return result;

        result.Insert(0, form.Id);
        // Abilities unusable in Rage/Energy forms are dropped
        result = result.Where(id => ClassAbilityRegistry.FindById(id) is { } a && ShapeshiftForms.IsUsableIn(a, form.Form)).ToList();
        return result.Take(ClassAbilityRegistry.MaxSlotsPerClass).ToList();
    }

    /// <summary>Readable key of the pool – if it doesn't change, no re-optimization is needed.</summary>
    public static string PoolKey(IReadOnlyList<Ability> pool) =>
        string.Join(",", pool.Select(a => a.Id).OrderBy(id => id));

    /// <summary>
    /// Starts at the heuristic loadout <paramref name="seedIds"/> and swaps abilities for pool candidates
    /// as long as a swap noticeably increases XP per hour (hill climbing, first hit wins).
    /// </summary>
    public static List<string> Optimize(Player player, IReadOnlyList<string> seedIds, CancellationToken ct = default)
    {
        seedIds   = WithTalentForm(player, seedIds);
        var pool  = CandidatePool(player, seedIds);
        var fixedIds = seedIds.Where(id => ClassAbilityRegistry.FindById(id) is { IsForm: true }).ToList();
        // Raise equipped abilities to the highest available rank; abilities no longer usable are dropped
        var current  = seedIds.Except(fixedIds)
            .Select(id => ClassAbilityRegistry.FindById(id) is { } a ? pool.FirstOrDefault(c => c.Name == a.Name)?.Id : null)
            .OfType<string>()
            .Distinct()
            .Take(ClassAbilityRegistry.MaxSlotsPerClass - fixedIds.Count)
            .ToList();
        int freeSlots = ClassAbilityRegistry.MaxSlotsPerClass - fixedIds.Count;

        double best = Evaluate(player, fixedIds.Concat(current).ToList(), ct);

        for (int pass = 0; pass < MaxPasses; pass++)
        {
            bool improved = false;
            foreach (var candidate in pool.Where(c => !current.Contains(c.Id)))
            {
                // Free slot: simply add, otherwise try every occupied slot as swap target
                var trials = current.Count < freeSlots
                    ? [current.Append(candidate.Id).ToList()]
                    : Enumerable.Range(0, current.Count)
                        .Select(i => current.Select((id, j) => j == i ? candidate.Id : id).ToList())
                        .ToList();

                List<string>? bestTrial = null;
                double bestTrialScore = best * MinImprovement;
                foreach (var trial in trials.Where(t => !HasExclusiveConflict(t)))
                {
                    double score = Evaluate(player, fixedIds.Concat(trial).ToList(), ct);
                    if (score > bestTrialScore)
                    {
                        bestTrial      = trial;
                        bestTrialScore = score;
                    }
                }

                if (bestTrial is null) continue;
                current  = bestTrial;
                best     = bestTrialScore;
                improved = true;
            }
            if (!improved) break;
        }

        return fixedIds.Concat(current).ToList();
    }

    /// <summary>Two abilities of the same ExclusiveGroup (e.g. two Seals or Pets) are not allowed.</summary>
    internal static bool HasExclusiveConflict(IEnumerable<string> ids) =>
        ids.Select(ClassAbilityRegistry.FindById)
            .Where(a => a is not null && !string.IsNullOrEmpty(a.ExclusiveGroup))
            .GroupBy(a => a!.ExclusiveGroup)
            .Any(g => g.Count() > 1);

    /// <summary>
    /// Trial fight with a fresh engine that takes over level, talents and gear from <paramref name="template"/>.
    /// Result: XP per simulated second. The damaged last enemy counts proportionally to the HP removed,
    /// so the value doesn't jump in whole kills.
    /// <paramref name="extraTalent"/> is additionally raised by one rank (talent trial).
    /// </summary>
    internal static double Evaluate(Player template, List<string> ids, CancellationToken ct, TalentDefinition? extraTalent = null)
    {
        // Rates over all seeds together: a single trial is too short for its deaths to mean much on their own
        var trials = TrialSeeds.Select(seed => EvaluateOnce(template, ids, seed, ct, extraTalent)).ToList();
        return NetXpRate(trials.Average(t => t.XpPerSecond), trials.Average(t => t.DeathsPerSecond), template.Level);
    }

    private static (double XpPerSecond, double DeathsPerSecond) EvaluateOnce(Player template, List<string> ids, int seed, CancellationToken ct, TalentDefinition? extraTalent)
    {
        var cls   = template.ActiveClass;
        int level = template.Level;

        var engine = new IdleGameEngine(seed: seed);
        engine.SetCharacter(RaceClassInfo.RaceToFaction[template.Race], template.Race, cls, Gender.Male);
        engine.EnableRareRequeue = false;
        engine.DisableGearDrops  = true;

        var player   = engine.Player;
        var progress = player.ClassProgress[cls];
        progress.SetLevel(level);
        player.SetActiveClass(cls);
        var talents = new Dictionary<string, int>(template.TalentState.Invested);
        if (extraTalent is not null)
            talents[extraTalent.Id] = talents.GetValueOrDefault(extraTalent.Id) + 1;
        player.RestoreTalentPoints(talents);
        foreach (var armor in template.EquippedArmor.Values)
            player.ForceEquipArmor(armor);
        if (template.EquippedWeapon is { } weapon)
            player.ForceEquipWeapon(weapon);
        player.ApplyLoadout(new ClassLoadout { Class = cls, EquippedAbilityIds = ids });

        engine.Meter.Reset();
        engine.CurrentEnemy = null;

        long xpGained = 0;
        int  steps    = (int)Math.Round(TrialSeconds / engine.StepSeconds);
        for (int i = 0; i < steps; i++)
        {
            if ((i & 1023) == 0) ct.ThrowIfCancellationRequested();
            try
            {
                engine.RunSteps(1);
            }
            catch (InvalidOperationException)
            {
                // Spawner has no template for the rolled enemy level
            }

            // Freeze level so all trials run under the same conditions
            if (progress.Xp > 0 || progress.Level != level)
            {
                xpGained += progress.Xp;
                progress.SetLevel(level);
            }
        }

        double partialXp = engine.CurrentEnemy is { IsAlive: true } enemy && enemy.Stats.MaxHp > 0
            ? StaticHelpers.GetEnemyXpReward(enemy.Stats.Level) * (1.0 - (double)enemy.CurrentHp / enemy.Stats.MaxHp)
            : 0;
        double seconds = steps * engine.StepSeconds;
        return ((xpGained + partialXp) / seconds, engine.TotalDeaths / seconds);
    }
}
