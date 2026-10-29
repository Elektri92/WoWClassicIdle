// Models/BuffTracker.cs
namespace Models;

/// <summary>
/// A running buff: the source ability and the remaining duration in seconds.
/// <see cref="TickAcc"/>/<see cref="TicksDone"/> keep track of the HoT ticks.
/// </summary>
public sealed record ActiveBuff(Ability Source, double Remaining, double TickAcc = 0, int TicksDone = 0, int MotAmountPerTick = 0, int HotAmountPerTick = 0, int ShieldRemaining = 0, int ChargesRemaining = 0, int FreeCastsRemaining = 0);

/// <summary>A pending HoT or MoT tick. <see cref="IsMana"/> = true → mana regeneration instead of HP healing.</summary>
public readonly record struct HotTick(Ability Source, int Amount, bool IsMana = false);

/// <summary>
/// Active buffs of a character. Written by the simulation thread and read by the UI,
/// hence all accesses are lock-guarded.
/// </summary>
public partial class BuffTracker
{
    // Guard against floating-point drift (20 × 0.1 s doesn't give exactly 2.0)
    private const double Epsilon = 1e-9;

    private readonly Dictionary<string, ActiveBuff> _active = new();
    private readonly object _lock = new();
    // Pre-allocated helper lists for Tick() – avoid a Keys.ToList() allocation per step
    private readonly List<string> _expiredBuffs = new();
    private readonly List<(string Key, ActiveBuff Value)> _buffUpdates = new();
    // Pre-allocated for AbsorbDamage – no heap allocation per hit
    private readonly List<(Ability Source, int Absorbed)> _absorbLog = new();

    public bool IsActive(string abilityId)
    {
        lock (_lock) return _active.ContainsKey(abilityId);
    }

    /// <summary>Starts the buff or refreshes its duration. Pass <paramref name="maxMana"/> for MoT buffs, <paramref name="maxHp"/> for percentage HoT buffs.</summary>
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
            // Remove an old rank of the same spell so two ranks aren't active at the same time
            var oldKey = _active.Keys.FirstOrDefault(k => k != ability.Id && _active[k].Source.Name == ability.Name);
            if (oldKey is not null) _active.Remove(oldKey);
            _active[ability.Id] = new ActiveBuff(ability, ability.BuffDuration,
                MotAmountPerTick: motAmount, HotAmountPerTick: hotPctAmount, ShieldRemaining: shieldAmount,
                ChargesRemaining: ability.BuffHitCharges, FreeCastsRemaining: ability.BuffFreeCasts);
        }
    }

    /// <summary>
    /// Absorbs as much damage as possible from active shields.
    /// Returns the absorbed amount per shield source; the list is only valid until the next call.
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

    /// <summary>Advances all buffs by <paramref name="dt"/> and returns the HoT ticks that became due.</summary>
    public IReadOnlyList<HotTick> Tick(double dt)
    {
        List<HotTick>? ticks = null;

        lock (_lock)
        {
            _expiredBuffs.Clear();
            _buffUpdates.Clear();

            // Read first (without dictionary mutation), then apply changes
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

    /// <summary>Multiplier on all damage dealt (bonuses stack multiplicatively).</summary>
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

    /// <summary>True if an active buff grants immunity to damage (e.g. Divine Shield).</summary>
    public bool HasImmunity
    {
        get
        {
            lock (_lock)
                return _active.Values.Any(b => b.Source.BuffGrantsImmunity);
        }
    }

    /// <summary>Active buff with extra-attack chance (Windfury Weapon), otherwise null.</summary>
    public Ability? GetExtraAttackSource()
    {
        lock (_lock)
            return _active.Values.FirstOrDefault(b => b.Source.BuffExtraAttackChance > 0)?.Source;
    }

    /// <summary>Returns all buff sources that have retaliation damage (e.g. Thorns, Lightning Shield).</summary>
    public IReadOnlyList<Ability> GetRetaliationSources()
    {
        lock (_lock)
            return _active.Values
                .Where(b => b.Source.BuffRetaliationDamage > 0)
                .Select(b => b.Source)
                .ToList();
    }

    /// <summary>Rage generation bonus from active buffs (e.g. Battle Stance).</summary>
    public double RageGenBonus
    {
        get
        {
            lock (_lock)
                return _active.Values.Sum(b => b.Source.BuffRageGenBonus);
        }
    }

    /// <summary>Crit chance bonus from active buffs (e.g. Berserker Stance).</summary>
    public double CritChanceBonus
    {
        get
        {
            lock (_lock)
                return _active.Values.Sum(b => b.Source.BuffCritChanceBonus);
        }
    }

    /// <summary>Damage reduction bonus from active buffs (e.g. Defensive Stance).</summary>
    public double DamageTakenReduction
    {
        get
        {
            lock (_lock)
                return _active.Values.Sum(b => b.Source.BuffDamageTakenReduction);
        }
    }

    /// <summary>Sum of all shield points still absorbable (e.g. Ice Barrier).</summary>
    public int TotalShield
    {
        get
        {
            lock (_lock)
                return _active.Values.Sum(b => b.ShieldRemaining);
        }
    }

    /// <summary>Copy of the active buffs for display.</summary>
    public IReadOnlyList<ActiveBuff> Snapshot()
    {
        lock (_lock) return _active.Values.ToList();
    }
}
