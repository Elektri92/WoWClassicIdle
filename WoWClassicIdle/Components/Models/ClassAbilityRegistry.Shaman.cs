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
            Damage = 14,        // +4 Buff gesamt
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
            Damage = 15,        // +4 Buff gesamt
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
            ExclusiveGroup = "shaman_weapon_enchant",   // nur eine Waffenverzauberung gleichzeitig
            LevelRequired = 2,
            Rank = 1,
        },
        new Ability
        {
            Id = "Rockbiter_Weapon_r2",
            Name = "Rockbiter Weapon",
            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/spell_nature_rockbiter.jpg",
            Cooldown = 0,
            ManaCost = 13,                // ×1,25 gegenüber Rank 1
            BuffDuration = 60,
            BuffWeaponDamageBonus = 0.10, // ×1,3 gegenüber Rank 1
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
            ManaCost = 6,                       // ×1,25 gegenüber Rank 1
            DebuffDuration = 10,
            DebuffAttackSpeedReduction = 0.26,  // ×1,3 gegenüber Rank 1
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
            BuffDuration = 30,              // Buff läuft 30 s
            BuffRetaliationDamage = 4,      // 4 DMG wenn der Spieler getroffen wird (war 5 – Shaman Lv 7–10 ~12 % zu schnell)
            LevelRequired = 7,
            Rank = 1,
        },
        new Ability
        {
            Id = "Lightning_Shield_r2",
            Name = "Lightning Shield",
            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/spell_nature_lightningshield.jpg",
            Cooldown = 0,
            ManaCost = 13,                  // ×1,25 gegenüber Rank 1
            BuffDuration = 30,
            BuffRetaliationDamage = 6,      // ×1,5 gegenüber Rank 1
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
            DebuffDuration = 15,                // Feuer-DoT über 15 s
            DebuffDotDamage = 8,                // 5 Ticks à 8 = 40 DMG gesamt
            DebuffDotTickInterval = 3,
            LevelRequired = 14,
            Rank = 1,
        },
        new Ability
        {
            Id = "Healing_Wave_r1",
            Name = "Healing Wave",
            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/spell_nature_magicimmunity.jpg",
            Cooldown = 0,       // kein Cooldown – Tempo kommt aus Cast-Zeit
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
            BuffExtraAttackChance = 0.20,       // 20 % Chance pro Auto-Attack …
            BuffExtraAttacks = 2,               // … auf 2 Zusatzschläge (3 statt 1)
            ExclusiveGroup = "shaman_weapon_enchant",   // ersetzt Rockbiter Weapon
            TalentUnlockOnly = true,
            SpecHint = 1,
            LevelRequired = 1,
            Rank = 1,
        },

        // ── Talent-Fähigkeiten (Reihe 2) ──
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
            BuffDuration = 12,                  // MoT über 12 s
            MotManaRestorePct = 0.24,           // 4 Ticks à 6 % maximales Mana
            MotTickInterval = 3.0,
            CastWhenManaBelowPct = 0.5,         // erst unter 50 % Mana
            TalentUnlockOnly = true,
            SpecHint = 2,
            LevelRequired = 1,
            Rank = 1,
        },

        // ── Spezialisierungs-Fähigkeiten (Talent-Reihe 3, ab Lv 20) ──
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
            SynergyCritBonus = 1.0,             // garantierter Crit, wenn Flame Shock auf dem Ziel liegt
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
            HitCount = 2,                       // zwei Waffenschläge
            DebuffDuration = 10,
            DebuffDamageTakenBonus = 0.05,      // Ziel erleidet 10 s lang +5 % Schaden (10 % ergab +13,7 % XP/h)
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
            BuffHealOnHit = 12,                 // heilt bei jedem erlittenen Treffer
            TalentUnlockOnly = true,
            SpecHint = 2,
            LevelRequired = 20,
            Rank = 1,
        },
    };
}
