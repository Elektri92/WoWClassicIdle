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
    }

    /// <summary>Anzeigename eines Rüstungsslots (WoW-Classic-Slotnamen fürs UI).</summary>
    public static class ArmorSlots
    {
        public static string Label(ArmorSlot slot) => slot switch
        {
            ArmorSlot.Hands  => "Hands",
            ArmorSlot.Legs   => "Legs",
            ArmorSlot.Chest  => "Chest",
            ArmorSlot.Belt   => "Waist",
            ArmorSlot.Feet   => "Feet",
            ArmorSlot.Wrists => "Wrist",
            ArmorSlot.Cape   => "Back",
            _                => slot.ToString(),
        };
    }

    /// <summary>Ein Rüstungsteil das in einen Ausrüstungsslot angelegt werden kann.</summary>
    public record ArmorItem(
        int Id,
        string Name,
        string Icon,
        ArmorSlot Slot,
        PrimaryStats Bonuses,
        int RequiredLevel = 1,
        ItemQuality Quality = ItemQuality.Uncommon,
        long SellPrice = 0)
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
        CharacterClass? RequiredClass = null)
        : Item(Id, Name, Icon, Quality, MaxStack: 1, SellPrice);

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
