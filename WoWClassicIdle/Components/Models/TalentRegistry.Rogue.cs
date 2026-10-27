namespace Models;

public static partial class TalentRegistry
{
    private static List<TalentDefinition> BuildRogueTalents() =>
    [
        // ── Assassination-Baum (0), Reihe 0 ───────────────────────────────────
        new TalentDefinition
        {
            Id            = "rogue_ass_critical_assassin",
            Name          = "Critical Assassin",
            Description   = "Increases your critical strike chance by 1% per rank.",
            Class         = CharacterClass.Rogue,
            TreeIndex     = 0,
            Row           = 0,
            Column        = 0,
            MaxRanks      = 5,
            EffectType    = TalentEffectType.CritChance,
            EffectPerRank = 0.01,
            IconUrl       = "https://wow.zamimg.com/images/wow/icons/medium/ability_rogue_slicedice.jpg",
        },
        new TalentDefinition
        {
            Id            = "rogue_ass_poison_master",
            Name          = "Poison Master",
            Description   = "Increases the maximum stacks of Deadly Poison by 1 per rank.",
            Class         = CharacterClass.Rogue,
            TreeIndex     = 0,
            Row           = 0,
            Column        = 1,
            MaxRanks      = 5,
            EffectType    = TalentEffectType.DeadlyPoisonStacks,
            EffectPerRank = 1.0,
            IconUrl       = "https://wow.zamimg.com/images/wow/icons/medium/ability_poisons.jpg",
        },

        // ── Combat-Baum (1), Reihe 0 ──────────────────────────────────────────
        new TalentDefinition
        {
            Id            = "rogue_combat_master_striker",
            Name          = "Master Striker",
            Description   = "Increases Sinister Strike damage by 2% per rank.",
            Class         = CharacterClass.Rogue,
            TreeIndex     = 1,
            Row           = 0,
            Column        = 0,
            MaxRanks      = 5,
            EffectType    = TalentEffectType.SinisterStrikeDamage,
            EffectPerRank = 0.02,
            IconUrl       = "https://wow.zamimg.com/images/wow/icons/medium/spell_shadow_ritualofsacrifice.jpg",
        },
        new TalentDefinition
        {
            Id            = "rogue_combat_fast_blade",
            Name          = "Fast Blade",
            Description   = "Increases auto-attack speed by 1% per rank.",
            Class         = CharacterClass.Rogue,
            TreeIndex     = 1,
            Row           = 0,
            Column        = 1,
            MaxRanks      = 3,
            EffectType    = TalentEffectType.AutoAttackSpeed,
            EffectPerRank = 0.01,
            IconUrl       = "https://wow.zamimg.com/images/wow/icons/medium/ability_rogue_sprint.jpg",
        },

        // ── Subtlety-Baum (2), Reihe 0 ────────────────────────────────────────
        new TalentDefinition
        {
            Id            = "rogue_sub_kidney_killer",
            Name          = "Kidney Killer",
            Description   = "Increases the duration of Kidney Shot by 5% per rank.",
            Class         = CharacterClass.Rogue,
            TreeIndex     = 2,
            Row           = 0,
            Column        = 0,
            MaxRanks      = 5,
            EffectType    = TalentEffectType.KidneyShotDuration,
            EffectPerRank = 0.05,
            IconUrl       = "https://wow.zamimg.com/images/wow/icons/medium/ability_rogue_kidneyshot.jpg",
        },
        new TalentDefinition
        {
            Id            = "rogue_sub_stealthy_killer",
            Name          = "Stealthy Killer",
            Description   = "Reduces damage taken by 1% per rank.",
            Class         = CharacterClass.Rogue,
            TreeIndex     = 2,
            Row           = 0,
            Column        = 1,
            MaxRanks      = 5,
            EffectType    = TalentEffectType.DamageTaken,
            EffectPerRank = 0.01,
            IconUrl       = "https://wow.zamimg.com/images/wow/icons/medium/ability_stealth.jpg",
        },

        // ── Assassination-Baum (0), Reihe 1 ─────────────────────────────────
        new TalentDefinition
        {
            Id              = "rogue_ass_cold_blood",
            Name            = "Cold Blood",
            Description     = "Grants the ability Cold Blood: your next damaging ability is a guaranteed critical strike.",
            Class           = CharacterClass.Rogue,
            TreeIndex       = 0,
            Row             = 1,
            Column          = 0,
            MaxRanks        = 1,
            EffectType      = TalentEffectType.GrantColdBlood,
            EffectPerRank   = 1.0,
            GrantsAbilityId = "Cold_Blood_r1",
            IconUrl         = "https://wow.zamimg.com/images/wow/icons/medium/spell_ice_lament.jpg",
        },
        new TalentDefinition
        {
            Id            = "rogue_ass_poison_procs",
            Name          = "Virulent Poisons",
            Description   = "Deadly Poison deals its damage 6% faster per rank.",
            Class         = CharacterClass.Rogue,
            TreeIndex     = 0,
            Row           = 1,
            Column        = 1,
            MaxRanks      = 5,
            EffectType    = TalentEffectType.DeadlyPoisonTickRate,
            EffectPerRank = 0.06,
            IconUrl       = "https://wow.zamimg.com/images/wow/icons/medium/ability_rogue_dualweild.jpg",
        },

        // ── Combat-Baum (1), Reihe 1 ────────────────────────────────────────
        new TalentDefinition
        {
            Id              = "rogue_combat_blade_flurry",
            Name            = "Blade Flurry",
            Description     = "Grants the ability Blade Flurry: your auto attacks strike twice for 10 seconds (45 sec cooldown).",
            Class           = CharacterClass.Rogue,
            TreeIndex       = 1,
            Row             = 1,
            Column          = 0,
            MaxRanks        = 1,
            EffectType      = TalentEffectType.GrantBladeFlurry,
            EffectPerRank   = 1.0,
            GrantsAbilityId = "Blade_Flurry_r1",
            IconUrl         = "https://wow.zamimg.com/images/wow/icons/medium/ability_warrior_punishingblow.jpg",
        },
        new TalentDefinition
        {
            Id            = "rogue_combat_eff_sinister_strike",
            Name          = "Efficient Sinister Strike",
            Description   = "Reduces the energy cost of Sinister Strike by 3 per rank.",
            Class         = CharacterClass.Rogue,
            TreeIndex     = 1,
            Row           = 1,
            Column        = 1,
            MaxRanks      = 3,
            EffectType    = TalentEffectType.SinisterStrikeEnergyCost,
            EffectPerRank = 3.0,
            IconUrl       = "https://wow.zamimg.com/images/wow/icons/medium/spell_shadow_ritualofsacrifice.jpg",
        },
        new TalentDefinition
        {
            Id            = "rogue_combat_imp_slice_and_dice",
            Name          = "Improved Slice and Dice",
            Description   = "Increases the attack speed bonus of Slice and Dice by 2% per rank.",
            Class         = CharacterClass.Rogue,
            TreeIndex     = 1,
            Row           = 1,
            Column        = 2,
            MaxRanks      = 2,
            EffectType    = TalentEffectType.SliceAndDiceBonus,
            EffectPerRank = 0.02,
            IconUrl       = "https://wow.zamimg.com/images/wow/icons/medium/ability_rogue_slicedice.jpg",
        },

        // ── Subtlety-Baum (2), Reihe 1 ──────────────────────────────────────
        new TalentDefinition
        {
            Id              = "rogue_sub_ghostly_strike",
            Name            = "Ghostly Strike",
            Description     = "Grants the ability Ghostly Strike: a weapon strike that also shields you for 10% of your max HP.",
            Class           = CharacterClass.Rogue,
            TreeIndex       = 2,
            Row             = 1,
            Column          = 0,
            MaxRanks        = 1,
            EffectType      = TalentEffectType.GrantGhostlyStrike,
            EffectPerRank   = 1.0,
            GrantsAbilityId = "Ghostly_Strike_r1",
            IconUrl         = "https://wow.zamimg.com/images/wow/icons/medium/spell_shadow_curse.jpg",
        },
        new TalentDefinition
        {
            Id            = "rogue_sub_strong_life",
            Name          = "Strong Life",
            Description   = "Increases the health gained from Stamina by 10% per rank.",
            Class         = CharacterClass.Rogue,
            TreeIndex     = 2,
            Row           = 1,
            Column        = 1,
            MaxRanks      = 3,
            EffectType    = TalentEffectType.StaminaHpBonus,
            EffectPerRank = 0.10,
            IconUrl       = "https://wow.zamimg.com/images/wow/icons/medium/spell_holy_wordfortitude.jpg",
        },
        new TalentDefinition
        {
            Id            = "rogue_sub_imp_expose_armor",
            Name          = "Improved Expose Armor",
            Description   = "Increases the effect of Expose Armor by 25% per rank.",
            Class         = CharacterClass.Rogue,
            TreeIndex     = 2,
            Row           = 1,
            Column        = 2,
            MaxRanks      = 2,
            EffectType    = TalentEffectType.ExposeArmorBonus,
            EffectPerRank = 0.25,
            IconUrl       = "https://wow.zamimg.com/images/wow/icons/medium/ability_warrior_riposte.jpg",
        },
    ];
}
