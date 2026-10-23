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
            Description   = "Increases the chance that Deadly Poison is applied by 2% per rank.",
            Class         = CharacterClass.Rogue,
            TreeIndex     = 0,
            Row           = 0,
            Column        = 1,
            MaxRanks      = 5,
            EffectType    = TalentEffectType.DeadlyPoisonProcChance,
            EffectPerRank = 0.02,
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
    ];
}
