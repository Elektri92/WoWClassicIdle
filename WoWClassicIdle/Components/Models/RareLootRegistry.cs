// Models/RareLootRegistry.cs
namespace Models
{
    /// <summary>
    /// Loot tables for rare enemies.
    /// Green/Uncommon: always 1 item per rare kill.
    /// Blue/Rare:     15 % chance, double stats, from level 11.
    /// </summary>
    public static class RareLootRegistry
    {
        // All slots in order – later tiers use more of them
        private static readonly (ArmorSlot Slot, string Display, string Icon)[] AllSlots =
        [
            (ArmorSlot.Hands,   "Handwraps",  "inv_gauntlets_04"),
            (ArmorSlot.Legs,    "Legguards",  "inv_pants_03"),
            (ArmorSlot.Chest,   "Chestpiece", "inv_chest_cloth_05"),
            (ArmorSlot.Belt,    "Belt",        "inv_belt_05"),
            (ArmorSlot.Feet,    "Boots",       "inv_boots_08"),
            (ArmorSlot.Wrists,  "Bracers",     "inv_bracer_07"),
            (ArmorSlot.Cape,    "Cloak",       "inv_misc_cape_18"),
            (ArmorSlot.Ring,    "Ring",        "inv_jewelry_ring_03"),
            (ArmorSlot.Head,    "Helm",        "inv_helmet_03"),
            (ArmorSlot.Neck,    "Necklace",    "inv_jewelry_necklace_06"),
            (ArmorSlot.Trinket, "Trinket",     "inv_trinket_naxxramas01"),
        ];

        private static readonly (string Name, Func<int, PrimaryStats> Bonus)[] SingleStatDefs =
        [
            ("Strength",  v => new PrimaryStats { Strength     = v }),
            ("Agility",   v => new PrimaryStats { Agility      = v }),
            ("Stamina",   v => new PrimaryStats { Stamina      = v }),
            ("Spirit",    v => new PrimaryStats { Spirit       = v }),
            ("Intellect", v => new PrimaryStats { Intelligence = v }),
        ];

        private static readonly (string Suffix, Func<int, PrimaryStats> Bonus)[] TwoStatCombos =
        [
            ("the Warrior",  v => new PrimaryStats { Strength = v, Agility      = v }),
            ("the Bear",     v => new PrimaryStats { Strength = v, Stamina      = v }),
            ("the Boar",     v => new PrimaryStats { Strength = v, Spirit       = v }),
            ("the Eagle",    v => new PrimaryStats { Strength = v, Intelligence = v }),
            ("the Tiger",    v => new PrimaryStats { Agility  = v, Stamina      = v }),
            ("the Fox",      v => new PrimaryStats { Agility  = v, Spirit       = v }),
            ("the Owl",      v => new PrimaryStats { Agility  = v, Intelligence = v }),
            ("the Gorilla",  v => new PrimaryStats { Stamina  = v, Spirit       = v }),
            ("the Whale",    v => new PrimaryStats { Stamina  = v, Intelligence = v }),
            ("the Prophet",  v => new PrimaryStats { Spirit   = v, Intelligence = v }),
        ];

        private static readonly (string Suffix, Func<int, PrimaryStats> Bonus)[] ThreeStatCombos =
        [
            ("the Soldier",  v => new PrimaryStats { Strength = v, Agility = v, Stamina      = v }),
            ("the Scout",    v => new PrimaryStats { Strength = v, Agility = v, Spirit       = v }),
            ("the Ranger",   v => new PrimaryStats { Strength = v, Agility = v, Intelligence = v }),
            ("the Defender", v => new PrimaryStats { Strength = v, Stamina = v, Spirit       = v }),
            ("the Champion", v => new PrimaryStats { Strength = v, Stamina = v, Intelligence = v }),
            ("the Crusader", v => new PrimaryStats { Strength = v, Spirit  = v, Intelligence = v }),
            ("the Stalker",  v => new PrimaryStats { Agility  = v, Stamina = v, Spirit       = v }),
            ("the Tracker",  v => new PrimaryStats { Agility  = v, Stamina = v, Intelligence = v }),
            ("the Shade",    v => new PrimaryStats { Agility  = v, Spirit  = v, Intelligence = v }),
            ("the Mystic",   v => new PrimaryStats { Stamina  = v, Spirit  = v, Intelligence = v }),
        ];

