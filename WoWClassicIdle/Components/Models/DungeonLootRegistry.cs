// Models/DungeonLootRegistry.cs
namespace Models;

/// <summary>
/// Named boss items of the dungeons. Every boss table holds one piece of each of the three sets of the dungeon,
/// 1–2 free blue items and 3 green items; a boss drops one of them. Weapons are bound to the classes that use them, armor outside of sets is free.
/// Item IDs are saved in save games – never renumber them.
/// </summary>
public static class DungeonLootRegistry
{
    private const int BossCount = 5;
    private const long SellPrice = 16000;
    private const long GreenSellPrice = 4000;

    // Who uses which weapon
    private static readonly CharacterClass[] StrengthWeapons = [CharacterClass.Warrior, CharacterClass.Paladin];
    private static readonly CharacterClass[] BladeWeapons    = [CharacterClass.Warrior, CharacterClass.Rogue, CharacterClass.Paladin];
    private static readonly CharacterClass[] AgilityWeapons  = [CharacterClass.Rogue, CharacterClass.Hunter, CharacterClass.Warrior];
    private static readonly CharacterClass[] HybridWeapons   = [CharacterClass.Paladin, CharacterClass.Shaman, CharacterClass.Druid];
    private static readonly CharacterClass[] Staves          = [CharacterClass.Mage, CharacterClass.Warlock, CharacterClass.Priest, CharacterClass.Druid, CharacterClass.Shaman];
    private static readonly CharacterClass[] Wands           = [CharacterClass.Mage, CharacterClass.Warlock, CharacterClass.Priest];
    private static readonly CharacterClass[] Bows            = [CharacterClass.Hunter];

    // Per boss position: slot of the set pieces, required level per dungeon
    private static readonly ArmorSlot[] SetSlots = [ArmorSlot.Feet, ArmorSlot.Hands, ArmorSlot.Legs, ArmorSlot.Head, ArmorSlot.Chest];
    private static readonly string[] SlotIcons = ["inv_boots_08", "inv_gauntlets_04", "inv_pants_03", "inv_helmet_03", "inv_chest_cloth_05"];
    private static readonly int[] DeadminesLevels = [18, 18, 19, 19, 20];
    private static readonly int[] WailingLevels   = [17, 18, 18, 19, 20];

    private static readonly PrimaryStats[] CasterStats =
    [
        new() { Intelligence = 5, Spirit = 3, Stamina = 4 },
        new() { Intelligence = 5, Spirit = 3, Stamina = 4 },
        new() { Intelligence = 5, Spirit = 4, Stamina = 4 },
        new() { Intelligence = 5, Spirit = 4, Stamina = 4 },
        new() { Intelligence = 6, Spirit = 4, Stamina = 4 },
    ];

    private static readonly PrimaryStats[] MeleeStats =
    [
        new() { Strength = 4, Agility = 4, Stamina = 4 },
        new() { Strength = 4, Agility = 4, Stamina = 4 },
        new() { Strength = 5, Agility = 4, Stamina = 4 },
        new() { Strength = 4, Agility = 5, Stamina = 4 },
        new() { Strength = 5, Agility = 5, Stamina = 4 },
    ];

    private static readonly PrimaryStats[] HybridStats =
    [
        new() { Intelligence = 3, Spirit = 2, Stamina = 4, Strength = 3 },
        new() { Intelligence = 3, Spirit = 2, Stamina = 4, Agility  = 3 },
        new() { Intelligence = 3, Spirit = 2, Stamina = 4, Strength = 4 },
        new() { Intelligence = 3, Spirit = 2, Stamina = 4, Agility  = 4 },
        new() { Intelligence = 4, Spirit = 2, Stamina = 4, Strength = 2, Agility = 2 },
    ];

    private static readonly Dictionary<int, Item> _byId = new();
    private static readonly Dictionary<(string Dungeon, int Boss), List<Item>> _tables = new();
    private static readonly Dictionary<string, List<ArmorItem>> _setPieces = new();

