using System.Diagnostics;
using Models;
using Timer = System.Timers.Timer;

/// <summary>Running pass in the active slot (crafting, disenchanting) – only exists while the profession subpage is open.</summary>
public class ActiveProfessionRun
{
    public required Player Player { get; init; }
    public required Profession Profession { get; init; }
    /// <summary>Recipe ID or <see cref="IdleGameEngine.DisenchantTarget"/>.</summary>
    public required string Target { get; init; }
    /// <summary>Remaining crafts; <c>null</c> = endless.</summary>
    public int? Remaining { get; set; }
    /// <summary>Duration of a pass (seconds) – disenchanting is shorter than crafting.</summary>
    public double Duration { get; init; } = Professions.ActiveDurationSeconds;
    /// <summary>Queue when disenchanting (<see cref="IdleGameEngine.DisenchantTarget"/>), otherwise <c>null</c>.</summary>
    public Queue<Item>? DisenchantQueue { get; init; }
    public double Elapsed { get; set; }
    public double Progress => Math.Clamp(Elapsed / Duration, 0, 1);
}

public enum ProfessionLogKind { Loot, SkillUp, Error }

/// <summary>Profession event; <see cref="Id"/> increases continuously (UI detects new entries for the scrolling texts).</summary>
public record ProfessionLogEntry(
    long Id, Profession Profession, string Text, ProfessionLogKind Kind,
    Item? Item = null, int Count = 0, string? Source = null);

// Professions: own real-time timer (independent of combat pause and game speed)
// Passive Slots (Charakter-Belegung) → IdleGameEngine.ProfessionPassive.cs, Lehrer → IdleGameEngine.ProfessionTrainer.cs
public partial class IdleGameEngine
{
    private const int ProfessionLogSize = 12;

    private Timer? _professionTimer;
    private readonly Stopwatch _professionClock = new();
    private double _lastProfessionTime;
    // Own RNG so cloth drops and professions don't change the combat random sequence (simulator seed)
    private readonly Random _professionRng = new();
    private readonly List<ProfessionLogEntry> _professionLog = new();
    private long _nextProfessionLogId;

    // Gathering and secondary professions are not bound to a character (account-wide)
    private readonly Dictionary<Profession, ProfessionProgress> _sharedProfessions = new();

    // Last chosen zone (gathering) or recipe (crafting) per profession – selected again when the subpage is opened
    private readonly Dictionary<Profession, string> _lastProfessionTargets = new();

    public ActiveProfessionRun? ActiveProfessionRun { get; private set; }

    internal IReadOnlyDictionary<Profession, string> LastProfessionTargets => _lastProfessionTargets;

    /// <summary>Raised after every profession tick (timer thread!).</summary>
    public event Action? ProfessionUpdated;

    /// <summary>Latest profession events, newest first.</summary>
    public IReadOnlyList<ProfessionLogEntry> ProfessionLog
    {
        get { lock (_simLock) return _professionLog.ToList(); }
    }

    public IReadOnlyCollection<ProfessionProgress> SharedProfessions => _sharedProfessions.Values;

    /// <summary>Starts the profession timer (once from Program.cs – not in the headless simulator).</summary>
    public void StartProfessionTimer()
    {
        if (_professionTimer is not null) return;
        _professionClock.Start();
        _professionTimer = new Timer(100) { AutoReset = true };
        _professionTimer.Elapsed += (_, _) => OnProfessionTick();
        _professionTimer.Start();
    }

    private void OnProfessionTick()
    {
        double now = _professionClock.Elapsed.TotalSeconds;
        double dt = (now - _lastProfessionTime) * ProfessionSpeed;
        _lastProfessionTime = now;

        lock (_simLock)
        {
            UnlockSharedProfessions();
            AdvanceActiveRun(dt);
            AdvancePassiveSlots(dt);
        }
        ProfessionUpdated?.Invoke();
    }

    // ─── Abfragen ───

