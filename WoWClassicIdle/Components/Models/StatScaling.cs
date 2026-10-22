// Models/StatScaling.cs
namespace Models
{
    /// <summary>
    /// Wie die Primärattribute in abgeleitete Werte umgerechnet werden.
    /// </summary>
    public static class StatScaling
    {
        public const int HpPerStamina            = 2;
        public const int ManaPerIntelligence     = 5;
        public const int SpiritPerManaRegen      = 5;
        public const int ManaRegenPerSpiritStep  = 2;   // +2 Mana / 5s je 5 Spirit
        public const int BaseManaRegenPer5       = 3;

        /// <summary>Kritische Trefferchance, die jede Klasse ohne Ausrüstung mitbringt (5 %).</summary>
        public const double DefaultCritChance       = 0.05;

        /// <summary>Angriffsgeschwindigkeits-Bonus, den jede Klasse ohne Ausrüstung mitbringt (0 %).</summary>
        public const double DefaultAttackSpeedBonus = 0.0;

        /// <summary>Warrior und Rogue nutzen Rage/Energy – Int und Spirit wirken bei ihnen nicht.</summary>
        public static bool UsesMana(CharacterClass cls) =>
            cls is not (CharacterClass.Warrior or CharacterClass.Rogue);

        public static int BonusHp(PrimaryStats p) => p.Stamina * HpPerStamina;

        public static int BonusMana(CharacterClass cls, PrimaryStats p) =>
            UsesMana(cls) ? p.Intelligence * ManaPerIntelligence : 0;

        public static int ManaRegenPer5(CharacterClass cls, PrimaryStats p) =>
            UsesMana(cls) ? BaseManaRegenPer5 + p.Spirit * ManaRegenPerSpiritStep / SpiritPerManaRegen : 0;

        /// <summary>
        /// Nahkampf-Angriffskraft nach WoW-Classic-Formeln. 14 AP = +1 DPS auf Auto-Attacks.
        /// </summary>
        public static int AttackPower(CharacterClass cls, int level, PrimaryStats p) => Math.Max(0, cls switch
        {
            CharacterClass.Warrior or CharacterClass.Paladin or CharacterClass.Shaman
                                   => level * 3 + p.Strength * 2 - 20,
            CharacterClass.Rogue or CharacterClass.Hunter
                                   => level * 2 + p.Strength + p.Agility - 20,
            CharacterClass.Druid   => p.Strength * 2 - 20,
            _                      => p.Strength - 10,   // Mage, Priest, Warlock
        });

        /// <summary>
        /// Klassenfaktor auf Nahkampfschaden (Auto-Attacks und Waffenschläge wie Heroic Strike).
        /// Caster/Hybride leben in Classic nicht vom Weißschaden; Nahkämpfer sind auf Idle-Niveau halbiert.
        /// </summary>
        public static double MeleeDamageMultiplier(CharacterClass cls) => cls switch
        {
            CharacterClass.Warrior or CharacterClass.Rogue or CharacterClass.Hunter    => 0.5,
            CharacterClass.Paladin or CharacterClass.Druid                             => 0.4,
            CharacterClass.Shaman                                                      => 0.28,  // −30 % ggü. Hybriden (Balancing)
            _                                                                          => 0.5,   // Mage, Priest, Warlock
        };

        /// <summary>
        /// Klassenfaktor ausschließlich auf Auto-Attack-Schaden (Waffenschläge wie Heroic Strike bleiben unberührt).
        /// </summary>
        public static double AutoAttackDamageMultiplier(CharacterClass cls) => cls switch
        {
            CharacterClass.Druid   => 0.5,   // −50 % Weißschaden (Balancing)
            CharacterClass.Hunter  => 0.95,  // −5 % Weißschaden (Balancing)
            _                      => 1.0,
        };

        /// <summary>
        /// Kritische Trefferchance je Klasse (Anteil, 0.05 = 5 %).
        /// Ein Crit verdoppelt Schaden, Heilung und Schilde.
        /// Abweichende Klassenwerte hier als eigenen switch-Arm ergänzen.
        /// </summary>
        public static double CritChance(CharacterClass cls) => cls switch
        {
            _ => DefaultCritChance,
        };

        /// <summary>
        /// Angriffsgeschwindigkeits-Bonus je Klasse (Anteil, 0.10 = +10 %).
        /// Beschleunigt Auto-Attacks und die Cooldowns der Fähigkeiten.
        /// Abweichende Klassenwerte hier als eigenen switch-Arm ergänzen.
        /// </summary>
        public static double AttackSpeedBonus(CharacterClass cls) => cls switch
        {
            _ => DefaultAttackSpeedBonus,
        };
    }
}
