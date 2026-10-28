// Logik der Seite /ability-loadout (Markup in AbilityLoadout.razor, Zauberkarte in Components/SpellCard.razor)
using Models;

namespace WoWClassicIdle.Components.Pages;

public partial class AbilityLoadout
{
    private enum AbilityCategory { All, Damage, Buff, Heal, Debuff, Passive }

    private ClassLoadout CurrentLoadout { get; set; } = new();
    private IReadOnlyList<Ability> CurrentPool { get; set; } = Array.Empty<Ability>();
    private HashSet<string> _multiRankNames = new();
    private string _statusMessage = "";
    private bool _statusPositive;
    // Zähler als @key, damit die Statusmeldung auch bei gleichem Text neu einblendet
    private int _statusVersion;
    private string _search = "";
    private AbilityCategory _categoryFilter = AbilityCategory.All;
    private bool _comingUpExpanded = false;

    private CharacterClass SelectedClass => Engine.PlayerClass;

    // Hintergrund des Kopfbereichs: aktuelle Zone, sonst Startgebiet der Rasse
    private string HeaderTheme =>
        Engine.CurrentCombatZone?.Theme ?? CombatZoneRegistry.StartZoneFor(Engine.Race)?.Theme ?? "grassland";

    // Abilities, die per Talent freigeschaltet wurden
    private HashSet<string> TalentGrantedAbilityIds =>
        TalentRegistry.GetTalents(Engine.PlayerClass)
            .Where(t => Engine.Player.TalentState.GetRanks(t.Id) > 0)
            .SelectMany(t => t.GrantedAbilityIds)
            .ToHashSet();

    // Gestalt laut Loadout (nicht Engine), damit der Pool sofort nach dem Wechsel stimmt
    private DruidForm CurrentForm => ShapeshiftForms.FormOf(CurrentLoadout);

    // Nur in der aktuellen Gestalt nutzbare Fähigkeiten zeigen; Gestalten selbst bleiben zum Wechseln sichtbar
    private bool IsUsableNow(Ability a) => a.IsForm || ShapeshiftForms.IsUsableIn(a, CurrentForm);

    private bool IsUnlocked(Ability a, HashSet<string> granted) =>
        a.LevelRequired <= Engine.Player.Level && (!a.TalentUnlockOnly || granted.Contains(a.Id));

    private IEnumerable<Ability> AllUnlockedAbilities
    {
        get
        {
            var granted = TalentGrantedAbilityIds;
            return CurrentPool.Where(a => IsUnlocked(a, granted));
        }
    }

    // Anzahl freigeschalteter Fähigkeiten, die nur wegen der Gestalt ausgeblendet sind (Namen, nicht Ränge)
    private int HiddenByFormCount => AllUnlockedAbilities
        .Where(a => !IsUsableNow(a))
        .Select(a => a.Name)
        .Distinct()
        .Count();

    /// <summary>
    /// Ränge je Zauber gruppieren. Sichtbar sind nutzbare Ränge und level-gesperrte Ränge
    /// (talent-exklusive erst nach Freischaltung). Reihenfolge = erstes Auftreten im Pool.
    /// </summary>
    private IReadOnlyList<SpellRankGroup> BuildGroups()
    {
        var granted = TalentGrantedAbilityIds;
        return CurrentPool
            .Where(a => IsUsableNow(a) && (IsUnlocked(a, granted) || (a.LevelRequired > Engine.Player.Level && !a.TalentUnlockOnly)))
            .GroupBy(a => a.Name)
            .Select(g =>
            {
                var ranks = g.OrderBy(a => a.Rank).ThenBy(a => a.LevelRequired).ToList();
                var unlocked = ranks.Where(a => IsUnlocked(a, granted)).ToList();
                return new SpellRankGroup(g.Key, ranks, unlocked, ranks.FirstOrDefault(a => CurrentLoadout.IsEquipped(a.Id)));
            })
            .ToList();
    }

