// Models/DungeonLootRegistry.ScarletMonastery.cs
namespace Models;

public static partial class DungeonLootRegistry
{
    private static readonly int[] ScarletLevels = [34, 35, 37, 38, 40];

    private static readonly PrimaryStats[] ScarletCasterStats =
    [
        new() { Intelligence = 8,  Spirit = 5, Stamina = 7 },
        new() { Intelligence = 8,  Spirit = 6, Stamina = 7 },
        new() { Intelligence = 9,  Spirit = 6, Stamina = 7 },
        new() { Intelligence = 9,  Spirit = 6, Stamina = 8 },
        new() { Intelligence = 10, Spirit = 6, Stamina = 8 },
    ];

    private static readonly PrimaryStats[] ScarletMeleeStats =
    [
        new() { Strength = 7, Agility = 6, Stamina = 7 },
        new() { Strength = 6, Agility = 7, Stamina = 7 },
        new() { Strength = 7, Agility = 7, Stamina = 7 },
        new() { Strength = 7, Agility = 7, Stamina = 8 },
        new() { Strength = 8, Agility = 8, Stamina = 8 },
    ];

    private static readonly PrimaryStats[] ScarletHybridStats =
    [
        new() { Intelligence = 5, Spirit = 3, Stamina = 7, Strength = 5 },
        new() { Intelligence = 5, Spirit = 3, Stamina = 7, Agility  = 6 },
        new() { Intelligence = 5, Spirit = 4, Stamina = 7, Strength = 6 },
        new() { Intelligence = 6, Spirit = 4, Stamina = 7, Agility  = 6 },
        new() { Intelligence = 6, Spirit = 4, Stamina = 8, Strength = 3, Agility = 3 },
    ];

    // Same slot order as SetSlots: feet, hands, legs, head, chest
    private static readonly string[] ClothIcons = ["inv_boots_cloth_03", "inv_gauntlets_17", "inv_pants_cloth_05", "inv_helmet_29", "inv_chest_cloth_21"];
    private static readonly string[] MailIcons  = ["inv_boots_chain_04", "inv_gauntlets_21", "inv_pants_mail_03", "inv_helmet_15", "inv_chest_chain_05"];
    private static readonly string[] PlateIcons = ["inv_boots_plate_01", "inv_gauntlets_10", "inv_pants_04", "inv_helmet_09", "inv_chest_plate06"];

