using Models;

// Create new systems as their own partial file; keep files below 300 lines where possible.
public partial class IdleGameEngine
{
    // ─── Debug-Helfer ───

    /// <summary>
    /// Equips an armor piece directly on the active character and fires the ItemDropped event (debug pages).
    /// </summary>
    public void DebugGiveArmor(ArmorItem item)
    {
        lock (_simLock)
        {
            Player.ForceEquipArmor(item);
        }
        ItemDropped?.Invoke(item);
        TickUpdated?.Invoke();
    }

    /// <summary>
    /// Equips a weapon directly on the active character and fires the ItemDropped event (debug pages).
    /// </summary>
    public void DebugGiveWeapon(WeaponItem weapon)
    {
        lock (_simLock)
        {
            Player.ForceEquipWeapon(weapon);
        }
        ItemDropped?.Invoke(weapon);
        TickUpdated?.Invoke();
    }

    // ─── Talente ───

    /// <summary>Invests a talent point for the active character. Returns true if successful.</summary>
    public bool InvestTalentPoint(string talentId)
    {
        var talent = TalentRegistry.GetTalents(Player.ActiveClass)
            .FirstOrDefault(t => t.Id == talentId);
        if (talent is null) return false;
        bool ok;
        lock (_simLock)
        {
            ok = Player.InvestTalent(talent);
        }
        if (ok)
        {
            TickUpdated?.Invoke();
            // Notify the player of talent-unlocked abilities (e.g. Warrior stances, Druid forms)
            foreach (var abilityId in talent.GrantedAbilityIds)
            {
                var granted = ClassAbilityRegistry.FindById(abilityId);
                if (granted is not null)
                    AbilityUnlocked?.Invoke(granted);
            }
        }
        return ok;
    }

    /// <summary>
    /// Applies a changed loadout to the running fight (thread-safe).
    /// Druid forms switch resource and stats in the process.
    /// </summary>
    public void ApplyLoadout(ClassLoadout loadout)
    {
        lock (_simLock)
        {
            Player.ApplyLoadout(loadout);
        }
    }

    /// <summary>Resets all talents of the active class (respec).</summary>
    public void ResetTalents(CharacterClass cls)
    {
        lock (_simLock)
        {
            Player.ResetTalents(cls);
        }
        TickUpdated?.Invoke();
    }
}