    /// <summary>Progress of a profession from the perspective of the active character (main profession) or account-wide (gathering/secondary profession).</summary>
    public ProfessionProgress? GetProfessionProgress(Profession profession) =>
        Professions.Get(profession).IsPrimary
            ? Player.ProfessionState.Get(profession)
            : _sharedProfessions.GetValueOrDefault(profession);

    /// <summary>Level relevant for training tiers: active character or highest character (account-wide).</summary>
    public int ProfessionTrainingLevel(Profession profession) =>
        Professions.Get(profession).IsPrimary || _roster.Count == 0
            ? Player.Level
            : _roster.Max(s => s.Player.Level);

    /// <summary>Bonus on crafting and disenchanting skill-up chances (not gathering) from the levels of all characters combined.</summary>
    public double ProfessionSkillUpBonus => Professions.SkillUpBonus(_roster.Sum(s => s.Player.Level));

    /// <summary>Highest skill of a profession: main profession across all characters, shared professions account-wide (achievements).</summary>
    public int HighestProfessionSkill(Profession profession)
    {
        lock (_simLock)
            return Professions.Get(profession).IsPrimary
                ? _roster.Select(s => s.Player.ProfessionState.Get(profession)?.Skill ?? 0).DefaultIfEmpty().Max()
                : _sharedProfessions.GetValueOrDefault(profession)?.Skill ?? 0;
    }

    /// <summary>Is the (account-wide) gathering skill enough for the zone? See <see cref="ProfessionRegistry.ZoneRequiredSkill"/>.</summary>
    public bool IsZoneUnlocked(Profession profession, string? zoneId) =>
        ProfessionRegistry.GetZone(profession, zoneId) is { } zone
        && (GetProfessionProgress(profession)?.Skill ?? 0) >= ProfessionRegistry.ZoneRequiredSkill(zone);

    public string? GetLastProfessionTarget(Profession profession) =>
        _lastProfessionTargets.GetValueOrDefault(profession);

    public void RememberProfessionTarget(Profession profession, string target)
    {
        lock (_simLock) _lastProfessionTargets[profession] = target;
    }

    // ─── Lernen / Verlernen / Ausbildung ───

    /// <summary>Account-wide professions are learned automatically as soon as a character reaches level 5.</summary>
    private void UnlockSharedProfessions()
    {
        if (_sharedProfessions.Count == Professions.All.Count(d => !d.IsPrimary)) return;
        if (!_roster.Any(s => s.Player.Level >= Professions.MinCharacterLevel)) return;
        foreach (var def in Professions.All.Where(d => !d.IsPrimary))
            _sharedProfessions.TryAdd(def.Profession, new ProfessionProgress(def.Profession));
    }

    /// <summary>True once any character on the account has reached the profession unlock level.</summary>
    public bool ProfessionsUnlockedOnAccount
    {
        // Polled from the tick thread by the nav menu, so the roster must not change mid-enumeration
        get { lock (_simLock) return _roster.Any(s => s.Player.Level >= Professions.MinCharacterLevel); }
    }

    /// <summary>Character who learned the main profession – each main profession may only be learned by one character at a time.</summary>
    public CharacterSlot? ProfessionOwner(Profession profession) =>
        _roster.FirstOrDefault(s => s.Player.ProfessionState.IsLearned(profession));

    /// <summary>Reason why the active character cannot learn the profession (UI text) – <c>null</c> if allowed.</summary>
    public string? ProfessionLearnBlocker(Profession profession)
    {
        if (Professions.Get(profession).IsPrimary
            && Player.Level >= Professions.MinCharacterLevel
            && ProfessionOwner(profession) is { } owner
            && owner.Player != Player)
            return $"Learned by {CharacterName(owner)}";
        return Player.ProfessionState.LearnBlocker(profession, Player.Level);
    }