        private static readonly (string Suffix, Func<int, PrimaryStats> Bonus)[] FourStatCombos =
        [
            ("the Guardian",   v => new PrimaryStats { Strength = v, Agility = v, Stamina = v, Spirit       = v }),
            ("the Warden",     v => new PrimaryStats { Strength = v, Agility = v, Stamina = v, Intelligence = v }),
            ("the Vindicator", v => new PrimaryStats { Strength = v, Agility = v, Spirit  = v, Intelligence = v }),
            ("the Paragon",    v => new PrimaryStats { Strength = v, Stamina = v, Spirit  = v, Intelligence = v }),
            ("the Sage",       v => new PrimaryStats { Agility  = v, Stamina = v, Spirit  = v, Intelligence = v }),
        ];

        private static readonly string[] AllStatSuffixes =
        [
            "the Immortal", "the Undying", "the Eternal", "the Transcendent",
            "the Ancient",  "the Legendary",
        ];

        // Green tier prefixes (tier 1–12)
        private static readonly string[] GreenPrefixes =
            ["Rough", "Worn", "Sturdy", "Crafted", "Refined", "Tempered",
             "Hardened", "Exceptional", "Superior", "Masterwork", "Ancient", "Eternal"];

        // Blue tier prefixes (tier 3–12, from level 11)
        private static readonly string[] BluePrefixes =
            ["Gleaming", "Radiant", "Brilliant", "Lustrous", "Resplendent",
             "Illustrious", "Magnificent", "Exquisite", "Mythical", "Timeless"];

        // ─── Green tier lists ───
        private static readonly IReadOnlyList<ArmorItem> _tier1, _tier2, _tier3, _tier4,
            _tier5, _tier6, _tier7, _tier8, _tier9, _tier10, _tier11, _tier12;

        // ─── Blaue Tier-Listen (Tier 3–12, ab Lv. 11) ───
        private static readonly IReadOnlyList<ArmorItem> _tier3B, _tier4B, _tier5B, _tier6B,
            _tier7B, _tier8B, _tier9B, _tier10B, _tier11B, _tier12B;

        private static readonly Dictionary<int, ArmorItem> _byId = new();

        /// <summary>Loot table item by the stored ID (null if unknown).</summary>
        public static ArmorItem? GetById(int id) => _byId.TryGetValue(id, out var item) ? item : null;

