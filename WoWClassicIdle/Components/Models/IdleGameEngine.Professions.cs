using System.Diagnostics;
using Models;
using Timer = System.Timers.Timer;

/// <summary>Laufender Durchlauf im aktiven Slot – existiert nur, solange die Berufs-Unterseite offen ist.</summary>
public class ActiveProfessionRun
{
    public required Player Player { get; init; }
    public required Profession Profession { get; init; }
    /// <summary>Zonen-ID (Sammeln) bzw. Rezept-ID (Herstellen).</summary>
    public required string Target { get; init; }
    /// <summary>Verbleibende Herstellungen; <c>null</c> = endlos (Sammeln).</summary>
    public int? Remaining { get; set; }
    /// <summary>Dauer eines Durchlaufs (Sekunden) – Entzaubern ist kürzer als Sammeln/Herstellen.</summary>
    public double Duration { get; init; } = Professions.ActiveDurationSeconds;
    /// <summary>Warteschlange beim Entzaubern (<see cref="IdleGameEngine.DisenchantTarget"/>), sonst <c>null</c>.</summary>
    public Queue<Item>? DisenchantQueue { get; init; }
    public double Elapsed { get; set; }
    public double Progress => Math.Clamp(Elapsed / Duration, 0, 1);
}

public enum ProfessionLogKind { Loot, SkillUp, Error }

/// <summary>Berufsereignis; <see cref="Id"/> steigt fortlaufend (UI erkennt neue Einträge für die Scrolling-Texte).</summary>
public record ProfessionLogEntry(
    long Id, Profession Profession, string Text, ProfessionLogKind Kind,
    Item? Item = null, int Count = 0, string? Source = null);

// Berufe: eigener Echtzeit-Timer (unabhängig von Kampf-Pause und Spielgeschwindigkeit)
// Passive Slots (Charakter-Belegung) → IdleGameEngine.ProfessionPassive.cs
public partial class IdleGameEngine
{
    private const int ProfessionLogSize = 12;

    private Timer? _professionTimer;
    private readonly Stopwatch _professionClock = new();
    private double _lastProfessionTime;
    // Eigener RNG, damit Stoff-Drops und Berufe die Kampf-Zufallsfolge (Simulator-Seed) nicht verändern
    private readonly Random _professionRng = new();
    private readonly List<ProfessionLogEntry> _professionLog = new();
    private long _nextProfessionLogId;

    // Sammel- und Sekundärberufe sind an keinen Charakter gebunden (accountweit)
    private readonly Dictionary<Profession, ProfessionProgress> _sharedProfessions = new();

    // Zuletzt gewählte Zone (Sammeln) bzw. Rezept (Herstellen) je Beruf – wird beim Öffnen der Unterseite wieder ausgewählt
    private readonly Dictionary<Profession, string> _lastProfessionTargets = new();

    public ActiveProfessionRun? ActiveProfessionRun { get; private set; }

    internal IReadOnlyDictionary<Profession, string> LastProfessionTargets => _lastProfessionTargets;

    /// <summary>Wird nach jedem Berufs-Tick ausgelöst (Timer-Thread!).</summary>
    public event Action? ProfessionUpdated;

    /// <summary>Letzte Berufsereignisse, neueste zuerst.</summary>
    public IReadOnlyList<ProfessionLogEntry> ProfessionLog
    {
        get { lock (_simLock) return _professionLog.ToList(); }
    }

    public IReadOnlyCollection<ProfessionProgress> SharedProfessions => _sharedProfessions.Values;

    /// <summary>Startet den Berufs-Timer (einmalig aus Program.cs – nicht im Headless-Simulator).</summary>
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

    /// <summary>Fortschritt eines Berufs aus Sicht des aktiven Charakters (Hauptberuf) bzw. accountweit (Sammel-/Sekundärberuf).</summary>
    public ProfessionProgress? GetProfessionProgress(Profession profession) =>
        Professions.Get(profession).IsPrimary
            ? Player.ProfessionState.Get(profession)
            : _sharedProfessions.GetValueOrDefault(profession);

    /// <summary>Für Ausbildungsstufen maßgebliches Level: aktiver Charakter bzw. höchster Charakter (accountweit).</summary>
    public int ProfessionTrainingLevel(Profession profession) =>
        Professions.Get(profession).IsPrimary || _roster.Count == 0
            ? Player.Level
            : _roster.Max(s => s.Player.Level);

    /// <summary>Höchster Skill eines Berufs: Hauptberuf über alle Charaktere, geteilte Berufe accountweit (Erfolge).</summary>
    public int HighestProfessionSkill(Profession profession)
    {
        lock (_simLock)
            return Professions.Get(profession).IsPrimary
                ? _roster.Select(s => s.Player.ProfessionState.Get(profession)?.Skill ?? 0).DefaultIfEmpty().Max()
                : _sharedProfessions.GetValueOrDefault(profession)?.Skill ?? 0;
    }

    /// <summary>Reicht der (accountweite) Sammel-Skill für die Zone? Siehe <see cref="ProfessionRegistry.ZoneRequiredSkill"/>.</summary>
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

