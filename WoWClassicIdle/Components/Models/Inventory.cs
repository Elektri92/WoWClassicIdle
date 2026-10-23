// Models/Inventory.cs
namespace Models
{
    /// <summary>
    /// Inventar wie in WoW Classic: fester Rucksack (32 Slots) + 4 Taschenplätze.
    /// Die Taschenplätze werden automatisch per Level freigeschaltet und wachsen mit dem Charakter.
    /// </summary>
    public class Inventory
    {
        public const int BackpackSlotCount = 32;
        public const int BagSlotCount = 4;

        /// <summary>Level-Schwellen, bei denen die einzelnen Taschenplätze freigeschaltet werden (Platz 0-3).</summary>
        public static readonly int[] BagUnlockLevels = { 3, 5, 8, 12 };

        /// <summary>Basisgröße einer neu freigeschalteten Tasche.</summary>
        public const int BaseBagSlotSize = 12;

        public const string BackpackIconUrl = "https://wow.zamimg.com/images/wow/icons/large/inv_misc_bag_08.jpg";
        private const string VirtualBagIconUrl = "https://wow.zamimg.com/images/wow/icons/large/inv_misc_bag_07.jpg";

        /// <summary>Anzahl freigeschalteter Taschenplätze für das gegebene Level.</summary>
        public static int BagUnlockedCount(int level) =>
            BagUnlockLevels.Count(l => level >= l);

        /// <summary>Slot-Anzahl je Tasche für das gegebene Level (12 + 1 je angefangene 5 Level).</summary>
        public static int BagSlotSize(int level) =>
            BaseBagSlotSize + level / 5;

        /// <summary>Level, bei dem alle Taschen das nächste Mal wachsen.</summary>
        public static int NextBagGrowthLevel(int level) => (level / 5 + 1) * 5;

        public Bag Backpack { get; } = new("Backpack", BackpackIconUrl, BackpackSlotCount);

        private readonly Bag?[] _bags = new Bag?[BagSlotCount];

        /// <summary>Die 4 Taschenplätze (gesperrt = <c>null</c>).</summary>
        public IReadOnlyList<Bag?> Bags => _bags;

        /// <summary>Rucksack + alle freigeschalteten Taschen, in Füll-Reihenfolge.</summary>
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

        /// <summary>
        /// Erstellt oder vergrößert virtuelle Taschen basierend auf dem Spielerlevel.
        /// Taschenplätze mit einem manuell ausgerüsteten BagItem (Source != null) werden nicht verändert.
        /// </summary>
        public void UpdateBagsForLevel(int level)
        {
            int unlocked = BagUnlockedCount(level);
            int size = BagSlotSize(level);

            for (int i = 0; i < BagSlotCount; i++)
            {
                if (i >= unlocked) continue;

                if (_bags[i] is null)
                {
                    _bags[i] = new Bag($"Bag {i + 1}", VirtualBagIconUrl, size);
                }
                else if (_bags[i]!.Source is null && _bags[i]!.SlotCount < size)
                {
                    _bags[i] = GrowBag(_bags[i]!, size);
                }
            }
        }

        private static Bag GrowBag(Bag old, int newSize)
        {
            var bag = new Bag(old.Name, old.IconUrl, newSize);
            for (int s = 0; s < old.SlotCount; s++)
                if (old[s] is { } stack) bag.Put(s, stack);
            return bag;
        }

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