    private bool MatchesCategory(Ability a) => _categoryFilter switch {
        AbilityCategory.Damage  => a.IsDamage || a.IsDebuffDot,
        AbilityCategory.Buff    => a.IsPlayerBuff || a.IsShield || a.IsForm,
        AbilityCategory.Heal    => a.IsHealing,
        AbilityCategory.Debuff  => a.IsDebuff && !a.IsDebuffDot && !a.IsDamage,
        AbilityCategory.Passive => a.IsPassive,
        _                       => true
    };

    private bool MatchesSearch(Ability a) =>
        string.IsNullOrEmpty(_search) ||
        a.Name.Contains(_search, StringComparison.OrdinalIgnoreCase);

    private IEnumerable<SpellRankGroup> FilteredGroups =>
        BuildGroups().Where(g => MatchesCategory(g.Display) && MatchesSearch(g.Display));

    // Zauber mit mindestens einem nutzbaren Rang
    private IReadOnlyList<SpellRankGroup> FilteredKnown => FilteredGroups.Where(g => !g.IsLocked).ToList();

    // "Coming Up": Zauber, von denen noch kein Rang nutzbar ist (höhere Ränge bekannter Zauber stehen auf deren Rangleiste)
    private IReadOnlyList<SpellRankGroup> FilteredComingUp => FilteredGroups.Where(g => g.IsLocked).ToList();

    /// <summary>1-basierte Slot-Nummer des ausgerüsteten Rangs, 0 = nicht ausgerüstet.</summary>
    private int SlotNumberOf(Ability? a) =>
        a is null ? 0 : CurrentLoadout.EquippedAbilityIds.IndexOf(a.Id) + 1;

    /// <summary>Ausgerüsteter Zauber derselben ExclusiveGroup, den das Ausrüsten von <paramref name="g"/> ersetzen würde.</summary>
    private Ability? ConflictOf(SpellRankGroup g)
    {
        var group = g.Display.ExclusiveGroup;
        if (string.IsNullOrEmpty(group) || g.Equipped is not null) return null;
        return CurrentPool.FirstOrDefault(a => a.ExclusiveGroup == group && a.Name != g.Name
                                            && CurrentLoadout.IsEquipped(a.Id));
    }

    /// <summary>Höherer nutzbarer Rang für einen ausgerüsteten Zauber, sonst null.</summary>
    private Ability? UpgradeFor(Ability equipped)
    {
        var granted = TalentGrantedAbilityIds;
        var best = CurrentPool
            .Where(a => a.Name == equipped.Name && IsUnlocked(a, granted) && IsUsableNow(a))
            .MaxBy(a => a.Rank);
        return best is not null && best.Rank > equipped.Rank ? best : null;
    }

    protected override void OnInitialized()
    {
        LoadCurrentClass();
        Engine.TickUpdated += OnEngineTick;
    }

    private void OnEngineTick()
    {
        InvokeAsync(StateHasChanged);
    }

    private void LoadCurrentClass()
    {
        CurrentLoadout = LoadoutService.GetLoadout(SelectedClass, Engine.Player.Level);
        CurrentPool = ClassAbilityRegistry.GetAbilitiesFor(SelectedClass).ToList();
        _multiRankNames = CurrentPool
            .GroupBy(a => a.Name)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToHashSet();
    }

    private void SetStatus(string message, bool positive)
    {
        _statusMessage = message;
        _statusPositive = positive;
        _statusVersion++;
    }

    private void OnSlotClicked(int slotIndex)
    {
        if (slotIndex >= CurrentLoadout.EquippedAbilityIds.Count)
        {
            SetStatus("Slot is empty.", false);
            return;
        }

        CurrentLoadout.RemoveSlot(slotIndex);
        SetStatus("Ability removed.", true);
        CommitLoadout();
    }

    /// <summary>Klick auf die Zauberkarte: ausgerüsteten Rang entfernen, sonst besten Rang ausrüsten.</summary>
    private void OnSpellClicked(SpellRankGroup group)
    {
        if (group.Equipped is { } equipped)
            OnPoolClicked(equipped);
        else if (group.Best is { } best)
            OnPoolClicked(best);
    }

    /// <summary>Klick auf einen Rang der Rangleiste: genau diesen Rang ausrüsten (ersetzt einen anderen Rang an Ort und Stelle).</summary>
    private void OnRankClicked(Ability rank)
    {
        if (CurrentLoadout.IsEquipped(rank.Id)) return;
        OnPoolClicked(rank);
    }

