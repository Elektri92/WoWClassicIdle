namespace Models;

public static partial class ClassAbilityRegistry
{
    private static IReadOnlyList<Ability> PaladinAbilities => new[]
    {
        new Ability
        {
            Id = "Judgement_r1",
            Name = "Judgement",
            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/spell_holy_righteousfury.jpg",
            Cooldown = 8,
            ManaCost = 20,
            Damage = 15,        // 22 → 17 → 15 nerf (Lv 1–2 still +29 % DPS above median)
            LevelRequired = 1,
            Rank = 1,
        },
        new Ability
        {
            Id = "Judgement_r2",
            Name = "Judgement",
            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/spell_holy_righteousfury.jpg",
            Cooldown = 8,
            ManaCost = 20,
            Damage = 19,        // 24 → 19 nerf (gap to Rank 1 stays)
            LevelRequired = 10,
            Rank = 2,
        },
        new Ability
        {
            Id = "Holy_Light_r1",
            Name = "Holy Light",
            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/spell_holy_holybolt.jpg",
            Cooldown = 15,      // was 0 – constant spamming made the Paladin immortal
            CastTime = 3.0,     // 3,0 s Cast-Zeit
            ManaCost = 30,              // war 18
            Heal = 24,                  // was 19 – stronger per cast, but less frequent
            LevelRequired = 3,
            Rank = 1,
        },
        new Ability
        {
            Id = "Holy_Light_r2",
            Name = "Holy Light",
            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/spell_holy_holybolt.jpg",
            Cooldown = 15,
            CastTime = 3.0,
            ManaCost = 38,              // ×1.25 compared to Rank 1
            Heal = 36,                  // ×1.5 compared to Rank 1
            LevelRequired = 13,
            Rank = 2,
        },
        new Ability
        {
            Id = "Holy_Strike_r1",
            Name = "Holy Strike",
            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/spell_holy_excorcism_02.jpg",
            Cooldown = 10,
            ManaCost = 15,
            Damage = 20,                // Waffenschaden + 20 (+4 Buff)
            AddsWeaponDamage = true,
            ReplacesAutoAttack = true,
            LevelRequired = 4,
            Rank = 1,
        },
        new Ability
        {
            Id = "Holy_Strike_r2",
            Name = "Holy Strike",
            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/spell_holy_excorcism_02.jpg",
            Cooldown = 10,
            ManaCost = 19,              // ×1.25 compared to Rank 1
            Damage = 30,                // Weapon damage + 30 (×1.5 compared to Rank 1)
            AddsWeaponDamage = true,
            ReplacesAutoAttack = true,
            LevelRequired = 14,
            Rank = 2,
        },
        new Ability
        {
            Id = "Divine_Shield_r1",
            Name = "Divine Shield",
            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/spell_holy_divineshield.jpg",
            Cooldown = 60,
            ManaCost = 0,
            BuffDuration = 6,           // 6 s immunity
            BuffGrantsImmunity = true,
            LevelRequired = 7,
            Rank = 1,
        },
        new Ability
        {
            Id = "Divine_Shield_r2",
            Name = "Divine Shield",
            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/spell_holy_divineshield.jpg",
            Cooldown = 60,
            ManaCost = 0,
            BuffDuration = 8,           // +2 s immunity compared to Rank 1
            BuffGrantsImmunity = true,
            LevelRequired = 17,
            Rank = 2,
        },
        new Ability
        {
            Id = "Consecration_r1",
            Name = "Consecration",
            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/spell_holy_innerfire.jpg",
            Cooldown = 20,
            ManaCost = 20,
            DebuffDuration = 12,                // Holy fire over 12 s
            DebuffDotDamage = 10,               // 4 ticks of 10 = 40 damage total
            DebuffDotTickInterval = 3,
            LevelRequired = 14,
            Rank = 1,
        },
        new Ability
        {
            Id = "Seal_of_Righteousness_r1",
            Name = "Seal of Righteousness",
            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/ability_thunderbolt.jpg",
            Cooldown = 0,
            ManaCost = 15,
            BuffDuration = 30,
            BuffWeaponDamageBonus = 0.06,       // +6 % Waffenschaden
            ExclusiveGroup = "paladin_seal",    // only one Seal at a time
            LevelRequired = 18,
            Rank = 1,
        },
        new Ability
        {
            Id = "Hammer_of_Justice_r1",
            Name = "Hammer of Justice",
            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/spell_holy_sealofmight.jpg",
            Cooldown = 40,
            ManaCost = 20,
            Damage = 15,
            DebuffDuration = 4,                 // Stun 4 s
            DebuffIsStun = true,
            LevelRequired = 24,
            Rank = 1,
        },

        // ── Talent abilities (row 2) ──────────────────────────────────────
        new Ability
        {
            Id               = "Holy_Shock_r1",
            Name             = "Holy Shock",
            IconUrl          = "https://wow.zamimg.com/images/wow/icons/large/spell_holy_searinglight.jpg",
            Cooldown         = 15,
            ManaCost         = 25,
            Damage           = 16,              // Damage + healing at the same time (like Holy Nova)
            Heal             = 16,
            LevelRequired    = 1,
            Rank             = 1,
            TalentUnlockOnly = true,
        },
        new Ability
        {
            Id                       = "Holy_Shield_r1",
            Name                     = "Holy Shield",
            IconUrl                  = "https://wow.zamimg.com/images/wow/icons/large/spell_holy_blessingofprotection.jpg",
            Cooldown                 = 20,
            ManaCost                 = 20,
            BuffDuration             = 10,
            BuffHitCharges           = 4,       // ends after 4 hits taken (was 3)
            BuffDamageTakenReduction = 0.35,    // −35 % damage from these hits (was 30 %)
            BuffRetaliationDamage    = 16,      // Holy damage to the attacker (was 8 – Prot was at −14 % XP/h)
            LevelRequired            = 1,
            Rank                     = 1,
            TalentUnlockOnly         = true,
        },
        new Ability
        {
            Id                    = "Seal_of_Command_r1",
            Name                  = "Seal of Command",
            IconUrl               = "https://wow.zamimg.com/images/wow/icons/large/ability_warrior_innerrage.jpg",
            Cooldown              = 0,
            ManaCost              = 15,
            BuffDuration          = 30,
            BuffWeaponDamageBonus = 0.12,       // +12 % auto-attack damage (was 5 % – sim: 0 % compared to Seal of Righteousness)
            BuffExtraAttackChance = 0.25,       // 25 % chance for an extra swing (was 15 %)
            BuffExtraAttacks      = 1,
            ExclusiveGroup        = "paladin_seal",
            LevelRequired         = 1,
            Rank                  = 1,
            TalentUnlockOnly      = true,
        },

        // ── Specialization abilities (talent row 3, from Lv 20): passive auras, only one at a time ──
        new Ability
        {
            Id                     = "Holy_Aura",
            Name                   = "Holy Aura",
            IconUrl                = "https://wow.zamimg.com/images/wow/icons/large/spell_holy_mindsooth.jpg",
            ManaCost               = 0,
            IsPassive              = true,
            BuffDuration           = 9999,      // Aura: works while equipped
            BuffHealingBonus       = 0.20,
            BuffResourceRegenBonus = 0.25,      // +25 % Manaregeneration
            BuffDamageBonus        = 0.05,      // never equipped in the sim without a damage component
            ExclusiveGroup         = "paladin_aura",
            SpecHint               = 0,
            LevelRequired          = 20,
            Rank                   = 1,
            TalentUnlockOnly       = true,
        },
        new Ability
        {
            Id                       = "Protection_Aura",
            Name                     = "Protection Aura",
            IconUrl                  = "https://wow.zamimg.com/images/wow/icons/large/spell_holy_devotionaura.jpg",
            ManaCost                 = 0,
            IsPassive                = true,
            BuffDuration             = 9999,
            BuffDamageTakenReduction = 0.10,
            ExclusiveGroup           = "paladin_aura",
            SpecHint                 = 1,
            LevelRequired            = 20,
            Rank                     = 1,
            TalentUnlockOnly         = true,
        },
        new Ability
        {
            Id                    = "Retribution_Aura",
            Name                  = "Retribution Aura",
            IconUrl               = "https://wow.zamimg.com/images/wow/icons/large/spell_holy_auraoflight.jpg",
            ManaCost              = 0,
            IsPassive             = true,
            BuffDuration          = 9999,
            BuffRetaliationDamage = 7,          // Heiliger Schaden an jedem Angreifer
            ExclusiveGroup        = "paladin_aura",
            SpecHint              = 2,
            LevelRequired         = 20,
            Rank                  = 1,
            TalentUnlockOnly      = true,
        },
    };
}
