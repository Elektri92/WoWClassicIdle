// Models/CraftedGearRegistry.cs
namespace Models
{
    /// <summary>
    /// Herstellbare Rüstung: je Klasse ein Set pro Stufe, an die Klasse gebunden.
    /// Schneiderei → Stoff (Magier, Priester, Hexenmeister), Lederverarbeitung → Leder/Kette (Schurke, Druide, Jäger, Schamane),
    /// Schmiedekunst → Platte (Krieger, Paladin).
    /// Stat-Budget liegt leicht über den nutzbaren Stats grüner Drops derselben Stufe (nur Klassen-Stats),
    /// blaue Drops bleiben besser. IDs ab 9501.
    /// </summary>
    public static class CraftedGearRegistry
    {
        /// <summary>Stufe: Item-Level, benötigter Berufs-Skill, Stat-Budget (Kopf/Brust/Beine), Basis-Materialmenge, Slots.</summary>
        private record GearTier(int Level, int Skill, int Budget, int BaseReagents, ArmorSlot[] Slots);

        private static readonly GearTier[] Tiers =
        [
            new( 5,   1,  3,  3, [ArmorSlot.Chest, ArmorSlot.Legs, ArmorSlot.Wrists]),
            new(15,  50,  6,  4, [ArmorSlot.Chest, ArmorSlot.Legs, ArmorSlot.Hands, ArmorSlot.Feet]),
            new(25, 100, 13,  5, [ArmorSlot.Chest, ArmorSlot.Legs, ArmorSlot.Hands, ArmorSlot.Belt, ArmorSlot.Feet]),
            new(35, 150, 21,  6, [ArmorSlot.Head, ArmorSlot.Chest, ArmorSlot.Legs, ArmorSlot.Hands, ArmorSlot.Feet]),
            new(45, 200, 28,  8, [ArmorSlot.Head, ArmorSlot.Chest, ArmorSlot.Legs, ArmorSlot.Hands, ArmorSlot.Belt, ArmorSlot.Feet]),
            new(55, 250, 35, 10, [ArmorSlot.Head, ArmorSlot.Chest, ArmorSlot.Legs, ArmorSlot.Hands, ArmorSlot.Belt, ArmorSlot.Feet, ArmorSlot.Wrists]),
        ];

        /// <summary>Material einer Stufe: Item + Anteil an der Basismenge.</summary>
        private record TierMaterial(string Name, (TradeItem Item, double Share)[] Reagents);

        /// <summary>Teile-Namen und Icons je Slot (Stoff, Leder, Kette, Platte).</summary>
        private record ArmorStyle(Dictionary<ArmorSlot, (string Piece, string Icon)> Pieces);

        /// <summary>Klassen-Set: Name im Item, Stil und Stat-Gewichtung (Summe 1).</summary>
        private record ClassSet(
            CharacterClass Class, string SetName, ArmorStyle Style,
            double Str = 0, double Agi = 0, double Sta = 0, double Int = 0, double Spi = 0);

        private record ProfessionGear(Profession Profession, TierMaterial[] Materials, ClassSet[] Sets);

        private static readonly ArmorStyle Cloth = new(new()
        {
            [ArmorSlot.Head]   = ("Hood",     "inv_helmet_29"),
            [ArmorSlot.Chest]  = ("Robe",     "inv_chest_cloth_21"),
            [ArmorSlot.Legs]   = ("Pants",    "inv_pants_cloth_05"),
            [ArmorSlot.Hands]  = ("Gloves",   "inv_gauntlets_17"),
            [ArmorSlot.Belt]   = ("Sash",     "inv_belt_08"),
            [ArmorSlot.Feet]   = ("Slippers", "inv_boots_cloth_03"),
            [ArmorSlot.Wrists] = ("Cuffs",    "inv_bracer_07"),
        });

        private static readonly ArmorStyle Leather = new(new()
        {
            [ArmorSlot.Head]   = ("Cap",      "inv_helmet_15"),
            [ArmorSlot.Chest]  = ("Tunic",    "inv_chest_leather_07"),
            [ArmorSlot.Legs]   = ("Leggings", "inv_pants_leather_09"),
            [ArmorSlot.Hands]  = ("Gloves",   "inv_gauntlets_21"),
            [ArmorSlot.Belt]   = ("Belt",     "inv_belt_03"),
            [ArmorSlot.Feet]   = ("Boots",    "inv_boots_05"),
            [ArmorSlot.Wrists] = ("Bracers",  "inv_bracer_04"),
        });

