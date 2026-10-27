// Models/ProfessionRegistry.cs
namespace Models
{
    /// <summary>Möglicher Drop eines Sammel-Durchlaufs; <see cref="Chance"/> wird über alle Drops der Zone normiert.</summary>
    public record GatherDrop(TradeItem Item, double Chance, int Count = 1);

    /// <summary>
    /// Farmgebiet eines Sammelberufs – jeder Durchlauf liefert immer genau einen Drop.
    /// Jede Zone hat einen aktiven Slot und <see cref="PassiveSlotCount"/> passive Slots (mit Charakteren belegt;
    /// bei Sammelberufen und Angeln). <see cref="Theme"/> wählt den Farbverlauf des Zonen-Blocks (CSS-Klasse zone-theme-*).
    /// </summary>
    public record GatheringZone(
        string Id,
        string Name,
        IReadOnlyList<GatherDrop> Drops,
        string Theme,
        string Description = "",
        int PassiveSlotCount = 3);

    public record Reagent(TradeItem Item, int Count);

    /// <summary>Rezept; <see cref="Result"/> ist ein Handwerksgut (<see cref="TradeItem"/>) oder Rüstung (<see cref="ArmorItem"/>).</summary>
    public record Recipe(
        string Id,
        Profession Profession,
        Item Result,
        IReadOnlyList<Reagent> Reagents,
        int RequiredSkill = 1,
        int ResultCount = 1);

    /// <summary>Stoff-Drop im Kampf: <see cref="ProfessionRegistry.ClothDropChance"/> je Kill bei Gegnern im Levelbereich.</summary>
    public record ClothDrop(TradeItem Item, int MinEnemyLevel, int MaxEnemyLevel);

    /// <summary>
    /// Statische Berufsdaten: Handwerksmaterialien (IDs ab 9001), Stoff-Drops, Farmgebiete
    /// (<c>ProfessionRegistry.Zones.cs</c>) und Rezepte (<c>ProfessionRegistry.Recipes.cs</c>).
    /// </summary>
    public static partial class ProfessionRegistry
    {
        // ─── Stoffe (Kampf-Drops) ───
        public static readonly TradeItem LinenCloth     = new(9001, "Linen Cloth",     "inv_fabric_linen_01",     true);
        public static readonly TradeItem WoolCloth      = new(9014, "Wool Cloth",      "inv_fabric_wool_01",      true);
        public static readonly TradeItem SilkCloth      = new(9015, "Silk Cloth",      "inv_fabric_silk_01",      true);
        public static readonly TradeItem MageweaveCloth = new(9016, "Mageweave Cloth", "inv_fabric_mageweave_01", true);
        public static readonly TradeItem Runecloth      = new(9017, "Runecloth",       "inv_fabric_purple_01",    true);

        // ─── Bergbau ───
        public static readonly TradeItem CopperOre     = new(9003, "Copper Ore",     "inv_ore_copper_01",     true);
        public static readonly TradeItem TinOre        = new(9006, "Tin Ore",        "inv_ore_tin_01",        true);
        public static readonly TradeItem SilverOre     = new(9007, "Silver Ore",     "inv_stone_16",          true);
        public static readonly TradeItem IronOre       = new(9018, "Iron Ore",       "inv_ore_iron_01",       true);
        public static readonly TradeItem MithrilOre    = new(9019, "Mithril Ore",    "inv_ore_mithril_02",    true);
        public static readonly TradeItem TruesilverOre = new(9020, "Truesilver Ore", "inv_ore_truesilver_01", true);
        public static readonly TradeItem ThoriumOre    = new(9021, "Thorium Ore",    "inv_ore_thorium_02",    true);
        public static readonly TradeItem RoughStone    = new(9022, "Rough Stone",    "inv_stone_06",          true);
        public static readonly TradeItem CoarseStone   = new(9023, "Coarse Stone",   "inv_stone_09",          true);
        public static readonly TradeItem HeavyStone    = new(9024, "Heavy Stone",    "inv_stone_07",          true);
        public static readonly TradeItem SolidStone    = new(9025, "Solid Stone",    "inv_stone_10",          true);
        public static readonly TradeItem DenseStone    = new(9026, "Dense Stone",    "inv_stone_14",          true);

        // ─── Kürschnerei ───
        public static readonly TradeItem RuinedLeatherScraps = new(9027, "Ruined Leather Scraps", "inv_misc_pelt_bear_ruin_05", true, ItemQuality.Poor);
        public static readonly TradeItem LightLeather        = new(9002, "Light Leather",         "inv_misc_leatherscrap_03", true);
        public static readonly TradeItem MediumLeather       = new(9012, "Medium Leather",        "inv_misc_leatherscrap_05", true);
        public static readonly TradeItem HeavyLeather        = new(9028, "Heavy Leather",         "inv_misc_leatherscrap_07", true);
        public static readonly TradeItem ThickLeather        = new(9029, "Thick Leather",         "inv_misc_leatherscrap_08", true);
        public static readonly TradeItem RuggedLeather       = new(9030, "Rugged Leather",        "inv_misc_leatherscrap_02", true);

