// Logic of the /inventory page (markup in Inventory.razor, trade window in Components/MerchantWindow.razor)
using Microsoft.AspNetCore.Components.Web;
using Models;

namespace WoWClassicIdle.Components.Pages;

public partial class Inventory
{
    private record EquipSlot(string Name, string Icon, ArmorSlot? Armor = null);

    private static readonly EquipSlot[] LeftSlots =
    {
        new(ArmorSlots.Label(ArmorSlot.Head), "head", ArmorSlot.Head),
        new(ArmorSlots.Label(ArmorSlot.Neck), "neck", ArmorSlot.Neck),
        new("Shoulder", "shoulder"),
        new(ArmorSlots.Label(ArmorSlot.Cape), "chest", ArmorSlot.Cape),
        new(ArmorSlots.Label(ArmorSlot.Chest), "chest", ArmorSlot.Chest),
        new("Shirt", "shirt"), new("Tabard", "tabard"),
        new(ArmorSlots.Label(ArmorSlot.Wrists), "wrists", ArmorSlot.Wrists),
    };

    private static readonly EquipSlot[] RightSlots =
    {
        new(ArmorSlots.Label(ArmorSlot.Hands), "hands", ArmorSlot.Hands),
        new(ArmorSlots.Label(ArmorSlot.Belt), "waist", ArmorSlot.Belt),
        new(ArmorSlots.Label(ArmorSlot.Legs), "legs", ArmorSlot.Legs),
        new(ArmorSlots.Label(ArmorSlot.Feet), "feet", ArmorSlot.Feet),
        new(ArmorSlots.Label(ArmorSlot.Ring), "finger", ArmorSlot.Ring),
        new("Finger", "finger"),
        new(ArmorSlots.Label(ArmorSlot.Trinket), "trinket", ArmorSlot.Trinket),
        new("Trinket", "trinket"),
    };

    private static readonly EquipSlot WeaponSlot = new("Weapon", "mainhand");

    // Usage hints in the WoW tooltip (instead of a floating system tooltip)
    private const string UnequipHint = "Right-click to unequip";
    private const string EquipHint   = "Right-click or drag to equip";
    private const string SellHint    = "Right-click to sell";
    private const string UseHint     = "Right-click to use";

    private PrimaryStats Primary => _engine.Player.Stats.Primary;

    // Background of the header: current zone, otherwise the starting zone of the race
    private string HeaderTheme =>
        _engine.CurrentCombatZone?.Theme ?? CombatZoneRegistry.StartZoneFor(_engine.Race)?.Theme ?? "grassland";

    private string ResourceName => ShapeshiftForms.ResourceName(_engine.Player.Resource);

    /// <summary>Share as a percentage, e.g. 0.05 → "5.0%".</summary>
    private static string FormatPercent(double fraction) =>
        (fraction * 100).ToString("0.0") + "%";

    /// <summary>Like <see cref="FormatPercent"/>, but with a sign for bonus values ("+0.0%").</summary>
    private static string FormatBonusPercent(double fraction) =>
        (fraction >= 0 ? "+" : "") + FormatPercent(fraction);

    private static string StaminaTooltip() =>
        $"+{StatScaling.HpPerStamina} maximum Health per point.";

    private static string StrengthTooltip(CharacterClass cls) => cls switch
    {
        CharacterClass.Warrior or CharacterClass.Paladin or CharacterClass.Shaman or CharacterClass.Druid
            => "+2 Attack Power per point.",
        CharacterClass.Rogue or CharacterClass.Hunter
            => "+1 Attack Power per point.",
        _ // Mage, Priest, Warlock
            => "+1 Attack Power per point. Casters rarely benefit from Attack Power.",
    };

    private static bool IsStrengthUseless(CharacterClass cls) => cls is
        CharacterClass.Mage or CharacterClass.Priest or CharacterClass.Warlock;

    private static string AgilityTooltip(CharacterClass cls) => cls switch
    {
        CharacterClass.Rogue or CharacterClass.Hunter
            => "+1 Attack Power per point.",
        _
            => "No benefit for this class.",
    };

