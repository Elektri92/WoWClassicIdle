using System.Timers;
using System.Diagnostics;
using Models;
using Timer = System.Timers.Timer;

// Neue Systeme als eigene Partial-Datei anlegen; Dateien möglichst < 300 Zeilen halten.
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
    private readonly double _baseDt; // fester Zeitschritt pro Sub-Step in Sekunden

    // ─── Roster ───
    private readonly List<CharacterSlot> _roster = new();
    private int? _activeSlotId;
    private int _nextSlotId;

    public IReadOnlyList<CharacterSlot> CharacterRoster => _roster;
    public bool HasActiveCharacter => _activeSlotId.HasValue;

    internal int? ActiveSlotId => _activeSlotId;
    internal int NextSlotId    => _nextSlotId;

    // ─── Charakterauswahl ───
    public Faction Faction { get; private set; } = Faction.Alliance;
    public Race Race { get; private set; } = Race.Human;
    public Gender Gender { get; private set; } = Gender.Male;
    public CharacterClass PlayerClass { get; private set; } = CharacterClass.Warrior;
    public bool IsGameRunning { get; private set; }

    public Player Player { get; private set; } = null!;

    public Enemy? CurrentEnemy { get; set; }
    // Rare-Gegner, gegen den der Spieler gestorben ist – nach Respawn erneut bekämpfen
    private Enemy? _pendingRareEnemy;
    /// <summary>Rare nach Spielertod erneut in die Warteschlange stellen. Im Headless-Modus deaktivieren um Endlos-Loops zu vermeiden.</summary>
    public bool EnableRareRequeue { get; set; } = true;
    public int TotalKills { get; private set; }
    public int TotalDeaths { get; private set; }
    public int TotalRareKills { get; private set; }
    public int TotalItemsSold { get; private set; }
    public int TotalUncommonLooted { get; private set; }
    public int TotalRareItemsLooted { get; private set; }
    public string? LastLog { get; private set; }

    /// <summary>Teilt einen Inventar-Stapel (Shift-Klick) – gesperrt gegen gleichzeitige Loot-/Berufs-Ticks.</summary>
    public bool SplitInventoryStack(Bag bag, int index, int amount)
    {
        lock (_simLock) return _sharedInventory.SplitStack(bag, index, amount);
    }

    /// <summary>Erhöht den Zähler für manuell verkaufte Items (z.B. per Drag-and-Drop an den Händler).</summary>
    public void TrackItemSold(int count = 1) => TotalItemsSold += count;

    /// <summary>Verbleibende Sekunden beim Geistheiler, bevor der nächste Gegner spawnt.</summary>
    public double RespawnRemaining { get; private set; }
    /// <summary>Gesamtdauer des aktuellen Geistheiler-Aufenthalts (10 s + 1 s pro Spielerlevel).</summary>
    public double RespawnDuration { get; private set; }
    public bool IsAtSpiritHealer => RespawnRemaining > 0;
    public CombatMeter Meter { get; } = new();

    // Gemeinsames Inventar: alle Charaktere teilen einen Rucksack für nicht-klassengebundene Items
    private Inventory _sharedInventory = new();
    public Inventory SharedInventory => _sharedInventory;

    // Serialisiert Simulationsschritte mit Charakterwechseln (Timer-Thread vs. UI-Thread)
    private readonly object _simLock = new();

    public event Action? TickUpdated;

    /// <summary>Wird ausgelöst wenn ein neuer Charakter zum Roster hinzugefügt wird.</summary>
    public event Action? RosterChanged;

    /// <summary>Wird für jeden Treffer/jede Heilung ausgelöst (Timer-Thread!).</summary>
    public event Action<CombatText>? CombatTextRaised;

    /// <summary>Wird ausgelöst, sobald der Effekt einer aktiv gewirkten Fähigkeit eintritt (Timer-Thread!).</summary>
    public event Action<Ability>? AbilityTriggered;

    /// <summary>Fires once per ability that becomes available on level-up (Timer-Thread!).</summary>
    public event Action<Ability>? AbilityUnlocked;

    /// <summary>Wird ausgelöst wenn ein Item von einem Rare-Gegner gedroppt wird (Timer-Thread!).</summary>
    public event Action<Item>? ItemDropped;

    /// <summary>
    /// true = gedroppte Rüstung wird automatisch angelegt wenn sie eine Verbesserung ist
    /// (genutzt vom Debug-Simulator). Normalerweise false: Items landen nur im Inventar.
    /// </summary>
    public bool AutoEquipDrops { get; set; }

    /// <param name="seed">Fester Zufalls-Seed für reproduzierbare Kämpfe (Debug-Simulator); null = zufällig.</param>
    public IdleGameEngine(int intervalMs = 100, int? seed = null)
    {
        _rng = seed is { } s ? new Random(s) : new Random();
        var spawnRng = seed is { } s2 ? new Random(s2 + 1) : new Random();
#if DEBUG
        _spawner = new EnemySpawner(DefaultEnemyTemplates.Default.Concat(DebugEnemyTemplates.All).ToList(), spawnRng);
#else
        _spawner = new EnemySpawner(DefaultEnemyTemplates.Default, spawnRng);
#endif
        _baseDt = intervalMs / 1000.0;
        Player = CreatePlayerInternal();

        _timer = new Timer(intervalMs) { AutoReset = true };
        _timer.Elapsed += OnTick;
        CurrentEnemy = _spawner.Spawn(1, 0);
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
    /// Setzt den Geschwindigkeitsmultiplikator und passt die Timer-Intervall entsprechend an.
    /// Wert wird auf [0.1, 20.0] geklemmt.
    /// </summary>
    public void SetSpeed(double multiplier)
    {
        SpeedMultiplier = Math.Clamp(multiplier, MinSpeed, MaxSpeed);
    }

    /// <summary>
    /// Setzt das Level des aktiven Charakters direkt und spawnt sofort einen passenden Gegner.
    /// </summary>
    public void SetPlayerLevel(int level)
    {
        lock (_simLock)
        {
            Player.SetLevel(Math.Clamp(level, 1, 60));
            CurrentEnemy = _spawner.Spawn(Player.Stats.Level, _stopwatch.Elapsed.TotalSeconds);
        }
        TickUpdated?.Invoke();
    }
}
