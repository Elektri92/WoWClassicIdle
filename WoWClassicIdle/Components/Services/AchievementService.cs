using Models;

namespace Services;

public class AchievementService : IAchievementService
{
    private readonly IdleGameEngine _engine;
    private readonly List<AchievementDefinition> _all;
    private readonly List<EarnedAchievement> _earned = new();
    private readonly HashSet<string> _earnedIds = new();
    private readonly object _lock = new();

    // Tracking state for change detection
    private readonly Dictionary<CharacterClass, int> _lastKnownLevels = new();
    private int _lastKnownKills;
    private int _lastKnownDeaths;

    public IReadOnlyList<AchievementDefinition> AllAchievements => _all;
    public IReadOnlyList<EarnedAchievement> EarnedAchievements { get { lock (_lock) return _earned.ToList(); } }
    public int TotalPoints { get { lock (_lock) return _earned.Sum(e => _all.Find(a => a.Id == e.AchievementId)?.Points ?? 0); } }

    public event Action<AchievementDefinition>? AchievementEarned;

    private static readonly int[] LevelMilestones = { 10, 20, 30, 40, 50, 60 };
    private static readonly int[] KillMilestones  = { 10, 1_000, 5_000, 10_000, 50_000, 100_000 };
    private static readonly int[] DeathMilestones = { 10, 100, 250, 500, 1_000, 2_500 };

    private static readonly Dictionary<CharacterClass, string> ClassColors = new()
    {
        [CharacterClass.Warrior] = "#C79C6E",
        [CharacterClass.Mage]    = "#40C7EB",
        [CharacterClass.Priest]  = "#FFFFFF",
        [CharacterClass.Rogue]   = "#FFF569",
        [CharacterClass.Paladin] = "#F58CBA",
        [CharacterClass.Hunter]  = "#ABD473",
        [CharacterClass.Warlock] = "#8787ED",
        [CharacterClass.Shaman]  = "#0070DE",
        [CharacterClass.Druid]   = "#FF7D0A",
    };

    public AchievementService(IdleGameEngine engine)
    {
        _engine = engine;
        _all = BuildAchievements();

        foreach (var cls in Enum.GetValues<CharacterClass>())
            _lastKnownLevels[cls] = 1;

        _engine.TickUpdated += OnTick;
    }

    private void OnTick()
    {
        CheckClassLevels();
        CheckKills();
        CheckDeaths();
        CheckGeneral();
    }

    private void CheckClassLevels()
    {
        var player = _engine.Player;
        foreach (var cls in Enum.GetValues<CharacterClass>())
        {
            if (!player.ClassProgress.TryGetValue(cls, out var progress)) continue;

            int current = progress.Level;
            int last    = _lastKnownLevels[cls];

            if (current > last)
            {
                foreach (int milestone in LevelMilestones)
                {
                    if (last < milestone && current >= milestone)
                    {
                        string id = $"class_{cls}_{milestone}";
                        TryAward(id);
                    }
                }
                _lastKnownLevels[cls] = current;
            }
        }
    }

    private void CheckKills()
    {
        int kills = _engine.TotalKills;
        if (kills <= _lastKnownKills) return;

        foreach (int milestone in KillMilestones)
        {
            if (_lastKnownKills < milestone && kills >= milestone)
                TryAward($"kills_{milestone}");
        }
        _lastKnownKills = kills;
    }

    private void CheckDeaths()
    {
        int deaths = _engine.TotalDeaths;
        if (deaths <= _lastKnownDeaths) return;

        foreach (int milestone in DeathMilestones)
        {
            if (_lastKnownDeaths < milestone && deaths >= milestone)
                TryAward($"deaths_{milestone}");
        }
        _lastKnownDeaths = deaths;
    }

    private void CheckGeneral()
    {
        var player = _engine.Player;

        // First character created
        if (_engine.CharacterRoster.Count > 0)
            TryAward("general_first_character");

        // Any class level 10+
        if (player.ClassProgress.Values.Any(p => p.Level >= 10))
            TryAward("general_level10");

        // Any class level 30+
        if (player.ClassProgress.Values.Any(p => p.Level >= 30))
            TryAward("general_level30");

        // Any class level 60
        if (player.ClassProgress.Values.Any(p => p.Level >= 60))
            TryAward("general_level60");

        // All classes reached level 10
        if (player.ClassProgress.Values.All(p => p.Level >= 10))
            TryAward("general_all_classes_10");
    }

