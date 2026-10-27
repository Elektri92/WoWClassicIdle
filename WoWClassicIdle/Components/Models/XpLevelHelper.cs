// Models/StaticHelpers.cs
namespace Models;

public static partial class StaticHelpers
{
    // Tabellen reichen bis Level 60; im Release ist das Spiel vorerst auf ReleaseLevelCap begrenzt.
    private const int TableMaxLevel   = 60;
    public  const int ReleaseLevelCap = 20;

    public static int MaxLevel =>
        WoWClassicIdle.Components.BuildInfo.IsDebug ? TableMaxLevel : ReleaseLevelCap;

    // XP needed to go from level N to level N+1 (index 0 = level 1 → 2)
    private static readonly int[] XpTable =
    {
         400,   900,  1400,  2100,  2800,  3600,  4500,  5400,  6500,  7600,
        8800, 10100, 11400, 12900, 14400, 16000, 17700, 19400, 21300, 23200,
       25200, 27300, 29400, 31700, 34000, 36400, 38900, 41400, 44300, 47400,
       50800, 54500, 58600, 62800, 67100, 71600, 76100, 80800, 85700, 90700,
       95800,101000,106300,111800,117500,123200,129100,135100,141200,147500,
      153900,160400,167100,173900,180800,187900,195000,202300,209800
    };

    // XP-Reward a defeated enemy of this level grants (index 0 = level 1)
    private static readonly int[] EnemyXpReward =
    {
        50,  55,  60,  65,  70,  75,  80,  85,  90,  95,
       100, 105, 110, 115, 120, 125, 130, 135, 140, 145,
       150, 155, 160, 165, 170, 175, 180, 185, 190, 195,
       200, 205, 210, 215, 220, 225, 230, 235, 240, 245,
       250, 255, 260, 265, 270, 275, 280, 285, 290, 295,
       300, 305, 310, 315, 320, 325, 330, 335, 340, 345
    };

    public static bool IsMaxLevel(int level) => level >= MaxLevel;

    /// <summary>
    /// XP, die man ab <paramref name="level"/> für das *nächste* Level braucht.
    /// Auf Max-Level wird 0 zurückgegeben.
    /// </summary>
    public static int GetXpForLevel(int level)
    {
        if (level < 1) level = 1;
        if (level >= MaxLevel) return 0;
        return XpTable[level - 1];
    }

    /// <summary>
    /// XP-Belohnung, die ein besiegter Gegner auf <paramref name="level"/> gibt.
    /// </summary>
    public static int GetEnemyXpReward(int level)
    {
        if (level < 1) level = 1;
        if (level > TableMaxLevel) level = TableMaxLevel;
        return EnemyXpReward[level - 1];
    }
}
