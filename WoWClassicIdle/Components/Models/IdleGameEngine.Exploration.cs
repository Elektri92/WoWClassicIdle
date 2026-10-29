using System.Collections.Immutable;
using Models;

// Erkundung: Entdeckungs-XP beim ersten Betreten einer Zone, bekannte Flugpunkte und Zonen-Ziele („Zone Mastery“).
public partial class IdleGameEngine
{
    /// <summary>Zone schon betreten (= Flugpunkt bekannt). Ohne Roster-Charakter gilt alles als entdeckt.</summary>
    public bool IsZoneDiscovered(CombatZone zone) =>
        ActiveCharacter is not { } slot || slot.DiscoveredZoneIds.Contains(zone.Id);

    /// <summary>Entdeckungs-XP, die der aktive Charakter beim ersten Betreten bekäme.</summary>
    public int DiscoveryXpFor(CombatZone zone) => ZoneObjectiveRegistry.DiscoveryXp(zone, Player.Level);

    /// <summary>Fortschritt eines Zonen-Ziels für den aktiven Charakter (auf das Soll begrenzt).</summary>
    public int ObjectiveProgress(ZoneObjective objective) =>
        ActiveCharacter?.ZoneObjectiveProgress.GetValueOrDefault(objective.Id) is { } count
            ? Math.Min(count, objective.Required)
            : 0;

    public bool IsObjectiveDone(ZoneObjective objective) => ObjectiveProgress(objective) >= objective.Required;

    /// <summary>Anzahl erledigter Ziele der Zone.</summary>
    public int CompletedObjectives(CombatZone zone) => ZoneObjectiveRegistry.For(zone).Count(IsObjectiveDone);

    public bool IsZoneMastered(CombatZone zone) => CompletedObjectives(zone) == ZoneObjectiveRegistry.For(zone).Count;

    /// <summary>Zone als entdeckt markieren; mit <paramref name="grantXp"/> gibt es beim ersten Mal Entdeckungs-XP.</summary>
    private void DiscoverZone(CharacterSlot slot, CombatZone zone, bool grantXp)
    {
        if (slot.DiscoveredZoneIds.Contains(zone.Id)) return;
        slot.DiscoveredZoneIds = slot.DiscoveredZoneIds.Add(zone.Id);
        if (!grantXp || slot.Player != Player) return;

        int xp = ZoneObjectiveRegistry.DiscoveryXp(zone, Player.Level);
        bool levelUp = GrantXp(xp) > 0;
        LastLog = $"🗺 Discovered {zone.Name}: +{xp} XP, flight point learned"
                  + (levelUp ? $" – ⬆ Lv. {Player.Level}" : "");
    }

    /// <summary>Kill für die Ziele der aktuellen Zone zählen und erledigte Ziele belohnen.</summary>
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

    // Ziel erledigt: XP + Kupfer; letztes Ziel der Zone → Mastery-Item
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

    // Garantiertes klassenpassendes Item auf Belohnungsstufe – mit etwas Glück blau
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

    /// <summary>Entdeckte Zonen und Ziel-Fortschritt aus dem Spielstand (Altstand: aktuelle Zone + Reise-Start).</summary>
    private static void RestoreExploration(CharacterSlot slot, SlotSaveData data)
    {
        var discovered = data.DiscoveredZones ?? [data.CombatZone ?? ""];
        slot.DiscoveredZoneIds = discovered.Where(id => CombatZoneRegistry.Get(id) is not null).ToImmutableHashSet();

        if (data.ZoneObjectives is { } progress)
            slot.ZoneObjectiveProgress = progress.Where(p => p.Value > 0).ToImmutableDictionary();
    }
}
