namespace Models;

/// <summary>
/// All visible ranks of a spell (same name) for the spellbook in /ability-loadout.
/// </summary>
/// <param name="Ranks">Visible ranks, ascending by rank.</param>
/// <param name="Unlocked">Ranks of those already usable (level reached or unlocked by talent).</param>
/// <param name="Equipped">Currently equipped rank or null.</param>
public sealed record SpellRankGroup(string Name, IReadOnlyList<Ability> Ranks, IReadOnlyList<Ability> Unlocked, Ability? Equipped)
{
    /// <summary>Highest usable rank.</summary>
    public Ability? Best => Unlocked.Count > 0 ? Unlocked[^1] : null;

    /// <summary>Displayed rank: equipped → best usable → first locked.</summary>
    public Ability Display => Equipped ?? Best ?? Ranks[0];

    public bool MultiRank => Ranks.Count > 1;
    public bool IsLocked => Unlocked.Count == 0;
    public bool CanUpgrade => Equipped is not null && Best is not null && Best.Rank > Equipped.Rank;
}
