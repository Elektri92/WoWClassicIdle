// Models/Stats.cs

using Models;

public record Stats
{
    public int MaxHp { get; set; }
    public int MaxMana { get; set; }
    public int Attack { get; set; }
    public int Defense { get; set; }
    public double Speed { get; init; }        // Actions per second (e.g. 1.0)
    public int Level { get; init; } = 1;
    public int CopperReward { get; init; }
    public int XpReward { get; init; }

    /// <summary>Strength, Agility, Stamina, Intelligence, Spirit – depending on class and level.</summary>
    public PrimaryStats Primary { get; init; } = PrimaryStats.Zero;

    /// <summary>Mana regeneration every 5 seconds (base + Spirit).</summary>
    public int ManaRegenPer5 { get; init; }

    /// <summary>Passive HP regeneration every 5 seconds (from Spirit, all classes).</summary>
    public int HpRegenPer5 { get; init; }

    /// <summary>Melee attack power – scales auto-attacks (14 AP = +1 DPS).</summary>
    public int AttackPower { get; init; }

    /// <summary>Weapon damage range for auto-attacks.</summary>
    public int WeaponMinDamage { get; init; }
    public int WeaponMaxDamage { get; init; }

    /// <summary>Class factor on melee damage – auto-attacks and weapon strikes.</summary>
    public double MeleeDamageMultiplier { get; init; } = 1.0;

    /// <summary>Class factor on auto-attack damage only (1.0 = no increase/reduction).</summary>
    public double AutoAttackDamageMultiplier { get; init; } = 1.0;

    /// <summary>
    /// Critical strike chance as a share (0.05 = 5 %). Applies to all damage dealt,
    /// every heal and every shield – the effect is doubled on a crit.
    /// </summary>
    public double CritChance { get; init; }

    /// <summary>
    /// Attack speed bonus as a share (0.10 = +10 %).
    /// Affects auto-attacks and ability cooldowns.
    /// </summary>
    public double AttackSpeedBonus { get; init; }

    /// <summary>Multiplier from <see cref="AttackSpeedBonus"/> (1.0 = no increase).</summary>
    public double AttackSpeedMultiplier => 1.0 + AttackSpeedBonus;

    /// <summary>Percentage damage reduction bonus from talents (0.05 = 5% less damage). Clamped to [0, 1).</summary>
    public double DamageTakenReduction { get; init; }

    /// <summary>Multiplier on all healing (1.0 = no change).</summary>
    public double HealingMultiplier { get; init; } = 1.0;
}
