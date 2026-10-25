namespace Models;

public static partial class TalentRegistry
{
    private static List<TalentDefinition> BuildPaladinTalents() =>
    [
        // ── Holy-Baum (0), Reihe 0 ────────────────────────────────────────────
        new TalentDefinition
        {
            Id            = "paladin_holy_light_efficiency",
            Name          = "Holy Power",
            Description   = "Increases your critical strike chance by 1% per rank.",
            Class         = CharacterClass.Paladin,
            TreeIndex     = 0,
            Row           = 0,
            Column        = 0,
            MaxRanks      = 5,
            EffectType    = TalentEffectType.CritChance,
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
            Description   = "Reduces damage taken by 2% per rank.",
            Class         = CharacterClass.Paladin,
            TreeIndex     = 1,
            Row           = 0,
            Column        = 1,
            MaxRanks      = 5,
            EffectType    = TalentEffectType.DamageTaken,
            EffectPerRank = 0.02,
            IconUrl       = "https://wow.zamimg.com/images/wow/icons/medium/inv_shield_06.jpg",
        },

        // ── Retribution-Baum (2), Reihe 0 ─────────────────────────────────────
        new TalentDefinition
        {
            Id            = "paladin_ret_speedy_attacks",
            Name          = "Speedy Attacks",
            Description   = "Increases auto-attack speed by 2% per rank.",
            Class         = CharacterClass.Paladin,
            TreeIndex     = 2,
            Row           = 0,
            Column        = 0,
            MaxRanks      = 3,
            EffectType    = TalentEffectType.AutoAttackSpeed,
            EffectPerRank = 0.02,
            IconUrl       = "https://wow.zamimg.com/images/wow/icons/medium/ability_meleedamage.jpg",
        },
        new TalentDefinition
        {
            Id            = "paladin_ret_rightful_crits",
            Name          = "Rightful Crits",
            Description   = "Increases critical strike chance by 2% per rank.",
            Class         = CharacterClass.Paladin,
            TreeIndex     = 2,
            Row           = 0,
            Column        = 1,
            MaxRanks      = 3,
            EffectType    = TalentEffectType.CritChance,
            EffectPerRank = 0.02,
            IconUrl       = "https://wow.zamimg.com/images/wow/icons/medium/spell_holy_auraoflight.jpg",
        },
        new TalentDefinition
        {
            Id            = "paladin_ret_holy_strike_efficiency",
            Name          = "Holy Strike Mastery",
            Description   = "Increases the damage of Holy Strike by 3% per rank.",
            Class         = CharacterClass.Paladin,
            TreeIndex     = 2,
            Row           = 0,
            Column        = 2,
            MaxRanks      = 5,
            EffectType    = TalentEffectType.HolyStrikeDamage,
            EffectPerRank = 0.03,
            IconUrl       = "https://wow.zamimg.com/images/wow/icons/medium/spell_holy_righteousfury.jpg",
        },

        // ── Holy-Baum (0), Reihe 1 ──────────────────────────────────────────
        new TalentDefinition
        {
            Id              = "paladin_holy_holy_shock",
            Name            = "Holy Shock",
            Description     = "Grants the ability Holy Shock: deals Holy damage to the enemy and heals you at the same time.",
            Class           = CharacterClass.Paladin,
            TreeIndex       = 0,
            Row             = 1,
            Column          = 0,
            MaxRanks        = 1,
            EffectType      = TalentEffectType.GrantHolyShock,
            EffectPerRank   = 1.0,
            GrantsAbilityId = "Holy_Shock_r1",
            IconUrl         = "https://wow.zamimg.com/images/wow/icons/medium/spell_holy_searinglight.jpg",
        },
        new TalentDefinition
        {
            Id            = "paladin_holy_holy_barrier",
            Name          = "Holy Barrier",
            Description   = "Holy Light also shields you for 3% of your max HP per rank for 15 seconds.",
            Class         = CharacterClass.Paladin,
            TreeIndex     = 0,
            Row           = 1,
            Column        = 1,
            MaxRanks      = 3,
            EffectType    = TalentEffectType.HolyLightShield,
            EffectPerRank = 0.03,
            IconUrl       = "https://wow.zamimg.com/images/wow/icons/medium/spell_holy_powerwordshield.jpg",
        },

        // ── Protection-Baum (1), Reihe 1 ────────────────────────────────────
        new TalentDefinition
        {
            Id              = "paladin_prot_holy_shield",
            Name            = "Holy Shield",
            Description     = "Grants the ability Holy Shield: reduces the damage of the next 4 attacks against you by 35% and deals 16 Holy damage to the attacker each time.",
            Class           = CharacterClass.Paladin,
            TreeIndex       = 1,
            Row             = 1,
            Column          = 0,
            MaxRanks        = 1,
            EffectType      = TalentEffectType.GrantHolyShield,
            EffectPerRank   = 1.0,
            GrantsAbilityId = "Holy_Shield_r1",
            IconUrl         = "https://wow.zamimg.com/images/wow/icons/medium/spell_holy_blessingofprotection.jpg",
        },
        new TalentDefinition
        {
            Id            = "paladin_prot_imp_consecration",
            Name          = "Improved Consecration",
            Description   = "Increases the damage of Consecration by 25% per rank.",
            Class         = CharacterClass.Paladin,
            TreeIndex     = 1,
            Row           = 1,
            Column        = 1,
            MaxRanks      = 3,
            EffectType    = TalentEffectType.ConsecrationDamage,
            EffectPerRank = 0.25,
            IconUrl       = "https://wow.zamimg.com/images/wow/icons/medium/spell_holy_innerfire.jpg",
        },

        // ── Retribution-Baum (2), Reihe 1 ───────────────────────────────────
        new TalentDefinition
        {
            Id              = "paladin_ret_seal_of_command",
            Name            = "Seal of Command",
            Description     = "Grants the ability Seal of Command: your auto attacks deal 12% more damage and have a 25% chance to strike an additional time. Only one Seal can be active at a time.",
            Class           = CharacterClass.Paladin,
            TreeIndex       = 2,
            Row             = 1,
            Column          = 0,
            MaxRanks        = 1,
            EffectType      = TalentEffectType.GrantSealOfCommand,
            EffectPerRank   = 1.0,
            GrantsAbilityId = "Seal_of_Command_r1",
            IconUrl         = "https://wow.zamimg.com/images/wow/icons/medium/ability_warrior_innerrage.jpg",
        },
        new TalentDefinition
        {
            Id            = "paladin_ret_imp_holy_strike",
            Name          = "Improved Holy Strike",
            Description   = "Increases the damage of Holy Strike by 10% per rank.",
            Class         = CharacterClass.Paladin,
            TreeIndex     = 2,
            Row           = 1,
            Column        = 1,
            MaxRanks      = 3,
            EffectType    = TalentEffectType.HolyStrikeDamage,
            EffectPerRank = 0.10,
            IconUrl       = "https://wow.zamimg.com/images/wow/icons/medium/spell_holy_excorcism_02.jpg",
        },
    ];
}
