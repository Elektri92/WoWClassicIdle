// Models/TalentRegistry.cs
namespace Models;

public static partial class TalentRegistry
{
    private static readonly Dictionary<CharacterClass, List<TalentDefinition>> _talents;

    static TalentRegistry()
    {
        _talents = new Dictionary<CharacterClass, List<TalentDefinition>>();
        foreach (CharacterClass cls in Enum.GetValues<CharacterClass>())
            _talents[cls] = BuildTalents(cls);
    }

    public static IReadOnlyList<TalentDefinition> GetTalents(CharacterClass cls) =>
        _talents.TryGetValue(cls, out var list) ? list : Array.Empty<TalentDefinition>();

    public static TalentDefinition? GetById(CharacterClass cls, string id) =>
        GetTalents(cls).FirstOrDefault(t => t.Id == id);

    public static string[] GetTreeNames(CharacterClass cls) => cls switch
    {
        CharacterClass.Warrior  => new[] { "Arms", "Fury", "Protection" },
        CharacterClass.Mage     => new[] { "Arcane", "Fire", "Frost" },
        CharacterClass.Priest   => new[] { "Discipline", "Holy", "Shadow" },
        CharacterClass.Rogue    => new[] { "Assassination", "Combat", "Subtlety" },
        CharacterClass.Paladin  => new[] { "Holy", "Protection", "Retribution" },
        CharacterClass.Hunter   => new[] { "Beast Mastery", "Marksmanship", "Survival" },
        CharacterClass.Warlock  => new[] { "Affliction", "Demonology", "Destruction" },
        CharacterClass.Shaman   => new[] { "Elemental", "Enhancement", "Restoration" },
        CharacterClass.Druid    => new[] { "Balance", "Feral Combat", "Restoration" },
        _ => new[] { "Tree 1", "Tree 2", "Tree 3" },
    };

    private static List<TalentDefinition> BuildTalents(CharacterClass cls) => cls switch
    {
        CharacterClass.Mage     => BuildMageTalents(),
        CharacterClass.Warrior  => BuildWarriorTalents(),
        CharacterClass.Priest   => BuildPriestTalents(),
        CharacterClass.Rogue    => BuildRogueTalents(),
        CharacterClass.Paladin  => BuildPaladinTalents(),
        CharacterClass.Hunter   => BuildHunterTalents(),
        CharacterClass.Warlock  => BuildWarlockTalents(),
        CharacterClass.Shaman   => BuildShamanTalents(),
        CharacterClass.Druid    => BuildDruidTalents(),
        _ => [],
    };