        private static readonly ArmorStyle Mail = new(new()
        {
            [ArmorSlot.Head]   = ("Coif",      "inv_helmet_09"),
            [ArmorSlot.Chest]  = ("Hauberk",   "inv_chest_chain_05"),
            [ArmorSlot.Legs]   = ("Legguards", "inv_pants_mail_03"),
            [ArmorSlot.Hands]  = ("Gauntlets", "inv_gauntlets_10"),
            [ArmorSlot.Belt]   = ("Girdle",    "inv_belt_14"),
            [ArmorSlot.Feet]   = ("Greaves",   "inv_boots_chain_04"),
            [ArmorSlot.Wrists] = ("Wristguards", "inv_bracer_02"),
        });

        private static readonly ArmorStyle Plate = new(new()
        {
            [ArmorSlot.Head]   = ("Helm",        "inv_helmet_01"),
            [ArmorSlot.Chest]  = ("Breastplate", "inv_chest_plate06"),
            [ArmorSlot.Legs]   = ("Legplates",   "inv_pants_04"),
            [ArmorSlot.Hands]  = ("Gauntlets",   "inv_gauntlets_26"),
            [ArmorSlot.Belt]   = ("Waistguard",  "inv_belt_11"),
            [ArmorSlot.Feet]   = ("Sabatons",    "inv_boots_plate_01"),
            [ArmorSlot.Wrists] = ("Vambraces",   "inv_bracer_14"),
        });

        private static ProfessionGear[] Definitions() =>
        [
            new(Profession.Tailoring,
            [
                new("Linen",     [(ProfessionRegistry.LinenCloth, 1.0)]),
                new("Woolen",    [(ProfessionRegistry.WoolCloth, 1.0)]),
                new("Silken",    [(ProfessionRegistry.SilkCloth, 1.0)]),
                new("Mageweave", [(ProfessionRegistry.MageweaveCloth, 1.0)]),
                new("Runecloth", [(ProfessionRegistry.Runecloth, 1.0)]),
                new("Runeweave", [(ProfessionRegistry.Runecloth, 1.5), (ProfessionRegistry.MageweaveCloth, 0.5)]),
            ],
            [
                new(CharacterClass.Mage,    "Arcanist",   Cloth, Int: 0.50, Sta: 0.30, Spi: 0.20),
                new(CharacterClass.Priest,  "Devout",     Cloth, Int: 0.40, Spi: 0.35, Sta: 0.25),
                new(CharacterClass.Warlock, "Shadoweave", Cloth, Int: 0.40, Sta: 0.45, Spi: 0.15),
            ]),

            new(Profession.Leatherworking,
            [
                new("Rawhide",  [(ProfessionRegistry.LightLeather, 1.0), (ProfessionRegistry.RuinedLeatherScraps, 0.5)]),
                new("Tanned",   [(ProfessionRegistry.MediumLeather, 1.0), (ProfessionRegistry.LightLeather, 0.5)]),
                new("Hardened", [(ProfessionRegistry.HeavyLeather, 1.0), (ProfessionRegistry.MediumLeather, 0.5)]),
                new("Thickhide", [(ProfessionRegistry.ThickLeather, 1.0), (ProfessionRegistry.HeavyLeather, 0.5)]),
                new("Rugged",   [(ProfessionRegistry.RuggedLeather, 1.0), (ProfessionRegistry.ThickLeather, 0.5)]),
                new("Primal",   [(ProfessionRegistry.RuggedLeather, 1.5), (ProfessionRegistry.ThickLeather, 0.5)]),
            ],
            [
                new(CharacterClass.Rogue,  "Stalker",     Leather, Agi: 0.50, Sta: 0.30, Str: 0.20),
                new(CharacterClass.Druid,  "Wildheart",   Leather, Int: 0.35, Agi: 0.30, Sta: 0.20, Spi: 0.15),
                new(CharacterClass.Hunter, "Beastmaster", Mail,    Agi: 0.50, Sta: 0.30, Int: 0.20),
                new(CharacterClass.Shaman, "Earthcaller", Mail,    Int: 0.35, Agi: 0.25, Sta: 0.25, Spi: 0.15),
            ]),

            new(Profession.Blacksmithing,
            [
                new("Copper",     [(ProfessionRegistry.CopperOre, 1.0), (ProfessionRegistry.RoughStone, 0.5)]),
                new("Bronze",     [(ProfessionRegistry.CopperOre, 1.0), (ProfessionRegistry.TinOre, 0.5), (ProfessionRegistry.CoarseStone, 0.5)]),
                new("Iron",       [(ProfessionRegistry.IronOre, 1.0), (ProfessionRegistry.HeavyStone, 0.5)]),
                new("Mithril",    [(ProfessionRegistry.MithrilOre, 1.0), (ProfessionRegistry.SolidStone, 0.5)]),
                new("Thorium",    [(ProfessionRegistry.ThoriumOre, 1.0), (ProfessionRegistry.DenseStone, 0.5)]),
                new("Truesilver", [(ProfessionRegistry.ThoriumOre, 1.0), (ProfessionRegistry.TruesilverOre, 0.5), (ProfessionRegistry.DenseStone, 0.5)]),
            ],
            [
                new(CharacterClass.Warrior, "Vanguard",    Plate, Str: 0.45, Sta: 0.40, Agi: 0.15),
                new(CharacterClass.Paladin, "Lightforged", Plate, Str: 0.40, Sta: 0.30, Int: 0.30),
            ]),
        ];

