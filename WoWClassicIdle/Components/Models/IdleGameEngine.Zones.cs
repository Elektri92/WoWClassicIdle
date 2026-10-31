using Models;

// Combat zones: zone choice per character, travel between zones, enemy spawn from the active zone.
public partial class IdleGameEngine
{
    /// <summary>
    /// Zone being fought in. Without a roster character (headless simulation) automatically matched to the level.
    /// </summary>
    public CombatZone? CurrentCombatZone => ActiveCharacter is { } slot
        ? CombatZoneRegistry.Get(slot.CombatZoneId)
        : CombatZoneRegistry.AutoZoneFor(Player.Level);

    /// <summary>The active character has never chosen a zone – no fighting before that.</summary>
    public bool NeedsCombatZone => ActiveCharacter is { CombatZoneId: null };

    public ZoneTravelState? Travel => ActiveCharacter?.Travel;
    public bool IsTraveling => Travel is not null;

    /// <summary>Travel of the played character, unaffected by a temporary switch in the profession windows.</summary>
    public ZoneTravelState? CombatCharacterTravel => CombatSlotId is { } id ? GetCharacter(id)?.Travel : null;

    /// <summary>Ground travel of the active character (walking, form, mount).</summary>
    public GroundTravel GroundTravel => ZoneTravel.GroundFor(PlayerClass, Player.Level, EquippedMount);

    /// <summary>Route from the current zone to <paramref name="target"/>; null without a start zone (the first choice is immediate).</summary>
    public TravelRoute? RouteTo(CombatZone target) =>
        CurrentCombatZone is { } from && ActiveCharacter is { } slot
            ? ZoneTravel.Route(from, target, Faction, GroundTravel, slot.DiscoveredZoneIds)
            : null;

    /// <summary>Reason why the zone cannot be chosen (null = selectable).</summary>
    public string? CombatZoneBlocker(CombatZone zone)
    {
        if (!CombatZoneRegistry.CanEnter(zone, Player.Level)) return $"Requires level {zone.MinLevel}";
        if (IsTraveling) return "You are already traveling";
        if (zone.Id == ActiveCharacter?.CombatZoneId) return "You are already here";
        return null;
    }

    /// <summary>
    /// Chooses the combat zone of the active character. The first choice is active immediately, afterwards travel is required.
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
                DiscoverZone(slot, zone, grantXp: true);
            }
            else
            {
                slot.Travel = new ZoneTravelState
                {
                    FromZoneId   = from.Id,
                    TargetZoneId = zone.Id,
                    Route        = ZoneTravel.Route(from, zone, slot.Faction, GroundTravel, slot.DiscoveredZoneIds),
                };
                LeaveCombat();
                LastLog = $"🧭 Traveling to {zone.Name} ({ZoneTravel.FormatDuration(slot.Travel.Duration)})";
            }
        }
        TickUpdated?.Invoke();
        return true;
    }

    /// <summary>Cancel the travel – the character immediately returns to its previous zone.</summary>
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

    /// <summary>Enemy from the current zone – null without a zone or during travel.</summary>
    private Enemy? SpawnEnemy() =>
        !IsTraveling && CurrentCombatZone is { } zone ? _spawner.Spawn(Player.Stats.Level, zone) : null;

    // Leave combat: the enemy disappears, the running cast and swing timer are discarded
    private void LeaveCombat()
    {
        CurrentEnemy      = null;
        _pendingRareEnemy = null;
        CancelCast();
        _playerAttackAcc = 0;
        _enemyAttackAcc  = 0;
    }

    /// <summary>Travel progress of the active character for one combat step; true = character is traveling (no combat).</summary>
    private bool AdvanceTravel(double dt) => ActiveCharacter is { } slot && AdvanceTravel(slot, dt);

    /// <summary>Travel progress of any character; true = it was traveling during this step.</summary>
    private bool AdvanceTravel(CharacterSlot slot, double dt)
    {
        if (slot.Travel is not { } travel) return false;

        travel.Elapsed += dt;
        RegenWhileTraveling(slot.Player, travel, dt);
        if (!travel.IsArrived) return true;

        slot.CombatZoneId = travel.TargetZoneId;
        slot.Travel = null;
        var arrived = CombatZoneRegistry.Get(travel.TargetZoneId);
        LastLog = slot.Player == Player
            ? $"🗺 Arrived in {arrived?.Name}"
            : $"🗺 {slot.Name} arrived in {arrived?.Name}";
        if (arrived is not null)
            DiscoverZone(slot, arrived, grantXp: true);
        return true;
    }

    // While traveling the character recovers like out of combat (HP + mana every 5 s)
    private static void RegenWhileTraveling(Player player, ZoneTravelState travel, double dt)
    {
        travel.RegenAcc += dt;
        while (travel.RegenAcc >= 5.0)
        {
            travel.RegenAcc -= 5.0;
            player.RegenHp(player.Stats.HpRegenPer5);
            if (player.Resource == ResourceKind.Mana)
                player.RegenMana(player.Stats.ManaRegenPer5);
        }
    }

    /// <summary>Take over zone and travel of a character from the save game (invalid values are discarded).</summary>
    private static void RestoreCombatZone(CharacterSlot slot, SlotSaveData data)
    {
        if (CombatZoneRegistry.Get(data.CombatZone) is not { } zone) return;
        slot.CombatZoneId = zone.Id;

        if (CombatZoneRegistry.Get(data.TravelTarget) is not { } target || target.Id == zone.Id) return;
        var ground = ZoneTravel.GroundFor(slot.Class, slot.Player.Level, MountOf(slot));
        slot.Travel = new ZoneTravelState
        {
            FromZoneId   = zone.Id,
            TargetZoneId = target.Id,
            Route        = ZoneTravel.Route(zone, target, slot.Faction, ground, slot.DiscoveredZoneIds),
            Elapsed      = Math.Max(0, data.TravelElapsed),
        };
    }
}
