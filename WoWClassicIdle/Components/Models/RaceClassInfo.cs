// Models/RaceClassInfo.cs
namespace Models
{
    public static class RaceClassInfo
    {
        public static readonly Dictionary<Race, Faction> RaceToFaction = new()
        {
            { Race.Human,    Faction.Alliance },
            { Race.Dwarf,    Faction.Alliance },
            { Race.NightElf, Faction.Alliance },
            { Race.Gnome,    Faction.Alliance },
            { Race.Orc,      Faction.Horde    },
            { Race.Undead,   Faction.Horde    },
            { Race.Tauren,   Faction.Horde    },
            { Race.Troll,    Faction.Horde    },
        };

        public static readonly Dictionary<Race, CharacterClass[]> RaceToClasses = new()
        {
            // Alliance
            { Race.Human,    new[] { CharacterClass.Warrior, CharacterClass.Paladin, CharacterClass.Rogue, CharacterClass.Mage, CharacterClass.Warlock, CharacterClass.Priest } },
            { Race.Dwarf,    new[] { CharacterClass.Warrior, CharacterClass.Paladin, CharacterClass.Hunter, CharacterClass.Rogue, CharacterClass.Priest } },
            { Race.NightElf, new[] { CharacterClass.Warrior, CharacterClass.Hunter,  CharacterClass.Rogue, CharacterClass.Priest, CharacterClass.Druid } },
            { Race.Gnome,    new[] { CharacterClass.Warrior, CharacterClass.Rogue,   CharacterClass.Mage,   CharacterClass.Warlock } },

            // Horde
            { Race.Orc,      new[] { CharacterClass.Warrior, CharacterClass.Hunter, CharacterClass.Rogue, CharacterClass.Shaman, CharacterClass.Warlock } },
            { Race.Undead,   new[] { CharacterClass.Warrior, CharacterClass.Rogue,  CharacterClass.Mage,   CharacterClass.Warlock, CharacterClass.Priest } },
            { Race.Tauren,   new[] { CharacterClass.Warrior, CharacterClass.Hunter, CharacterClass.Shaman,  CharacterClass.Druid } },
            { Race.Troll,    new[] { CharacterClass.Warrior, CharacterClass.Hunter, CharacterClass.Rogue,   CharacterClass.Priest, CharacterClass.Shaman, CharacterClass.Mage } },
        };

        public static bool IsRaceValidForFaction(Race race, Faction faction) =>
            RaceToFaction.TryGetValue(race, out var f) && f == faction;

        public static bool IsClassAvailableForRace(Race race, CharacterClass characterClass) =>
            RaceToClasses.TryGetValue(race, out var classes) && classes.Contains(characterClass);

        public static IReadOnlyList<CharacterClass> GetAvailableClasses(Race race) =>
            RaceToClasses.TryGetValue(race, out var classes)
                ? classes
                : Array.Empty<CharacterClass>();
    }
}
