namespace Models;

public static partial class ClassAbilityRegistry
{
    private static IReadOnlyList<Ability> RogueAbilities => new[]
    {
        new Ability
        {
            Id = "Sinester_Strike_r1",
            Name = "Sinister Strike",
            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/spell_shadow_ritualofsacrifice.jpg",
            Cooldown = 1,
            ManaCost = 45,      // 45 Energy
            Damage = 3,
            AddsWeaponDamage = true,
            LevelRequired = 1,
            Rank = 1,
        },
        new Ability
        {
            Id = "Sinister_Strike_r2",
            Name = "Sinister Strike",
            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/spell_shadow_ritualofsacrifice.jpg",
            Cooldown = 1,
            ManaCost = 45,
            Damage = 4,         // +10 % compared to Rank 1 (3.3 → 4)
            AddsWeaponDamage = true,
            LevelRequired = 10,
            Rank = 2,
        },
        new Ability
        {
            Id = "Slice_and_Dice_r1",
            Name = "Slice and Dice",
            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/ability_rogue_slicedice.jpg",
            Cooldown = 0,
            ManaCost = 5,               // 5 Energy
            BuffDuration = 10,          // 10 s
            BuffAttackSpeedBonus = 0.1, // +10% attack speed
            LevelRequired = 3,
            Rank = 1,
        },
        new Ability
        {
            Id = "Slice_and_Dice_r2",
            Name = "Slice and Dice",
            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/ability_rogue_slicedice.jpg",
            Cooldown = 0,
            ManaCost = 5,
            BuffDuration = 10,
            BuffAttackSpeedBonus = 0.13, // ×1.3 compared to Rank 1
            LevelRequired = 13,
            Rank = 2,
        },
        new Ability
        {
            Id = "Deadly_Poison_r1",
            Name = "Deadly Poison",
            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/ability_poisons.jpg",
            Cooldown = 0,
            ManaCost = 0,
            IsPassive = true,           // no active cast – procs on every attack
            ProcChance = 0.30,          // 30 % proc chance per swing (x3 buff)
            DebuffDuration = 15,        // Debuff duration per stack application
            DebuffDotDamage = 1,        // 1 damage / 3 s per stack
            DebuffDotTickInterval = 3,
            DebuffMaxStacks = 5,        // up to 5 stacks
            LevelRequired = 5,
            Rank = 1,
        },
        new Ability
        {
            Id = "Deadly_Poison_r2",
            Name = "Deadly Poison",
            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/ability_poisons.jpg",
            Cooldown = 0,
            ManaCost = 0,
            IsPassive = true,
            ProcChance = 0.30,
            DebuffDuration = 15,
            DebuffDotDamage = 1,        // int – scaling via stacks instead
            DebuffDotTickInterval = 3,
            DebuffMaxStacks = 7,        // ×1.4 max damage compared to Rank 1
            LevelRequired = 15,
            Rank = 2,
        },
        new Ability
        {
            Id = "Kidney_Shot_r1",
            Name = "Kidney Shot",
            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/ability_rogue_kidneyshot.jpg",
            Cooldown = 30,
            ManaCost = 0,
            DebuffDuration = 6,         // Stun for 6 s
            DebuffIsStun = true,
            LevelRequired = 7,
            Rank = 1,
        },
        new Ability
        {
            Id = "Kidney_Shot_r2",
            Name = "Kidney Shot",
            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/ability_rogue_kidneyshot.jpg",
            Cooldown = 30,
            ManaCost = 0,
            DebuffDuration = 7,         // +1 s stun compared to Rank 1
            DebuffIsStun = true,
            LevelRequired = 17,
            Rank = 2,
        },
        new Ability
        {
            Id = "Backstab_r1",
            Name = "Backstab",
            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/ability_backstab.jpg",
            Cooldown = 6,
            ManaCost = 40,                      // 40 Energy
            Damage = 25,
            AddsWeaponDamage = true,
            LevelRequired = 14,
            Rank = 1,
        },
        new Ability
        {
            Id = "Expose_Armor_r1",
            Name = "Expose Armor",
            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/ability_warrior_riposte.jpg",
            Cooldown = 20,
            ManaCost = 25,                      // 25 Energy
            DebuffDuration = 15,
            DebuffDamageTakenBonus = 0.10,      // +10 % damage taken by the target
            LevelRequired = 18,
            Rank = 1,
        },
        new Ability
        {
            Id = "Evasion_r1",
            Name = "Evasion",
            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/spell_shadow_shadowward.jpg",
            Cooldown = 60,
            ManaCost = 0,
            BuffDuration = 8,
            BuffDamageTakenReduction = 0.20,    // −20 % damage taken
            LevelRequired = 24,
            Rank = 1,
        },

        // ── Talent abilities (row 2) ──────────────────────────────────────
        new Ability
        {
            Id                 = "Cold_Blood_r1",
            Name               = "Cold Blood",
            IconUrl            = "https://wow.zamimg.com/images/wow/icons/large/spell_ice_lament.jpg",
            Cooldown           = 30,
            ManaCost           = 0,
            BuffDuration       = 20,
            BuffGuaranteesCrit = true,          // next damaging ability is a guaranteed crit
            LevelRequired      = 1,
            Rank               = 1,
            TalentUnlockOnly   = true,
        },
        new Ability
        {
            Id                    = "Blade_Flurry_r1",
            Name                  = "Blade Flurry",
            IconUrl               = "https://wow.zamimg.com/images/wow/icons/large/ability_warrior_punishingblow.jpg",
            Cooldown              = 45,
            ManaCost              = 25,         // 25 Energy
            BuffDuration          = 10,
            BuffExtraAttackChance = 1.0,        // every auto-attack hits twice
            BuffExtraAttacks      = 1,
            LevelRequired         = 1,
            Rank                  = 1,
            TalentUnlockOnly      = true,
        },
        new Ability
        {
            Id               = "Ghostly_Strike_r1",
            Name             = "Ghostly Strike",
            IconUrl          = "https://wow.zamimg.com/images/wow/icons/large/spell_shadow_curse.jpg",
            Cooldown         = 20,
            ManaCost         = 40,              // 40 Energy
            Damage           = 8,
            AddsWeaponDamage = true,
            BuffDuration     = 15,
            BuffShieldPct    = 0.10,            // Shield of 10 % max HP
            LevelRequired    = 1,
            Rank             = 1,
            TalentUnlockOnly = true,
        },

        // ── Specialization abilities (talent row 3, from Lv 20) ───────────
        new Ability
        {
            Id                 = "Mutilate_r1",
            Name               = "Mutilate",
            IconUrl            = "https://wow.zamimg.com/images/wow/icons/large/ability_rogue_shadowstrikes.jpg",
            Cooldown           = 6,
            ManaCost           = 50,            // 50 Energy
            Damage             = 4,
            AddsWeaponDamage   = true,
            HitCount           = 2,             // hits with both blades
            SynergyDebuffName  = "Deadly Poison",
            SynergyDamageBonus = 0.25,          // +25 % against poisoned enemies
            SpecHint           = 0,
            LevelRequired      = 20,
            Rank               = 1,
            TalentUnlockOnly   = true,
        },
        new Ability
        {
            Id                     = "Adrenaline_Rush_r1",
            Name                   = "Adrenaline Rush",
            IconUrl                = "https://wow.zamimg.com/images/wow/icons/large/spell_shadow_shadowworddominate.jpg",
            Cooldown               = 60,
            ManaCost               = 0,
            BuffDuration           = 15,
            BuffResourceRegenBonus = 1.0,       // double energy regeneration
            SpecHint               = 1,
            LevelRequired          = 20,
            Rank                   = 1,
            TalentUnlockOnly       = true,
        },
        new Ability
        {
            Id                 = "Vanish_r1",
            Name               = "Vanish",
            IconUrl            = "https://wow.zamimg.com/images/wow/icons/large/ability_vanish.jpg",
            Cooldown           = 45,
            ManaCost           = 0,
            BuffDuration       = 4,
            BuffGrantsImmunity = true,          // briefly no damage taken
            BuffFreeCasts      = 1,             // next ability costs no energy
            SpecHint           = 2,
            LevelRequired      = 20,
            Rank               = 1,
            TalentUnlockOnly   = true,
        },
    };
}
