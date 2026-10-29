// Debug/ClassSimulator.cs – only compiled in the DEBUG build (see .csproj)
namespace DebugTools;

using Models;

/// <summary>Result of one simulation run (one class at a fixed level).</summary>
public sealed record SimulationResult(
    CharacterClass Class,
    int Level,
    double SimSeconds,
    double CombatSeconds,
    long TotalDamage,
    long AutoAttackDamage,
    long TotalHealing,
    long DamageTaken,
    int Kills,
    int Deaths,
    int SpawnErrors,
    long XpGained,
    long CopperGained,
    Stats Stats,
    IReadOnlyList<MeterRow> Breakdown,
    WeaponItem? EquippedWeapon,
    IReadOnlyDictionary<ArmorSlot, ArmorItem> EquippedArmor,
    int SpecIndex,
    string SpecName,
    string TalentSummary,
    IReadOnlyList<string> LoadoutIds,
    IReadOnlyList<TalentPick> TalentPicks)
{
    private double Hours => SimSeconds / 3600.0;

    public double Dps          => CombatSeconds > 0 ? TotalDamage / CombatSeconds : 0;
    public double AutoDps      => CombatSeconds > 0 ? AutoAttackDamage / CombatSeconds : 0;
    public double AbilityDps   => CombatSeconds > 0 ? (TotalDamage - AutoAttackDamage) / CombatSeconds : 0;
    public double Hps          => CombatSeconds > 0 ? TotalHealing / CombatSeconds : 0;
    public double Dtps         => CombatSeconds > 0 ? DamageTaken / CombatSeconds : 0;
    public double KillsPerMin  => SimSeconds > 0 ? Kills / (SimSeconds / 60.0) : 0;
    public double AvgTtk       => Kills > 0 ? CombatSeconds / Kills : double.NaN;
    public double DeathsPerHour => Hours > 0 ? Deaths / Hours : 0;
    public double KillsPerDeath => Deaths > 0 ? (double)Kills / Deaths : double.PositiveInfinity;
    public double XpPerHour    => Hours > 0 ? XpGained / Hours : 0;
    public double CopperPerHour => Hours > 0 ? CopperGained / Hours : 0;

    /// <summary>Play time until the next level in minutes (NaN = no XP gain / max level).</summary>
    public double MinutesToLevel
    {
        get
        {
            int needed = StaticHelpers.GetXpForLevel(Level);
            return needed > 0 && XpPerHour > 0 ? needed / XpPerHour * 60.0 : double.NaN;
        }
    }
}

public sealed record SimulationSettings(
    int LevelFrom,
    int LevelTo,
    double MinutesPerRun,
    IReadOnlyList<CharacterClass> Classes);

/// <summary>
/// Simulated spec variant: talent tree plus an optionally fixed Druid form.
/// <see cref="Index"/> is unique per class (Druid: 4 variants), <see cref="TreeIndex"/> is the talent tree.
/// </summary>
public sealed record SimSpec(int Index, int TreeIndex, string Name, DruidForm? Form = null);

/// <summary>Point in time at which a level was reached during a leveling run.</summary>
public sealed record LevelTiming(int Level, double SimSecondsToReach);

/// <summary>Loadout phase: which abilities were equipped from LevelFrom to LevelTo.</summary>
public sealed record LoadoutSnapshot(int LevelFrom, int LevelTo, IReadOnlyList<string> AbilityIds);

/// <summary>Result of a complete leveling run from LevelFrom to LevelTo for one class.</summary>
public sealed record LevelingResult(
    CharacterClass Class,
    int LevelFrom,
    int LevelTo,
    int FinalLevel,
    double SimSecondsTotal,
    long TotalDamage,
    long AutoAttackDamage,
    long TotalHealing,
    long DamageTaken,
    int TotalKills,
    int TotalDeaths,
    int SpawnErrors,
    long TotalXpGained,
    long TotalCopperGained,
    IReadOnlyList<LevelTiming> LevelTimings,
    IReadOnlyList<MeterRow> FinalBreakdown,
    IReadOnlyList<LoadoutSnapshot> LoadoutHistory,
    string TalentSummary,
    WeaponItem? FinalWeapon,
    IReadOnlyDictionary<ArmorSlot, ArmorItem> FinalArmor,
    int SpecIndex,
    string SpecName,
    IReadOnlyList<TalentPick> TalentPicks)
{
    public bool ReachedTarget => FinalLevel >= LevelTo;

    /// <summary>Simulated seconds until the target level. NaN if the target was not reached.</summary>
    public double SimSecondsToTarget
    {
        get
        {
            var t = LevelTimings.LastOrDefault(x => x.Level == LevelTo);
            return t is not null ? t.SimSecondsToReach : double.NaN;
        }
    }

    private double EffectiveHours => (ReachedTarget ? SimSecondsToTarget : SimSecondsTotal) / 3600.0;

    public double Dps           => SimSecondsTotal > 0 ? (double)TotalDamage / SimSecondsTotal : 0;
    public double KillsPerMin   => SimSecondsTotal > 0 ? TotalKills / (SimSecondsTotal / 60.0) : 0;
    public double DeathsPerHour => EffectiveHours > 0 ? TotalDeaths / EffectiveHours : 0;
    public double CopperPerHour => EffectiveHours > 0 ? TotalCopperGained / EffectiveHours : 0;
}

