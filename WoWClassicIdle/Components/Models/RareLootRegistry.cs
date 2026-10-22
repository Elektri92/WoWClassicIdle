// Models/RareLootRegistry.cs
namespace Models
{
    /// <summary>
    /// Loottabellen für seltene Gegner. Jede Tabelle enthält Rüstungsteile für einen Levelbereich.
    /// </summary>
    public static class RareLootRegistry
    {
        private static readonly (ArmorSlot Slot, string Display, string Icon)[] AllSlots =
        [
            (ArmorSlot.Hands,  "Handwraps",  "inv_gauntlets_04"),
            (ArmorSlot.Legs,   "Legguards",  "inv_pants_03"),
            (ArmorSlot.Chest,  "Chestpiece", "inv_chest_cloth_05"),
            (ArmorSlot.Belt,   "Belt",       "inv_belt_05"),
            (ArmorSlot.Feet,   "Boots",      "inv_boots_08"),
            (ArmorSlot.Wrists, "Bracers",    "inv_bracer_07"),
            (ArmorSlot.Cape,   "Cloak",      "inv_misc_cape_18"),
        ];

        private static readonly (string StatName, PrimaryStats B1, PrimaryStats B2, PrimaryStats B4)[] SingleStats =
        [
            ("Strength",  new PrimaryStats { Strength     = 1 }, new PrimaryStats { Strength     = 2 }, new PrimaryStats { Strength     = 4 }),
            ("Agility",   new PrimaryStats { Agility      = 1 }, new PrimaryStats { Agility      = 2 }, new PrimaryStats { Agility      = 4 }),
            ("Stamina",   new PrimaryStats { Stamina      = 1 }, new PrimaryStats { Stamina      = 2 }, new PrimaryStats { Stamina      = 4 }),
            ("Spirit",    new PrimaryStats { Spirit       = 1 }, new PrimaryStats { Spirit       = 2 }, new PrimaryStats { Spirit       = 4 }),
            ("Intellect", new PrimaryStats { Intelligence = 1 }, new PrimaryStats { Intelligence = 2 }, new PrimaryStats { Intelligence = 4 }),
        ];

        // WoW-Classic-konforme Doppelstat-Suffixe mit je +2 auf zwei Primärattribute
        private static readonly (string Suffix, PrimaryStats Bonus)[] ComboStats =
        [
            ("the Warrior",  new PrimaryStats { Strength = 2, Agility      = 2 }),
            ("the Bear",     new PrimaryStats { Strength = 2, Stamina      = 2 }),
            ("the Boar",     new PrimaryStats { Strength = 2, Spirit       = 2 }),
            ("the Eagle",    new PrimaryStats { Strength = 2, Intelligence = 2 }),
            ("the Tiger",    new PrimaryStats { Agility  = 2, Stamina      = 2 }),
            ("the Fox",      new PrimaryStats { Agility  = 2, Spirit       = 2 }),
            ("the Owl",      new PrimaryStats { Agility  = 2, Intelligence = 2 }),
            ("the Gorilla",  new PrimaryStats { Stamina  = 2, Spirit       = 2 }),
            ("the Whale",    new PrimaryStats { Stamina  = 2, Intelligence = 2 }),
            ("the Prophet",  new PrimaryStats { Spirit   = 2, Intelligence = 2 }),
        ];

        private static readonly IReadOnlyList<ArmorItem> _tier1;
        private static readonly IReadOnlyList<ArmorItem> _tier2;
        private static readonly IReadOnlyList<ArmorItem> _tier3;

        private static readonly Dictionary<int, ArmorItem> _byId = new();

        /// <summary>Loottabellen-Item anhand der gespeicherten Id (null wenn unbekannt).</summary>
        public static ArmorItem? GetById(int id) => _byId.TryGetValue(id, out var item) ? item : null;

        static RareLootRegistry()
        {
            var items = new List<ArmorItem>();
            int id = 6001;

            // Tier 1 (Lv. 1–5): 6 Slots (kein Umhang), je +1 Stat
            foreach (var (slot, display, icon) in AllSlots.Take(6))
                foreach (var (statName, b1, _, _) in SingleStats)
                    items.Add(new ArmorItem(id++, $"Rough {display} of {statName}", icon, slot, b1, 1, ItemQuality.Uncommon, 100));
            _tier1 = items.ToList();
            items.Clear();

            // Tier 2 (Lv. 6–10): 7 Slots, je +2 Stat
            foreach (var (slot, display, icon) in AllSlots)
                foreach (var (statName, _, b2, _) in SingleStats)
                    items.Add(new ArmorItem(id++, $"Worn {display} of {statName}", icon, slot, b2, 6, ItemQuality.Uncommon, 500));
            _tier2 = items.ToList();
            items.Clear();

            // Tier 3 (Lv. 11–15): 7 Slots, je +4 Einzel-Stat + 10 Kombi-Stats (+2/+2)
            foreach (var (slot, display, icon) in AllSlots)
            {
                foreach (var (statName, _, _, b4) in SingleStats)
                    items.Add(new ArmorItem(id++, $"Sturdy {display} of {statName}", icon, slot, b4, 11, ItemQuality.Uncommon, 2000));
                foreach (var (suffix, comboBonus) in ComboStats)
                    items.Add(new ArmorItem(id++, $"Sturdy {display} of {suffix}", icon, slot, comboBonus, 11, ItemQuality.Uncommon, 1500));
            }
            _tier3 = items.ToList();

            // Id→Item für den Spielstand (Items werden nur als Id gespeichert)
            foreach (var item in _tier1.Concat(_tier2).Concat(_tier3))
                _byId[item.Id] = item;
        }

        /// <summary>
        /// Gibt die passende Loottabelle für den Gegner-Level zurück.
        /// Leere Liste wenn kein Loottable für diesen Level existiert.
        /// </summary>
        public static IReadOnlyList<ArmorItem> GetLootTable(int enemyLevel) => enemyLevel switch
        {
            >= 1  and <= 5  => _tier1,
            >= 6  and <= 10 => _tier2,
            >= 11 and <= 15 => _tier3,
            _               => [],
        };
    }
}
