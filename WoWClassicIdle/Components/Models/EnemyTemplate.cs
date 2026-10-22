// Combat/EnemyTemplate.cs
public class EnemyTemplate
{
    public string Name        { get; init; } = "";
    public string Type        { get; init; } = "";
    public string? ImageUrl   { get; init; }
    public int MinLevel       { get; init; } = 1;
    public int MaxLevel       { get; init; } = 999;
    public int BaseMaxHp      { get; init; }
    public IReadOnlyList<int>? MaxHpByLevel { get; init; } // Index 0 = MinLevel; überschreibt BaseMaxHp

    public int GetMaxHp(int level)
    {
        if (MaxHpByLevel is not { Count: > 0 })
            return BaseMaxHp;

        var index = Math.Clamp(level - MinLevel, 0, MaxHpByLevel.Count - 1);
        return MaxHpByLevel[index];
    }
    public int BaseAttack     { get; init; }
    public int BaseDefense    { get; init; }
    public double BaseSpeed   { get; init; } = 1.0;
    public int MinCopperReward { get; init; }   // Loot-Spanne auf MinLevel, +1 Kupfer je Level darüber
    public int MaxCopperReward { get; init; }
    public int BaseXpReward   { get; init; }
    public int Weight         { get; init; } = 1;   // Spawn-Wahrscheinlichkeit (relativ)
}
