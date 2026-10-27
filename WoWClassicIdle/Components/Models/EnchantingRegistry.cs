// Models/EnchantingRegistry.cs
namespace Models
{
    /// <summary>Ergebnis-Band beim Entzaubern: Item-Level-Bereich → Staub/Essenz, benötigter Verzauberkunst-Skill.</summary>
    public record DisenchantBand(int MinLevel, int MaxLevel, int RequiredSkill, TradeItem Dust, TradeItem Essence);

    /// <summary>
    /// Verzauberkunst: Entzaubern grüner Ausrüstung (Materialien nach Item-Level) und
    /// herstellbarer Schmuck (Ring + Halskette je Klasse und Stufe, an die Klasse gebunden). IDs ab 9801.
    /// </summary>
    public static class EnchantingRegistry
    {
        // ─── Entzaubern ───

        /// <summary>Rüstung: 75 % Staub, sonst Essenz. Waffen umgekehrt (wie in Classic).</summary>
        private const double ArmorDustChance = 0.75;

        public static readonly IReadOnlyList<DisenchantBand> Bands =
        [
            new( 1, 20,   1, ProfessionRegistry.StrangeDust,  ProfessionRegistry.GreaterMagicEssence),
            new(21, 30,  50, ProfessionRegistry.SoulDust,     ProfessionRegistry.GreaterAstralEssence),
            new(31, 40, 100, ProfessionRegistry.VisionDust,   ProfessionRegistry.GreaterMysticEssence),
            new(41, 50, 150, ProfessionRegistry.DreamDust,    ProfessionRegistry.GreaterNetherEssence),
            new(51, int.MaxValue, 200, ProfessionRegistry.IllusionDust, ProfessionRegistry.GreaterEternalEssence),
        ];

        /// <summary>Nur grüne (Uncommon) Rüstung und Waffen lassen sich entzaubern.</summary>
        public static bool CanDisenchant(Item item) =>
            item.Quality == ItemQuality.Uncommon && item is ArmorItem or WeaponItem;

        public static DisenchantBand BandFor(Item item)
        {
            int level = ItemRequirements.LevelOf(item);
            return Bands.FirstOrDefault(b => level >= b.MinLevel && level <= b.MaxLevel) ?? Bands[0];
        }

        /// <summary>Würfelt das Material: Staub 1–3, Essenz 1–2.</summary>
        public static (TradeItem Item, int Count) RollDisenchant(Item item, Random rng)
        {
            var band = BandFor(item);
            double dustChance = item is WeaponItem ? 1 - ArmorDustChance : ArmorDustChance;
            return rng.NextDouble() < dustChance
                ? (band.Dust, rng.Next(1, 4))
                : (band.Essence, rng.Next(1, 3));
        }

        // ─── Herstellbarer Schmuck ───

        /// <summary>Stufe wie bei CraftedGearRegistry (Lv 5/15/…/55, Skill 1/50/…/250) plus Materialien.</summary>
        private record JewelryTier(
            int Level, int Skill, int Budget, string Prefix,
            TradeItem Dust, int DustCount, TradeItem? Essence, int EssenceCount,
            string RingIcon, string NeckIcon);

        private static readonly JewelryTier[] Tiers =
        [
            new( 5,   1,  3, "Shimmering", ProfessionRegistry.StrangeDust,  3, null,                                      0, "inv_jewelry_ring_02", "inv_jewelry_amulet_01"),
            new(15,  50,  6, "Glowing",    ProfessionRegistry.StrangeDust,  4, ProfessionRegistry.GreaterMagicEssence,   1, "inv_jewelry_ring_05", "inv_jewelry_amulet_02"),
            new(25, 100, 13, "Astral",     ProfessionRegistry.SoulDust,     4, ProfessionRegistry.GreaterAstralEssence,  1, "inv_jewelry_ring_08", "inv_jewelry_amulet_03"),
            new(35, 150, 21, "Mystic",     ProfessionRegistry.VisionDust,   5, ProfessionRegistry.GreaterMysticEssence,  1, "inv_jewelry_ring_11", "inv_jewelry_amulet_04"),
            new(45, 200, 28, "Nether",     ProfessionRegistry.DreamDust,    5, ProfessionRegistry.GreaterNetherEssence,  2, "inv_jewelry_ring_13", "inv_jewelry_amulet_05"),
            new(55, 250, 35, "Eternal",    ProfessionRegistry.IllusionDust, 6, ProfessionRegistry.GreaterEternalEssence, 2, "inv_jewelry_ring_15", "inv_jewelry_amulet_07"),
        ];

