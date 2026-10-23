namespace Models;

public static partial class TalentRegistry
{
    private static List<TalentDefinition> BuildWarlockTalents() =>
    [
        // ── Affliction-Baum (0), Reihe 0 ──────────────────────────────────────
        new TalentDefinition
        {
            Id            = "warlock_aff_imp_coa",
            Name          = "Improved Curse of Agony",
            Description   = "Increases the duration of Curse of Agony by 2 seconds per rank.",
            Class         = CharacterClass.Warlock,
            TreeIndex     = 0,
            Row           = 0,
            Column        = 0,
            MaxRanks      = 5,
            EffectType    = TalentEffectType.CurseOfAgonyDuration,
            EffectPerRank = 2.0,
            IconUrl       = "https://wow.zamimg.com/images/wow/icons/medium/spell_shadow_curseofsargeras.jpg",
        },
        new TalentDefinition
        {
            Id            = "warlock_aff_imp_drain_life",
            Name          = "Improved Drain Life",
            Description   = "Increases the damage per tick of Drain Life by 1 per rank.",
            Class         = CharacterClass.Warlock,
            TreeIndex     = 0,
            Row           = 0,
            Column        = 1,
            MaxRanks      = 2,
            EffectType    = TalentEffectType.DrainLifeTickDamage,
            EffectPerRank = 1.0,
            IconUrl       = "https://wow.zamimg.com/images/wow/icons/medium/spell_shadow_lifedrain02.jpg",
        },
        new TalentDefinition
        {
            Id            = "warlock_aff_wanding",
            Name          = "Wanding",
            Description   = "Increases auto-attack speed by 2% per rank.",
            Class         = CharacterClass.Warlock,
            TreeIndex     = 0,
            Row           = 0,
            Column        = 2,
            MaxRanks      = 3,
            EffectType    = TalentEffectType.AutoAttackSpeed,
            EffectPerRank = 0.02,
            IconUrl       = "https://wow.zamimg.com/images/wow/icons/medium/inv_wand_01.jpg",
        },

        // ── Demonology-Baum (1), Reihe 0 ──────────────────────────────────────
        new TalentDefinition
        {
            Id            = "warlock_demo_demon_tank",
            Name          = "Demon Tank",
            Description   = "Reduces damage taken by 1% per rank.",
            Class         = CharacterClass.Warlock,
            TreeIndex     = 1,
            Row           = 0,
            Column        = 0,
            MaxRanks      = 5,
            EffectType    = TalentEffectType.DamageTaken,
            EffectPerRank = 0.01,
            IconUrl       = "https://wow.zamimg.com/images/wow/icons/medium/spell_shadow_shadowpact.jpg",
        },
        new TalentDefinition
        {
            Id            = "warlock_demo_imp_life_tap",
            Name          = "Improved Life Tap",
            Description   = "Reduces the HP cost of Life Tap by 1 per rank.",
            Class         = CharacterClass.Warlock,
            TreeIndex     = 1,
            Row           = 0,
            Column        = 1,
            MaxRanks      = 5,
            EffectType    = TalentEffectType.LifeTapHpCostReduction,
            EffectPerRank = 1.0,
            IconUrl       = "https://wow.zamimg.com/images/wow/icons/medium/spell_shadow_burningspirit.jpg",
        },

        // ── Destruction-Baum (2), Reihe 0 ─────────────────────────────────────
        new TalentDefinition
        {
            Id            = "warlock_dest_imp_shadow_bolt",
            Name          = "Improved Shadow Bolt",
            Description   = "Reduces the mana cost of Shadow Bolt by 1 per rank.",
            Class         = CharacterClass.Warlock,
            TreeIndex     = 2,
            Row           = 0,
            Column        = 0,
            MaxRanks      = 5,
            EffectType    = TalentEffectType.ShadowBoltManaCost,
            EffectPerRank = 1.0,
            IconUrl       = "https://wow.zamimg.com/images/wow/icons/medium/spell_shadow_shadowbolt.jpg",
        },
        new TalentDefinition
        {
            Id            = "warlock_dest_destruction_overload",
            Name          = "Destruction Overload",
            Description   = "Increases your critical strike chance by 1% per rank.",
            Class         = CharacterClass.Warlock,
            TreeIndex     = 2,
            Row           = 0,
            Column        = 1,
            MaxRanks      = 5,
            EffectType    = TalentEffectType.CritChance,
            EffectPerRank = 0.01,
            IconUrl       = "https://wow.zamimg.com/images/wow/icons/medium/spell_fire_flamebolt.jpg",
        },
    ];
}
