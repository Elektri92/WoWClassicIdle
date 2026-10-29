// Models/ZoneObjectiveRegistry.cs
namespace Models;

public enum ZoneObjectiveKind { SlayMob, SlayRare }

/// <summary>
/// One-time objective of a combat zone (per character), based on Classic quests.
/// <paramref name="MobName"/> only for <see cref="ZoneObjectiveKind.SlayMob"/>.
/// </summary>
public record ZoneObjective(string Id, string ZoneId, ZoneObjectiveKind Kind, string? MobName, string ImageUrl, int Required)
{
    public string Label => Kind == ZoneObjectiveKind.SlayRare
        ? "Defeat a rare enemy"
        : $"Slay {Required} {MobName}";
}

/// <summary>
/// Zone objectives ("Zone Mastery") and reward formulas for objectives and discovery.
/// Every zone: hunt the first two enemy types + defeat a rare; all done → guaranteed item.
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

    /// <summary>Reward tier: player level, clamped to the level range of the zone (old zones give little).</summary>
    public static int RewardLevel(CombatZone zone, int playerLevel) => Math.Clamp(playerLevel, zone.MinLevel, zone.MaxLevel);

    /// <summary>Discovery XP on the first visit: as much as 5 kills at reward tier.</summary>
    public static int DiscoveryXp(CombatZone zone, int playerLevel) =>
        StaticHelpers.GetEnemyXpReward(RewardLevel(zone, playerLevel)) * 5;

    /// <summary>XP per completed objective: as much as 12 kills at reward tier.</summary>
    public static int ObjectiveXp(CombatZone zone, int playerLevel) =>
        StaticHelpers.GetEnemyXpReward(RewardLevel(zone, playerLevel)) * 12;

    /// <summary>Copper per completed objective (≈ loot from 30–40 kills).</summary>
    public static long ObjectiveCopper(CombatZone zone, int playerLevel) => 40L * RewardLevel(zone, playerLevel);

    /// <summary>Chance that the mastery item is blue instead of green (if there is a blue table).</summary>
    public const double MasteryBlueChance = 0.25;
}
