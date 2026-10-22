namespace Models
{
    public class CharacterSlot
    {
        public int Id             { get; init; }
        public CharacterClass Class { get; init; }   // never changes – one slot per class
        public Faction Faction    { get; internal set; }
        public Race Race          { get; internal set; }
        public Gender Gender      { get; internal set; }
        public Player Player      { get; init; } = null!;
    }
}
