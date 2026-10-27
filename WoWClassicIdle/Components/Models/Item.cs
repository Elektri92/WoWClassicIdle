// Models/Item.cs
namespace Models
{
    /// <summary>Item-Qualität wie in WoW Classic (bestimmt Namens-/Rahmenfarbe).</summary>
    public enum ItemQuality
    {
        Poor,
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary,
    }

    public static class ItemQualityColors
    {
        public static string Hex(ItemQuality quality) => quality switch
        {
            ItemQuality.Poor      => "#9d9d9d",
            ItemQuality.Common    => "#ffffff",
            ItemQuality.Uncommon  => "#1eff00",
            ItemQuality.Rare      => "#0070dd",
            ItemQuality.Epic      => "#a335ee",
            ItemQuality.Legendary => "#ff8000",
            _                     => "#ffffff",
        };
    }

    /// <summary>
    /// Unveränderliche Item-Definition (Template). Mehrere Stacks können dieselbe Definition teilen.
    /// <see cref="Icon"/> ist der Icon-Name auf wow.zamimg.com, z.B. "inv_misc_bag_08".
    /// </summary>
    public record Item(
        int Id,
        string Name,
        string Icon,
        ItemQuality Quality = ItemQuality.Common,
        int MaxStack = 1,
        long SellPrice = 0)
    {
        public string IconUrl => $"https://wow.zamimg.com/images/wow/icons/large/{Icon}.jpg";
        public bool IsStackable => MaxStack > 1;
    }

    /// <summary>Eine Tasche – kann in einen der 4 Taschenplätze ausgerüstet werden.</summary>
    public record BagItem(
        int Id,
        string Name,
        string Icon,
        int SlotCount,
        ItemQuality Quality = ItemQuality.Common,
        long SellPrice = 0)
        : Item(Id, Name, Icon, Quality, MaxStack: 1, SellPrice);

    /// <summary>
    /// Berufs-Item (Rohstoff oder hergestelltes Verbrauchsgut) – stapelt bis 9999.
    /// <paramref name="IsCraftingReagent"/> = wird in Rezepten verarbeitet (Tooltip-Zeile "Crafting Reagent").
    /// </summary>
    public record TradeItem(
        int Id,
        string Name,
        string Icon,
        bool IsCraftingReagent,
        ItemQuality Quality = ItemQuality.Common,
        long SellPrice = 1)
        : Item(Id, Name, Icon, Quality, MaxStack: TradeItem.StackSize, SellPrice)
    {
        public const int StackSize = 9999;
    }

    public enum WeaponType { Sword, Mace, Axe, Dagger, Staff, Wand, Bow }

    public enum ArmorSlot
    {
        Hands,
        Legs,
        Chest,
        Belt,
        Feet,
        Wrists,
        Cape,
        Ring,
        Head,
        Neck,
        Trinket,
    }

    /// <summary>Anzeigename eines Rüstungsslots (WoW-Classic-Slotnamen fürs UI).</summary>
    public static class ArmorSlots
    {
        public static string Label(ArmorSlot slot) => slot switch
        {
            ArmorSlot.Hands   => "Hands",
            ArmorSlot.Legs    => "Legs",
            ArmorSlot.Chest   => "Chest",
            ArmorSlot.Belt    => "Waist",
            ArmorSlot.Feet    => "Feet",
            ArmorSlot.Wrists  => "Wrist",
            ArmorSlot.Cape    => "Back",
            ArmorSlot.Ring    => "Finger",
            ArmorSlot.Head    => "Head",
            ArmorSlot.Neck    => "Neck",
            ArmorSlot.Trinket => "Trinket",
            _                 => slot.ToString(),
        };
    }

    /// <summary>
    /// Ein Rüstungsteil das in einen Ausrüstungsslot angelegt werden kann.
    /// <paramref name="RequiredClass"/> = nur für diese Klasse (hergestellte Klassen-Sets), <c>null</c> = alle Klassen.
    /// </summary>
    public record ArmorItem(
        int Id,
        string Name,
        string Icon,
        ArmorSlot Slot,
        PrimaryStats Bonuses,
        int RequiredLevel = 1,
        ItemQuality Quality = ItemQuality.Uncommon,
        long SellPrice = 0,
        CharacterClass? RequiredClass = null)
        : Item(Id, Name, Icon, Quality, MaxStack: 1, SellPrice);

    /// <summary>A weapon item that can be equipped in a weapon slot.</summary>
    public record WeaponItem(
        int Id,
        string Name,
        string Icon,
        int MinDamage,
        int MaxDamage,
        double Speed,
        WeaponType Type,
        ItemQuality Quality = ItemQuality.Common,
        long SellPrice = 0,
        int RequiredLevel = 1,
        CharacterClass? RequiredClass = null,
        PrimaryStats? Bonuses = null)
        : Item(Id, Name, Icon, Quality, MaxStack: 1, SellPrice);

    /// <summary>Anforderungen eines Items (Level/Klasse), die beim Anlegen geprüft werden.</summary>
    public static class ItemRequirements
    {
        /// <summary>Gefordertes Level des Items (1 wenn das Item kein Level verlangt).</summary>
        public static int LevelOf(Item item) => item switch
        {
            ArmorItem      a => a.RequiredLevel,
            WeaponItem     w => w.RequiredLevel,
            ConsumableItem c => c.RequiredLevel,
            _               => 1,
        };

        public static bool MatchesLevel(Item item, int level) => LevelOf(item) <= level;

        /// <summary>Geforderte Klasse des Items (<c>null</c> = alle Klassen).</summary>
        public static CharacterClass? ClassOf(Item item) => item switch
        {
            ArmorItem  a => a.RequiredClass,
            WeaponItem w => w.RequiredClass,
            _            => null,
        };

        /// <summary>Klassenspezifische Waffen und hergestellte Klassen-Rüstung sind an ihre Klasse gebunden.</summary>
        public static bool MatchesClass(Item item, CharacterClass cls) =>
            ClassOf(item) is not { } required || required == cls;
    }

    /// <summary>Ein Stapel eines Items in einem Taschenslot.</summary>
    public class ItemStack
    {
        public Item Item { get; }
        public int Count { get; private set; }

        public ItemStack(Item item, int count = 1)
        {
            if (count < 1 || count > item.MaxStack)
                throw new ArgumentOutOfRangeException(nameof(count), $"Count must be between 1 and {item.MaxStack}.");
            Item = item;
            Count = count;
        }

        public int SpaceLeft => Item.MaxStack - Count;

        /// <summary>Legt bis zu <paramref name="amount"/> drauf und gibt die tatsächlich hinzugefügte Menge zurück.</summary>
        public int Add(int amount)
        {
            int added = Math.Min(amount, SpaceLeft);
            Count += added;
            return added;
        }

        /// <summary>Nimmt bis zu <paramref name="amount"/> herunter und gibt die tatsächlich entfernte Menge zurück.</summary>
        public int Remove(int amount)
        {
            int removed = Math.Min(amount, Count);
            Count -= removed;
            return removed;
        }
    }
}
