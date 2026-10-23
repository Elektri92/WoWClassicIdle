namespace Models;

public static partial class TalentRegistry
{
    private static List<TalentDefinition> BuildDruidTalents() =>
    [
        // ── Balance-Baum (0), Reihe 0 ─────────────────────────────────────────
        new TalentDefinition
        {
            Id            = "druid_bal_critical_moon",
            Name          = "Critical Moon",
            Description   = "Increases your critical strike chance by 1% per rank.",
            Class         = CharacterClass.Druid,
            TreeIndex     = 0,
            Row           = 0,
            Column        = 0,
            MaxRanks      = 5,
            EffectType    = TalentEffectType.CritChance,
            EffectPerRank = 0.01,
            IconUrl       = "https://wow.zamimg.com/images/wow/icons/medium/spell_nature_starfall.jpg",
        },
        new TalentDefinition
        {
            Id            = "druid_bal_imp_wrath",
            Name          = "Improved Wrath",
            Description   = "Reduces the mana cost of Wrath by 1 per rank.",
            Class         = CharacterClass.Druid,
            TreeIndex     = 0,
            Row           = 0,
            Column        = 1,
            MaxRanks      = 5,
            EffectType    = TalentEffectType.WrathManaCost,
            EffectPerRank = 1.0,
            IconUrl       = "https://wow.zamimg.com/images/wow/icons/medium/spell_nature_abolishmagic.jpg",
        },

        // ── Feral-Baum (1), Reihe 0 ───────────────────────────────────────────
        new TalentDefinition
        {
            Id            = "druid_feral_feral_speed",
            Name          = "Feral Speed",
            Description   = "Increases auto-attack speed by 2% per rank.",
            Class         = CharacterClass.Druid,
            TreeIndex     = 1,
            Row           = 0,
            Column        = 0,
            MaxRanks      = 5,
            EffectType    = TalentEffectType.AutoAttackSpeed,
            EffectPerRank = 0.02,
            IconUrl       = "https://wow.zamimg.com/images/wow/icons/medium/ability_racial_avatar.jpg",
        },
        new TalentDefinition
        {
            Id            = "druid_feral_feral_crit",
            Name          = "Feral Crit",
            Description   = "Increases your auto-attack critical strike chance by 2% per rank.",
            Class         = CharacterClass.Druid,
            TreeIndex     = 1,
            Row           = 0,
            Column        = 1,
            MaxRanks      = 5,
            EffectType    = TalentEffectType.AutoAttackCrit,
            EffectPerRank = 0.02,
            IconUrl       = "https://wow.zamimg.com/images/wow/icons/medium/ability_rogue_slicedice.jpg",
        },

        // ── Restoration-Baum (2), Reihe 0 ────────────────────────────────────
        new TalentDefinition
        {
            Id            = "druid_rest_damage_avoider",
            Name          = "Damage Avoider",
            Description   = "Reduces damage taken by 1% per rank.",
            Class         = CharacterClass.Druid,
            TreeIndex     = 2,
            Row           = 0,
            Column        = 0,
            MaxRanks      = 5,
            EffectType    = TalentEffectType.DamageTaken,
            EffectPerRank = 0.01,
            IconUrl       = "https://wow.zamimg.com/images/wow/icons/medium/ability_dodge.jpg",
        },
        new TalentDefinition
        {
            Id            = "druid_rest_healing_roots",
            Name          = "Healing Roots",
            Description   = "Increases all healing by 2% per rank.",
            Class         = CharacterClass.Druid,
            TreeIndex     = 2,
            Row           = 0,
            Column        = 1,
            MaxRanks      = 5,
            EffectType    = TalentEffectType.HealingBonus,
            EffectPerRank = 0.02,
            IconUrl       = "https://wow.zamimg.com/images/wow/icons/medium/spell_nature_rejuvenation.jpg",
        },
    ];
}
