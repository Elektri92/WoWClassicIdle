// Debug/ClassSimulator.cs – wird nur im DEBUG-Build kompiliert (siehe .csproj)
namespace DebugTools;

using Models;

/// <summary>Ergebnis eines Simulationslaufs (eine Klasse auf einem festen Level).</summary>
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
    IReadOnlyDictionary<ArmorSlot, ArmorItem> EquippedArmor)
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

    /// <summary>Spielzeit bis zum nächsten Level in Minuten (NaN = kein XP-Gewinn / Max-Level).</summary>
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

/// <summary>Zeitpunkt, zu dem ein Level während eines Leveling-Laufs erreicht wurde.</summary>
public sealed record LevelTiming(int Level, double SimSecondsToReach);

/// <summary>Ergebnis eines vollständigen Leveling-Laufs von LevelFrom bis LevelTo für eine Klasse.</summary>
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
    string TalentSummary,
    WeaponItem? FinalWeapon,
    IReadOnlyDictionary<ArmorSlot, ArmorItem> FinalArmor,
    int SpecIndex,
    string SpecName)
{
    public bool ReachedTarget => FinalLevel >= LevelTo;

    /// <summary>Simulierte Sekunden bis zum Ziel-Level. NaN wenn Ziel nicht erreicht.</summary>
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
/// Simuliert Klassen headless mit eigenen <see cref="IdleGameEngine"/>-Instanzen –
/// exakt dieselbe Kampflogik wie im Spiel, das laufende Spiel bleibt unberührt.
/// </summary>
public static class ClassSimulator
{
    /// <summary>Anzahl der Benchmark-Läufe für die gegebenen Einstellungen.</summary>
    public static int RunCount(SimulationSettings s) =>
        s.Classes.Count * Math.Max(0, s.LevelTo - s.LevelFrom + 1);

    /// <summary>Anzahl der Leveling-Läufe (3 Specs je Klasse).</summary>
    public static int LevelingRunCount(SimulationSettings s) => s.Classes.Count * 3;

    public static async Task<IReadOnlyList<SimulationResult>> RunAllAsync(
        SimulationSettings settings,
        Action<SimulationResult>? onResult = null,
        CancellationToken ct = default)
    {
        var results = new List<SimulationResult>();

        await Task.Run(() =>
        {
            foreach (var cls in settings.Classes)
            {
                for (int level = settings.LevelFrom; level <= settings.LevelTo; level++)
                {
                    ct.ThrowIfCancellationRequested();
                    var result = Run(cls, level, settings.MinutesPerRun, ct);
                    results.Add(result);
                    onResult?.Invoke(result);
                }
            }
        }, ct);

        return results;
    }

    public static async Task<IReadOnlyList<LevelingResult>> RunAllLevelingAsync(
        SimulationSettings settings,
        Action<LevelingResult>? onResult = null,
        Action<string>? onStart = null,
        CancellationToken ct = default)
    {
        var results = new List<LevelingResult>();

        await Task.Run(() =>
        {
            foreach (var cls in settings.Classes)
            {
                var treeNames = TalentRegistry.GetTreeNames(cls);
                for (int specIdx = 0; specIdx < 3; specIdx++)
                {
                    ct.ThrowIfCancellationRequested();
                    onStart?.Invoke($"{cls} · {treeNames[specIdx]}");
                    var result = RunLeveling(cls, settings.LevelFrom, settings.LevelTo, settings.MinutesPerRun, specIdx, ct);
                    results.Add(result);
                    onResult?.Invoke(result);
                }
            }
        }, ct);

        return results;
    }

    /// <summary>
    /// Benchmark-Lauf: Level wird eingefroren, XP wird gezählt aber nicht angerechnet.
    /// Loadout und Talente werden automatisch für das gegebene Level gewählt.
    /// </summary>
    public static SimulationResult Run(CharacterClass cls, int level, double minutes, CancellationToken ct = default)
    {
        var race    = RaceClassInfo.RaceToClasses.First(kv => kv.Value.Contains(cls)).Key;
        var faction = RaceClassInfo.RaceToFaction[race];

        var engine = new IdleGameEngine();
        engine.SetCharacter(faction, race, cls, Gender.Male);

        var player   = engine.Player;
        var progress = player.ClassProgress[cls];
        progress.SetLevel(level);
        player.SetActiveClass(cls);

        var loadout = new ClassLoadout { Class = cls };
        loadout.EquippedAbilityIds = ClassAbilityRegistry.GetDefaultLoadoutIds(cls, level);
        player.ApplyLoadout(loadout);
        AutoInvestTalents(player, cls);
        SimStarterGear.Equip(player, cls, specIndex: 0, level);

        engine.Meter.Reset();
        engine.CurrentEnemy = null;

        // Auto-Equip: Seltene Items werden automatisch angelegt wenn sie ein Upgrade sind
        engine.AutoEquipDrops = true;
        // Kein Rare-Requeue im Headless-Modus – verhindert Endlos-Sterbeschleifen
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
                // Spawner hat kein Template für das gewürfelte Gegner-Level
                spawnErrors++;
            }

            // Level einfrieren: XP mitschreiben und zurücksetzen
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
            EquippedArmor: new Dictionary<ArmorSlot, ArmorItem>(player.EquippedArmor));
    }

    /// <summary>
    /// Leveling-Lauf: Level-Ups werden tatsächlich angerechnet.
    /// Loadout und Talente werden nach jedem Level-Up automatisch aktualisiert.
    /// Stoppt sobald <paramref name="levelTo"/> erreicht wird oder <paramref name="maxMinutes"/> verstrichen sind.
    /// </summary>
    public static LevelingResult RunLeveling(CharacterClass cls, int levelFrom, int levelTo, double maxMinutes, int specIndex, CancellationToken ct = default)
    {
        var treeNames = TalentRegistry.GetTreeNames(cls);
        var specName  = treeNames[specIndex];

        var race    = RaceClassInfo.RaceToClasses.First(kv => kv.Value.Contains(cls)).Key;
        var faction = RaceClassInfo.RaceToFaction[race];

        var engine = new IdleGameEngine();
        engine.SetCharacter(faction, race, cls, Gender.Male);

        var player   = engine.Player;
        var progress = player.ClassProgress[cls];
        progress.SetLevel(levelFrom);
        player.SetActiveClass(cls);

        var loadout = new ClassLoadout { Class = cls };
        loadout.EquippedAbilityIds = ClassAbilityRegistry.GetDefaultLoadoutIds(cls, levelFrom);
        player.ApplyLoadout(loadout);
        AutoInvestTalentsForSpec(player, cls, specIndex);
        SimStarterGear.Equip(player, cls, specIndex, levelFrom);

        engine.Meter.Reset();
        engine.CurrentEnemy = null;

        // Auto-Equip: Seltene Items werden automatisch angelegt wenn sie ein Upgrade sind
        engine.AutoEquipDrops = true;
        // Kein Rare-Requeue im Headless-Modus – verhindert Endlos-Sterbeschleifen
        engine.EnableRareRequeue = false;

        long damageTaken = 0;
        player.DamageTaken += amount => damageTaken += amount;

        long copperStart = player.Copper;
        int  spawnErrors = 0;
        int  maxSteps    = (int)Math.Round(maxMinutes * 60.0 / engine.StepSeconds);
        int  lastLevel   = levelFrom;
        double simSeconds = 0;
        var levelTimings  = new List<LevelTiming>();

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

                // Loadout und Talente für das neue Level aktualisieren
                var newLoadout = new ClassLoadout { Class = cls };
                newLoadout.EquippedAbilityIds = ClassAbilityRegistry.GetDefaultLoadoutIds(cls, currentLevel);
                player.ApplyLoadout(newLoadout);
                AutoInvestTalentsForSpec(player, cls, specIndex);

                if (currentLevel >= levelTo) break;
            }
        }

        // Gesamt-XP: abgeschlossene Level + aktuelles Zwischen-XP
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
            TalentSummary: BuildTalentSummary(player, cls),
            FinalWeapon: player.EquippedWeapon,
            FinalArmor: new Dictionary<ArmorSlot, ArmorItem>(player.EquippedArmor),
            SpecIndex: specIndex,
            SpecName: specName);
    }

    /// <summary>Investiert alle Punkte greedy in einen einzigen Talent-Baum (für Spec-Simulationen).</summary>
    private static void AutoInvestTalentsForSpec(Player player, CharacterClass cls, int specIndex)
    {
        var talents = TalentRegistry.GetTalents(cls)
            .Where(t => t.TreeIndex == specIndex)
            .OrderBy(t => t.Row)
            .ThenBy(t => t.Column)
            .ToList();

        int available;
        while ((available = player.GetAvailableTalentPoints(cls)) > 0)
        {
            var next = talents.FirstOrDefault(t => player.TalentState.CanInvest(t, available));
            if (next is null) break;
            player.InvestTalent(next);
        }
    }

    /// <summary>Investiert alle verfügbaren Talent-Punkte greedy (Baum 0 → 1 → 2, frühe Reihen zuerst).</summary>
    private static void AutoInvestTalents(Player player, CharacterClass cls)
    {
        var talents = TalentRegistry.GetTalents(cls)
            .OrderBy(t => t.TreeIndex)
            .ThenBy(t => t.Row)
            .ThenBy(t => t.Column)
            .ToList();

        int available;
        while ((available = player.GetAvailableTalentPoints(cls)) > 0)
        {
            var next = talents.FirstOrDefault(t => player.TalentState.CanInvest(t, available));
            if (next is null) break;
            player.InvestTalent(next);
        }
    }

    /// <summary>Lesbare Zusammenfassung der investierten Talente.</summary>
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
