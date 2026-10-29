namespace Models;

public static partial class ClassAbilityRegistry
{
    private static IReadOnlyList<Ability> DruidAbilities => new[]
    {
        new Ability
        {
            Id = "Wrath_r1",
            Name = "Wrath",
            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/spell_nature_abolishmagic.jpg",
            Cooldown = 0,
            CastTime = 2.5,     // 2,5 s Cast-Zeit
            ManaCost = 20,
            Damage = 14,
            LevelRequired = 1,
            Rank = 1,
        },
        new Ability
        {
            Id = "Wrath_r2",
            Name = "Wrath",
            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/spell_nature_abolishmagic.jpg",
            Cooldown = 0,
            CastTime = 2.5,     // 2,5 s Cast-Zeit
            ManaCost = 20,
            Damage = 15,
            LevelRequired = 10,
            Rank = 2,
        },
        new Ability
        {
            Id = "Moonfire_r1",
            Name = "Moonfire",
            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/spell_nature_starfall.jpg",
            Cooldown = 20,
            ManaCost = 5,
            Damage = 6,             // +20 % Buff
            DebuffDuration = 20,
            DebuffDotDamage = 6,    // +20 % Buff
            DebuffDotTickInterval = 5,
            LevelRequired = 3,
            Rank = 1,
        },
        new Ability
        {
            Id = "Moonfire_r2",
            Name = "Moonfire",
            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/spell_nature_starfall.jpg",
            Cooldown = 20,
            ManaCost = 6,           // ×1.25 compared to Rank 1
            Damage = 9,             // ×1.5 compared to Rank 1
            DebuffDuration = 20,
            DebuffDotDamage = 9,    // ×1.5 compared to Rank 1
            DebuffDotTickInterval = 5,
            LevelRequired = 13,
            Rank = 2,
        },
        new Ability
        {
            Id = "Rejuvenation_r1",
            Name = "Rejuvenation",
            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/spell_nature_rejuvenation.jpg",
            Cooldown = 15,              // war 10
            ManaCost = 14,              // was 9 – most efficient heal in the game
            FormCosts = new Dictionary<DruidForm, int> { [DruidForm.Bear] = 10, [DruidForm.Cat] = 25 },
            BuffDuration = 10,          // HoT over 10 s
            HotTotalHeal = 20,          // 5 Ticks à 4 HP = 20 HP
            HotTickInterval = 2,
            LevelRequired = 3,          // was 5 – Druids died most often at Lv 1–4 (sim: 19–31 deaths/h)
            Rank = 1,
        },
        new Ability
        {
            Id = "Rejuvenation_r2",
            Name = "Rejuvenation",
            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/spell_nature_rejuvenation.jpg",
            Cooldown = 15,
            ManaCost = 18,              // ×1.25 compared to Rank 1
            FormCosts = new Dictionary<DruidForm, int> { [DruidForm.Bear] = 12, [DruidForm.Cat] = 30 },
            BuffDuration = 10,
            HotTotalHeal = 30,          // ×1.5 compared to Rank 1 (5 ticks of 6 HP)
            HotTickInterval = 2,
            LevelRequired = 15,
            Rank = 2,
        },
        new Ability
        {
            Id = "Thorns_r1",
            Name = "Thorns",
            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/spell_nature_thorns.jpg",
            Cooldown = 0,
            ManaCost = 10,
            FormCosts = new Dictionary<DruidForm, int> { [DruidForm.Bear] = 10 },   // in Bear Form: 10 Rage
            BuffDuration = 30,              // Buff lasts 30 s
            BuffRetaliationDamage = 5,      // 5 damage when the player is hit
            LevelRequired = 7,
            Rank = 1,
        },
        new Ability
        {
            Id = "Thorns_r2",
            Name = "Thorns",
            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/spell_nature_thorns.jpg",
            Cooldown = 0,
            ManaCost = 13,                  // ×1.25 compared to Rank 1
            FormCosts = new Dictionary<DruidForm, int> { [DruidForm.Bear] = 12 },
            BuffDuration = 30,
            BuffRetaliationDamage = 6,      // was 8 – Druids (Balance/Bear) were at +10–19 % XP/h at Lv 17–20
            LevelRequired = 17,
            Rank = 2,
        },
        new Ability
        {
            Id = "Regrowth_r1",
            Name = "Regrowth",
            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/spell_nature_resistnature.jpg",
            Cooldown = 12,
            CastTime = 2.5,     // 2,5 s Cast-Zeit
            ManaCost = 20,
            Heal = 12,                          // direct instant heal
            BuffDuration = 12,                  // HoT over 12 s
            HotTotalHeal = 20,                  // 6 ticks of ~3 = 20 HP total
            HotTickInterval = 2,
            LevelRequired = 14,
            Rank = 1,
        },
        new Ability
        {
            Id = "Insect_Swarm_r1",
            Name = "Insect Swarm",
            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/spell_nature_insectswarm.jpg",
            Cooldown = 20,
            ManaCost = 10,
            DebuffDuration = 15,                // Naturdot + Tempo-Debuff
            DebuffDotDamage = 6,                // 5 ticks of 6 = 30 damage total
            DebuffDotTickInterval = 3,
            DebuffAttackSpeedReduction = 0.10,  // −10 % Angriffstempo
            LevelRequired = 18,
            Rank = 1,
        },
        new Ability
        {
            Id = "Starfire_r1",
            Name = "Starfire",
            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/spell_arcane_starfire.jpg",
            Cooldown = 0,
            CastTime = 3.0,     // 3,0 s Cast-Zeit
            ManaCost = 30,
            Damage = 28,
            LevelRequired = 20,     // was 24 – Classic rank, prerequisite for Eclipse
            Rank = 1,
        },

        // ── Forms (talent, row 2) – only one at a time, effects in ShapeshiftForms ──
        new Ability
        {
            Id = "Moonkin_Form",
            Name = "Moonkin Form",
            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/spell_nature_forceofnature.jpg",
            Form = DruidForm.Moonkin,
            IsPassive = true,               // no cast – works while equipped
            ExclusiveGroup = "druid_form",
            TalentUnlockOnly = true,
            SpecHint = 0,
            LevelRequired = 1,
            Rank = 1,
        },
        new Ability
        {
            Id = "Bear_Form",
            Name = "Bear Form",
            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/ability_racial_bearform.jpg",
            Form = DruidForm.Bear,
            IsPassive = true,
            ExclusiveGroup = "druid_form",
            TalentUnlockOnly = true,
            LevelRequired = 1,
            Rank = 1,
        },
        new Ability
        {
            Id = "Cat_Form",
            Name = "Cat Form",
            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/ability_druid_catform.jpg",
            Form = DruidForm.Cat,
            IsPassive = true,
            ExclusiveGroup = "druid_form",
            TalentUnlockOnly = true,
            SpecHint = 1,                   // Simulation: Feral DPS prefers Cat over Bear
            LevelRequired = 1,
            Rank = 1,
        },
        new Ability
        {
            Id = "Tree_Form",
            Name = "Tree Form",
            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/ability_druid_treeoflife.jpg",
            Form = DruidForm.Tree,
            IsPassive = true,
            ExclusiveGroup = "druid_form",
            TalentUnlockOnly = true,
            SpecHint = 2,
            LevelRequired = 1,
            Rank = 1,
        },
        new Ability
        {
            Id = "Maul_r1",
            Name = "Maul",
            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/ability_druid_maul.jpg",
            Cooldown = 0,                   // like Heroic Strike – replaces the next auto-attack
            ManaCost = 15,                  // 15 Rage
            MinResourceToCast = 60,         // like Heroic Strike: only from 60 rage, so rage remains for Thorns/Rejuvenation
            Damage = 12,                    // was 18 – Bear was at +13 to +26 % XP/h at Lv 16–20
            AddsWeaponDamage = true,
            ReplacesAutoAttack = true,
            RequiredForm = DruidForm.Bear,
            TalentUnlockOnly = true,
            LevelRequired = 1,
            Rank = 1,
        },
        new Ability
        {
            Id = "Claw_r1",
            Name = "Claw",
            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/ability_druid_rake.jpg",
            Cooldown = 1,                   // like Sinister Strike
            ManaCost = 40,                  // 40 Energy
            Damage = 5,
            AddsWeaponDamage = true,
            RequiredForm = DruidForm.Cat,
            TalentUnlockOnly = true,
            LevelRequired = 1,
            Rank = 1,
        },
        new Ability
        {
            Id = "Rip_r1",
            Name = "Rip",
            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/ability_ghoulfrenzy.jpg",
            Cooldown = 0,                   // only recast after the debuff expires
            ManaCost = 30,                  // 30 Energy
            DebuffDuration = 12,            // Bleed DoT over 12 s (like Rend)
            DebuffDotDamage = 7,            // 6 ticks of 7 = 42 damage total
            DebuffDotTickInterval = 2,
            RequiredForm = DruidForm.Cat,
            TalentUnlockOnly = true,
            LevelRequired = 1,
            Rank = 1,
        },

        // ── Specialization abilities (talent row 3, from Lv 20) ──
        new Ability
        {
            Id = "Faerie_Fire_Cat",
            Name = "Faerie Fire (Cat)",
            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/spell_nature_faeriefire.jpg",
            ManaCost = 0,                   // in Classic kostenlos in Gestalt
            DebuffDuration = 30,
            DebuffDamageTakenBonus = 0.08,  // Armor lowered: +8 % damage taken
            RequiredForm = DruidForm.Cat,
            TalentUnlockOnly = true,
            SpecHint = 1,
            LevelRequired = 20,
            Rank = 1,
        },
        new Ability
        {
            Id = "Faerie_Fire_Bear",
            Name = "Faerie Fire (Bear)",
            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/spell_nature_faeriefire.jpg",
            Cooldown = 10,
            ManaCost = 0,
            Damage = 4,                     // Bear: hits additionally and slows the enemy (10 / CD 6 gave +12.5 % XP/h)
            DebuffDuration = 30,
            DebuffDamageTakenBonus = 0.05,
            DebuffAttackSpeedReduction = 0.10,
            RequiredForm = DruidForm.Bear,
            TalentUnlockOnly = true,
            LevelRequired = 20,
            Rank = 1,
        },
        new Ability
        {
            Id = "Tranquility_r1",
            Name = "Tranquility",
            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/spell_nature_tranquility.jpg",
            Cooldown = 30,
            CastTime = 4.0,                 // Kanalisierung
            ManaCost = 45,
            Damage = 10,                    // 4 waves of 10 damage – just above Starfire DPS, otherwise never cast
            HitCount = 4,
            Heal = 40,                      // … and heals you at the same time
            TalentUnlockOnly = true,
            SpecHint = 2,
            LevelRequired = 20,
            Rank = 1,
        },
    };
}
