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
            Cooldown = 0,
            CastTime = 2.5,     // 2,5 s Cast-Zeit statt Cooldown
            ManaCost = 18,
            Damage = 14,
            LevelRequired = 1,
            Rank = 1,
            ExclusiveGroup = "mage_bolt",
            SpecHint = 1,       // Fire-Baum
        },
        new Ability
        {
            Id = "Fireball_r2",
            Name = "Fireball",
            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/spell_fire_flamebolt.jpg",
            Cooldown = 0,
            CastTime = 2.5,     // 2,5 s Cast-Zeit statt Cooldown
            ManaCost = 20,
            Damage = 16,        // +2 gegenüber Rank 1
            LevelRequired = 10,
            Rank = 2,
            ExclusiveGroup = "mage_bolt",
            SpecHint = 1,       // Fire-Baum
        },
        new Ability
        {
            Id = "Frostbolt_r1",
            Name = "Frostbolt",
            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/spell_frost_frostbolt02.jpg",
            Cooldown = 0,
            CastTime = 2.5,     // 2,5 s Cast-Zeit statt Cooldown
            ManaCost = 18,
            Damage = 13,        // 1 weniger als Fireball – Utility durch Slow gleicht aus
            DebuffDuration = 5,
            DebuffAttackSpeedReduction = 0.05,  // −5 % Angriffstempo
            LevelRequired = 1,
            Rank = 1,
            ExclusiveGroup = "mage_bolt",
            SpecHint = 2,       // Frost-Baum
        },
        new Ability
        {
            Id = "Frostbolt_r2",
            Name = "Frostbolt",
            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/spell_frost_frostbolt02.jpg",
            Cooldown = 0,
            CastTime = 2.5,     // 2,5 s Cast-Zeit statt Cooldown
            ManaCost = 20,
            Damage = 15,        // +2 gegenüber Rank 1, Slow bleibt
            DebuffDuration = 5,
            DebuffAttackSpeedReduction = 0.05,
            LevelRequired = 10,
            Rank = 2,
            ExclusiveGroup = "mage_bolt",
            SpecHint = 2,       // Frost-Baum
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
            Id = "Arcane_Intellect_r2",
            Name = "Arcane Intellect",
            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/spell_holy_magicalsentry.jpg",
            Cooldown = 0,
            ManaCost = 0,
            BuffDuration = 60,
            BuffIntellectBonus = 45,    // ×1,5 gegenüber Rank 1
            BuffSpiritBonus = 45,       // ×1,5 gegenüber Rank 1
            LevelRequired = 12,
            Rank = 2,
        },
        new Ability
        {
            Id = "Conjure_Water_r1",
            Name = "Conjure Water",
            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/inv_drink_10.jpg",
            Cooldown = 60,
            CastTime = 1.0,
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
            CastTime = 1.0,
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
            Cooldown = 40,
            CastTime = 1.0,
            ManaCost = 10,
            BuffDuration = 20,              // HoT über 20 s
            HotHealPct = 0.25,              // 25 % max. HP gesamt (war 35 % – Mage ab Lv 4 kaum noch Tode)
            HotTickInterval = 5.0,          // 4 Ticks à 6,25 %
            LevelRequired = 4,              // war 8 – Lv-4-Ausreißer entstand nur mit HP-Regen-Faktor 1,0
            Rank = 1,
        },
        new Ability
        {
            Id = "Fire_Blast_r1",
            Name = "Fire Blast",
            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/spell_fire_fireball.jpg",
            Cooldown = 8,
            ManaCost = 18,
            Damage = 22,                        // Sofortschaden ohne Cast-Zeit
            LevelRequired = 14,
            Rank = 1,
        },
        new Ability
        {
            Id = "Arcane_Missiles_r1",
            Name = "Arcane Missiles",
            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/spell_nature_starfall.jpg",
            Cooldown = 0,
            CastTime = 2.5,     // 2,5 s Cast-Zeit; kein Cooldown
            ManaCost = 20,
            Damage = 15,        // Etwas mehr als Fireball/Frostbolt, kein Utility-Effekt
            LevelRequired = 1,
            Rank = 1,
            ExclusiveGroup = "mage_bolt",
            SpecHint = 0,       // Arcane-Baum
        },
        new Ability
        {
            Id = "Arcane_Missiles_r2",
            Name = "Arcane Missiles",
            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/spell_nature_starfall.jpg",
            Cooldown = 0,
            CastTime = 2.5,     // 2,5 s Cast-Zeit; kein Cooldown
            ManaCost = 22,
            Damage = 19,        // +4 gegenüber Rank 1; höchster Schaden der Bolt-Gruppe ohne Slow
            LevelRequired = 10,
            Rank = 2,
            ExclusiveGroup = "mage_bolt",
            SpecHint = 0,       // Arcane-Baum
        },
        new Ability
        {
            Id = "Frost_Nova_r1",
            Name = "Frost Nova",
            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/spell_frost_frostnova.jpg",
            Cooldown = 30,
            ManaCost = 15,
            DebuffDuration = 4,                 // Einstun für 4 s
            DebuffIsStun = true,
            LevelRequired = 24,
            Rank = 1,
        },
        new Ability
        {
            Id = "Conjure_Food_r2",
            Name = "Conjure Food",
            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/inv_misc_food_30.jpg",
            Cooldown = 30,                  // 10 s weniger CD gegenüber Rank 1
            CastTime = 1.0,
            ManaCost = 10,
            BuffDuration = 20,
            HotHealPct = 0.35,
            HotTickInterval = 5.0,
            LevelRequired = 11,
            Rank = 2,
        },

        // ─── Talent-exklusive Fähigkeiten (nur über Talentsystem freischaltbar) ───
        new Ability
        {
            Id                   = "Evocation_r1",
            Name                 = "Evocation",
            IconUrl              = "https://wow.zamimg.com/images/wow/icons/large/spell_nature_purge.jpg",
            Cooldown             = 60,
            ManaCost             = 0,
            BuffDuration         = 8,
            MotManaRestorePct    = 0.80,
            MotTickInterval      = 2.0,
            LevelRequired        = 1,
            TalentUnlockOnly     = true,
            CastWhenManaBelowPct = 0.20,
        },
        new Ability
        {
            Id               = "Ignite_r1",
            Name             = "Ignite",
            IconUrl          = "https://wow.zamimg.com/images/wow/icons/large/spell_fire_incinerate.jpg",
            ManaCost         = 0,
            IsPassive        = true,
            LevelRequired    = 1,
            TalentUnlockOnly = true,
        },
        new Ability
        {
            Id               = "Ice_Barrier_r1",
            Name             = "Ice Barrier",
            IconUrl          = "https://wow.zamimg.com/images/wow/icons/large/spell_ice_lament.jpg",
            Cooldown         = 60,
            ManaCost         = 0,
            BuffDuration     = 60,
            BuffShieldPct    = 0.30,
            LevelRequired    = 1,
            TalentUnlockOnly = true,
        },
    };
}
