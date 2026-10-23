namespace Models;

public static partial class TalentRegistry
{
    private static List<TalentDefinition> BuildHunterTalents() =>
    [
        // ── Beast Mastery-Baum (0), Reihe 0 ───────────────────────────────────
        new TalentDefinition
        {
            Id            = "hunter_bm_imp_raptor_strike",
            Name          = "Improved Raptor Strike",
            Description   = "Reduces the mana cost of Raptor Strike by 1 per rank.",
            Class         = CharacterClass.Hunter,
            TreeIndex     = 0,
            Row           = 0,
            Column        = 0,
            MaxRanks      = 5,
            EffectType    = TalentEffectType.RaptorStrikeManaCost,
            EffectPerRank = 1.0,
            IconUrl       = "https://wow.zamimg.com/images/wow/icons/medium/ability_meleedamage.jpg",
        },
        new TalentDefinition
        {
            Id            = "hunter_bm_imp_serpent_sting",
            Name          = "Improved Serpent Sting",
            Description   = "Reduces the mana cost of Serpent Sting by 1 per rank.",
            Class         = CharacterClass.Hunter,
            TreeIndex     = 0,
            Row           = 0,
            Column        = 1,
            MaxRanks      = 5,
            EffectType    = TalentEffectType.SerpentStingManaCost,
            EffectPerRank = 1.0,
            IconUrl       = "https://wow.zamimg.com/images/wow/icons/medium/ability_poisonsting.jpg",
        },

        // ── Marksmanship-Baum (1), Reihe 0 ───────────────────────────────────
        new TalentDefinition
        {
            Id            = "hunter_mm_focused_shot",
            Name          = "Focused Shot",
            Description   = "Increases your critical strike chance by 1% per rank.",
            Class         = CharacterClass.Hunter,
            TreeIndex     = 1,
            Row           = 0,
            Column        = 0,
            MaxRanks      = 5,
            EffectType    = TalentEffectType.CritChance,
            EffectPerRank = 0.01,
            IconUrl       = "https://wow.zamimg.com/images/wow/icons/medium/ability_hunter_snipershot.jpg",
        },
        new TalentDefinition
        {
            Id            = "hunter_mm_marked_for_death",
            Name          = "Marked for Death",
            Description   = "Increases the damage bonus of Hunter's Mark by 1% per rank.",
            Class         = CharacterClass.Hunter,
            TreeIndex     = 1,
            Row           = 0,
            Column        = 1,
            MaxRanks      = 5,
            EffectType    = TalentEffectType.HuntersMarkBonus,
            EffectPerRank = 0.01,
            IconUrl       = "https://wow.zamimg.com/images/wow/icons/medium/ability_hunter_humiliationsting.jpg",
        },

        // ── Survival-Baum (2), Reihe 0 ────────────────────────────────────────
        new TalentDefinition
        {
            Id            = "hunter_surv_auto_survivor",
            Name          = "Auto Survivor",
            Description   = "Increases auto-attack speed by 1% per rank.",
            Class         = CharacterClass.Hunter,
            TreeIndex     = 2,
            Row           = 0,
            Column        = 0,
            MaxRanks      = 5,
            EffectType    = TalentEffectType.AutoAttackSpeed,
            EffectPerRank = 0.01,
            IconUrl       = "https://wow.zamimg.com/images/wow/icons/medium/ability_racial_avatar.jpg",
        },
        new TalentDefinition
        {
            Id            = "hunter_surv_dodge_master",
            Name          = "Dodge Master",
            Description   = "Reduces damage taken by 1% per rank.",
            Class         = CharacterClass.Hunter,
            TreeIndex     = 2,
            Row           = 0,
            Column        = 1,
            MaxRanks      = 5,
            EffectType    = TalentEffectType.DamageTaken,
            EffectPerRank = 0.01,
            IconUrl       = "https://wow.zamimg.com/images/wow/icons/medium/ability_dodge.jpg",
        },
    ];
}
