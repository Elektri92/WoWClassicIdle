// Models/BuffTracker.SpecMechanics.cs
namespace Models;

// Buff totals and consumption for the specialization abilities (auras, free/instant casts, healing on hits).
public partial class BuffTracker
{
    /// <summary>Healing bonus from active buffs (e.g. Holy Aura, Spirit of Redemption).</summary>
    public double HealingBonus
    {
        get
        {
            lock (_lock)
                return _active.Values.Sum(b => b.Source.BuffHealingBonus);
        }
    }

    /// <summary>Bonus on mana and energy regeneration (e.g. Adrenaline Rush, Aspect of the Owl).</summary>
    public double ResourceRegenBonus
    {
        get
        {
            lock (_lock)
                return _active.Values.Sum(b => b.Source.BuffResourceRegenBonus);
        }
    }

    /// <summary>Pet damage bonus from active buffs.</summary>
    public double PetDamageBonus
    {
        get
        {
            lock (_lock)
                return _active.Values.Sum(b => b.Source.BuffPetDamageBonus);
        }
    }

    /// <summary>Additional damage share absorbed by the pet (Soul Link).</summary>
    public double PetDamageShareBonus
    {
        get
        {
            lock (_lock)
                return _active.Values.Sum(b => b.Source.BuffPetDamageShareBonus);
        }
    }

    /// <summary>Active buff that heals on hits taken (Earth Shield), otherwise null.</summary>
    public Ability? GetHealOnHitSource()
    {
        lock (_lock)
            return _active.Values.FirstOrDefault(b => b.Source.BuffHealOnHit > 0)?.Source;
    }

    /// <summary>True if a buff makes the next ability free (Vanish, Spirit of Redemption).</summary>
    public bool HasFreeCast
    {
        get
        {
            lock (_lock)
                return _active.Values.Any(b => b.FreeCastsRemaining > 0);
        }
    }

    /// <summary>Consumes a free charge. The buff itself keeps running (Vanish stays immune).</summary>
    public bool ConsumeFreeCast()
    {
        lock (_lock)
        {
            var key = _active.FirstOrDefault(kvp => kvp.Value.FreeCastsRemaining > 0).Key;
            if (key is null) return false;
            var buff = _active[key];
            _active[key] = buff with { FreeCastsRemaining = buff.FreeCastsRemaining - 1 };
            return true;
        }
    }

    /// <summary>Consumes an instant-cast buff for <paramref name="abilityName"/> (Shadow Trance → Shadow Bolt).</summary>
    public bool ConsumeInstantCast(string abilityName)
    {
        lock (_lock)
        {
            var key = _active.FirstOrDefault(kvp => kvp.Value.Source.BuffInstantCastName == abilityName).Key;
            if (key is null) return false;
            _active.Remove(key);
            return true;
        }
    }

    /// <summary>Damage multiplier of the empowerments for <paramref name="abilityName"/> without consuming them (cast selection).</summary>
    public double PeekEmpower(string abilityName)
    {
        lock (_lock)
            return _active.Values
                .Where(b => b.Source.IsEmpowering && b.Source.EmpowersAbilityName == abilityName)
                .Aggregate(1.0, (m, b) => m * (1.0 + b.Source.EmpowerDamageBonus));
    }

    /// <summary>Removes auras whose ability is no longer equipped (loadout change).</summary>
    public void RemoveAurasExcept(IReadOnlyList<Ability> equipped)
    {
        lock (_lock)
        {
            foreach (var key in _active.Keys.ToList())
                if (_active[key].Source.IsAura && !equipped.Any(a => a.Id == key))
                    _active.Remove(key);
        }
    }
}