    private void TryAward(string id)
    {
        lock (_lock)
        {
            if (_earnedIds.Contains(id)) return;
            var def = _all.Find(a => a.Id == id);
            if (def is null) return;

            _earnedIds.Add(id);
            _earned.Add(new EarnedAchievement { AchievementId = id, EarnedAt = DateTime.UtcNow });
        }

        var defForEvent = _all.Find(a => a.Id == id)!;
        AchievementEarned?.Invoke(defForEvent);
    }

    internal void RestoreFromSave(IEnumerable<AchievementSaveData> entries, IdleGameEngine engine)
    {
        lock (_lock)
        {
            _earned.Clear();
            _earnedIds.Clear();

            foreach (var entry in entries)
            {
                if (string.IsNullOrEmpty(entry.Id)) continue;
                _earnedIds.Add(entry.Id);
                _earned.Add(new EarnedAchievement { AchievementId = entry.Id, EarnedAt = entry.EarnedAt });
            }
        }

        // Tracking-State auf aktuellen Stand setzen, damit keine Achievements doppelt feuern
        foreach (var cls in Enum.GetValues<CharacterClass>())
        {
            if (engine.Player.ClassProgress.TryGetValue(cls, out var progress))
                _lastKnownLevels[cls] = progress.Level;
        }
        _lastKnownKills  = engine.TotalKills;
        _lastKnownDeaths = engine.TotalDeaths;
    }

    public bool IsEarned(string id) { lock (_lock) return _earnedIds.Contains(id); }

    public EarnedAchievement? GetEarned(string id)
    {
        lock (_lock) return _earned.FirstOrDefault(e => e.AchievementId == id);
    }