        /// <summary>Anteil am Stufen-Budget (wie Hände/Füße bei hergestellter Rüstung).</summary>
        private const double SlotWeight = 0.75;

        /// <summary>Klassen-Set: Name (wie die Rüstungssets) und Stat-Gewichtung (Summe 1).</summary>
        private record ClassSet(
            CharacterClass Class, string SetName,
            double Str = 0, double Agi = 0, double Sta = 0, double Int = 0, double Spi = 0);

        // Reihenfolge nicht ändern – IDs (Spielstand) hängen daran
        private static readonly ClassSet[] Sets =
        [
            new(CharacterClass.Warrior, "Vanguard",    Str: 0.45, Sta: 0.40, Agi: 0.15),
            new(CharacterClass.Paladin, "Lightforged", Str: 0.40, Sta: 0.30, Int: 0.30),
            new(CharacterClass.Hunter,  "Beastmaster", Agi: 0.50, Sta: 0.30, Int: 0.20),
            new(CharacterClass.Rogue,   "Stalker",     Agi: 0.50, Sta: 0.30, Str: 0.20),
            new(CharacterClass.Priest,  "Devout",      Int: 0.40, Spi: 0.35, Sta: 0.25),
            new(CharacterClass.Shaman,  "Earthcaller", Int: 0.35, Agi: 0.25, Sta: 0.25, Spi: 0.15),
            new(CharacterClass.Mage,    "Arcanist",    Int: 0.50, Sta: 0.30, Spi: 0.20),
            new(CharacterClass.Warlock, "Shadoweave",  Int: 0.40, Sta: 0.45, Spi: 0.15),
            new(CharacterClass.Druid,   "Wildheart",   Int: 0.35, Agi: 0.30, Sta: 0.20, Spi: 0.15),
        ];

        public static readonly IReadOnlyList<ArmorItem> Items;
        public static readonly IReadOnlyList<Recipe> Recipes;
        private static readonly Dictionary<int, ArmorItem> _byId = new();

        public static ArmorItem? GetById(int id) => _byId.TryGetValue(id, out var item) ? item : null;

        static EnchantingRegistry()
        {
            var items = new List<ArmorItem>();
            var recipes = new List<Recipe>();
            int id = 9801;

            // Reihenfolge nicht ändern – IDs (Spielstand) und Rezept-IDs hängen daran
            foreach (var tier in Tiers)
            {
                foreach (var set in Sets)
                {
                    foreach (var (slot, piece, icon, extraDust) in new[]
                             {
                                 (ArmorSlot.Ring, "Band",    tier.RingIcon, 0),
                                 (ArmorSlot.Neck, "Pendant", tier.NeckIcon, 1),
                             })
                    {
                        var item = new ArmorItem(id++, $"{tier.Prefix} {set.SetName} {piece}", icon, slot,
                            Stats(tier.Budget * SlotWeight, set), tier.Level, ItemQuality.Uncommon,
                            SellPrice: 12L * tier.Level * tier.Level, RequiredClass: set.Class);

                        var reagents = new List<Reagent> { new(tier.Dust, tier.DustCount + extraDust) };
                        if (tier.Essence is { } essence)
                            reagents.Add(new Reagent(essence, tier.EssenceCount));

                        items.Add(item);
                        _byId[item.Id] = item;
                        recipes.Add(new Recipe($"ench_{item.Id}", Profession.Enchanting, item, reagents, tier.Skill));
                    }
                }
            }

            Items = items;
            Recipes = recipes;
        }

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
