// Models/Inventory.cs
namespace Models
{
    /// <summary>
    /// Inventory like in WoW Classic: fixed backpack (32 slots) + 4 bag slots.
    /// The bag slots are unlocked automatically by level and grow with the character.
    /// </summary>
    public class Inventory
    {
        public const int BackpackSlotCount = 32;
        public const int BagSlotCount = 4;

        /// <summary>Level thresholds at which the individual bag slots are unlocked (slot 0-3).
        /// Deliberately not on the levels of other features (see GameFeatureRegistry) – one unlock popup per level.</summary>
        public static readonly int[] BagUnlockLevels = { 2, 4, 8, 12 };

        /// <summary>Base size of a newly unlocked bag.</summary>
        public const int BaseBagSlotSize = 12;

        /// <summary>Bag slots new crafting materials go to first: Bag 1 + Bag 3.</summary>
        private static readonly int[] MaterialBagSlots = { 0, 2 };

        /// <summary>Bag slots new consumables go to first: Bag 2 + Bag 4.</summary>
        private static readonly int[] ConsumableBagSlots = { 1, 3 };

        public const string BackpackIconUrl = "https://wow.zamimg.com/images/wow/icons/large/inv_misc_bag_08.jpg";
        private const string VirtualBagIconUrl = "https://wow.zamimg.com/images/wow/icons/large/inv_misc_bag_07.jpg";

        /// <summary>Number of unlocked bag slots for the given level.</summary>
        public static int BagUnlockedCount(int level) =>
            BagUnlockLevels.Count(l => level >= l);

        /// <summary>Slot count per bag for the given level (12 + 1 per started 5 levels).</summary>
        public static int BagSlotSize(int level) =>
            BaseBagSlotSize + level / 5;

        /// <summary>Level at which all bags grow next.</summary>
        public static int NextBagGrowthLevel(int level) => (level / 5 + 1) * 5;

        public Bag Backpack { get; } = new("Backpack", BackpackIconUrl, BackpackSlotCount);

        private readonly Bag?[] _bags = new Bag?[BagSlotCount];

        /// <summary>The 4 bag slots (locked = <c>null</c>).</summary>
        public IReadOnlyList<Bag?> Bags => _bags;

        /// <summary>Backpack + all unlocked bags, in fill order.</summary>
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

        /// <summary>
        /// Adds items: existing stacks in any container are filled before a new slot is used,
        /// new slots follow <see cref="PlacementOrder"/>. Returns the amount that didn't fit.
        /// </summary>
        public int TryAdd(Item item, int count = 1)
        {
            var containers = PlacementOrder(item).ToList();
            foreach (var bag in containers)
                count = bag.FillStacks(item, count);
            foreach (var bag in containers)
                count = bag.AddToFreeSlots(item, count);
            return count;
        }

        /// <summary>Preferred bags of the item's category first, then the remaining containers in fill order.</summary>
        private IEnumerable<Bag> PlacementOrder(Item item)
        {
            int[] preferredSlots = item switch
            {
                ConsumableItem => ConsumableBagSlots,
                TradeItem      => MaterialBagSlots,
                _              => [],
            };
            var preferred = preferredSlots.Select(i => _bags[i]).OfType<Bag>().ToList();
            return preferred.Concat(AllContainers.Except(preferred));
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
        /// Splits <paramref name="amount"/> items off a stack and puts them into the next free slot
        /// (same bag first, then the other containers). <c>false</c> if there is no space or the amount is invalid.
        /// </summary>
        public bool SplitStack(Bag bag, int index, int amount)
        {
            if (bag[index] is not { } stack || amount < 1 || amount >= stack.Count) return false;

            var target = bag.FreeSlots > 0 ? bag : AllContainers.FirstOrDefault(b => b.FreeSlots > 0);
            if (target is null) return false;

            int freeIndex = Enumerable.Range(0, target.SlotCount).First(i => target[i] is null);
            stack.Remove(amount);
            target.Put(freeIndex, new ItemStack(stack.Item, amount));
            return true;
        }

        /// <summary>
        /// Moves a stack to another slot (also across containers): fills up a stack of the same item,
        /// otherwise swaps with whatever is in the target slot. <c>false</c> if there is nothing to move.
        /// </summary>
        public bool MoveStack(Bag from, int fromIndex, Bag to, int toIndex)
        {
            if (from[fromIndex] is not { } moving) return false;
            if (ReferenceEquals(from, to) && fromIndex == toIndex) return false;

            if (to[toIndex] is { } existing && existing.Item.Id == moving.Item.Id && existing.SpaceLeft > 0)
            {
                moving.Remove(existing.Add(moving.Count));
                if (moving.Count == 0) from.Take(fromIndex);
                return true;
            }

            var displaced = to.Take(toIndex);
            from.Take(fromIndex);
            to.Put(toIndex, moving);
            if (displaced is not null) from.Put(fromIndex, displaced);
            return true;
        }

        /// <summary>
        /// Creates or enlarges virtual bags based on the player level.
        /// Bag slots with a manually equipped BagItem (Source != null) are not changed.
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

        /// <summary>Equips a bag into a free bag slot.</summary>
        public bool EquipBag(int slot, BagItem item)
        {
            if (_bags[slot] is not null) return false;
            _bags[slot] = Bag.FromItem(item);
            return true;
        }

        /// <summary>Removes a bag – only possible if it is empty (like in the original).</summary>
        public BagItem? UnequipBag(int slot)
        {
            var bag = _bags[slot];
            if (bag is null || !bag.IsEmpty) return null;
            _bags[slot] = null;
            return bag.Source;
        }
    }
}
