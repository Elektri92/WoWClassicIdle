using System.Timers;
using System.Diagnostics;
using Models;
using Timer = System.Timers.Timer;

// Create new systems as their own partial file; keep files below 300 lines where possible.
// Kampf → IdleGameEngine.Combat.cs | Roster → .Roster.cs | Talente → .Talent.cs | Save/Load → .SaveLoad.cs
public partial class IdleGameEngine
{
    private Timer _timer;
    private readonly Stopwatch _stopwatch = new();
    private readonly EnemySpawner _spawner;
    private readonly Random _rng;

    private double _playerAttackAcc;
    private double _enemyAttackAcc;
    private double _rogueManaAcc;
    private double _manaRegenAcc;
    private double _hpRegenAcc;

    public double SpeedMultiplier { get; private set; } = 1.0;
    private const double MinSpeed = 0.1;
    private const double MaxSpeed = 10000.0;
    private readonly double _baseDt; // fixed time step per sub-step in seconds

    // ─── Roster ───
    private readonly List<CharacterSlot> _roster = new();
    private int? _activeSlotId;
    private int _nextSlotId;

    public IReadOnlyList<CharacterSlot> CharacterRoster => _roster;
    public bool HasActiveCharacter => _activeSlotId.HasValue;

    internal int NextSlotId    => _nextSlotId;

    // ─── Charakterauswahl ───
    public Faction Faction { get; private set; } = Faction.Alliance;
    public Race Race { get; private set; } = Race.Human;
    public Gender Gender { get; private set; } = Gender.Male;
    public CharacterClass PlayerClass { get; private set; } = CharacterClass.Warrior;
    public bool IsGameRunning { get; private set; }

    public Player Player { get; private set; } = null!;

    public Enemy? CurrentEnemy { get; set; }
    // Rare enemy the player died against – fight it again after respawn
    private Enemy? _pendingRareEnemy;
    /// <summary>Requeue the rare after the player's death. Disable in headless mode to avoid endless loops.</summary>
    public bool EnableRareRequeue { get; set; } = true;
    public int TotalKills { get; private set; }
    public int TotalDeaths { get; private set; }
    public int TotalRareKills { get; private set; }
    public int TotalItemsSold { get; private set; }
    public int TotalUncommonLooted { get; private set; }
    public int TotalRareItemsLooted { get; private set; }
    public string? LastLog { get; private set; }

    /// <summary>Splits an inventory stack (shift-click) – locked against concurrent loot/profession ticks.</summary>
    public bool SplitInventoryStack(Bag bag, int index, int amount)
    {
        lock (_simLock) return _sharedInventory.SplitStack(bag, index, amount);
    }

    public bool MoveInventoryStack(Bag from, int fromIndex, Bag to, int toIndex)
    {
        lock (_simLock) return _sharedInventory.MoveStack(from, fromIndex, to, toIndex);
    }

    /// <summary>Increments the counter for manually sold items (e.g. via drag and drop onto the merchant).</summary>
    public void TrackItemSold(int count = 1) => TotalItemsSold += count;

    /// <summary>Remaining seconds at the spirit healer before the next enemy spawns.</summary>
    public double RespawnRemaining { get; private set; }
    /// <summary>Total duration of the current spirit healer stay (10 s + 1 s per player level).</summary>
    public double RespawnDuration { get; private set; }
    public bool IsAtSpiritHealer => RespawnRemaining > 0;
    public CombatMeter Meter { get; } = new();

    // Shared inventory: all characters share one backpack for non-class-bound items
    private Inventory _sharedInventory = new();
    public Inventory SharedInventory => _sharedInventory;

    // Serializes simulation steps with character switches (timer thread vs. UI thread)
    private readonly object _simLock = new();

    public event Action? TickUpdated;

    /// <summary>Raised when a new character is added to the roster.</summary>
    public event Action? RosterChanged;

    /// <summary>Raised for every hit/heal (timer thread!).</summary>
    public event Action<CombatText>? CombatTextRaised;

    /// <summary>Raised as soon as the effect of an actively cast ability occurs (timer thread!).</summary>
    public event Action<Ability>? AbilityTriggered;

    /// <summary>Fires once per ability that becomes available on level-up (Timer-Thread!).</summary>
    public event Action<Ability>? AbilityUnlocked;

    /// <summary>Raised when an item is dropped by a rare enemy (timer thread!).</summary>
    public event Action<Item>? ItemDropped;

    /// <summary>
    /// true = rare enemies drop no armor or weapons (used by the debug simulator, which equips
    /// fixed gear per level). Normally false.
    /// </summary>
    public bool DisableGearDrops { get; set; }

    /// <param name="seed">Fixed random seed for reproducible fights (debug simulator); null = random.</param>
    public IdleGameEngine(int intervalMs = 100, int? seed = null)
    {
        _rng = seed is { } s ? new Random(s) : new Random();
        var spawnRng = seed is { } s2 ? new Random(s2 + 1) : new Random();
#if DEBUG
        _spawner = new EnemySpawner(DebugEnemyTemplates.All, spawnRng);
#else
        _spawner = new EnemySpawner(rng: spawnRng);
#endif
        _baseDt = intervalMs / 1000.0;
        Player = CreatePlayerInternal();

        _timer = new Timer(intervalMs) { AutoReset = true };
        _timer.Elapsed += OnTick;
        CurrentEnemy = SpawnEnemy();
    }

    // ─── Spielsteuerung ───
    public void Start()
    {
        IsGameRunning = true;
        _stopwatch.Restart();
        _timer.Start();
    }

    public void Stop()
    {
        IsGameRunning = false;
        _timer.Stop();
        _stopwatch.Stop();
    }

    /// <summary>
    /// Sets the speed multiplier and adjusts the timer interval accordingly.
    /// The value is clamped to [0.1, 20.0].
    /// </summary>
    public void SetSpeed(double multiplier)
    {
        SpeedMultiplier = Math.Clamp(multiplier, MinSpeed, MaxSpeed);
    }

    /// <summary>
    /// Sets the level of the active character directly and immediately spawns a matching enemy.
    /// </summary>
    public void SetPlayerLevel(int level)
    {
        lock (_simLock)
        {
            Player.SetLevel(Math.Clamp(level, 1, 60));
            CurrentEnemy = SpawnEnemy();
        }
        TickUpdated?.Invoke();
    }
}
