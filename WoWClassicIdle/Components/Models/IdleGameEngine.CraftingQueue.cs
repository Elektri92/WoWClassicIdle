using Models;

/// <summary>Position of the crafting queue: a recipe with its open crafts, or a single item to disenchant (<see cref="Recipe"/> = <c>null</c>).</summary>
public class CraftingQueueEntry
{
    public required long Id { get; init; }
    public required Profession Profession { get; init; }
    /// <summary>Crafted result or the item to disenchant.</summary>
    public required Item Item { get; init; }
    public Recipe? Recipe { get; init; }
    public required int Remaining { get; set; }

    public bool IsDisenchant => Recipe is null;
    public double Duration => IsDisenchant ? IdleGameEngine.DisenchantCastSeconds : Professions.ActiveDurationSeconds;
}

// Crafting queue: account-wide, survives character switches and is saved. Each main profession has one owner,
// so entries of several professions are crafted by their respective characters, whoever is being played.
public partial class IdleGameEngine
{
    private readonly List<CraftingQueueEntry> _craftingQueue = new();
    private long _nextCraftingQueueId;
    private double _craftingElapsed;
    // Open queue panels – the queue only runs while the player is in the professions menu
    private int _craftingMenuViewers;

    public const int ReturnToIdleUnlockLevel = 15;

    /// <summary>The queue ran empty by itself while a panel was open and the player wants to go back to combat (not on the sim lock).</summary>
    public event Action? CraftingQueueFinished;

    /// <summary>Player setting: leave the professions for /idle once the queue has run empty.</summary>
    public bool ReturnToIdleAfterCrafting { get; set; }

    public bool IsReturnToIdleUnlocked => _roster.Any(s => s.Player.Level >= ReturnToIdleUnlockLevel);

    public IReadOnlyList<CraftingQueueEntry> CraftingQueue
    {
        get { lock (_simLock) return _craftingQueue.ToList(); }
    }

    /// <summary>Progress of the current position (0–1).</summary>
    public double CraftingProgress
    {
        get
        {
            lock (_simLock)
                return _craftingQueue.Count == 0 ? 0 : Math.Clamp(_craftingElapsed / _craftingQueue[0].Duration, 0, 1);
        }
    }

    /// <summary>Estimated seconds until the whole queue is done, assuming no position drops out for missing materials.</summary>
    public double CraftingQueueRemainingSeconds
    {
        get
        {
            lock (_simLock)
            {
                if (_craftingQueue.Count == 0) return 0;
                double total = _craftingQueue.Sum(e => e.Duration * e.Remaining / ProfessionSpeedFactor(e.Profession));
                return Math.Max(0, total - _craftingElapsed / ProfessionSpeedFactor(_craftingQueue[0].Profession));
            }
        }
    }

    public void EnterCraftingMenu()
    {
        lock (_simLock) _craftingMenuViewers++;
    }

    public void LeaveCraftingMenu()
    {
        lock (_simLock) _craftingMenuViewers--;
    }

    /// <summary>Appends crafts to the queue; directly repeated recipes extend the last position.</summary>
    public void AddToCraftingQueue(string recipeId, int count)
    {
        if (count <= 0 || ProfessionRegistry.GetRecipe(recipeId) is not { } recipe) return;
        lock (_simLock)
        {
            if (ProfessionOwner(recipe.Profession) is null) return;
            if (_craftingQueue.LastOrDefault() is { } last && last.Recipe?.Id == recipe.Id)
                last.Remaining += count;
            else
                _craftingQueue.Add(NewEntry(recipe, count));
            _lastProfessionTargets[recipe.Profession] = recipe.Id;
        }
        ProfessionUpdated?.Invoke();
    }

    public void RemoveFromCraftingQueue(long entryId)
    {
        lock (_simLock)
        {
            int index = _craftingQueue.FindIndex(e => e.Id == entryId);
            if (index < 0) return;
            _craftingQueue.RemoveAt(index);
            if (index == 0) _craftingElapsed = 0;
        }
        ProfessionUpdated?.Invoke();
    }

