// Debug/DungeonSimulator.cs – only compiled in the DEBUG build (see .csproj)
namespace DebugTools;

using Models;

/// <summary>One simulated dungeon series compared with the open zone at the same level.</summary>
public sealed record DungeonSimResult(
    string Dungeon, CharacterClass Class, string SpecName, DungeonRole Role, int Level,
    int Runs, int FailedRuns, int Deaths, double SecondsPerRun, double XpPerHour, double OpenZoneXpPerHour)
{
    public double DeathsPerRun => Runs > 0 ? (double)Deaths / Runs : 0;
    public double XpRatio => OpenZoneXpPerHour > 0 ? XpPerHour / OpenZoneXpPerHour : 0;
}

/// <summary>
/// Plays dungeon runs without a roster slot: the encounters come from the same builder as in the engine
/// and are handed to the engine one by one. Lockout and loot are not part of this.
/// </summary>
public static class DungeonSimulator
{
    public const int RunsPerSeries = 6;
    private const double OpenZoneMinutes = 30;
    private const double MaxSecondsPerRun = 7200;

    public static async Task<IReadOnlyList<DungeonSimResult>> RunAllAsync(IProgress<string>? progress = null, CancellationToken ct = default)
    {
        var jobs = (from zone in CombatZoneRegistry.Dungeons
                    from level in new[] { zone.MinLevel, zone.MinLevel + 2, zone.MaxLevel }
                    from cls in Enum.GetValues<CharacterClass>()
                    from spec in ClassSimulator.SpecsFor(cls)
                    from role in DungeonPartyRegistry.RolesFor(cls)
                    select (zone, level, cls, spec, role)).ToList();

        var results = new System.Collections.Concurrent.ConcurrentBag<DungeonSimResult>();
        int done = 0;
        var options = new ParallelOptions { CancellationToken = ct, MaxDegreeOfParallelism = Math.Max(1, Environment.ProcessorCount - 1) };
        await Parallel.ForEachAsync(jobs, options, (job, token) =>
        {
            results.Add(Run(job.zone, job.cls, job.level, job.spec, job.role, token));
            progress?.Report($"{Interlocked.Increment(ref done)}/{jobs.Count}");
            return ValueTask.CompletedTask;
        });
        return results.OrderBy(r => r.Dungeon).ThenBy(r => r.Level).ThenBy(r => r.Class.ToString()).ThenBy(r => r.SpecName).ThenBy(r => r.Role).ToList();
    }

    public static DungeonSimResult Run(CombatZone zone, CharacterClass cls, int level, SimSpec spec, DungeonRole role, CancellationToken ct = default)
    {
        double openXpPerHour = ClassSimulator.Run(cls, level, OpenZoneMinutes, spec, ct) is { SimSeconds: > 0 } open
            ? open.XpGained / (open.SimSeconds / 3600.0)
            : 0;

        var (engine, _) = ClassSimulator.PrepareBenchmark(cls, level, spec, ct);
        var dungeon = zone.Dungeon!;
        var faction = RaceClassInfo.RaceToFaction[RaceClassInfo.RaceToClasses.First(kv => kv.Value.Contains(cls)).Key];
        engine.DebugSetSimParty(DungeonPartyRegistry.Build(level, faction, DungeonPartyRegistry.DefaultSetup(level, faction, cls, role)), role);
        var progress = engine.Player.ClassProgress[cls];
        var rng = new Random(HashCode.Combine(zone.Id, cls, level, spec.Index));

        long xp = 0;
        int steps = 0, deaths = 0, failed = 0;
        int maxSteps = (int)(MaxSecondsPerRun * RunsPerSeries / engine.StepSeconds);

        for (int run = 0; run < RunsPerSeries; run++)
        {
            int wipes = 0;
            for (int index = 0; index < dungeon.Encounters.Count && wipes < DungeonRules.MaxWipes;)
            {
                var encounter = dungeon.Encounters[index];
                for (int kill = 0; kill < encounter.Kills && wipes < DungeonRules.MaxWipes;)
                {
                    engine.CurrentEnemy = DungeonEnemies.Build(zone, encounter, level, rng);
                    steps += (int)(engine.CurrentEnemy.IntroRemaining / engine.StepSeconds); // the entrance costs time, too
                    engine.CurrentEnemy.IntroRemaining = 0;
                    int killsBefore = engine.TotalKills, deathsBefore = engine.TotalDeaths;
                    while (engine.TotalKills == killsBefore && engine.TotalDeaths == deathsBefore && steps < maxSteps)
                    {
                        if ((steps & 1023) == 0) ct.ThrowIfCancellationRequested();
                        xp += Step(engine, progress, level, ref steps);
                    }
                    if (steps >= maxSteps) return Result(failed + RunsPerSeries - run);

                    if (engine.TotalDeaths != deathsBefore)
                    {
                        deaths++;
                        wipes++;
                        kill = 0; // the encounter starts over
                        while (engine.IsAtSpiritHealer) xp += Step(engine, progress, level, ref steps);
                    }
                    else
                    {
                        kill++;
                    }
                }
                index++;
            }
            if (wipes >= DungeonRules.MaxWipes) failed++;
        }
        return Result(failed);

        DungeonSimResult Result(int failedRuns)
        {
            double seconds = steps * engine.StepSeconds;
            return new DungeonSimResult(zone.Name, cls, spec.Name, role, level, RunsPerSeries, failedRuns, deaths,
                seconds / RunsPerSeries, seconds > 0 ? xp / (seconds / 3600.0) : 0, openXpPerHour);
        }
    }

    // One engine step; returns the XP change (negative with the death penalty) and holds the level
    private static long Step(IdleGameEngine engine, ClassProgress progress, int level, ref int steps)
    {
        int xpBefore = progress.Xp;
        engine.RunSteps(1);
        steps++;
        if (progress.Level == level) return progress.Xp - xpBefore;

        long gained = StaticHelpers.GetXpForLevel(level) - xpBefore + progress.Xp;
        progress.SetLevel(level);
        if (engine.Player.EquippedWeapon is { } weapon) engine.Player.ForceEquipWeapon(weapon); // back to the stats of the level
        return gained;
    }
}
