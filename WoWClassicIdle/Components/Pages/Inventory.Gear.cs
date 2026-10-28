// Ausrüsten/Ablegen auf /inventory inkl. Wechsel-Animation (Paperdoll-Slot + Stat-Differenzen)
using Models;

namespace WoWClassicIdle.Components.Pages;

public partial class Inventory
{
    private const string WeaponSlotKey = "Weapon";

    // Letzter Gear-Wechsel; Zähler als @key, damit die Animation bei jedem Wechsel neu startet
    private int _swapKey;
    private string? _swapSlot;
    private bool _swapEquipped;
    private string? _swapOldIcon;
    private Dictionary<string, double> _statDeltas = new();

    private static string SlotKey(ArmorSlot slot) => slot.ToString();

    private Item? EquippedFor(string slotKey) =>
        slotKey == WeaponSlotKey
            ? _engine.Player.EquippedWeapon
            : Enum.TryParse<ArmorSlot>(slotKey, out var slot) && _engine.Player.EquippedArmor.TryGetValue(slot, out var armor)
                ? armor
                : null;

    // Werte der Übersicht, deren Änderung nach einem Wechsel kurz eingeblendet wird
    private Dictionary<string, double> SnapshotStats()
    {
        var stats = _engine.Player.Stats;
        return new()
        {
            ["Strength"]     = stats.Primary.Strength,
            ["Agility"]      = stats.Primary.Agility,
            ["Stamina"]      = stats.Primary.Stamina,
            ["Intellect"]    = stats.Primary.Intelligence,
            ["Spirit"]       = stats.Primary.Spirit,
            ["Health"]       = stats.MaxHp,
            ["Mana"]         = stats.MaxMana,
            ["ManaRegen"]    = stats.ManaRegenPer5,
            ["HpRegen"]      = stats.HpRegenPer5,
            ["AttackPower"]  = stats.AttackPower,
            ["Crit"]         = stats.CritChance * 100,
            ["AttackSpeed"]  = stats.AttackSpeedBonus * 100,
        };
    }

    /// <summary>Führt einen Wechsel aus und startet die Animation, falls sich der Slot tatsächlich geändert hat.</summary>
    private void Swap(string slotKey, Action change)
    {
        var before = EquippedFor(slotKey);
        var statsBefore = SnapshotStats();
        change();
        var after = EquippedFor(slotKey);
        if (ReferenceEquals(before, after)) return;

        _swapSlot = slotKey;
        _swapEquipped = after is not null;
        _swapOldIcon = before?.IconUrl;
        var statsAfter = SnapshotStats();
        _statDeltas = statsAfter
            .Where(kv => Math.Abs(kv.Value - statsBefore[kv.Key]) > 0.001)
            .ToDictionary(kv => kv.Key, kv => kv.Value - statsBefore[kv.Key]);
        _swapKey++;
    }

    private bool IsSwapping(string? slotKey) => slotKey is not null && slotKey == _swapSlot;

    // @key des Slots: wechselt nur beim animierten Slot, damit nur dieser neu gemountet wird
    private string SlotRenderKey(string? slotKey) =>
        IsSwapping(slotKey) ? $"{slotKey}-{_swapKey}" : slotKey ?? "";

    private string SwapCss(string? slotKey) =>
        !IsSwapping(slotKey) ? "" : _swapEquipped ? "swap-in" : "swap-out";

    private static string FormatDelta(string stat, double delta)
    {
        string sign = delta > 0 ? "+" : "−";
        double abs = Math.Abs(delta);
        return stat is "Crit" or "AttackSpeed" ? $"{sign}{abs:0.0}%" : $"{sign}{abs:0}";
    }

    private void DropOnWeaponSlot()
    {
        if (_dragSource is not { } src) return;
        _dragSource = null;
        var bag = ContainerAt(src.bagIndex);
        if (bag?.Slots[src.slotIndex]?.Item is WeaponItem weapon)
            Swap(WeaponSlotKey, () => _engine.Player.EquipWeapon(weapon, bag, src.slotIndex));
    }

    /// <summary>Legt ein auf einen Paperdoll-Slot gezogenes Rüstungsteil an.</summary>
    private void DropOnArmorSlot(ArmorSlot? armorSlot)
    {
        if (armorSlot is null || _dragSource is not { } src) return;
        _dragSource = null;
        var bag = ContainerAt(src.bagIndex);
        if (bag?.Slots[src.slotIndex]?.Item is ArmorItem armor && armor.Slot == armorSlot)
            Swap(SlotKey(armor.Slot), () => _engine.Player.EquipArmor(armor, bag, src.slotIndex));
    }

    private void TryEquipItem(int bagIndex, int slotIndex)
    {
        var bag = ContainerAt(bagIndex);
        if (bag is null) return;
        switch (bag.Slots[slotIndex]?.Item)
        {
            case WeaponItem weapon:
                Swap(WeaponSlotKey, () => _engine.Player.EquipWeapon(weapon, bag, slotIndex));
                break;
            case ArmorItem armor:
                Swap(SlotKey(armor.Slot), () => _engine.Player.EquipArmor(armor, bag, slotIndex));
                break;
        }
    }

    private void UnequipWeapon() =>
        Swap(WeaponSlotKey, () => _engine.Player.UnequipWeapon());

    /// <summary>Rechtsklick auf einen Paperdoll-Slot: getragenes Rüstungsteil zurück ins Inventar.</summary>
    private void UnequipArmorSlot(ArmorSlot? armorSlot)
    {
        if (armorSlot is null) return;
        Swap(SlotKey(armorSlot.Value), () => _engine.Player.UnequipArmor(armorSlot.Value));
    }
}
