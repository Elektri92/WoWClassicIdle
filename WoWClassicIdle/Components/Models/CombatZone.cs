// Models/CombatZone.cs
namespace Models;

/// <summary>Landmass – travel between different landmasses is only possible by ship/zeppelin.</summary>
public enum Landmass { EasternKingdoms, Kalimdor, Teldrassil }

/// <summary>Enemy type of a combat zone; stats come from the <see cref="EnemyArchetype"/>.</summary>
public record ZoneMob(string Name, EnemyArchetype Archetype, string ImageUrl, int Weight = 3);

/// <summary>
/// Combat zone. <paramref name="X"/>/<paramref name="Y"/> are WoW world coordinates in yards (X = north, Y = west)
/// of the zone center – basis of the travel time. <paramref name="Theme"/> selects the gradient (CSS zone-theme-*).
/// </summary>
public record CombatZone(
    string Id,
    string Name,
    int MinLevel,
    int MaxLevel,
    Landmass Landmass,
    double X,
    double Y,
    string Theme,
    string Description,
    IReadOnlyList<ZoneMob> Mobs,
    DungeonDefinition? Dungeon = null)
{
    public bool IsDungeon => Dungeon is not null;

    /// <summary>Player level from which kills no longer grant XP (enemies are then 6+ levels below).</summary>
    public int NoXpLevel => MaxLevel + StaticHelpers.NoXpLevelDifference;

    /// <summary>Zone left behind at this level: no discovery XP, no objectives – only profession drops still matter.</summary>
    public bool IsOutgrownAt(int playerLevel) => !IsDungeon && playerLevel >= NoXpLevel;

    public string LevelRange => MinLevel == MaxLevel ? $"{MinLevel}" : $"{MinLevel}–{MaxLevel}";
}
