// Models/Item.cs
namespace Models
{
    /// <summary>Item quality like in WoW Classic (determines name/border color).</summary>
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
    /// Immutable item definition (template). Several stacks can share the same definition.
    /// <see cref="Icon"/> is the icon name on wow.zamimg.com, e.g. "inv_misc_bag_08".
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

    /// <summary>A bag – can be equipped into one of the 4 bag slots.</summary>
    public record BagItem(
        int Id,
        string Name,
        string Icon,
        int SlotCount,
        ItemQuality Quality = ItemQuality.Common,
        long SellPrice = 0)
        : Item(Id, Name, Icon, Quality, MaxStack: 1, SellPrice);

    /// <summary>
    /// Profession item (raw material or crafted consumable) – stacks up to 9999.
    /// <paramref name="IsCraftingReagent"/> = used in recipes (tooltip line "Crafting Reagent").
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

    /// <summary>Display name of an armor slot (WoW Classic slot names for the UI).</summary>
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
    /// Special effect of a worn armor piece beyond its primary stats.
    /// <paramref name="PotionLevel"/> = required level of the potions improved by <paramref name="PotionBonus"/>.
    /// </summary>
    public record EquipEffect(string Text, double CritChance = 0, int PotionLevel = 0, double PotionBonus = 0);

    /// <summary>
    /// An armor piece that can be equipped into an equipment slot.
    /// <paramref name="RequiredClass"/> = only for this class (crafted class sets), <c>null</c> = all classes.
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
        CharacterClass? RequiredClass = null,
        EquipEffect? Effect = null)
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

    /// <summary>Requirements of an item (level/class) that are checked when equipping.</summary>
    public static class ItemRequirements
    {
        /// <summary>Required level of the item (1 if the item requires no level).</summary>
        public static int LevelOf(Item item) => item switch
        {
            ArmorItem      a => a.RequiredLevel,
            WeaponItem     w => w.RequiredLevel,
            ConsumableItem c => c.RequiredLevel,
            _               => 1,
        };

        public static bool MatchesLevel(Item item, int level) => LevelOf(item) <= level;

        /// <summary>Required class of the item (<c>null</c> = all classes).</summary>
        public static CharacterClass? ClassOf(Item item) => item switch
        {
            ArmorItem  a => a.RequiredClass,
            WeaponItem w => w.RequiredClass,
            _            => null,
        };

        /// <summary>Class-specific weapons and crafted class armor are bound to their class.</summary>
        public static bool MatchesClass(Item item, CharacterClass cls) =>
            ClassOf(item) is not { } required || required == cls;
    }

    /// <summary>A stack of an item in a bag slot.</summary>
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

        /// <summary>Adds up to <paramref name="amount"/> and returns the amount actually added.</summary>
        public int Add(int amount)
        {
            int added = Math.Min(amount, SpaceLeft);
            Count += added;
            return added;
        }

        /// <summary>Takes off up to <paramref name="amount"/> and returns the amount actually removed.</summary>
        public int Remove(int amount)
        {
            int removed = Math.Min(amount, Count);
            Count -= removed;
            return removed;
        }
    }
}
