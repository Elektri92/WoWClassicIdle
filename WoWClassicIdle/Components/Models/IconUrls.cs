namespace Models
{
    public static class IconUrls
    {
        private static readonly Dictionary<Race, string> RaceUrlNames = new()
        {
            [Race.Human] = "human",
            [Race.Orc] = "orc",
            [Race.Dwarf] = "dwarf",
            [Race.NightElf] = "nightelf",
            [Race.Undead] = "scourge",
            [Race.Tauren] = "tauren",
            [Race.Gnome] = "gnome",
            [Race.Troll] = "troll",
        };

        private static readonly Dictionary<CharacterClass, string> ClassIcons = new()
        {
            [CharacterClass.Warrior] = "https://wow.zamimg.com/images/wow/icons/large/class_warrior.jpg",
            [CharacterClass.Mage] = "https://wow.zamimg.com/images/wow/icons/large/class_mage.jpg",
            [CharacterClass.Rogue] = "https://wow.zamimg.com/images/wow/icons/large/class_rogue.jpg",
            [CharacterClass.Priest] = "https://wow.zamimg.com/images/wow/icons/large/class_priest.jpg",
            [CharacterClass.Hunter] = "https://wow.zamimg.com/images/wow/icons/large/class_hunter.jpg",
            [CharacterClass.Paladin] = "https://wow.zamimg.com/images/wow/icons/large/class_paladin.jpg",
            [CharacterClass.Warlock] = "https://wow.zamimg.com/images/wow/icons/large/class_warlock.jpg",
            [CharacterClass.Shaman] = "https://wow.zamimg.com/images/wow/icons/large/class_shaman.jpg",
            [CharacterClass.Druid] = "https://wow.zamimg.com/images/wow/icons/large/class_druid.jpg",
        };

        public static string RaceIconUrl(Race race, Gender gender)
        {
            if (!RaceUrlNames.TryGetValue(race, out var name))
                return "";
            var genderName = gender == Gender.Male ? "male" : "female";
            return $"https://wow.zamimg.com/images/wow/icons/large/race_{name}_{genderName}.jpg";
        }

        // Freigestellte Volkswappen (wwwroot/images/races, Quelle: Wowhead-Screenshots)
        public static string RaceCrestUrl(Race race) =>
            $"images/races/crest_{race.ToString().ToLowerInvariant()}.png";

        // Stadtkarte der Fraktionshauptstadt (wwwroot/images/cities): Stormwind bzw. Orgrimmar
        public static string CapitalMapUrl(Faction faction) =>
            faction == Faction.Horde ? "images/cities/orgrimmar.jpg" : "images/cities/stormwind.jpg";

        // Gebietskarten (wwwroot/images/zones) als Hintergrund der Sammelberufe – zufällig gewählt
        private static readonly string[] GatheringMaps =
            ["eastern_plaguelands", "winterspring", "burning_steppes", "stranglethorn", "duskwood", "ashenvale"];

        public static string RandomGatheringMapUrl() =>
            $"images/zones/{GatheringMaps[Random.Shared.Next(GatheringMaps.Length)]}.jpg";

        public static string ClassIconUrl(CharacterClass cls) =>
            ClassIcons.TryGetValue(cls, out var url) ? url : "";

        // PvP-Banner als Fraktionswappen (Classic: _01 = Horde, _02 = Alliance)
        public static string FactionIconUrl(Faction faction) =>
            faction == Faction.Horde
                ? "https://wow.zamimg.com/images/wow/icons/large/inv_bannerpvp_01.jpg"
                : "https://wow.zamimg.com/images/wow/icons/large/inv_bannerpvp_02.jpg";

        // UI-Icons für Buttons (ersetzen Unicode-Glyphen)
        public const string EditIcon = "https://wow.zamimg.com/images/wow/icons/medium/inv_misc_wrench_01.jpg";
        public const string PlayIcon = "https://wow.zamimg.com/images/wow/icons/medium/inv_sword_04.jpg";
        public const string NewCharacterIcon = "https://wow.zamimg.com/images/wow/icons/medium/spell_holy_resurrection.jpg";
    }
}