/// <summary>
/// Simulates classes headless with their own <see cref="IdleGameEngine"/> instances –
/// exactly the same combat logic as in the game, the running game stays untouched.
/// </summary>
public static class ClassSimulator
{
    /// <summary>
    /// Spec variants of a class to simulate: one per talent tree,
    /// for the Druid Feral is additionally split into Bear and Cat (4 variants).
    /// </summary>
    public static IReadOnlyList<SimSpec> SpecsFor(CharacterClass cls)
    {
        var trees = TalentRegistry.GetTreeNames(cls);
        if (cls == CharacterClass.Druid)
            return
            [
                new(0, 0, trees[0]),
                new(1, 1, $"{trees[1]} (Bear)", DruidForm.Bear),
                new(2, 1, $"{trees[1]} (Cat)",  DruidForm.Cat),
                new(3, 2, trees[2]),
            ];
        return Enumerable.Range(0, 3).Select(i => new SimSpec(i, i, trees[i])).ToList();
    }

    /// <summary>Number of benchmark runs for the given settings (all specs per class and level).</summary>
    public static int RunCount(SimulationSettings s) =>
        s.Classes.Sum(c => SpecsFor(c).Count) * Math.Max(0, s.LevelTo - s.LevelFrom + 1);

    /// <summary>Number of leveling runs (all specs per class).</summary>
    public static int LevelingRunCount(SimulationSettings s) => s.Classes.Sum(c => SpecsFor(c).Count);

    /// <summary>Readable label of a leveling run (for the progress display).</summary>
    public static string LevelingLabel(CharacterClass cls, string specName) => $"{cls} · {specName}";

    // Each run has its own engine (own RNG, own state); the static registries
    // are only filled in static constructors → runs can safely execute in parallel.
    private static ParallelOptions ParallelOpts(CancellationToken ct) => new()
    {
        CancellationToken      = ct,
        MaxDegreeOfParallelism = Environment.ProcessorCount,
    };

    /// <summary>All benchmark runs (class × spec × level) in parallel. Callbacks fire on worker threads.</summary>
    public static async Task<IReadOnlyList<SimulationResult>> RunAllAsync(
        SimulationSettings settings,
        Action<SimulationResult>? onResult = null,
        CancellationToken ct = default)
    {
        var jobs = settings.Classes
            .SelectMany(cls => SpecsFor(cls)
                .SelectMany(spec => Enumerable.Range(settings.LevelFrom, Math.Max(0, settings.LevelTo - settings.LevelFrom + 1))
                    .Select(level => (Class: cls, Spec: spec, Level: level))))
            .ToList();
        var results = new SimulationResult[jobs.Count];

        await Task.Run(() => Parallel.For(0, jobs.Count, ParallelOpts(ct), i =>
        {
            var result = Run(jobs[i].Class, jobs[i].Level, settings.MinutesPerRun, jobs[i].Spec, ct);
            results[i] = result;
            onResult?.Invoke(result);
        }), ct);

        return results;
    }

    /// <summary>All leveling runs (class × spec) in parallel. Callbacks fire on worker threads.</summary>
    public static async Task<IReadOnlyList<LevelingResult>> RunAllLevelingAsync(
        SimulationSettings settings,
        Action<LevelingResult>? onResult = null,
        Action<string>? onStart = null,
        CancellationToken ct = default)
    {
        var jobs = settings.Classes
            .SelectMany(cls => SpecsFor(cls).Select(spec => (Class: cls, Spec: spec)))
            .ToList();
        var results = new LevelingResult[jobs.Count];

        await Task.Run(() => Parallel.For(0, jobs.Count, ParallelOpts(ct), i =>
        {
            var (cls, spec) = jobs[i];
            onStart?.Invoke(LevelingLabel(cls, spec.Name));
            var result = RunLeveling(cls, settings.LevelFrom, settings.LevelTo, settings.MinutesPerRun, spec, ct);
            results[i] = result;
            onResult?.Invoke(result);
        }), ct);

        return results;
    }

