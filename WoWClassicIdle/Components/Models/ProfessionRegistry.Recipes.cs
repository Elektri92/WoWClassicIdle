// Models/ProfessionRegistry.Recipes.cs
namespace Models
{
    // Recipes: consumables from ConsumableRegistry, craftable armor from CraftedGearRegistry.
    // Lazy so the static constructor doesn't run into an init cycle with these registries
    // (both read the raw materials of this class during construction).
    public static partial class ProfessionRegistry
    {
        private static readonly Lazy<IReadOnlyList<Recipe>> _recipes = new(BuildRecipes);

        public static IReadOnlyList<Recipe> Recipes => _recipes.Value;

        private static IReadOnlyList<Recipe> BuildRecipes() =>
        [
            .. ConsumableRegistry.Recipes,
            .. CraftedGearRegistry.Recipes,
            .. EnchantingRegistry.Recipes,
        ];

        public static IReadOnlyList<Recipe> GetRecipes(Profession profession) =>
            Recipes.Where(r => r.Profession == profession).ToList();

        public static Recipe? GetRecipe(string? id) => Recipes.FirstOrDefault(r => r.Id == id);
    }
}
