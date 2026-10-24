// Models/BuffTracker.cs
namespace Models;

/// <summary>
/// Ein laufender Buff: die auslösende Fähigkeit und die Restlaufzeit in Sekunden.
/// <see cref="TickAcc"/>/<see cref="TicksDone"/> zählen die HoT-Ticks mit.
/// </summary>
public sealed record ActiveBuff(Ability Source, double Remaining, double TickAcc = 0, int TicksDone = 0, int MotAmountPerTick = 0, int HotAmountPerTick = 0, int ShieldRemaining = 0);

/// <summary>Ein fälliger HoT- oder MoT-Tick. <see cref="IsMana"/> = true → Mana-Regenerierung statt HP-Heilung.</summary>
public readonly record struct HotTick(Ability Source, int Amount, bool IsMana = false);

/// <summary>
/// Aktive Buffs eines Charakters. Wird vom Simulations-Thread geschrieben und vom UI gelesen,
/// daher alle Zugriffe unter Lock.
/// </summary>
public class BuffTracker
{
    // Schutz gegen Gleitkomma-Drift (20 × 0.1 s ergibt nicht exakt 2.0)
    private const double Epsilon = 1e-9;

    private readonly Dictionary<string, ActiveBuff> _active = new();
    private readonly object _lock = new();
    // Vorab alloziierte Hilfslisten für Tick() – vermeiden Keys.ToList()-Allokation pro Step
    private readonly List<string> _expiredBuffs = new();
    private readonly List<(string Key, ActiveBuff Value)> _buffUpdates = new();
    // Vorab alloziiert für AbsorbDamage – kein Heap-Alloc pro Treffer
    private readonly List<(Ability Source, int Absorbed)> _absorbLog = new();

    public bool IsActive(string abilityId)
    {
        lock (_lock) return _active.ContainsKey(abilityId);
    }

    /// <summary>Startet den Buff bzw. frischt die Laufzeit auf. <paramref name="maxMana"/> für MoT-Buffs, <paramref name="maxHp"/> für prozentuale HoT-Buffs übergeben.</summary>
    public void Apply(Ability ability, int maxMana = 0, int maxHp = 0)
    {
        int motAmount = 0;
        if (ability.IsMot && ability.MotTickCount > 0 && maxMana > 0)
            motAmount = (int)Math.Round(ability.MotManaRestorePct * maxMana / ability.MotTickCount);
        int hotPctAmount = 0;
        if (ability.HotHealPct > 0 && ability.HotTickCount > 0 && maxHp > 0)
            hotPctAmount = (int)Math.Round(ability.HotHealPct * maxHp / ability.HotTickCount);
        int shieldAmount = ability.BuffShieldPct > 0 && maxHp > 0
            ? (int)Math.Round(ability.BuffShieldPct * maxHp)
            : 0;
        lock (_lock)
        {
            // Alten Rang desselben Spells entfernen, damit zwei Ränge nicht gleichzeitig aktiv sind
            var oldKey = _active.Keys.FirstOrDefault(k => k != ability.Id && _active[k].Source.Name == ability.Name);
            if (oldKey is not null) _active.Remove(oldKey);
            _active[ability.Id] = new ActiveBuff(ability, ability.BuffDuration,
                MotAmountPerTick: motAmount, HotAmountPerTick: hotPctAmount, ShieldRemaining: shieldAmount);
        }
    }

    /// <summary>
    /// Absorbiert so viel Schaden wie möglich aus aktiven Schildern.
    /// Gibt pro Schild-Quelle die absorbierte Menge zurück; Liste ist nur bis zum nächsten Aufruf gültig.
    /// </summary>
    public IReadOnlyList<(Ability Source, int Absorbed)> AbsorbDamage(int damage)
    {
        _absorbLog.Clear();
        if (damage <= 0) return _absorbLog;
        lock (_lock)
        {
            int remaining = damage;
            foreach (var key in _active.Keys.ToList())
            {
                if (remaining <= 0) break;
                var buff = _active[key];
                if (buff.ShieldRemaining <= 0) continue;
                int take = Math.Min(remaining, buff.ShieldRemaining);
                remaining -= take;
                _active[key] = buff with { ShieldRemaining = buff.ShieldRemaining - take };
                _absorbLog.Add((buff.Source, take));
            }
        }
        return _absorbLog;
    }

