namespace Models;

public static partial class TalentRegistry
{
    private static List<TalentDefinition> BuildShamanTalents() =>
    [
        // ── Elemental-Baum (0), Reihe 0 ───────────────────────────────────────
        new TalentDefinition
        {
            Id            = "shaman_ele_mindful_lb",
            Name          = "Mindful Lightning Bolt",
            Description   = "Reduces the mana cost of Lightning Bolt by 1 per rank.",
            Class         = CharacterClass.Shaman,
            TreeIndex     = 0,
            Row           = 0,
            Column        = 0,
            MaxRanks      = 5,
            EffectType    = TalentEffectType.LightningBoltManaCost,
            EffectPerRank = 1.0,
            IconUrl       = "https://wow.zamimg.com/images/wow/icons/medium/spell_nature_lightning.jpg",
        },
        new TalentDefinition
        {
            Id            = "shaman_ele_imp_lb",
            Name          = "Improved Lightning Bolt",
            Description   = "Increases the damage of Lightning Bolt by 1 per rank.",
            Class         = CharacterClass.Shaman,
            TreeIndex     = 0,
            Row           = 0,
            Column        = 1,
            MaxRanks      = 5,
            EffectType    = TalentEffectType.LightningBoltDamage,
            EffectPerRank = 1.0,
            IconUrl       = "https://wow.zamimg.com/images/wow/icons/medium/spell_nature_lightningoverload.jpg",
        },

        // ── Enhancement-Baum (1), Reihe 0 ────────────────────────────────────
        new TalentDefinition
        {
            Id            = "shaman_enh_imp_rockbiter",
            Name          = "Improved Rockbiter Weapon",
            Description   = "Increases the auto-attack damage bonus of Rockbiter Weapon by 1% per rank.",
            Class         = CharacterClass.Shaman,
            TreeIndex     = 1,
            Row           = 0,
            Column        = 0,
            MaxRanks      = 5,
            EffectType    = TalentEffectType.RockbiterDmgBonus,
            EffectPerRank = 0.01,
            IconUrl       = "https://wow.zamimg.com/images/wow/icons/medium/spell_nature_rockbiter.jpg",
        },
        new TalentDefinition
        {
            Id            = "shaman_enh_critical_strike",
            Name          = "Critical Strike",
            Description   = "Increases your critical strike chance by 1% per rank.",
            Class         = CharacterClass.Shaman,
            TreeIndex     = 1,
            Row           = 0,
            Column        = 1,
            MaxRanks      = 5,
            EffectType    = TalentEffectType.CritChance,
            EffectPerRank = 0.01,
            IconUrl       = "https://wow.zamimg.com/images/wow/icons/medium/ability_rogue_slicedice.jpg",
        },

        // ── Restoration-Baum (2), Reihe 0 ────────────────────────────────────
        new TalentDefinition
        {
            Id            = "shaman_rest_faster_slow_totem",
            Name          = "Faster Slow Totem",
            Description   = "Reduces the cooldown of Earthbind Totem by 3 seconds per rank.",
            Class         = CharacterClass.Shaman,
            TreeIndex     = 2,
            Row           = 0,
            Column        = 0,
            MaxRanks      = 5,
            EffectType    = TalentEffectType.EarthbindCooldown,
            EffectPerRank = 3.0,
            IconUrl       = "https://wow.zamimg.com/images/wow/icons/medium/spell_nature_earthbindtotem.jpg",
        },
        new TalentDefinition
        {
            Id            = "shaman_rest_restoration_prot",
            Name          = "Restoration Protection",
            Description   = "Reduces damage taken by 1% per rank.",
            Class         = CharacterClass.Shaman,
            TreeIndex     = 2,
            Row           = 0,
            Column        = 1,
            MaxRanks      = 5,
            EffectType    = TalentEffectType.DamageTaken,
            EffectPerRank = 0.01,
            IconUrl       = "https://wow.zamimg.com/images/wow/icons/medium/inv_shield_06.jpg",
        },
    ];
}
