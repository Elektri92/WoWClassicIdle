using System.Collections.Immutable;
using Models;

// Exploration: discovery XP when entering a zone for the first time, known flight points and zone objectives ("Zone Mastery").
public partial class IdleGameEngine
{
    // Account-wide; immutable: the sim thread replaces the set completely, the UI reads without a lock
    private ImmutableHashSet<string> _flightPoints = ImmutableHashSet<string>.Empty;

    /// <summary>Number of flight points the account has learned (zones and dungeons).</summary>
    public int FlightPointCount => _flightPoints.Count;

    /// <summary>Any character of the account has already been here.</summary>
    public bool IsFlightPointKnown(CombatZone zone) => _flightPoints.Contains(zone.Id);

    /// <summary>Zone already visited by the active character. Without a roster character everything counts as discovered.</summary>
    public bool IsZoneDiscovered(CombatZone zone) =>
        ActiveCharacter is not { } slot || slot.DiscoveredZoneIds.Contains(zone.Id);

    /// <summary>Discovery XP the active character would get on the first visit.</summary>
    public int DiscoveryXpFor(CombatZone zone) => ZoneObjectiveRegistry.DiscoveryXp(zone, Player.Level);

    /// <summary>Progress of a zone objective for the active character (clamped to the target).</summary>
    public int ObjectiveProgress(ZoneObjective objective) =>
        ActiveCharacter?.ZoneObjectiveProgress.GetValueOrDefault(objective.Id) is { } count
            ? Math.Min(count, objective.Required)
            : 0;

    public bool IsObjectiveDone(ZoneObjective objective) => ObjectiveProgress(objective) >= objective.Required;

    /// <summary>Number of completed objectives of the zone.</summary>
    public int CompletedObjectives(CombatZone zone) => ZoneObjectiveRegistry.For(zone).Count(IsObjectiveDone);

    public bool IsZoneMastered(CombatZone zone) => !zone.IsDungeon && CompletedObjectives(zone) == ZoneObjectiveRegistry.For(zone).Count;

    /// <summary>All objectives done and all drops of the zone found (drops are account-wide).</summary>
    public bool IsZoneCompleted(CombatZone zone) =>
        IsZoneMastered(zone) && ProfessionBonusRegistry.ForZone(zone).All(OwnsProfessionBonus);

    /// <summary>Zone is below the active character's level range: only its profession drops still matter.</summary>
    public bool IsZoneOutgrown(CombatZone zone) => zone.IsOutgrownAt(Player.Level);

    /// <summary>Nothing left to do here: completed, or outgrown with all of its drops found.</summary>
    public bool IsZoneFinished(CombatZone zone) =>
        !zone.IsDungeon && (IsZoneCompleted(zone)
                            || IsZoneOutgrown(zone) && ProfessionBonusRegistry.ForZone(zone).All(OwnsProfessionBonus));

    /// <summary>Unlocks the flight point for the whole account; true = it was not known yet.</summary>
    private bool LearnFlightPoint(CombatZone zone)
    {
        if (_flightPoints.Contains(zone.Id)) return false;
        _flightPoints = _flightPoints.Add(zone.Id);
        return true;
    }

    /// <summary>Mark the zone as discovered; with <paramref name="grantXp"/> there is discovery XP the first time.</summary>
    private void DiscoverZone(CharacterSlot slot, CombatZone zone, bool grantXp)
    {
        string flightNote = LearnFlightPoint(zone) ? ", flight point learned" : "";
        if (slot.DiscoveredZoneIds.Contains(zone.Id)) return;
        slot.DiscoveredZoneIds = slot.DiscoveredZoneIds.Add(zone.Id);
        // Outgrown zones are still marked as discovered, but give no XP anymore
        if (!grantXp || zone.IsOutgrownAt(slot.Player.Level)) return;

        if (slot.Player != Player)
        {
            // Arrived in the background as an inactive character: XP without unlock notifications
            int slotXp = ZoneObjectiveRegistry.DiscoveryXp(zone, slot.Player.Level);
            slot.Player.GainXp(slotXp);
            LastLog = $"🗺 {slot.Name} discovered {zone.Name}: +{slotXp} XP{flightNote}";
            return;
        }

        int xp = ZoneObjectiveRegistry.DiscoveryXp(zone, Player.Level);
        bool levelUp = GrantXp(xp) > 0;
        LastLog = $"🗺 Discovered {zone.Name}: +{xp} XP{flightNote}"
                  + (levelUp ? $" – ⬆ Lv. {Player.Level}" : "");
    }