    /// <summary>Learns a main profession for the active character. Returns an error text or <c>null</c>.</summary>
    public string? LearnProfession(Profession profession)
    {
        lock (_simLock)
        {
            var blocker = ProfessionLearnBlocker(profession);
            if (blocker is not null) return blocker;
            Player.ProfessionState.Learn(profession, Player.Level, Player.ActiveClass);
        }
        ProfessionUpdated?.Invoke();
        return null;
    }

    public void UnlearnProfession(Profession profession)
    {
        lock (_simLock)
        {
            if (!Player.ProfessionState.Unlearn(profession)) return;
            if (ActiveProfessionRun?.Profession == profession) ActiveProfessionRun = null;
            // Free passive slots the character may no longer work without the profession
            if (_activeSlotId is { } id && GetCharacter(id) is { } character && !CanWorkPassive(character, profession))
                _passiveAssignments.RemoveAll(a => a.CharacterId == id && a.Profession == profession);
        }
        ProfessionUpdated?.Invoke();
    }

    // ─── Aktiver Slot ───

    /// <summary>Starts the active slot (replaces a running pass). Gathering professions only have passive slots.</summary>
    public void StartActiveProfession(Profession profession, string target, int? count = null)
    {
        lock (_simLock)
        {
            if (GetProfessionProgress(profession) is null) return;
            if (Professions.Get(profession).IsGathering) return;
            ActiveProfessionRun = new ActiveProfessionRun
            {
                Player = Player, Profession = profession, Target = target, Remaining = count,
            };
            _lastProfessionTargets[profession] = target;
        }
        ProfessionUpdated?.Invoke();
    }

    public void StopActiveProfession()
    {
        lock (_simLock) ActiveProfessionRun = null;
        ProfessionUpdated?.Invoke();
    }

    /// <summary>How often the recipe can be crafted with the current inventory.</summary>
    public int MaxCraftable(Recipe recipe) =>
        recipe.Reagents.Min(r => _sharedInventory.CountOf(r.Item.Id) / r.Count);

    private void AdvanceActiveRun(double dt)
    {
        if (ActiveProfessionRun is not { } run) return;

        // Character switch or unlearned → the pass is discarded
        var progress = run.Player == Player ? GetProfessionProgress(run.Profession) : null;
        if (progress is null)
        {
            ActiveProfessionRun = null;
            return;
        }

        // With accelerated simulation (debug) several passes can fall into one tick
        run.Elapsed += dt;
        while (run.Elapsed >= run.Duration)
        {
            run.Elapsed -= run.Duration;

            if (run.DisenchantQueue is { } queue)
            {
                ExecuteNextDisenchant(progress, queue);
                run.Remaining = queue.Count;
                if (queue.Count == 0)
                {
                    ActiveProfessionRun = null;
                    return;
                }
                continue;
            }

            if (!ExecuteProfession(progress, run.Target, Professions.SkillUpChance, source: null))
            {
                ActiveProfessionRun = null;
                return;
            }

            if (run.Remaining is { } remaining)
            {
                run.Remaining = remaining - 1;
                if (run.Remaining <= 0)
                {
                    ActiveProfessionRun = null;
                    return;
                }
            }
        }
    }

    // ─── Execution ───

    /// <summary>
    /// Executes one pass (gathering or crafting) and rolls <paramref name="skillUpChance"/> for a skill point.
    /// <paramref name="source"/> = name of the passive character for the log.
    /// </summary>
    private bool ExecuteProfession(ProfessionProgress progress, string target, double skillUpChance, string? source)
    {
        bool success = progress.Definition.IsGathering
            ? ExecuteGathering(progress, target, source)
            : ExecuteCrafting(progress, target, source);

        if (success && ProfessionRegistry.TargetRequiredSkill(progress.Profession, target) is { } required)
            RollSkillUp(progress, required, skillUpChance);

        return success;
    }

