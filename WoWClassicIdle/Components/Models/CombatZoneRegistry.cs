// Models/CombatZoneRegistry.cs
namespace Models;

/// <summary>
/// All combat zones (data in .EasternKingdoms.cs / .Kalimdor.cs) and their fully scaled enemy templates.
/// </summary>
public static partial class CombatZoneRegistry
{
    private static string Icon(string name) => $"https://wow.zamimg.com/images/wow/icons/large/{name}.jpg";

    private static ZoneMob Mob(string name, EnemyArchetype archetype, string icon, int weight = 3) =>
        new(name, archetype, Icon(icon), weight);

    /// <summary>All zones, sorted by minimum level (then max level, name).</summary>
    public static IReadOnlyList<CombatZone> All { get; } =
        EasternKingdoms().Concat(Kalimdor())
            .OrderBy(z => z.MinLevel).ThenBy(z => z.MaxLevel).ThenBy(z => z.Name)
            .ToList();

    private static readonly Dictionary<string, CombatZone> ById = All.ToDictionary(z => z.Id);

    private static readonly Dictionary<string, IReadOnlyList<EnemyTemplate>> TemplatesByZone =
        All.ToDictionary(z => z.Id, z => (IReadOnlyList<EnemyTemplate>)z.Mobs.Select(m => EnemyScaling.BuildTemplate(m, z)).ToList());

    public static CombatZone? Get(string? id) => id is not null && ById.TryGetValue(id, out var zone) ? zone : null;

    public static IReadOnlyList<EnemyTemplate> TemplatesFor(CombatZone zone) => TemplatesByZone[zone.Id];

    public static bool CanEnter(CombatZone zone, int playerLevel) => playerLevel >= zone.MinLevel;

    // Dark zone themes: combat and travel backgrounds show a night sky
    private static readonly HashSet<string> NightThemes =
        ["duskwood", "tirisfal", "teldrassil", "darkshore", "sorrows", "plague", "deadwind", "felwood", "blasted", "moonglade", "ashenvale", "silverpine"];

    public static bool IsNightTheme(string theme) => NightThemes.Contains(theme);

    /// <summary>Starting zone of the race like in WoW Classic – new characters start there.</summary>
    public static CombatZone? StartZoneFor(Race race) => Get(race switch
    {
        Race.Human               => "elwynn_forest",
        Race.Dwarf or Race.Gnome => "dun_morogh",
        Race.NightElf            => "teldrassil",
        Race.Orc or Race.Troll   => "durotar",
        Race.Undead              => "tirisfal_glades",
        Race.Tauren              => "mulgore",
        _                        => null
    });

    /// <summary>
    /// Zone without player choice (headless simulation): the first zone whose level range contains the level –
    /// preferring the lowest max level so the enemies are as close to the player level as possible.
    /// </summary>
    public static CombatZone AutoZoneFor(int level) =>
        All.Where(z => z.MinLevel <= level && level <= z.MaxLevel).OrderBy(z => z.MaxLevel).FirstOrDefault()
        ?? All.Where(z => z.MinLevel <= level).OrderByDescending(z => z.MaxLevel).First();
}
