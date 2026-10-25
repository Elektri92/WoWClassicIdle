namespace Models;

public static partial class TalentRegistry
{
    private static List<TalentDefinition> BuildPriestTalents() =>
    [
        // ── Discipline-Baum (0), Reihe 0 ──────────────────────────────────────
        new TalentDefinition
        {
            Id            = "priest_disc_crit_mult",
            Name          = "Critical Multiplier",
            Description   = "Increases your critical strike chance by 1% per rank.",
            Class         = CharacterClass.Priest,
            TreeIndex     = 0,
            Row           = 0,
            Column        = 0,
            MaxRanks      = 5,
            EffectType    = TalentEffectType.CritChance,
            EffectPerRank = 0.01,
            IconUrl       = "https://wow.zamimg.com/images/wow/icons/medium/ability_rogue_slicedice.jpg",
        },
        new TalentDefinition
        {
            Id            = "priest_disc_healing_smite",
            Name          = "Healing Smite",
            Description   = "Heals you for 1 HP when Smite deals damage, per rank.",
            Class         = CharacterClass.Priest,
            TreeIndex     = 0,
            Row           = 0,
            Column        = 1,
            MaxRanks      = 2,
            EffectType    = TalentEffectType.SmiteHeal,
            EffectPerRank = 1.0,
            IconUrl       = "https://wow.zamimg.com/images/wow/icons/medium/spell_holy_holysmite.jpg",
        },
        new TalentDefinition
        {
            Id            = "priest_disc_wanding",
            Name          = "Inner Focus",
            Description   = "Increases the damage of all your abilities by 3% per rank.",
            Class         = CharacterClass.Priest,
            TreeIndex     = 0,
            Row           = 0,
            Column        = 2,
            MaxRanks      = 2,
            EffectType    = TalentEffectType.DamageBonus,
            EffectPerRank = 0.03,
            IconUrl       = "https://wow.zamimg.com/images/wow/icons/medium/inv_wand_01.jpg",
        },

        // ── Discipline-Baum (0), Reihe 1 ──────────────────────────────────────
        new TalentDefinition
        {
            Id            = "priest_disc_divine_shield",
            Name          = "Divine Shield",
            Description   = "Grants the ability Divine Shield: a shield that absorbs damage equal to 25% of your maximum health.",
            Class         = CharacterClass.Priest,
            TreeIndex     = 0,
            Row           = 1,
            Column        = 0,
            MaxRanks      = 1,
            EffectType    = TalentEffectType.GrantDivineShield,
            EffectPerRank = 1.0,
            GrantsAbilityId = "Priest_Divine_Shield_r1",
            IconUrl       = "https://wow.zamimg.com/images/wow/icons/medium/spell_holy_powerwordshield.jpg",
        },
        new TalentDefinition
        {
            Id            = "priest_disc_imp_smite",
            Name          = "Improved Smite",
            Description   = "Increases the damage of Smite by 20% per rank.",
            Class         = CharacterClass.Priest,
            TreeIndex     = 0,
            Row           = 1,
            Column        = 1,
            MaxRanks      = 2,
            EffectType    = TalentEffectType.SmiteDamage,
            EffectPerRank = 0.20,
            IconUrl       = "https://wow.zamimg.com/images/wow/icons/medium/spell_holy_holysmite.jpg",
        },
        new TalentDefinition
        {
            Id            = "priest_disc_imp_pwf",
            Name          = "Improved Power Word: Fortitude",
            Description   = "Increases the damage reduction of Power Word: Fortitude by 2% per rank.",
            Class         = CharacterClass.Priest,
            TreeIndex     = 0,
            Row           = 1,
            Column        = 2,
            MaxRanks      = 3,
            EffectType    = TalentEffectType.PowerWordFortitudeBonus,
            EffectPerRank = 0.02,
            IconUrl       = "https://wow.zamimg.com/images/wow/icons/medium/spell_holy_wordfortitude.jpg",
        },

        // ── Holy-Baum (1), Reihe 0 ────────────────────────────────────────────
        new TalentDefinition
        {
            Id            = "priest_holy_imp_healing",
            Name          = "Searing Light",
            Description   = "Increases the damage of Smite by 6% per rank.",
            Class         = CharacterClass.Priest,
            TreeIndex     = 1,
            Row           = 0,
            Column        = 0,
            MaxRanks      = 5,
            EffectType    = TalentEffectType.SmiteDamage,
            EffectPerRank = 0.06,
            IconUrl       = "https://wow.zamimg.com/images/wow/icons/medium/spell_holy_healingfocus.jpg",
        },
        new TalentDefinition
        {
            Id            = "priest_holy_holy_speed",
            Name          = "Holy Speed",
            Description   = "Increases auto-attack and casting speed by 2% per rank.",
            Class         = CharacterClass.Priest,
            TreeIndex     = 1,
            Row           = 0,
            Column        = 1,
            MaxRanks      = 3,
            EffectType    = TalentEffectType.AutoAttackSpeed,
            EffectPerRank = 0.02,
            IconUrl       = "https://wow.zamimg.com/images/wow/icons/medium/spell_holy_divinespirit.jpg",
        },

        // ── Holy-Baum (1), Reihe 1 ────────────────────────────────────────────
        new TalentDefinition
        {
            Id            = "priest_holy_holy_nova",
            Name          = "Holy Nova",
            Description   = "Grants the ability Holy Nova: deals Holy damage to the enemy and heals you at the same time.",
            Class         = CharacterClass.Priest,
            TreeIndex     = 1,
            Row           = 1,
            Column        = 0,
            MaxRanks      = 1,
            EffectType    = TalentEffectType.GrantHolyNova,
            EffectPerRank = 1.0,
            GrantsAbilityId = "Holy_Nova_r1",
            IconUrl       = "https://wow.zamimg.com/images/wow/icons/medium/spell_holy_holynova.jpg",
        },
        new TalentDefinition
        {
            Id            = "priest_holy_mindful_renew",
            Name          = "Mindful Renew",
            Description   = "Reduces the mana cost of Renew by 10% per rank.",
            Class         = CharacterClass.Priest,
            TreeIndex     = 1,
            Row           = 1,
            Column        = 1,
            MaxRanks      = 3,
            EffectType    = TalentEffectType.RenewManaCost,
            EffectPerRank = 0.10,
            IconUrl       = "https://wow.zamimg.com/images/wow/icons/medium/spell_holy_renew.jpg",
        },
        new TalentDefinition
        {
            Id            = "priest_holy_imp_renew",
            Name          = "Improved Renew",
            Description   = "Increases the healing of Renew by 10% per rank.",
            Class         = CharacterClass.Priest,
            TreeIndex     = 1,
            Row           = 1,
            Column        = 2,
            MaxRanks      = 2,
            EffectType    = TalentEffectType.RenewHealing,
            EffectPerRank = 0.10,
            IconUrl       = "https://wow.zamimg.com/images/wow/icons/medium/spell_holy_renew.jpg",
        },

        // ── Shadow-Baum (2), Reihe 0 ──────────────────────────────────────────
        new TalentDefinition
        {
            Id            = "priest_shadow_imp_swp",
            Name          = "Improved Shadow Word: Pain",
            Description   = "Increases the damage of Shadow Word: Pain by 6% per rank.",
            Class         = CharacterClass.Priest,
            TreeIndex     = 2,
            Row           = 0,
            Column        = 0,
            MaxRanks      = 5,
            EffectType    = TalentEffectType.ShadowWordPainDamage,
            EffectPerRank = 0.06,
            IconUrl       = "https://wow.zamimg.com/images/wow/icons/medium/spell_shadow_shadowwordpain.jpg",
        },
        new TalentDefinition
        {
            Id            = "priest_shadow_critical_shadow",
            Name          = "Shadow Power",
            Description   = "Increases the damage of all your abilities by 4% per rank.",
            Class         = CharacterClass.Priest,
            TreeIndex     = 2,
            Row           = 0,
            Column        = 1,
            MaxRanks      = 5,
            EffectType    = TalentEffectType.DamageBonus,
            EffectPerRank = 0.04,
            IconUrl       = "https://wow.zamimg.com/images/wow/icons/medium/spell_shadow_shadowbolt.jpg",
        },

        // ── Shadow-Baum (2), Reihe 1 ──────────────────────────────────────────
        new TalentDefinition
        {
            Id            = "priest_shadow_shadowform",
            Name          = "Shadowform",
            Description   = "Grants the ability Shadowform. While equipped, you assume a Shadowform: Smite, Mind Blast and Shadow Word: Pain deal 25% more damage and you take 15% less damage.",
            Class         = CharacterClass.Priest,
            TreeIndex     = 2,
            Row           = 1,
            Column        = 0,
            MaxRanks      = 1,
            EffectType    = TalentEffectType.GrantShadowForm,
            EffectPerRank = 1.0,
            GrantsAbilityId = "Shadow_Form",
            IconUrl       = "https://wow.zamimg.com/images/wow/icons/medium/spell_shadow_shadowform.jpg",
        },
        new TalentDefinition
        {
            Id            = "priest_shadow_mindful_mind_blast",
            Name          = "Improved Mind Blast",
            Description   = "Increases the damage of Mind Blast by 15% per rank.",
            Class         = CharacterClass.Priest,
            TreeIndex     = 2,
            Row           = 1,
            Column        = 1,
            MaxRanks      = 2,
            EffectType    = TalentEffectType.MindBlastDamage,
            EffectPerRank = 0.15,
            IconUrl       = "https://wow.zamimg.com/images/wow/icons/medium/spell_shadow_unholyfrenzy.jpg",
        },
        new TalentDefinition
        {
            Id            = "priest_shadow_imp_drain_mana",
            Name          = "Improved Drain Mana",
            Description   = "Increases the mana restored by Drain Mana by 10% per rank.",
            Class         = CharacterClass.Priest,
            TreeIndex     = 2,
            Row           = 1,
            Column        = 2,
            MaxRanks      = 3,
            EffectType    = TalentEffectType.DrainManaBonus,
            EffectPerRank = 0.10,
            IconUrl       = "https://wow.zamimg.com/images/wow/icons/medium/spell_shadow_siphonmana.jpg",
        },
    ];
}