    // Scarlet Monastery: three sets like the Deadmines, plus 1–3 blue and 3 green items per boss.
    // Item IDs: sets 30400–30424, blue 30450–30461, green 30470–30484.
    private static void AddScarletMonastery()
    {
        const string sm = DungeonRegistry.ScarletMonasteryId;

        AddSet(sm, ItemSetRegistry.ScarletCaster, 30400, ScarletCasterStats, ScarletLevels,
            ["Inquisitor's Slippers", "Inquisitor's Gloves", "Inquisitor's Leggings", "Inquisitor's Cowl", "Inquisitor's Robe"], ClothIcons);
        AddSet(sm, ItemSetRegistry.ScarletMelee, 30410, ScarletMeleeStats, ScarletLevels,
            ["Scarlet Boots", "Scarlet Gauntlets", "Scarlet Leggings", "Scarlet Crusader's Helm", "Scarlet Chestpiece"], MailIcons);
        AddSet(sm, ItemSetRegistry.ScarletHybrid, 30420, ScarletHybridStats, ScarletLevels,
            ["Vanguard Sabatons", "Vanguard Gauntlets", "Vanguard Legplates", "Vanguard Visor", "Vanguard Breastplate"], PlateIcons);

        AddFree(sm, 0, Weapon(30450, "Torturing Poker", "inv_weapon_shortblade_05", 4, 7, 1.8, WeaponType.Dagger, 34, AgilityWeapons, new() { Agility = 13, Strength = 6, Stamina = 20 }));
        AddFree(sm, 0, Armor(30451, "Interrogator's Bindings", "inv_bracer_14", ArmorSlot.Wrists, 34, new() { Strength = 7, Agility = 6, Stamina = 8 }));
        AddFree(sm, 1, Weapon(30452, "Illusionary Rod", "inv_staff_29", 5, 9, 3.0, WeaponType.Staff, 35, Staves, new() { Intelligence = 13, Spirit = 9, Stamina = 18 }));
        AddFree(sm, 1, Armor(30453, "Robe of Doan", "inv_chest_cloth_05", ArmorSlot.Chest, 35, new() { Intelligence = 9, Spirit = 7, Stamina = 7 }));
        AddFree(sm, 2, Weapon(30454, "Ravager", "inv_axe_09", 5, 10, 2.8, WeaponType.Axe, 37, StrengthWeapons, new() { Strength = 14, Stamina = 22 }));
        AddFree(sm, 2, Armor(30455, "Herod's Signet", "inv_jewelry_ring_11", ArmorSlot.Ring, 37, new() { Strength = 6, Agility = 6, Stamina = 8 }));
        AddFree(sm, 3, Weapon(30456, "Mograine's Might", "inv_hammer_10", 5, 10, 2.9, WeaponType.Mace, 38, HybridWeapons, new() { Strength = 9, Intelligence = 8, Stamina = 21 }));
        AddFree(sm, 3, Armor(30457, "Mograine's Resolve", "inv_jewelry_talisman_12", ArmorSlot.Trinket, 38, new() { Strength = 7, Intelligence = 6, Stamina = 9 }));
        AddFree(sm, 4, Weapon(30458, "Scepter of Resurrection", "inv_wand_07", 4, 8, 1.9, WeaponType.Wand, 40, Wands, new() { Intelligence = 13, Spirit = 7, Stamina = 20 }));
        AddFree(sm, 4, Weapon(30459, "Bow of the Crusade", "inv_weapon_bow_07", 4, 8, 3.4, WeaponType.Bow, 40, Bows, new() { Agility = 14, Stamina = 21 }));
        AddFree(sm, 4, Armor(30460, "Triune Amulet", "inv_jewelry_amulet_03", ArmorSlot.Neck, 40, new() { Intelligence = 9, Spirit = 7, Stamina = 8 }));
        AddFree(sm, 4, Armor(30461, "Whitemane's Mantle", "inv_misc_cape_20", ArmorSlot.Cape, 40, new() { Strength = 6, Agility = 6, Stamina = 9 }));

        // Green items: two armor pieces and a weapon per boss
        AddFree(sm, 0, Green(30470, "Graveyard Shroud", "inv_misc_cape_16", ArmorSlot.Cape, 34, new() { Intelligence = 5, Spirit = 4 }));
        AddFree(sm, 0, Green(30471, "Bonechill Band", "inv_jewelry_ring_13", ArmorSlot.Ring, 34, new() { Agility = 5, Stamina = 4 }));
        AddFree(sm, 0, GreenWeapon(30472, "Gravewatcher's Wand", "inv_wand_05", 3, 6, 1.9, WeaponType.Wand, 34, Wands, new() { Intelligence = 6, Stamina = 11 }));
        AddFree(sm, 1, Green(30473, "Librarian's Sash", "inv_belt_08", ArmorSlot.Belt, 35, new() { Intelligence = 5, Stamina = 4 }));
        AddFree(sm, 1, Green(30474, "Tome-Bound Bracers", "inv_bracer_02", ArmorSlot.Wrists, 35, new() { Intelligence = 4, Spirit = 5 }));
        AddFree(sm, 1, GreenWeapon(30475, "Arcanist's Blade", "inv_sword_04", 3, 6, 2.3, WeaponType.Sword, 35, BladeWeapons, new() { Agility = 6, Strength = 4, Stamina = 11 }));
        AddFree(sm, 2, Green(30476, "Armory Gauntlets", "inv_gauntlets_26", ArmorSlot.Hands, 37, new() { Strength = 5, Stamina = 5 }));
        AddFree(sm, 2, Green(30477, "Scarlet Belt", "inv_belt_14", ArmorSlot.Belt, 37, new() { Agility = 5, Stamina = 5 }));
        AddFree(sm, 2, GreenWeapon(30478, "Scarlet Shortbow", "inv_weapon_bow_07", 3, 6, 3.4, WeaponType.Bow, 37, Bows, new() { Agility = 6, Stamina = 11 }));
        AddFree(sm, 3, Green(30479, "Crusader's Cloak", "inv_misc_cape_11", ArmorSlot.Cape, 38, new() { Strength = 5, Agility = 5 }));
        AddFree(sm, 3, Green(30480, "Ashen Band", "inv_jewelry_ring_15", ArmorSlot.Ring, 38, new() { Intelligence = 5, Spirit = 5 }));
        AddFree(sm, 3, GreenWeapon(30481, "Crusader's Battleaxe", "inv_axe_09", 4, 7, 2.8, WeaponType.Axe, 38, StrengthWeapons, new() { Strength = 6, Stamina = 12 }));
        AddFree(sm, 4, Green(30482, "Cathedral Sabatons", "inv_boots_plate_01", ArmorSlot.Feet, 40, new() { Strength = 5, Stamina = 6 }));
        AddFree(sm, 4, Green(30483, "Chapel Pendant", "inv_jewelry_amulet_05", ArmorSlot.Neck, 40, new() { Intelligence = 5, Spirit = 5 }));
        AddFree(sm, 4, GreenWeapon(30484, "Cathedral Staff", "inv_staff_08", 4, 7, 3.0, WeaponType.Staff, 40, Staves, new() { Intelligence = 6, Spirit = 4, Stamina = 12 }));
    }
}
