// Models/BuffTracker.Consumables.cs
namespace Models;

// Buffs, die sich vor Ablauf ihrer Laufzeit verbrauchen: Treffer-Ladungen, garantierter Crit, Verstärkungen.
public partial class BuffTracker
{
    /// <summary>Ein erlittener Gegnertreffer verbraucht je eine Ladung aller Ladungs-Buffs (Holy Shield).</summary>
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

    /// <summary>Verbraucht einen Garantiert-Crit-Buff (Cold Blood). True, wenn einer aktiv war.</summary>
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
    /// Verbraucht alle Verstärkungs-Buffs für die Fähigkeit <paramref name="abilityName"/>
    /// und gibt den Schadensmultiplikator zurück (1.0 = keine Verstärkung).
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
