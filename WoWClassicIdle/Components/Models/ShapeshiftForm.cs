// Models/ShapeshiftForm.cs
namespace Models;

/// <summary>Gestalt (Druide, Shadowform des Priesters, Warrior-Haltungen) – aktiv, solange die zugehörige Form-Fähigkeit ausgerüstet ist.</summary>
public enum DruidForm { None, Moonkin, Bear, Cat, Tree, Shadow, BattleStance, BerserkerStance, DefensiveStance }

/// <summary>Ressource, die die Fähigkeiten des Charakters verbrauchen.</summary>
public enum ResourceKind { Mana, Rage, Energy }

/// <summary>Statische Werte einer Gestalt (Ressource, Portrait, passive Boni).</summary>
public sealed record ShapeshiftFormInfo
{
    public DruidForm Form { get; init; }
    public string Name { get; init; } = "";

    /// <summary>Ersetzt das Charakter-Portrait, solange die Form ausgerüstet ist.</summary>
    public string PortraitUrl { get; init; } = "";

    public ResourceKind Resource { get; init; } = ResourceKind.Mana;

    /// <summary>Weniger erlittener Schaden (0.20 = −20 %).</summary>
    public double DamageTakenReduction { get; init; }

    /// <summary>Mehr verursachte Heilung (0.20 = +20 %).</summary>
    public double HealingBonus { get; init; }

    /// <summary>Mehr Manaregeneration aus Spirit (0.50 = +50 %).</summary>
    public double SpiritManaRegenBonus { get; init; }

    /// <summary>Schadensbonus (Direkt + DoT) für die Zauber in <see cref="BoostedAbilityNames"/>.</summary>
    public double BoostedAbilityDamageBonus { get; init; }
    public IReadOnlyList<string> BoostedAbilityNames { get; init; } = [];

    /// <summary>
    /// Haltung (Warrior): kein echter Gestaltwechsel – Ressource, Portrait und Stats bleiben, alle Fähigkeiten bleiben nutzbar.
    /// Dient nur als Voraussetzung für <see cref="Ability.RequiredForm"/> (Overpower → Battle Stance); die Boni kommen aus dem Aura-Buff.
    /// </summary>
    public bool IsStance { get; init; }
}

/// <summary>Gestalt-Daten und Regeln, welche Fähigkeiten in welcher Gestalt nutzbar sind.</summary>
public static class ShapeshiftForms
{
    /// <summary>Größe des Rage-/Energie-Pools (wie Warrior/Rogue).</summary>
    public const int NonManaPoolSize = 100;

    private static readonly Dictionary<DruidForm, ShapeshiftFormInfo> Forms = new()
    {
        [DruidForm.Moonkin] = new ShapeshiftFormInfo
        {
            Form                      = DruidForm.Moonkin,
            Name                      = "Moonkin Form",
            PortraitUrl               = "https://wow.zamimg.com/images/wow/icons/large/spell_nature_forceofnature.jpg",
            BoostedAbilityDamageBonus = 0.15,
            // Starfire ergänzt – ab Lv 20 ersetzt Starfire Wrath im Loadout, Moonkin brachte nur noch +2,4 %
            BoostedAbilityNames       = ["Wrath", "Starfire", "Moonfire"],
        },
        [DruidForm.Bear] = new ShapeshiftFormInfo
        {
            Form                 = DruidForm.Bear,
            Name                 = "Bear Form",
            PortraitUrl          = "https://wow.zamimg.com/images/wow/icons/large/ability_racial_bearform.jpg",
            Resource             = ResourceKind.Rage,
            DamageTakenReduction = 0.06,   // war 0.20 – Bear lag Lv 20 bei +21 % XP/h
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
            // Tree Form brachte im Sim nur +1,8 % – Resto-Druiden kämpfen mit Naturzaubern.
            // Insect Swarm entfernt: mit beiden DoTs lag Resto Lv 20 bei +34 % XP/h (vor Balance)
            BoostedAbilityDamageBonus = 0.10,
            BoostedAbilityNames       = ["Wrath", "Moonfire"],
        },
        [DruidForm.Shadow] = new ShapeshiftFormInfo
        {
            Form                      = DruidForm.Shadow,
            Name                      = "Shadowform",
            PortraitUrl               = "https://wow.zamimg.com/images/wow/icons/large/spell_shadow_shadowform.jpg",
            DamageTakenReduction      = 0.15,
            // war +15 % nur Mind Blast/SW:P – bis Mind Blast (Lv 18) wirkungslos, Shadow lag bei −24 bis −31 % XP/h
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

    /// <summary>Erste ausgerüstete Form-Fähigkeit bestimmt die Gestalt (es kann nur eine geben).</summary>
    public static DruidForm FormOf(IEnumerable<Ability> equipped) =>
        equipped.FirstOrDefault(a => a.IsForm)?.Form ?? DruidForm.None;

    public static DruidForm FormOf(ClassLoadout loadout) =>
        FormOf(ResolveAbilities(loadout));

    /// <summary>
    /// Form-gebundene Fähigkeiten (Maul, Claw) nur in ihrer Gestalt; in Rage-/Energie-Gestalten
    /// keine Fähigkeiten, die Mana kosten – außer sie haben dort eigene <see cref="Ability.FormCosts"/>.
    /// </summary>
    public static bool IsUsableIn(Ability ability, DruidForm form)
    {
        if (ability.RequiredForm != DruidForm.None) return ability.RequiredForm == form;
        if (ability.FormCosts.ContainsKey(form)) return true;
        if (form == DruidForm.None || ability.IsForm || ability.ManaCost == 0) return true;
        return Forms[form].IsStance || Forms[form].Resource == ResourceKind.Mana;
    }

    /// <summary>Entfernt alle Fähigkeiten, die in der ausgerüsteten Gestalt nicht nutzbar sind. Gibt sie zurück.</summary>
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
