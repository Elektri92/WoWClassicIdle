using Models;

namespace Services;

// Erkundungs-Erfolge: entdeckte Zonen, Kontinente, Zonen-Ziele und Mastery – accountweit über alle Charaktere
public partial class AchievementService
{
    private static readonly int[] DiscoveredZoneMilestones = { 5, 10, 20, 30 };
    private static readonly int[] ObjectiveMilestones      = { 1, 10, 25, 50, 100 };
    private static readonly int[] MasteredZoneMilestones   = { 1, 5, 10, 20 };

    private static int ZoneCount => CombatZoneRegistry.All.Count;

    private static IEnumerable<CombatZone> ContinentZones(bool easternKingdoms) =>
        CombatZoneRegistry.All.Where(z => (z.Landmass == Landmass.EasternKingdoms) == easternKingdoms);

    // ToArray kopiert per Array.Copy – kein Enumerationsfehler, wenn der UI-Thread parallel Charaktere anlegt
    private CharacterSlot[] RosterSnapshot() => _engine.CharacterRoster.ToArray();

    /// <summary>Von irgendeinem Charakter entdeckte Zonen.</summary>
    private HashSet<string> DiscoveredZones() =>
        RosterSnapshot().SelectMany(s => s.DiscoveredZoneIds).ToHashSet();

    private static bool IsDone(CharacterSlot slot, ZoneObjective objective) =>
        slot.ZoneObjectiveProgress.GetValueOrDefault(objective.Id) >= objective.Required;

    /// <summary>Erledigte Zonen-Ziele, über alle Charaktere summiert.</summary>
    private long CompletedObjectives() =>
        RosterSnapshot().Sum(s => CombatZoneRegistry.All.Sum(z => ZoneObjectiveRegistry.For(z).Count(o => IsDone(s, o))));

    /// <summary>Zonen, die mindestens ein Charakter gemeistert hat.</summary>
    private HashSet<string> MasteredZones()
    {
        var roster = RosterSnapshot();
        return CombatZoneRegistry.All
            .Where(z => roster.Any(s => ZoneObjectiveRegistry.For(z).All(o => IsDone(s, o))))
            .Select(z => z.Id)
            .ToHashSet();
    }

    private void CheckExploration()
    {
        var discovered = DiscoveredZones();
        foreach (int m in DiscoveredZoneMilestones)
            if (discovered.Count >= m) TryAward($"explore_zones_{m}");
        if (discovered.Count >= ZoneCount) TryAward("explore_all_zones");
        if (ContinentZones(true).All(z => discovered.Contains(z.Id)))  TryAward("explore_eastern_kingdoms");
        if (ContinentZones(false).All(z => discovered.Contains(z.Id))) TryAward("explore_kalimdor");

        long objectives = CompletedObjectives();
        foreach (int m in ObjectiveMilestones)
            if (objectives >= m) TryAward($"zone_objectives_{m}");

        int mastered = MasteredZones().Count;
        foreach (int m in MasteredZoneMilestones)
            if (mastered >= m) TryAward($"zone_mastery_{m}");
        if (mastered >= ZoneCount) TryAward("zone_mastery_all");
    }

    private void AddExplorationTrackers(Dictionary<string, (Func<long>, long)> map)
    {
        foreach (int m in DiscoveredZoneMilestones)
            map[$"explore_zones_{m}"] = (() => DiscoveredZones().Count, m);
        map["explore_all_zones"] = (() => DiscoveredZones().Count, ZoneCount);
        map["explore_eastern_kingdoms"] = (() => ContinentZones(true).Count(z => DiscoveredZones().Contains(z.Id)), ContinentZones(true).Count());
        map["explore_kalimdor"]         = (() => ContinentZones(false).Count(z => DiscoveredZones().Contains(z.Id)), ContinentZones(false).Count());

        foreach (int m in ObjectiveMilestones)
            map[$"zone_objectives_{m}"] = (CompletedObjectives, m);

        foreach (int m in MasteredZoneMilestones)
            map[$"zone_mastery_{m}"] = (() => MasteredZones().Count, m);
        map["zone_mastery_all"] = (() => MasteredZones().Count, ZoneCount);
    }