    private static bool IsAgilityUseless(CharacterClass cls) =>
        cls is not (CharacterClass.Rogue or CharacterClass.Hunter);

    private static string IntellectTooltip(CharacterClass cls) =>
        StatScaling.UsesMana(cls)
            ? $"+{StatScaling.ManaPerIntelligence} maximum Mana per point."
            : "No benefit for this class (does not use Mana).";

    private static string SpiritTooltip(CharacterClass cls)
    {
        // HP regeneration applies to all classes, mana regeneration only to mana users
        string hpPart = $"+{StatScaling.HpRegenPerSpiritStep} Health per 5s for every {StatScaling.SpiritPerHpRegen} points of Spirit.";
        return StatScaling.UsesMana(cls)
            ? $"+{StatScaling.ManaRegenPerSpiritStep} Mana per 5s for every {StatScaling.SpiritPerManaRegen} points of Spirit. {hpPart}"
            : hpPart;
    }

    private static string SlotIconUrl(string icon) =>
        $"https://wow.zamimg.com/images/wow/icons/large/inventoryslot_{icon}.jpg";

    private static readonly string EmptyBagSlotIconUrl = SlotIconUrl("bag");

    // Explanation text for bag slots – generated from the constants so it stays correct after balancing changes
    private static readonly string BagUnlockHint =
        $"Bag slots unlock at levels {string.Join(", ", Models.Inventory.BagUnlockLevels[..^1])} and {Models.Inventory.BagUnlockLevels[^1]}. " +
        $"Every bag has {Models.Inventory.BaseBagSlotSize} slots, plus one more for every 5 character levels.";

    // ─── Paperdoll-Slots ───

    /// <summary>The armor piece worn in a paperdoll slot (null if the slot is empty or not equippable).</summary>
    private ArmorItem? EquippedIn(EquipSlot slot) =>
        slot.Armor is { } armor && _engine.Player.EquippedArmor.TryGetValue(armor, out var item)
            ? item
            : null;

    /// <summary>Piece worn in the bag item's slot – comparison target for the tooltip.</summary>
    private ArmorItem? EquippedArmorFor(ArmorItem item) =>
        _engine.Player.EquippedArmor.TryGetValue(item.Slot, out var equipped) ? equipped : null;

    private static string SlotImage(EquipSlot slot, ArmorItem? gear) =>
        gear?.IconUrl ?? SlotIconUrl(slot.Icon);

    private static string SlotCss(ArmorItem? gear) =>
        gear is not null ? "equipped-item" : "";

    private static string? SlotHint(EquipSlot slot, ArmorItem? gear) =>
        gear is not null
            ? null // equipped pieces have a WoW tooltip – no floating system tooltip
            : slot.Armor is null ? slot.Name : $"{slot.Name} (drag item here)";

    private static string SlotQuality(ArmorItem? gear) =>
        gear is not null ? ItemQualityColors.Hex(gear.Quality) : "#4a3d22";

    /// <summary>Only slots with an armor slot accept dragged items.</summary>
    private static bool CanDropArmor(EquipSlot slot) => slot.Armor is not null;

    // -1 = Backpack, 0..3 = equipped bag slots
    private const int BackpackIndex = -1;
    private readonly HashSet<int> OpenContainers = new();
    private (int bagIndex, int slotIndex)? _dragSource;

    private Bag? ContainerAt(int index) =>
        index == BackpackIndex ? _engine.Player.Inventory.Backpack : _engine.Player.Inventory.Bags[index];

    private void StartDrag(int bagIndex, int slotIndex) =>
        _dragSource = (bagIndex, slotIndex);

    private void ClearDrag() =>
        _dragSource = null;

    /// <summary>Drop on a bag slot: rearranges the dragged stack (see <see cref="Models.Inventory.MoveStack"/>).</summary>
    private void DropOnBagSlot(int bagIndex, int slotIndex)
    {
        if (_dragSource is not { } src) return;
        _dragSource = null;
        if (ContainerAt(src.bagIndex) is { } from && ContainerAt(bagIndex) is { } to)
            _engine.MoveInventoryStack(from, src.slotIndex, to, slotIndex);
    }

