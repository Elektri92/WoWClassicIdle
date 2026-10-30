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
            Description   = "Increases the damage of Raptor Strike by 3% and reduces its mana cost by 3% per rank.",
            Class         = CharacterClass.Hunter,
            TreeIndex     = 0,
            Row           = 0,
            Column        = 0,
            MaxRanks      = 5,
            EffectType    = TalentEffectType.RaptorStrikeBonus,
            EffectPerRank = 0.03,
            IconUrl       = "https://wow.zamimg.com/images/wow/icons/medium/ability_meleedamage.jpg",
        },
        new TalentDefinition
        {
            Id            = "hunter_bm_imp_serpent_sting",
            Name          = "Improved Serpent Sting",
            Description   = "Increases the damage of Serpent Sting by 5% per rank.",
            Class         = CharacterClass.Hunter,
            TreeIndex     = 0,
            Row           = 0,
            Column        = 1,
            MaxRanks      = 5,
            EffectType    = TalentEffectType.SerpentStingDamage,
            EffectPerRank = 0.05,
            IconUrl       = "https://wow.zamimg.com/images/wow/icons/medium/ability_poisonsting.jpg",
        },

        // ── Marksmanship-Baum (1), Reihe 0 ───────────────────────────────────
        new TalentDefinition
        {
            Id            = "hunter_mm_focused_shot",
            Name          = "Focused Shot",
            Description   = "Increases your critical strike chance by 2% per rank.",
            Class         = CharacterClass.Hunter,
            TreeIndex     = 1,
            Row           = 0,
            Column        = 0,
            MaxRanks      = 5,
            EffectType    = TalentEffectType.CritChance,
            EffectPerRank = 0.02,
            IconUrl       = "https://wow.zamimg.com/images/wow/icons/medium/ability_hunter_snipershot.jpg",
        },
        new TalentDefinition
        {
            Id            = "hunter_mm_marked_for_death",
            Name          = "Marked for Death",
            Description   = "Increases the damage bonus of Hunter's Mark by 2% per rank.",
            Class         = CharacterClass.Hunter,
            TreeIndex     = 1,
            Row           = 0,
            Column        = 1,
            MaxRanks      = 5,
            EffectType    = TalentEffectType.HuntersMarkBonus,
            EffectPerRank = 0.02,
            IconUrl       = "https://wow.zamimg.com/images/wow/icons/medium/ability_hunter_humiliationsting.jpg",
        },

        // ── Survival-Baum (2), Reihe 0 ────────────────────────────────────────
        new TalentDefinition
        {
            Id            = "hunter_surv_auto_survivor",
            Name          = "Auto Survivor",
            Description   = "Increases auto-attack speed by 2% per rank.",
            Class         = CharacterClass.Hunter,
            TreeIndex     = 2,
            Row           = 0,
            Column        = 0,
            MaxRanks      = 5,
            EffectType    = TalentEffectType.AutoAttackSpeed,
            EffectPerRank = 0.02,
            IconUrl       = "https://wow.zamimg.com/images/wow/icons/medium/ability_racial_avatar.jpg",
        },
        new TalentDefinition
        {
            Id            = "hunter_surv_dodge_master",
            Name          = "Dodge Master",
            Description   = "Reduces damage taken by 2% per rank.",
            Class         = CharacterClass.Hunter,
            TreeIndex     = 2,
            Row           = 0,
            Column        = 1,
            MaxRanks      = 5,
            EffectType    = TalentEffectType.DamageTaken,
            EffectPerRank = 0.02,
            IconUrl       = "https://wow.zamimg.com/images/wow/icons/medium/ability_dodge.jpg",
        },

        // ── Beast Mastery-Baum (0), Reihe 1 ─────────────────────────────────
        new TalentDefinition
        {
            Id                   = "hunter_bm_tame_pet",
            Name                 = "Tame Pet",
            Description          = "Grants the abilities Call Cat (damage) and Call Bear (tank). Your pet attacks on its own, has its own health pool and takes 10% of the damage dealt to you. A dead pet is revived automatically after 10 seconds.",
            Class                = CharacterClass.Hunter,
            TreeIndex            = 0,
            Row                  = 1,
            Column               = 0,
            MaxRanks             = 1,
            EffectType           = TalentEffectType.GrantTamePet,
            EffectPerRank        = 1.0,
            GrantsAbilityId      = "Call_Pet_Cat_r1",
            AlsoGrantsAbilityIds = ["Call_Pet_Bear_r1"],
            IconUrl              = "https://wow.zamimg.com/images/wow/icons/medium/ability_hunter_beasttaming.jpg",
        },
        new TalentDefinition
        {
            Id            = "hunter_bm_strong_pet",
            Name          = "Strong Pet",
            Description   = "Increases your pet's health by 10% of your max HP per rank.",
            Class         = CharacterClass.Hunter,
            TreeIndex     = 0,
            Row           = 1,
            Column        = 1,
            MaxRanks      = 3,
            EffectType    = TalentEffectType.PetHealthBonus,
            EffectPerRank = 0.10,
            IconUrl       = "https://wow.zamimg.com/images/wow/icons/medium/ability_hunter_pet_bear.jpg",
        },

        // ── Marksmanship-Baum (1), Reihe 1 ──────────────────────────────────
        new TalentDefinition
        {
            Id              = "hunter_mm_sniper_shot",
            Name            = "Sniper Shot",
            Description     = "Grants the ability Sniper Shot: a shot with a long cast time that deals very high damage.",
            Class           = CharacterClass.Hunter,
            TreeIndex       = 1,
            Row             = 1,
            Column          = 0,
            MaxRanks        = 1,
            EffectType      = TalentEffectType.GrantSniperShot,
            EffectPerRank   = 1.0,
            GrantsAbilityId = "Sniper_Shot_r1",
            IconUrl         = "https://wow.zamimg.com/images/wow/icons/medium/ability_marksmanship.jpg",
        },
        new TalentDefinition
        {
            Id            = "hunter_mm_imp_multi_shot",
            Name          = "Improved Multi-Shot",
            Description   = "Increases the damage of Multi-Shot by 10% per rank.",
            Class         = CharacterClass.Hunter,
            TreeIndex     = 1,
            Row           = 1,
            Column        = 1,
            MaxRanks      = 3,
            EffectType    = TalentEffectType.MultiShotDamage,
            EffectPerRank = 0.10,
            IconUrl       = "https://wow.zamimg.com/images/wow/icons/medium/ability_upgrademoonglaive.jpg",
        },

        // ── Survival-Baum (2), Reihe 1 ──────────────────────────────────────
        new TalentDefinition
        {
            Id              = "hunter_surv_mongoose_bite",
            Name            = "Mongoose Bite",
            Description     = "Grants the ability Mongoose Bite: a weapon strike that increases the damage of your next Raptor Strike by 45%.",
            Class           = CharacterClass.Hunter,
            TreeIndex       = 2,
            Row             = 1,
            Column          = 0,
            MaxRanks        = 1,
            EffectType      = TalentEffectType.GrantMongooseBite,
            EffectPerRank   = 1.0,
            GrantsAbilityId = "Mongoose_Bite_r1",
            IconUrl         = "https://wow.zamimg.com/images/wow/icons/medium/ability_hunter_swiftstrike.jpg",
        },
        new TalentDefinition
        {
            Id            = "hunter_surv_imp_raptor_strike",
            Name          = "Improved Raptor Strike",
            Description   = "Increases the damage of Raptor Strike by 10% and reduces its mana cost by 10% per rank.",
            Class         = CharacterClass.Hunter,
            TreeIndex     = 2,
            Row           = 1,
            Column        = 1,
            MaxRanks      = 3,
            EffectType    = TalentEffectType.RaptorStrikeBonus,
            EffectPerRank = 0.10,
            IconUrl       = "https://wow.zamimg.com/images/wow/icons/medium/ability_meleedamage.jpg",
        },
    ];
}
