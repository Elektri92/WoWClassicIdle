// Models/BuffTracker.SpecMechanics.cs
namespace Models;

// Buff-Summen und Verbrauch für die Spezialisierungs-Fähigkeiten (Auren, Gratis-/Sofort-Zauber, Heilung bei Treffern).
public partial class BuffTracker
{
    /// <summary>Heilungsbonus aus aktiven Buffs (z. B. Holy Aura, Spirit of Redemption).</summary>
    public double HealingBonus
    {
        get
        {
            lock (_lock)
                return _active.Values.Sum(b => b.Source.BuffHealingBonus);
        }
    }

    /// <summary>Bonus auf Mana- und Energie-Regeneration (z. B. Adrenaline Rush, Aspect of the Owl).</summary>
    public double ResourceRegenBonus
    {
        get
        {
            lock (_lock)
                return _active.Values.Sum(b => b.Source.BuffResourceRegenBonus);
        }
    }

    /// <summary>Schadensbonus des Begleittiers aus aktiven Buffs.</summary>
    public double PetDamageBonus
    {
        get
        {
            lock (_lock)
                return _active.Values.Sum(b => b.Source.BuffPetDamageBonus);
        }
    }

    /// <summary>Zusätzlicher Schadensanteil, den das Begleittier abfängt (Soul Link).</summary>
    public double PetDamageShareBonus
    {
        get
        {
            lock (_lock)
                return _active.Values.Sum(b => b.Source.BuffPetDamageShareBonus);
        }
    }

    /// <summary>Aktiver Buff, der bei erlittenen Treffern heilt (Earth Shield), sonst null.</summary>
    public Ability? GetHealOnHitSource()
    {
        lock (_lock)
            return _active.Values.FirstOrDefault(b => b.Source.BuffHealOnHit > 0)?.Source;
    }

    /// <summary>True, wenn ein Buff die nächste Fähigkeit kostenlos macht (Vanish, Spirit of Redemption).</summary>
    public bool HasFreeCast
    {
        get
        {
            lock (_lock)
                return _active.Values.Any(b => b.FreeCastsRemaining > 0);
        }
    }

    /// <summary>Verbraucht eine Gratis-Ladung. Der Buff selbst läuft weiter (Vanish bleibt immun).</summary>
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

    /// <summary>Verbraucht einen Sofort-Zauber-Buff für <paramref name="abilityName"/> (Shadow Trance → Shadow Bolt).</summary>
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

    /// <summary>Schadensmultiplikator der Verstärkungen für <paramref name="abilityName"/>, ohne sie zu verbrauchen (Cast-Auswahl).</summary>
    public double PeekEmpower(string abilityName)
    {
        lock (_lock)
            return _active.Values
                .Where(b => b.Source.IsEmpowering && b.Source.EmpowersAbilityName == abilityName)
                .Aggregate(1.0, (m, b) => m * (1.0 + b.Source.EmpowerDamageBonus));
    }

    /// <summary>Entfernt Auren, deren Fähigkeit nicht mehr ausgerüstet ist (Loadout-Wechsel).</summary>
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