    /// <summary>Accountweite Berufe werden automatisch gelernt, sobald ein Charakter Level 5 erreicht.</summary>
    private void UnlockSharedProfessions()
    {
        if (_sharedProfessions.Count == Professions.All.Count(d => !d.IsPrimary)) return;
        if (!_roster.Any(s => s.Player.Level >= Professions.MinCharacterLevel)) return;
        foreach (var def in Professions.All.Where(d => !d.IsPrimary))
            _sharedProfessions.TryAdd(def.Profession, new ProfessionProgress(def.Profession));
    }

    /// <summary>Charakter, der den Hauptberuf erlernt hat – jeder Hauptberuf darf nur von einem Charakter gleichzeitig gelernt sein.</summary>
    public CharacterSlot? ProfessionOwner(Profession profession) =>
        _roster.FirstOrDefault(s => s.Player.ProfessionState.IsLearned(profession));

    /// <summary>Grund, warum der aktive Charakter den Beruf nicht erlernen kann (UI-Text) – <c>null</c> wenn erlaubt.</summary>
    public string? ProfessionLearnBlocker(Profession profession)
    {
        if (Professions.Get(profession).IsPrimary
            && Player.Level >= Professions.MinCharacterLevel
            && ProfessionOwner(profession) is { } owner
            && owner.Player != Player)
            return $"Learned by {CharacterName(owner)}";
        return Player.ProfessionState.LearnBlocker(profession, Player.Level);
    }

    /// <summary>Erlernt einen Hauptberuf für den aktiven Charakter. Gibt einen Fehlertext zurück oder <c>null</c>.</summary>
    public string? LearnProfession(Profession profession)
    {
        lock (_simLock)
        {
            var blocker = ProfessionLearnBlocker(profession);
            if (blocker is not null) return blocker;
            Player.ProfessionState.Learn(profession, Player.Level);
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
            // Passive Slots, die der Charakter ohne den Beruf nicht mehr ausüben darf, freigeben
            if (_activeSlotId is { } id && GetCharacter(id) is { } character && !CanWorkPassive(character, profession))
                _passiveAssignments.RemoveAll(a => a.CharacterId == id && a.Profession == profession);
        }
        ProfessionUpdated?.Invoke();
    }

    public bool TrainProfessionRank(Profession profession)
    {
        bool trained;
        lock (_simLock)
            trained = GetProfessionProgress(profession)?.TrainNextRank(ProfessionTrainingLevel(profession)) ?? false;
        ProfessionUpdated?.Invoke();
        return trained;
    }

    // ─── Aktiver Slot ───

    /// <summary>Startet den aktiven Slot (ersetzt einen laufenden Durchlauf). <paramref name="count"/> nur beim Herstellen.</summary>
    public void StartActiveProfession(Profession profession, string target, int? count = null)
    {
        lock (_simLock)
        {
            if (GetProfessionProgress(profession) is null) return;
            if (Professions.Get(profession).IsGathering && !IsZoneUnlocked(profession, target)) return;
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

    /// <summary>Wie oft das Rezept mit dem aktuellen Inventar hergestellt werden kann.</summary>
    public int MaxCraftable(Recipe recipe) =>
        recipe.Reagents.Min(r => _sharedInventory.CountOf(r.Item.Id) / r.Count);

    private void AdvanceActiveRun(double dt)
    {
        if (ActiveProfessionRun is not { } run) return;

        // Charakterwechsel oder verlernt → Durchlauf verfällt
        var progress = run.Player == Player ? GetProfessionProgress(run.Profession) : null;
        if (progress is null)
        {
            ActiveProfessionRun = null;
            return;
        }

        // Bei beschleunigter Simulation (Debug) können mehrere Durchläufe in einen Tick fallen
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

            if (!ExecuteProfession(progress, run.Target, isActive: true, source: null))
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

    // ─── Ausführung ───

    /// <summary>
    /// Führt einen Durchlauf aus (Sammeln oder Herstellen). Skillpunkte gibt es nur im aktiven Slot.
    /// <paramref name="source"/> = Name des passiven Charakters fürs Protokoll.
    /// </summary>
    private bool ExecuteProfession(ProfessionProgress progress, string target, bool isActive, string? source)
    {
        bool success = progress.Definition.IsGathering
            ? ExecuteGathering(progress, target, source)
            : ExecuteCrafting(progress, target, source);

        if (success && isActive && ProfessionRegistry.TargetRequiredSkill(progress.Profession, target) is { } required)
            RollSkillUp(progress, required);

        return success;
    }

    /// <summary>Skillpunkt-Chance – nur solange die Stufe in der eigenen Skill-Range liegt (<see cref="ProfessionRegistry.GivesSkillUp"/>).</summary>
    private void RollSkillUp(ProfessionProgress progress, int requiredSkill)
    {
        if (!ProfessionRegistry.GivesSkillUp(progress.Profession, requiredSkill, progress.Skill)) return;
        if (_professionRng.NextDouble() < Professions.SkillUpChance && progress.TrySkillUp())
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
            || progress.Skill < recipe.RequiredSkill)
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
            // Kein Platz → Herstellung rückgängig machen
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

    // ─── Stoff-Drops im Kampf ───

    /// <summary>Stoffe (Leinen → Runenstoff): je passendem Levelband, Chance/Menge nach Gegnerart (<see cref="ProfessionRegistry.ClothDropFor"/>).</summary>
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
