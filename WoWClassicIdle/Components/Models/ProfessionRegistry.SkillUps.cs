// Models/ProfessionRegistry.SkillUps.cs
namespace Models
{
    // Skill range: a recipe/zone only gives skill points until the next material tier of the profession
    // is unlocked (next higher skill requirement among all recipes or zones of the profession).
    // That way there is never a gap without skill points, but old tiers turn "grey" as soon as better materials are usable.
    public static partial class ProfessionRegistry
    {
        private static readonly Lazy<Dictionary<Profession, int[]>> _skillSteps = new(BuildSkillSteps);

        private static Dictionary<Profession, int[]> BuildSkillSteps() =>
            Professions.All.ToDictionary(d => d.Profession, d => SkillRequirements(d).Distinct().Order().ToArray());

        private static IEnumerable<int> SkillRequirements(ProfessionDefinition def)
        {
            if (def.IsGathering) return GetZones(def.Profession).Select(ZoneRequiredSkill);
            // Expensive one-off recipes (IsSkillStep = false) must not turn the tier below them grey
            var recipes = GetRecipes(def.Profession).Where(r => r.IsSkillStep).Select(r => r.RequiredSkill);
            // For Enchanting, disenchanting counts as its own recipe line
            return def.Profession == Profession.Enchanting
                ? recipes.Concat(EnchantingRegistry.Bands.Select(b => b.RequiredSkill))
                : recipes;
        }

        /// <summary>From this skill a tier with <paramref name="requiredSkill"/> gives no more skill points (<c>null</c> = never).</summary>
        public static int? TrivialSkill(Profession profession, int requiredSkill) =>
            _skillSteps.Value.TryGetValue(profession, out var steps) && steps.FirstOrDefault(s => s > requiredSkill) is > 0 and var next
                ? next
                : null;

        public static bool GivesSkillUp(Profession profession, int requiredSkill, int skill) =>
            TrivialSkill(profession, requiredSkill) is not { } trivial || skill < trivial;

        /// <summary>Skill requirement of the target of a pass (zone or recipe); <c>null</c> if unknown.</summary>
        public static int? TargetRequiredSkill(Profession profession, string target) =>
            Professions.Get(profession).IsGathering
                ? GetZone(profession, target) is { } zone ? ZoneRequiredSkill(zone) : null
                : GetRecipe(target)?.RequiredSkill;
    }
}
