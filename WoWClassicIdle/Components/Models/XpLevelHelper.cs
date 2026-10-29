// Models/StaticHelpers.cs
namespace Models;

public static partial class StaticHelpers
{
    // Tabellen reichen bis Level 60; im Release ist das Spiel vorerst auf ReleaseLevelCap begrenzt.
    private const int TableMaxLevel   = 60;
    public  const int ReleaseLevelCap = 10;

    public static int MaxLevel =>
        WoWClassicIdle.Components.BuildInfo.IsDebug ? TableMaxLevel : ReleaseLevelCap;

    // XP needed to go from level N to level N+1 (index 0 = level 1 → 2)
    // Classic-Werte × 1,1 (auf 100 gerundet) – gleicht Entdeckungs-XP und Zonen-Ziele aus (≈ +10 % XP je Levelbereich)
    private static readonly int[] XpTable =
    {
         400,  1000,  1500,  2300,  3100,  4000,  5000,  5900,  7200,  8400,
        9700, 11100, 12500, 14200, 15800, 17600, 19500, 21300, 23400, 25500,
       27700, 30000, 32300, 34900, 37400, 40000, 42800, 45500, 48700, 52100,
       55900, 60000, 64500, 69100, 73800, 78800, 83700, 88900, 94300, 99800,
      105400,111100,116900,123000,129300,135500,142000,148600,155300,162200,
      169300,176400,183800,191300,198900,206700,214500,222500,230800
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

    /// <summary>Ab dieser Level-Differenz (Spieler − Gegner) gibt ein Kill keine XP mehr.</summary>
    public const int NoXpLevelDifference = 6;

    /// <summary>
    /// Anteil der XP je nach Level-Differenz: bis 2 Level darunter voll, 3/4/5 darunter stark reduziert, ab 6 nichts.
    /// </summary>
    public static double GetXpPenaltyFactor(int playerLevel, int enemyLevel) => (playerLevel - enemyLevel) switch
    {
        <= 2 => 1.0,
        3    => 0.5,
        4    => 0.25,
        5    => 0.1,
        _    => 0.0,
    };

    /// <summary>XP für einen Kill inklusive Abzug für zu niedrige Gegner.</summary>
    public static int GetKillXp(int playerLevel, int enemyLevel) =>
        (int)Math.Round(GetEnemyXpReward(enemyLevel) * GetXpPenaltyFactor(playerLevel, enemyLevel));
}
