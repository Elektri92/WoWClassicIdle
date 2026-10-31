// Models/CraftedTrinketRegistry.cs
namespace Models
{
    /// <summary>
    /// Expensive craftable trinkets for all classes: Alchemist's Stones (one per potion tier) and an Enchanting charm.
    /// Reached in the middle of a skill tier without shifting the skill range (<see cref="Recipe.IsSkillStep"/>).
    /// IDs 9910–9916.
    /// </summary>
    public static class CraftedTrinketRegistry
    {
        private const int FirstId = 9910;
        private const int MaterialsPerKind = 100;
        private const double StonePotionBonus = 0.30;
        private const int SkillTierWidth = 50;

        private static readonly string[] StoneIcons =
            ["inv_misc_gem_01", "inv_misc_gem_02", "inv_misc_gem_03", "inv_misc_gem_opal_01", "inv_misc_gem_bloodstone_01", "inv_misc_gem_diamond_02"];

        public static readonly IReadOnlyList<ArmorItem> Items;
        public static readonly IReadOnlyList<Recipe> Recipes;
        private static readonly Dictionary<int, ArmorItem> _byId = new();

        public static ArmorItem? GetById(int id) => _byId.TryGetValue(id, out var item) ? item : null;

        static CraftedTrinketRegistry()
        {
            var items = new List<ArmorItem>();
            var recipes = new List<Recipe>();
            int id = FirstId;

            void Add(string name, string icon, int level, EquipEffect effect, Profession profession, int skill, List<Reagent> reagents)
            {
                // Sells below the value of its materials
                long sellPrice = reagents.Sum(r => r.Item.SellPrice * r.Count) / 2;
                var item = new ArmorItem(id++, name, icon, ArmorSlot.Trinket, PrimaryStats.Zero, level, ItemQuality.Rare, sellPrice, Effect: effect);
                items.Add(item);
                _byId[item.Id] = item;
                recipes.Add(new Recipe($"trinket_{item.Id}", profession, item, reagents, skill, IsSkillStep: false));
            }

            // Do not change the order – IDs (save game) and recipe IDs depend on it
            var herbs = ConsumableRegistry.HerbTiers();
            for (int t = 0; t < herbs.Length; t++)
            {
                string prefix = ConsumableRegistry.PotionNames[t];
                string tierName = prefix.Length > 0 ? $"{prefix} " : "";
                int level = ConsumableRegistry.TierLevel[t];
                var effect = new EquipEffect(
                    $"Equip: Level {level} Healing and Mana Potions restore {StonePotionBonus * 100:0}% more.",
                    PotionLevel: level, PotionBonus: StonePotionBonus);
                Add($"{tierName}Alchemist's Stone", StoneIcons[t], level, effect, Profession.Alchemy,
                    t * SkillTierWidth + SkillTierWidth / 2,
                    herbs[t].Select(h => new Reagent(h, MaterialsPerKind)).ToList());
            }

            Add("Charm of Keen Insight", "inv_jewelry_talisman_12", 10,
                new EquipEffect("Equip: +1% Critical Strike chance.", CritChance: 0.01),
                Profession.Enchanting, 25,
                [new Reagent(ProfessionRegistry.StrangeDust, MaterialsPerKind), new Reagent(ProfessionRegistry.GreaterMagicEssence, MaterialsPerKind)]);

            Items = items;
            Recipes = recipes;
        }
    }
}
