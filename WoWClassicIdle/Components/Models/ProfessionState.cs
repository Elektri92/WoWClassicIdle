// Models/ProfessionState.cs
namespace Models
{
    /// <summary>
    /// Passiver Slot einer Sammelzone, mit einem Charakter belegt. Jeder Charakter kann höchstens einen Beruf passiv ausüben.
    /// <see cref="Group"/> = Zonen-ID des Slots (= <see cref="Target"/>).
    /// </summary>
    public class PassiveAssignment
    {
        public required Profession Profession { get; init; }
        public required string Group { get; init; }
        public required int Index { get; init; }
        public required int CharacterId { get; set; }

        /// <summary>Zonen-ID, in der gesammelt wird; <c>null</c> = Slot ruht.</summary>
        public string? Target { get; set; }

        /// <summary>Bereits verstrichene Sekunden des laufenden Durchlaufs.</summary>
        public double Elapsed { get; set; }

        public bool Matches(Profession profession, string group, int index) =>
            Profession == profession && Group == group && Index == index;
    }

    /// <summary>Skill-Stand eines erlernten Berufs inkl. der beim Lehrer gelernten Rezepte.</summary>
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

        /// <summary>Bekannte Rezept-IDs (nur Herstellungsberufe).</summary>
        public IReadOnlyCollection<string> KnownRecipes => _knownRecipes;

        /// <summary>Herstellungsberufe starten mit einem Basisrezept (<see cref="ProfessionTrainerRegistry.StarterRecipe"/>).</summary>
        public ProfessionProgress(Profession profession, CharacterClass? learnerClass = null)
        {
            Definition = Professions.Get(profession);
            if (ProfessionTrainerRegistry.StarterRecipe(profession, learnerClass) is { } starter)
                _knownRecipes.Add(starter.Id);
        }

        public bool KnowsRecipe(string recipeId) => _knownRecipes.Contains(recipeId);

        /// <summary>Lernt ein Rezept dieses Berufs (Kosten/Voraussetzungen prüft die Engine).</summary>
        internal bool LearnRecipe(Recipe recipe) =>
            recipe.Profession == Profession && _knownRecipes.Add(recipe.Id);

        /// <summary>+1 Skill, solange das Cap der aktuellen Stufe nicht erreicht ist.</summary>
        public bool TrySkillUp()
        {
            if (IsCapped) return false;
            Skill++;
            return true;
        }

        /// <summary>Lernt die nächste Ausbildungsstufe, wenn Charakterlevel und Skill reichen.</summary>
        public bool TrainNextRank(int characterLevel)
        {
            if (NextRank is not { } next || characterLevel < next.RequiredLevel || Skill < next.RequiredSkill) return false;
            RankIndex++;
            return true;
        }

        /// <summary>
        /// Stellt Skill, Stufe und Rezepte wieder her. <paramref name="knownRecipes"/> = <c>null</c> (Altstand ohne Lehrer, Debug):
        /// alle Rezepte bis zum aktuellen Skill gelten als gelernt.
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
            // Unbekannte/entfernte Rezept-IDs ignorieren; das Basisrezept aus dem Konstruktor bleibt erhalten
            var valid = recipes.Select(r => r.Id).ToHashSet();
            _knownRecipes.UnionWith(knownRecipes.Where(valid.Contains));
        }
    }

    /// <summary>Hauptberuf eines Charakters (max. einer). Sammel- und Sekundärberufe sind accountweit in der Engine.</summary>
    public class ProfessionState
    {
        public ProfessionProgress? Primary { get; private set; }

        public ProfessionProgress? Get(Profession profession) =>
            Primary?.Profession == profession ? Primary : null;

        public bool IsLearned(Profession profession) => Primary?.Profession == profession;

        /// <summary>Grund, warum der Beruf nicht erlernt werden kann (UI-Text) – <c>null</c> wenn erlaubt.</summary>
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

        /// <summary>Erlernt den Hauptberuf mit Apprentice (Skill 1) und dem Basisrezept passend zur Klasse.</summary>
        public ProfessionProgress? Learn(Profession profession, int characterLevel, CharacterClass? learnerClass = null)
        {
            if (LearnBlocker(profession, characterLevel) is not null) return null;
            return Primary = new ProfessionProgress(profession, learnerClass);
        }

        /// <summary>Verlernt den Hauptberuf – der Skill geht verloren.</summary>
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
