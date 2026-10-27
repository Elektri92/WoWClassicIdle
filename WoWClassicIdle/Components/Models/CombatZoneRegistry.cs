// Models/CombatZoneRegistry.cs
namespace Models;

/// <summary>
/// Alle Kampfgebiete (Daten in .EasternKingdoms.cs / .Kalimdor.cs) und ihre fertig skalierten Gegner-Templates.
/// </summary>
public static partial class CombatZoneRegistry
{
    private static string Icon(string name) => $"https://wow.zamimg.com/images/wow/icons/large/{name}.jpg";

    private static ZoneMob Mob(string name, EnemyArchetype archetype, string icon, int weight = 3) =>
        new(name, archetype, Icon(icon), weight);

    /// <summary>Alle Zonen, sortiert nach Mindestlevel (dann Maxlevel, Name).</summary>
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

    // Dunkle Zonen-Themes: Kampf- und Reisehintergrund zeigen Nachthimmel
    private static readonly HashSet<string> NightThemes =
        ["duskwood", "tirisfal", "teldrassil", "darkshore", "sorrows", "plague", "deadwind", "felwood", "blasted", "moonglade", "ashenvale", "silverpine"];

    public static bool IsNightTheme(string theme) => NightThemes.Contains(theme);

    /// <summary>
    /// Zone ohne Spielerwahl (Headless-Simulation): die erste Zone, deren Levelbereich das Level enthält –
    /// bevorzugt mit dem niedrigsten Maxlevel, damit die Gegner möglichst nah am Spielerlevel liegen.
    /// </summary>
    public static CombatZone AutoZoneFor(int level) =>
        All.Where(z => z.MinLevel <= level && level <= z.MaxLevel).OrderBy(z => z.MaxLevel).FirstOrDefault()
        ?? All.Where(z => z.MinLevel <= level).OrderByDescending(z => z.MaxLevel).First();
}
