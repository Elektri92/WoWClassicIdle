// Combat/DefaultEnemyTemplates.cs
public static class DefaultEnemyTemplates
{
    public static IReadOnlyList<EnemyTemplate> Default { get; } = new List<EnemyTemplate>
    {
        new EnemyTemplate
        {
            Name         = "Wolf",
            Type         = "Wolf",
            ImageUrl     = "https://wow.zamimg.com/images/wow/icons/large/ability_hunter_pet_wolf.jpg",
            MinLevel     = 1,
            MaxLevel     = 6,
            BaseMaxHp    = 42,
            MaxHpByLevel = [42, 55, 71, 86, 102, 120], // WoW Classic: Young Wolf (1) … Mangy Wolf (5-6)
            BaseAttack   = 6,
            AttackByLevel = [5, 5, 6, 6, 6, 6],         // Lv 1–2 −15 %: Sim zeigte 9–25 Tode/h auf Lv 1–2
            BaseDefense  = 0,
            BaseSpeed    = 3.3,
            MinCopperReward = 1,
            MaxCopperReward = 3,
            BaseXpReward   = 15,
            Weight       = 3
        },
        new EnemyTemplate
        {
            Name         = "Murloc",
            Type         = "Murloc",
            ImageUrl     = "https://wow.zamimg.com/images/wow/icons/large/inv_misc_head_murloc_01.jpg",
            MinLevel     = 4,
            MaxLevel     = 8,
            BaseMaxHp    = 80,
            MaxHpByLevel = [80, 96, 112, 129, 146], // WoW Classic: Murloc Streamer (4) … Murloc Lurker (7-8), etwas weicher als Wölfe
            BaseAttack   = 6,
            BaseDefense  = 0,
            BaseSpeed    = 2.6,                     // schlägt schneller zu als ein Wolf
            MinCopperReward = 2,
            MaxCopperReward = 6,
            BaseXpReward   = 15,
            Weight       = 3
        },
        new EnemyTemplate
        {
            Name         = "Boar",
            Type         = "Boar",
            ImageUrl     = "https://wow.zamimg.com/images/wow/icons/large/ability_hunter_pet_boar.jpg",
            MinLevel     = 1,
            MaxLevel     = 3,
            BaseMaxHp    = 42,
            MaxHpByLevel = [42, 55, 71],               // WoW Classic: Mottled Boar (1-2) … Small Crag Boar (3)
            BaseAttack   = 5,
            AttackByLevel = [4, 4, 5],                 // Lv 1–2 −15 % (wie Wolf)
            BaseDefense  = 0,
            BaseSpeed    = 3.0,
            MinCopperReward = 1,
            MaxCopperReward = 3,
            BaseXpReward   = 15,
            Weight       = 3
        },
        new EnemyTemplate
        {
            Name         = "Boar",
            Type         = "Boar",
            ImageUrl     = "https://wow.zamimg.com/images/wow/icons/large/ability_hunter_pet_boar.jpg",
            MinLevel     = 6,
            MaxLevel     = 9,
            BaseMaxHp    = 120,
            MaxHpByLevel = [120, 137, 156, 176],       // WoW Classic: Dire Mottled Boar (6-7) … Elder Mottled Boar (8-9)
            BaseAttack   = 8,
            BaseDefense  = 0,
            BaseSpeed    = 3.0,
            MinCopperReward = 3,
            MaxCopperReward = 7,
            BaseXpReward   = 15,
            Weight       = 3
        },
        new EnemyTemplate
        {
            Name         = "Scorpid",
            Type         = "Scorpid",
            ImageUrl     = "https://wow.zamimg.com/images/wow/icons/large/ability_hunter_pet_scorpid.jpg",
            MinLevel     = 8,
            MaxLevel     = 11,
            BaseMaxHp    = 156,
            MaxHpByLevel = [156, 176, 198, 222],       // WoW Classic: Clattering Scorpid (8-9) … Scorpid Reaver (10-11)
            BaseAttack   = 9,
            BaseDefense  = 1,                          // gepanzert: leichte Schadensminderung
            BaseSpeed    = 2.0,                        // schnelle Stiche
            MinCopperReward = 4,
            MaxCopperReward = 9,
            BaseXpReward   = 15,
            Weight       = 3
        },
        new EnemyTemplate
        {
            Name         = "Bear",
            Type         = "Bear",
            ImageUrl     = "https://wow.zamimg.com/images/wow/icons/large/ability_hunter_pet_bear.jpg",
            MinLevel     = 3,
            MaxLevel     = 8,
            BaseMaxHp    = 82,
            MaxHpByLevel = [82, 99, 117, 138, 158, 179],  // WoW Classic: Young Black Bear (3-4) … Grizzled Black Bear (6-8); +15 % HP wegen dickem Fell
            BaseAttack   = 8,
            BaseDefense  = 1,                              // dickes Fell
            BaseSpeed    = 2.5,
            MinCopperReward = 2,
            MaxCopperReward = 6,
            BaseXpReward   = 15,
            Weight       = 2
        },
        new EnemyTemplate
        {
            Name         = "Defias Bandit",
            Type         = "Humanoid",
            ImageUrl     = "https://wow.zamimg.com/images/wow/icons/large/ability_rogue_shadowstep.jpg",
            MinLevel     = 12,
            MaxLevel     = 18,
            BaseMaxHp    = 247,
            MaxHpByLevel = [247, 274, 302, 331, 362, 394, 427],  // WoW Classic: Defias Thug (12-14) … Defias Pillager (15-18)
            BaseAttack   = 10,
            BaseDefense  = 1,
            BaseSpeed    = 2.4,                                   // flinke Banditen
            MinCopperReward = 8,
            MaxCopperReward = 16,
            BaseXpReward   = 15,
            Weight       = 3
        },
        new EnemyTemplate
        {
            Name         = "Goblin",
            Type         = "Humanoid",
            ImageUrl     = "https://wow.zamimg.com/images/wow/icons/large/inv_misc_bomb_07.jpg",
            MinLevel     = 14,
            MaxLevel     = 16,
            BaseMaxHp    = 287,
            MaxHpByLevel = [287, 314, 344],  // WoW Classic: Venture Co. Goblins; etwas weniger HP als gleichstufige Menschen
            BaseAttack   = 10,
            BaseDefense  = 0,
            BaseSpeed    = 2.2,              // kleiner und flinker
            MinCopperReward = 10,
            MaxCopperReward = 18,
            BaseXpReward   = 15,
            Weight       = 2
        },
        new EnemyTemplate
        {
            Name         = "Dragon Whelp",
            Type         = "Dragon",
            ImageUrl     = "https://wow.zamimg.com/images/wow/icons/large/inv_misc_head_dragon_01.jpg",
            MinLevel     = 16,
            MaxLevel     = 19,
            BaseMaxHp    = 434,
            MaxHpByLevel = [434, 473, 512, 554],  // WoW Classic: Emberstrife-Welpling / Flamescale Welpling; +20 % HP durch Schuppen
            BaseAttack   = 13,
            BaseDefense  = 2,                     // Schuppenpanzer
            BaseSpeed    = 2.0,                   // schnelle Bisse und Feueratem
            MinCopperReward = 12,
            MaxCopperReward = 24,
            BaseXpReward   = 15,
            Weight       = 1                      // selten
        },
        new EnemyTemplate
        {
            Name         = "Worgen",
            Type         = "Humanoid",
            ImageUrl     = "https://wow.zamimg.com/images/wow/icons/large/ability_druid_ravage.jpg",
            MinLevel     = 17,
            MaxLevel     = 24,
            BaseMaxHp    = 394,
            MaxHpByLevel = [394, 427, 462, 498, 535, 573, 613, 654],  // WoW Classic: Blackwald Worgen / Rot Hide Gnoll-ähnlich (Silverpine/Ashenvale)
            BaseAttack   = 13,
            BaseDefense  = 1,
            BaseSpeed    = 2.2,                                        // reißende Angriffe
            MinCopperReward = 11,
            MaxCopperReward = 20,
            BaseXpReward   = 15,
            Weight       = 3
        },
        new EnemyTemplate
        {
            Name         = "Slime",
            Type         = "Ooze",
            ImageUrl     = "https://wow.zamimg.com/images/wow/icons/large/inv_misc_slime_01.jpg",
            MinLevel     = 22,
            MaxLevel     = 32,
            BaseMaxHp    = 630,
            MaxHpByLevel = [630, 674, 719, 766, 812, 860, 911, 963, 1015, 1069, 1125],  // Basis ×1.10: Ooze / Acidic Slime; WoW Classic: zähe Gallerte (Wailing Caverns, Stonetalon)
            BaseAttack   = 15,
            BaseDefense  = 2,                                                             // dickflüssig: leichte Schadensminderung
            BaseSpeed    = 3.1,                                                           // langsam, klebrig
            MinCopperReward = 14,
            MaxCopperReward = 30,
            BaseXpReward   = 15,
            Weight       = 2
        },
        new EnemyTemplate
        {
            Name         = "Troll",
            Type         = "Humanoid",
            ImageUrl     = "https://wow.zamimg.com/images/wow/icons/large/race_troll_male.jpg",
            MinLevel     = 30,
            MaxLevel     = 36,
            BaseMaxHp    = 1066,
            MaxHpByLevel = [1066, 1124, 1182, 1243, 1305, 1369, 1433],  // WoW Classic: Vilebranch / Witherbark Troll; +15 % wegen Regeneration
            BaseAttack   = 18,
            BaseDefense  = 1,
            BaseSpeed    = 2.5,
            MinCopperReward = 20,
            MaxCopperReward = 38,
            BaseXpReward   = 15,
            Weight       = 3
        },
        new EnemyTemplate
        {
            Name         = "Dragon Whelp",
            Type         = "Dragon",
            ImageUrl     = "https://wow.zamimg.com/images/wow/icons/large/inv_misc_head_dragon_01.jpg",
            MinLevel     = 34,
            MaxLevel     = 44,
            BaseMaxHp    = 1362,
            MaxHpByLevel = [1362, 1428, 1495, 1565, 1636, 1708, 1782, 1858, 1934, 2012, 2093],  // WoW Classic: Scarlet/Azure Whelpling; +20 % durch Schuppen
            BaseAttack   = 22,
            BaseDefense  = 4,
            BaseSpeed    = 2.0,
            MinCopperReward = 26,
            MaxCopperReward = 50,
            BaseXpReward   = 15,
            Weight       = 1                                                                      // selten
        },
        new EnemyTemplate
        {
            Name         = "Bear",
            Type         = "Bear",
            ImageUrl     = "https://wow.zamimg.com/images/wow/icons/large/ability_hunter_pet_bear.jpg",
            MinLevel     = 40,
            MaxLevel     = 45,
            BaseMaxHp    = 1708,
            MaxHpByLevel = [1708, 1780, 1854, 1929, 2006, 2084],  // WoW Classic: Ironfur Bear / Elder Ironfur Bear; +15 % HP
            BaseAttack   = 22,
            BaseDefense  = 3,
            BaseSpeed    = 2.5,
            MinCopperReward = 32,
            MaxCopperReward = 56,
            BaseXpReward   = 15,
            Weight       = 2
        },
        new EnemyTemplate
        {
            Name         = "Pirate",
            Type         = "Humanoid",
            ImageUrl     = "https://wow.zamimg.com/images/wow/icons/large/inv_helmet_66.jpg",
            MinLevel     = 42,
            MaxLevel     = 52,
            BaseMaxHp    = 1612,
            MaxHpByLevel = [1612, 1677, 1744, 1812, 1884, 1956, 2028, 2100, 2171, 2247, 2324],  // WoW Classic: Southsea Freebooter / Bloodsail Buccaneer; Standardskalierung für Humanoids
            BaseAttack   = 23,
            BaseDefense  = 1,                                                                     // leichte Lederrüstung
            BaseSpeed    = 2.3,                                                                   // flinke Säbelschläge
            MinCopperReward = 35,
            MaxCopperReward = 68,
            BaseXpReward   = 15,
            Weight       = 3
        },
        new EnemyTemplate
        {
            Name         = "Dark Iron Dwarf",
            Type         = "Humanoid",
            ImageUrl     = "https://wow.zamimg.com/images/wow/icons/large/inv_misc_head_dwarf_01.jpg",
            MinLevel     = 50,
            MaxLevel     = 58,
            BaseMaxHp    = 2279,
            MaxHpByLevel = [2279, 2359, 2440, 2522, 2606, 2691, 2777, 2864, 2954],  // WoW Classic: Dark Iron Slaver / Golem Lord (Searing Gorge/Burning Steppes); +5 % schwere Rüstung
            BaseAttack   = 27,
            BaseDefense  = 4,                                                         // Plattenrüstung
            BaseSpeed    = 2.8,
            MinCopperReward = 50,
            MaxCopperReward = 95,
            BaseXpReward   = 15,
            Weight       = 3
        },
    };
}
