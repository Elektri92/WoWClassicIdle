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
            Name          = "Wanding",
            Description   = "Increases auto-attack damage by 5% per rank.",
            Class         = CharacterClass.Priest,
            TreeIndex     = 0,
            Row           = 0,
            Column        = 2,
            MaxRanks      = 2,
            EffectType    = TalentEffectType.AutoAttackDamage,
            EffectPerRank = 0.05,
            IconUrl       = "https://wow.zamimg.com/images/wow/icons/medium/inv_wand_01.jpg",
        },

        // ── Holy-Baum (1), Reihe 0 ────────────────────────────────────────────
        new TalentDefinition
        {
            Id            = "priest_holy_imp_healing",
            Name          = "Improved Healing",
            Description   = "Increases all healing by 1% per rank.",
            Class         = CharacterClass.Priest,
            TreeIndex     = 1,
            Row           = 0,
            Column        = 0,
            MaxRanks      = 5,
            EffectType    = TalentEffectType.HealingBonus,
            EffectPerRank = 0.01,
            IconUrl       = "https://wow.zamimg.com/images/wow/icons/medium/spell_holy_healingfocus.jpg",
        },
        new TalentDefinition
        {
            Id            = "priest_holy_holy_speed",
            Name          = "Holy Speed",
            Description   = "Increases auto-attack speed by 1% per rank.",
            Class         = CharacterClass.Priest,
            TreeIndex     = 1,
            Row           = 0,
            Column        = 1,
            MaxRanks      = 3,
            EffectType    = TalentEffectType.AutoAttackSpeed,
            EffectPerRank = 0.01,
            IconUrl       = "https://wow.zamimg.com/images/wow/icons/medium/spell_holy_divinespirit.jpg",
        },

        // ── Shadow-Baum (2), Reihe 0 ──────────────────────────────────────────
        new TalentDefinition
        {
            Id            = "priest_shadow_imp_swp",
            Name          = "Improved Shadow Word: Pain",
            Description   = "Increases Shadow Word: Pain damage by 2% per rank.",
            Class         = CharacterClass.Priest,
            TreeIndex     = 2,
            Row           = 0,
            Column        = 0,
            MaxRanks      = 5,
            EffectType    = TalentEffectType.ShadowWordPainDamage,
            EffectPerRank = 0.02,
            IconUrl       = "https://wow.zamimg.com/images/wow/icons/medium/spell_shadow_shadowwordpain.jpg",
        },
        new TalentDefinition
        {
            Id            = "priest_shadow_critical_shadow",
            Name          = "Critical Shadow",
            Description   = "Increases your critical strike chance by 1% per rank.",
            Class         = CharacterClass.Priest,
            TreeIndex     = 2,
            Row           = 0,
            Column        = 1,
            MaxRanks      = 5,
            EffectType    = TalentEffectType.CritChance,
            EffectPerRank = 0.01,
            IconUrl       = "https://wow.zamimg.com/images/wow/icons/medium/spell_shadow_shadowbolt.jpg",
        },
    ];
}
