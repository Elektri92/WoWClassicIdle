// Models/DebuffTracker.cs
namespace Models;

/// <summary>A running debuff on an enemy: the source ability and remaining duration in seconds.</summary>
public sealed record ActiveDebuff(Ability Source, double Remaining, double TickAcc = 0, int TicksDone = 0, int StackCount = 1);

/// <summary>A DoT tick ready to be applied to the enemy.</summary>
public readonly record struct DebuffDotTick(Ability Source, int Amount);

/// <summary>
/// Active debuffs on an enemy. Written by the simulation thread, read by the UI thread —
/// all accesses are lock-guarded.
/// </summary>
public class DebuffTracker
{
    private const double Epsilon = 1e-9;

    private readonly Dictionary<string, ActiveDebuff> _active = new();
    private readonly object _lock = new();
    private readonly List<string> _expiredDebuffs = new();
    private readonly List<(string Key, ActiveDebuff Value)> _debuffUpdates = new();

    public bool IsActive(string abilityId)
    {
        lock (_lock) return _active.ContainsKey(abilityId);
    }

    /// <summary>True if a debuff with this name is active – regardless of rank (synergies like Lava Burst → Flame Shock).</summary>
    public bool HasDebuffNamed(string name)
    {
        lock (_lock) return _active.Values.Any(d => d.Source.Name == name);
    }

    /// <summary>Applies the debuff, resetting duration if already present.</summary>
    public void Apply(Ability ability)
    {
        lock (_lock) _active[ability.Id] = new ActiveDebuff(ability, ability.DebuffDuration);
    }

    /// <summary>Applies Ignite – only overwrites if the new damage per tick is greater than the current one.</summary>
    public void ApplyIgnite(Ability ability)
    {
        lock (_lock)
        {
            if (_active.TryGetValue(ability.Id, out var existing)
                && existing.Source.DebuffDotDamage >= ability.DebuffDotDamage)
                return;
            _active[ability.Id] = new ActiveDebuff(ability, ability.DebuffDuration);
        }
    }

    /// <summary>Advances all debuffs by <paramref name="dt"/> seconds and returns any pending DoT ticks.</summary>
    public IReadOnlyList<DebuffDotTick> Tick(double dt)
    {
        List<DebuffDotTick>? ticks = null;

        lock (_lock)
        {
            _expiredDebuffs.Clear();
            _debuffUpdates.Clear();

            foreach (var kvp in _active)
            {
                var debuff = kvp.Value;
                double remaining = debuff.Remaining - dt;
                double tickAcc = debuff.TickAcc + dt;
                int ticksDone = debuff.TicksDone;

                var src = debuff.Source;
                while (src.IsDebuffDot && ticksDone < src.DebuffDotTickCount && tickAcc + Epsilon >= src.DebuffDotTickInterval)
                {
                    tickAcc -= src.DebuffDotTickInterval;
                    (ticks ??= new()).Add(new DebuffDotTick(src, src.DebuffDotDamage * debuff.StackCount));
                    ticksDone++;
                }

                if (remaining <= Epsilon)
                    _expiredDebuffs.Add(kvp.Key);
                else
                    _debuffUpdates.Add((kvp.Key, debuff with { Remaining = remaining, TickAcc = tickAcc, TicksDone = ticksDone }));
            }

            foreach (var k in _expiredDebuffs)      _active.Remove(k);
            foreach (var (k, v) in _debuffUpdates)  _active[k] = v;
        }

        return ticks ?? (IReadOnlyList<DebuffDotTick>)Array.Empty<DebuffDotTick>();
    }

    public void Reset()
    {
        lock (_lock) _active.Clear();
    }

    /// <summary>Applies a stackable debuff: raises the stack counter up to <see cref="Ability.DebuffMaxStacks"/> and refreshes the duration.</summary>
    public void ApplyStack(Ability ability)
    {
        lock (_lock)
        {
            int maxStacks = ability.DebuffMaxStacks > 0 ? ability.DebuffMaxStacks : 1;
            if (_active.TryGetValue(ability.Id, out var existing))
            {
                int newStacks = Math.Min(maxStacks, existing.StackCount + 1);
                _active[ability.Id] = existing with { Remaining = ability.DebuffDuration, StackCount = newStacks };
            }
            else
            {
                _active[ability.Id] = new ActiveDebuff(ability, ability.DebuffDuration, StackCount: 1);
            }
        }
    }

    /// <summary>Multiplier on incoming damage from player (e.g. Hunter's Mark).</summary>
    public double DamageTakenMultiplier
    {
        get
        {
            lock (_lock)
                return _active.Values.Aggregate(1.0, (m, d) => m * (1 + d.Source.DebuffDamageTakenBonus));
        }
    }

    /// <summary>Attack speed multiplier of the enemy (e.g. 0.80 at −20 %). enemyInterval = Speed / Multiplier → longer interval = slower.</summary>
    public double AttackSpeedMultiplier
    {
        get
        {
            lock (_lock)
                return Math.Max(0.01, _active.Values.Aggregate(1.0, (m, d) => m * (1.0 - d.Source.DebuffAttackSpeedReduction)));
        }
    }

    /// <summary>True if the enemy is incapacitated by a stun debuff.</summary>
    public bool IsStunned
    {
        get
        {
            lock (_lock)
                return _active.Values.Any(d => d.Source.DebuffIsStun);
        }
    }

    /// <summary>Snapshot of active debuffs for UI display.</summary>
    public IReadOnlyList<ActiveDebuff> Snapshot()
    {
        lock (_lock) return _active.Values.ToList();
    }
}
