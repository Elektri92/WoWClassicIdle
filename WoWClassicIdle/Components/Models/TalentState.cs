// Models/TalentState.cs
namespace Models;

public class TalentState
{
    private readonly Dictionary<string, int> _invested = new();

    public IReadOnlyDictionary<string, int> Invested => _invested;

    public int GetRanks(string talentId) =>
        _invested.TryGetValue(talentId, out var r) ? r : 0;

    public int TotalInvestedInTree(CharacterClass cls, int treeIndex) =>
        TalentRegistry.GetTalents(cls)
            .Where(t => t.TreeIndex == treeIndex)
            .Sum(t => GetRanks(t.Id));

    public int TotalInvested(CharacterClass cls) =>
        TalentRegistry.GetTalents(cls).Sum(t => GetRanks(t.Id));

    /// <summary>Points invested in the rows above <paramref name="row"/> of the same tree.</summary>
    public int PointsBeforeRow(CharacterClass cls, int treeIndex, int row) =>
        TalentRegistry.GetTalents(cls)
            .Where(t => t.TreeIndex == treeIndex && t.Row < row)
            .Sum(t => GetRanks(t.Id));

    /// <summary>Row N requires N*5 points in the earlier rows of the same tree (WoW Classic rule).</summary>
    public bool IsRowUnlocked(CharacterClass cls, int treeIndex, int row) =>
        row == 0 || PointsBeforeRow(cls, treeIndex, row) >= row * TalentRegistry.PointsPerRow;

    public bool CanInvest(TalentDefinition talent, int availablePoints, int level) =>
        InvestBlocker(talent, availablePoints, level) is null;

    /// <summary>Why a point cannot be invested right now; null = it can.</summary>
    public string? InvestBlocker(TalentDefinition talent, int availablePoints, int level)
    {
        if (GetRanks(talent.Id) >= talent.MaxRanks) return $"{talent.Name} is already at maximum rank.";
        var missing = MissingRequirements(talent, level).FirstOrDefault();
        if (missing is not null) return missing;
        if (availablePoints <= 0) return "No talent points available.";
        return null;
    }

    /// <summary>All unmet requirements of the talent as player-facing lines (empty = all met).</summary>
    public IEnumerable<string> MissingRequirements(TalentDefinition talent, int level)
    {
        if (talent.ExclusiveWithTalentId is { } exclusiveId && GetRanks(exclusiveId) > 0)
            yield return $"Cannot be learned together with {TalentRegistry.GetById(talent.Class, exclusiveId)?.Name ?? exclusiveId}";
        if (level < talent.LevelRequired)
            yield return $"Requires Level {talent.LevelRequired}";
        if (!IsRowUnlocked(talent.Class, talent.TreeIndex, talent.Row))
            yield return $"Requires {talent.Row * TalentRegistry.PointsPerRow} points in {TalentRegistry.GetTreeName(talent)}";
        if (talent.RequiresTalentId is { } requiredId && GetRanks(requiredId) == 0)
            yield return $"Requires {TalentRegistry.GetById(talent.Class, requiredId)?.Name ?? requiredId}";
    }

    /// <summary>Invests a point. Returns true if successful.</summary>
    public bool Invest(TalentDefinition talent, int availablePoints, int level)
    {
        if (!CanInvest(talent, availablePoints, level)) return false;
        _invested[talent.Id] = GetRanks(talent.Id) + 1;
        return true;
    }

    /// <summary>
    /// Why a point cannot be removed; null = it can. Removing must keep every dependent talent
    /// and the point requirement of every later row with points intact.
    /// </summary>
    public string? RemoveBlocker(TalentDefinition talent)
    {
        int ranks = GetRanks(talent.Id);
        if (ranks == 0) return $"{talent.Name} has no points to remove.";

        var treeTalents = TalentRegistry.GetTalents(talent.Class).Where(t => t.TreeIndex == talent.TreeIndex).ToList();
        if (ranks == 1 && treeTalents.FirstOrDefault(t => t.RequiresTalentId == talent.Id && GetRanks(t.Id) > 0) is { } dependent)
            return $"Cannot unlearn {talent.Name}: {dependent.Name} depends on it.";

        foreach (var other in treeTalents.Where(t => t.Row > talent.Row && GetRanks(t.Id) > 0).OrderBy(t => t.Row))
        {
            int required = other.Row * TalentRegistry.PointsPerRow;
            if (PointsBeforeRow(talent.Class, talent.TreeIndex, other.Row) - 1 < required)
                return $"Cannot unlearn {talent.Name}: {other.Name} needs {required} points in the rows above it.";
        }
        return null;
    }

    /// <summary>Removes a point. Returns true if successful.</summary>
    public bool Remove(TalentDefinition talent)
    {
        if (RemoveBlocker(talent) is not null) return false;
        RestoreRanks(talent.Id, GetRanks(talent.Id) - 1);
        return true;
    }

    /// <summary>
    /// Do all invested talents of the class still meet their requirements (required/exclusive talent, row points)?
    /// Saves from older versions can violate rules that were added later.
    /// </summary>
    public bool IsConsistent(CharacterClass cls) =>
        TalentRegistry.GetTalents(cls).Where(t => GetRanks(t.Id) > 0).All(t =>
            (t.RequiresTalentId is null || GetRanks(t.RequiresTalentId) > 0)
            && (t.ExclusiveWithTalentId is null || GetRanks(t.ExclusiveWithTalentId) == 0)
            && IsRowUnlocked(cls, t.TreeIndex, t.Row));

    /// <summary>
    /// After loading: talent rules may have changed since the save (e.g. Bear Form and Cat Form are exclusive now).
    /// Refunds the whole class instead of keeping an impossible build.
    /// </summary>
    public void ResetInconsistentClasses()
    {
        foreach (var cls in Enum.GetValues<CharacterClass>())
            if (!IsConsistent(cls))
                Reset(cls);
    }

    public void Reset(CharacterClass cls)
    {
        foreach (var talent in TalentRegistry.GetTalents(cls))
            _invested.Remove(talent.Id);
    }

    /// <summary>For restoring a save game.</summary>
    internal void RestoreRanks(string talentId, int ranks)
    {
        if (ranks > 0)
            _invested[talentId] = ranks;
        else
            _invested.Remove(talentId);
    }

    /// <summary>Sum of all invested ranks × EffectPerRank for the given effect type.</summary>
    public double GetTotalBonus(CharacterClass cls, TalentEffectType type) =>
        TalentRegistry.GetTalents(cls)
            .Where(t => t.EffectType == type)
            .Sum(t => GetRanks(t.Id) * t.EffectPerRank);

    public double GetTotalCritBonus(CharacterClass cls) =>
        GetTotalBonus(cls, TalentEffectType.CritChance);
}
