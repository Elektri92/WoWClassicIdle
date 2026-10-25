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
            ManaCost = 6,           // ×1,25 gegenüber Rank 1
            Damage = 9,             // ×1,5 gegenüber Rank 1
            DebuffDuration = 20,
            DebuffDotDamage = 9,    // ×1,5 gegenüber Rank 1
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
            ManaCost = 14,              // war 9 – effizientester Heal im Spiel
            FormCosts = new Dictionary<DruidForm, int> { [DruidForm.Bear] = 10, [DruidForm.Cat] = 25 },
            BuffDuration = 10,          // HoT über 10 s
            HotTotalHeal = 20,          // 5 Ticks à 4 HP = 20 HP
            HotTickInterval = 2,
            LevelRequired = 3,          // war 5 – Druiden starben auf Lv 1–4 am häufigsten (Sim: 19–31 Tode/h)
            Rank = 1,
        },
        new Ability
        {
            Id = "Rejuvenation_r2",
            Name = "Rejuvenation",
            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/spell_nature_rejuvenation.jpg",
            Cooldown = 15,
            ManaCost = 18,              // ×1,25 gegenüber Rank 1
            FormCosts = new Dictionary<DruidForm, int> { [DruidForm.Bear] = 12, [DruidForm.Cat] = 30 },
            BuffDuration = 10,
            HotTotalHeal = 30,          // ×1,5 gegenüber Rank 1 (5 Ticks à 6 HP)
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
            BuffDuration = 30,              // Buff läuft 30 s
            BuffRetaliationDamage = 5,      // 5 DMG wenn der Spieler getroffen wird
            LevelRequired = 7,
            Rank = 1,
        },
        new Ability
        {
            Id = "Thorns_r2",
            Name = "Thorns",
            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/spell_nature_thorns.jpg",
            Cooldown = 0,
            ManaCost = 13,                  // ×1,25 gegenüber Rank 1
            FormCosts = new Dictionary<DruidForm, int> { [DruidForm.Bear] = 12 },
            BuffDuration = 30,
            BuffRetaliationDamage = 6,      // war 8 – Druiden (Balance/Bear) lagen Lv 17–20 bei +10–19 % XP/h
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
            Heal = 12,                          // direkter Sofort-Heal
            BuffDuration = 12,                  // HoT über 12 s
            HotTotalHeal = 20,                  // 6 Ticks à ~3 = 20 HP gesamt
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
            DebuffDotDamage = 6,                // 5 Ticks à 6 = 30 DMG gesamt
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
            LevelRequired = 24,
            Rank = 1,
        },

        // ── Gestalten (Talent, Reihe 2) – nur eine gleichzeitig, Effekte in ShapeshiftForms ──
        new Ability
        {
            Id = "Moonkin_Form",
            Name = "Moonkin Form",
            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/spell_nature_forceofnature.jpg",
            Form = DruidForm.Moonkin,
            IsPassive = true,               // kein Cast – wirkt solange ausgerüstet
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
            SpecHint = 1,                   // Simulation: Feral-DPS bevorzugt Cat statt Bear
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
            Cooldown = 0,                   // wie Heroic Strike – ersetzt die nächste Auto-Attack
            ManaCost = 15,                  // 15 Rage
            MinResourceToCast = 60,         // wie Heroic Strike: erst ab 60 Rage, damit Rage für Thorns/Rejuvenation bleibt
            Damage = 12,                    // war 18 – Bear lag Lv 16–20 bei +13 bis +26 % XP/h
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
            Cooldown = 1,                   // wie Sinister Strike
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
            Cooldown = 0,                   // wird erst nach Ablauf des Debuffs neu gewirkt
            ManaCost = 30,                  // 30 Energy
            DebuffDuration = 12,            // Blutungs-DoT über 12 s (wie Rend)
            DebuffDotDamage = 7,            // 6 Ticks à 7 = 42 DMG gesamt
            DebuffDotTickInterval = 2,
            RequiredForm = DruidForm.Cat,
            TalentUnlockOnly = true,
            LevelRequired = 1,
            Rank = 1,
        },
    };
}
