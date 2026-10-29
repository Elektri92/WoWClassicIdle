// Models/ProfessionTrainerRegistry.cs
namespace Models
{
    /// <summary>Profession trainer: NPC data and speech bubble lines per occasion (random, never the same one twice in a row).</summary>
    /// <param name="Race">Part of the Wowhead portrait URL, e.g. <c>dwarf_male</c>.</param>
    public record ProfessionTrainer(
        Profession Profession,
        string Name,
        string Race,
        string Greeting,
        IReadOnlyList<string> GreetingLines,
        IReadOnlyList<string> TrainLines,
        IReadOnlyList<string> RefuseLines,
        IReadOnlyList<string> FarewellLines)
    {
        public string Title => $"{Professions.Get(Profession).Name} Trainer";
        public string PortraitUrl => $"https://wow.zamimg.com/images/wow/icons/large/race_{Race}.jpg";
        public string BadgeUrl => Professions.Get(Profession).IconUrl;
    }

    /// <summary>
    /// Trainer per profession (data in <c>ProfessionTrainerRegistry.Trainers.cs</c>), recipe costs and basic recipe.
    /// Training tiers cost <see cref="ProfessionRank.Cost"/>.
    /// </summary>
    public static partial class ProfessionTrainerRegistry
    {
        public static ProfessionTrainer Get(Profession profession) =>
            Trainers.First(t => t.Profession == profession);

        /// <summary>Recipe cost in copper: rises quadratically with the skill (skill 1 ≈ 10 copper, skill 250 ≈ 16 silver).</summary>
        public static long RecipeCost(Recipe recipe)
        {
            long raw = 10 + (long)recipe.RequiredSkill * recipe.RequiredSkill / 40;
            // From 1 silver round to even 10 copper, below that to 5
            int step = raw >= 100 ? 10 : 5;
            return Math.Max(step, (raw + step / 2) / step * step);
        }

        /// <summary>
        /// Basic recipe every crafting profession starts with: lowest skill tier,
        /// preferably matching the learner's class, otherwise classless, otherwise the first one.
        /// </summary>
        public static Recipe? StarterRecipe(Profession profession, CharacterClass? learnerClass)
        {
            if (Professions.Get(profession).IsGathering) return null;
            var recipes = ProfessionRegistry.GetRecipes(profession);
            if (recipes.Count == 0) return null;
            int minSkill = recipes.Min(r => r.RequiredSkill);
            var basics = recipes.Where(r => r.RequiredSkill == minSkill).ToList();
            return basics.FirstOrDefault(r => learnerClass is not null && ItemRequirements.ClassOf(r.Result) == learnerClass)
                ?? basics.FirstOrDefault(r => ItemRequirements.ClassOf(r.Result) is null)
                ?? basics[0];
        }
    }
}
