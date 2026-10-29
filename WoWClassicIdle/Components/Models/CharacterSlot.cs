using System.Collections.Immutable;

namespace Models
{
    public class CharacterSlot
    {
        public int Id             { get; init; }
        public CharacterClass Class { get; init; }   // never changes – one slot per class
        /// <summary>Name given by the player (empty for legacy saves without a name).</summary>
        public string Name        { get; internal set; } = "";
        public Faction Faction    { get; internal set; }
        public Race Race          { get; internal set; }
        public Gender Gender      { get; internal set; }
        public Player Player      { get; init; } = null!;
        /// <summary>Selected combat zone (null = never chosen → combat paused until a choice is made).</summary>
        public string? CombatZoneId { get; internal set; }
        /// <summary>Ongoing travel to another combat zone (no fighting during travel).</summary>
        public ZoneTravelState? Travel { get; internal set; }
        // Immutable collections: the sim thread replaces them completely, the UI reads without a lock
        /// <summary>Zones already visited – the character knows the flight point there.</summary>
        public ImmutableHashSet<string> DiscoveredZoneIds { get; internal set; } = ImmutableHashSet<string>.Empty;
        /// <summary>Progress of the zone objectives (<see cref="ZoneObjective.Id"/> → counter).</summary>
        public ImmutableDictionary<string, int> ZoneObjectiveProgress { get; internal set; } = ImmutableDictionary<string, int>.Empty;
    }
}
