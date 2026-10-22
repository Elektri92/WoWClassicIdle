// Models/Inventory.cs
namespace Models
{
    /// <summary>
    /// Inventar wie in WoW Classic: fester Rucksack (16 Slots) + 4 Taschenplätze.
    /// </summary>
    public class Inventory
    {
        public const int BackpackSlotCount = 16;
        public const int BagSlotCount = 4;

        public const string BackpackIconUrl = "https://wow.zamimg.com/images/wow/icons/large/inv_misc_bag_08.jpg";

        public Bag Backpack { get; } = new("Backpack", BackpackIconUrl, BackpackSlotCount);

        private readonly Bag?[] _bags = new Bag?[BagSlotCount];

        /// <summary>Die 4 Taschenplätze (leer = <c>null</c>).</summary>
        public IReadOnlyList<Bag?> Bags => _bags;

        /// <summary>Rucksack + alle ausgerüsteten Taschen, in Füll-Reihenfolge.</summary>
        public IEnumerable<Bag> AllContainers
        {
            get
            {
                yield return Backpack;
                foreach (var bag in _bags)
                    if (bag is not null) yield return bag;
            }
        }

        public int TotalSlots => AllContainers.Sum(b => b.SlotCount);
        public int FreeSlots  => AllContainers.Sum(b => b.FreeSlots);

        /// <summary>Fügt Items hinzu (Rucksack zuerst). Gibt die Menge zurück, die nicht gepasst hat.</summary>
        public int TryAdd(Item item, int count = 1)
        {
            foreach (var bag in AllContainers)
            {
                count = bag.TryAdd(item, count);
                if (count == 0) break;
            }
            return count;
        }

        public int Remove(int itemId, int count = 1)
        {
            int removed = 0;
            foreach (var bag in AllContainers)
            {
                removed += bag.Remove(itemId, count - removed);
                if (removed == count) break;
            }
            return removed;
        }

        public int CountOf(int itemId) => AllContainers.Sum(b => b.CountOf(itemId));

        /// <summary>Rüstet eine Tasche in einen freien Taschenplatz aus.</summary>
        public bool EquipBag(int slot, BagItem item)
        {
            if (_bags[slot] is not null) return false;
            _bags[slot] = Bag.FromItem(item);
            return true;
        }

        /// <summary>Nimmt eine Tasche ab – nur möglich, wenn sie leer ist (wie im Original).</summary>
        public BagItem? UnequipBag(int slot)
        {
            var bag = _bags[slot];
            if (bag is null || !bag.IsEmpty) return null;
            _bags[slot] = null;
            return bag.Source;
        }
    }
}