        static RareLootRegistry()
        {
            int id = 6001;

            // ── Green tiers ──
            _tier1  = BuildSingle  (ref id, 6,  GreenPrefixes[0],  1,  1,    100);
            _tier2  = BuildSingle  (ref id, 7,  GreenPrefixes[1],  2,  6,    500);
            _tier3  = BuildWithCombos(ref id, 7,  GreenPrefixes[2],  4, 11,  2000,  1500, TwoStatCombos,   2);
            _tier4  = BuildWithCombos(ref id, 8,  GreenPrefixes[3],  6, 16,  5000,  4000, TwoStatCombos,   3);
            _tier5  = BuildWithCombos(ref id, 9,  GreenPrefixes[4],  8, 21, 10000,  8000, ThreeStatCombos, 4);
            _tier6  = BuildWithCombos(ref id, 10, GreenPrefixes[5], 10, 26, 20000, 16000, FourStatCombos,  5);
            _tier7  = BuildAllStat  (ref id, GreenPrefixes[6],  12, 31, 40000,   32000, AllStatSuffixes[0], 6);
            _tier8  = BuildAllStat  (ref id, GreenPrefixes[7],  14, 36, 80000,   64000, AllStatSuffixes[1], 7);
            _tier9  = BuildAllStat  (ref id, GreenPrefixes[8],  16, 41, 150000, 120000, AllStatSuffixes[2], 8);
            _tier10 = BuildAllStat  (ref id, GreenPrefixes[9],  18, 46, 300000, 240000, AllStatSuffixes[3], 9);
            _tier11 = BuildAllStat  (ref id, GreenPrefixes[10], 20, 51, 600000, 480000, AllStatSuffixes[4], 10);
            _tier12 = BuildAllStat  (ref id, GreenPrefixes[11], 22, 56, 1200000, 960000, AllStatSuffixes[5], 11);

            // ── Blue tiers (id starts at 7001, double stats) ──
            id = 7001;
            _tier3B  = BuildWithCombos(ref id, 7,  BluePrefixes[0],   8, 11,  8000,  6000, TwoStatCombos,   4,  ItemQuality.Rare);
            _tier4B  = BuildWithCombos(ref id, 8,  BluePrefixes[1],  12, 16, 20000, 16000, TwoStatCombos,   6,  ItemQuality.Rare);
            _tier5B  = BuildWithCombos(ref id, 9,  BluePrefixes[2],  16, 21, 40000, 32000, ThreeStatCombos, 8,  ItemQuality.Rare);
            _tier6B  = BuildWithCombos(ref id, 10, BluePrefixes[3],  20, 26, 80000, 64000, FourStatCombos,  10, ItemQuality.Rare);
            _tier7B  = BuildAllStat   (ref id, BluePrefixes[4],  24, 31, 160000, 128000, AllStatSuffixes[0], 12, ItemQuality.Rare);
            _tier8B  = BuildAllStat   (ref id, BluePrefixes[5],  28, 36, 320000, 256000, AllStatSuffixes[1], 14, ItemQuality.Rare);
            _tier9B  = BuildAllStat   (ref id, BluePrefixes[6],  32, 41, 600000, 480000, AllStatSuffixes[2], 16, ItemQuality.Rare);
            _tier10B = BuildAllStat   (ref id, BluePrefixes[7],  36, 46, 1200000, 960000, AllStatSuffixes[3], 18, ItemQuality.Rare);
            _tier11B = BuildAllStat   (ref id, BluePrefixes[8],  40, 51, 2400000, 1920000, AllStatSuffixes[4], 20, ItemQuality.Rare);
            _tier12B = BuildAllStat   (ref id, BluePrefixes[9],  44, 56, 4800000, 3840000, AllStatSuffixes[5], 22, ItemQuality.Rare);

            // Id→item for the save game
            foreach (var tier in new[] { _tier1, _tier2, _tier3, _tier4, _tier5, _tier6,
                                          _tier7, _tier8, _tier9, _tier10, _tier11, _tier12,
                                          _tier3B, _tier4B, _tier5B, _tier6B, _tier7B, _tier8B,
                                          _tier9B, _tier10B, _tier11B, _tier12B })
                foreach (var item in tier)
                    _byId[item.Id] = item;
        }

        /// <summary>Green loot table for the enemy level.</summary>
        public static IReadOnlyList<ArmorItem> GetLootTable(int enemyLevel) => enemyLevel switch
        {
            >= 1  and <= 5  => _tier1,
            >= 6  and <= 10 => _tier2,
            >= 11 and <= 15 => _tier3,
            >= 16 and <= 20 => _tier4,
            >= 21 and <= 25 => _tier5,
            >= 26 and <= 30 => _tier6,
            >= 31 and <= 35 => _tier7,
            >= 36 and <= 40 => _tier8,
            >= 41 and <= 45 => _tier9,
            >= 46 and <= 50 => _tier10,
            >= 51 and <= 55 => _tier11,
            >= 56           => _tier12,
            _               => [],
        };

