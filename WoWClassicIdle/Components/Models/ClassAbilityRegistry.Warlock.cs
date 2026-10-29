namespace Models;

public static partial class ClassAbilityRegistry
{
    private static IReadOnlyList<Ability> WarlockAbilities => new[]
    {
        new Ability
        {
            Id = "Shdow_bolt_r1",
            Name = "Shadow Bolt",
            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/spell_shadow_shadowbolt.jpg",
            Cooldown = 0,
            CastTime = 2.5,     // 2,5 s Cast-Zeit
            ManaCost = 20,      // was 25 – Warlock Lv 1–2 too often OOM
            Damage = 12,        // −2 Nerf
            LevelRequired = 1,
            Rank = 1,
        },
        new Ability
        {
            Id = "Shadow_Bolt_r2",
            Name = "Shadow Bolt",
            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/spell_shadow_shadowbolt.jpg",
            Cooldown = 0,
            CastTime = 2.5,     // 2,5 s Cast-Zeit
            ManaCost = 25,
            Damage = 14,        // −2 Nerf
            LevelRequired = 10,
            Rank = 2,
        },
        new Ability
        {
            Id = "Curse_of_Agony_r1",
            Name = "Curse of Agony",
            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/spell_shadow_curseofsargeras.jpg",
            Cooldown = 0,
            ManaCost = 10,
            DebuffDuration = 15,
            DebuffDotDamage = 5,        // −25 % total nerf (was 8, then 6)
            DebuffDotTickInterval = 3,
            LevelRequired = 3,
            Rank = 1,
        },
        new Ability
        {
            Id = "Curse_of_Agony_r2",
            Name = "Curse of Agony",
            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/spell_shadow_curseofsargeras.jpg",
            Cooldown = 0,
            ManaCost = 13,              // ×1.25 compared to Rank 1
            DebuffDuration = 15,
            DebuffDotDamage = 8,        // ×1.5 compared to Rank 1
            DebuffDotTickInterval = 3,
            LevelRequired = 13,
            Rank = 2,
        },
        new Ability
        {
            Id = "Drain_Life_r1",
            Name = "Drain Life",
            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/spell_shadow_lifedrain02.jpg",
            Cooldown = 30,              // CD 60 → 30 s
            CastTime = 2.0,             // 2,0 s Cast-Zeit
            ManaCost = 0,
            DebuffDuration = 12,        // Kanal 12 s
            DebuffDotDamage = 7,        // 4 ticks of 7 = 28 damage (was 5 – Warlock died too often)
            DebuffDotTickInterval = 3,
            DebuffDotLifesteal = 1.0,   // 100 % of the damage heals the player
            LevelRequired = 5,
            Rank = 1,
        },
        new Ability
        {
            Id = "Drain_Life_r2",
            Name = "Drain Life",
            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/spell_shadow_lifedrain02.jpg",
            Cooldown = 30,              // CD 60 → 30 s
            CastTime = 2.0,             // 2,0 s Cast-Zeit
            ManaCost = 0,
            DebuffDuration = 12,
            DebuffDotDamage = 10,       // double damage compared to Rank 1
            DebuffDotTickInterval = 3,
            DebuffDotLifesteal = 1.0,
            LevelRequired = 11,
            Rank = 2,
        },
        new Ability
        {
            Id = "Life_Tap_r1",
            Name = "Life Tap",
            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/spell_shadow_burningspirit.jpg",
            Cooldown = 30,
            ManaCost = 0,
            LifeTapHpCost = 10,         // costs 10 HP
            LifeTapManaGain = 15,        // grants 15 mana
            LevelRequired = 7,
            Rank = 1,
        },
        new Ability
        {
            Id = "Life_Tap_r2",
            Name = "Life Tap",
            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/spell_shadow_burningspirit.jpg",
            Cooldown = 30,
            ManaCost = 0,
            LifeTapHpCost = 15,         // ×1.5 compared to Rank 1
            LifeTapManaGain = 23,       // ×1.5 compared to Rank 1
            LevelRequired = 17,
            Rank = 2,
        },
        new Ability
        {
            Id = "Corruption_r1",
            Name = "Corruption",
            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/spell_shadow_abominationexplosion.jpg",
            Cooldown = 0,
            ManaCost = 0,
            DebuffDuration = 20,                // Shadow DoT over 20 s
            DebuffDotDamage = 6,                // 5 ticks of 6 = 30 damage total
            DebuffDotTickInterval = 4,
            LevelRequired = 14,
            Rank = 1,
        },
        new Ability
        {
            Id = "Curse_of_Weakness_r1",
            Name = "Curse of Weakness",
            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/spell_shadow_curseofmannoroth.jpg",
            Cooldown = 30,
            ManaCost = 10,
            DebuffDuration = 20,
            DebuffAttackSpeedReduction = 0.15,  // −15 % Angriffstempo
            LevelRequired = 18,
            Rank = 1,
        },
        new Ability
        {
            Id = "Immolate_r1",
            Name = "Immolate",
            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/spell_fire_immolation.jpg",
            Cooldown = 15,
            CastTime = 2.0,
            ManaCost = 20,
            Damage = 12,                        // Sofortschaden
            DebuffDuration = 12,                // Fire DoT over 12 s
            DebuffDotDamage = 10,               // 4 ticks of 10 = 40 damage total
            DebuffDotTickInterval = 3,
            LevelRequired = 24,
            Rank = 1,
        },

        // ── Talent abilities (row 2) ──────────────────────────────────────
        new Ability
        {
            Id                    = "Siphon_Life_r1",
            Name                  = "Siphon Life",
            IconUrl               = "https://wow.zamimg.com/images/wow/icons/large/spell_shadow_requiem.jpg",
            Cooldown              = 0,
            ManaCost              = 20,
            DebuffDuration        = 15,
            DebuffDotDamage       = 5,          // 5 ticks of 5 = 25 damage total (was 6 – Affliction Lv 15 at +16 %)
            DebuffDotTickInterval = 3,
            DebuffDotLifesteal    = 1.0,        // 100 % of the damage heals the player
            LevelRequired         = 1,
            Rank                  = 1,
            TalentUnlockOnly      = true,
        },
        new Ability
        {
            Id               = "Summon_Voidwalker_r1",
            Name             = "Summon Voidwalker",
            IconUrl          = "https://wow.zamimg.com/images/wow/icons/large/spell_shadow_summonvoidwalker.jpg",
            CastTime         = 3.0,             // Summon or revive
            ManaCost         = 30,
            SummonsPet       = PetKind.Voidwalker,
            LevelRequired    = 1,
            Rank             = 1,
            TalentUnlockOnly = true,
        },
        new Ability
        {
            Id               = "Incinerate_r1",
            Name             = "Incinerate",
            IconUrl          = "https://wow.zamimg.com/images/wow/icons/large/spell_fire_burnout.jpg",
            Cooldown         = 10,
            CastTime         = 2.5,
            ManaCost         = 30,
            Damage           = 24,              // considerably more than Shadow Bolt, but with a cooldown
            LevelRequired    = 1,
            Rank             = 1,
            TalentUnlockOnly = true,
        },

        // ── Specialization abilities (talent row 3, from Lv 20) ───────────
        new Ability
        {
            Id                      = "Soul_Link",
            Name                    = "Soul Link",
            IconUrl                 = "https://wow.zamimg.com/images/wow/icons/large/spell_shadow_gathershadows.jpg",
            ManaCost                = 0,
            IsPassive               = true,
            BuffDuration            = 9999,     // Aura: works while equipped and the demon is alive
            RequiresActivePet       = true,
            BuffPetDamageShareBonus = 0.20,     // Demon additionally absorbs 20 % of the damage
            BuffDamageBonus         = 0.05,
            SpecHint                = 1,
            LevelRequired           = 20,
            Rank                    = 1,
            TalentUnlockOnly        = true,
        },
        new Ability
        {
            Id                 = "Conflagrate_r1",
            Name               = "Conflagrate",
            IconUrl            = "https://wow.zamimg.com/images/wow/icons/large/spell_fire_fireball.jpg",
            Cooldown           = 10,
            ManaCost           = 25,
            Damage             = 22,            // sofort
            SynergyDebuffName  = "Immolate",
            SynergyDamageBonus = 0.50,          // +50 % against burning enemies
            SpecHint           = 2,
            LevelRequired      = 20,
            Rank               = 1,
            TalentUnlockOnly   = true,
        },
    };
}
