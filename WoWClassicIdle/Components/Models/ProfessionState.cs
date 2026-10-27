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

    /// <summary>Skill-Stand eines erlernten Berufs.</summary>
    public class ProfessionProgress
    {
        public ProfessionDefinition Definition { get; }
        public Profession Profession => Definition.Profession;
        public int Skill { get; private set; } = 1;
        public int RankIndex { get; private set; }

        public ProfessionRank Rank => Professions.Ranks[RankIndex];
        public int SkillCap => Rank.MaxSkill;
        public bool IsCapped => Skill >= SkillCap;

        public ProfessionRank? NextRank =>
            RankIndex + 1 < Professions.Ranks.Count ? Professions.Ranks[RankIndex + 1] : null;

        public ProfessionProgress(Profession profession) => Definition = Professions.Get(profession);

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

        internal void Restore(int skill, int rankIndex)
        {
            RankIndex = Math.Clamp(rankIndex, 0, Professions.Ranks.Count - 1);
            Skill = Math.Clamp(skill, 1, SkillCap);
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

        /// <summary>Erlernt den Hauptberuf mit Apprentice (Skill 1).</summary>
        public ProfessionProgress? Learn(Profession profession, int characterLevel)
        {
            if (LearnBlocker(profession, characterLevel) is not null) return null;
            return Primary = new ProfessionProgress(profession);
        }

        /// <summary>Verlernt den Hauptberuf – der Skill geht verloren.</summary>
        public bool Unlearn(Profession profession)
        {
            if (!IsLearned(profession)) return false;
            Primary = null;
            return true;
        }

        internal void Restore(Profession profession, int skill, int rankIndex)
        {
            if (!Professions.Get(profession).IsPrimary) return;
            Primary = new ProfessionProgress(profession);
            Primary.Restore(skill, rankIndex);
        }
    }
}
