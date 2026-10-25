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

    /// <summary>
    /// Spec-bewusster Standard-Loadout: Abilities mit passendem <see cref="Ability.SpecHint"/> werden
    /// innerhalb einer ExclusiveGroup bevorzugt; sonst identisch mit <see cref="GetDefaultLoadoutIds(CharacterClass,int)"/>.
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
                // Bevorzuge Ability die zum Spec passt; sonst höchstes LevelRequired
                var match = g.FirstOrDefault(a => a.SpecHint == specIndex);
                return match ?? g.OrderByDescending(a => a.LevelRequired).First();
            })
            .OrderByDescending(a => a.LevelRequired)
            .Take(MaxSlotsPerClass)
            .Select(a => a.Id)
            .ToList();

    /// <summary>
    /// Spec-bewusster Loadout mit explizit freigeschalteten Talent-Abilities.
    /// Talent-Abilities werden immer bevorzugt eingebaut; wenn dadurch reguläre Slots fehlen,
    /// werden Abilities die am wenigsten zum Spec passen zuerst entfernt
    /// (Spec-Fremdlinge vor neutralen, niedrigstes LevelRequired zuerst).
    /// </summary>
    public static List<string> GetDefaultLoadoutIds(
        CharacterClass cls, int level, int specIndex,
        IReadOnlySet<string> unlockedTalentAbilityIds)
    {
        // Talent-freigeschaltete Abilities die der Spieler bereits besitzt
        // (pro ExclusiveGroup nur eine, z. B. eine Druiden-Gestalt – Spec-passende bevorzugt)
        var talentAbilities = GetAbilitiesFor(cls)
            .Where(a => a.TalentUnlockOnly && unlockedTalentAbilityIds.Contains(a.Id))
            .GroupBy(a => string.IsNullOrEmpty(a.ExclusiveGroup) ? a.Id : a.ExclusiveGroup)
            .Select(g => g.FirstOrDefault(a => a.SpecHint == specIndex) ?? g.First())
            .ToList();

        // Gestalt bestimmt, welche Fähigkeiten nutzbar sind (Bear/Cat: keine Mana-Zauber)
        var form = ShapeshiftForms.FormOf(talentAbilities);
        talentAbilities = talentAbilities.Where(a => ShapeshiftForms.IsUsableIn(a, form)).ToList();

        // Reguläre Abilities (gleiche Spec-Logik wie GetDefaultLoadoutIds(cls, level, specIndex))
        var regularAbilities = GetAbilitiesFor(cls)
            .Where(a => a.LevelRequired <= level)
            .Where(a => !a.TalentUnlockOnly)
            .Where(a => ShapeshiftForms.IsUsableIn(a, form))
            // Talent-Ability ersetzt reguläre derselben ExclusiveGroup (Windfury statt Rockbiter)
            .Where(a => string.IsNullOrEmpty(a.ExclusiveGroup) || talentAbilities.All(t => t.ExclusiveGroup != a.ExclusiveGroup))
            .GroupBy(a => a.Name)
            .Select(g => g.OrderByDescending(a => a.Rank).First())
            .GroupBy(a => string.IsNullOrEmpty(a.ExclusiveGroup) ? a.Id : a.ExclusiveGroup)
            .Select(g =>
            {
                var match = g.FirstOrDefault(a => a.SpecHint == specIndex);
                return match ?? g.OrderByDescending(a => a.LevelRequired).First();
            })
            // Spec-passende Abilities haben Vorrang; danach höchstes LevelRequired
            .OrderBy(a => a.SpecHint >= 0 && a.SpecHint != specIndex ? 0 : 1) // Fremdlinge zuerst raus → hohe Priorität = niedrige sort-Position
            .ThenByDescending(a => a.LevelRequired)
            .ToList();

        // Sortierung: Spec-Fremdlinge sollen rausfliegen → von hinten nehmen wir reguläre
        // d.h. wir nehmen von der sortierten Liste die letzten (spec-passendsten / höchst-leveligsten)
        int regularSlots = Math.Max(0, MaxSlotsPerClass - talentAbilities.Count);

        // Nehme die "besten" regulären: spec-passend und hoher Level → am Ende der sortierten Liste
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
