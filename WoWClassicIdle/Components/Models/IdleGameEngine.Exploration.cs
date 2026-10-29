using System.Collections.Immutable;
using Models;

// Exploration: discovery XP when entering a zone for the first time, known flight points and zone objectives ("Zone Mastery").
public partial class IdleGameEngine
{
    /// <summary>Zone already visited (= flight point known). Without a roster character everything counts as discovered.</summary>
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

    public bool IsZoneMastered(CombatZone zone) => CompletedObjectives(zone) == ZoneObjectiveRegistry.For(zone).Count;

    /// <summary>Mark the zone as discovered; with <paramref name="grantXp"/> there is discovery XP the first time.</summary>
    private void DiscoverZone(CharacterSlot slot, CombatZone zone, bool grantXp)
    {
        if (slot.DiscoveredZoneIds.Contains(zone.Id)) return;
        slot.DiscoveredZoneIds = slot.DiscoveredZoneIds.Add(zone.Id);
        if (!grantXp) return;

        if (slot.Player != Player)
        {
            // Arrived in the background as an inactive character: XP without unlock notifications
            int slotXp = ZoneObjectiveRegistry.DiscoveryXp(zone, slot.Player.Level);
            slot.Player.GainXp(slotXp);
            LastLog = $"🗺 {slot.Name} discovered {zone.Name}: +{slotXp} XP, flight point learned";
            return;
        }

        int xp = ZoneObjectiveRegistry.DiscoveryXp(zone, Player.Level);
        bool levelUp = GrantXp(xp) > 0;
        LastLog = $"🗺 Discovered {zone.Name}: +{xp} XP, flight point learned"
                  + (levelUp ? $" – ⬆ Lv. {Player.Level}" : "");
    }

    /// <summary>Count a kill for the objectives of the current zone and reward completed objectives.</summary>
    private void TrackZoneObjectives(Enemy dead)
    {
        if (ActiveCharacter is not { } slot || IsTraveling || CurrentCombatZone is not { } zone) return;

        foreach (var objective in ZoneObjectiveRegistry.For(zone))
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
}
