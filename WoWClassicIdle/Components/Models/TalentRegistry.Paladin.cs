namespace Models;

public static partial class TalentRegistry
{
    private static List<TalentDefinition> BuildPaladinTalents() =>
    [
        // ── Holy-Baum (0), Reihe 0 ────────────────────────────────────────────
        new TalentDefinition
        {
            Id            = "paladin_holy_light_efficiency",
            Name          = "Holy Light Efficiency",
            Description   = "Reduces the mana cost of Holy Light by 1% per rank.",
            Class         = CharacterClass.Paladin,
            TreeIndex     = 0,
            Row           = 0,
            Column        = 0,
            MaxRanks      = 5,
            EffectType    = TalentEffectType.HolyLightManaCost,
            EffectPerRank = 0.01,
            IconUrl       = "https://wow.zamimg.com/images/wow/icons/medium/spell_holy_holybolt.jpg",
        },
        new TalentDefinition
        {
            Id            = "paladin_holy_healing_mastery",
            Name          = "Healing Mastery",
            Description   = "Increases healing by 2% per rank.",
            Class         = CharacterClass.Paladin,
            TreeIndex     = 0,
            Row           = 0,
            Column        = 1,
            MaxRanks      = 5,
            EffectType    = TalentEffectType.HealingBonus,
            EffectPerRank = 0.02,
            IconUrl       = "https://wow.zamimg.com/images/wow/icons/medium/spell_holy_heal.jpg",
        },

        // ── Protection-Baum (1), Reihe 0 ──────────────────────────────────────
        new TalentDefinition
        {
            Id            = "paladin_prot_bubble_up",
            Name          = "Bubble-up",
            Description   = "Increases the duration of Divine Shield by 1 second per rank.",
            Class         = CharacterClass.Paladin,
            TreeIndex     = 1,
            Row           = 0,
            Column        = 0,
            MaxRanks      = 5,
            EffectType    = TalentEffectType.DivineshieldDuration,
            EffectPerRank = 1.0,
            IconUrl       = "https://wow.zamimg.com/images/wow/icons/medium/spell_holy_divineshield.jpg",
        },
        new TalentDefinition
        {
            Id            = "paladin_prot_shield_mastery",
            Name          = "Shield Mastery",
            Description   = "Reduces damage taken by 1% per rank.",
            Class         = CharacterClass.Paladin,
            TreeIndex     = 1,
            Row           = 0,
            Column        = 1,
            MaxRanks      = 5,
            EffectType    = TalentEffectType.DamageTaken,
            EffectPerRank = 0.01,
            IconUrl       = "https://wow.zamimg.com/images/wow/icons/medium/inv_shield_06.jpg",
        },

        // ── Retribution-Baum (2), Reihe 0 ─────────────────────────────────────
        new TalentDefinition
        {
            Id            = "paladin_ret_speedy_attacks",
            Name          = "Speedy Attacks",
            Description   = "Increases auto-attack speed by 1% per rank.",
            Class         = CharacterClass.Paladin,
            TreeIndex     = 2,
            Row           = 0,
            Column        = 0,
            MaxRanks      = 3,
            EffectType    = TalentEffectType.AutoAttackSpeed,
            EffectPerRank = 0.01,
            IconUrl       = "https://wow.zamimg.com/images/wow/icons/medium/ability_meleedamage.jpg",
        },
        new TalentDefinition
        {
            Id            = "paladin_ret_rightful_crits",
            Name          = "Rightful Crits",
            Description   = "Increases critical strike chance by 1% per rank.",
            Class         = CharacterClass.Paladin,
            TreeIndex     = 2,
            Row           = 0,
            Column        = 1,
            MaxRanks      = 3,
            EffectType    = TalentEffectType.CritChance,
            EffectPerRank = 0.01,
            IconUrl       = "https://wow.zamimg.com/images/wow/icons/medium/spell_holy_auraoflight.jpg",
        },
        new TalentDefinition
        {
            Id            = "paladin_ret_holy_strike_efficiency",
            Name          = "Holy Strike Efficiency",
            Description   = "Reduces the mana cost of Holy Strike by 1% per rank.",
            Class         = CharacterClass.Paladin,
            TreeIndex     = 2,
            Row           = 0,
            Column        = 2,
            MaxRanks      = 5,
            EffectType    = TalentEffectType.HolyStrikeManaCost,
            EffectPerRank = 0.01,
            IconUrl       = "https://wow.zamimg.com/images/wow/icons/medium/spell_holy_righteousfury.jpg",
        },
    ];
}
