// Models/ShapeshiftForm.cs
namespace Models;

/// <summary>Form (Druid, Priest Shadowform, Warrior stances) – active as long as the matching form ability is equipped.</summary>
public enum DruidForm { None, Moonkin, Bear, Cat, Tree, Shadow, BattleStance, BerserkerStance, DefensiveStance }

/// <summary>Resource the character's abilities consume.</summary>
public enum ResourceKind { Mana, Rage, Energy }

/// <summary>Static values of a form (resource, portrait, passive bonuses).</summary>
public sealed record ShapeshiftFormInfo
{
    public DruidForm Form { get; init; }
    public string Name { get; init; } = "";

    /// <summary>Replaces the character portrait as long as the form is equipped.</summary>
    public string PortraitUrl { get; init; } = "";

    public ResourceKind Resource { get; init; } = ResourceKind.Mana;

    /// <summary>Less damage taken (0.20 = −20 %).</summary>
    public double DamageTakenReduction { get; init; }

    /// <summary>More healing done (0.20 = +20 %).</summary>
    public double HealingBonus { get; init; }

    /// <summary>More mana regeneration from Spirit (0.50 = +50 %).</summary>
    public double SpiritManaRegenBonus { get; init; }

    /// <summary>Damage bonus (direct + DoT) for the spells in <see cref="BoostedAbilityNames"/>.</summary>
    public double BoostedAbilityDamageBonus { get; init; }
    public IReadOnlyList<string> BoostedAbilityNames { get; init; } = [];

    /// <summary>
    /// Stance (Warrior): no real shapeshift – resource, portrait and stats stay, all abilities remain usable.
    /// Only serves as a prerequisite for <see cref="Ability.RequiredForm"/> (Overpower → Battle Stance); the bonuses come from the aura buff.
    /// </summary>
    public bool IsStance { get; init; }
}

/// <summary>Form data and rules for which abilities are usable in which form.</summary>
public static class ShapeshiftForms
{
    /// <summary>Size of the rage/energy pool (like Warrior/Rogue).</summary>
    public const int NonManaPoolSize = 100;

    private static readonly Dictionary<DruidForm, ShapeshiftFormInfo> Forms = new()
    {
        [DruidForm.Moonkin] = new ShapeshiftFormInfo
        {
            Form                      = DruidForm.Moonkin,
            Name                      = "Moonkin Form",
            PortraitUrl               = "https://wow.zamimg.com/images/wow/icons/large/spell_nature_forceofnature.jpg",
            BoostedAbilityDamageBonus = 0.15,
            // Starfire added – from Lv 20 Starfire replaces Wrath in the loadout, Moonkin only gave +2.4 %
            BoostedAbilityNames       = ["Wrath", "Starfire", "Moonfire"],
        },
        [DruidForm.Bear] = new ShapeshiftFormInfo
        {
            Form                 = DruidForm.Bear,
            Name                 = "Bear Form",
            PortraitUrl          = "https://wow.zamimg.com/images/wow/icons/large/ability_racial_bearform.jpg",
            Resource             = ResourceKind.Rage,
            DamageTakenReduction = 0.06,   // was 0.20 – Bear was at +21 % XP/h at Lv 20
        },
        [DruidForm.Cat] = new ShapeshiftFormInfo
        {
            Form        = DruidForm.Cat,
            Name        = "Cat Form",
            PortraitUrl = "https://wow.zamimg.com/images/wow/icons/large/ability_druid_catform.jpg",
            Resource    = ResourceKind.Energy,
        },
        [DruidForm.Tree] = new ShapeshiftFormInfo
        {
            Form                 = DruidForm.Tree,
            Name                 = "Tree Form",
            PortraitUrl          = "https://wow.zamimg.com/images/wow/icons/large/ability_druid_treeoflife.jpg",
            HealingBonus         = 0.20,
            SpiritManaRegenBonus = 0.50,
            // Tree Form only gave +1.8 % in the sim – Resto Druids fight with nature spells.
            // Insect Swarm removed: with both DoTs Resto was at +34 % XP/h at Lv 20 (before balancing)
            BoostedAbilityDamageBonus = 0.10,
            BoostedAbilityNames       = ["Wrath", "Moonfire"],
        },
        [DruidForm.Shadow] = new ShapeshiftFormInfo
        {
            Form                      = DruidForm.Shadow,
            Name                      = "Shadowform",
            PortraitUrl               = "https://wow.zamimg.com/images/wow/icons/large/spell_shadow_shadowform.jpg",
            DamageTakenReduction      = 0.15,
            // was +15 % on Mind Blast/SW:P only – useless until Mind Blast (Lv 18), Shadow was at −24 to −31 % XP/h
            BoostedAbilityDamageBonus = 0.25,
            BoostedAbilityNames       = ["Smite", "Mind Blast", "Shadow Word: Pain"],
        },
        [DruidForm.BattleStance]    = new ShapeshiftFormInfo { Form = DruidForm.BattleStance,    Name = "Battle Stance",    IsStance = true },
        [DruidForm.BerserkerStance] = new ShapeshiftFormInfo { Form = DruidForm.BerserkerStance, Name = "Berserker Stance", IsStance = true },
        [DruidForm.DefensiveStance] = new ShapeshiftFormInfo { Form = DruidForm.DefensiveStance, Name = "Defensive Stance", IsStance = true },
    };

