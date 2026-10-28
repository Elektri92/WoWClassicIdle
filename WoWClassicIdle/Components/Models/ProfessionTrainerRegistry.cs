// Models/ProfessionTrainerRegistry.cs
namespace Models
{
    /// <summary>Berufslehrer: NPC-Daten und Sprechblasen-Sprüche je Anlass (zufällig, nie zweimal hintereinander derselbe).</summary>
    /// <param name="Race">Teil der Wowhead-Porträt-URL, z. B. <c>dwarf_male</c>.</param>
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
    /// Lehrer je Beruf (Daten in <c>ProfessionTrainerRegistry.Trainers.cs</c>), Rezeptkosten und Basisrezept.
    /// Ausbildungsstufen kosten <see cref="ProfessionRank.Cost"/>.
    /// </summary>
    public static partial class ProfessionTrainerRegistry
    {
        public static ProfessionTrainer Get(Profession profession) =>
            Trainers.First(t => t.Profession == profession);

        /// <summary>Rezeptkosten in Kupfer: steigen quadratisch mit dem Skill (Skill 1 ≈ 10 Kupfer, Skill 250 ≈ 16 Silber).</summary>
        public static long RecipeCost(Recipe recipe)
        {
            long raw = 10 + (long)recipe.RequiredSkill * recipe.RequiredSkill / 40;
            // Ab 1 Silber auf glatte 10 Kupfer runden, darunter auf 5
            int step = raw >= 100 ? 10 : 5;
            return Math.Max(step, (raw + step / 2) / step * step);
        }

        /// <summary>
        /// Basisrezept, mit dem jeder Herstellungsberuf startet: niedrigste Skill-Stufe,
        /// bevorzugt passend zur Klasse des Lernenden, sonst klassenlos, sonst das erste.
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