    /// <summary>Count a kill for the objectives of the current zone and reward completed objectives.</summary>
    private void TrackZoneObjectives(Enemy dead)
    {
        if (ActiveCharacter is not { } slot || IsTraveling || CurrentCombatZone is not { } zone) return;
        // Objectives of outgrown zones can no longer be done
        IReadOnlyList<ZoneObjective> objectives = IsZoneOutgrown(zone) ? [] : ZoneObjectiveRegistry.For(zone);
        foreach (var objective in objectives)
        {
            bool counts = objective.Kind switch
            {
                ZoneObjectiveKind.SlayMob  => dead.MobName == objective.MobName,
                ZoneObjectiveKind.SlayRare => dead.IsRare,
                _                          => false,
            };
            int before = slot.ZoneObjectiveProgress.GetValueOrDefault(objective.Id);
            if (!counts || before >= objective.Required) continue;

            slot.ZoneObjectiveProgress = slot.ZoneObjectiveProgress.SetItem(objective.Id, before + 1);
            if (before + 1 >= objective.Required)
                CompleteObjective(zone, objective);
        }

        // Checked on every kill: drops are account-wide, so another character may have found the last one
        if (IsZoneCompleted(zone))
            RequestZoneCompletedPopup(slot, zone);
    }

    // Objective completed: XP + copper; last objective of the zone → mastery item
    private void CompleteObjective(CombatZone zone, ZoneObjective objective)
    {
        int xp = ZoneObjectiveRegistry.ObjectiveXp(zone, Player.Level);
        long copper = ZoneObjectiveRegistry.ObjectiveCopper(zone, Player.Level);
        Player.Copper += copper;
        bool levelUp = GrantXp(xp) > 0;

        LastLog = $"📜 {objective.Label} complete: +{xp} XP, {Money.Format(copper)}"
                  + (levelUp ? $" – ⬆ Lv. {Player.Level}" : "");

        if (IsZoneMastered(zone))
            GrantMasteryReward(zone);
    }

    // Guaranteed class-appropriate item at reward tier – blue with a bit of luck
    private void GrantMasteryReward(CombatZone zone)
    {
        int level = ZoneObjectiveRegistry.RewardLevel(zone, Player.Level);
        var blue  = RareLootRegistry.GetBlueLootTable(level).Where(Useful).ToList();
        var green = RareLootRegistry.GetLootTable(level).Where(Useful).ToList();
        var table = blue.Count > 0 && _rng.NextDouble() < ZoneObjectiveRegistry.MasteryBlueChance ? blue : green;

        LastLog = $"🏆 {zone.Name} mastered!";
        if (table.Count == 0) return;

        var item = table[_rng.Next(table.Count)] with { RequiredLevel = level };
        AddLootOrSell(item);
        LastLog = $"🏆 {zone.Name} mastered! Reward: {item.Name}";

        bool Useful(ArmorItem i) => RareLootRegistry.IsUsefulForClass(i.Bonuses, Player.ActiveClass);
    }

    /// <summary>Discovered zones and objective progress from the save game (legacy: current zone + travel start).</summary>
    private static void RestoreExploration(CharacterSlot slot, SlotSaveData data)
    {
        var discovered = data.DiscoveredZones ?? [data.CombatZone ?? ""];
        slot.DiscoveredZoneIds = discovered.Where(id => CombatZoneRegistry.Get(id) is not null).ToImmutableHashSet();

        if (data.ZoneObjectives is { } progress)
            slot.ZoneObjectiveProgress = progress.Where(p => p.Value > 0).ToImmutableDictionary();
    }

    internal List<string> BuildFlightPointSaveData() => _flightPoints.Order().ToList();

    /// <summary>Account-wide flight points from the save game (legacy: everything the characters have visited, cleared dungeons included).</summary>
    private void RestoreFlightPoints(SaveData save)
    {
        var known = save.FlightPoints ?? save.Roster
            .SelectMany(s => (s.DiscoveredZones ?? [])
                .Concat(s.DungeonClears?.Keys.AsEnumerable() ?? [])
                .Append(s.CombatZone ?? ""))
            .ToList();
        _flightPoints = known.Where(id => CombatZoneRegistry.Get(id) is not null).ToImmutableHashSet();
    }
}
