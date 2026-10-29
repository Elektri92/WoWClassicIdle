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
            CastTime = 2.5,     // 2.5 s cast time instead of cooldown
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
            CastTime = 2.5,     // 2.5 s cast time instead of cooldown
            ManaCost = 20,
            Damage = 16,        // +2 compared to Rank 1
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
            CastTime = 2.5,     // 2.5 s cast time instead of cooldown
            ManaCost = 18,
            Damage = 13,        // 1 less than Fireball – utility from the slow compensates
            DebuffDuration = 5,
            DebuffAttackSpeedReduction = 0.05,  // −5 % Angriffstempo
            SynergyDebuffName = "Frost Nova",   // Shatter: +50 % crit against frozen enemies
            SynergyCritBonus = 0.50,
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
            CastTime = 2.5,     // 2.5 s cast time instead of cooldown
            ManaCost = 20,
            Damage = 15,        // +2 compared to Rank 1, slow remains
            DebuffDuration = 5,
            DebuffAttackSpeedReduction = 0.05,
            SynergyDebuffName = "Frost Nova",
            SynergyCritBonus = 0.50,
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
            BuffIntellectBonus = 45,    // ×1.5 compared to Rank 1
            BuffSpiritBonus = 45,       // ×1.5 compared to Rank 1
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
            BuffDuration = 20,              // regenerates over 20 s
            MotManaRestorePct = 0.14,       // 12 % max mana
            MotTickInterval = 5.0,          // 4 Ticks à 2,5 %
            LevelRequired = 3,
            Rank = 1,
        },
        new Ability
        {
            Id = "Conjure_Water_r2",
            Name = "Conjure Water",
            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/inv_drink_10.jpg",
            Cooldown = 50,                  // 10 s less CD compared to Rank 1
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
            BuffDuration = 20,              // HoT over 20 s
            HotHealPct = 0.25,              // 25 % max HP in total (was 35 % – Mage barely died anymore from Lv 4)
            HotTickInterval = 5.0,          // 4 Ticks à 6,25 %
            LevelRequired = 4,              // was 8 – the Lv 4 outlier only occurred with HP regen factor 1.0
            Rank = 1,
        },
        new Ability
        {
            Id = "Fire_Blast_r1",
            Name = "Fire Blast",
            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/spell_fire_fireball.jpg",
            Cooldown = 8,
            ManaCost = 18,
            Damage = 22,                        // Instant damage without cast time
            LevelRequired = 14,
            Rank = 1,
        },
        new Ability
        {
            Id = "Arcane_Missiles_r1",
            Name = "Arcane Missiles",
            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/spell_nature_starfall.jpg",
            Cooldown = 0,
            CastTime = 2.5,     // 2.5 s cast time; no cooldown
            ManaCost = 20,
            Damage = 15,        // Slightly more than Fireball/Frostbolt, no utility effect
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
            CastTime = 2.5,     // 2.5 s cast time; no cooldown
            ManaCost = 22,
            Damage = 19,        // +4 compared to Rank 1; highest damage of the bolt group without slow
            LevelRequired = 10,
            Rank = 2,
            ExclusiveGroup = "mage_bolt",
            SpecHint = 0,       // Arcane-Baum
        },
        new Ability
        {
            Id = "Conjure_Food_r2",
            Name = "Conjure Food",
            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/inv_misc_food_30.jpg",
            Cooldown = 30,                  // 10 s less CD compared to Rank 1
            CastTime = 1.0,
            ManaCost = 10,
            BuffDuration = 20,
            HotHealPct = 0.35,
            HotTickInterval = 5.0,
            LevelRequired = 11,
            Rank = 2,
        },

        // ─── Talent-exclusive abilities (only unlockable via the talent system) ───
        new Ability
        {
            Id                   = "Evocation_r1",
            Name                 = "Evocation",
            IconUrl              = "https://wow.zamimg.com/images/wow/icons/large/spell_nature_purge.jpg",
            Cooldown             = 45,          // was 60 + only below 20 % mana – never used in the sim (0 % XP/h)
            ManaCost             = 0,
            BuffDuration         = 8,
            MotManaRestorePct    = 0.80,
            MotTickInterval      = 2.0,
            BuffDamageBonus      = 0.20,        // +20 % damage during Evocation
            LevelRequired        = 1,
            TalentUnlockOnly     = true,
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

        // ─── Specialization abilities (talent row 3, from Lv 20) ───
        new Ability
        {
            Id                  = "Arcane_Blast_r1",
            Name                = "Arcane Blast",
            IconUrl             = "https://wow.zamimg.com/images/wow/icons/large/spell_arcane_blast.jpg",
            Cooldown            = 8,
            CastTime            = 2.0,
            ManaCost            = 30,
            Damage              = 30,
            EmpowersAbilityName = "Arcane Missiles",   // Arcane rotation: Blast → empowered Missiles
            EmpowerDamageBonus  = 0.30,
            EmpowerDuration     = 10,
            SpecHint            = 0,
            LevelRequired       = 20,
            TalentUnlockOnly    = true,
        },
        new Ability
        {
            Id                    = "Flamestrike_r1",
            Name                  = "Flamestrike",
            IconUrl               = "https://wow.zamimg.com/images/wow/icons/large/spell_fire_selfdestruct.jpg",
            Cooldown              = 15,
            CastTime              = 3.0,
            ManaCost              = 35,
            Damage                = 24,
            DebuffDuration        = 8,             // burning ground; crits trigger Ignite
            DebuffDotDamage       = 5,
            DebuffDotTickInterval = 2,
            SpecHint              = 1,
            LevelRequired         = 20,
            TalentUnlockOnly      = true,
        },
        new Ability
        {
            Id               = "Frost_Nova_r1",
            Name             = "Frost Nova",
            IconUrl          = "https://wow.zamimg.com/images/wow/icons/large/spell_frost_frostnova.jpg",
            Cooldown         = 25,
            ManaCost         = 20,
            Damage           = 10,
            DebuffDuration   = 5,                  // frozen: no attacks, Frostbolt with Shatter crit
            DebuffIsStun     = true,
            SpecHint         = 2,
            LevelRequired    = 20,                 // was regular Lv 24 – now a Frost talent
            TalentUnlockOnly = true,
        },
    };
}
