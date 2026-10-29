// Models/ZoneObjectiveRegistry.cs
namespace Models;

public enum ZoneObjectiveKind { SlayMob, SlayRare }

/// <summary>
/// Einmaliges Ziel einer Kampfzone (je Charakter), angelehnt an Classic-Quests.
/// <paramref name="MobName"/> nur bei <see cref="ZoneObjectiveKind.SlayMob"/>.
/// </summary>
public record ZoneObjective(string Id, string ZoneId, ZoneObjectiveKind Kind, string? MobName, string ImageUrl, int Required)
{
    public string Label => Kind == ZoneObjectiveKind.SlayRare
        ? "Defeat a rare enemy"
        : $"Slay {Required} {MobName}";
}

/// <summary>
/// Zonen-Ziele („Zone Mastery“) und Belohnungsformeln für Ziele und Entdeckung.
/// Jede Zone: die beiden ersten Gegnerarten jagen + einen Rare besiegen; alle erledigt → garantiertes Item.
/// </summary>
public static class ZoneObjectiveRegistry
{
    private const int SlayCount = 20;
    private const string RareIcon = "https://wow.zamimg.com/images/wow/icons/large/inv_misc_head_dragon_01.jpg";

    private static readonly Dictionary<string, IReadOnlyList<ZoneObjective>> ByZone =
        CombatZoneRegistry.All.ToDictionary(z => z.Id, Build);

    private static IReadOnlyList<ZoneObjective> Build(CombatZone zone) =>
    [
        ..zone.Mobs.Take(2).Select(m =>
            new ZoneObjective($"{zone.Id}:slay:{m.Name}", zone.Id, ZoneObjectiveKind.SlayMob, m.Name, m.ImageUrl, SlayCount)),
        new ZoneObjective($"{zone.Id}:rare", zone.Id, ZoneObjectiveKind.SlayRare, null, RareIcon, 1),
    ];

    public static IReadOnlyList<ZoneObjective> For(CombatZone zone) => ByZone[zone.Id];

    /// <summary>Belohnungsstufe: Spielerlevel, auf den Levelbereich der Zone begrenzt (alte Zonen geben wenig).</summary>
    public static int RewardLevel(CombatZone zone, int playerLevel) => Math.Clamp(playerLevel, zone.MinLevel, zone.MaxLevel);

    /// <summary>Entdeckungs-XP beim ersten Betreten: so viel wie 5 Kills auf Belohnungsstufe.</summary>
    public static int DiscoveryXp(CombatZone zone, int playerLevel) =>
        StaticHelpers.GetEnemyXpReward(RewardLevel(zone, playerLevel)) * 5;

    /// <summary>XP je erledigtem Ziel: so viel wie 12 Kills auf Belohnungsstufe.</summary>
    public static int ObjectiveXp(CombatZone zone, int playerLevel) =>
        StaticHelpers.GetEnemyXpReward(RewardLevel(zone, playerLevel)) * 12;

    /// <summary>Kupfer je erledigtem Ziel (≈ Beute aus 30–40 Kills).</summary>
    public static long ObjectiveCopper(CombatZone zone, int playerLevel) => 40L * RewardLevel(zone, playerLevel);

    /// <summary>Chance, dass das Mastery-Item blau statt grün ist (sofern es eine blaue Tabelle gibt).</summary>
    public const double MasteryBlueChance = 0.25;
}