    private static List<TalentDefinition> BuildMageTalents() =>
    [
        // ── Arcane-Baum (0), Reihe 0 ──────────────────────────────────────────
        new TalentDefinition
        {
            Id            = "mage_arcane_improved_ai",
            Name          = "Improved Arcane Intellect",
            Description   = "Increases the effect of Arcane Intellect by 5% per rank.",
            Class         = CharacterClass.Mage,
            TreeIndex     = 0,
            Row           = 0,
            Column        = 0,
            MaxRanks      = 5,
            EffectType    = TalentEffectType.ArcaneIntellectBonus,
            EffectPerRank = 0.05,
            IconUrl       = "https://wow.zamimg.com/images/wow/icons/medium/spell_holy_magicalsentry.jpg",
        },
        new TalentDefinition
        {
            Id            = "mage_arcane_improved_cw",
            Name          = "Improved Conjure Water",
            Description   = "Increases the mana restore value of Conjure Water by 3% per rank.",
            Class         = CharacterClass.Mage,
            TreeIndex     = 0,
            Row           = 0,
            Column        = 1,
            MaxRanks      = 2,
            EffectType    = TalentEffectType.ConjureWaterManaRegen,
            EffectPerRank = 0.03,
            IconUrl       = "https://wow.zamimg.com/images/wow/icons/medium/inv_drink_10.jpg",
        },
        new TalentDefinition
        {
            Id            = "mage_arcane_improved_wand",
            Name          = "Improved Wand",
            Description   = "Increases auto-attack damage by 5% per rank.",
            Class         = CharacterClass.Mage,
            TreeIndex     = 0,
            Row           = 0,
            Column        = 2,
            MaxRanks      = 3,
            EffectType    = TalentEffectType.AutoAttackDamage,
            EffectPerRank = 0.05,
            IconUrl       = "https://wow.zamimg.com/images/wow/icons/medium/inv_wand_01.jpg",
        },

        // ── Fire-Baum (1), Reihe 0 ─────────────────────────────────────────────
        new TalentDefinition
        {
            Id            = "mage_fire_improved_fireball",
            Name          = "Improved Fireball",
            Description   = "Reduces the mana cost of Fireball by 1 per rank.",
            Class         = CharacterClass.Mage,
            TreeIndex     = 1,
            Row           = 0,
            Column        = 0,
            MaxRanks      = 5,
            EffectType    = TalentEffectType.FireballManaCost,
            EffectPerRank = 1.0,
            IconUrl       = "https://wow.zamimg.com/images/wow/icons/medium/spell_fire_flamebolt.jpg",
        },
        new TalentDefinition
        {
            Id            = "mage_fire_periodic_fire",
            Name          = "Periodic Fire",
            Description   = "Fireball leaves a DoT on the enemy dealing 1 DMG over 5 seconds per rank.",
            Class         = CharacterClass.Mage,
            TreeIndex     = 1,
            Row           = 0,
            Column        = 1,
            MaxRanks      = 2,
            EffectType    = TalentEffectType.FireballDot,
            EffectPerRank = 1.0,
            IconUrl       = "https://wow.zamimg.com/images/wow/icons/medium/spell_fire_immolation.jpg",
        },
        new TalentDefinition
        {
            Id            = "mage_fire_critical_fire",
            Name          = "Critical Fire",
            Description   = "Increases the critical strike chance of Fireball by 1% per rank.",
            Class         = CharacterClass.Mage,
            TreeIndex     = 1,
            Row           = 0,
            Column        = 2,
            MaxRanks      = 3,
            EffectType    = TalentEffectType.FireballCrit,
            EffectPerRank = 0.01,
            IconUrl       = "https://wow.zamimg.com/images/wow/icons/medium/spell_fire_fireball02.jpg",
        },

        // ── Frost-Baum (2), Reihe 0 ────────────────────────────────────────────
        new TalentDefinition
        {
            Id            = "mage_frost_improved_frostbolt",
            Name          = "Improved Frostbolt",
            Description   = "Reduces the mana cost of Frostbolt by 1 per rank.",
            Class         = CharacterClass.Mage,
            TreeIndex     = 2,
            Row           = 0,
            Column        = 0,
            MaxRanks      = 5,
            EffectType    = TalentEffectType.FrostboltManaCost,
            EffectPerRank = 1.0,
            IconUrl       = "https://wow.zamimg.com/images/wow/icons/medium/spell_frost_frostbolt02.jpg",
        },
        new TalentDefinition
        {
            Id            = "mage_frost_winters_chill",
            Name          = "Winter's Chill",
            Description   = "Increases the attack speed debuff of Frostbolt by an additional 5% per rank.",
            Class         = CharacterClass.Mage,
            TreeIndex     = 2,
            Row           = 0,
            Column        = 1,
            MaxRanks      = 2,
            EffectType    = TalentEffectType.FrostboltAttackSpeedDebuff,
            EffectPerRank = 0.05,
            IconUrl       = "https://wow.zamimg.com/images/wow/icons/medium/spell_frost_chillingblast.jpg",
        },
        new TalentDefinition
        {
            Id            = "mage_frost_critical_frost",
            Name          = "Critical Frost",
            Description   = "Increases the critical strike chance of Frostbolt by 1% per rank.",
            Class         = CharacterClass.Mage,
            TreeIndex     = 2,
            Row           = 0,
            Column        = 2,
            MaxRanks      = 3,
            EffectType    = TalentEffectType.FrostboltCrit,
            EffectPerRank = 0.01,
            IconUrl       = "https://wow.zamimg.com/images/wow/icons/medium/spell_frost_frozencore.jpg",
        },
    ];
}
