// Models/Stats.cs

using Models;

public record Stats
{
    public int MaxHp { get; set; }
    public int MaxMana { get; set; }
    public int Attack { get; set; }
    public int Defense { get; set; }
    public double Speed { get; init; }        // Aktionen pro Sekunde (z. B. 1.0)
    public int Level { get; init; } = 1;
    public int CopperReward { get; init; }
    public int XpReward { get; init; }

    /// <summary>Strength, Agility, Stamina, Intelligence, Spirit – abhängig von Klasse und Level.</summary>
    public PrimaryStats Primary { get; init; } = PrimaryStats.Zero;

    /// <summary>Mana-Regeneration alle 5 Sekunden (Basis + Spirit).</summary>
    public int ManaRegenPer5 { get; init; }

    /// <summary>Passive HP-Regeneration alle 5 Sekunden (aus Spirit, alle Klassen).</summary>
    public int HpRegenPer5 { get; init; }

    /// <summary>Nahkampf-Angriffskraft – skaliert Auto-Attacks (14 AP = +1 DPS).</summary>
    public int AttackPower { get; init; }

    /// <summary>Waffenschaden-Spanne für Auto-Attacks.</summary>
    public int WeaponMinDamage { get; init; }
    public int WeaponMaxDamage { get; init; }

    /// <summary>Klassenfaktor auf Nahkampfschaden – Auto-Attacks und Waffenschläge.</summary>
    public double MeleeDamageMultiplier { get; init; } = 1.0;

    /// <summary>Klassenfaktor nur auf Auto-Attack-Schaden (1.0 = kein Aufschlag/Abzug).</summary>
    public double AutoAttackDamageMultiplier { get; init; } = 1.0;

    /// <summary>
    /// Kritische Trefferchance als Anteil (0.05 = 5 %). Gilt für allen verursachten Schaden,
    /// jede Heilung und jeden Schild – der Effekt wird bei einem Crit verdoppelt.
    /// </summary>
    public double CritChance { get; init; }

    /// <summary>
    /// Angriffsgeschwindigkeits-Bonus als Anteil (0.10 = +10 %).
    /// Wirkt auf Auto-Attacks und auf die Cooldowns der Fähigkeiten.
    /// </summary>
    public double AttackSpeedBonus { get; init; }

    /// <summary>Multiplikator aus <see cref="AttackSpeedBonus"/> (1.0 = keine Erhöhung).</summary>
    public double AttackSpeedMultiplier => 1.0 + AttackSpeedBonus;

    /// <summary>Prozentualer Schadensreduktions-Bonus aus Talenten (0.05 = 5% weniger Schaden). Klemmt auf [0, 1).</summary>
    public double DamageTakenReduction { get; init; }

    /// <summary>Multiplikator auf alle Heilungen (1.0 = keine Änderung).</summary>
    public double HealingMultiplier { get; init; } = 1.0;
}
