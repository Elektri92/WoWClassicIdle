// Models/ProfessionRegistry.Recipes.cs
namespace Models
{
    // Rezepte: Verbrauchsgüter aus ConsumableRegistry, herstellbare Rüstung aus CraftedGearRegistry.
    // Lazy, damit der statische Konstruktor nicht in einen Init-Zyklus mit diesen Registries läuft
    // (beide lesen beim Aufbau die Rohstoffe dieser Klasse).
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