    /// <summary>
    /// Benchmark run: level is frozen, XP is counted but not credited.
    /// Talents are fully invested into the tree of <paramref name="spec"/>; the loadout
    /// prefers the talent abilities unlocked by that and spec-appropriate abilities.
    /// </summary>
    public static SimulationResult Run(CharacterClass cls, int level, double minutes, SimSpec spec, CancellationToken ct = default)
    {
        int specIndex = spec.TreeIndex;

        var race    = RaceClassInfo.RaceToClasses.First(kv => kv.Value.Contains(cls)).Key;
        var faction = RaceClassInfo.RaceToFaction[race];

        var engine = new IdleGameEngine();
        engine.SetCharacter(faction, race, cls, Gender.Male);

        var player   = engine.Player;
        var progress = player.ClassProgress[cls];
        progress.SetLevel(level);
        player.SetActiveClass(cls);

        // Gear and starting loadout first – talents are then measured via trial fights
        SimStarterGear.Equip(player, cls, specIndex, level, spec.Form);
        var loadout = new ClassLoadout { Class = cls };
        loadout.EquippedAbilityIds = ClassAbilityRegistry.GetDefaultLoadoutIds(cls, level, specIndex, new HashSet<string>());
        player.ApplyLoadout(loadout);
        var talentPicks = AutoInvestTalentsForSpec(player, cls, spec, ct);

        // Optimize loadout (incl. talent abilities) for maximum damage via trial fights
        loadout = new ClassLoadout
        {
            Class              = cls,
            EquippedAbilityIds = SimLoadoutOptimizer.Optimize(player, player.GetCurrentLoadout().EquippedAbilityIds, ct),
        };
        player.ApplyLoadout(loadout);

        engine.Meter.Reset();
        engine.CurrentEnemy = null;

        // Auto-equip: rare items are equipped automatically if they are an upgrade
        engine.AutoEquipDrops = true;
        // No rare requeue in headless mode – prevents endless death loops
        engine.EnableRareRequeue = false;

        long damageTaken = 0;
        player.DamageTaken += amount => damageTaken += amount;

        long xpGained    = 0;
        long copperStart = player.Copper;
        int  spawnErrors = 0;
        int  steps       = (int)Math.Round(minutes * 60.0 / engine.StepSeconds);

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
                spawnErrors++;
            }

