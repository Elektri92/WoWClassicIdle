// Models/ProfessionRegistry.SkillUps.cs
namespace Models
{
    // Skill-Range: Ein Rezept/eine Zone gibt nur Skillpunkte, bis die nächste Material-Stufe des Berufs
    // freigeschaltet ist (nächsthöhere Skill-Anforderung unter allen Rezepten bzw. Zonen des Berufs).
    // So gibt es nie eine Lücke ohne Skillpunkte, alte Stufen werden aber "grau", sobald bessere Materialien nutzbar sind.
    public static partial class ProfessionRegistry
    {
        private static readonly Lazy<Dictionary<Profession, int[]>> _skillSteps = new(BuildSkillSteps);

        private static Dictionary<Profession, int[]> BuildSkillSteps() =>
            Professions.All.ToDictionary(d => d.Profession, d => SkillRequirements(d).Distinct().Order().ToArray());

        private static IEnumerable<int> SkillRequirements(ProfessionDefinition def)
        {
            if (def.IsGathering) return GetZones(def.Profession).Select(ZoneRequiredSkill);
            var recipes = GetRecipes(def.Profession).Select(r => r.RequiredSkill);
            // Entzaubern zählt bei Verzauberkunst wie eine eigene Rezeptreihe
            return def.Profession == Profession.Enchanting
                ? recipes.Concat(EnchantingRegistry.Bands.Select(b => b.RequiredSkill))
                : recipes;
        }

        /// <summary>Ab diesem Skill gibt eine Stufe mit <paramref name="requiredSkill"/> keine Skillpunkte mehr (<c>null</c> = nie).</summary>
        public static int? TrivialSkill(Profession profession, int requiredSkill) =>
            _skillSteps.Value.TryGetValue(profession, out var steps) && steps.FirstOrDefault(s => s > requiredSkill) is > 0 and var next
                ? next
                : null;

        public static bool GivesSkillUp(Profession profession, int requiredSkill, int skill) =>
            TrivialSkill(profession, requiredSkill) is not { } trivial || skill < trivial;

        /// <summary>Skill-Anforderung des Ziels eines Durchlaufs (Zone bzw. Rezept); <c>null</c> wenn unbekannt.</summary>
        public static int? TargetRequiredSkill(Profession profession, string target) =>
            Professions.Get(profession).IsGathering
                ? GetZone(profession, target) is { } zone ? ZoneRequiredSkill(zone) : null
                : GetRecipe(target)?.RequiredSkill;
    }
}
