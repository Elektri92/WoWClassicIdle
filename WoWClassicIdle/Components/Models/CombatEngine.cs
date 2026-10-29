// Combat/CombatEngine.cs
using Models;
public static class CombatEngine
{
    /// <summary>
    /// Berechnet Schaden: (ATK - DEF/2) ± 15% Varianz, min. 1
    /// </summary>
    public static int CalculateDamage(int attack, int defense, Random rng)
    {
        var baseDamage = Math.Max(1, attack - defense / 2);
        var variance = 0.85 + rng.NextDouble() * 0.30; // 85–115 %
        return Math.Max(1, (int)(baseDamage * variance));
    }

    /// <summary>
    /// Auto-attack like in WoW Classic: weapon damage (random within the range)
    /// + AttackPower / 14 × weapon speed, with class factor for auto-attacks.
    /// </summary>
    public static int CalculateAutoAttack(Stats stats, Random rng, double damageMultiplier = 1.0) =>
        Math.Max(1, (int)Math.Round(RollWeaponSwing(stats, rng) * stats.MeleeDamageMultiplier * stats.AutoAttackDamageMultiplier * damageMultiplier));

    /// <summary>
    /// Damage of an ability: melee strikes = (weapon swing + bonus) × melee factor, spells = fixed value.
    /// <paramref name="damageMultiplier"/> = bonus from active buffs.
    /// </summary>
    public static int CalculateAbilityDamage(Ability ability, Stats stats, Random rng, double damageMultiplier = 1.0)
    {
        double dmg = ability.Damage;
        if (ability.AddsWeaponDamage)
            dmg = (dmg + RollWeaponSwing(stats, rng)) * stats.MeleeDamageMultiplier;
        return Math.Max(1, (int)Math.Round(dmg * ability.DamageMultiplier * damageMultiplier));
    }

    /// <summary>
    /// Expected total damage of a cast (direct damage with average weapon swing + complete DoT),
    /// without crit and buffs – only used for prioritization.
    /// </summary>
    public static double EstimateAbilityDamage(Ability ability, Stats stats)
    {
        double dmg = ability.Damage;
        if (ability.AddsWeaponDamage)
        {
            double avgSwing = (stats.WeaponMinDamage + stats.WeaponMaxDamage) / 2.0 + stats.AttackPower / 14.0 * stats.Speed;
            dmg = (dmg + avgSwing) * stats.MeleeDamageMultiplier;
        }
        return dmg * ability.DamageMultiplier * ability.HitCount + ability.DebuffDotDamage * ability.DebuffDotTickCount;
    }

    private static double RollWeaponSwing(Stats stats, Random rng)
    {
        int weaponDmg = rng.Next(stats.WeaponMinDamage, stats.WeaponMaxDamage + 1);
        return weaponDmg + stats.AttackPower / 14.0 * stats.Speed;
    }

    /// <summary>A critical hit doubles the effect (damage, healing, shield).</summary>
    public const double CritMultiplier = 2.0;

    /// <summary>Rolls whether the next effect is a critical hit (<see cref="Stats.CritChance"/>).</summary>
    public static bool RollCrit(Stats stats, Random rng) => rng.NextDouble() < stats.CritChance;

    /// <summary>Doubles <paramref name="amount"/> if <paramref name="isCrit"/> is set.</summary>
    public static int ApplyCrit(int amount, bool isCrit) =>
        isCrit ? (int)Math.Round(amount * CritMultiplier) : amount;

    public static bool IsKill(int damage, int targetHp) => damage >= targetHp;
}
