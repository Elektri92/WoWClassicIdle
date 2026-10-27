// Models/ConsumableRegistry.cs
using static Models.ProfessionRegistry;

namespace Models
{
    /// <summary>
    /// Verbrauchsgüter und ihre Rezepte: Alchemie (Heil-/Manatränke, Stat-Elixiere, Fläschchen), Kochkunst (Buff-Essen),
    /// Erste Hilfe (Verbände). Stufen passen zu den Kräutern/Fischen der Farmgebiete (Skill 1/50/100/150/200/250).
    /// Ingenieurskunst (Dynamit aus Stoff + Stein).
    /// IDs: 9101–9103 (Altbestand), Alchemie ab 9110, Kochkunst ab 9200, Erste Hilfe ab 9300, Ingenieurskunst ab 9400.
    /// Wird nicht aus dem statischen Konstruktor von ProfessionRegistry gelesen (sonst Init-Zyklus).
    /// </summary>
    public static class ConsumableRegistry
    {
        private const double BuffSeconds  = 60;
        private const double FlaskSeconds = 3600;

        private static readonly int[] TierSkill = [1, 50, 100, 150, 200, 250];
        private static readonly int[] TierLevel = [1, 10, 20, 30, 40, 50];

        private static readonly string[] PotionNames  = ["Minor", "Lesser", "", "Greater", "Superior", "Major"];
        private static readonly int[]    HealAmounts  = [40, 70, 110, 160, 220, 300];
        private static readonly int[]    ManaAmounts  = [80, 140, 220, 320, 440, 600];
        private static readonly int[]    ElixirStats  = [2, 4, 6, 9, 12, 16];
        private static readonly int[]    FoodStats    = [2, 3, 5, 7, 10, 13];
        private const int FlaskStat = 25;

        private enum Stat { Strength, Agility, Stamina, Intellect, Spirit }

        private static readonly Stat[] AllStats = Enum.GetValues<Stat>();

        private static PrimaryStats Bonus(Stat stat, int v) => stat switch
        {
            Stat.Strength  => new PrimaryStats { Strength     = v },
            Stat.Agility   => new PrimaryStats { Agility      = v },
            Stat.Stamina   => new PrimaryStats { Stamina      = v },
            Stat.Intellect => new PrimaryStats { Intelligence = v },
            _              => new PrimaryStats { Spirit       = v },
        };

        private static readonly Dictionary<Stat, string> ElixirIcons = new()
        {
            [Stat.Strength]  = "inv_potion_61",
            [Stat.Agility]   = "inv_potion_93",
            [Stat.Stamina]   = "inv_potion_43",
            [Stat.Intellect] = "inv_potion_10",
            [Stat.Spirit]    = "inv_potion_12",
        };

        // ─── Kräuter je Alchemie-Stufe (passend zu den Herbalism-Zonen) ───
        private static TradeItem[][] HerbTiers() =>
        [
            [Peacebloom, Silverleaf, Earthroot],
            [Mageroyal, Briarthorn],
            [WildSteelbloom, Bruiseweed],
            [Kingsblood, Liferoot],
            [Sungrass, Blindweed, Gromsblood],
            [GoldenSansam, Dreamfoil, MountainSilversage, Plaguebloom],
        ];

        // ─── Fische je Kochstufe (passend zu den Fishing-Zonen) ───
        private static TradeItem[][] FishTiers() =>
        [
            [RawBrilliantSmallfish],
            [RawBristleWhiskerCatfish],   // Deviate Fish nur für Savory Deviate Delight
            [RawGreaterSagefish, RawLongjawMudSnapper],
            [RawGlossyMightfish, RawNightfinSnapper],
            [RawRedgill, RawSpottedYellowtail],
            [RawSummerBass, RawWhitescaleSalmon, StonescaleEel, WinterSquid],
        ];

        // Essen: Zubereitung je Stat
        private static readonly Dictionary<Stat, string> FoodPrefix = new()
        {
            [Stat.Strength]  = "Hearty",
            [Stat.Agility]   = "Spiced",
            [Stat.Stamina]   = "Smoked",
            [Stat.Intellect] = "Savory",
            [Stat.Spirit]    = "Herb-Baked",
        };

        private static readonly (Stat Stat, string Name, string Icon)[] Flasks =
        [
            (Stat.Strength,  "Flask of Titanic Strength", "inv_potion_41"),
            (Stat.Agility,   "Flask of the Wind",         "inv_potion_48"),
            (Stat.Stamina,   "Flask of the Titans",       "inv_potion_62"),
            (Stat.Intellect, "Flask of Supreme Power",    "inv_potion_41"),
            (Stat.Spirit,    "Flask of Distilled Wisdom", "inv_potion_97"),
        ];