        public static readonly IReadOnlyList<ArmorItem> Items;
        public static readonly IReadOnlyList<Recipe> Recipes;
        private static readonly Dictionary<int, ArmorItem> _byId = new();

        public static ArmorItem? GetById(int id) => _byId.TryGetValue(id, out var item) ? item : null;

        static CraftedGearRegistry()
        {
            var items = new List<ArmorItem>();
            var recipes = new List<Recipe>();
            int id = 9501;

            // Reihenfolge nicht ändern – IDs (Spielstand) und Rezept-IDs hängen daran
            foreach (var gear in Definitions())
            {
                for (int t = 0; t < Tiers.Length; t++)
                {
                    var tier = Tiers[t];
                    var material = gear.Materials[t];
                    foreach (var set in gear.Sets)
                    {
                        foreach (var slot in tier.Slots)
                        {
                            var (piece, icon) = set.Style.Pieces[slot];
                            double weight = SlotWeight(slot);
                            var item = new ArmorItem(id++, $"{material.Name} {set.SetName} {piece}", icon, slot,
                                Stats(tier.Budget * weight, set), tier.Level, ItemQuality.Uncommon,
                                SellPrice: 12L * tier.Level * tier.Level, RequiredClass: set.Class);

                            var reagents = material.Reagents
                                .Select(r => new Reagent(r.Item, Math.Max(1, (int)Math.Round(tier.BaseReagents * r.Share * weight))))
                                .ToList();

                            items.Add(item);
                            _byId[item.Id] = item;
                            recipes.Add(new Recipe($"gear_{item.Id}", gear.Profession, item, reagents, tier.Skill));
                        }
                    }
                }
            }

            Items = items;
            Recipes = recipes;
        }

        /// <summary>Anteil am Stat-Budget je Slot (große Teile mehr, Armschienen am wenigsten).</summary>
        private static double SlotWeight(ArmorSlot slot) => slot switch
        {
            ArmorSlot.Head or ArmorSlot.Chest or ArmorSlot.Legs   => 1.0,
            ArmorSlot.Hands or ArmorSlot.Feet or ArmorSlot.Belt   => 0.75,
            _                                                     => 0.55,
        };

        /// <summary>Verteilt das Budget nach der Klassen-Gewichtung; der Hauptstat bekommt mindestens 1.</summary>
        private static PrimaryStats Stats(double budget, ClassSet set)
        {
            int Part(double share) => (int)Math.Round(budget * share);
            var stats = new PrimaryStats
            {
                Strength     = Part(set.Str),
                Agility      = Part(set.Agi),
                Stamina      = Part(set.Sta),
                Intelligence = Part(set.Int),
                Spirit       = Part(set.Spi),
            };
            if (stats.Strength + stats.Agility + stats.Stamina + stats.Intelligence + stats.Spirit > 0) return stats;

            double max = new[] { set.Str, set.Agi, set.Sta, set.Int, set.Spi }.Max();
            return max == set.Str ? stats with { Strength = 1 }
                 : max == set.Agi ? stats with { Agility = 1 }
                 : max == set.Int ? stats with { Intelligence = 1 }
                 : stats with { Stamina = 1 };
        }
    }
}
