namespace Models
{
    public class CharacterSlot
    {
        public int Id             { get; init; }
        public CharacterClass Class { get; init; }   // never changes – one slot per class
        /// <summary>Vom Spieler vergebener Name (leer bei Altständen ohne Namen).</summary>
        public string Name        { get; internal set; } = "";
        public Faction Faction    { get; internal set; }
        public Race Race          { get; internal set; }
        public Gender Gender      { get; internal set; }
        public Player Player      { get; init; } = null!;
        /// <summary>Gewähltes Kampfgebiet (null = noch nie gewählt → Kampf pausiert bis zur Wahl).</summary>
        public string? CombatZoneId { get; internal set; }
        /// <summary>Laufende Reise in ein anderes Kampfgebiet (während der Reise wird nicht gekämpft).</summary>
        public ZoneTravelState? Travel { get; internal set; }
    }
}
