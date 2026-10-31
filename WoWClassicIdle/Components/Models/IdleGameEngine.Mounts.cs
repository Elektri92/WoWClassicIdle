using Models;

// Riding: learned per character from the trainer on /inventory; the mount itself follows the race of the character.
public partial class IdleGameEngine
{
    public int RidingRank => ActiveCharacter?.RidingRank ?? 0;

    public Mount? EquippedMount => ActiveCharacter is { } slot ? MountOf(slot) : null;

    /// <summary>Reason why the riding rank cannot be learned (UI text) – <c>null</c> if allowed.</summary>
    public string? RidingBlocker(RidingSkill skill)
    {
        if (ActiveCharacter is not { } slot) return "You have no character.";
        if (slot.RidingRank >= skill.Rank) return "Already known";
        if (slot.RidingRank < skill.Rank - 1)
            return $"Requires {MountRegistry.Skills.First(s => s.Rank == skill.Rank - 1).Name}";
        if (Player.Level < skill.RequiredLevel) return $"Requires level {skill.RequiredLevel}";
        if (Player.Copper < skill.Cost) return "You don't have enough money.";
        return null;
    }

    /// <summary>Buys a riding rank and rides the new mount right away. Returns an English error message on failure, otherwise <c>null</c>.</summary>
    public string? LearnRiding(RidingSkill skill)
    {
        lock (_simLock)
        {
            if (RidingBlocker(skill) is { } blocker) return blocker;
            var slot = ActiveCharacter!;
            Player.Copper -= skill.Cost;
            slot.RidingRank = skill.Rank;
            SetEquippedMount(slot, skill.Rank);
            LastLog = $"🐎 Learned {skill.Name}";
        }
        TickUpdated?.Invoke();
        return null;
    }

    /// <summary>Rides a learned mount (<paramref name="rank"/> = rank of the mount, 0 = on foot).</summary>
    public void EquipMount(int rank)
    {
        lock (_simLock)
        {
            if (ActiveCharacter is not { } slot || rank < 0 || rank > slot.RidingRank) return;
            SetEquippedMount(slot, rank);
        }
        TickUpdated?.Invoke();
    }

    private void SetEquippedMount(CharacterSlot slot, int rank)
    {
        slot.EquippedMountRank = rank;
        RerouteTravel(slot);
    }

    private static Mount? MountOf(CharacterSlot slot) => MountRegistry.Get(slot.Race, slot.EquippedMountRank);

    // Speed change during a journey: the share already covered stays the same, the remaining time adapts
    private static void RerouteTravel(CharacterSlot slot)
    {
        if (slot.Travel is not { } travel
            || CombatZoneRegistry.Get(travel.FromZoneId) is not { } from
            || CombatZoneRegistry.Get(travel.TargetZoneId) is not { } target) return;

        var ground = ZoneTravel.GroundFor(slot.Class, slot.Player.Level, MountOf(slot));
        var route = ZoneTravel.Route(from, target, slot.Faction, ground, slot.DiscoveredZoneIds);
        slot.Travel = new ZoneTravelState
        {
            FromZoneId   = travel.FromZoneId,
            TargetZoneId = travel.TargetZoneId,
            Route        = route,
            Elapsed      = travel.Progress * route.Seconds,
            RegenAcc     = travel.RegenAcc,
        };
    }

    // Invalid values from the save game fall back to the nearest valid rank
    private static void RestoreRiding(CharacterSlot slot, SlotSaveData data)
    {
        slot.RidingRank = Math.Clamp(data.RidingRank, 0, MountRegistry.Skills.Count);
        slot.EquippedMountRank = Math.Clamp(data.EquippedMountRank, 0, slot.RidingRank);
    }
}