        // Verbände je Stoff: normal + schwer, Prozent schnellere Wiederbelebung
        private static (TradeItem Cloth, string Name, int Skill, int Percent, string Icon)[] Bandages() =>
        [
            (LinenCloth,     "Linen Bandage",           1, 10, "inv_misc_bandage_15"),
            (LinenCloth,     "Heavy Linen Bandage",    40, 15, "inv_misc_bandage_18"),
            (WoolCloth,      "Wool Bandage",           80, 20, "inv_misc_bandage_14"),
            (WoolCloth,      "Heavy Wool Bandage",    115, 25, "inv_misc_bandage_17"),
            (SilkCloth,      "Silk Bandage",          150, 30, "inv_misc_bandage_01"),
            (SilkCloth,      "Heavy Silk Bandage",    180, 35, "inv_misc_bandage_02"),
            (MageweaveCloth, "Mageweave Bandage",     210, 40, "inv_misc_bandage_19"),
            (MageweaveCloth, "Heavy Mageweave Bandage", 240, 45, "inv_misc_bandage_20"),
            (Runecloth,      "Runecloth Bandage",     260, 50, "inv_misc_bandage_11"),
            (Runecloth,      "Heavy Runecloth Bandage", 290, 60, "inv_misc_bandage_12"),
        ];

        public static readonly IReadOnlyList<ConsumableItem> Items;
        public static readonly IReadOnlyList<Recipe> Recipes;
        private static readonly Dictionary<int, ConsumableItem> _byId = new();

        public static ConsumableItem? GetById(int id) => _byId.TryGetValue(id, out var item) ? item : null;

        static ConsumableRegistry()
        {
            var items = new List<ConsumableItem>();
            var recipes = new List<Recipe>();

            void Add(ConsumableItem item, Profession profession, int skill, IEnumerable<Reagent> reagents, string? recipeId = null)
            {
                items.Add(item);
                _byId[item.Id] = item;
                recipes.Add(new Recipe(recipeId ?? $"consumable_{item.Id}", profession, item, reagents.ToList(), skill));
            }

            // Reihenfolge nicht ändern – IDs (Spielstand) und Rezept-IDs hängen daran
            BuildAlchemy(Add);
            BuildCooking(Add);
            BuildDeviateDelight(Add);
            BuildFirstAid(Add);
            BuildEngineering(Add);

            Items = items;
            Recipes = recipes;
        }

        private delegate void AddFn(ConsumableItem item, Profession profession, int skill, IEnumerable<Reagent> reagents, string? recipeId = null);

        /// <summary>Zwei Reagenzien aus einer Materialliste: Haupt-Material ×main, nächstes ×1 (bei einer Sorte nur eines).</summary>
        private static IEnumerable<Reagent> Pick(TradeItem[] mats, int index, int main)
        {
            var first = mats[index % mats.Length];
            yield return new Reagent(first, main);
            var second = mats[(index + 1) % mats.Length];
            if (second != first) yield return new Reagent(second, 1);
        }

        private static void BuildAlchemy(AddFn add)
        {
            var herbs = HerbTiers();
            int id = 9110;
            for (int t = 0; t < herbs.Length; t++)
            {
                string prefix = PotionNames[t].Length > 0 ? PotionNames[t] + " " : "";
                int main = t < 2 ? 1 : 2;

                // Heiltrank (Tier 1 = bestehender Minor Healing Potion, ID 9103)
                var heal = new ConsumableItem(t == 0 ? 9103 : id++, $"{prefix}Healing Potion", HealIcon(t),
                    ConsumableKind.HealthPotion, Amount: HealAmounts[t], RequiredLevel: TierLevel[t], SellPrice: 5 + 10 * t * t);
                add(heal, Profession.Alchemy, TierSkill[t], Pick(herbs[t], 0, main), t == 0 ? "minor_healing_potion" : null);

                var mana = new ConsumableItem(id++, $"{prefix}Mana Potion", ManaIcon(t),
                    ConsumableKind.ManaPotion, Amount: ManaAmounts[t], RequiredLevel: TierLevel[t], SellPrice: 5 + 10 * t * t);
                add(mana, Profession.Alchemy, TierSkill[t], Pick(herbs[t], 1, main));

                for (int s = 0; s < AllStats.Length; s++)
                {
                    var stat = AllStats[s];
                    var elixir = new ConsumableItem(id++, $"{prefix}Elixir of {stat}", ElixirIcons[stat],
                        ConsumableKind.Elixir, Bonus(stat, ElixirStats[t]), DurationSeconds: BuffSeconds,
                        RequiredLevel: TierLevel[t], SellPrice: 5 + 8 * t * t);
                    add(elixir, Profession.Alchemy, TierSkill[t], Pick(herbs[t], s + 2, main));
                }
            }

            // Fläschchen: einzige Rezepte mit Black Lotus, je Hauptstat eines, 1 Stunde
            var top = herbs[^1];
            for (int s = 0; s < Flasks.Length; s++)
            {
                var (stat, name, icon) = Flasks[s];
                var flask = new ConsumableItem(id++, name, icon, ConsumableKind.Flask, Bonus(stat, FlaskStat),
                    DurationSeconds: FlaskSeconds, RequiredLevel: 50, Quality: ItemQuality.Uncommon, SellPrice: 1500);
                add(flask, Profession.Alchemy, 275,
                    [new Reagent(BlackLotus, 1), new Reagent(top[s % top.Length], 4), new Reagent(top[(s + 1) % top.Length], 2)]);
            }
        }

