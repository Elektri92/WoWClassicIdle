// Models/TalentRegistry.cs
namespace Models;

public static partial class TalentRegistry
{
    private static readonly Dictionary<CharacterClass, List<TalentDefinition>> _talents;

    static TalentRegistry()
    {
        _talents = new Dictionary<CharacterClass, List<TalentDefinition>>();
        foreach (CharacterClass cls in Enum.GetValues<CharacterClass>())
            _talents[cls] = BuildTalents(cls).Concat(BuildSpecAbilityTalents(cls)).ToList();
    }

    public static IReadOnlyList<TalentDefinition> GetTalents(CharacterClass cls) =>
        _talents.TryGetValue(cls, out var list) ? list : Array.Empty<TalentDefinition>();

    /// <summary>Talents that unlock the ability <paramref name="abilityId"/> (empty = not a talent ability).</summary>
    public static IEnumerable<TalentDefinition> GetGrantingTalents(CharacterClass cls, string abilityId) =>
        GetTalents(cls).Where(t => t.GrantedAbilityIds.Contains(abilityId));

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

    // Themed background per spec (wwwroot/images/talents/{class}-{tree}.jpg)
    public static string GetTreeBackgroundUrl(CharacterClass cls, int treeIndex)
    {
        var tree = GetTreeNames(cls)[treeIndex].ToLowerInvariant().Replace(' ', '-');
        return $"images/talents/{cls.ToString().ToLowerInvariant()}-{tree}.jpg";
    }

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
            Name          = "Arcane Mind",
            Description   = "Increases your critical strike chance by 1% per rank.",
            Class         = CharacterClass.Mage,
            TreeIndex     = 0,
            Row           = 1,
            Column        = 1,
            MaxRanks      = 5,
            EffectType    = TalentEffectType.CritChance,
            EffectPerRank = 0.01,
            IconUrl       = "https://wow.zamimg.com/images/wow/icons/medium/spell_holy_magicalsentry.jpg",
        },
        new TalentDefinition
        {
            Id            = "mage_arcane_improved_cw",
            Name          = "Arcane Instability",
            Description   = "Increases the damage of all your abilities by 3% per rank.",
            Class         = CharacterClass.Mage,
            TreeIndex     = 0,
            Row           = 0,
            Column        = 1,
            MaxRanks      = 2,
            EffectType    = TalentEffectType.DamageBonus,
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
            Description   = "Increases the damage of Fireball by 5% per rank.",
            Class         = CharacterClass.Mage,
            TreeIndex     = 1,
            Row           = 0,
            Column        = 0,
            MaxRanks      = 5,
            EffectType    = TalentEffectType.FireballDamage,
            EffectPerRank = 0.05,
            IconUrl       = "https://wow.zamimg.com/images/wow/icons/medium/spell_fire_flamebolt.jpg",
        },
        new TalentDefinition
        {
            Id            = "mage_fire_periodic_fire",
            Name          = "Periodic Fire",
            Description   = "Fireball leaves a DoT on the enemy dealing 10% of its damage over 5 seconds per rank.",
            Class         = CharacterClass.Mage,
            TreeIndex     = 1,
            Row           = 0,
            Column        = 1,
            MaxRanks      = 2,
            EffectType    = TalentEffectType.FireballDot,
            EffectPerRank = 0.10,
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
            Description   = "Increases the damage of Frostbolt by 3% per rank.",
            Class         = CharacterClass.Mage,
            TreeIndex     = 2,
            Row           = 0,
            Column        = 0,
            MaxRanks      = 5,
            EffectType    = TalentEffectType.FrostboltDamage,
            EffectPerRank = 0.03,
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

        // ── Arcane-Baum (0), Reihe 1 ──────────────────────────────────────────
        new TalentDefinition
        {
            Id              = "mage_arcane_evocation",
            Name            = "Evocation",
            Description     = "Teaches you the Evocation ability: restores 80% of your mana over 8 seconds and increases all damage you deal by 20% during that time.",
            Class           = CharacterClass.Mage,
            TreeIndex       = 0,
            Row             = 1,
            Column          = 0,
            MaxRanks        = 1,
            EffectType      = TalentEffectType.GrantEvocation,
            EffectPerRank   = 1.0,
            GrantsAbilityId = "Evocation_r1",
            IconUrl         = "https://wow.zamimg.com/images/wow/icons/medium/spell_nature_purge.jpg",
        },
        new TalentDefinition
        {
            Id            = "mage_arcane_improved_am",
            Name          = "Improved Arcane Missiles",
            Description   = "Increases the damage of Arcane Missiles by 4% per rank.",
            Class         = CharacterClass.Mage,
            TreeIndex     = 0,
            Row           = 0,
            Column        = 0,
            MaxRanks      = 3,
            EffectType    = TalentEffectType.ArcaneMissilesDamage,
            EffectPerRank = 0.04,
            IconUrl       = "https://wow.zamimg.com/images/wow/icons/medium/spell_nature_starfall.jpg",
        },

        // ── Fire-Baum (1), Reihe 1 ─────────────────────────────────────────────
        new TalentDefinition
        {
            Id              = "mage_fire_ignite",
            Name            = "Ignite",
            Description     = "Your fire spell crits apply Ignite, dealing 40% of the crit damage as a DoT over 3 seconds.",
            Class           = CharacterClass.Mage,
            TreeIndex       = 1,
            Row             = 1,
            Column          = 0,
            MaxRanks        = 1,
            EffectType      = TalentEffectType.GrantIgnite,
            EffectPerRank   = 1.0,
            GrantsAbilityId = "Ignite_r1",
            IconUrl         = "https://wow.zamimg.com/images/wow/icons/medium/spell_fire_incinerate.jpg",
        },
        new TalentDefinition
        {
            Id            = "mage_fire_efficient_fb",
            Name          = "Improved Fire Blast",
            Description   = "Increases the damage of Fire Blast by 6% per rank.",
            Class         = CharacterClass.Mage,
            TreeIndex     = 1,
            Row           = 1,
            Column        = 1,
            MaxRanks      = 3,
            EffectType    = TalentEffectType.FireBlastDamage,
            EffectPerRank = 0.06,
            IconUrl       = "https://wow.zamimg.com/images/wow/icons/medium/spell_fire_fireball.jpg",
        },

        // ── Frost-Baum (2), Reihe 1 ────────────────────────────────────────────
        new TalentDefinition
        {
            Id              = "mage_frost_ice_barrier",
            Name            = "Ice Barrier",
            Description     = "Teaches you the Ice Barrier ability, which absorbs 30% of your max HP as a shield for 60 seconds.",
            Class           = CharacterClass.Mage,
            TreeIndex       = 2,
            Row             = 1,
            Column          = 0,
            MaxRanks        = 1,
            EffectType      = TalentEffectType.GrantIceBarrier,
            EffectPerRank   = 1.0,
            GrantsAbilityId = "Ice_Barrier_r1",
            IconUrl         = "https://wow.zamimg.com/images/wow/icons/medium/spell_ice_lament.jpg",
        },
        new TalentDefinition
        {
            Id            = "mage_frost_frostbolt_crusher",
            Name          = "Frostbolt Crusher",
            Description   = "Increases the damage of Frostbolt by 10% per rank.",
            Class         = CharacterClass.Mage,
            TreeIndex     = 2,
            Row           = 1,
            Column        = 1,
            MaxRanks      = 3,
            EffectType    = TalentEffectType.FrostboltDamage,
            EffectPerRank = 0.10,
            IconUrl       = "https://wow.zamimg.com/images/wow/icons/medium/spell_frost_frostbolt02.jpg",
        },
    ];
}
