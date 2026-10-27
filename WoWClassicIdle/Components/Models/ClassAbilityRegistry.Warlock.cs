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
            ManaCost = 20,      // war 25 – Warlock Lv 1–2 zu oft OOM
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
            DebuffDotDamage = 5,        // −25 % gesamt Nerf (war 8, dann 6)
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
            ManaCost = 13,              // ×1,25 gegenüber Rank 1
            DebuffDuration = 15,
            DebuffDotDamage = 8,        // ×1,5 gegenüber Rank 1
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
            DebuffDotDamage = 7,        // 4 Ticks à 7 = 28 DMG (war 5 – Warlock starb zu oft)
            DebuffDotTickInterval = 3,
            DebuffDotLifesteal = 1.0,   // 100 % des Schadens heilt den Spieler
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
            DebuffDotDamage = 10,       // doppelter Schaden gegenüber Rank 1
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
            LifeTapHpCost = 10,         // kostet 10 HP
            LifeTapManaGain = 15,        // gibt 15 Mana
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
            LifeTapHpCost = 15,         // ×1,5 gegenüber Rank 1
            LifeTapManaGain = 23,       // ×1,5 gegenüber Rank 1
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
            DebuffDuration = 20,                // Schatten-DoT über 20 s
            DebuffDotDamage = 6,                // 5 Ticks à 6 = 30 DMG gesamt
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
            DebuffDuration = 12,                // Feuer-DoT über 12 s
            DebuffDotDamage = 10,               // 4 Ticks à 10 = 40 DMG gesamt
            DebuffDotTickInterval = 3,
            LevelRequired = 24,
            Rank = 1,
        },

        // ── Talent-Fähigkeiten (Reihe 2) ──────────────────────────────────────
        new Ability
        {
            Id                    = "Siphon_Life_r1",
            Name                  = "Siphon Life",
            IconUrl               = "https://wow.zamimg.com/images/wow/icons/large/spell_shadow_requiem.jpg",
            Cooldown              = 0,
            ManaCost              = 20,
            DebuffDuration        = 15,
            DebuffDotDamage       = 5,          // 5 Ticks à 5 = 25 DMG gesamt (war 6 – Affliction Lv 15 bei +16 %)
            DebuffDotTickInterval = 3,
            DebuffDotLifesteal    = 1.0,        // 100 % des Schadens heilt den Spieler
            LevelRequired         = 1,
            Rank                  = 1,
            TalentUnlockOnly      = true,
        },
        new Ability
        {
            Id               = "Summon_Voidwalker_r1",
            Name             = "Summon Voidwalker",
            IconUrl          = "https://wow.zamimg.com/images/wow/icons/large/spell_shadow_summonvoidwalker.jpg",
            CastTime         = 3.0,             // Beschwören bzw. Wiederbeleben
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
            Damage           = 24,              // deutlich mehr als Shadow Bolt, dafür Cooldown
            LevelRequired    = 1,
            Rank             = 1,
            TalentUnlockOnly = true,
        },

        // ── Spezialisierungs-Fähigkeiten (Talent-Reihe 3, ab Lv 20) ───────────
        new Ability
        {
            Id                      = "Soul_Link",
            Name                    = "Soul Link",
            IconUrl                 = "https://wow.zamimg.com/images/wow/icons/large/spell_shadow_gathershadows.jpg",
            ManaCost                = 0,
            IsPassive               = true,
            BuffDuration            = 9999,     // Aura: wirkt solange ausgerüstet und der Dämon lebt
            RequiresActivePet       = true,
            BuffPetDamageShareBonus = 0.20,     // Dämon fängt zusätzlich 20 % des Schadens ab
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
            SynergyDamageBonus = 0.50,          // +50 % gegen brennende Gegner
            SpecHint           = 2,
            LevelRequired      = 20,
            Rank               = 1,
            TalentUnlockOnly   = true,
        },
    };
}
