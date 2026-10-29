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

        /// <summary>
        /// Equips an armor piece if it is an upgrade (or the slot was empty).
        /// Only for auto-equip (debug simulator) – in the normal game the player equips manually.
        /// Items whose level requirement is not met are not equipped.
        /// Returns true if equipped; the displaced piece goes into <paramref name="displaced"/>.
        /// </summary>
        public bool EquipArmorIfUpgrade(ArmorItem item, out ArmorItem? displaced)
        {
            if (!MeetsItemRequirements(item))
            {
                displaced = null;
                return false;
            }

            EquippedArmor.TryGetValue(item.Slot, out var current);
            if (current is not null && SumPrimary(item.Bonuses) <= SumPrimary(current.Bonuses))
            {
                displaced = null;
                return false;
            }
            displaced = current;
            EquippedArmor[item.Slot] = item;
            RecalculateStats();
            return true;
        }

        /// <summary>
        /// Equips an armor piece from the given bag slot.
        /// The previously worn piece (if any) goes back into the inventory.
        /// Returns false if the required level is not reached – the item stays in the bag.
        /// </summary>
        public bool EquipArmor(ArmorItem item, Bag sourceBag, int sourceSlot)
        {
            if (!MeetsItemRequirements(item)) return false;

            sourceBag.Take(sourceSlot);
            if (EquippedArmor.TryGetValue(item.Slot, out var current))
                Inventory.TryAdd(current);
            EquippedArmor[item.Slot] = item;
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
            EquippedArmor[item.Slot] = item;
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
        /// Equips a weapon if it is an upgrade (higher avg. damage / speed) or no slot is occupied.
        /// Only for auto-equip (debug simulator). Level/class requirements are checked.
        /// Returns true if equipped; the displaced weapon goes into <paramref name="displaced"/>.
        /// </summary>
        public bool EquipWeaponIfUpgrade(WeaponItem item, out WeaponItem? displaced)
        {
            if (!MeetsItemRequirements(item))
            {
                displaced = null;
                return false;
            }

            double newDps = (item.MinDamage + item.MaxDamage) / 2.0 / item.Speed;
            if (EquippedWeapon is not null)
            {
                double curDps = (EquippedWeapon.MinDamage + EquippedWeapon.MaxDamage) / 2.0 / EquippedWeapon.Speed;
                if (newDps <= curDps)
                {
                    displaced = null;
                    return false;
                }
            }

            displaced = EquippedWeapon;
            EquippedWeapon = item;
            _equippedByClass[ActiveClass] = item;
            RecalculateStats();
            return true;
        }

        private static int SumPrimary(PrimaryStats p) =>
            p.Strength + p.Agility + p.Stamina + p.Intelligence + p.Spirit;

        /// <summary>
        /// No-op — starter weapon assignment is now handled inside SetActiveClass.
        /// Kept so call-sites in IdleGameEngine don't need to be changed.
        /// </summary>
        public void EquipStarterWeapon(CharacterClass cls) { }
    }
}
