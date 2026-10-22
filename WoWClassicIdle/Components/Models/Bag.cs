// Models/Bag.cs
namespace Models
{
    /// <summary>
    /// Ein Container mit fester Slot-Anzahl (Rucksack oder ausgerüstete Tasche).
    /// Leere Slots sind <c>null</c>.
    /// </summary>
    public class Bag
    {
        public string Name { get; }
        public string IconUrl { get; }

        /// <summary>Das Taschen-Item, aus dem dieser Container stammt (<c>null</c> beim Rucksack).</summary>
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

        /// <summary>
        /// Fügt Items hinzu: zuerst auf bestehende Stacks, dann in freie Slots.
        /// Gibt die Menge zurück, die NICHT mehr hineingepasst hat.
        /// </summary>
        public int TryAdd(Item item, int count)
        {
            if (item.IsStackable)
            {
                foreach (var stack in _slots)
                {
                    if (count == 0) return 0;
                    if (stack is not null && stack.Item.Id == item.Id)
                        count -= stack.Add(count);
                }
            }

            for (int i = 0; i < _slots.Length && count > 0; i++)
            {
                if (_slots[i] is not null) continue;
                int amount = Math.Min(count, item.MaxStack);
                _slots[i] = new ItemStack(item, amount);
                count -= amount;
            }

            return count;
        }

        /// <summary>Entfernt den kompletten Stack aus einem Slot und gibt ihn zurück.</summary>
        public ItemStack? Take(int index)
        {
            var stack = _slots[index];
            _slots[index] = null;
            return stack;
        }

        /// <summary>Legt einen Stack in einen leeren Slot. Gibt <c>false</c> zurück, wenn der Slot belegt ist.</summary>
        public bool Put(int index, ItemStack stack)
        {
            if (_slots[index] is not null) return false;
            _slots[index] = stack;
            return true;
        }

        public void Swap(int a, int b) => (_slots[a], _slots[b]) = (_slots[b], _slots[a]);

        /// <summary>Entfernt bis zu <paramref name="count"/> Items mit der ID und gibt die entfernte Menge zurück.</summary>
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
