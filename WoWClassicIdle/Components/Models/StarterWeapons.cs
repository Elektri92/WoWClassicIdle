// Models/StarterWeapons.cs
namespace Models
{
    public record WeaponProfile(string Name, int MinDamage, int MaxDamage, double Speed);

    /// <summary>
    /// Starter weapons per class – based on the level 1 gear from WoW Classic.
    /// Basis for auto-attacks as long as there is no weapon equipment.
    /// </summary>
    public static class StarterWeapons
    {
        private static readonly Dictionary<CharacterClass, WeaponProfile> Weapons = new()
        {
            [CharacterClass.Warrior] = new("Worn Shortsword",   1, 3, 2.3),
            [CharacterClass.Paladin] = new("Battleworn Hammer", 2, 4, 2.9),
            [CharacterClass.Hunter]  = new("Worn Axe",          1, 3, 3.4),
            [CharacterClass.Rogue]   = new("Worn Dagger",       1, 3, 1.8),
            [CharacterClass.Priest]  = new("Worn Mace",         1, 3, 1.9),
            [CharacterClass.Shaman]  = new("Worn Mace",         1, 3, 1.8),
            [CharacterClass.Mage]    = new("Bent Staff",        2, 4, 2.9),
            [CharacterClass.Warlock] = new("Worn Dagger",       1, 3, 1.6),
            [CharacterClass.Druid]   = new("Worn Mace",         1, 3, 1.9),
        };

        private static readonly WeaponProfile Fallback = new("Fists", 1, 2, 2.0);

        public static WeaponProfile For(CharacterClass cls) =>
            Weapons.TryGetValue(cls, out var weapon) ? weapon : Fallback;
    }
}
