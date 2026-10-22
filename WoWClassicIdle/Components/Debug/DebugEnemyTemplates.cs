// Nur im Debug-Build kompiliert (siehe WoWClassicIdle.csproj)
public static class DebugEnemyTemplates
{
    public static IReadOnlyList<EnemyTemplate> All { get; } = new List<EnemyTemplate>
    {
        new EnemyTemplate
        {
            Name         = "Debug-Monster",
            Type         = "Debug",
            ImageUrl     = "https://wow.zamimg.com/images/wow/icons/large/inv_misc_skull_02.jpg",
            MinLevel     = 1,
            MaxLevel     = 60,
            BaseMaxHp    = 42,
            MaxHpByLevel = [
                   42,   55,   71,   86,  102,  120,  137,  156,  176,  198,  // L1–10
                  222,  247,  274,  302,  331,  362,  394,  427,  462,  498,  // L11–20
                  535,  573,  613,  654,  696,  738,  782,  828,  875,  923,  // L21–30
                  972, 1023, 1075, 1128, 1182, 1238, 1295, 1353, 1412, 1485, // L31–40
                 1548, 1612, 1677, 1744, 1812, 1882, 1954, 2027, 2102, 2171, // L41–50
                 2247, 2324, 2402, 2482, 2563, 2645, 2728, 2813, 2900, 2990, // L51–60
            ],
            BaseAttack      = 5,
            BaseDefense     = 0,
            BaseSpeed       = 2.5,
            MinCopperReward = 1,
            MaxCopperReward = 5,
            BaseXpReward    = 15,
            Weight          = 10  // im Debug sehr häufig gespawnt
        },
    };
}