    /// <summary>Skill-up chance – only while the tier is within its own skill range (<see cref="ProfessionRegistry.GivesSkillUp"/>).</summary>
    private void RollSkillUp(ProfessionProgress progress, int requiredSkill, double chance = Professions.SkillUpChance)
    {
        if (!ProfessionRegistry.GivesSkillUp(progress.Profession, requiredSkill, progress.Skill)) return;
        if (!progress.Definition.IsGathering) chance += ProfessionSkillUpBonus;
        if (_professionRng.NextDouble() < chance && progress.TrySkillUp())
            LogProfession(progress.Profession, $"{progress.Definition.Name} {progress.Skill}", ProfessionLogKind.SkillUp);
    }

    private bool ExecuteGathering(ProfessionProgress progress, string zoneId, string? source)
    {
        if (ProfessionRegistry.GetZone(progress.Profession, zoneId) is not { } zone) return false;
        if (!IsZoneUnlocked(progress.Profession, zoneId)) return false;

        var drop = ProfessionRegistry.RollDrop(zone, _professionRng);
        int leftover = _sharedInventory.TryAdd(drop.Item, drop.Count);
        if (leftover == drop.Count)
        {
            LogProfession(progress.Profession, "Inventory is full", ProfessionLogKind.Error, source: source);
            return false;
        }
        LogProfession(progress.Profession, drop.Item.Name, ProfessionLogKind.Loot, drop.Item, drop.Count - leftover, source);
        return true;
    }

    private bool ExecuteCrafting(ProfessionProgress progress, string recipeId, string? source)
    {
        if (ProfessionRegistry.GetRecipe(recipeId) is not { } recipe
            || recipe.Profession != progress.Profession
            || progress.Skill < recipe.RequiredSkill
            || !progress.KnowsRecipe(recipe.Id))
            return false;

        if (recipe.Reagents.Any(r => _sharedInventory.CountOf(r.Item.Id) < r.Count))
        {
            LogProfession(progress.Profession, "Missing reagents", ProfessionLogKind.Error, source: source);
            return false;
        }

        foreach (var reagent in recipe.Reagents)
            _sharedInventory.Remove(reagent.Item.Id, reagent.Count);

        int leftover = _sharedInventory.TryAdd(recipe.Result, recipe.ResultCount);
        if (leftover > 0)
        {
            // No space → undo the crafting
            _sharedInventory.Remove(recipe.Result.Id, recipe.ResultCount - leftover);
            foreach (var reagent in recipe.Reagents)
                _sharedInventory.TryAdd(reagent.Item, reagent.Count);
            LogProfession(progress.Profession, "Inventory is full", ProfessionLogKind.Error, source: source);
            return false;
        }

        LogProfession(progress.Profession, recipe.Result.Name, ProfessionLogKind.Loot, recipe.Result, recipe.ResultCount, source);
        return true;
    }

    private void LogProfession(Profession profession, string text, ProfessionLogKind kind,
                               Item? item = null, int count = 0, string? source = null)
    {
        _professionLog.Insert(0, new ProfessionLogEntry(++_nextProfessionLogId, profession, text, kind, item, count, source));
        if (_professionLog.Count > ProfessionLogSize)
            _professionLog.RemoveAt(_professionLog.Count - 1);
    }

    // ─── Cloth drops in combat ───

    /// <summary>Cloth (Linen → Runecloth): per matching level band, chance/amount by enemy type (<see cref="ProfessionRegistry.ClothDropFor"/>).</summary>
    private void RollCombatTradeDrops(Enemy enemy)
    {
        int level = enemy.Stats.Level;
        var (chance, minCount, maxCount) = ProfessionRegistry.ClothDropFor(enemy);
        foreach (var cloth in ProfessionRegistry.ClothDrops)
        {
            if (level < cloth.MinEnemyLevel || level > cloth.MaxEnemyLevel) continue;
            if (_professionRng.NextDouble() < chance)
                Player.Inventory.TryAdd(cloth.Item, _professionRng.Next(minCount, maxCount + 1));
        }
    }
}