            // Freeze level: record XP and reset it
            if (progress.Xp > 0 || progress.Level != level)
            {
                xpGained += progress.Xp;
                progress.SetLevel(level);
            }
        }

        var meter = engine.Meter.GetSnapshot();
        long auto = meter.Rows.FirstOrDefault(r => r.Name == CombatMeter.AutoAttackName)?.Total ?? 0;

        return new SimulationResult(
            cls, level,
            SimSeconds: steps * engine.StepSeconds,
            CombatSeconds: meter.CombatSeconds,
            TotalDamage: meter.TotalDamage,
            AutoAttackDamage: auto,
            TotalHealing: meter.TotalHealing,
            DamageTaken: damageTaken,
            Kills: engine.TotalKills,
            Deaths: engine.TotalDeaths,
            SpawnErrors: spawnErrors,
            XpGained: xpGained,
            CopperGained: player.Copper - copperStart,
            Stats: player.Stats,
            Breakdown: meter.Rows,
            EquippedWeapon: player.EquippedWeapon,
            EquippedArmor: new Dictionary<ArmorSlot, ArmorItem>(player.EquippedArmor),
            SpecIndex: spec.Index,
            SpecName: spec.Name,
            TalentSummary: BuildTalentSummary(player, cls),
            LoadoutIds: player.GetCurrentLoadout().EquippedAbilityIds.ToList(),
            TalentPicks: talentPicks);
    }

    /// <summary>
    /// Leveling run: level-ups are actually credited.
    /// Loadout and talents are updated automatically after every level-up.
    /// Stops as soon as <paramref name="levelTo"/> is reached or <paramref name="maxMinutes"/> have elapsed.
    /// </summary>
    public static LevelingResult RunLeveling(CharacterClass cls, int levelFrom, int levelTo, double maxMinutes, SimSpec spec, CancellationToken ct = default)
    {
        int specIndex = spec.TreeIndex;

        var race    = RaceClassInfo.RaceToClasses.First(kv => kv.Value.Contains(cls)).Key;
        var faction = RaceClassInfo.RaceToFaction[race];

        var engine = new IdleGameEngine();
        engine.SetCharacter(faction, race, cls, Gender.Male);

        var player   = engine.Player;
        var progress = player.ClassProgress[cls];
        progress.SetLevel(levelFrom);
        player.SetActiveClass(cls);

        // Gear and starting loadout first – talents are then measured via trial fights
        SimStarterGear.Equip(player, cls, specIndex, levelFrom, spec.Form);
        var loadout = new ClassLoadout { Class = cls };
        loadout.EquippedAbilityIds = ClassAbilityRegistry.GetDefaultLoadoutIds(cls, levelFrom, specIndex, new HashSet<string>());
        player.ApplyLoadout(loadout);
        var talentPicks = AutoInvestTalentsForSpec(player, cls, spec, ct);

        // Optimize loadout (incl. talent abilities) for maximum damage via trial fights
        var startIds = player.GetCurrentLoadout().EquippedAbilityIds;
        string poolKey = SimLoadoutOptimizer.PoolKey(SimLoadoutOptimizer.CandidatePool(player, startIds));
        loadout = new ClassLoadout { Class = cls, EquippedAbilityIds = SimLoadoutOptimizer.Optimize(player, startIds, ct) };
        player.ApplyLoadout(loadout);

        engine.Meter.Reset();
        engine.CurrentEnemy = null;

        // Auto-equip: rare items are equipped automatically if they are an upgrade
        engine.AutoEquipDrops = true;
        // No rare requeue in headless mode – prevents endless death loops
        engine.EnableRareRequeue = false;

        long damageTaken = 0;
        player.DamageTaken += amount => damageTaken += amount;

        long copperStart = player.Copper;
        int  spawnErrors = 0;
        int  maxSteps    = (int)Math.Round(maxMinutes * 60.0 / engine.StepSeconds);
        int  lastLevel   = levelFrom;
        double simSeconds = 0;
        var levelTimings   = new List<LevelTiming>();
        var loadoutHistory = new List<LoadoutSnapshot>();
        // Remember the current loadout for tracking
        var currentLoadoutIds = player.GetCurrentLoadout().EquippedAbilityIds.ToList();
        int loadoutSinceLevel = levelFrom;

        for (int i = 0; i < maxSteps; i++)
        {
            if ((i & 1023) == 0) ct.ThrowIfCancellationRequested();

            try
            {
                engine.RunSteps(1);
            }
            catch (InvalidOperationException)
            {
                spawnErrors++;
            }

            simSeconds += engine.StepSeconds;
            int currentLevel = progress.Level;

            if (currentLevel != lastLevel)
            {
                for (int l = lastLevel + 1; l <= currentLevel; l++)
                    levelTimings.Add(new LevelTiming(l, simSeconds));

                lastLevel = currentLevel;

                // Invest talents first via trial fight (takes over new talent abilities into the loadout if any)
                talentPicks.AddRange(AutoInvestTalentsForSpec(player, cls, spec, ct));
                var seedIds = player.GetCurrentLoadout().EquippedAbilityIds.ToList();

                // Only re-optimize when new abilities or ranks are available (trial fights are expensive)
                string newPoolKey = SimLoadoutOptimizer.PoolKey(SimLoadoutOptimizer.CandidatePool(player, seedIds));
                var newAbilityIds = newPoolKey == poolKey
                    ? seedIds
                    : SimLoadoutOptimizer.Optimize(player, seedIds, ct);
                poolKey = newPoolKey;

                // Detect loadout change: store a snapshot for the completed range
                if (!newAbilityIds.SequenceEqual(currentLoadoutIds))
                {
                    loadoutHistory.Add(new LoadoutSnapshot(loadoutSinceLevel, currentLevel - 1, currentLoadoutIds));
                    currentLoadoutIds = newAbilityIds;
                    loadoutSinceLevel = currentLevel;
                }

                var newLoadout = new ClassLoadout { Class = cls };
                newLoadout.EquippedAbilityIds = newAbilityIds;
                player.ApplyLoadout(newLoadout);

                if (currentLevel >= levelTo) break;
            }
        }

        // Close the last (or only) loadout section
        loadoutHistory.Add(new LoadoutSnapshot(loadoutSinceLevel, progress.Level, currentLoadoutIds));

        // Total XP: completed levels + current partial XP
        long xpGainedTotal = 0;
        for (int l = levelFrom; l < progress.Level; l++)
            xpGainedTotal += StaticHelpers.GetXpForLevel(l);
        xpGainedTotal += progress.Xp;

        var meter = engine.Meter.GetSnapshot();
        long auto = meter.Rows.FirstOrDefault(r => r.Name == CombatMeter.AutoAttackName)?.Total ?? 0;

        return new LevelingResult(
            cls, levelFrom, levelTo,
            FinalLevel: progress.Level,
            SimSecondsTotal: simSeconds,
            TotalDamage: meter.TotalDamage,
            AutoAttackDamage: auto,
            TotalHealing: meter.TotalHealing,
            DamageTaken: damageTaken,
            TotalKills: engine.TotalKills,
            TotalDeaths: engine.TotalDeaths,
            SpawnErrors: spawnErrors,
            TotalXpGained: xpGainedTotal,
            TotalCopperGained: player.Copper - copperStart,
            LevelTimings: levelTimings,
            FinalBreakdown: meter.Rows,
            LoadoutHistory: loadoutHistory,
            TalentSummary: BuildTalentSummary(player, cls),
            FinalWeapon: player.EquippedWeapon,
            FinalArmor: new Dictionary<ArmorSlot, ArmorItem>(player.EquippedArmor),
            SpecIndex: spec.Index,
            SpecName: spec.Name,
            TalentPicks: talentPicks);
    }

    /// <summary>Collects the ability IDs of all talent-unlocked abilities the player already owns.</summary>
    internal static IReadOnlySet<string> GetUnlockedTalentAbilityIds(Player player, CharacterClass cls)
    {
        var result = new HashSet<string>();
        foreach (var talent in TalentRegistry.GetTalents(cls))
        {
            if (player.TalentState.GetRanks(talent.Id) > 0)
                result.UnionWith(talent.GrantedAbilityIds);
        }
        return result;
    }

    /// <summary>
    /// Invests all points into a single talent tree (for spec simulations): per point the talent
    /// with the largest measured damage wins (<see cref="SimTalentOptimizer"/>).
    /// </summary>
    private static List<TalentPick> AutoInvestTalentsForSpec(Player player, CharacterClass cls, SimSpec spec, CancellationToken ct)
    {
        // Order only serves as a tie-breaker: ability grants first, then row/column
        var all = TalentRegistry.GetTalents(cls);
        var talents = all
            .Where(t => t.TreeIndex == spec.TreeIndex)
            // Fixed form (Bear/Cat): skip talents of the other form including follow-up talents
            .Where(t => !GrantsOtherForm(t, spec.Form, all))
            .OrderBy(t => string.IsNullOrEmpty(t.GrantsAbilityId) ? 1 : 0)
            .ThenBy(t => t.Row)
            .ThenBy(t => t.Column)
            .ToList();

        return SimTalentOptimizer.InvestPoints(player, talents, spec.TreeIndex, ct);
    }

    /// <summary>True if the talent (or one of its prerequisites) unlocks a form other than <paramref name="form"/>.</summary>
    private static bool GrantsOtherForm(TalentDefinition talent, DruidForm? form, IReadOnlyList<TalentDefinition> all)
    {
        if (form is null) return false;

        for (var t = talent; t is not null;
             t = t.RequiresTalentId is { } req ? all.FirstOrDefault(x => x.Id == req) : null)
        {
            var granted = string.IsNullOrEmpty(t.GrantsAbilityId) ? null : ClassAbilityRegistry.FindById(t.GrantsAbilityId);
            if (granted is { IsForm: true } && granted.Form != form) return true;
        }
        return false;
    }

    /// <summary>Readable summary of the invested talents.</summary>
    private static string BuildTalentSummary(Player player, CharacterClass cls)
    {
        var treeNames = TalentRegistry.GetTreeNames(cls);
        var parts     = new List<string>();

        for (int ti = 0; ti < 3; ti++)
        {
            int invested = player.TalentState.TotalInvestedInTree(cls, ti);
            if (invested == 0) continue;

            var entries = TalentRegistry.GetTalents(cls)
                .Where(t => t.TreeIndex == ti && player.TalentState.GetRanks(t.Id) > 0)
                .Select(t => $"{t.Name} {player.TalentState.GetRanks(t.Id)}/{t.MaxRanks}");

            parts.Add($"{treeNames[ti]} ({invested}): {string.Join(", ", entries)}");
        }

        return parts.Count > 0 ? string.Join(" | ", parts) : "No talents invested";
    }
}