    static DungeonLootRegistry()
    {
        const string dm = DungeonRegistry.DeadminesId;
        AddSet(dm, ItemSetRegistry.DeadminesCaster, 30000, CasterStats, DeadminesLevels,
            ["Conjurer's Sandals", "Conjurer's Gloves", "Conjurer's Leggings", "Conjurer's Cowl", "Conjurer's Robe"]);
        AddSet(dm, ItemSetRegistry.DeadminesMelee, 30010, MeleeStats, DeadminesLevels,
            ["Blackened Defias Boots", "Blackened Defias Gloves", "Blackened Defias Leggings", "Blackened Defias Mask", "Blackened Defias Tunic"]);
        AddSet(dm, ItemSetRegistry.DeadminesHybrid, 30020, HybridStats, DeadminesLevels,
            ["Moonbrook Greaves", "Moonbrook Gauntlets", "Moonbrook Legplates", "Moonbrook Helm", "Moonbrook Breastplate"]);

        AddFree(dm, 0, Weapon(30050, "Rhahk'Zor's Hammer", "inv_hammer_05", 2, 6, 2.9, WeaponType.Mace, 18, StrengthWeapons, new() { Strength = 8, Stamina = 16 }));
        AddFree(dm, 1, Weapon(30051, "Buzzer Blade", "inv_weapon_shortblade_05", 2, 5, 1.8, WeaponType.Dagger, 18, AgilityWeapons, new() { Agility = 9, Stamina = 15 }));
        AddFree(dm, 1, Armor(30052, "Sneed's Bracers", "inv_bracer_07", ArmorSlot.Wrists, 18, new() { Strength = 6, Stamina = 6 }));
        AddFree(dm, 2, Weapon(30053, "Foreman's Staff", "inv_staff_08", 3, 6, 3.0, WeaponType.Staff, 19, Staves, new() { Intelligence = 9, Spirit = 7, Stamina = 14 }));
        AddFree(dm, 2, Armor(30054, "Lavishly Jeweled Ring", "inv_jewelry_ring_03", ArmorSlot.Ring, 19, new() { Intelligence = 6, Spirit = 6 }));
        AddFree(dm, 3, Weapon(30055, "Smite's Mighty Hammer", "inv_hammer_10", 3, 6, 2.9, WeaponType.Mace, 19, HybridWeapons, new() { Strength = 7, Intelligence = 6, Stamina = 16 }));
        AddFree(dm, 4, Weapon(30056, "Cruel Barb", "inv_sword_04", 2, 5, 2.3, WeaponType.Sword, 20, BladeWeapons, new() { Strength = 9, Agility = 9, Stamina = 18 }));
        AddFree(dm, 4, Armor(30057, "Cape of the Brotherhood", "inv_misc_cape_18", ArmorSlot.Cape, 20, new() { Agility = 6, Stamina = 6 }));

        // Green items: two armor pieces and a weapon per boss
        AddFree(dm, 0, Green(30060, "Ogre-Hide Bracers", "inv_bracer_07", ArmorSlot.Wrists, 18, new() { Strength = 3, Stamina = 3 }));
        AddFree(dm, 0, Green(30061, "Foreman's Belt", "inv_belt_05", ArmorSlot.Belt, 18, new() { Agility = 3, Stamina = 3 }));
        AddFree(dm, 0, GreenWeapon(30062, "Rusty Mining Axe", "inv_axe_09", 2, 4, 2.6, WeaponType.Axe, 18, StrengthWeapons, new() { Strength = 4, Stamina = 8 }));
        AddFree(dm, 1, Green(30063, "Shredder Gloves", "inv_gauntlets_04", ArmorSlot.Hands, 18, new() { Strength = 3, Agility = 3 }));
        AddFree(dm, 1, Green(30064, "Oil-Stained Cloak", "inv_misc_cape_18", ArmorSlot.Cape, 18, new() { Intelligence = 3, Spirit = 3 }));
        AddFree(dm, 1, GreenWeapon(30065, "Sparking Gear Wand", "inv_wand_05", 2, 4, 1.9, WeaponType.Wand, 18, Wands, new() { Intelligence = 4, Stamina = 8 }));
        AddFree(dm, 2, Green(30066, "Smelter's Boots", "inv_boots_08", ArmorSlot.Feet, 19, new() { Strength = 3, Stamina = 3 }));
        AddFree(dm, 2, Green(30067, "Molten Ring", "inv_jewelry_ring_03", ArmorSlot.Ring, 19, new() { Intelligence = 3, Stamina = 3 }));
        AddFree(dm, 2, GreenWeapon(30068, "Foundry Hammer", "inv_hammer_05", 2, 5, 2.9, WeaponType.Mace, 19, HybridWeapons, new() { Strength = 4, Intelligence = 3, Stamina = 8 }));
        AddFree(dm, 3, Green(30069, "Deckhand's Trousers", "inv_pants_03", ArmorSlot.Legs, 19, new() { Agility = 3, Stamina = 3 }));
        AddFree(dm, 3, Green(30070, "Smite's Wristguards", "inv_bracer_07", ArmorSlot.Wrists, 19, new() { Intelligence = 3, Spirit = 3 }));
        AddFree(dm, 3, GreenWeapon(30071, "Deckhand's Cutlass", "inv_sword_04", 2, 4, 2.3, WeaponType.Sword, 19, BladeWeapons, new() { Strength = 4, Agility = 3, Stamina = 8 }));
        AddFree(dm, 4, Green(30072, "Defias Medallion", "inv_jewelry_necklace_06", ArmorSlot.Neck, 20, new() { Strength = 3, Agility = 3 }));
        AddFree(dm, 4, Green(30073, "Brotherhood Hood", "inv_helmet_03", ArmorSlot.Head, 20, new() { Intelligence = 3, Spirit = 3 }));
        AddFree(dm, 4, GreenWeapon(30074, "Defias Longbow", "inv_weapon_bow_07", 2, 4, 3.4, WeaponType.Bow, 20, Bows, new() { Agility = 4, Stamina = 8 }));

        const string wc = DungeonRegistry.WailingCavernsId;
        AddSet(wc, ItemSetRegistry.WailingCaster, 30100, CasterStats, WailingLevels,
            ["Fanglord's Slippers", "Fanglord's Handwraps", "Fanglord's Trousers", "Fanglord's Hood", "Fanglord's Robe"]);
        AddSet(wc, ItemSetRegistry.WailingMelee, 30110, MeleeStats.Select(SwapStrengthAgility).ToArray(), WailingLevels,
            ["Deviate Scale Boots", "Deviate Scale Gloves", "Deviate Scale Leggings", "Deviate Scale Helm", "Deviate Scale Hauberk"]);
        AddSet(wc, ItemSetRegistry.WailingHybrid, 30120, HybridStats, WailingLevels,
            ["Viper Boots", "Viper Gloves", "Viper Leggings", "Viper Circlet", "Viper Vest"]);

        AddFree(wc, 0, Armor(30150, "Fang Wrap", "inv_belt_05", ArmorSlot.Belt, 17, new() { Intelligence = 6, Stamina = 6 }));
        AddFree(wc, 1, Armor(30151, "Cobrahn's Grasp", "inv_bracer_07", ArmorSlot.Wrists, 18, new() { Strength = 6, Stamina = 6 }));
        AddFree(wc, 1, Weapon(30152, "Fanglord Staff", "inv_staff_08", 3, 6, 3.0, WeaponType.Staff, 18, Staves, new() { Intelligence = 10, Spirit = 8, Stamina = 14 }));
        AddFree(wc, 2, Weapon(30153, "Stinging Viper", "inv_sword_04", 2, 5, 2.3, WeaponType.Sword, 18, BladeWeapons, new() { Agility = 9, Strength = 5, Stamina = 15 }));
        AddFree(wc, 2, Armor(30154, "Serpent Band", "inv_jewelry_ring_03", ArmorSlot.Ring, 18, new() { Agility = 6, Stamina = 6 }));
        AddFree(wc, 3, Armor(30155, "Serpentis' Pendant", "inv_jewelry_necklace_06", ArmorSlot.Neck, 19, new() { Intelligence = 6, Spirit = 6 }));
        AddFree(wc, 4, Weapon(30156, "Venomstrike", "inv_weapon_bow_07", 2, 5, 3.4, WeaponType.Bow, 20, Bows, new() { Agility = 10, Intelligence = 5, Stamina = 15 }));
        AddFree(wc, 4, Armor(30157, "Glowing Lizardscale Cloak", "inv_misc_cape_18", ArmorSlot.Cape, 20, new() { Strength = 4, Agility = 4, Stamina = 4 }));

        AddFree(wc, 0, Green(30160, "Serpent Hide Belt", "inv_belt_05", ArmorSlot.Belt, 17, new() { Intelligence = 3, Stamina = 3 }));
        AddFree(wc, 0, Green(30161, "Druidic Cuffs", "inv_bracer_07", ArmorSlot.Wrists, 17, new() { Intelligence = 3, Spirit = 3 }));
        AddFree(wc, 0, GreenWeapon(30162, "Thornroot Wand", "inv_wand_05", 2, 4, 1.9, WeaponType.Wand, 17, Wands, new() { Intelligence = 4, Stamina = 8 }));
        AddFree(wc, 1, Green(30163, "Cobra Scale Boots", "inv_boots_08", ArmorSlot.Feet, 18, new() { Agility = 3, Stamina = 3 }));
        AddFree(wc, 1, Green(30164, "Fang Gauntlets", "inv_gauntlets_04", ArmorSlot.Hands, 18, new() { Strength = 3, Stamina = 3 }));
        AddFree(wc, 1, GreenWeapon(30165, "Serpent Fang", "inv_weapon_shortblade_05", 2, 4, 1.8, WeaponType.Dagger, 18, AgilityWeapons, new() { Agility = 4, Stamina = 8 }));
        AddFree(wc, 2, Green(30166, "Stormcaller's Cloak", "inv_misc_cape_18", ArmorSlot.Cape, 18, new() { Intelligence = 3, Spirit = 3 }));
        AddFree(wc, 2, Green(30167, "Deviate Band", "inv_jewelry_ring_03", ArmorSlot.Ring, 18, new() { Strength = 3, Agility = 3 }));
        AddFree(wc, 2, GreenWeapon(30168, "Lightning Rod", "inv_staff_08", 2, 5, 3.0, WeaponType.Staff, 18, Staves, new() { Intelligence = 4, Spirit = 3, Stamina = 8 }));
        AddFree(wc, 3, Green(30169, "Viper Legwraps", "inv_pants_03", ArmorSlot.Legs, 19, new() { Agility = 3, Intelligence = 3 }));
        AddFree(wc, 3, Green(30170, "Fanglord's Helm", "inv_helmet_03", ArmorSlot.Head, 19, new() { Strength = 3, Stamina = 3 }));
        AddFree(wc, 3, GreenWeapon(30171, "Serpent Mace", "inv_hammer_05", 2, 5, 2.9, WeaponType.Mace, 19, HybridWeapons, new() { Strength = 4, Intelligence = 3, Stamina = 8 }));
        AddFree(wc, 4, Green(30172, "Murloc Hide Vest", "inv_chest_cloth_05", ArmorSlot.Chest, 20, new() { Stamina = 3, Spirit = 3 }));
        AddFree(wc, 4, Green(30173, "Deviate Pendant", "inv_jewelry_necklace_06", ArmorSlot.Neck, 20, new() { Agility = 3, Stamina = 3 }));
        AddFree(wc, 4, GreenWeapon(30174, "Nightmare Blade", "inv_sword_04", 2, 4, 2.3, WeaponType.Sword, 20, BladeWeapons, new() { Strength = 4, Agility = 3, Stamina = 8 }));
    }

