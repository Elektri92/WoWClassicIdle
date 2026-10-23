// Models/ClassAbilityRegistry.cs
namespace Models;

// Neue Klassen als eigene Partial-Datei anlegen; Dateien möglichst < 300 Zeilen halten.
// Pro Klasse eine Datei: ClassAbilityRegistry.Warrior.cs, .Mage.cs usw.

/// <summary>
/// Rank-1-Werte aus WoW Classic (1.12).
/// Schaden/Heilung = Durchschnitt der Tooltip-Spanne, Kosten = Mana/Rage/Energy.
/// Cooldown: echter CD falls vorhanden, sonst Cast-Zeit bzw. GCD (Idle-Taktung).
/// </summary>
public static partial class ClassAbilityRegistry
{
    public const int MaxSlotsPerClass = 6;

    public static IReadOnlyDictionary<CharacterClass, IReadOnlyList<Ability>> Abilities { get; }
        = new Dictionary<CharacterClass, IReadOnlyList<Ability>>
    {
        [CharacterClass.Warrior] = WarriorAbilities,
        [CharacterClass.Mage]    = MageAbilities,
        [CharacterClass.Priest]  = PriestAbilities,
        [CharacterClass.Rogue]   = RogueAbilities,
        [CharacterClass.Paladin] = PaladinAbilities,
        [CharacterClass.Hunter]  = HunterAbilities,
        [CharacterClass.Warlock] = WarlockAbilities,
        [CharacterClass.Shaman]  = ShamanAbilities,
        [CharacterClass.Druid]   = DruidAbilities,
    };

    public static IReadOnlyList<Ability> GetAbilitiesFor(CharacterClass cls)
        => Abilities.TryGetValue(cls, out var list) ? list : Array.Empty<Ability>();

    /// <summary>
    /// Wählt den optimalen Standard-Loadout für eine Klasse auf einem bestimmten Level:
    /// pro Ability-Familie (gleicher Name) nur der höchste verfügbare Rank,
    /// bei mehr Familien als Slots werden die zuletzt freigeschalteten bevorzugt.
    /// </summary>
    public static List<string> GetDefaultLoadoutIds(CharacterClass cls, int level) =>
        GetAbilitiesFor(cls)
            .Where(a => a.LevelRequired <= level)
            .Where(a => !a.TalentUnlockOnly)
            .GroupBy(a => a.Name)
            .Select(g => g.OrderByDescending(a => a.Rank).First())
            // Pro ExclusiveGroup nur die zuletzt freigeschaltete Ability behalten
            .GroupBy(a => string.IsNullOrEmpty(a.ExclusiveGroup) ? a.Id : a.ExclusiveGroup)
            .Select(g => g.OrderByDescending(a => a.LevelRequired).First())
            .OrderByDescending(a => a.LevelRequired)
            .Take(MaxSlotsPerClass)
            .Select(a => a.Id)
            .ToList();

    public static Ability? FindById(string id)
    {
        foreach (var list in Abilities.Values)
            if (list.Any(a => a.Id == id))
                return list.First(a => a.Id == id);
        return null;
    }

    /// <summary>Sucht eine Fähigkeit der Klasse per Anzeigename (z. B. für Damage-Meter-Zeilen).</summary>
    public static Ability? FindByName(CharacterClass cls, string name)
        => GetAbilitiesFor(cls).FirstOrDefault(a => a.Name == name);

    /// <summary>Klasse, zu der die Fähigkeit gehört.</summary>
    public static CharacterClass? ClassOf(Ability ability)
    {
        foreach (var (cls, list) in Abilities)
            if (list.Any(a => a.Id == ability.Id))
                return cls;
        return null;
    }
}
