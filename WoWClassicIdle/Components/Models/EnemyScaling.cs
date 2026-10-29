// Models/EnemyScaling.cs
namespace Models;

/// <summary>
/// Default enemy values per level (HP curve, attack formula, copper) – builds finished <see cref="EnemyTemplate"/>s from zone enemies.
/// </summary>
public static class EnemyScaling
{
    // Default HP per level (index 0 = level 1); up to L11 from WoW Classic anchors, after that Δ ≈ 24 + (L − 10) × 1.5
    private static readonly int[] BaseHp =
    [
          42,   55,   71,   86,  102,  120,  137,  156,  176,  198,  // L1–10
         222,  247,  274,  302,  331,  362,  394,  427,  462,  498,  // L11–20
         535,  573,  613,  654,  696,  738,  782,  828,  875,  923,  // L21–30
         972, 1023, 1075, 1128, 1182, 1238, 1295, 1353, 1412, 1485, // L31–40
        1548, 1612, 1677, 1744, 1812, 1882, 1954, 2027, 2102, 2171, // L41–50
        2247, 2324, 2402, 2482, 2563, 2645, 2728, 2813, 2900, 2990, // L51–60
    ];

    public static int Hp(int level, EnemyArchetype archetype) =>
        (int)Math.Round(BaseHp[Math.Clamp(level, 1, BaseHp.Length) - 1] * archetype.HpMultiplier);

    // BaseAttack ≈ 5 + level × 0.38 (rounded down so Lv 1–2 stays at 5 like after balancing)
    public static int Attack(int level, EnemyArchetype archetype) =>
        Math.Max(3, (int)(5 + level * 0.38) + archetype.AttackBonus);

    // Copper at the minimum level of the zone; the spawner adds +1 per level above
    public static int MinCopper(int level) => Math.Max(1, (int)Math.Round(level * 0.8));
    public static int MaxCopper(int level) => Math.Max(3, (int)Math.Round(level * 1.5));

    /// <summary>Enemy template for the whole level range of a zone.</summary>
    public static EnemyTemplate BuildTemplate(ZoneMob mob, CombatZone zone)
    {
        var levels = Enumerable.Range(zone.MinLevel, zone.MaxLevel - zone.MinLevel + 1).ToList();
        int midLevel = (zone.MinLevel + zone.MaxLevel) / 2;
        return new EnemyTemplate
        {
            Name            = mob.Name,
            Type            = mob.Archetype.Type,
            ImageUrl        = mob.ImageUrl,
            MinLevel        = zone.MinLevel,
            MaxLevel        = zone.MaxLevel,
            BaseMaxHp       = Hp(zone.MinLevel, mob.Archetype),
            MaxHpByLevel    = levels.Select(l => Hp(l, mob.Archetype)).ToList(),
            BaseAttack      = Attack(zone.MinLevel, mob.Archetype),
            AttackByLevel   = levels.Select(l => Attack(l, mob.Archetype)).ToList(),
            BaseDefense     = mob.Archetype.DefenseAt(midLevel),
            BaseSpeed       = mob.Archetype.Speed,
            MinCopperReward = MinCopper(zone.MinLevel),
            MaxCopperReward = MaxCopper(zone.MinLevel),
            BaseXpReward    = 15,
            Weight          = mob.Weight,
        };
    }
}
