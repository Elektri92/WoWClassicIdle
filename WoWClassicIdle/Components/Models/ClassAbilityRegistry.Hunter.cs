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
            // Normaler Sofortschlag (wie Sinister Strike), unabhängig von der Auto-Attack
            Cooldown = 6,
            ManaCost = 15,
            Damage = 6,                 // war 4 – Hunter Lv 1–3 zu schwach
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
            Damage = 7,                 // war 5 – bleibt über Rank 1
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
            DebuffDamageTakenBonus = 0.07, // ×1,3 gegenüber Rank 1
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
            DebuffDuration = 10,        // DoT über 10 s
            DebuffDotDamage = 10,       // 2 Ticks à 10 = 20 DMG (−10 % Nerf, war 11)
            DebuffDotTickInterval = 5,
            LevelRequired = 3,          // war 4 – Kill-Loch auf Lv 3
            Rank = 1,
        },
        new Ability
        {
            Id = "Serpent_Sting_r2",
            Name = "Serpent Sting",
            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/ability_poisonsting.jpg",
            Cooldown = 20,
            ManaCost = 19,              // ×1,25 gegenüber Rank 1
            DebuffDuration = 10,
            DebuffDotDamage = 15,       // ×1,5 gegenüber Rank 1 (2 Ticks à 15 = 30 DMG)
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
            ManaCost = 0,       // kostenlos (−100 % Mana)
            Damage = 9,                 // war 10 – Hunter Lv 9–10 Kills zu hoch
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
            Damage = 14,                // ×1,5 gegenüber Rank 1
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
            BuffDamageTakenReduction = 0.15,    // −15 % erlittener Schaden
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

        // ── Talent-Fähigkeiten (Reihe 2) ──────────────────────────────────────
        new Ability
        {
            Id               = "Call_Pet_Cat_r1",
            Name             = "Call Cat",
            IconUrl          = "https://wow.zamimg.com/images/wow/icons/large/ability_hunter_pet_cat.jpg",
            CastTime         = 3.0,             // Beschwören bzw. Wiederbeleben
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
            Damage           = 70,              // war 45 / 3,5 s – wurde im Sim nie gewählt
            LevelRequired    = 1,
            Rank             = 1,
            TalentUnlockOnly = true,
        },
        new Ability
        {
            Id                  = "Mongoose_Bite_r1",
            Name                = "Mongoose Bite",
            IconUrl             = "https://wow.zamimg.com/images/wow/icons/large/ability_hunter_swiftstrike.jpg",
            Cooldown            = 10,
            ManaCost            = 15,
            Damage              = 8,
            AddsWeaponDamage    = true,
            EmpowersAbilityName = "Raptor Strike",
            EmpowerDamageBonus  = 0.30,         // nächster Raptor Strike +30 % Schaden (war 50 %)
            EmpowerDuration     = 10,
            LevelRequired       = 1,
            Rank                = 1,
            TalentUnlockOnly    = true,
        },

        // ── Spezialisierungs-Fähigkeiten (Talent-Reihe 3, ab Lv 20): passive Aspekte, nur einer gleichzeitig ──
        new Ability
        {
            Id                     = "Aspect_of_the_Owl",
            Name                   = "Aspect of the Owl",
            IconUrl                = "https://wow.zamimg.com/images/wow/icons/large/ability_eyeoftheowl.jpg",
            ManaCost               = 0,
            IsPassive              = true,
            BuffDuration           = 9999,      // Aspekt: wirkt solange ausgerüstet
            BuffPetDamageBonus     = 0.50,      // Beast Mastery: stärkeres Begleittier
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
            BuffDamageBonus     = 0.05,         // Marksmanship: mehr Schusschaden (8 % + 5 % Crit ergab +9,3 % XP/h)
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
            BuffDamageTakenReduction = 0.15,    // Survival: Ausweichen im Nahkampf (12 % / 5 % brachte nur +0,9 %)
            BuffAttackSpeedBonus     = 0.08,
            ExclusiveGroup           = "hunter_aspect",
            SpecHint                 = 2,
            LevelRequired            = 20,
            Rank                     = 1,
            TalentUnlockOnly         = true,
        },
    };
}
