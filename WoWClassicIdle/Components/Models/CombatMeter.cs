// Models/CombatMeter.cs
namespace Models;

public enum CombatActionKind { Damage, Heal }

/// <summary>Ein einzelner Ability-Einsatz für die Aktionshistorie.</summary>
public sealed record CombatAction(int Id, double GameTime, string Name, string IconUrl, CombatActionKind Kind, int Amount);

/// <summary>Aggregierte Werte einer Quelle (Ability oder Auto Attack) für das Damage Meter.</summary>
public sealed record MeterRow(string Name, string IconUrl, CombatActionKind Kind, long Total, int Count, int Max);

public sealed record MeterSnapshot(
    IReadOnlyList<MeterRow> Rows,
    IReadOnlyList<CombatAction> History,
    long TotalDamage,
    long TotalHealing,
    double CombatSeconds)
{
    public double Dps => CombatSeconds > 0 ? TotalDamage / CombatSeconds : 0;
    public double Hps => CombatSeconds > 0 ? TotalHealing / CombatSeconds : 0;
}

/// <summary>
/// Sammelt Schaden/Heilung des Spielers. Wird vom Engine-Timer-Thread beschrieben
/// und von der UI per <see cref="GetSnapshot"/> gelesen – daher durchgehend gelockt.
/// </summary>
public sealed class CombatMeter
{
    public const int MaxHistory = 20;
    public const string AutoAttackName = "Auto Attack";
    public const string AutoAttackIconUrl = "https://wow.zamimg.com/images/wow/icons/large/inv_sword_04.jpg";
    public const string SpiritRegenName = "Passive Spirit Regeneration";
    public const string SpiritRegenIconUrl = "https://wow.zamimg.com/images/wow/icons/large/spell_holy_divinespirit.jpg";

    private sealed class Entry
    {
        public required string IconUrl;
        public required CombatActionKind Kind;
        public long Total;
        public int Count;
        public int Max;
    }

    private readonly object _lock = new();
    private readonly Dictionary<string, Entry> _entries = new();
    private readonly LinkedList<CombatAction> _history = new();
    private double _combatSeconds;
    private int _nextActionId;

    public void AddCombatTime(double dt)
    {
        lock (_lock) _combatSeconds += dt;
    }

    public void RecordAutoAttack(int amount, string? weaponIconUrl = null) =>
        Record(AutoAttackName, weaponIconUrl ?? AutoAttackIconUrl, CombatActionKind.Damage, amount);

    public void RecordAbility(Ability ability, CombatActionKind kind, int amount) =>
        Record(ability.Name, ability.IconUrl, kind, amount);

    /// <summary>Passive HP-Regeneration aus Spirit – erscheint als eigene Heal-Zeile.</summary>
    public void RecordSpiritRegen(int amount) =>
        Record(SpiritRegenName, SpiritRegenIconUrl, CombatActionKind.Heal, amount);

    private void Record(string name, string iconUrl, CombatActionKind kind, int amount)
    {
        lock (_lock)
        {
            Accumulate(name, iconUrl, kind, amount);
            // Zeitstempel relativ zum letzten Reset, passend zur Kampfzeit im Meter
            _history.AddFirst(new CombatAction(_nextActionId++, _combatSeconds, name, iconUrl, kind, amount));
            while (_history.Count > MaxHistory)
                _history.RemoveLast();
        }
    }

    public void Reset()
    {
        lock (_lock)
        {
            _entries.Clear();
            _history.Clear();
            _combatSeconds = 0;
        }
    }

    public MeterSnapshot GetSnapshot()
    {
        lock (_lock)
        {
            var rows = _entries
                .Select(kv => new MeterRow(kv.Key, kv.Value.IconUrl, kv.Value.Kind, kv.Value.Total, kv.Value.Count, kv.Value.Max))
                .OrderByDescending(r => r.Total)
                .ToList();

            long dmg  = rows.Where(r => r.Kind == CombatActionKind.Damage).Sum(r => r.Total);
            long heal = rows.Where(r => r.Kind == CombatActionKind.Heal).Sum(r => r.Total);

            return new MeterSnapshot(rows, _history.ToList(), dmg, heal, _combatSeconds);
        }
    }

    private void Accumulate(string name, string iconUrl, CombatActionKind kind, int amount)
    {
        if (!_entries.TryGetValue(name, out var entry))
        {
            entry = new Entry { IconUrl = iconUrl, Kind = kind };
            _entries[name] = entry;
        }

        entry.Total += amount;
        entry.Count++;
        entry.Max = Math.Max(entry.Max, amount);
    }
}
