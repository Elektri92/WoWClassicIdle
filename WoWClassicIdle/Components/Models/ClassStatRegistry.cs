// Models/ClassStatRegistry.cs
namespace Models
{
    /// <summary>
    /// Primary attributes per class – based on the values from WoW Classic.
    /// </summary>
    public static class ClassStatRegistry
    {
        private static readonly Dictionary<CharacterClass, ClassStatProfile> Profiles = new()
        {
            // Base values (level 1): Str, Agi, Sta, Int, Spi  |  Growth per level: Str, Agi, Sta, Int, Spi
            { CharacterClass.Warrior, Profile(23, 20, 22, 20, 20, new StatGrowth(1.4, 0.9, 1.3, 0.2, 0.4)) },
            { CharacterClass.Paladin, Profile(22, 20, 22, 20, 21, new StatGrowth(1.2, 0.6, 1.1, 0.6, 0.7)) },
            { CharacterClass.Hunter,  Profile(20, 23, 21, 20, 21, new StatGrowth(0.4, 1.4, 1.0, 0.6, 0.6)) },
            { CharacterClass.Rogue,   Profile(21, 23, 21, 20, 20, new StatGrowth(0.7, 1.5, 0.9, 0.2, 0.4)) },
            { CharacterClass.Priest,  Profile(20, 20, 20, 22, 23, new StatGrowth(0.2, 0.2, 0.4, 1.3, 1.4)) },
            { CharacterClass.Shaman,  Profile(21, 20, 21, 21, 22, new StatGrowth(0.9, 0.5, 1.0, 0.9, 1.0)) },
            { CharacterClass.Mage,    Profile(20, 20, 20, 23, 22, new StatGrowth(0.2, 0.2, 0.4, 1.5, 1.3)) },
            { CharacterClass.Warlock, Profile(20, 20, 21, 22, 22, new StatGrowth(0.3, 0.3, 0.7, 1.2, 1.3)) },
            { CharacterClass.Druid,   Profile(21, 20, 20, 22, 22, new StatGrowth(0.6, 0.6, 0.7, 1.0, 1.1)) },
        };

        public static ClassStatProfile GetProfile(CharacterClass characterClass) => Profiles[characterClass];

        /// <summary>
        /// Calculates the primary attributes of a class for the given level.
        /// </summary>
        public static PrimaryStats GetPrimaryStats(CharacterClass characterClass, int level)
        {
            var profile = GetProfile(characterClass);
            int levelsGained = Math.Max(0, level - 1);

            return profile.Base + new PrimaryStats
            {
                Strength     = (int)(profile.PerLevel.Strength     * levelsGained),
                Agility      = (int)(profile.PerLevel.Agility      * levelsGained),
                Stamina      = (int)(profile.PerLevel.Stamina      * levelsGained),
                Intelligence = (int)(profile.PerLevel.Intelligence * levelsGained),
                Spirit       = (int)(profile.PerLevel.Spirit       * levelsGained),
            };
        }

        private static ClassStatProfile Profile(int str, int agi, int sta, int intel, int spi, StatGrowth perLevel) =>
            new(new PrimaryStats { Strength = str, Agility = agi, Stamina = sta, Intelligence = intel, Spirit = spi }, perLevel);
    }
}