    private static void AddExplorationAchievements(List<AchievementDefinition> list)
    {
        const string map = "https://wow.zamimg.com/images/wow/icons/large/inv_misc_map_01.jpg";
        const string spyglass = "https://wow.zamimg.com/images/wow/icons/large/inv_misc_spyglass_02.jpg";
        const string scroll = "https://wow.zamimg.com/images/wow/icons/large/inv_scroll_03.jpg";
        const string book = "https://wow.zamimg.com/images/wow/icons/large/inv_misc_book_11.jpg";
        const string trophy = "https://wow.zamimg.com/images/wow/icons/large/inv_misc_head_dragon_01.jpg";
        var cat = AchievementCategory.Exploration;

        // ── Entdeckte Zonen ──
        list.Add(new() { Id = "explore_zones_5",  Category = cat, Points = 10, Title = "Wanderlust",       Description = "Discover 5 zones.",  IconUrl = spyglass });
        list.Add(new() { Id = "explore_zones_10", Category = cat, Points = 10, Title = "Pathfinder",       Description = "Discover 10 zones.", IconUrl = spyglass });
        list.Add(new() { Id = "explore_zones_20", Category = cat, Points = 15, Title = "Seasoned Traveler", Description = "Discover 20 zones.", IconUrl = spyglass });
        list.Add(new() { Id = "explore_zones_30", Category = cat, Points = 20, Title = "Globetrotter",     Description = "Discover 30 zones.", IconUrl = spyglass });
        list.Add(new() { Id = "explore_eastern_kingdoms", Category = cat, Points = 25, Title = "Explore Eastern Kingdoms", Description = "Discover every zone in the Eastern Kingdoms.", IconUrl = "https://wow.zamimg.com/images/wow/icons/large/inv_misc_map02.jpg" });
        list.Add(new() { Id = "explore_kalimdor",         Category = cat, Points = 25, Title = "Explore Kalimdor",         Description = "Discover every zone in Kalimdor.",             IconUrl = map });
        list.Add(new() { Id = "explore_all_zones",        Category = cat, Points = 50, Title = "World Explorer",           Description = $"Discover all {ZoneCount} zones of Azeroth.",  IconUrl = "https://wow.zamimg.com/images/wow/icons/large/inv_misc_spyglass_03.jpg" });

        // ── Zonen-Ziele ──
        list.Add(new() { Id = "zone_objectives_1",   Category = cat, Points = 10, Title = "Odd Jobs",          Description = "Complete your first zone objective.", IconUrl = scroll });
        list.Add(new() { Id = "zone_objectives_10",  Category = cat, Points = 10, Title = "Helping Hand",      Description = "Complete 10 zone objectives.",        IconUrl = scroll });
        list.Add(new() { Id = "zone_objectives_25",  Category = cat, Points = 15, Title = "Problem Solver",    Description = "Complete 25 zone objectives.",        IconUrl = scroll });
        list.Add(new() { Id = "zone_objectives_50",  Category = cat, Points = 20, Title = "Pillar of the Realm", Description = "Complete 50 zone objectives.",      IconUrl = book });
        list.Add(new() { Id = "zone_objectives_100", Category = cat, Points = 25, Title = "Tireless Champion", Description = "Complete 100 zone objectives.",       IconUrl = book });

        // ── Zone Mastery ──
        list.Add(new() { Id = "zone_mastery_1",   Category = cat, Points = 10, Title = "Master of the Land", Description = "Master your first zone.",         IconUrl = trophy });
        list.Add(new() { Id = "zone_mastery_5",   Category = cat, Points = 15, Title = "Regional Hero",      Description = "Master 5 different zones.",       IconUrl = trophy });
        list.Add(new() { Id = "zone_mastery_10",  Category = cat, Points = 20, Title = "Defender of Azeroth", Description = "Master 10 different zones.",     IconUrl = trophy });
        list.Add(new() { Id = "zone_mastery_20",  Category = cat, Points = 25, Title = "Legend of the Realm", Description = "Master 20 different zones.",     IconUrl = trophy });
        list.Add(new() { Id = "zone_mastery_all", Category = cat, Points = 50, Title = "Loremaster",         Description = $"Master all {ZoneCount} zones.", IconUrl = "https://wow.zamimg.com/images/wow/icons/large/inv_misc_book_09.jpg" });
    }
}
