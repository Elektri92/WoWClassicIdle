namespace Models;

public static partial class TalentRegistry
{
    private static List<TalentDefinition> BuildWarriorTalents() =>
    [
        // ── Arms-Baum (0), Reihe 0 ────────────────────────────────────────────
        new TalentDefinition
        {
            Id            = "warrior_arms_crit_mult",
            Name          = "Critical Multiplier",
            Description   = "Increases your critical strike chance by 1% per rank.",
            Class         = CharacterClass.Warrior,
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
            Id            = "warrior_arms_auto_striker",
            Name          = "Automatic Striker",
            Description   = "Increases auto-attack damage by 1% per rank.",
            Class         = CharacterClass.Warrior,
            TreeIndex     = 0,
            Row           = 0,
            Column        = 1,
            MaxRanks      = 5,
            EffectType    = TalentEffectType.AutoAttackDamage,
            EffectPerRank = 0.01,
            IconUrl       = "https://wow.zamimg.com/images/wow/icons/medium/ability_warrior_sunder.jpg",
        },

        // ── Arms-Baum (0), Reihe 1 ────────────────────────────────────────────
        new TalentDefinition
        {
            Id              = "warrior_arms_battle_stance",
            Name            = "Battle Stance",
            Description     = "Grants the ability Battle Stance. Increases rage generation from auto-attacks by 10%.",
            Class           = CharacterClass.Warrior,
            TreeIndex       = 0,
            Row             = 1,
            Column          = 0,
            MaxRanks        = 1,
            EffectType      = TalentEffectType.GrantBattleStance,
            EffectPerRank   = 1.0,
            GrantsAbilityId = "Battle_Stance",
            IconUrl         = "https://wow.zamimg.com/images/wow/icons/medium/ability_warrior_offensivestance.jpg",
        },
        new TalentDefinition
        {
            Id            = "warrior_arms_crit_heal",
            Name          = "Critical Heal",
            Description   = "Heals you for 1 HP when one of your abilities critically strikes.",
            Class         = CharacterClass.Warrior,
            TreeIndex     = 0,
            Row           = 1,
            Column        = 1,
            MaxRanks      = 2,
            EffectType    = TalentEffectType.CritHeal,
            EffectPerRank = 1.0,
            IconUrl       = "https://wow.zamimg.com/images/wow/icons/medium/spell_holy_sealofsacrifice.jpg",
        },
        new TalentDefinition
        {
            Id            = "warrior_arms_speedy_attacks",
            Name          = "Speedy Attacks",
            Description   = "Increases auto-attack speed by 1% per rank.",
            Class         = CharacterClass.Warrior,
            TreeIndex     = 0,
            Row           = 1,
            Column        = 2,
            MaxRanks      = 2,
            EffectType    = TalentEffectType.AutoAttackSpeed,
            EffectPerRank = 0.01,
            IconUrl       = "https://wow.zamimg.com/images/wow/icons/medium/ability_warrior_weaponmastery.jpg",
        },

        // ── Fury-Baum (1), Reihe 0 ────────────────────────────────────────────
        new TalentDefinition
        {
            Id            = "warrior_fury_more_rage",
            Name          = "More Rage",
            Description   = "Increases rage generation from auto-attacks by 2% per rank.",
            Class         = CharacterClass.Warrior,
            TreeIndex     = 1,
            Row           = 0,
            Column        = 0,
            MaxRanks      = 5,
            EffectType    = TalentEffectType.RageGenBonus,
            EffectPerRank = 0.02,
            IconUrl       = "https://wow.zamimg.com/images/wow/icons/medium/spell_shadow_unholyfrenzy.jpg",
        },
        new TalentDefinition
        {
            Id            = "warrior_fury_heroic_striker",
            Name          = "Heroic Striker",
            Description   = "Increases Heroic Strike damage by 5% per rank.",
            Class         = CharacterClass.Warrior,
            TreeIndex     = 1,
            Row           = 0,
            Column        = 1,
            MaxRanks      = 2,
            EffectType    = TalentEffectType.HeroicStrikeDamage,
            EffectPerRank = 0.05,
            IconUrl       = "https://wow.zamimg.com/images/wow/icons/medium/ability_rogue_ambush.jpg",
        },
        new TalentDefinition
        {
            Id            = "warrior_fury_excecuter",
            Name          = "Excecuter",
            Description   = "Reduces the cooldown of Execute by 2 seconds per rank.",
            Class         = CharacterClass.Warrior,
            TreeIndex     = 1,
            Row           = 0,
            Column        = 2,
            MaxRanks      = 3,
            EffectType    = TalentEffectType.ExecuteCooldown,
            EffectPerRank = 2.0,
            IconUrl       = "https://wow.zamimg.com/images/wow/icons/medium/inv_sword_48.jpg",
        },

        // ── Fury-Baum (1), Reihe 1 ────────────────────────────────────────────
        new TalentDefinition
        {
            Id              = "warrior_fury_berserker_stance",
            Name            = "Berserker Stance",
            Description     = "Grants the ability Berserker Stance. Increases total damage output by 5% and critical strike chance by 3%.",
            Class           = CharacterClass.Warrior,
            TreeIndex       = 1,
            Row             = 1,
            Column          = 0,
            MaxRanks        = 1,
            EffectType      = TalentEffectType.GrantBerserkerStance,
            EffectPerRank   = 1.0,
            GrantsAbilityId = "Berserker_Stance",
            IconUrl         = "https://wow.zamimg.com/images/wow/icons/medium/ability_racial_avatar.jpg",
        },
        new TalentDefinition
        {
            Id            = "warrior_fury_imp_heroic_strike",
            Name          = "Improved Heroic Strike",
            Description   = "Reduces the rage cost of Heroic Strike by 1 per rank.",
            Class         = CharacterClass.Warrior,
            TreeIndex     = 1,
            Row           = 1,
            Column        = 1,
            MaxRanks      = 3,
            EffectType    = TalentEffectType.HeroicStrikeManaCost,
            EffectPerRank = 1.0,
            IconUrl       = "https://wow.zamimg.com/images/wow/icons/medium/ability_warrior_innerrage.jpg",
        },

        // ── Defence-Baum (2), Reihe 0 ─────────────────────────────────────────
        new TalentDefinition
        {
            Id            = "warrior_def_shield_mastery",
            Name          = "Shield Mastery",
            Description   = "Reduces damage taken by 1% per rank.",
            Class         = CharacterClass.Warrior,
            TreeIndex     = 2,
            Row           = 0,
            Column        = 0,
            MaxRanks      = 5,
            EffectType    = TalentEffectType.DamageTaken,
            EffectPerRank = 0.01,
            IconUrl       = "https://wow.zamimg.com/images/wow/icons/medium/inv_shield_06.jpg",
        },
        new TalentDefinition
        {
            Id            = "warrior_def_imp_thunderclap",
            Name          = "Improved Thunderclap",
            Description   = "Increases the damage and attack speed reduction of Thunder Clap by 2% per rank.",
            Class         = CharacterClass.Warrior,
            TreeIndex     = 2,
            Row           = 0,
            Column        = 1,
            MaxRanks      = 5,
            EffectType    = TalentEffectType.ThunderclapBonus,
            EffectPerRank = 0.02,
            IconUrl       = "https://wow.zamimg.com/images/wow/icons/medium/spell_nature_thunderclap.jpg",
        },

        // ── Defence-Baum (2), Reihe 1 ─────────────────────────────────────────
        new TalentDefinition
        {
            Id              = "warrior_def_defensive_stance",
            Name            = "Defensive Stance",
            Description     = "Grants the ability Defensive Stance. Reduces damage taken by 10%.",
            Class           = CharacterClass.Warrior,
            TreeIndex       = 2,
            Row             = 1,
            Column          = 0,
            MaxRanks        = 1,
            EffectType      = TalentEffectType.GrantDefensiveStance,
            EffectPerRank   = 1.0,
            GrantsAbilityId = "Defensive_Stance",
            IconUrl         = "https://wow.zamimg.com/images/wow/icons/medium/ability_warrior_defensivestance.jpg",
        },
        new TalentDefinition
        {
            Id            = "warrior_def_defensive_rage",
            Name          = "Defensive Rage",
            Description   = "Gain 1 rage whenever you are attacked, per rank.",
            Class         = CharacterClass.Warrior,
            TreeIndex     = 2,
            Row           = 1,
            Column        = 1,
            MaxRanks      = 3,
            EffectType    = TalentEffectType.RageOnHit,
            EffectPerRank = 1.0,
            IconUrl       = "https://wow.zamimg.com/images/wow/icons/medium/ability_warrior_challange.jpg",
        },
    ];
}