        // ─── Kräuterkunde ───
        public static readonly TradeItem Peacebloom         = new(9004, "Peacebloom",          "inv_misc_flower_02",               true);
        public static readonly TradeItem Silverleaf         = new(9008, "Silverleaf",          "inv_misc_herb_10",                 true);
        public static readonly TradeItem Earthroot          = new(9009, "Earthroot",           "inv_misc_herb_07",                 true);
        public static readonly TradeItem Mageroyal          = new(9010, "Mageroyal",           "inv_jewelry_talisman_03",          true);
        public static readonly TradeItem Briarthorn         = new(9011, "Briarthorn",          "inv_misc_root_01",                 true);
        public static readonly TradeItem WildSteelbloom     = new(9031, "Wild Steelbloom",     "inv_misc_flower_01",               true);
        public static readonly TradeItem Bruiseweed         = new(9032, "Bruiseweed",          "inv_misc_herb_01",                 true);
        public static readonly TradeItem Kingsblood         = new(9033, "Kingsblood",          "inv_misc_herb_03",                 true);
        public static readonly TradeItem Liferoot           = new(9034, "Liferoot",            "inv_misc_root_02",                 true);
        public static readonly TradeItem Sungrass           = new(9035, "Sungrass",            "inv_misc_herb_18",                 true);
        public static readonly TradeItem Blindweed          = new(9036, "Blindweed",           "inv_misc_herb_14",                 true);
        public static readonly TradeItem Gromsblood         = new(9037, "Gromsblood",          "inv_misc_herb_16",                 true);
        public static readonly TradeItem GoldenSansam       = new(9038, "Golden Sansam",       "inv_misc_herb_sansamroot",         true);
        public static readonly TradeItem Dreamfoil          = new(9039, "Dreamfoil",           "inv_misc_herb_dreamfoil",          true);
        public static readonly TradeItem MountainSilversage = new(9040, "Mountain Silversage", "inv_misc_herb_mountainsilversage", true);
        public static readonly TradeItem Plaguebloom        = new(9041, "Plaguebloom",         "inv_misc_herb_plaguebloom",        true);
        public static readonly TradeItem BlackLotus         = new(9042, "Black Lotus",         "inv_misc_herb_blacklotus",         true, ItemQuality.Uncommon, SellPrice: 50);

        // ─── Angeln ───
        public static readonly TradeItem RawBrilliantSmallfish    = new(9005, "Raw Brilliant Smallfish",     "inv_misc_fish_02", true);
        public static readonly TradeItem DeviateFish              = new(9013, "Deviate Fish",                "inv_misc_monsterhead_01", true);
        public static readonly TradeItem RawBristleWhiskerCatfish = new(9043, "Raw Bristle Whisker Catfish", "inv_misc_fish_05", true);
        public static readonly TradeItem RawGreaterSagefish       = new(9044, "Raw Greater Sagefish",        "inv_misc_fish_20", true);
        public static readonly TradeItem RawLongjawMudSnapper     = new(9045, "Raw Longjaw Mud Snapper",     "inv_misc_fish_04", true);
        public static readonly TradeItem RawGlossyMightfish       = new(9046, "Raw Glossy Mightfish",        "inv_misc_fish_11", true);
        public static readonly TradeItem RawNightfinSnapper       = new(9047, "Raw Nightfin Snapper",        "inv_misc_fish_03", true);
        public static readonly TradeItem RawRedgill               = new(9048, "Raw Redgill",                 "inv_misc_fish_13", true);
        public static readonly TradeItem RawSpottedYellowtail     = new(9049, "Raw Spotted Yellowtail",      "inv_misc_fish_15", true);
        public static readonly TradeItem RawSummerBass            = new(9050, "Raw Summer Bass",             "inv_misc_fish_21", true);
        public static readonly TradeItem RawWhitescaleSalmon      = new(9051, "Raw Whitescale Salmon",       "inv_misc_fish_19", true);
        public static readonly TradeItem StonescaleEel            = new(9052, "Stonescale Eel",              "inv_misc_fish_08", true);
        public static readonly TradeItem WinterSquid              = new(9053, "Winter Squid",                "inv_misc_fish_07", true);