    private static PrimaryStats SwapStrengthAgility(PrimaryStats s) => s with { Strength = s.Agility, Agility = s.Strength };

    private static void AddSet(string dungeon, string setId, int firstId, PrimaryStats[] stats, int[] levels, string[] names)
    {
        var pieces = new List<ArmorItem>();
        for (int boss = 0; boss < BossCount; boss++)
        {
            var piece = new ArmorItem(firstId + boss, names[boss], SlotIcons[boss], SetSlots[boss], stats[boss],
                levels[boss], ItemQuality.Rare, SellPrice, SetId: setId);
            pieces.Add(piece);
            AddFree(dungeon, boss, piece);
        }
        _setPieces[setId] = pieces;
    }

    private static void AddFree(string dungeon, int boss, Item item)
    {
        _byId[item.Id] = item;
        if (!_tables.TryGetValue((dungeon, boss), out var table))
            _tables[(dungeon, boss)] = table = [];
        table.Add(item);
    }

    private static ArmorItem Armor(int id, string name, string icon, ArmorSlot slot, int level, PrimaryStats stats) =>
        new(id, name, icon, slot, stats, level, ItemQuality.Rare, SellPrice);

    private static WeaponItem Weapon(int id, string name, string icon, int min, int max, double speed, WeaponType type, int level, CharacterClass[] classes, PrimaryStats stats) =>
        new(id, name, icon, min, max, speed, type, ItemQuality.Rare, SellPrice, level, Bonuses: stats, AllowedClasses: classes);

