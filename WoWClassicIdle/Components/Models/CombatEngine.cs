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
    /// Auto-Attack wie in WoW Classic: Waffenschaden (zufällig in der Spanne)
    /// + AttackPower / 14 × Waffengeschwindigkeit, mit Klassenfaktor für Auto-Attacks.
    /// </summary>
    public static int CalculateAutoAttack(Stats stats, Random rng, double damageMultiplier = 1.0) =>
        Math.Max(1, (int)Math.Round(RollWeaponSwing(stats, rng) * stats.MeleeDamageMultiplier * stats.AutoAttackDamageMultiplier * damageMultiplier));

    /// <summary>
    /// Schaden einer Fähigkeit: Nahkampf-Schläge = (Waffenschlag + Bonus) × Nahkampf-Faktor, Zauber = fixer Wert.
    /// <paramref name="damageMultiplier"/> = Bonus durch aktive Buffs.
    /// </summary>
    public static int CalculateAbilityDamage(Ability ability, Stats stats, Random rng, double damageMultiplier = 1.0)
    {
        double dmg = ability.Damage;
        if (ability.AddsWeaponDamage)
            dmg = (dmg + RollWeaponSwing(stats, rng)) * stats.MeleeDamageMultiplier;
        return Math.Max(1, (int)Math.Round(dmg * damageMultiplier));
    }

    private static double RollWeaponSwing(Stats stats, Random rng)
    {
        int weaponDmg = rng.Next(stats.WeaponMinDamage, stats.WeaponMaxDamage + 1);
        return weaponDmg + stats.AttackPower / 14.0 * stats.Speed;
    }

    /// <summary>Ein kritischer Treffer verdoppelt den Effekt (Schaden, Heilung, Schild).</summary>
    public const double CritMultiplier = 2.0;

    /// <summary>Würfelt, ob der nächste Effekt kritisch trifft (<see cref="Stats.CritChance"/>).</summary>
    public static bool RollCrit(Stats stats, Random rng) => rng.NextDouble() < stats.CritChance;

    /// <summary>Verdoppelt <paramref name="amount"/>, wenn <paramref name="isCrit"/> gesetzt ist.</summary>
    public static int ApplyCrit(int amount, bool isCrit) =>
        isCrit ? (int)Math.Round(amount * CritMultiplier) : amount;

    public static bool IsKill(int damage, int targetHp) => damage >= targetHp;
}
