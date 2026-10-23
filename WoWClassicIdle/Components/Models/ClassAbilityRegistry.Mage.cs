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
            SpecHint = 1,       // Fire-Baum
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
            SpecHint = 1,       // Fire-Baum
        },
        new Ability
        {
            Id = "Frostbolt_r1",
            Name = "Frostbolt",
            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/spell_frost_frostbolt02.jpg",
            Cooldown = 1.5,
            ManaCost = 18,
            Damage = 14,        // +15 % Buff
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
            Cooldown = 1.5,
            ManaCost = 18,
            Damage = 18,        // +3 erhöht
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
            Cooldown = 40,
            ManaCost = 10,
            BuffDuration = 20,              // HoT über 20 s
            HotHealPct = 0.30,              // 30 % max. HP gesamt
            HotTickInterval = 5.0,          // 4 Ticks à 6 %
            LevelRequired = 8,
            Rank = 1,
        },
        new Ability
        {
            Id = "Fire_Blast_r1",
            Name = "Fire Blast",
            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/spell_fire_fireblast.jpg",
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
            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/spell_magic_managain.jpg",
            Cooldown = 5.0,
            ManaCost = 25,
            DebuffDuration = 3.0,               // Kanal 3 s; IsDebuff blockiert Neucast bis Ablauf
            DebuffDotDamage = 6,                // 5 Ticks à 6 = 30 DMG (10 DPS, vergleichbar mit bolt-Gruppe)
            DebuffDotTickInterval = 0.6,        // 5 Treffer über 3 s
            LevelRequired = 10,
            Rank = 1,
            ExclusiveGroup = "mage_bolt",
            SpecHint = 0,                       // Arcane-Baum
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
            ManaCost = 10,
            BuffDuration = 20,
            HotHealPct = 0.30,
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
