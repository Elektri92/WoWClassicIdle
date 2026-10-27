using Models;

// Kampfgebiete: Zonenwahl je Charakter, Reisen zwischen Zonen, Gegner-Spawn aus der aktiven Zone.
public partial class IdleGameEngine
{
    private double _travelRegenAcc;

    /// <summary>
    /// Zone, in der gekämpft wird. Ohne Roster-Charakter (Headless-Simulation) automatisch passend zum Level.
    /// </summary>
    public CombatZone? CurrentCombatZone => ActiveCharacter is { } slot
        ? CombatZoneRegistry.Get(slot.CombatZoneId)
        : CombatZoneRegistry.AutoZoneFor(Player.Level);

    /// <summary>Der aktive Charakter hat noch nie eine Zone gewählt – vorher wird nicht gekämpft.</summary>
    public bool NeedsCombatZone => ActiveCharacter is { CombatZoneId: null };

    public ZoneTravelState? Travel => ActiveCharacter?.Travel;
    public bool IsTraveling => Travel is not null;

    /// <summary>Bodenreise des aktiven Charakters (Laufen, Gestalt, Reittier).</summary>
    public GroundTravel GroundTravel => ZoneTravel.GroundFor(PlayerClass, Race, Player.Level);

    /// <summary>Route von der aktuellen Zone in <paramref name="target"/>; null ohne Startzone (erste Wahl ist sofort).</summary>
    public TravelRoute? RouteTo(CombatZone target) =>
        CurrentCombatZone is { } from && ActiveCharacter is not null
            ? ZoneTravel.Route(from, target, Faction, GroundTravel)
            : null;

    /// <summary>Grund, warum die Zone nicht gewählt werden kann (null = wählbar).</summary>
    public string? CombatZoneBlocker(CombatZone zone)
    {
        if (!CombatZoneRegistry.CanEnter(zone, Player.Level)) return $"Requires level {zone.MinLevel}";
        if (IsTraveling) return "You are already traveling";
        if (zone.Id == ActiveCharacter?.CombatZoneId) return "You are already here";
        return null;
    }

    /// <summary>
    /// Wählt das Kampfgebiet des aktiven Charakters. Die erste Wahl ist sofort aktiv, danach muss gereist werden.
    /// </summary>
    public bool SelectCombatZone(string zoneId)
    {
        lock (_simLock)
        {
            if (ActiveCharacter is not { } slot || CombatZoneRegistry.Get(zoneId) is not { } zone) return false;
            if (CombatZoneBlocker(zone) is not null) return false;

            if (CombatZoneRegistry.Get(slot.CombatZoneId) is not { } from)
            {
                slot.CombatZoneId = zone.Id;
                LeaveCombat();
                LastLog = $"🗺 Entered {zone.Name}";
            }
            else
            {
                slot.Travel = new ZoneTravelState
                {
                    FromZoneId   = from.Id,
                    TargetZoneId = zone.Id,
                    Route        = ZoneTravel.Route(from, zone, slot.Faction, GroundTravel),
                };
                LeaveCombat();
                LastLog = $"🧭 Traveling to {zone.Name} ({ZoneTravel.FormatDuration(slot.Travel.Duration)})";
            }
        }
        TickUpdated?.Invoke();
        return true;
    }

    /// <summary>Reise abbrechen – der Charakter kehrt sofort in seine bisherige Zone zurück.</summary>
    public void CancelTravel()
    {
        lock (_simLock)
        {
            if (ActiveCharacter is not { Travel: not null } slot) return;
            slot.Travel = null;
            LastLog = $"↩ Turned back to {CombatZoneRegistry.Get(slot.CombatZoneId)?.Name}";
        }
        TickUpdated?.Invoke();
    }

    /// <summary>Gegner aus der aktuellen Zone – null ohne Zone oder während einer Reise.</summary>
    private Enemy? SpawnEnemy() =>
        !IsTraveling && CurrentCombatZone is { } zone ? _spawner.Spawn(Player.Stats.Level, zone) : null;

    // Kampf verlassen: Gegner verschwindet, laufender Cast und Swing-Timer verfallen
    private void LeaveCombat()
    {
        CurrentEnemy      = null;
        _pendingRareEnemy = null;
        CancelCast();
        _playerAttackAcc = 0;
        _enemyAttackAcc  = 0;
    }

    /// <summary>Reisefortschritt für einen Schritt; true = Charakter ist unterwegs (kein Kampf).</summary>
    private bool AdvanceTravel(double dt)
    {
        if (ActiveCharacter is not { Travel: { } travel } slot) return false;

        travel.Elapsed += dt;
        RegenWhileTraveling(dt);
        if (!travel.IsArrived) return true;

        slot.CombatZoneId = travel.TargetZoneId;
        slot.Travel = null;
        LastLog = $"🗺 Arrived in {CombatZoneRegistry.Get(travel.TargetZoneId)?.Name}";
        return true;
    }

    // Unterwegs erholt sich der Charakter wie außerhalb des Kampfes (HP + Mana alle 5 s)
    private void RegenWhileTraveling(double dt)
    {
        _travelRegenAcc += dt;
        while (_travelRegenAcc >= 5.0)
        {
            _travelRegenAcc -= 5.0;
            Player.RegenHp(Player.Stats.HpRegenPer5);
            if (Player.Resource == ResourceKind.Mana)
                Player.RegenMana(Player.Stats.ManaRegenPer5);
        }
    }

    /// <summary>Zone und Reise eines Charakters aus dem Spielstand übernehmen (ungültige Werte werden verworfen).</summary>
    private static void RestoreCombatZone(CharacterSlot slot, SlotSaveData data)
    {
        if (CombatZoneRegistry.Get(data.CombatZone) is not { } zone) return;
        slot.CombatZoneId = zone.Id;

        if (CombatZoneRegistry.Get(data.TravelTarget) is not { } target || target.Id == zone.Id) return;
        var ground = ZoneTravel.GroundFor(slot.Class, slot.Race, slot.Player.Level);
        slot.Travel = new ZoneTravelState
        {
            FromZoneId   = zone.Id,
            TargetZoneId = target.Id,
            Route        = ZoneTravel.Route(zone, target, slot.Faction, ground),
            Elapsed      = Math.Max(0, data.TravelElapsed),
        };
    }
}