    /// <summary>Lässt alle Buffs um <paramref name="dt"/> ablaufen und liefert die dabei fälligen HoT-Ticks.</summary>
    public IReadOnlyList<HotTick> Tick(double dt)
    {
        List<HotTick>? ticks = null;

        lock (_lock)
        {
            _expiredBuffs.Clear();
            _buffUpdates.Clear();

            // Erst lesen (ohne Dictionary-Mutation), dann Änderungen anwenden
            foreach (var kvp in _active)
            {
                var buff = kvp.Value;
                double remaining = buff.Remaining - dt;
                double tickAcc = buff.TickAcc + dt;
                int ticksDone = buff.TicksDone;

                var src = buff.Source;
                while (src.IsHot && ticksDone < src.HotTickCount && tickAcc + Epsilon >= src.HotTickInterval)
                {
                    tickAcc -= src.HotTickInterval;
                    int amount = src.HotHealPct > 0 ? buff.HotAmountPerTick : src.HotHealForTick(ticksDone);
                    (ticks ??= new()).Add(new HotTick(src, amount));
                    ticksDone++;
                }

                // Mana-over-Time (z. B. Conjure Water)
                while (src.IsMot && ticksDone < src.MotTickCount && tickAcc + Epsilon >= src.MotTickInterval)
                {
                    tickAcc -= src.MotTickInterval;
                    (ticks ??= new()).Add(new HotTick(src, buff.MotAmountPerTick, IsMana: true));
                    ticksDone++;
                }

                if (remaining <= Epsilon)
                    _expiredBuffs.Add(kvp.Key);
                else
                    _buffUpdates.Add((kvp.Key, buff with { Remaining = remaining, TickAcc = tickAcc, TicksDone = ticksDone }));
            }

            foreach (var k in _expiredBuffs)       _active.Remove(k);
            foreach (var (k, v) in _buffUpdates)   _active[k] = v;
        }

        return ticks ?? (IReadOnlyList<HotTick>)Array.Empty<HotTick>();
    }

    public void Remove(string abilityId)
    {
        lock (_lock) _active.Remove(abilityId);
    }

    public void Reset()
    {
        lock (_lock) _active.Clear();
    }

    /// <summary>Multiplikator auf allen verursachten Schaden (Boni stapeln multiplikativ).</summary>
    public double DamageMultiplier
    {
        get
        {
            lock (_lock)
                return _active.Values.Aggregate(1.0, (m, b) => m * (1 + b.Source.BuffDamageBonus));
        }
    }

    /// <summary>Attack-speed multiplier (> 1 = faster). Divide swing interval by this value.</summary>
    public double AttackSpeedMultiplier
    {
        get
        {
            lock (_lock)
                return _active.Values.Aggregate(1.0, (m, b) => m * (1 + b.Source.BuffAttackSpeedBonus));
        }
    }

    /// <summary>Multiplier on auto-attack and weapon-based ability damage only.</summary>
    public double WeaponDamageMultiplier
    {
        get
        {
            lock (_lock)
                return _active.Values.Aggregate(1.0, (m, b) => m * (1 + b.Source.BuffWeaponDamageBonus));
        }
    }

    /// <summary>Total flat Intellect bonus from active buffs.</summary>
    public int IntellectBonus
    {
        get
        {
            lock (_lock)
                return _active.Values.Sum(b => b.Source.BuffIntellectBonus);
        }
    }

    /// <summary>Extra max mana from Intellect buffs (15 mana per Intellect point, classic formula).</summary>
    public int BonusMaxMana => IntellectBonus * 15;

    /// <summary>True wenn ein aktiver Buff Immunität gegen Schaden verleiht (z. B. Divine Shield).</summary>
    public bool HasImmunity
    {
        get
        {
            lock (_lock)
                return _active.Values.Any(b => b.Source.BuffGrantsImmunity);
        }
    }

    /// <summary>Gibt alle Buff-Quellen zurück, die Vergeltungsschaden haben (z. B. Thorns, Lightning Shield).</summary>
    public IReadOnlyList<Ability> GetRetaliationSources()
    {
        lock (_lock)
            return _active.Values
                .Where(b => b.Source.BuffRetaliationDamage > 0)
                .Select(b => b.Source)
                .ToList();
    }

    /// <summary>Rage-Gen-Bonus aus aktiven Buffs (z. B. Battle Stance).</summary>
    public double RageGenBonus
    {
        get
        {
            lock (_lock)
                return _active.Values.Sum(b => b.Source.BuffRageGenBonus);
        }
    }

    /// <summary>Crit-Chance-Bonus aus aktiven Buffs (z. B. Berserker Stance).</summary>
    public double CritChanceBonus
    {
        get
        {
            lock (_lock)
                return _active.Values.Sum(b => b.Source.BuffCritChanceBonus);
        }
    }

    /// <summary>Schadensreduktions-Bonus aus aktiven Buffs (z. B. Defensive Stance).</summary>
    public double DamageTakenReduction
    {
        get
        {
            lock (_lock)
                return _active.Values.Sum(b => b.Source.BuffDamageTakenReduction);
        }
    }

    /// <summary>Summe aller noch absorbierbaren Schild-Punkte (z. B. Ice Barrier).</summary>
    public int TotalShield
    {
        get
        {
            lock (_lock)
                return _active.Values.Sum(b => b.ShieldRemaining);
        }
    }

    /// <summary>Kopie der aktiven Buffs für die Anzeige.</summary>
    public IReadOnlyList<ActiveBuff> Snapshot()
    {
        lock (_lock) return _active.Values.ToList();
    }
}
