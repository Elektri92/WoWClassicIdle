// Models/Player.Equipment.cs
namespace Models
{
    // Neue Systeme als eigene Partial-Datei anlegen; Dateien möglichst < 300 Zeilen halten.
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
        /// Gibt false zurück wenn die Level-/Klassen-Voraussetzungen nicht erfüllt sind – das Item bleibt im Beutel.
        /// </summary>
        public bool EquipWeapon(WeaponItem weapon, Bag sourceBag, int sourceSlot)
        {
            if (!MeetsItemRequirements(weapon)) return false;

            sourceBag.Take(sourceSlot);
            if (EquippedWeapon is not null)
                Inventory.TryAdd(EquippedWeapon);
            EquippedWeapon = weapon;
            _equippedByClass[ActiveClass] = weapon;
            RecalculateStats();
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
            RecalculateStats();
            return true;
        }

        /// <summary>
        /// Legt ein Rüstungsteil an wenn es eine Verbesserung ist (oder der Slot leer war).
        /// Nur für Auto-Equip (Debug-Simulator) – im normalen Spiel wird selbst angelegt.
        /// Items deren Levelvoraussetzung nicht erfüllt ist werden nicht angelegt.
        /// Gibt true zurück wenn angelegt wurde; das verdrängte Teil kommt in <paramref name="displaced"/>.
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
        /// Rüstet ein Rüstungsteil aus dem gegebenen Taschen-Slot aus.
        /// Das bisher getragene Teil (falls vorhanden) wandert zurück ins Inventar.
        /// Gibt false zurück wenn das erforderliche Level nicht erreicht ist – das Item bleibt im Beutel.
        /// </summary>
        public bool EquipArmor(ArmorItem item, Bag sourceBag, int sourceSlot)
        {
            if (!MeetsItemRequirements(item)) return false;

            sourceBag.Take(sourceSlot);
            if (EquippedArmor.TryGetValue(item.Slot, out var current))
                Inventory.TryAdd(current);
            EquippedArmor[item.Slot] = item;
            RecalculateStats();
            return true;
        }

        /// <summary>
        /// Legt ein getragenes Rüstungsteil ins Inventar zurück.
        /// Gibt false zurück wenn das Inventar voll ist.
        /// </summary>
        public bool UnequipArmor(ArmorSlot slot)
        {
            if (!EquippedArmor.TryGetValue(slot, out var item)) return true;
            if (Inventory.TryAdd(item) > 0) return false;
            EquippedArmor.Remove(slot);
            RecalculateStats();
            return true;
        }

        /// <summary>Stellt getragene Rüstungsteile aus einem Spielstand wieder her.</summary>
        internal void RestoreEquippedArmor(IReadOnlyDictionary<ArmorSlot, ArmorItem> armor)
        {
            foreach (var (slot, item) in armor)
                EquippedArmor[slot] = item;
            RecalculateStats();
        }

        /// <summary>Legt ein Rüstungsteil ohne Upgrade-Prüfung an (z. B. für Debug-Zwecke).</summary>
        public void ForceEquipArmor(ArmorItem item)
        {
            EquippedArmor[item.Slot] = item;
            RecalculateStats();
        }

        /// <summary>Legt eine Waffe ohne Klassen-/Level-Prüfung an (Debug-Zwecke).</summary>
        public void ForceEquipWeapon(WeaponItem weapon)
        {
            EquippedWeapon = weapon;
            _equippedByClass[ActiveClass] = weapon;
            RecalculateStats();
        }

        /// <summary>
        /// Legt eine Waffe an wenn sie eine Verbesserung ist (höherer Ø-Schaden / Speed) oder kein Slot belegt ist.
        /// Nur für Auto-Equip (Debug-Simulator). Level-/Klassen-Voraussetzungen werden geprüft.
        /// Gibt true zurück wenn angelegt wurde; die verdrängte Waffe kommt in <paramref name="displaced"/>.
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
