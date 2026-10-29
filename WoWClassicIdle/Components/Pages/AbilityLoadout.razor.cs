// Logic of the /ability-loadout page (markup in AbilityLoadout.razor, spell card in Components/SpellCard.razor)
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
    // Counter as @key so the status message fades in again even with the same text
    private int _statusVersion;
    private string _search = "";
    private AbilityCategory _categoryFilter = AbilityCategory.All;
    private bool _comingUpExpanded = false;

    private CharacterClass SelectedClass => Engine.PlayerClass;

    // Background of the header: current zone, otherwise the starting zone of the race
    private string HeaderTheme =>
        Engine.CurrentCombatZone?.Theme ?? CombatZoneRegistry.StartZoneFor(Engine.Race)?.Theme ?? "grassland";

    // Abilities unlocked by talent
    private HashSet<string> TalentGrantedAbilityIds =>
        TalentRegistry.GetTalents(Engine.PlayerClass)
            .Where(t => Engine.Player.TalentState.GetRanks(t.Id) > 0)
            .SelectMany(t => t.GrantedAbilityIds)
            .ToHashSet();

    // Form according to the loadout (not the engine) so the pool is correct right after switching
    private DruidForm CurrentForm => ShapeshiftForms.FormOf(CurrentLoadout);

    // Only show abilities usable in the current form; forms themselves stay visible for switching
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

    // Number of unlocked abilities hidden only because of the form (names, not ranks)
    private int HiddenByFormCount => AllUnlockedAbilities
        .Where(a => !IsUsableNow(a))
        .Select(a => a.Name)
        .Distinct()
        .Count();

    /// <summary>
    /// Group ranks per spell. Visible are usable ranks and level-locked ranks
    /// (talent-exclusive ones only after unlocking). Order = first occurrence in the pool.
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

    // Spells with at least one usable rank
    private IReadOnlyList<SpellRankGroup> FilteredKnown => FilteredGroups.Where(g => !g.IsLocked).ToList();

    // "Coming Up": spells of which no rank is usable yet (higher ranks of known spells are on their rank bar)
    private IReadOnlyList<SpellRankGroup> FilteredComingUp => FilteredGroups.Where(g => g.IsLocked).ToList();

    /// <summary>1-based slot number of the equipped rank, 0 = not equipped.</summary>
    private int SlotNumberOf(Ability? a) =>
        a is null ? 0 : CurrentLoadout.EquippedAbilityIds.IndexOf(a.Id) + 1;

    /// <summary>Equipped spell of the same ExclusiveGroup that equipping <paramref name="g"/> would replace.</summary>
    private Ability? ConflictOf(SpellRankGroup g)
    {
        var group = g.Display.ExclusiveGroup;
        if (string.IsNullOrEmpty(group) || g.Equipped is not null) return null;
        return CurrentPool.FirstOrDefault(a => a.ExclusiveGroup == group && a.Name != g.Name
                                            && CurrentLoadout.IsEquipped(a.Id));
    }

    /// <summary>Higher usable rank for an equipped spell, otherwise null.</summary>
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

    /// <summary>Click on the spell card: remove the equipped rank, otherwise equip the best rank.</summary>
    private void OnSpellClicked(SpellRankGroup group)
    {
        if (group.Equipped is { } equipped)
            OnPoolClicked(equipped);
        else if (group.Best is { } best)
            OnPoolClicked(best);
    }

    /// <summary>Click on a rank of the rank bar: equip exactly this rank (replaces another rank in place).</summary>
    private void OnRankClicked(Ability rank)
    {
        if (CurrentLoadout.IsEquipped(rank.Id)) return;
        OnPoolClicked(rank);
    }

    /// <summary>
    /// Removes abilities unusable in the equipped form, saves the loadout
    /// and applies it directly to the engine (resource/portrait switch immediately).
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
            // Form rules: forms replace each other, everything else must fit the current form
            var form = ShapeshiftForms.FormOf(CurrentLoadout);
            if (!ability.IsForm && !ShapeshiftForms.IsUsableIn(ability, form))
            {
                SetStatus(ability.RequiredForm != DruidForm.None
                    ? $"{ability.Name} requires {ShapeshiftForms.Get(ability.RequiredForm)?.Name}."
                    : $"{ability.Name} cannot be equipped in {ShapeshiftForms.Get(form)?.Name}.", false);
                return;
            }

            // Same spell name (different rank): replace the existing rank in place
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

            // Exclusive group: replace the existing ability of the same group
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
