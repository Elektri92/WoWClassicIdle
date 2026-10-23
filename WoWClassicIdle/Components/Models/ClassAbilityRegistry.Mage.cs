namespace Models;

public static partial class ClassAbilityRegistry
{
    private static IReadOnlyList<Ability> MageAbilities => new[]
    {
        new Ability
        {
            Id = "Fireball_r1",
            Name = "Fireball",
            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/spell_fire_flamebolt.jpg",
            Cooldown = 1.5,
            ManaCost = 18,
            Damage = 14,        // -20% nerf (was 18)
            LevelRequired = 1,
            Rank = 1,
            ExclusiveGroup = "mage_bolt",
        },
        new Ability
        {
            Id = "Fireball_r2",
            Name = "Fireball",
            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/spell_fire_flamebolt.jpg",
            Cooldown = 1.5,
            ManaCost = 18,
            Damage = 15,        // +10 % gegenüber Rank 1
            LevelRequired = 10,
            Rank = 2,
            ExclusiveGroup = "mage_bolt",
        },
        new Ability
        {
            Id = "Frostbolt_r1",
            Name = "Frostbolt",
            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/spell_frost_frostbolt02.jpg",
            Cooldown = 1.5,
            ManaCost = 18,
            Damage = 12,
            DebuffDuration = 5,
            DebuffAttackSpeedReduction = 0.05,  // −5 % Angriffstempo
            LevelRequired = 1,
            Rank = 1,
            ExclusiveGroup = "mage_bolt",
        },
        new Ability
        {
            Id = "Frostbolt_r2",
            Name = "Frostbolt",
            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/spell_frost_frostbolt02.jpg",
            Cooldown = 1.5,
            ManaCost = 18,
            Damage = 13,        // +10 % gegenüber Rank 1
            DebuffDuration = 5,
            DebuffAttackSpeedReduction = 0.05,
            LevelRequired = 10,
            Rank = 2,
            ExclusiveGroup = "mage_bolt",
        },
        new Ability
        {
            Id = "Arcane_Intellect_r1",
            Name = "Arcane Intellect",
            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/spell_holy_magicalsentry.jpg",
            Cooldown = 0,
            ManaCost = 0,
            BuffDuration = 60,          // 60 s in idle context
            BuffIntellectBonus = 30,    // +30 Int → +450 max mana (+100% buff)
            BuffSpiritBonus = 30,       // +30 Spirit (+100% buff)
            LevelRequired = 2,
            Rank = 1,
        },
        new Ability
        {
            Id = "Conjure_Water_r1",
            Name = "Conjure Water",
            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/inv_drink_10.jpg",
            Cooldown = 60,
            ManaCost = 0,
            BuffDuration = 20,              // regeneriert über 20 s
            MotManaRestorePct = 0.14,       // 12 % maximales Mana
            MotTickInterval = 5.0,          // 4 Ticks à 2,5 %
            LevelRequired = 3,
            Rank = 1,
        },
        new Ability
        {
            Id = "Conjure_Water_r2",
            Name = "Conjure Water",
            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/inv_drink_10.jpg",
            Cooldown = 50,                  // 10 s weniger CD gegenüber Rank 1
            ManaCost = 0,
            BuffDuration = 20,
            MotManaRestorePct = 0.14,
            MotTickInterval = 5.0,
            LevelRequired = 11,
            Rank = 2,
        },
        new Ability
        {
            Id = "Conjure_Food_r1",
            Name = "Conjure Food",
            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/inv_misc_food_30.jpg",
            Cooldown = 60,
            ManaCost = 10,
            BuffDuration = 20,              // HoT über 20 s
            HotHealPct = 0.24,              // 24 % max. HP gesamt
            HotTickInterval = 5.0,          // 4 Ticks à 6 %
            LevelRequired = 8,
            Rank = 1,
        },
        new Ability
        {
            Id = "Conjure_Food_r2",
            Name = "Conjure Food",
            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/inv_misc_food_30.jpg",
            Cooldown = 50,                  // 10 s weniger CD gegenüber Rank 1
            ManaCost = 10,
            BuffDuration = 20,
            HotHealPct = 0.24,
            HotTickInterval = 5.0,
            LevelRequired = 11,
            Rank = 2,
        },
    };
}