    public static ShapeshiftFormInfo? Get(DruidForm form) => Forms.GetValueOrDefault(form);

    public static ResourceKind ResourceFor(CharacterClass cls, DruidForm form) =>
        Get(form) is { IsStance: false } info ? info.Resource : cls switch
        {
            CharacterClass.Warrior => ResourceKind.Rage,
            CharacterClass.Rogue   => ResourceKind.Energy,
            _                      => ResourceKind.Mana,
        };

    public static string ResourceName(ResourceKind kind) => kind switch
    {
        ResourceKind.Rage   => "Rage",
        ResourceKind.Energy => "Energy",
        _                   => "Mana",
    };

    /// <summary>The first equipped form ability determines the form (there can be only one).</summary>
    public static DruidForm FormOf(IEnumerable<Ability> equipped) =>
        equipped.FirstOrDefault(a => a.IsForm)?.Form ?? DruidForm.None;

    public static DruidForm FormOf(ClassLoadout loadout) =>
        FormOf(ResolveAbilities(loadout));

    /// <summary>
    /// Form-bound abilities (Maul, Claw) only in their form; in rage/energy forms
    /// no abilities that cost mana – unless they have their own <see cref="Ability.FormCosts"/> there.
    /// </summary>
    public static bool IsUsableIn(Ability ability, DruidForm form)
    {
        if (ability.RequiredForm != DruidForm.None) return ability.RequiredForm == form;
        if (ability.FormCosts.ContainsKey(form)) return true;
        if (form == DruidForm.None || ability.IsForm || ability.ManaCost == 0) return true;
        return Forms[form].IsStance || Forms[form].Resource == ResourceKind.Mana;
    }

    /// <summary>Removes all abilities not usable in the equipped form. Returns them.</summary>
    public static List<Ability> PurgeIncompatible(ClassLoadout loadout)
    {
        var equipped = ResolveAbilities(loadout);
        var form     = FormOf(equipped);
        var removed  = equipped.Where(a => !IsUsableIn(a, form)).ToList();
        if (removed.Count > 0)
            loadout.EquippedAbilityIds.RemoveAll(id => removed.Any(a => a.Id == id));
        return removed;
    }

    private static List<Ability> ResolveAbilities(ClassLoadout loadout)
    {
        var pool = ClassAbilityRegistry.GetAbilitiesFor(loadout.Class);
        return loadout.EquippedAbilityIds
            .Select(id => pool.FirstOrDefault(a => a.Id == id))
            .OfType<Ability>()
            .ToList();
    }
}
