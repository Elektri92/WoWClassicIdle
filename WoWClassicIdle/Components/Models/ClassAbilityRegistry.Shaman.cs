namespace Models;

public static partial class ClassAbilityRegistry
{
    private static IReadOnlyList<Ability> ShamanAbilities => new[]
    {
        new Ability
        {
            Id = "Lightning_bolt_r1",
            Name = "Lightning Bolt",
            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/spell_nature_lightning.jpg",
            Cooldown = 0,
            CastTime = 2.5,     // 2,5 s Cast-Zeit
            ManaCost = 15,
            Damage = 14,        // +4 buff total
            LevelRequired = 1,
            Rank = 1,
        },
        new Ability
        {
            Id = "Lightning_Bolt_r2",
            Name = "Lightning Bolt",
            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/spell_nature_lightning.jpg",
            Cooldown = 0,
            CastTime = 2.5,     // 2,5 s Cast-Zeit
            ManaCost = 15,
            Damage = 15,        // +4 buff total
            LevelRequired = 10,
            Rank = 2,
        },
        new Ability
        {
            Id = "Rockbiter_Weapon_r1",
            Name = "Rockbiter Weapon",
            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/spell_nature_rockbiter.jpg",
            Cooldown = 0,
            ManaCost = 10,
            BuffDuration = 60,
            BuffWeaponDamageBonus = 0.08, // +8% auto-attack damage
            ExclusiveGroup = "shaman_weapon_enchant",   // only one weapon enchant at a time
            LevelRequired = 2,
            Rank = 1,
        },
        new Ability
        {
            Id = "Rockbiter_Weapon_r2",
            Name = "Rockbiter Weapon",
            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/spell_nature_rockbiter.jpg",
            Cooldown = 0,
            ManaCost = 13,                // ×1.25 compared to Rank 1
            BuffDuration = 60,
            BuffWeaponDamageBonus = 0.10, // ×1.3 compared to Rank 1
            ExclusiveGroup = "shaman_weapon_enchant",
            LevelRequired = 12,
            Rank = 2,
        },
        new Ability
        {
            Id = "Earthbind_Totem_r1",
            Name = "Earthbind Totem",
            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/spell_nature_earthbindtotem.jpg",
            Cooldown = 30,
            ManaCost = 5,
            DebuffDuration = 10,
            DebuffAttackSpeedReduction = 0.20,  // −20 % Angriffstempo
            LevelRequired = 4,
            Rank = 1,
        },
        new Ability
        {
            Id = "Earthbind_Totem_r2",
            Name = "Earthbind Totem",
            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/spell_nature_earthbindtotem.jpg",
            Cooldown = 30,
            ManaCost = 6,                       // ×1.25 compared to Rank 1
            DebuffDuration = 10,
            DebuffAttackSpeedReduction = 0.26,  // ×1.3 compared to Rank 1
            LevelRequired = 14,
            Rank = 2,
        },
        new Ability
        {
            Id = "Lightning_Shield_r1",
            Name = "Lightning Shield",
            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/spell_nature_lightningshield.jpg",
            Cooldown = 0,
            ManaCost = 10,
            BuffDuration = 30,              // Buff lasts 30 s
            BuffRetaliationDamage = 4,      // 4 damage when the player is hit (was 5 – Shaman Lv 7–10 ~12 % too fast)
            LevelRequired = 7,
            Rank = 1,
        },
        new Ability
        {
            Id = "Lightning_Shield_r2",
            Name = "Lightning Shield",
            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/spell_nature_lightningshield.jpg",
            Cooldown = 0,
            ManaCost = 13,                  // ×1.25 compared to Rank 1
            BuffDuration = 30,
            BuffRetaliationDamage = 6,      // ×1.5 compared to Rank 1
            LevelRequired = 17,
            Rank = 2,
        },
        new Ability
        {
            Id = "Flame_Shock_r1",
            Name = "Flame Shock",
            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/spell_fire_flameshock.jpg",
            Cooldown = 20,
            ManaCost = 20,
            Damage = 10,                        // Sofortschaden
            DebuffDuration = 15,                // Fire DoT over 15 s
            DebuffDotDamage = 8,                // 5 ticks of 8 = 40 damage total
            DebuffDotTickInterval = 3,
            LevelRequired = 14,
            Rank = 1,
        },
        new Ability
        {
            Id = "Healing_Wave_r1",
            Name = "Healing Wave",
            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/spell_nature_magicimmunity.jpg",
            Cooldown = 0,       // no cooldown – pacing comes from cast time
            CastTime = 3.0,     // 3,0 s Cast-Zeit
            ManaCost = 30,
            Heal = 30,
            LevelRequired = 13,
            Rank = 1,
        },
        new Ability
        {
            Id = "Windfury_Weapon_r1",
            Name = "Windfury Weapon",
            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/spell_nature_windfury.jpg",
            Cooldown = 0,
            ManaCost = 10,
            BuffDuration = 60,
            BuffExtraAttackChance = 0.20,       // 20 % chance per auto-attack …
            BuffExtraAttacks = 2,               // … for 2 extra swings (3 instead of 1)
            ExclusiveGroup = "shaman_weapon_enchant",   // replaces Rockbiter Weapon
            TalentUnlockOnly = true,
            SpecHint = 1,
            LevelRequired = 1,
            Rank = 1,
        },

        // ── Talent abilities (row 2) ──
        new Ability
        {
            Id = "Chain_Lightning_r1",
            Name = "Chain Lightning",
            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/spell_nature_chainlightning.jpg",
            Cooldown = 8,
            CastTime = 2.5,
            ManaCost = 30,
            Damage = 8,                         // 3 Treffer à 8 = 24 DMG (Lightning Bolt R2: 15)
            HitCount = 3,
            BonusCritChance = 0.20,             // +20 % Crit-Chance
            TalentUnlockOnly = true,
            SpecHint = 0,
            LevelRequired = 1,
            Rank = 1,
        },
        new Ability
        {
            Id = "Mana_Tide_Totem_r1",
            Name = "Mana Tide Totem",
            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/spell_frost_summonwaterelemental.jpg",
            Cooldown = 60,
            ManaCost = 0,
            BuffDuration = 12,                  // MoT over 12 s
            MotManaRestorePct = 0.24,           // 4 ticks of 6 % max mana
            MotTickInterval = 3.0,
            CastWhenManaBelowPct = 0.5,         // only below 50 % mana
            TalentUnlockOnly = true,
            SpecHint = 2,
            LevelRequired = 1,
            Rank = 1,
        },

        // ── Specialization abilities (talent row 3, from Lv 20) ──
        new Ability
        {
            Id = "Lava_Burst_r1",
            Name = "Lava Burst",
            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/spell_shaman_lavaburst.jpg",
            Cooldown = 8,
            CastTime = 2.0,
            ManaCost = 25,
            Damage = 26,
            SynergyDebuffName = "Flame Shock",
            SynergyCritBonus = 1.0,             // guaranteed crit if Flame Shock is on the target
            TalentUnlockOnly = true,
            SpecHint = 0,
            LevelRequired = 20,
            Rank = 1,
        },
        new Ability
        {
            Id = "Stormstrike_r1",
            Name = "Stormstrike",
            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/ability_shaman_stormstrike.jpg",
            Cooldown = 10,
            ManaCost = 20,
            Damage = 3,
            AddsWeaponDamage = true,
            HitCount = 2,                       // two weapon strikes
            DebuffDuration = 10,
            DebuffDamageTakenBonus = 0.05,      // target takes +5 % damage for 10 s (10 % gave +13.7 % XP/h)
            TalentUnlockOnly = true,
            SpecHint = 1,
            LevelRequired = 20,
            Rank = 1,
        },
        new Ability
        {
            Id = "Earth_Shield_r1",
            Name = "Earth Shield",
            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/spell_nature_skinofearth.jpg",
            Cooldown = 0,
            ManaCost = 25,
            BuffDuration = 60,
            BuffHitCharges = 6,                 // 6 Ladungen
            BuffHealOnHit = 12,                 // heals on every hit taken
            TalentUnlockOnly = true,
            SpecHint = 2,
            LevelRequired = 20,
            Rank = 1,
        },
    };
}
