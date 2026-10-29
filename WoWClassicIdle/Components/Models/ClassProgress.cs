// Models/ClassProgress.cs
namespace Models;

public class ClassProgress
{
    public CharacterClass Class { get; }
    public int Level { get; private set; } = 1;
    public int Xp { get; private set; }
    public int XpToNextLevel { get; private set; }

    public ClassProgress(CharacterClass characterClass)
    {
        Class = characterClass;
        XpToNextLevel = StaticHelpers.GetXpForLevel(1);
    }

    /// <summary>
    /// Adds XP and levels up as often as needed.
    /// Returns how many level-ups occurred.
    /// </summary>
    public int GainXp(int amount)
    {
        Xp += amount;
        int levelsGained = 0;

        // XpToNextLevel == 0 → max level reached
        while (XpToNextLevel > 0 && Xp >= XpToNextLevel)
        {
            Xp -= XpToNextLevel;
            Level++;
            XpToNextLevel = StaticHelpers.GetXpForLevel(Level);
            levelsGained++;
        }

        // No XP accumulates at max level
        if (XpToNextLevel == 0) Xp = 0;

        return levelsGained;
    }

    /// <summary>
    /// Deducts a share of the current XP (e.g. on death).
    /// Losing a level is impossible – XP drops to 0 at most.
    /// Returns the XP actually lost.
    /// </summary>
    public int LoseXpPercent(double percent)
    {
        int lost = (int)Math.Floor(Xp * percent);
        Xp = Math.Max(0, Xp - lost);
        return lost;
    }

    /// <summary>
    /// Sets the class directly to a level (XP = 0), e.g. for simulations.
    /// </summary>
    public void SetLevel(int level)
    {
        Level = Math.Max(1, level);
        Xp = 0;
        XpToNextLevel = StaticHelpers.GetXpForLevel(Level);
    }

    /// <summary>
    /// Sets level and XP directly (for restoring a save).
    /// </summary>
    internal void Restore(int level, int xp)
    {
        Level = Math.Max(1, level);
        XpToNextLevel = StaticHelpers.GetXpForLevel(Level);
        Xp = Math.Clamp(xp, 0, XpToNextLevel > 0 ? XpToNextLevel - 1 : 0);
    }

    /// <summary>
    /// Reset to level 1 (e.g. for a class never played).
    /// </summary>
    public void ResetToLevelOne()
    {
        Level = 1;
        Xp = 0;
        XpToNextLevel = StaticHelpers.GetXpForLevel(1);
    }
}
