namespace Models
{
    // Typische Sprüche je Rasse und Klasse für die Sprechblase in der Charaktererstellung (/character)
    public static class CharacterQuotes
    {
        private static readonly Dictionary<Race, string[]> RaceQuotes = new()
        {
            [Race.Human]    = new[] { "For the Alliance!", "Stormwind stands strong!", "Light be with you." },
            [Race.Dwarf]    = new[] { "Fer Khaz Modan!", "Keep yer feet on the ground!", "Ironforge welcomes ye!" },
            [Race.NightElf] = new[] { "Elune be with you.", "Ishnu-alah.", "The forest remembers." },
            [Race.Gnome]    = new[] { "Science will prevail!", "Gnomes are not a snack!", "Let me tinker with that." },
            [Race.Orc]      = new[] { "Lok'tar ogar!", "For the Horde!", "Zug zug." },
            [Race.Undead]   = new[] { "Death is only the beginning.", "Our time has come.", "Victory for the Forsaken!" },
            [Race.Tauren]   = new[] { "Walk with the Earth Mother.", "May the winds guide you.", "Moo? I think not." },
            [Race.Troll]    = new[] { "Stay away from da voodoo!", "Hey mon!", "Da Darkspear never die!" },
        };

        private static readonly Dictionary<CharacterClass, string[]> ClassQuotes = new()
        {
            [CharacterClass.Warrior] = new[] { "Let the battle begin!", "My blade thirsts!" },
            [CharacterClass.Mage]    = new[] { "Knowledge is power.", "Anyone need water?" },
            [CharacterClass.Priest]  = new[] { "The Light will guide me.", "Stand close, I'll keep you alive." },
            [CharacterClass.Rogue]   = new[] { "From the shadows...", "You never saw me." },
            [CharacterClass.Paladin] = new[] { "By the Light!", "Justice will be served!" },
            [CharacterClass.Hunter]  = new[] { "My aim is true.", "My pet is hungry." },
            [CharacterClass.Warlock] = new[] { "Your soul is mine!", "Fear my power!" },
            [CharacterClass.Shaman]  = new[] { "The elements guide me.", "Feel the storm!" },
            [CharacterClass.Druid]   = new[] { "Nature's wrath!", "I am one with the wild." },
        };

        public static string ForRace(Race race) => Pick(RaceQuotes[race]);

        public static string ForClass(CharacterClass cls) => Pick(ClassQuotes[cls]);

        private static string Pick(string[] quotes) => quotes[Random.Shared.Next(quotes.Length)];
    }
}
