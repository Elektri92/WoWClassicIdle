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

        // ── Elemental-Baum (0), Reihe 1 ───────────────────────────────────────
        new TalentDefinition
        {
            Id            = "shaman_ele_chain_lightning",
            Name          = "Chain Lightning",
            Description   = "Grants the ability Chain Lightning: a stronger Lightning Bolt that strikes the enemy 3 times and has a 20% increased critical strike chance.",
            Class         = CharacterClass.Shaman,
            TreeIndex     = 0,
            Row           = 1,
            Column        = 0,
            MaxRanks      = 1,
            EffectType    = TalentEffectType.GrantChainLightning,
            EffectPerRank = 1.0,
            GrantsAbilityId = "Chain_Lightning_r1",
            IconUrl       = "https://wow.zamimg.com/images/wow/icons/medium/spell_nature_chainlightning.jpg",
        },
        new TalentDefinition
        {
            Id            = "shaman_ele_imp_flame_shock",
            Name          = "Improved Flame Shock",
            Description   = "Increases the damage of Flame Shock (direct and over time) by 10% per rank.",
            Class         = CharacterClass.Shaman,
            TreeIndex     = 0,
            Row           = 1,
            Column        = 1,
            MaxRanks      = 3,
            EffectType    = TalentEffectType.FlameShockDamage,
            EffectPerRank = 0.10,
            IconUrl       = "https://wow.zamimg.com/images/wow/icons/medium/spell_fire_flameshock.jpg",
        },

        // ── Enhancement-Baum (1), Reihe 0 ────────────────────────────────────
        new TalentDefinition
        {
            Id            = "shaman_enh_imp_lightning_shield",
            Name          = "Improved Lightning Shield",
            Description   = "Increases the damage dealt by Lightning Shield by 10% per rank.",
            Class         = CharacterClass.Shaman,
            TreeIndex     = 1,
            Row           = 0,
            Column        = 0,
            MaxRanks      = 5,
            EffectType    = TalentEffectType.LightningShieldDamage,
            EffectPerRank = 0.10,
            IconUrl       = "https://wow.zamimg.com/images/wow/icons/medium/spell_nature_lightningshield.jpg",
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

        // ── Enhancement-Baum (1), Reihe 1 ────────────────────────────────────
        new TalentDefinition
        {
            Id            = "shaman_enh_windfury_weapon",
            Name          = "Windfury Weapon",
            Description   = "Grants the ability Windfury Weapon: each auto attack has a 20% chance to strike 2 additional times. Only one weapon enchant can be equipped at a time (replaces Rockbiter Weapon).",
            Class         = CharacterClass.Shaman,
            TreeIndex     = 1,
            Row           = 1,
            Column        = 0,
            MaxRanks      = 1,
            EffectType    = TalentEffectType.GrantWindfuryWeapon,
            EffectPerRank = 1.0,
            GrantsAbilityId = "Windfury_Weapon_r1",
            IconUrl       = "https://wow.zamimg.com/images/wow/icons/medium/spell_nature_windfury.jpg",
        },
        new TalentDefinition
        {
            Id            = "shaman_enh_flurry",
            Name          = "Flurry",
            Description   = "After a melee critical strike, increases your attack speed by 10% per rank for the next 3 swings.",
            Class         = CharacterClass.Shaman,
            TreeIndex     = 1,
            Row           = 1,
            Column        = 1,
            MaxRanks      = 3,
            EffectType    = TalentEffectType.FlurryAttackSpeed,
            EffectPerRank = 0.10,
            IconUrl       = "https://wow.zamimg.com/images/wow/icons/medium/ability_ghoulfrenzy.jpg",
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

        // ── Restoration-Baum (2), Reihe 1 ────────────────────────────────────
        new TalentDefinition
        {
            Id            = "shaman_rest_mana_tide",
            Name          = "Mana Tide Totem",
            Description   = "Grants the ability Mana Tide Totem: restores 24% of your maximum mana over 12 sec. Used automatically below 50% mana.",
            Class         = CharacterClass.Shaman,
            TreeIndex     = 2,
            Row           = 1,
            Column        = 0,
            MaxRanks      = 1,
            EffectType    = TalentEffectType.GrantManaTideTotem,
            EffectPerRank = 1.0,
            GrantsAbilityId = "Mana_Tide_Totem_r1",
            IconUrl       = "https://wow.zamimg.com/images/wow/icons/medium/spell_frost_summonwaterelemental.jpg",
        },
        new TalentDefinition
        {
            Id            = "shaman_rest_restoration_mana",
            Name          = "Restoration Mana",
            Description   = "Increases mana regeneration from Spirit by 10% per rank.",
            Class         = CharacterClass.Shaman,
            TreeIndex     = 2,
            Row           = 1,
            Column        = 1,
            MaxRanks      = 3,
            EffectType    = TalentEffectType.SpiritManaRegen,
            EffectPerRank = 0.10,
            IconUrl       = "https://wow.zamimg.com/images/wow/icons/medium/spell_nature_manaregentotem.jpg",
        },
        new TalentDefinition
        {
            Id            = "shaman_rest_imp_healing_wave",
            Name          = "Improved Healing Wave",
            Description   = "Increases the healing of Healing Wave by 10% per rank.",
            Class         = CharacterClass.Shaman,
            TreeIndex     = 2,
            Row           = 1,
            Column        = 2,
            MaxRanks      = 2,
            EffectType    = TalentEffectType.HealingWaveHealing,
            EffectPerRank = 0.10,
            IconUrl       = "https://wow.zamimg.com/images/wow/icons/medium/spell_nature_magicimmunity.jpg",
        },
    ];
}
