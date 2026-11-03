// Models/DungeonLootRegistry.RazorfenKraul.cs
namespace Models;

public static partial class DungeonLootRegistry
{
    // Razorfen Kraul: no sets, 2–3 blue and 3 green items per boss. Item IDs 30500–30534.
    private static void AddRazorfenKraul()
    {
        const string rfk = DungeonRegistry.RazorfenKraulId;

        AddFree(rfk, 0, Weapon(30500, "Thornspike", "inv_weapon_shortblade_05", 3, 6, 1.8, WeaponType.Dagger, 29, AgilityWeapons, new() { Agility = 11, Strength = 5, Stamina = 17 }));
        AddFree(rfk, 0, Armor(30501, "Thorncurse Wristwraps", "inv_bracer_04", ArmorSlot.Wrists, 29, new() { Intelligence = 7, Spirit = 5, Stamina = 6 }));
        AddFree(rfk, 0, Green(30520, "Bramble Belt", "inv_belt_03", ArmorSlot.Belt, 29, new() { Strength = 4, Stamina = 4 }));
        AddFree(rfk, 0, Green(30521, "Thornweave Cloak", "inv_misc_cape_06", ArmorSlot.Cape, 29, new() { Intelligence = 4, Spirit = 4 }));
        AddFree(rfk, 0, GreenWeapon(30522, "Thorny Wand", "inv_wand_07", 2, 5, 1.9, WeaponType.Wand, 29, Wands, new() { Intelligence = 5, Stamina = 10 }));

        AddFree(rfk, 1, Weapon(30502, "Death Speaker Scepter", "inv_wand_05", 3, 6, 1.9, WeaponType.Wand, 30, Wands, new() { Intelligence = 11, Spirit = 5, Stamina = 16 }));
        AddFree(rfk, 1, Armor(30503, "Death Speaker Robes", "inv_chest_cloth_21", ArmorSlot.Chest, 30, new() { Intelligence = 8, Spirit = 6, Stamina = 6 }));
        AddFree(rfk, 1, Green(30523, "Bone-Studded Bracers", "inv_bracer_02", ArmorSlot.Wrists, 30, new() { Agility = 4, Stamina = 4 }));
        AddFree(rfk, 1, Green(30524, "Death's Head Cowl", "inv_helmet_29", ArmorSlot.Head, 30, new() { Intelligence = 4, Spirit = 4 }));
        AddFree(rfk, 1, GreenWeapon(30525, "Gravebone Staff", "inv_staff_29", 3, 5, 3.0, WeaponType.Staff, 30, Staves, new() { Intelligence = 5, Spirit = 3, Stamina = 10 }));

        AddFree(rfk, 2, Weapon(30504, "Ramtusk's Cleaver", "inv_axe_09", 4, 8, 2.8, WeaponType.Axe, 31, StrengthWeapons, new() { Strength = 12, Stamina = 18 }));
        AddFree(rfk, 2, Armor(30505, "Overlord's Girdle", "inv_belt_11", ArmorSlot.Belt, 31, new() { Strength = 6, Agility = 5, Stamina = 7 }));
        AddFree(rfk, 2, Green(30526, "Quilboar Hide Boots", "inv_boots_05", ArmorSlot.Feet, 31, new() { Agility = 4, Stamina = 4 }));
        AddFree(rfk, 2, Green(30527, "Tuskbone Ring", "inv_jewelry_ring_05", ArmorSlot.Ring, 31, new() { Strength = 4, Agility = 4 }));
        AddFree(rfk, 2, GreenWeapon(30528, "Spearhide Shortbow", "inv_weapon_bow_07", 3, 5, 3.4, WeaponType.Bow, 31, Bows, new() { Agility = 5, Stamina = 10 }));

        AddFree(rfk, 3, Weapon(30506, "Agathelos' Tusk", "inv_sword_04", 3, 7, 2.3, WeaponType.Sword, 32, BladeWeapons, new() { Agility = 9, Strength = 8, Stamina = 18 }));
        AddFree(rfk, 3, Armor(30507, "Rage-Hide Cloak", "inv_misc_cape_10", ArmorSlot.Cape, 32, new() { Agility = 6, Strength = 5, Stamina = 7 }));
        AddFree(rfk, 3, Green(30529, "Boar-Hide Gloves", "inv_gauntlets_21", ArmorSlot.Hands, 32, new() { Strength = 4, Stamina = 4 }));
        AddFree(rfk, 3, Green(30530, "Ragepelt Leggings", "inv_pants_leather_09", ArmorSlot.Legs, 32, new() { Agility = 4, Stamina = 5 }));
        AddFree(rfk, 3, GreenWeapon(30531, "Tusked Mace", "inv_hammer_15", 3, 6, 2.9, WeaponType.Mace, 32, HybridWeapons, new() { Strength = 5, Intelligence = 3, Stamina = 10 }));

        AddFree(rfk, 4, Weapon(30508, "Agamaggan's Fang", "inv_hammer_20", 4, 8, 2.9, WeaponType.Mace, 33, HybridWeapons, new() { Strength = 7, Intelligence = 7, Stamina = 18 }));
        AddFree(rfk, 4, Armor(30509, "Heart of Agamaggan", "inv_misc_root_02", ArmorSlot.Trinket, 33, new() { Strength = 6, Stamina = 11 }));
        AddFree(rfk, 4, Armor(30510, "Charlga's Talisman", "inv_jewelry_talisman_07", ArmorSlot.Neck, 33, new() { Intelligence = 7, Spirit = 6, Stamina = 6 }));
        AddFree(rfk, 4, Green(30532, "Razorflank Leggings", "inv_pants_cloth_05", ArmorSlot.Legs, 33, new() { Intelligence = 4, Spirit = 4 }));
        AddFree(rfk, 4, Green(30533, "Brambleguard Band", "inv_jewelry_ring_08", ArmorSlot.Ring, 33, new() { Agility = 4, Stamina = 5 }));
        AddFree(rfk, 4, GreenWeapon(30534, "Razorflank Cutlass", "inv_sword_04", 3, 5, 2.3, WeaponType.Sword, 33, BladeWeapons, new() { Strength = 5, Agility = 3, Stamina = 10 }));
    }
}
