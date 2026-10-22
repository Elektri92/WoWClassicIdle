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

    /// <summary>Reihe N erfordert N*5 Punkte im selben Baum (WoW-Classic-Regel).</summary>
    public bool IsRowUnlocked(CharacterClass cls, int treeIndex, int row) =>
        row == 0 || TotalInvestedInTree(cls, treeIndex) >= row * 5;

    public bool CanInvest(TalentDefinition talent, int availablePoints)
    {
        if (availablePoints <= 0) return false;
        if (GetRanks(talent.Id) >= talent.MaxRanks) return false;
        if (!IsRowUnlocked(talent.Class, talent.TreeIndex, talent.Row)) return false;
        if (talent.RequiresTalentId is not null && GetRanks(talent.RequiresTalentId) == 0) return false;
        return true;
    }

    /// <summary>Investiert einen Punkt. Gibt true zurück wenn erfolgreich.</summary>
    public bool Invest(TalentDefinition talent, int availablePoints)
    {
        if (!CanInvest(talent, availablePoints)) return false;
        _invested[talent.Id] = GetRanks(talent.Id) + 1;
        return true;
    }

    public void Reset(CharacterClass cls)
    {
        foreach (var talent in TalentRegistry.GetTalents(cls))
            _invested.Remove(talent.Id);
    }

    /// <summary>Für Spielstand-Wiederherstellung.</summary>
    internal void RestoreRanks(string talentId, int ranks)
    {
        if (ranks > 0)
            _invested[talentId] = ranks;
        else
            _invested.Remove(talentId);
    }

    /// <summary>Summe aller investierten Ränge × EffectPerRank für den gegebenen Effekttyp.</summary>
    public double GetTotalBonus(CharacterClass cls, TalentEffectType type) =>
        TalentRegistry.GetTalents(cls)
            .Where(t => t.EffectType == type)
            .Sum(t => GetRanks(t.Id) * t.EffectPerRank);

    public double GetTotalCritBonus(CharacterClass cls) =>
        GetTotalBonus(cls, TalentEffectType.CritChance);
}
