namespace Models;

public static partial class ClassAbilityRegistry
{
    private static IReadOnlyList<Ability> HunterAbilities => new[]
    {
        new Ability
        {
            Id = "Raptor_Strike_r1",
            Name = "Raptor Strike",
            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/ability_meleedamage.jpg",
            // Normal instant strike (like Sinister Strike), independent of the auto-attack
            Cooldown = 6,
            ManaCost = 15,
            Damage = 6,                 // was 4 – Hunter too weak at Lv 1–3
            AddsWeaponDamage = true,
            LevelRequired = 1,
            Rank = 1,
        },
        new Ability
        {
            Id = "Raptor_Strike_r2",
            Name = "Raptor Strike",
            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/ability_meleedamage.jpg",
            Cooldown = 6,
            ManaCost = 15,
            Damage = 7,                 // was 5 – stays above Rank 1
            AddsWeaponDamage = true,
            LevelRequired = 10,
            Rank = 2,
        },
        new Ability
        {
            Id = "Hunters_Mark_r1",
            Name = "Hunter's Mark",
            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/ability_hunter_snipershot.jpg",
            Cooldown = 20,
            CastTime = 1.0,
            ManaCost = 0,
            DebuffDuration = 10,
            DebuffDamageTakenBonus = 0.05, // +5% damage taken
            LevelRequired = 3,
            Rank = 1,
        },
        new Ability
        {
            Id = "Hunters_Mark_r2",
            Name = "Hunter's Mark",
            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/ability_hunter_snipershot.jpg",
            Cooldown = 20,
            CastTime = 1.0,
            ManaCost = 0,
            DebuffDuration = 10,
            DebuffDamageTakenBonus = 0.07, // ×1.3 compared to Rank 1
            LevelRequired = 13,
            Rank = 2,
        },
        new Ability
        {
            Id = "Serpent_Sting_r1",
            Name = "Serpent Sting",
            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/ability_poisonsting.jpg",
            Cooldown = 20,
            ManaCost = 15,
            DebuffDuration = 10,        // DoT over 10 s
            DebuffDotDamage = 10,       // 2 ticks of 10 = 20 damage (−10 % nerf, was 11)
            DebuffDotTickInterval = 5,
            LevelRequired = 3,          // was 4 – kill gap at Lv 3
            Rank = 1,
        },
        new Ability
        {
            Id = "Serpent_Sting_r2",
            Name = "Serpent Sting",
            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/ability_poisonsting.jpg",
            Cooldown = 20,
            ManaCost = 19,              // ×1.25 compared to Rank 1
            DebuffDuration = 10,
            DebuffDotDamage = 15,       // ×1.5 compared to Rank 1 (2 ticks of 15 = 30 damage)
            DebuffDotTickInterval = 5,
            LevelRequired = 13,
            Rank = 2,
        },
        new Ability
        {
            Id = "Arcane_Shot_r1",
            Name = "Arcane Shot",
            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/ability_impalingbolt.jpg",
            Cooldown = 6,
            ManaCost = 0,       // free (−100 % mana)
            Damage = 9,                 // was 10 – Hunter kills too high at Lv 9–10
            LevelRequired = 7,
            Rank = 1,
        },
        new Ability
        {
            Id = "Arcane_Shot_r2",
            Name = "Arcane Shot",
            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/ability_impalingbolt.jpg",
            Cooldown = 6,
            ManaCost = 0,
            Damage = 14,                // ×1.5 compared to Rank 1
            LevelRequired = 17,
            Rank = 2,
        },
        new Ability
        {
            Id = "Multi_Shot_r1",
            Name = "Multi-Shot",
            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/ability_upgrademoonglaive.jpg",
            Cooldown = 10,
            CastTime = 1.5,
            ManaCost = 15,
            Damage = 24,
            LevelRequired = 14,
            Rank = 1,
        },
        new Ability
        {
            Id = "Disengage_r1",
            Name = "Disengage",
            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/ability_rogue_sprint.jpg",
            Cooldown = 30,
            ManaCost = 0,
            BuffDuration = 6,
            BuffDamageTakenReduction = 0.15,    // −15 % damage taken
            LevelRequired = 18,
            Rank = 1,
        },
        new Ability
        {
            Id = "Aimed_Shot_r1",
            Name = "Aimed Shot",
            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/ability_hunter_aimedshot.jpg",
            Cooldown = 12,
            CastTime = 3.0,
            ManaCost = 20,
            Damage = 38,
            LevelRequired = 24,
            Rank = 1,
        },

        // ── Talent abilities (row 2) ──────────────────────────────────────
        new Ability
        {
            Id               = "Call_Pet_Cat_r1",
            Name             = "Call Cat",
            IconUrl          = "https://wow.zamimg.com/images/wow/icons/large/ability_hunter_pet_cat.jpg",
            CastTime         = 3.0,             // Summon or revive
            ManaCost         = 15,
            SummonsPet       = PetKind.Cat,
            ExclusiveGroup   = "hunter_pet",
            LevelRequired    = 1,
            Rank             = 1,
            TalentUnlockOnly = true,
        },
        new Ability
        {
            Id               = "Call_Pet_Bear_r1",
            Name             = "Call Bear",
            IconUrl          = "https://wow.zamimg.com/images/wow/icons/large/ability_hunter_pet_bear.jpg",
            CastTime         = 3.0,
            ManaCost         = 15,
            SummonsPet       = PetKind.Bear,
            ExclusiveGroup   = "hunter_pet",
            LevelRequired    = 1,
            Rank             = 1,
            TalentUnlockOnly = true,
        },
        new Ability
        {
            Id               = "Sniper_Shot_r1",
            Name             = "Sniper Shot",
            IconUrl          = "https://wow.zamimg.com/images/wow/icons/large/ability_marksmanship.jpg",
            Cooldown         = 15,
            CastTime         = 3.0,
            ManaCost         = 25,
            Damage           = 70,              // was 45 / 3.5 s – never chosen in the sim
            LevelRequired    = 1,
            Rank             = 1,
            TalentUnlockOnly = true,
        },
        new Ability
        {
            Id                  = "Mongoose_Bite_r1",
            Name                = "Mongoose Bite",
            IconUrl             = "https://wow.zamimg.com/images/wow/icons/large/ability_hunter_swiftstrike.jpg",
            Cooldown            = 8,
            ManaCost            = 15,
            Damage              = 12,
            AddsWeaponDamage    = true,
            EmpowersAbilityName = "Raptor Strike",
            EmpowerDamageBonus  = 0.45,         // next Raptor Strike +45 % damage (was 30 %, Survival at -10 % XP/h)
            EmpowerDuration     = 10,
            LevelRequired       = 1,
            Rank                = 1,
            TalentUnlockOnly    = true,
        },

        // ── Specialization abilities (talent row 3, from Lv 20): passive aspects, only one at a time ──
        new Ability
        {
            Id                     = "Aspect_of_the_Owl",
            Name                   = "Aspect of the Owl",
            IconUrl                = "https://wow.zamimg.com/images/wow/icons/large/ability_eyeoftheowl.jpg",
            ManaCost               = 0,
            IsPassive              = true,
            BuffDuration           = 9999,      // Aspect: works while equipped
            BuffPetDamageBonus     = 0.50,      // Beast Mastery: stronger pet
            BuffResourceRegenBonus = 0.30,
            ExclusiveGroup         = "hunter_aspect",
            SpecHint               = 0,
            LevelRequired          = 20,
            Rank                   = 1,
            TalentUnlockOnly       = true,
        },
        new Ability
        {
            Id                  = "Aspect_of_the_Hawk",
            Name                = "Aspect of the Hawk",
            IconUrl             = "https://wow.zamimg.com/images/wow/icons/large/spell_nature_ravenform.jpg",
            ManaCost            = 0,
            IsPassive           = true,
            BuffDuration        = 9999,
            BuffDamageBonus     = 0.05,         // Marksmanship: more shot damage (8 % + 5 % crit gave +9.3 % XP/h)
            BuffCritChanceBonus = 0.03,
            ExclusiveGroup      = "hunter_aspect",
            SpecHint            = 1,
            LevelRequired       = 20,
            Rank                = 1,
            TalentUnlockOnly    = true,
        },
        new Ability
        {
            Id                       = "Aspect_of_the_Monkey",
            Name                     = "Aspect of the Monkey",
            IconUrl                  = "https://wow.zamimg.com/images/wow/icons/large/ability_hunter_aspectofthemonkey.jpg",
            ManaCost                 = 0,
            IsPassive                = true,
            BuffDuration             = 9999,
            BuffDamageTakenReduction = 0.15,    // Survival: melee dodge (12 % / 5 % only gave +0.9 %)
            BuffAttackSpeedBonus     = 0.08,
            ExclusiveGroup           = "hunter_aspect",
            SpecHint                 = 2,
            LevelRequired            = 20,
            Rank                     = 1,
            TalentUnlockOnly         = true,
        },
    };
}
