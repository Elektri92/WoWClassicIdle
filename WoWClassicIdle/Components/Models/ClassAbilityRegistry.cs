// Models/ClassAbilityRegistry.cs
namespace Models;

// Create new classes as their own partial file; keep files below 300 lines where possible.
// One file per class: ClassAbilityRegistry.Warrior.cs, .Mage.cs etc.

/// <summary>
/// Rank 1 values from WoW Classic (1.12).
/// Damage/healing = average of the tooltip range, cost = Mana/Rage/Energy.
/// Cooldown: real CD if present, otherwise cast time or GCD (idle pacing).
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
    /// Picks the optimal default loadout for a class at a given level:
    /// per ability family (same name) only the highest available rank,
    /// with more families than slots the most recently unlocked are preferred.
    /// </summary>
    public static List<string> GetDefaultLoadoutIds(CharacterClass cls, int level) =>
        GetAbilitiesFor(cls)
            .Where(a => a.LevelRequired <= level)
            .Where(a => !a.TalentUnlockOnly)
            .GroupBy(a => a.Name)
            .Select(g => g.OrderByDescending(a => a.Rank).First())
            // Per ExclusiveGroup keep only the most recently unlocked ability
            .GroupBy(a => string.IsNullOrEmpty(a.ExclusiveGroup) ? a.Id : a.ExclusiveGroup)
            .Select(g => g.OrderByDescending(a => a.LevelRequired).First())
            .OrderByDescending(a => a.LevelRequired)
            .Take(MaxSlotsPerClass)
            .Select(a => a.Id)
            .ToList();

    /// <summary>
    /// Spec-aware default loadout: abilities with a matching <see cref="Ability.SpecHint"/> are
    /// preferred within an ExclusiveGroup; otherwise identical to <see cref="GetDefaultLoadoutIds(CharacterClass,int)"/>.
    /// </summary>
    public static List<string> GetDefaultLoadoutIds(CharacterClass cls, int level, int specIndex) =>
        GetAbilitiesFor(cls)
            .Where(a => a.LevelRequired <= level)
            .Where(a => !a.TalentUnlockOnly)
            .GroupBy(a => a.Name)
            .Select(g => g.OrderByDescending(a => a.Rank).First())
            .GroupBy(a => string.IsNullOrEmpty(a.ExclusiveGroup) ? a.Id : a.ExclusiveGroup)
            .Select(g =>
            {
                // Prefer the ability matching the spec; otherwise highest LevelRequired
                var match = g.FirstOrDefault(a => a.SpecHint == specIndex);
                return match ?? g.OrderByDescending(a => a.LevelRequired).First();
            })
            .OrderByDescending(a => a.LevelRequired)
            .Take(MaxSlotsPerClass)
            .Select(a => a.Id)
            .ToList();

    /// <summary>
    /// Spec-aware loadout with explicitly unlocked talent abilities.
    /// Talent abilities are always preferred; if regular slots run out because of that,
    /// the abilities matching the spec least are removed first
    /// (off-spec before neutral, lowest LevelRequired first).
    /// </summary>
    public static List<string> GetDefaultLoadoutIds(
        CharacterClass cls, int level, int specIndex,
        IReadOnlySet<string> unlockedTalentAbilityIds)
    {
        // Talent-unlocked abilities the player already owns
        // (only one per ExclusiveGroup, e.g. one Druid form – spec-matching preferred)
        var talentAbilities = GetAbilitiesFor(cls)
            .Where(a => a.TalentUnlockOnly && unlockedTalentAbilityIds.Contains(a.Id))
            .GroupBy(a => string.IsNullOrEmpty(a.ExclusiveGroup) ? a.Id : a.ExclusiveGroup)
            .Select(g => g.FirstOrDefault(a => a.SpecHint == specIndex) ?? g.First())
            .ToList();

        // The form determines which abilities are usable (Bear/Cat: no mana spells)
        var form = ShapeshiftForms.FormOf(talentAbilities);
        talentAbilities = talentAbilities.Where(a => ShapeshiftForms.IsUsableIn(a, form)).ToList();

        // Regular abilities (same spec logic as GetDefaultLoadoutIds(cls, level, specIndex))
        var regularAbilities = GetAbilitiesFor(cls)
            .Where(a => a.LevelRequired <= level)
            .Where(a => !a.TalentUnlockOnly)
            .Where(a => ShapeshiftForms.IsUsableIn(a, form))
            // Talent ability replaces a regular one of the same ExclusiveGroup (Windfury instead of Rockbiter)
            .Where(a => string.IsNullOrEmpty(a.ExclusiveGroup) || talentAbilities.All(t => t.ExclusiveGroup != a.ExclusiveGroup))
            .GroupBy(a => a.Name)
            .Select(g => g.OrderByDescending(a => a.Rank).First())
            .GroupBy(a => string.IsNullOrEmpty(a.ExclusiveGroup) ? a.Id : a.ExclusiveGroup)
            .Select(g =>
            {
                var match = g.FirstOrDefault(a => a.SpecHint == specIndex);
                return match ?? g.OrderByDescending(a => a.LevelRequired).First();
            })
            // Spec-matching abilities take precedence; then highest LevelRequired
            .OrderBy(a => a.SpecHint >= 0 && a.SpecHint != specIndex ? 0 : 1) // Off-spec ones out first → high priority = low sort position
            .ThenByDescending(a => a.LevelRequired)
            .ToList();

        // Sorting: off-spec ones should be dropped → we take regular ones from the back
        // i.e. we take the last ones of the sorted list (most spec-matching / highest level)
        int regularSlots = Math.Max(0, MaxSlotsPerClass - talentAbilities.Count);

        // Take the "best" regular ones: spec-matching and high level → at the end of the sorted list
        var keptRegular = regularAbilities
            .OrderByDescending(a => a.SpecHint == specIndex || a.SpecHint < 0 ? 1 : 0)
            .ThenByDescending(a => a.LevelRequired)
            .Take(regularSlots)
            .ToList();

        return talentAbilities
            .Concat(keptRegular)
            .Select(a => a.Id)
            .ToList();
    }

    public static Ability? FindById(string id)
    {
        foreach (var list in Abilities.Values)
            if (list.Any(a => a.Id == id))
                return list.First(a => a.Id == id);
        return null;
    }

    /// <summary>Finds an ability of the class by display name (e.g. for damage meter rows).</summary>
    public static Ability? FindByName(CharacterClass cls, string name)
        => GetAbilitiesFor(cls).FirstOrDefault(a => a.Name == name);

    /// <summary>Class the ability belongs to.</summary>
    public static CharacterClass? ClassOf(Ability ability)
    {
        foreach (var (cls, list) in Abilities)
            if (list.Any(a => a.Id == ability.Id))
                return cls;
        return null;
    }
}
