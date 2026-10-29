// Models/StatScaling.cs
namespace Models
{
    /// <summary>
    /// How the primary attributes are converted into derived values.
    /// </summary>
    public static class StatScaling
    {
        public const int HpPerStamina            = 2;
        public const int ManaPerIntelligence     = 5;
        public const int SpiritPerManaRegen      = 5;
        public const int ManaRegenPerSpiritStep  = 2;   // +2 mana / 5s per 5 Spirit
        public const int BaseManaRegenPer5       = 3;
        public const int SpiritPerHpRegen        = 5;
        public const int HpRegenPerSpiritStep    = 1;   // +1 HP / 5s per 5 Spirit (all classes)

        /// <summary>Critical strike chance every class has without gear (5 %).</summary>
        public const double DefaultCritChance       = 0.05;

        /// <summary>Attack speed bonus every class has without gear (0 %).</summary>
        public const double DefaultAttackSpeedBonus = 0.0;

        /// <summary>Warrior and Rogue use Rage/Energy – Int and Spirit have no effect for them.</summary>
        public static bool UsesMana(CharacterClass cls) =>
            cls is not (CharacterClass.Warrior or CharacterClass.Rogue);

        /// <summary>
        /// Class whose formulas melee values scale with (AP, melee/auto-attack factor,
        /// HP regeneration, crit, attack speed): Druids in Cat Form like Rogues, in Bear Form like Warriors.
        /// </summary>
        public static CharacterClass MeleeScalingClass(CharacterClass cls, DruidForm form) => (cls, form) switch
        {
            (CharacterClass.Druid, DruidForm.Cat)  => CharacterClass.Rogue,
            (CharacterClass.Druid, DruidForm.Bear) => CharacterClass.Warrior,
            _                                      => cls,
        };

        public static int BonusHp(PrimaryStats p) => p.Stamina * HpPerStamina;

        public static int BonusMana(CharacterClass cls, PrimaryStats p) =>
            UsesMana(cls) ? p.Intelligence * ManaPerIntelligence : 0;

        public static int ManaRegenPer5(CharacterClass cls, PrimaryStats p) =>
            UsesMana(cls) ? BaseManaRegenPer5 + p.Spirit * ManaRegenPerSpiritStep / SpiritPerManaRegen : 0;

        /// <summary>
        /// Passive HP regeneration every 5 seconds from Spirit – works for every class,
        /// scales with <see cref="HpRegenClassFactor"/>.
        /// </summary>
        public static int HpRegenPer5(CharacterClass cls, PrimaryStats p) =>
            (int)Math.Round(p.Spirit * HpRegenPerSpiritStep / (double)SpiritPerHpRegen * HpRegenClassFactor(cls));

        /// <summary>
        /// Like in Classic: classes without their own healing regenerate considerably more HP per Spirit,
        /// cloth wearers with healing spells less.
        /// </summary>
        public static double HpRegenClassFactor(CharacterClass cls) => cls switch
        {
            CharacterClass.Warrior or CharacterClass.Rogue or CharacterClass.Hunter    => 1.5,
            CharacterClass.Shaman                                                      => 0.8,   // no own heal until Lv 18
            CharacterClass.Paladin or CharacterClass.Druid                             => 0.6,   // Spirit loot + own healing
            _                                                                          => 0.75,  // Mage, Priest, Warlock
        };

        /// <summary>
        /// Does the stat do anything for the class according to the formulas here? (consumables are chosen based on this)
        /// Strength → attack power (not for cloth wearers), Agility → Rogue/Hunter only,
        /// Stamina and Spirit (HP regeneration) → all, Intellect → mana classes only.
        /// </summary>
        public static bool IsUsefulStat(CharacterClass cls, PrimaryStats bonus) =>
            bonus.Stamina > 0
            || bonus.Spirit > 0
            || (bonus.Strength > 0 && cls is not (CharacterClass.Mage or CharacterClass.Priest or CharacterClass.Warlock))
            || (bonus.Agility > 0 && cls is CharacterClass.Rogue or CharacterClass.Hunter)
            || (bonus.Intelligence > 0 && UsesMana(cls));

        /// <summary>
        /// Melee attack power according to WoW Classic formulas. 14 AP = +1 DPS on auto-attacks.
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
        /// Class factor on melee damage (auto-attacks and weapon strikes like Heroic Strike).
        /// Casters/hybrids in Classic don't live off white damage; melee classes are halved to idle level.
        /// </summary>
        public static double MeleeDamageMultiplier(CharacterClass cls) => cls switch
        {
            CharacterClass.Warrior or CharacterClass.Rogue or CharacterClass.Hunter    => 0.5,
            CharacterClass.Paladin or CharacterClass.Druid                             => 0.4,
            CharacterClass.Shaman                                                      => 0.28,  // −30 % vs. hybrids (balancing)
            _                                                                          => 0.5,   // Mage, Priest, Warlock
        };

        /// <summary>
        /// Class factor exclusively on auto-attack damage (weapon strikes like Heroic Strike stay unaffected).
        /// </summary>
        public static double AutoAttackDamageMultiplier(CharacterClass cls) => cls switch
        {
            CharacterClass.Druid    => 0.50,  // −50 % white damage (balancing)
            CharacterClass.Hunter   => 0.70,  // −30 % white damage (balancing)
            CharacterClass.Warrior  => 0.95,  // −5 % white damage (balancing)
            CharacterClass.Rogue    => 0.85,  // −15 % white damage (balancing)
            _                       => 1.0,
        };

        /// <summary>
        /// Critical strike chance per class (share, 0.05 = 5 %).
        /// A crit doubles damage, healing and shields.
        /// Add deviating class values here as their own switch arm.
        /// </summary>
        public static double CritChance(CharacterClass cls) => cls switch
        {
            _ => DefaultCritChance,
        };

        /// <summary>
        /// Attack speed bonus per class (share, 0.10 = +10 %).
        /// Speeds up auto-attacks and ability cooldowns.
        /// Add deviating class values here as their own switch arm.
        /// </summary>
        public static double AttackSpeedBonus(CharacterClass cls) => cls switch
        {
            _ => DefaultAttackSpeedBonus,
        };
    }
}
