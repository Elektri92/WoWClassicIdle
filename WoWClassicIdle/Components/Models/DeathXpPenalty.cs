// Models/DeathXpPenalty.cs
namespace Models;

/// <summary>
/// XP lost per death for one character. Every character that has reached the end of the current
/// ten-level bracket (e.g. level 20+ for levels 10–19) lowers the loss by one percentage point.
/// </summary>
/// <param name="Active">False below the minimum level and at max level, where there is no XP to lose.</param>
/// <param name="Milestone">Level other characters must have reached to lower the loss.</param>
/// <param name="HigherCharacters">Characters of the roster at or above <paramref name="Milestone"/>.</param>
/// <param name="RacialMultiplier">Racial factor on the loss.</param>
public sealed record DeathXpPenalty(bool Active, int Milestone, int HigherCharacters, double RacialMultiplier)
{
    public const int BracketSize = 10;

    /// <summary>Level that ends the bracket of this level; below the minimum level the first bracket counts.</summary>
    public static int MilestoneFor(int level) =>
        (Math.Max(level, Player.DeathXpPenaltyMinLevel) / BracketSize + 1) * BracketSize;

    public int BracketStart => Milestone - BracketSize;

    /// <summary>Percentage points removed by higher characters.</summary>
    public int ReductionPercent => Math.Min(HigherCharacters, Player.DeathXpPenaltyBasePercent);

    /// <summary>Share of the current XP lost per death.</summary>
    public double Share => Active
        ? (Player.DeathXpPenaltyBasePercent - ReductionPercent) / 100.0 * RacialMultiplier
        : 0;
}