    /// <summary>Right-click on a bag slot: sell when the merchant is open, otherwise equip or use.</summary>
    private void OnSlotRightClick(int bagIndex, int slotIndex)
    {
        if (_merchantOpen)
        {
            SellSlot(bagIndex, slotIndex);
            return;
        }
        if (ContainerAt(bagIndex)?.Slots[slotIndex]?.Item is TradeItem tradeItem)
            UseConsumable(tradeItem);
        else
            TryEquipItem(bagIndex, slotIndex);
    }

    // Only allow the browser context menu where right-clicking does nothing
    private bool PreventContextMenu(Item item) =>
        _merchantOpen || item is not TradeItem || item is ConsumableItem { Kind: ConsumableKind.Costume };

    private string? TradeHint(TradeItem item) =>
        _merchantOpen ? SellHint
        : item is ConsumableItem { Kind: ConsumableKind.Costume } ? UseHint
        : null;

    // ─── Split stack (shift-left-click) ───
    private (int bagIndex, int slotIndex)? _split;
    private int _splitAmount = 1;
    private string? _splitError;

    private string? _useMessage;
    private bool _useFailed;

    // Right-click: apply manually usable consumables (disguise), show the result as a message
    private void UseConsumable(TradeItem item)
    {
        if (item is not ConsumableItem { Kind: ConsumableKind.Costume } consumable) return;
        var error = _engine.UseConsumableManually(consumable);
        _useFailed = error is not null;
        _useMessage = error is not null
            ? $"{consumable.Name}: {error}"
            : $"{consumable.Name}: you turn into a {Costumes.Name(_engine.Player.Costume)} for {ConsumableItem.FormatDuration(consumable.DurationSeconds)} (combat time).";
    }

    private void OnSlotClick(MouseEventArgs e, int bagIndex, int slotIndex)
    {
        if (!e.ShiftKey) return;
        var stack = ContainerAt(bagIndex)?.Slots[slotIndex];
        if (stack is null || !stack.Item.IsStackable || stack.Count < 2) return;
        _split = (bagIndex, slotIndex);
        _splitAmount = 1;
        _splitError = null;
    }

    private void ChangeSplit(int delta) => _splitAmount += delta;

    private void OnSplitKey(KeyboardEventArgs e)
    {
        if (e.Key == "Enter") ConfirmSplit();
        else if (e.Key == "Escape") CancelSplit();
    }

    private void ConfirmSplit()
    {
        if (_split is not { } split || ContainerAt(split.bagIndex) is not { } bag || bag.Slots[split.slotIndex] is not { } stack)
        {
            CancelSplit();
            return;
        }
        int amount = Math.Clamp(_splitAmount, 1, stack.Count - 1);
        if (!_engine.SplitInventoryStack(bag, split.slotIndex, amount))
        {
            _splitError = "No free bag slot.";
            return;
        }
        CancelSplit();
    }

    private void CancelSplit()
    {
        _split = null;
        _splitError = null;
    }

    private void ToggleContainer(int index)
    {
        if (!OpenContainers.Remove(index))
            OpenContainers.Add(index);
    }

    // Bag slots that were already occupied – newly added ones are opened automatically
    private readonly HashSet<int> _knownBags = new();

    protected override void OnInitialized()
    {
        _engine.TickUpdated += OnTick;
        // Open the backpack and all existing bags on load
        OpenContainers.Add(BackpackIndex);
        SyncBags();
    }

    /// <summary>
    /// Opens bags unlocked since the last render (level-up, character switch),
    /// and forgets removed slots so they open again when unlocked again.
    /// </summary>
    private void SyncBags()
    {
        var bags = _engine.Player.Inventory.Bags;
        for (int i = 0; i < Models.Inventory.BagSlotCount; i++)
        {
            if (bags[i] is null)
            {
                _knownBags.Remove(i);
                OpenContainers.Remove(i);
            }
            else if (_knownBags.Add(i))
            {
                OpenContainers.Add(i);
            }
        }
    }

    private void OnTick() => _ = InvokeAsync(() =>
    {
        SyncBags();
        StateHasChanged();
    });

    public void Dispose()
    {
        _engine.TickUpdated -= OnTick;
    }
}
