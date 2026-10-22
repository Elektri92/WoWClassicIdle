// Models/WeaponRegistry.cs
namespace Models;

/// <summary>
/// Defines all starter weapons (one per class). Stats match StarterWeapons exactly.
/// Wand for Mage/Warlock/Priest, Bow for Hunter, Sword for Warrior/Rogue,
/// Mace for Druid/Shaman/Paladin.
/// </summary>
public static class WeaponRegistry
{
    public static readonly WeaponItem WarriorStarter = new(5001, "Worn Shortsword",   "inv_sword_04",          1, 3, 1.9, WeaponType.Sword, SellPrice: 1, RequiredClass: CharacterClass.Warrior);
    public static readonly WeaponItem PaladinStarter = new(5002, "Battleworn Hammer", "inv_hammer_05",         2, 4, 2.9, WeaponType.Mace,  SellPrice: 1, RequiredClass: CharacterClass.Paladin);
    public static readonly WeaponItem HunterStarter  = new(5003, "Worn Shortbow",     "inv_weapon_bow_07",     1, 3, 2.0, WeaponType.Bow,   SellPrice: 1, RequiredClass: CharacterClass.Hunter);
    public static readonly WeaponItem RogueStarter   = new(5004, "Short Sword",       "inv_sword_04",          1, 3, 1.6, WeaponType.Sword, SellPrice: 1, RequiredClass: CharacterClass.Rogue);
    public static readonly WeaponItem PriestStarter  = new(5005, "Worn Wand",         "inv_wand_05",           1, 3, 1.9, WeaponType.Wand,  SellPrice: 1, RequiredClass: CharacterClass.Priest);
    public static readonly WeaponItem MageStarter    = new(5006, "Arcane Wand",       "inv_wand_05",           2, 4, 2.9, WeaponType.Wand,  SellPrice: 1, RequiredClass: CharacterClass.Mage);
    public static readonly WeaponItem WarlockStarter = new(5007, "Fel-Touched Wand",  "inv_wand_05",           1, 3, 1.6, WeaponType.Wand,  SellPrice: 1, RequiredClass: CharacterClass.Warlock);
    public static readonly WeaponItem ShamanStarter  = new(5008, "Worn Mace",         "inv_hammer_05",         1, 3, 1.9, WeaponType.Mace,  SellPrice: 1, RequiredClass: CharacterClass.Shaman);
    public static readonly WeaponItem DruidStarter   = new(5009, "Worn Mace",         "inv_hammer_05",         1, 3, 1.9, WeaponType.Mace,  SellPrice: 1, RequiredClass: CharacterClass.Druid);

    private static readonly Dictionary<int, WeaponItem> _byId = new()
    {
        [WarriorStarter.Id] = WarriorStarter,
        [PaladinStarter.Id] = PaladinStarter,
        [HunterStarter.Id]  = HunterStarter,
        [RogueStarter.Id]   = RogueStarter,
        [PriestStarter.Id]  = PriestStarter,
        [MageStarter.Id]    = MageStarter,
        [WarlockStarter.Id] = WarlockStarter,
        [ShamanStarter.Id]  = ShamanStarter,
        [DruidStarter.Id]   = DruidStarter,
    };

    public static WeaponItem? GetById(int id) => _byId.TryGetValue(id, out var w) ? w : null;

    public static WeaponItem GetStarter(CharacterClass cls) => cls switch
    {
        CharacterClass.Warrior => WarriorStarter,
        CharacterClass.Paladin => PaladinStarter,
        CharacterClass.Hunter  => HunterStarter,
        CharacterClass.Rogue   => RogueStarter,
        CharacterClass.Priest  => PriestStarter,
        CharacterClass.Mage    => MageStarter,
        CharacterClass.Warlock => WarlockStarter,
        CharacterClass.Shaman  => ShamanStarter,
        CharacterClass.Druid   => DruidStarter,
        _                      => WarriorStarter,
    };
}