    private static ArmorItem Green(int id, string name, string icon, ArmorSlot slot, int level, PrimaryStats stats) =>
        new(id, name, icon, slot, stats, level, ItemQuality.Uncommon, GreenSellPrice);

    private static WeaponItem GreenWeapon(int id, string name, string icon, int min, int max, double speed, WeaponType type, int level, CharacterClass[] classes, PrimaryStats stats) =>
        new(id, name, icon, min, max, speed, type, ItemQuality.Uncommon, GreenSellPrice, level, Bonuses: stats, AllowedClasses: classes);

    public static Item? GetById(int id) => _byId.GetValueOrDefault(id);

    /// <summary>Complete loot table of a boss.</summary>
    public static IReadOnlyList<Item> TableFor(string dungeonId, int bossIndex) =>
        _tables.TryGetValue((dungeonId, bossIndex), out var table) ? table : [];

    public static IReadOnlyList<ArmorItem> PiecesOf(string setId) =>
        _setPieces.TryGetValue(setId, out var pieces) ? pieces : [];

    /// <summary>Exactly one item with the same chance for all, green or blue; items the class cannot use are removed before the roll.</summary>
    public static Item? RollBossDrop(string dungeonId, int bossIndex, CharacterClass cls, Random rng)
    {
        var candidates = TableFor(dungeonId, bossIndex).Where(i => ItemRequirements.MatchesClass(i, cls)).ToList();
        return candidates.Count > 0 ? candidates[rng.Next(candidates.Count)] : null;
    }
}
