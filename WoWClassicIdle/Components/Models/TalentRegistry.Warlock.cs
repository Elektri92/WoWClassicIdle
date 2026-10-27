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
            Name          = "Master Demonologist",
            Description   = "Increases the damage dealt by your demon by 6% per rank.",
            Class         = CharacterClass.Warlock,
            TreeIndex     = 1,
            Row           = 0,
            Column        = 0,
            MaxRanks      = 5,
            EffectType    = TalentEffectType.PetDamageBonus,
            EffectPerRank = 0.06,
            IconUrl       = "https://wow.zamimg.com/images/wow/icons/medium/spell_shadow_shadowpact.jpg",
        },
        new TalentDefinition
        {
            Id            = "warlock_demo_imp_life_tap",
            Name          = "Demonic Power",
            Description   = "Increases the damage of all your abilities by 1% per rank.",
            Class         = CharacterClass.Warlock,
            TreeIndex     = 1,
            Row           = 0,
            Column        = 1,
            MaxRanks      = 5,
            EffectType    = TalentEffectType.DamageBonus,
            EffectPerRank = 0.01,
            IconUrl       = "https://wow.zamimg.com/images/wow/icons/medium/spell_shadow_metamorphosis.jpg",
        },

        // ── Destruction-Baum (2), Reihe 0 ─────────────────────────────────────
        new TalentDefinition
        {
            Id            = "warlock_dest_imp_shadow_bolt",
            Name          = "Improved Shadow Bolt",
            Description   = "Increases the damage of Shadow Bolt by 3% per rank.",
            Class         = CharacterClass.Warlock,
            TreeIndex     = 2,
            Row           = 0,
            Column        = 0,
            MaxRanks      = 5,
            EffectType    = TalentEffectType.ShadowBoltDamage,
            EffectPerRank = 0.03,
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

        // ── Affliction-Baum (0), Reihe 1 ────────────────────────────────────
        new TalentDefinition
        {
            Id              = "warlock_aff_siphon_life",
            Name            = "Siphon Life",
            Description     = "Grants the ability Siphon Life: deals Shadow damage over time and heals you for the damage dealt.",
            Class           = CharacterClass.Warlock,
            TreeIndex       = 0,
            Row             = 1,
            Column          = 0,
            MaxRanks        = 1,
            EffectType      = TalentEffectType.GrantSiphonLife,
            EffectPerRank   = 1.0,
            GrantsAbilityId = "Siphon_Life_r1",
            IconUrl         = "https://wow.zamimg.com/images/wow/icons/medium/spell_shadow_requiem.jpg",
        },
        new TalentDefinition
        {
            Id            = "warlock_aff_imp_lifesteal",
            Name          = "Improved Lifesteal",
            Description   = "Increases the healing of your lifesteal abilities by 10% per rank.",
            Class         = CharacterClass.Warlock,
            TreeIndex     = 0,
            Row           = 1,
            Column        = 1,
            MaxRanks      = 3,
            EffectType    = TalentEffectType.LifestealBonus,
            EffectPerRank = 0.10,
            IconUrl       = "https://wow.zamimg.com/images/wow/icons/medium/spell_shadow_lifedrain02.jpg",
        },

        // ── Demonology-Baum (1), Reihe 1 ────────────────────────────────────
        new TalentDefinition
        {
            Id              = "warlock_demo_summon_demon",
            Name            = "Summon Demon",
            Description     = "Grants the ability Summon Voidwalker: a tanky demon that attacks on its own, has its own health pool and takes 10% of the damage dealt to you. A dead demon is resummoned automatically after 10 seconds.",
            Class           = CharacterClass.Warlock,
            TreeIndex       = 1,
            Row             = 1,
            Column          = 0,
            MaxRanks        = 1,
            EffectType      = TalentEffectType.GrantSummonDemon,
            EffectPerRank   = 1.0,
            GrantsAbilityId = "Summon_Voidwalker_r1",
            IconUrl         = "https://wow.zamimg.com/images/wow/icons/medium/spell_shadow_summonvoidwalker.jpg",
        },
        new TalentDefinition
        {
            Id            = "warlock_demo_strong_demon",
            Name          = "Strong Demon",
            Description   = "Increases your demon's health by 10% of your max HP per rank.",
            Class         = CharacterClass.Warlock,
            TreeIndex     = 1,
            Row           = 1,
            Column        = 1,
            MaxRanks      = 3,
            EffectType    = TalentEffectType.PetHealthBonus,
            EffectPerRank = 0.10,
            IconUrl       = "https://wow.zamimg.com/images/wow/icons/medium/spell_shadow_antishadow.jpg",
        },

        // ── Destruction-Baum (2), Reihe 1 ───────────────────────────────────
        new TalentDefinition
        {
            Id              = "warlock_dest_incinerate",
            Name            = "Incinerate",
            Description     = "Grants the ability Incinerate: deals high Fire damage, more than Shadow Bolt, with a cooldown.",
            Class           = CharacterClass.Warlock,
            TreeIndex       = 2,
            Row             = 1,
            Column          = 0,
            MaxRanks        = 1,
            EffectType      = TalentEffectType.GrantIncinerate,
            EffectPerRank   = 1.0,
            GrantsAbilityId = "Incinerate_r1",
            IconUrl         = "https://wow.zamimg.com/images/wow/icons/medium/spell_fire_burnout.jpg",
        },
        new TalentDefinition
        {
            Id            = "warlock_dest_incinerating_shadow",
            Name          = "Incinerating Shadow",
            Description   = "Shadow Bolt increases the damage of your next Incinerate by 10% per rank.",
            Class         = CharacterClass.Warlock,
            TreeIndex     = 2,
            Row           = 1,
            Column        = 1,
            MaxRanks      = 3,
            EffectType    = TalentEffectType.IncinerateEmpower,
            EffectPerRank = 0.10,
            IconUrl       = "https://wow.zamimg.com/images/wow/icons/medium/spell_shadow_shadowbolt.jpg",
        },
    ];
}