    /// <summary>Returns whether the player should now be sent back to combat.</summary>
    private bool AdvanceCraftingQueue(double dt)
    {
        bool hadEntries = _craftingQueue.Count > 0;
        RunCraftingQueue(dt);
        return hadEntries && _craftingQueue.Count == 0 && _craftingMenuViewers > 0
            && ReturnToIdleAfterCrafting && IsReturnToIdleUnlocked;
    }

    private void RunCraftingQueue(double dt)
    {
        // Unlearned professions leave the queue even while it is paused
        _craftingQueue.RemoveAll(e => ProfessionOwner(e.Profession) is null);

        if (_craftingMenuViewers == 0) return;
        if (!DropUnworkableEntries())
        {
            _craftingElapsed = 0;
            return;
        }

        // With accelerated simulation (debug) several passes can fall into one tick
        _craftingElapsed += dt * ProfessionSpeedFactor(_craftingQueue[0].Profession);
        while (_craftingElapsed >= _craftingQueue[0].Duration)
        {
            var entry = _craftingQueue[0];
            _craftingElapsed -= entry.Duration;

            var progress = ProfessionOwner(entry.Profession)!.Player.ProfessionState.Get(entry.Profession)!;
            bool success = entry.Recipe is { } recipe
                ? ExecuteProfession(progress, recipe.Id, Professions.SkillUpChance, source: null)
                : ExecuteDisenchant(progress, entry.Item);
            // Only a full inventory fails here: the position stays and is retried on the next pass
            if (!success)
            {
                _craftingElapsed = 0;
                return;
            }

            if (--entry.Remaining <= 0) _craftingQueue.RemoveAt(0);
            if (!DropUnworkableEntries())
            {
                _craftingElapsed = 0;
                return;
            }
        }
    }

    /// <summary>Removes positions that cannot be worked on when it is their turn. Returns whether one is left.</summary>
    private bool DropUnworkableEntries()
    {
        while (_craftingQueue.Count > 0)
        {
            var entry = _craftingQueue[0];
            if (entry.Recipe is { } recipe ? CanCraft(recipe) : CanDisenchant(entry.Item)) return true;
            _craftingQueue.RemoveAt(0);
            _craftingElapsed = 0;
        }
        return false;
    }

    private bool CanCraft(Recipe recipe) =>
        ProfessionOwner(recipe.Profession)?.Player.ProfessionState.Get(recipe.Profession) is { } progress
        && progress.Skill >= recipe.RequiredSkill
        && progress.KnowsRecipe(recipe.Id)
        && MaxCraftable(recipe) > 0;

    private CraftingQueueEntry NewEntry(Recipe recipe, int count) => new()
    {
        Id = ++_nextCraftingQueueId, Profession = recipe.Profession, Item = recipe.Result, Recipe = recipe, Remaining = count,
    };

    private CraftingQueueEntry NewDisenchantEntry(Item item) => new()
    {
        Id = ++_nextCraftingQueueId, Profession = Profession.Enchanting, Item = item, Remaining = 1,
    };

    private void RestoreCraftingQueue(IEnumerable<CraftingQueueSaveData> entries)
    {
        foreach (var data in entries)
        {
            if (data.DisenchantItemId is { } itemId)
            {
                if (ResolveSavedItem(itemId) is not { } item) continue;
                if (data.DisenchantRequiredLevel > 0)
                {
                    if (item is ArmorItem armor) item = armor with { RequiredLevel = data.DisenchantRequiredLevel };
                    else if (item is WeaponItem weapon) item = weapon with { RequiredLevel = data.DisenchantRequiredLevel };
                }
                _craftingQueue.Add(NewDisenchantEntry(item));
            }
            else if (data.Remaining > 0 && ProfessionRegistry.GetRecipe(data.RecipeId) is { } recipe)
            {
                _craftingQueue.Add(NewEntry(recipe, data.Remaining));
            }
        }
    }
}
