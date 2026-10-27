// Models/CombatZone.cs
namespace Models;

/// <summary>Landmasse – zwischen verschiedenen Landmassen geht es nur per Schiff/Zeppelin.</summary>
public enum Landmass { EasternKingdoms, Kalimdor, Teldrassil }

/// <summary>Gegnerart einer Kampfzone; Stats kommen aus dem <see cref="EnemyArchetype"/>.</summary>
public record ZoneMob(string Name, EnemyArchetype Archetype, string ImageUrl, int Weight = 3);

/// <summary>
/// Kampfgebiet. <paramref name="X"/>/<paramref name="Y"/> sind WoW-Weltkoordinaten in Yards (X = Norden, Y = Westen)
/// des Zonenzentrums – Grundlage der Reisezeit. <paramref name="Theme"/> wählt den Farbverlauf (CSS zone-theme-*).
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
    IReadOnlyList<ZoneMob> Mobs)
{
    /// <summary>Spielerlevel, ab dem Kills keine XP mehr geben (Gegner liegen dann 6+ Level darunter).</summary>
    public int NoXpLevel => MaxLevel + StaticHelpers.NoXpLevelDifference;

    public string LevelRange => MinLevel == MaxLevel ? $"{MinLevel}" : $"{MinLevel}–{MaxLevel}";
}