        private static void BuildCooking(AddFn add)
        {
            var fish = FishTiers();
            int id = 9200;
            for (int t = 0; t < fish.Length; t++)
            {
                int count = t < 3 ? 1 : 2;
                for (int s = 0; s < AllStats.Length; s++)
                {
                    var stat = AllStats[s];
                    var raw = fish[t][s % fish[t].Length];
                    string fishName = raw.Name.StartsWith("Raw ") ? raw.Name[4..] : raw.Name;

                    // Tier 1 Ausdauer = bestehender Brilliant Smallfish (ID 9101)
                    bool legacy = t == 0 && stat == Stat.Stamina;
                    var food = new ConsumableItem(legacy ? 9101 : id++, $"{FoodPrefix[stat]} {fishName}", FoodIcon(t, s),
                        ConsumableKind.Food, Bonus(stat, FoodStats[t]), DurationSeconds: BuffSeconds,
                        RequiredLevel: TierLevel[t], SellPrice: 3 + 6 * t * t);
                    add(food, Profession.Cooking, TierSkill[t], [new Reagent(raw, count)],
                        legacy ? "cooked_brilliant_smallfish" : null);
                }
            }
        }

        /// <summary>Einzige Verwendung von Deviate Fish: Verkleidung für 5 min (nur manuell, erneuert sich automatisch).</summary>
        public const int SavoryDeviateDelightId = 9290;

        private static void BuildDeviateDelight(AddFn add)
        {
            var delight = new ConsumableItem(SavoryDeviateDelightId, "Savory Deviate Delight", "inv_misc_monsterhead_04",
                ConsumableKind.Costume, DurationSeconds: 300, Quality: ItemQuality.Uncommon, SellPrice: 25);   // rein optisch → keine Levelanforderung
            add(delight, Profession.Cooking, 85, [new Reagent(DeviateFish, 1)]);
        }

        private static void BuildFirstAid(AddFn add)
        {
            int id = 9300;
            foreach (var (cloth, name, skill, percent, icon) in Bandages())
            {
                bool legacy = name == "Linen Bandage";   // bestehende ID 9102
                bool heavy = name.StartsWith("Heavy");
                var bandage = new ConsumableItem(legacy ? 9102 : id++, name, icon, ConsumableKind.Bandage,
                    Amount: percent, SellPrice: 10 + skill / 2);
                add(bandage, Profession.FirstAid, skill, [new Reagent(cloth, heavy ? 2 : 1)], legacy ? "linen_bandage" : null);
            }
        }

        // Dynamit: Stoff + Stein je Stufe; Schaden ≈ ein Viertel der Gegner-HP auf Stufenlevel
        private static (string Name, TradeItem Cloth, TradeItem Stone, int Skill, int Level, int Damage, string Icon)[] Explosives() =>
        [
            ("Rough Dynamite",  LinenCloth,     RoughStone,   1,  1,  15, "inv_misc_bomb_05"),
            ("Coarse Dynamite", WoolCloth,      CoarseStone, 50, 10,  55, "inv_misc_bomb_06"),
            ("Heavy Dynamite",  SilkCloth,      HeavyStone, 100, 20, 130, "inv_misc_bomb_04"),
            ("Solid Dynamite",  MageweaveCloth, SolidStone, 150, 30, 240, "inv_misc_bomb_03"),
            ("Dense Dynamite",  Runecloth,      DenseStone, 200, 40, 400, "inv_misc_bomb_02"),
        ];

        private static void BuildEngineering(AddFn add)
        {
            int id = 9400;
            foreach (var (name, cloth, stone, skill, level, damage, icon) in Explosives())
            {
                var dynamite = new ConsumableItem(id++, name, icon, ConsumableKind.Explosive, Amount: damage,
                    RequiredLevel: level, SellPrice: 5 + skill / 4);
                add(dynamite, Profession.Engineering, skill, [new Reagent(stone, 2), new Reagent(cloth, 1)]);
            }
        }

        private static string HealIcon(int tier) => tier switch
        {
            0 => "inv_potion_49", 1 => "inv_potion_50", 2 => "inv_potion_51",
            3 => "inv_potion_52", 4 => "inv_potion_53", _ => "inv_potion_54",
        };

        private static string ManaIcon(int tier) => tier switch
        {
            0 => "inv_potion_70", 1 => "inv_potion_71", 2 => "inv_potion_72",
            3 => "inv_potion_73", 4 => "inv_potion_74", _ => "inv_potion_76",
        };

        private static string FoodIcon(int tier, int stat) => ((tier + stat) % 4) switch
        {
            0 => "inv_misc_fish_09",
            1 => "inv_misc_fish_17",
            2 => "inv_misc_food_49",
            _ => "inv_misc_food_48",
        };
    }
}
