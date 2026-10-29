namespace Models;

public static partial class ClassAbilityRegistry
{
    private static IReadOnlyList<Ability> PriestAbilities => new[]
    {
        new Ability
        {
            Id = "Smite_r1",
            Name = "Smite",
            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/spell_holy_holysmite.jpg",
            Cooldown = 0,
            CastTime = 2.5,     // 2,5 s Cast-Zeit
            ManaCost = 18,
            Damage = 13,
            LevelRequired = 1,
            Rank = 1,
        },
        new Ability
        {
            Id = "Smite_r2",
            Name = "Smite",
            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/spell_holy_holysmite.jpg",
            Cooldown = 0,
            CastTime = 2.5,     // 2,5 s Cast-Zeit
            ManaCost = 18,
            Damage = 14,
            LevelRequired = 10,
            Rank = 2,
        },
        new Ability
        {
            Id = "Renew_r1",
            Name = "Renew",
            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/spell_holy_renew.jpg",
            Cooldown = 60,
            ManaCost = 15,      // −50 % Manakosten
            BuffDuration = 20,      // HoT over 20 s
            HotTotalHeal = 40,      // 40 healing total
            HotTickInterval = 4,    // 5 Ticks à 8
            LevelRequired = 3,
            Rank = 1,
        },
        new Ability
        {
            Id = "Renew_r2",
            Name = "Renew",
            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/spell_holy_renew.jpg",
            Cooldown = 60,
            ManaCost = 19,          // ×1.25 compared to Rank 1
            BuffDuration = 20,
            HotTotalHeal = 60,      // ×1.5 compared to Rank 1 (5 ticks of 12)
            HotTickInterval = 4,
            LevelRequired = 13,
            Rank = 2,
        },
        new Ability
        {
            Id = "Shadow_Word_Pain_r1",
            Name = "Shadow Word: Pain",
            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/spell_shadow_shadowwordpain.jpg",
            Cooldown = 60,
            ManaCost = 20,
            DebuffDuration = 30,        // DoT over 30 s
            DebuffDotDamage = 12,       // doubled (was 6), 6 ticks of 12 = 72 damage
            DebuffDotTickInterval = 5,
            LevelRequired = 5,
            Rank = 1,
        },
        new Ability
        {
            Id = "Shadow_Word_Pain_r2",
            Name = "Shadow Word: Pain",
            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/spell_shadow_shadowwordpain.jpg",
            Cooldown = 60,
            ManaCost = 25,              // ×1.25 compared to Rank 1
            DebuffDuration = 30,
            DebuffDotDamage = 18,       // ×1.5 compared to Rank 1 (6 ticks of 18 = 108 damage)
            DebuffDotTickInterval = 5,
            LevelRequired = 15,
            Rank = 2,
        },
        new Ability
        {
            Id = "Drain_Mana_r1",
            Name = "Drain Mana",
            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/spell_shadow_siphonmana.jpg",
            Cooldown = 30,
            CastTime = 1.0,
            ManaCost = 0,
            BuffDuration = 20,              // MoT over 20 s
            MotManaRestorePct = 0.08,       // 8 % max mana
            MotTickInterval = 5.0,          // 4 Ticks à 2 %
            LevelRequired = 7,
            Rank = 1,
        },
        new Ability
        {
            Id = "Drain_Mana_r2",
            Name = "Drain Mana",
            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/spell_shadow_siphonmana.jpg",
            Cooldown = 30,
            CastTime = 1.0,
            ManaCost = 0,
            BuffDuration = 20,
            MotManaRestorePct = 0.10,       // ×1.3 compared to Rank 1
            MotTickInterval = 5.0,
            LevelRequired = 17,
            Rank = 2,
        },
        new Ability
        {
            Id = "Power_Word_Fortitude_r1",
            Name = "Power Word: Fortitude",
            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/spell_holy_wordfortitude.jpg",
            Cooldown = 0,
            ManaCost = 0,
            BuffDuration = 60,
            BuffDamageTakenReduction = 0.15,    // −15 % damage taken
            LevelRequired = 14,
            Rank = 1,
        },
        new Ability
        {
            Id = "Mind_Blast_r1",
            Name = "Mind Blast",
            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/spell_shadow_unholyfrenzy.jpg",
            Cooldown = 8,
            CastTime = 1.5,     // 1,5 s Cast-Zeit
            ManaCost = 25,
            Damage = 26,
            LevelRequired = 18,
            Rank = 1,
        },
        new Ability
        {
            Id = "Flash_Heal_r1",
            Name = "Flash Heal",
            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/spell_holy_flashheal.jpg",
            Cooldown = 12,
            CastTime = 2.0,     // 2,0 s Cast-Zeit
            ManaCost = 30,
            Heal = 45,
            LevelRequired = 24,
            Rank = 1,
        },

        // ── Talent abilities (row 2) ──
        new Ability
        {
            Id = "Priest_Divine_Shield_r1",     // own prefix – "Divine_Shield" is the Paladin spell
            Name = "Divine Shield",
            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/spell_holy_powerwordshield.jpg",
            Cooldown = 30,
            ManaCost = 20,
            BuffDuration = 30,
            BuffShieldPct = 0.25,               // absorbs 25 % of max HP
            TalentUnlockOnly = true,
            SpecHint = 0,
            LevelRequired = 1,
            Rank = 1,
        },
        new Ability
        {
            Id = "Holy_Nova_r1",
            Name = "Holy Nova",
            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/spell_holy_holynova.jpg",
            Cooldown = 10,
            ManaCost = 25,
            Damage = 12,                        // Damage + healing at the same time, no HP gate
            Heal = 12,
            TalentUnlockOnly = true,
            SpecHint = 1,
            LevelRequired = 1,
            Rank = 1,
        },
        new Ability
        {
            Id = "Shadow_Form",
            Name = "Shadowform",
            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/spell_shadow_shadowform.jpg",
            Form = DruidForm.Shadow,
            IsPassive = true,                   // no cast – works while equipped
            ExclusiveGroup = "priest_form",
            TalentUnlockOnly = true,
            SpecHint = 2,
            LevelRequired = 1,
            Rank = 1,
        },

        // ─── Specialization abilities (talent row 3, from Lv 20) ───
        new Ability
        {
            Id = "Penance_r1",
            Name = "Penance",
            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/spell_holy_penance.jpg",
            Cooldown = 10,
            CastTime = 2.0,                     // Channeled: 3 holy bolts
            ManaCost = 30,
            Damage = 10,
            HitCount = 3,
            Heal = 20,                          // heals on the side – Discipline: damage + protection
            TalentUnlockOnly = true,
            SpecHint = 0,
            LevelRequired = 20,
            Rank = 1,
        },
        new Ability
        {
            Id = "Mind_Flay_r1",
            Name = "Mind Flay",
            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/spell_shadow_siphonmana.jpg",
            Cooldown = 0,
            CastTime = 3.0,                     // Channeled, filler instead of Smite
            ManaCost = 25,
            Damage = 27,
            DebuffDuration = 3,
            DebuffAttackSpeedReduction = 0.20,  // −20 % attack speed during the Flay
            RequiredForm = DruidForm.Shadow,
            TalentUnlockOnly = true,
            SpecHint = 2,
            LevelRequired = 20,
            Rank = 1,
        },
    };
}