        /// <summary>
        /// Checks whether an item contains useful stats for the class.
        /// Prevents e.g. pure Int/Spi items for Warriors or pure Str/Agi items for Mages.
        /// </summary>
        public static bool IsUsefulForClass(PrimaryStats b, CharacterClass cls)
        {
            int str = b.Strength, agi = b.Agility, sta = b.Stamina, intel = b.Intelligence, spi = b.Spirit;
            return cls switch
            {
                CharacterClass.Warrior  => str > 0 || agi > 0 || sta > 0,
                CharacterClass.Rogue    => agi > 0 || str > 0 || sta > 0,
                CharacterClass.Paladin  => str > 0 || sta > 0 || intel > 0,
                CharacterClass.Hunter   => agi > 0 || sta > 0 || intel > 0,
                CharacterClass.Priest   => intel > 0 || spi > 0 || sta > 0,
                CharacterClass.Mage     => intel > 0 || spi > 0 || sta > 0,
                CharacterClass.Warlock  => intel > 0 || spi > 0 || sta > 0,
                CharacterClass.Shaman   => agi > 0 || intel > 0 || spi > 0 || sta > 0,
                CharacterClass.Druid    => agi > 0 || intel > 0 || spi > 0 || sta > 0,
                _                       => true,
            };
        }

        /// <summary>Blaue Loottabelle (15 % Chance, ab Level 11).</summary>
        public static IReadOnlyList<ArmorItem> GetBlueLootTable(int enemyLevel) => enemyLevel switch
        {
            >= 11 and <= 15 => _tier3B,
            >= 16 and <= 20 => _tier4B,
            >= 21 and <= 25 => _tier5B,
            >= 26 and <= 30 => _tier6B,
            >= 31 and <= 35 => _tier7B,
            >= 36 and <= 40 => _tier8B,
            >= 41 and <= 45 => _tier9B,
            >= 46 and <= 50 => _tier10B,
            >= 51 and <= 55 => _tier11B,
            >= 56           => _tier12B,
            _               => [],
        };

        // ─── Hilfsmethoden ───

        private static IReadOnlyList<ArmorItem> BuildSingle(
            ref int id, int slotCount, string prefix, int statVal, int reqLevel, long sell,
            ItemQuality quality = ItemQuality.Uncommon)
        {
            var list = new List<ArmorItem>();
            foreach (var (slot, display, icon) in AllSlots.Take(slotCount))
                foreach (var (name, bonus) in SingleStatDefs)
                    list.Add(new ArmorItem(id++, $"{prefix} {display} of {name}", icon, slot,
                                           bonus(statVal), reqLevel, quality, sell));
            return list;
        }

        private static IReadOnlyList<ArmorItem> BuildWithCombos(
            ref int id, int slotCount, string prefix, int singleVal, int reqLevel,
            long sellSingle, long sellCombo,
            (string Suffix, Func<int, PrimaryStats> Bonus)[] combos, int comboVal,
            ItemQuality quality = ItemQuality.Uncommon)
        {
            var list = new List<ArmorItem>();
            foreach (var (slot, display, icon) in AllSlots.Take(slotCount))
            {
                foreach (var (name, bonus) in SingleStatDefs)
                    list.Add(new ArmorItem(id++, $"{prefix} {display} of {name}", icon, slot,
                                           bonus(singleVal), reqLevel, quality, sellSingle));
                foreach (var (suffix, bonus) in combos)
                    list.Add(new ArmorItem(id++, $"{prefix} {display} of {suffix}", icon, slot,
                                           bonus(comboVal), reqLevel, quality, sellCombo));
            }
            return list;
        }

        private static IReadOnlyList<ArmorItem> BuildAllStat(
            ref int id, string prefix, int singleVal, int reqLevel,
            long sellSingle, long sellCombo, string comboSuffix, int comboVal,
            ItemQuality quality = ItemQuality.Uncommon)
        {
            var list = new List<ArmorItem>();
            foreach (var (slot, display, icon) in AllSlots)
            {
                foreach (var (name, bonus) in SingleStatDefs)
                    list.Add(new ArmorItem(id++, $"{prefix} {display} of {name}", icon, slot,
                                           bonus(singleVal), reqLevel, quality, sellSingle));
                var allStats = new PrimaryStats
                {
                    Strength = comboVal, Agility = comboVal, Stamina = comboVal,
                    Intelligence = comboVal, Spirit = comboVal,
                };
                list.Add(new ArmorItem(id++, $"{prefix} {display} of {comboSuffix}", icon, slot,
                                        allStats, reqLevel, quality, sellCombo));
            }
            return list;
        }
    }
}
