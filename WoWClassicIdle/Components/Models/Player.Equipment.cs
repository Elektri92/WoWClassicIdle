// Models/Player.Equipment.cs
namespace Models
{
    // Create new systems as their own partial file; keep files below 300 lines where possible.
    public partial class Player
    {
        internal void RestoreEquippedWeapons(Dictionary<CharacterClass, WeaponItem?> weapons)
        {
            _equippedByClass.Clear();
            foreach (var (cls, weapon) in weapons)
                _equippedByClass[cls] = weapon;
            EquippedWeapon = _equippedByClass.TryGetValue(ActiveClass, out var w) ? w : null;
        }

        /// <summary>
        /// Equips a weapon from the given bag slot.
        /// The old weapon (if any) is returned to the inventory.
        /// Returns false if the level/class requirements are not met – the item stays in the bag.
        /// </summary>
        public bool EquipWeapon(WeaponItem weapon, Bag sourceBag, int sourceSlot)
        {
            if (!MeetsItemRequirements(weapon)) return false;

            sourceBag.Take(sourceSlot);
            if (EquippedWeapon is not null)
                Inventory.TryAdd(EquippedWeapon);
            EquippedWeapon = weapon;
            _equippedByClass[ActiveClass] = weapon;
            RecalculateStatsKeepResources();
            return true;
        }

        /// <summary>
        /// Moves the equipped weapon back to the inventory.
        /// Returns false if the inventory is full.
        /// </summary>
        public bool UnequipWeapon()
        {
            if (EquippedWeapon is null) return true;
            if (Inventory.TryAdd(EquippedWeapon) > 0) return false;
            EquippedWeapon = null;
            _equippedByClass[ActiveClass] = null;
            RecalculateStatsKeepResources();
            return true;
        }

        /// <summary>Position an item goes to without an explicit choice: the first free one, otherwise the first one.</summary>
        public ArmorSlot EquipTargetFor(ArmorItem item)
        {
            var positions = ArmorSlots.PositionsFor(item.Slot);
            foreach (var position in positions)
                if (!EquippedArmor.ContainsKey(position))
                    return position;
            return positions[0];
        }

        /// <summary>How many copies of the item are currently worn.</summary>
        public int WornCount(ArmorItem item) =>
            ArmorSlots.PositionsFor(item.Slot).Count(p => EquippedArmor.TryGetValue(p, out var worn) && worn.Id == item.Id);

        /// <summary>
        /// Equips an armor piece from the given bag slot, at <paramref name="target"/> or else at <see cref="EquipTargetFor"/>.
        /// The previously worn piece (if any) goes back into the inventory.
        /// Returns false if the required level is not reached – the item stays in the bag.
        /// </summary>
        public bool EquipArmor(ArmorItem item, Bag sourceBag, int sourceSlot, ArmorSlot? target = null)
        {
            if (!MeetsItemRequirements(item)) return false;

            var position = target ?? EquipTargetFor(item);
            sourceBag.Take(sourceSlot);
            if (EquippedArmor.TryGetValue(position, out var current))
                Inventory.TryAdd(current);
            EquippedArmor[position] = item;
            RecalculateStatsKeepResources();
            return true;
        }

        /// <summary>
        /// Puts a worn armor piece back into the inventory.
        /// Returns false if the inventory is full.
        /// </summary>
        public bool UnequipArmor(ArmorSlot slot)
        {
            if (!EquippedArmor.TryGetValue(slot, out var item)) return true;
            if (Inventory.TryAdd(item) > 0) return false;
            EquippedArmor.Remove(slot);
            RecalculateStatsKeepResources();
            return true;
        }

        /// <summary>Restores worn armor pieces from a save game.</summary>
        internal void RestoreEquippedArmor(IReadOnlyDictionary<ArmorSlot, ArmorItem> armor)
        {
            foreach (var (slot, item) in armor)
                EquippedArmor[slot] = item;
            RecalculateStats();
        }

        /// <summary>Equips an armor piece without upgrade check (e.g. for debug purposes).</summary>
        public void ForceEquipArmor(ArmorItem item)
        {
            EquippedArmor[EquipTargetFor(item)] = item;
            RecalculateStats();
        }

        /// <summary>Equips a weapon without class/level check (debug purposes).</summary>
        public void ForceEquipWeapon(WeaponItem weapon)
        {
            EquippedWeapon = weapon;
            _equippedByClass[ActiveClass] = weapon;
            RecalculateStats();
        }

        /// <summary>
        /// No-op — starter weapon assignment is now handled inside SetActiveClass.
        /// Kept so call-sites in IdleGameEngine don't need to be changed.
        /// </summary>
        public void EquipStarterWeapon(CharacterClass cls) { }
    }
}
