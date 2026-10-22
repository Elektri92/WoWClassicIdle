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

    public bool IsActive(string abilityId)
    {
        lock (_lock) return _active.ContainsKey(abilityId);
    }

    /// <summary>Applies the debuff, resetting duration if already present.</summary>
    public void Apply(Ability ability)
    {
        lock (_lock) _active[ability.Id] = new ActiveDebuff(ability, ability.DebuffDuration);
    }

    /// <summary>Advances all debuffs by <paramref name="dt"/> seconds and returns any pending DoT ticks.</summary>
    public IReadOnlyList<DebuffDotTick> Tick(double dt)
    {
        List<DebuffDotTick>? ticks = null;

        lock (_lock)
        {
            foreach (var key in _active.Keys.ToList())
            {
                var debuff = _active[key];
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
                    _active.Remove(key);
                else
                    _active[key] = debuff with { Remaining = remaining, TickAcc = tickAcc, TicksDone = ticksDone };
            }
        }

        return ticks ?? (IReadOnlyList<DebuffDotTick>)Array.Empty<DebuffDotTick>();
    }

    public void Reset()
    {
        lock (_lock) _active.Clear();
    }

    /// <summary>Wendet einen stapelbaren Debuff an: erhöht den Stack-Zähler bis <see cref="Ability.DebuffMaxStacks"/> und frischt die Laufzeit auf.</summary>
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

    /// <summary>Angriffstempo-Multiplikator des Gegners (z. B. 0.80 bei −20 %). enemyInterval = Speed / Multiplier → längeres Intervall = langsamer.</summary>
    public double AttackSpeedMultiplier
    {
        get
        {
            lock (_lock)
                return Math.Max(0.01, _active.Values.Aggregate(1.0, (m, d) => m * (1.0 - d.Source.DebuffAttackSpeedReduction)));
        }
    }

    /// <summary>True wenn der Gegner durch einen Betäubungs-Debuff handlungsunfähig ist.</summary>
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