    /// <summary>
    /// Entfernt in der ausgerüsteten Gestalt unbrauchbare Fähigkeiten, speichert das Loadout
    /// und übernimmt es direkt in die Engine (Ressource/Portrait wechseln sofort).
    /// </summary>
    private void CommitLoadout()
    {
        var removed = ShapeshiftForms.PurgeIncompatible(CurrentLoadout);
        if (removed.Count > 0)
            _statusMessage += $" Unequipped (not usable in current form): {string.Join(", ", removed.Select(a => a.Name))}.";
        LoadoutService.SaveLoadout(CurrentLoadout);
        Engine.ApplyLoadout(CurrentLoadout);
    }

    private void OnPoolClicked(Ability ability)
    {
        if (ability.LevelRequired > Engine.Player.Level)
        {
            SetStatus($"Level {ability.LevelRequired} required (current: {Engine.Player.Level}).", false);
            return;
        }

        if (ability.TalentUnlockOnly && !TalentGrantedAbilityIds.Contains(ability.Id))
        {
            SetStatus($"{ability.Name} requires a talent to unlock.", false);
            return;
        }

        if (CurrentLoadout.IsEquipped(ability.Id))
        {
            int idx = CurrentLoadout.EquippedAbilityIds.ToList().FindIndex(x => x == ability.Id);
            if (idx >= 0) CurrentLoadout.RemoveSlot(idx);
            SetStatus($"{ability.Name} removed.", true);
        }
        else
        {
            // Gestalt-Regeln: Formen ersetzen sich gegenseitig, alles andere muss zur aktuellen Form passen
            var form = ShapeshiftForms.FormOf(CurrentLoadout);
            if (!ability.IsForm && !ShapeshiftForms.IsUsableIn(ability, form))
            {
                SetStatus(ability.RequiredForm != DruidForm.None
                    ? $"{ability.Name} requires {ShapeshiftForms.Get(ability.RequiredForm)?.Name}."
                    : $"{ability.Name} cannot be equipped in {ShapeshiftForms.Get(form)?.Name}.", false);
                return;
            }

            // Gleicher Spell-Name (anderer Rank): vorhandenen Rank in-place ersetzen
            var sameSpell = CurrentPool
                .FirstOrDefault(a => a.Name == ability.Name
                                  && a.Id != ability.Id
                                  && CurrentLoadout.IsEquipped(a.Id));
            if (sameSpell != null)
            {
                int sameIdx = CurrentLoadout.EquippedAbilityIds.ToList().FindIndex(x => x == sameSpell.Id);
                CurrentLoadout.SetSlot(sameIdx, ability.Id);
                SetStatus($"{ability.Name} Rank {sameSpell.Rank} replaced by Rank {ability.Rank}.", true);
                CommitLoadout();
                return;
            }

            // Exklusiv-Gruppe: vorhandene Ability derselben Gruppe ersetzen
            if (!string.IsNullOrEmpty(ability.ExclusiveGroup))
            {
                var conflict = CurrentPool
                    .FirstOrDefault(a => a.ExclusiveGroup == ability.ExclusiveGroup
                                      && CurrentLoadout.IsEquipped(a.Id));
                if (conflict != null)
                {
                    int conflictIdx = CurrentLoadout.EquippedAbilityIds.ToList().FindIndex(x => x == conflict.Id);
                    CurrentLoadout.SetSlot(conflictIdx, ability.Id);
                    SetStatus($"{conflict.Name} replaced by {ability.Name}.", true);
                    CommitLoadout();
                    return;
                }
            }

            int nextSlot = CurrentLoadout.EquippedAbilityIds.Count;
            if (nextSlot >= ClassAbilityRegistry.MaxSlotsPerClass)
            {
                SetStatus("All slots are full!", false);
                return;
            }
            CurrentLoadout.SetSlot(nextSlot, ability.Id);
            SetStatus($"{ability.Name} → Slot {nextSlot + 1}.", true);
        }
        CommitLoadout();
    }

    public void Dispose()
    {
        Engine.TickUpdated -= OnEngineTick;
    }
}
