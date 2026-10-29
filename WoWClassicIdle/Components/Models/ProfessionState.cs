// Models/ProfessionState.cs
namespace Models
{
    /// <summary>
    /// Passive slot of a gathering zone, filled with a character. Each character can practice at most one profession passively.
    /// <see cref="Group"/> = zone ID of the slot (= <see cref="Target"/>).
    /// </summary>
    public class PassiveAssignment
    {
        public required Profession Profession { get; init; }
        public required string Group { get; init; }
        public required int Index { get; init; }
        public required int CharacterId { get; set; }

        /// <summary>Zone ID being gathered in; <c>null</c> = slot is idle.</summary>
        public string? Target { get; set; }

        /// <summary>Seconds already elapsed of the running pass.</summary>
        public double Elapsed { get; set; }

        public bool Matches(Profession profession, string group, int index) =>
            Profession == profession && Group == group && Index == index;
    }

    /// <summary>Skill state of a learned profession incl. the recipes learned from the trainer.</summary>
    public class ProfessionProgress
    {
        private readonly HashSet<string> _knownRecipes = new();

        public ProfessionDefinition Definition { get; }
        public Profession Profession => Definition.Profession;
        public int Skill { get; private set; } = 1;
        public int RankIndex { get; private set; }

        public ProfessionRank Rank => Professions.Ranks[RankIndex];
        public int SkillCap => Rank.MaxSkill;
        public bool IsCapped => Skill >= SkillCap;

        public ProfessionRank? NextRank =>
            RankIndex + 1 < Professions.Ranks.Count ? Professions.Ranks[RankIndex + 1] : null;

        /// <summary>Known recipe IDs (crafting professions only).</summary>
        public IReadOnlyCollection<string> KnownRecipes => _knownRecipes;

        /// <summary>Crafting professions start with a basic recipe (<see cref="ProfessionTrainerRegistry.StarterRecipe"/>).</summary>
        public ProfessionProgress(Profession profession, CharacterClass? learnerClass = null)
        {
            Definition = Professions.Get(profession);
            if (ProfessionTrainerRegistry.StarterRecipe(profession, learnerClass) is { } starter)
                _knownRecipes.Add(starter.Id);
        }

        public bool KnowsRecipe(string recipeId) => _knownRecipes.Contains(recipeId);

        /// <summary>Learns a recipe of this profession (the engine checks cost/requirements).</summary>
        internal bool LearnRecipe(Recipe recipe) =>
            recipe.Profession == Profession && _knownRecipes.Add(recipe.Id);

        /// <summary>+1 skill as long as the cap of the current tier is not reached.</summary>
        public bool TrySkillUp()
        {
            if (IsCapped) return false;
            Skill++;
            return true;
        }

        /// <summary>Learns the next training tier if character level and skill are sufficient.</summary>
        public bool TrainNextRank(int characterLevel)
        {
            if (NextRank is not { } next || characterLevel < next.RequiredLevel || Skill < next.RequiredSkill) return false;
            RankIndex++;
            return true;
        }

        /// <summary>
        /// Restores skill, tier and recipes. <paramref name="knownRecipes"/> = <c>null</c> (legacy save without trainer, debug):
        /// all recipes up to the current skill count as learned.
        /// </summary>
        internal void Restore(int skill, int rankIndex, IEnumerable<string>? knownRecipes = null)
        {
            RankIndex = Math.Clamp(rankIndex, 0, Professions.Ranks.Count - 1);
            Skill = Math.Clamp(skill, 1, SkillCap);

            var recipes = ProfessionRegistry.GetRecipes(Profession);
            if (knownRecipes is null)
            {
                foreach (var recipe in recipes.Where(r => r.RequiredSkill <= Skill))
                    _knownRecipes.Add(recipe.Id);
                return;
            }
            // Ignore unknown/removed recipe IDs; the basic recipe from the constructor is kept
            var valid = recipes.Select(r => r.Id).ToHashSet();
            _knownRecipes.UnionWith(knownRecipes.Where(valid.Contains));
        }
    }

    /// <summary>Main profession of a character (max. one). Gathering and secondary professions are account-wide in the engine.</summary>
    public class ProfessionState
    {
        public ProfessionProgress? Primary { get; private set; }

        public ProfessionProgress? Get(Profession profession) =>
            Primary?.Profession == profession ? Primary : null;

        public bool IsLearned(Profession profession) => Primary?.Profession == profession;

        /// <summary>Reason why the profession cannot be learned (UI text) – <c>null</c> if allowed.</summary>
        public string? LearnBlocker(Profession profession, int characterLevel)
        {
            if (!Professions.Get(profession).IsPrimary)
                return $"Learned automatically at level {Professions.MinCharacterLevel}";
            if (characterLevel < Professions.MinCharacterLevel)
                return $"Requires level {Professions.MinCharacterLevel}";
            if (IsLearned(profession))
                return "Already learned";
            if (Primary is { } primary)
                return $"Unlearn {primary.Definition.Name} first";
            return null;
        }

        /// <summary>Learns the main profession at Apprentice (skill 1) with the basic recipe matching the class.</summary>
        public ProfessionProgress? Learn(Profession profession, int characterLevel, CharacterClass? learnerClass = null)
        {
            if (LearnBlocker(profession, characterLevel) is not null) return null;
            return Primary = new ProfessionProgress(profession, learnerClass);
        }

        /// <summary>Unlearns the main profession – the skill is lost.</summary>
        public bool Unlearn(Profession profession)
        {
            if (!IsLearned(profession)) return false;
            Primary = null;
            return true;
        }

        internal void Restore(Profession profession, int skill, int rankIndex, IEnumerable<string>? knownRecipes, CharacterClass? learnerClass)
        {
            if (!Professions.Get(profession).IsPrimary) return;
            Primary = new ProfessionProgress(profession, learnerClass);
            Primary.Restore(skill, rankIndex, knownRecipes);
        }
    }
}
