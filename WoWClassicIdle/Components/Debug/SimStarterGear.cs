// Debug/SimStarterGear.cs – wird nur im DEBUG-Build kompiliert (siehe .csproj)
namespace DebugTools;

using Models;

/// <summary>
/// Rüstet einen Simulations-Charakter mit stufengerechtem grünem (Uncommon) Gear aus.
/// Stat-Werte entsprechen den grünen Loot-Tier-Stufen; Stats sind spec-spezifisch gewählt.
/// Die erzeugten Items haben negative IDs (nie gespeichert, nur im Simulator).
/// </summary>
internal static class SimStarterGear
{
    private static readonly (ArmorSlot Slot, string Name, string Icon)[] Slots =
    [
        (ArmorSlot.Head,    "Helm",      "inv_helmet_03"),
        (ArmorSlot.Neck,    "Necklace",  "inv_jewelry_necklace_06"),
        (ArmorSlot.Chest,   "Chestpiece","inv_chest_cloth_05"),
        (ArmorSlot.Wrists,  "Bracers",   "inv_bracer_07"),
        (ArmorSlot.Hands,   "Gloves",    "inv_gauntlets_04"),
        (ArmorSlot.Belt,    "Belt",      "inv_belt_05"),
        (ArmorSlot.Legs,    "Legguards", "inv_pants_03"),
        (ArmorSlot.Feet,    "Boots",     "inv_boots_08"),
        (ArmorSlot.Cape,    "Cloak",     "inv_misc_cape_18"),
        (ArmorSlot.Ring,    "Ring",      "inv_jewelry_ring_03"),
        (ArmorSlot.Trinket, "Trinket",   "inv_trinket_naxxramas01"),
    ];

    /// <summary>Equips the player with level-appropriate green starter gear for the given spec.</summary>
    public static void Equip(Player player, CharacterClass cls, int specIndex, int level)
    {
        var weapon = WeaponRegistry.GetTierWeaponForLevel(cls, level);
        if (weapon is not null)
            player.ForceEquipWeapon(weapon);

        int statVal       = StatValForLevel(level);
        int reqLevel      = Math.Max(1, level - 4);
        var bonusFn       = BonusFn(cls, specIndex);
        int fakeId        = -1;

        foreach (var (slot, name, icon) in Slots)
        {
            var item = new ArmorItem(
                fakeId--,
                $"Worn {name}",
                icon,
                slot,
                bonusFn(statVal),
                reqLevel,
                ItemQuality.Uncommon,
                SellPrice: 0);
            player.ForceEquipArmor(item);
        }
    }

    // Passt zu den Loot-Tier-Stufen von RareLootRegistry (grüne comboVal-Werte)
    private static int StatValForLevel(int level) => level switch
    {
        <= 5  => 1,
        <= 10 => 2,
        <= 15 => 2,
        <= 20 => 3,
        <= 25 => 4,
        <= 30 => 5,
        <= 35 => 6,
        <= 40 => 7,
        <= 45 => 8,
        <= 50 => 9,
        <= 55 => 10,
        _     => 11,
    };

    // Gibt eine spec-passende 2-Stat-Kombination zurück
    private static Func<int, PrimaryStats> BonusFn(CharacterClass cls, int specIndex) =>
        (cls, specIndex) switch
        {
            // Warrior: Arms(0)/Fury(1) → Str+Agi; Protection(2) → Str+Sta
            (CharacterClass.Warrior, 2) => v => new PrimaryStats { Strength = v, Stamina      = v },
            (CharacterClass.Warrior, _) => v => new PrimaryStats { Strength = v, Agility      = v },

            // Paladin: Holy(0) → Int+Spi; Protection(1) → Str+Sta; Retribution(2) → Str+Agi
            (CharacterClass.Paladin, 0) => v => new PrimaryStats { Intelligence = v, Spirit   = v },
            (CharacterClass.Paladin, 1) => v => new PrimaryStats { Strength = v, Stamina      = v },
            (CharacterClass.Paladin, 2) => v => new PrimaryStats { Strength = v, Agility      = v },

            // Hunter: Beast Mastery(0)/Marksmanship(1)/Survival(2) → Agi+Sta
            (CharacterClass.Hunter,  _) => v => new PrimaryStats { Agility  = v, Stamina      = v },

            // Rogue: alle Specs → Agi+Str
            (CharacterClass.Rogue,   _) => v => new PrimaryStats { Agility  = v, Strength     = v },

            // Priest: Discipline(0)/Holy(1) → Int+Spi; Shadow(2) → Int+Spi
            (CharacterClass.Priest,  _) => v => new PrimaryStats { Intelligence = v, Spirit   = v },

            // Mage: alle Specs → Int+Spi
            (CharacterClass.Mage,    _) => v => new PrimaryStats { Intelligence = v, Spirit   = v },

            // Warlock: alle Specs → Int+Spi
            (CharacterClass.Warlock, _) => v => new PrimaryStats { Intelligence = v, Spirit   = v },

            // Shaman: Elemental(0) → Int+Spi; Enhancement(1) → Agi+Str; Restoration(2) → Int+Spi
            (CharacterClass.Shaman, 1)  => v => new PrimaryStats { Agility  = v, Strength     = v },
            (CharacterClass.Shaman,  _) => v => new PrimaryStats { Intelligence = v, Spirit   = v },

            // Druid: Balance(0) → Int+Spi; Feral Combat(1) → Agi+Str; Restoration(2) → Int+Spi
            (CharacterClass.Druid,  1)  => v => new PrimaryStats { Agility  = v, Strength     = v },
            (CharacterClass.Druid,   _) => v => new PrimaryStats { Intelligence = v, Spirit   = v },

            _ => v => new PrimaryStats { Strength = v, Stamina = v },
        };
}
