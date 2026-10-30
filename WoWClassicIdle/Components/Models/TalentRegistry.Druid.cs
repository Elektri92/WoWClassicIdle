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
            Description   = "Increases the damage of Wrath and Starfire by 2% per rank.",
            Class         = CharacterClass.Druid,
            TreeIndex     = 0,
            Row           = 0,
            Column        = 1,
            MaxRanks      = 5,
            EffectType    = TalentEffectType.WrathDamage,   // was mana cost – gave 0 % in the sim
            EffectPerRank = 0.02,
            IconUrl       = "https://wow.zamimg.com/images/wow/icons/medium/spell_nature_abolishmagic.jpg",
        },

        // ── Balance-Baum (0), Reihe 1 ─────────────────────────────────────────
        new TalentDefinition
        {
            Id              = "druid_bal_moonkin_form",
            Name            = "Moonkin Form",
            Description     = "Grants the ability Moonkin Form. While equipped, you are shapeshifted into a Moonkin and Wrath, Starfire and Moonfire deal 15% more damage. Only one form can be equipped at a time.",
            Class           = CharacterClass.Druid,
            TreeIndex       = 0,
            Row             = 1,
            Column          = 0,
            MaxRanks        = 1,
            EffectType      = TalentEffectType.GrantMoonkinForm,
            EffectPerRank   = 1.0,
            GrantsAbilityId = "Moonkin_Form",
            IconUrl         = "https://wow.zamimg.com/images/wow/icons/medium/spell_nature_forceofnature.jpg",
        },
        new TalentDefinition
        {
            Id            = "druid_bal_imp_moonfire",
            Name          = "Improved Moonfire",
            Description   = "Increases the damage of Moonfire by 10% per rank.",
            Class         = CharacterClass.Druid,
            TreeIndex     = 0,
            Row           = 1,
            Column        = 1,
            MaxRanks      = 3,
            EffectType    = TalentEffectType.MoonfireDamage,
            EffectPerRank = 0.10,
            IconUrl       = "https://wow.zamimg.com/images/wow/icons/medium/spell_nature_sentinal.jpg",
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

        // ── Feral-Baum (1), Reihe 1 ───────────────────────────────────────────
        new TalentDefinition
        {
            Id                   = "druid_feral_bear_form",
            Name                 = "Bear Form",
            Description          = "Grants the abilities Bear Form and Maul. While equipped, you are shapeshifted into a Bear: you use Rage instead of Mana and take 6% less damage. Spells that cost Mana or Energy cannot be equipped, except Thorns and Rejuvenation, which cost Rage instead. Only one form can be equipped at a time.",
            Class                = CharacterClass.Druid,
            TreeIndex            = 1,
            Row                  = 0,
            LevelRequired        = 12,         // was row 2 (Lv 15) – Feral at -16 % XP/h at Lv 11–14
            Column               = 3,
            MaxRanks             = 1,
            EffectType           = TalentEffectType.GrantBearForm,
            EffectPerRank        = 1.0,
            GrantsAbilityId      = "Bear_Form",
            AlsoGrantsAbilityIds = ["Maul_r1"],
            IconUrl              = "https://wow.zamimg.com/images/wow/icons/medium/ability_racial_bearform.jpg",
        },
        new TalentDefinition
        {
            Id               = "druid_feral_imp_maul",
            Name             = "Improved Maul",
            Description      = "Increases the damage of Maul by 5% per rank.",
            Class            = CharacterClass.Druid,
            TreeIndex        = 1,
            Row              = 1,
            Column           = 1,
            MaxRanks         = 5,
            RequiresTalentId = "druid_feral_bear_form",
            EffectType       = TalentEffectType.MaulDamage,
            EffectPerRank    = 0.05,
            IconUrl          = "https://wow.zamimg.com/images/wow/icons/medium/ability_druid_maul.jpg",
        },
        new TalentDefinition
        {
            Id                   = "druid_feral_cat_form",
            Name                 = "Cat Form",
            Description          = "Grants the abilities Cat Form, Claw and Rip. While equipped, you are shapeshifted into a Cat and use Energy instead of Mana. Spells that cost Mana or Rage cannot be equipped, except Rejuvenation, which costs Energy instead. Only one form can be equipped at a time.",
            Class                = CharacterClass.Druid,
            TreeIndex            = 1,
            Row                  = 0,
            LevelRequired        = 12,         // was row 2 (Lv 15) – Feral at -20 % XP/h at Lv 11–14
            Column               = 2,
            MaxRanks             = 1,
            EffectType           = TalentEffectType.GrantCatForm,
            EffectPerRank        = 1.0,
            GrantsAbilityId      = "Cat_Form",
            AlsoGrantsAbilityIds = ["Claw_r1", "Rip_r1"],
            IconUrl              = "https://wow.zamimg.com/images/wow/icons/medium/ability_druid_catform.jpg",
        },
        new TalentDefinition
        {
            Id               = "druid_feral_imp_claw",
            Name             = "Improved Claw",
            Description      = "Increases the damage of Claw by 5% per rank.",
            Class            = CharacterClass.Druid,
            TreeIndex        = 1,
            Row              = 1,
            Column           = 3,
            MaxRanks         = 5,
            RequiresTalentId = "druid_feral_cat_form",
            EffectType       = TalentEffectType.ClawDamage,
            EffectPerRank    = 0.05,
            IconUrl          = "https://wow.zamimg.com/images/wow/icons/medium/ability_druid_rake.jpg",
        },

        // ── Restoration-Baum (2), Reihe 0 ────────────────────────────────────
        new TalentDefinition
        {
            Id            = "druid_rest_damage_avoider",
            Name          = "Improved Thorns",
            Description   = "Increases the damage dealt by Thorns by 8% per rank.",
            Class         = CharacterClass.Druid,
            TreeIndex     = 2,
            Row           = 0,
            Column        = 0,
            MaxRanks      = 5,
            EffectType    = TalentEffectType.ThornsDamage,
            EffectPerRank = 0.08,
            IconUrl       = "https://wow.zamimg.com/images/wow/icons/medium/spell_nature_thorns.jpg",
        },
        new TalentDefinition
        {
            Id            = "druid_rest_healing_roots",
            Name          = "Nature's Focus",
            Description   = "Increases the damage of all your abilities by 2% per rank.",
            Class         = CharacterClass.Druid,
            TreeIndex     = 2,
            Row           = 0,
            Column        = 1,
            MaxRanks      = 5,
            EffectType    = TalentEffectType.DamageBonus,
            EffectPerRank = 0.02,
            IconUrl       = "https://wow.zamimg.com/images/wow/icons/medium/spell_nature_rejuvenation.jpg",
        },

        // ── Restoration-Baum (2), Reihe 1 ────────────────────────────────────
        new TalentDefinition
        {
            Id              = "druid_rest_tree_form",
            Name            = "Tree Form",
            Description     = "Grants the ability Tree Form. While equipped, you are shapeshifted into a Tree: all healing done is increased by 20%, mana regeneration from Spirit by 50% and Wrath and Moonfire deal 10% more damage. Only one form can be equipped at a time.",
            Class           = CharacterClass.Druid,
            TreeIndex       = 2,
            Row             = 1,
            Column          = 0,
            MaxRanks        = 1,
            EffectType      = TalentEffectType.GrantTreeForm,
            EffectPerRank   = 1.0,
            GrantsAbilityId = "Tree_Form",
            IconUrl         = "https://wow.zamimg.com/images/wow/icons/medium/ability_druid_treeoflife.jpg",
        },
        new TalentDefinition
        {
            Id            = "druid_rest_imp_regrowth",
            Name          = "Improved Insect Swarm",
            Description   = "Increases the damage of Insect Swarm by 15% per rank.",
            Class         = CharacterClass.Druid,
            TreeIndex     = 2,
            Row           = 1,
            Column        = 1,
            MaxRanks      = 3,
            EffectType    = TalentEffectType.InsectSwarmDamage,
            EffectPerRank = 0.15,
            IconUrl       = "https://wow.zamimg.com/images/wow/icons/medium/spell_nature_insectswarm.jpg",
        },
    ];
}
