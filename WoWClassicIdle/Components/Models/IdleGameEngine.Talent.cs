using Models;

// Neue Systeme als eigene Partial-Datei anlegen; Dateien möglichst < 300 Zeilen halten.
public partial class IdleGameEngine
{
    // ─── Debug-Helfer ───

    /// <summary>
    /// Legt ein Rüstungsteil direkt am aktiven Charakter an und feuert das ItemDropped-Event (Debug-Seiten).
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
    /// Legt eine Waffe direkt am aktiven Charakter an und feuert das ItemDropped-Event (Debug-Seiten).
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

    /// <summary>Investiert einen Talent-Punkt für den aktiven Charakter. Gibt true zurück wenn erfolgreich.</summary>
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
            // Talent-freigeschaltete Abilities (z. B. Warrior-Stances) dem Spieler mitteilen
            if (!string.IsNullOrEmpty(talent.GrantsAbilityId))
            {
                var granted = ClassAbilityRegistry.FindById(talent.GrantsAbilityId);
                if (granted is not null)
                    AbilityUnlocked?.Invoke(granted);
            }
        }
        return ok;
    }

    /// <summary>Setzt alle Talente der aktiven Klasse zurück (Respek).</summary>
    public void ResetTalents(CharacterClass cls)
    {
        lock (_simLock)
        {
            Player.ResetTalents(cls);
        }
        TickUpdated?.Invoke();
    }
}
