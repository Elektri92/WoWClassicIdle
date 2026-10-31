using Models;

namespace Services;

// Progress display (e.g. "523 / 1,000") for countable achievements
public partial class AchievementService
{
    private readonly Dictionary<string, (Func<long> Current, long Target)> _progress;

    public AchievementProgress? GetProgress(string achievementId)
    {
        if (!_progress.TryGetValue(achievementId, out var tracker)) return null;

        long current;
        try { current = tracker.Current(); }
        catch (InvalidOperationException) { return null; }   // Collection was modified in parallel by the tick thread

        return new AchievementProgress(Math.Min(current, tracker.Target), tracker.Target);
    }

    private Dictionary<string, (Func<long>, long)> BuildProgressTrackers()
    {
        var map = new Dictionary<string, (Func<long>, long)>();

        void Track(IEnumerable<int> milestones, string prefix, Func<long> current)
        {
            foreach (int m in milestones)
                map[$"{prefix}{m}"] = (current, m);
        }

        Track(KillMilestones,         "kills_",         () => _engine.TotalKills);
        Track(DeathMilestones,        "deaths_",        () => _engine.TotalDeaths);
        Track(RareKillMilestones,     "rare_kills_",    () => _engine.TotalRareKills);
        Track(SoldMilestones,         "sold_",          () => _engine.TotalItemsSold);
        Track(UncommonLootMilestones, "loot_uncommon_", () => _engine.TotalUncommonLooted);
        Track(RareLootMilestones,     "loot_rare_",     () => _engine.TotalRareItemsLooted);
        Track(JumpMilestones,         "jumps_",         () => _engine.TotalJumps);

        foreach (var cls in Enum.GetValues<CharacterClass>())
            Track(LevelMilestones, $"class_{cls}_", () => ClassLevel(cls));

        foreach (var def in Professions.All)
            Track(ProfessionSkillMilestones, $"prof_{def.Profession}_", () => _engine.HighestProfessionSkill(def.Profession));

        Track(new[] { 10, 30, 60 }, "general_level", () => _engine.Player.ClassProgress.Values.Select(p => p.Level).DefaultIfEmpty(0).Max());

        int classCount = Enum.GetValues<CharacterClass>().Length;
        map["general_all_classes_10"] = (() => _engine.Player.ClassProgress.Values.Count(p => p.Level >= 10), classCount);

        AddExplorationTrackers(map);
        AddProfessionBonusTrackers(map);
        return map;
    }

    private long ClassLevel(CharacterClass cls) =>
        _engine.Player.ClassProgress.TryGetValue(cls, out var p) ? p.Level : 0;
}
