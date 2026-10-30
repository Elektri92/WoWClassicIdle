// Models/Bag.cs
namespace Models
{
    /// <summary>
    /// A container with a fixed slot count (backpack or equipped bag).
    /// Empty slots are <c>null</c>.
    /// </summary>
    public class Bag
    {
        public string Name { get; }
        public string IconUrl { get; }

        /// <summary>The bag item this container comes from (<c>null</c> for the backpack).</summary>
        public BagItem? Source { get; }

        private readonly ItemStack?[] _slots;
        public IReadOnlyList<ItemStack?> Slots => _slots;

        public int SlotCount => _slots.Length;
        public int FreeSlots => _slots.Count(s => s is null);
        public bool IsEmpty => FreeSlots == SlotCount;

        public Bag(string name, string iconUrl, int slotCount, BagItem? source = null)
        {
            Name = name;
            IconUrl = iconUrl;
            Source = source;
            _slots = new ItemStack?[slotCount];
        }

        public static Bag FromItem(BagItem item) => new(item.Name, item.IconUrl, item.SlotCount, item);

        public ItemStack? this[int index] => _slots[index];

        /// <summary>Tops up existing stacks of the item. Returns the amount that did NOT fit.</summary>
        public int FillStacks(Item item, int count)
        {
            if (!item.IsStackable) return count;

            foreach (var stack in _slots)
            {
                if (count == 0) return 0;
                if (stack is not null && stack.Item.Id == item.Id)
                    count -= stack.Add(count);
            }
            return count;
        }

        /// <summary>Puts items into free slots. Returns the amount that did NOT fit.</summary>
        public int AddToFreeSlots(Item item, int count)
        {
            for (int i = 0; i < _slots.Length && count > 0; i++)
            {
                if (_slots[i] is not null) continue;
                int amount = Math.Min(count, item.MaxStack);
                _slots[i] = new ItemStack(item, amount);
                count -= amount;
            }

            return count;
        }

        /// <summary>Removes the complete stack from a slot and returns it.</summary>
        public ItemStack? Take(int index)
        {
            var stack = _slots[index];
            _slots[index] = null;
            return stack;
        }

        /// <summary>Puts a stack into an empty slot. Returns <c>false</c> if the slot is occupied.</summary>
        public bool Put(int index, ItemStack stack)
        {
            if (_slots[index] is not null) return false;
            _slots[index] = stack;
            return true;
        }

        public void Swap(int a, int b) => (_slots[a], _slots[b]) = (_slots[b], _slots[a]);

        /// <summary>Removes up to <paramref name="count"/> items with the ID and returns the removed amount.</summary>
        public int Remove(int itemId, int count)
        {
            int removed = 0;
            for (int i = 0; i < _slots.Length && removed < count; i++)
            {
                var stack = _slots[i];
                if (stack is null || stack.Item.Id != itemId) continue;
                removed += stack.Remove(count - removed);
                if (stack.Count == 0) _slots[i] = null;
            }
            return removed;
        }

        public int CountOf(int itemId) =>
            _slots.Where(s => s is not null && s.Item.Id == itemId).Sum(s => s!.Count);
    }
}