    private static List<AchievementDefinition> BuildAchievements()
    {
        var list = new List<AchievementDefinition>();

        // ── General ──
        list.Add(new() { Id = "general_first_character", Category = AchievementCategory.General, Points = 10, Title = "A New Hero",        Description = "Create your first character.",            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/inv_misc_head_human_01.jpg" });
        list.Add(new() { Id = "general_level10",        Category = AchievementCategory.General, Points = 10, Title = "Hero's Journey",      Description = "Reach level 10 with any class.",         IconUrl = "https://wow.zamimg.com/images/wow/icons/large/inv_misc_map_01.jpg" });
        list.Add(new() { Id = "general_level30",        Category = AchievementCategory.General, Points = 10, Title = "Hardened Adventurer",  Description = "Reach level 30 with any class.",         IconUrl = "https://wow.zamimg.com/images/wow/icons/large/inv_misc_map_01.jpg" });
        list.Add(new() { Id = "general_level60",        Category = AchievementCategory.General, Points = 10, Title = "Champion of Azeroth",  Description = "Reach level 60 with any class.",         IconUrl = "https://wow.zamimg.com/images/wow/icons/large/inv_misc_map_01.jpg" });
        list.Add(new() { Id = "general_all_classes_10", Category = AchievementCategory.General, Points = 10, Title = "Jack of All Classes",  Description = "Reach level 10 with every class.",       IconUrl = "https://wow.zamimg.com/images/wow/icons/large/inv_misc_book_09.jpg" });

        // ── Classes ──
        foreach (var cls in Enum.GetValues<CharacterClass>())
        {
            string icon = IconUrls.ClassIconUrl(cls);
            string name = cls.ToString();

            list.Add(new() { Id = $"class_{cls}_10", Category = AchievementCategory.Classes, Points = 10, Class = cls, Title = $"{name}: Apprentice",  Description = $"Reach level 10 as a {name}.", IconUrl = icon });
            list.Add(new() { Id = $"class_{cls}_20", Category = AchievementCategory.Classes, Points = 10, Class = cls, Title = $"{name}: Journeyman",  Description = $"Reach level 20 as a {name}.", IconUrl = icon });
            list.Add(new() { Id = $"class_{cls}_30", Category = AchievementCategory.Classes, Points = 10, Class = cls, Title = $"{name}: Adept",       Description = $"Reach level 30 as a {name}.", IconUrl = icon });
            list.Add(new() { Id = $"class_{cls}_40", Category = AchievementCategory.Classes, Points = 10, Class = cls, Title = $"{name}: Expert",      Description = $"Reach level 40 as a {name}.", IconUrl = icon });
            list.Add(new() { Id = $"class_{cls}_50", Category = AchievementCategory.Classes, Points = 10, Class = cls, Title = $"{name}: Elite",       Description = $"Reach level 50 as a {name}.", IconUrl = icon });
            list.Add(new() { Id = $"class_{cls}_60", Category = AchievementCategory.Classes, Points = 10, Class = cls, Title = $"{name}: Master",      Description = $"Reach level 60 as a {name}.", IconUrl = icon });
        }

        // ── Kills ──
        list.Add(new() { Id = "kills_10",     Category = AchievementCategory.Kills, Points = 10, Title = "First Blood",      Description = "Defeat 10 enemies.",                 IconUrl = "https://wow.zamimg.com/images/wow/icons/large/ability_warrior_savageblow.jpg" });
        list.Add(new() { Id = "kills_1000",   Category = AchievementCategory.Kills, Points = 10, Title = "Slayer",           Description = "Defeat 1,000 enemies.",             IconUrl = "https://wow.zamimg.com/images/wow/icons/large/ability_warrior_savageblow.jpg" });
        list.Add(new() { Id = "kills_5000",   Category = AchievementCategory.Kills, Points = 10, Title = "Veteran Slayer",   Description = "Defeat 5,000 enemies.",             IconUrl = "https://wow.zamimg.com/images/wow/icons/large/inv_sword_04.jpg" });
        list.Add(new() { Id = "kills_10000",  Category = AchievementCategory.Kills, Points = 10, Title = "Master Slayer",    Description = "Defeat 10,000 enemies.",            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/inv_sword_04.jpg" });
        list.Add(new() { Id = "kills_50000",  Category = AchievementCategory.Kills, Points = 10, Title = "Unstoppable",      Description = "Defeat 50,000 enemies.",            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/spell_fire_meteorstorm.jpg" });
        list.Add(new() { Id = "kills_100000", Category = AchievementCategory.Kills, Points = 10, Title = "Legendary Slayer", Description = "Defeat 100,000 enemies.",          IconUrl = "https://wow.zamimg.com/images/wow/icons/large/spell_fire_meteorstorm.jpg" });

        // ── Deaths ──
        list.Add(new() { Id = "deaths_10",    Category = AchievementCategory.Deaths, Points = 10, Title = "Fallen Hero", Description = "Die 10 times and keep on fighting.",         IconUrl = "https://wow.zamimg.com/images/wow/icons/large/spell_holy_resurrection.jpg" });
        list.Add(new() { Id = "deaths_100",   Category = AchievementCategory.Deaths, Points = 10, Title = "Resilient",   Description = "Die 100 times and still rise.",              IconUrl = "https://wow.zamimg.com/images/wow/icons/large/spell_holy_resurrection.jpg" });
        list.Add(new() { Id = "deaths_250",   Category = AchievementCategory.Deaths, Points = 10, Title = "Martyr",      Description = "Die 250 times. Death starts to know your name.", IconUrl = "https://wow.zamimg.com/images/wow/icons/large/spell_holy_resurrection.jpg" });
        list.Add(new() { Id = "deaths_500",   Category = AchievementCategory.Deaths, Points = 10, Title = "Undying",     Description = "Die 500 times and return stronger.",         IconUrl = "https://wow.zamimg.com/images/wow/icons/large/spell_holy_resurrection.jpg" });
        list.Add(new() { Id = "deaths_1000",  Category = AchievementCategory.Deaths, Points = 10, Title = "Immortal",    Description = "Die 1,000 times. The graveyard considers you family.", IconUrl = "https://wow.zamimg.com/images/wow/icons/large/spell_holy_resurrection.jpg" });
        list.Add(new() { Id = "deaths_2500",  Category = AchievementCategory.Deaths, Points = 10, Title = "Deathless",   Description = "Die 2,500 times. Death has given up on you.", IconUrl = "https://wow.zamimg.com/images/wow/icons/large/spell_holy_resurrection.jpg" });

        return list;
    }
}
