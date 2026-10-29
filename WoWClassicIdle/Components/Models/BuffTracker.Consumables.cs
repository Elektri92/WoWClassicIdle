// Models/BuffTracker.Consumables.cs
namespace Models;

// Buffs that are consumed before their duration ends: hit charges, guaranteed crit, empowerments.
public partial class BuffTracker
{
    /// <summary>An enemy hit taken consumes one charge of every charge buff (Holy Shield).</summary>
    public void ConsumeHitCharges()
    {
        lock (_lock)
        {
            foreach (var key in _active.Keys.ToList())
            {
                var buff = _active[key];
                if (buff.Source.BuffHitCharges <= 0) continue;
                if (buff.ChargesRemaining <= 1)
                    _active.Remove(key);
                else
                    _active[key] = buff with { ChargesRemaining = buff.ChargesRemaining - 1 };
            }
        }
    }

    /// <summary>Consumes a guaranteed-crit buff (Cold Blood). True if one was active.</summary>
    public bool ConsumeGuaranteedCrit()
    {
        lock (_lock)
        {
            var key = _active.FirstOrDefault(kvp => kvp.Value.Source.BuffGuaranteesCrit).Key;
            if (key is null) return false;
            _active.Remove(key);
            return true;
        }
    }

    /// <summary>
    /// Consumes all empower buffs for the ability <paramref name="abilityName"/>
    /// and returns the damage multiplier (1.0 = no empowerment).
    /// </summary>
    public double ConsumeEmpower(string abilityName)
    {
        lock (_lock)
        {
            double mult = 1.0;
            foreach (var key in _active.Keys.ToList())
            {
                var src = _active[key].Source;
                if (!src.IsBuff || !src.IsEmpowering || src.EmpowersAbilityName != abilityName) continue;
                mult *= 1.0 + src.EmpowerDamageBonus;
                _active.Remove(key);
            }
            return mult;
        }
    }
}