        // ─── Verzauberkunst (aus dem Entzaubern grüner Items, siehe EnchantingRegistry) ───
        public static readonly TradeItem StrangeDust           = new(9060, "Strange Dust",            "inv_enchant_duststrange",          true, SellPrice: 20);
        public static readonly TradeItem GreaterMagicEssence   = new(9061, "Greater Magic Essence",   "inv_enchant_essencemagiclarge",    true, ItemQuality.Uncommon, SellPrice: 50);
        public static readonly TradeItem SoulDust              = new(9062, "Soul Dust",               "inv_enchant_dustsoul",             true, SellPrice: 60);
        public static readonly TradeItem GreaterAstralEssence  = new(9063, "Greater Astral Essence",  "inv_enchant_essenceastrallarge",   true, ItemQuality.Uncommon, SellPrice: 150);
        public static readonly TradeItem VisionDust            = new(9064, "Vision Dust",             "inv_enchant_dustvision",           true, SellPrice: 150);
        public static readonly TradeItem GreaterMysticEssence  = new(9065, "Greater Mystic Essence",  "inv_enchant_essencemysticallarge", true, ItemQuality.Uncommon, SellPrice: 400);
        public static readonly TradeItem DreamDust             = new(9066, "Dream Dust",              "inv_enchant_dustdream",            true, SellPrice: 300);
        public static readonly TradeItem GreaterNetherEssence  = new(9067, "Greater Nether Essence",  "inv_enchant_essencenetherlarge",   true, ItemQuality.Uncommon, SellPrice: 800);
        public static readonly TradeItem IllusionDust          = new(9068, "Illusion Dust",           "inv_enchant_dustillusion",         true, SellPrice: 600);
        public static readonly TradeItem GreaterEternalEssence = new(9069, "Greater Eternal Essence", "inv_enchant_essenceeternallarge",  true, ItemQuality.Uncommon, SellPrice: 1500);

        // Hergestellte Verbrauchsgüter (Tränke, Essen, Verbände) → ConsumableRegistry

        /// <summary>Alle Rohstoffe – fehlt ein Item hier (oder in ConsumableRegistry), geht es beim Laden verloren.</summary>
        public static readonly IReadOnlyList<TradeItem> AllItems =
        [
            LinenCloth, WoolCloth, SilkCloth, MageweaveCloth, Runecloth,
            CopperOre, TinOre, SilverOre, IronOre, MithrilOre, TruesilverOre, ThoriumOre,
            RoughStone, CoarseStone, HeavyStone, SolidStone, DenseStone,
            RuinedLeatherScraps, LightLeather, MediumLeather, HeavyLeather, ThickLeather, RuggedLeather,
            Peacebloom, Silverleaf, Earthroot, Mageroyal, Briarthorn, WildSteelbloom, Bruiseweed, Kingsblood,
            Liferoot, Sungrass, Blindweed, Gromsblood, GoldenSansam, Dreamfoil, MountainSilversage, Plaguebloom, BlackLotus,
            RawBrilliantSmallfish, DeviateFish, RawBristleWhiskerCatfish, RawGreaterSagefish, RawLongjawMudSnapper,
            RawGlossyMightfish, RawNightfinSnapper, RawRedgill, RawSpottedYellowtail, RawSummerBass,
            RawWhitescaleSalmon, StonescaleEel, WinterSquid,
            StrangeDust, GreaterMagicEssence, SoulDust, GreaterAstralEssence, VisionDust,
            GreaterMysticEssence, DreamDust, GreaterNetherEssence, IllusionDust, GreaterEternalEssence,
        ];

        public static TradeItem? GetById(int id) =>
            AllItems.FirstOrDefault(i => i.Id == id) ?? ConsumableRegistry.GetById(id);

        // ─── Stoff-Drops im Kampf: je passendem Levelband, Levelbänder überlappen leicht ───
        // Wie in Classic tragen vor allem Humanoide Stoff; alle anderen Gegner mit Grundchance,
        // damit es auch in Levelbereichen ohne Humanoide (z. B. Lv 1–11) Stoff gibt
        public const double ClothDropChance         = 0.10;
        public const double HumanoidClothDropChance = 0.35;
        public const string HumanoidType            = "Humanoid";

        /// <summary>Anzahl je Drop: Humanoide 1–2, seltene Gegner (immer Drop) 2–3, sonst 1.</summary>
        public static (double Chance, int MinCount, int MaxCount) ClothDropFor(Enemy enemy) =>
            enemy.IsRare                  ? (1.0, 2, 3)
            : enemy.Type == HumanoidType  ? (HumanoidClothDropChance, 1, 2)
            :                               (ClothDropChance, 1, 1);

        public static readonly IReadOnlyList<ClothDrop> ClothDrops =
        [
            new(LinenCloth,      2, 15),
            new(WoolCloth,      14, 25),
            new(SilkCloth,      24, 36),
            new(MageweaveCloth, 35, 48),
            new(Runecloth,      47, 63),
        ];

        // Zonen erst nach allen Item-Feldern bauen (Feld-Initialisierer laufen vor dem statischen Konstruktor,
        // ihre Reihenfolge über mehrere partial-Dateien ist aber nicht festgelegt)
        static ProfessionRegistry() => Zones = BuildZones();
    }
}
