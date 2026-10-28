// Logik der Seite /inventory (Markup in Inventory.razor, Handelsfenster in Components/MerchantWindow.razor)
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

    // Bedienhinweise im WoW-Tooltip (statt schwebendem Systemtooltip)
    private const string UnequipHint = "Right-click to unequip";
    private const string EquipHint   = "Right-click or drag to equip";
    private const string SellHint    = "Right-click to sell";
    private const string UseHint     = "Right-click to use";

    private PrimaryStats Primary => _engine.Player.Stats.Primary;

    // Hintergrund des Kopfbereichs: aktuelle Zone, sonst Startgebiet der Rasse
    private string HeaderTheme =>
        _engine.CurrentCombatZone?.Theme ?? CombatZoneRegistry.StartZoneFor(_engine.Race)?.Theme ?? "grassland";

    private string ResourceName => ShapeshiftForms.ResourceName(_engine.Player.Resource);

    /// <summary>Anteil als Prozentwert, z. B. 0.05 → "5.0%".</summary>
    private static string FormatPercent(double fraction) =>
        (fraction * 100).ToString("0.0") + "%";

    /// <summary>Wie <see cref="FormatPercent"/>, aber mit Vorzeichen für Bonuswerte ("+0.0%").</summary>
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
        // HP-Regeneration gilt für alle Klassen, Mana-Regeneration nur für Mana-Nutzer
        string hpPart = $"+{StatScaling.HpRegenPerSpiritStep} Health per 5s for every {StatScaling.SpiritPerHpRegen} points of Spirit.";
        return StatScaling.UsesMana(cls)
            ? $"+{StatScaling.ManaRegenPerSpiritStep} Mana per 5s for every {StatScaling.SpiritPerManaRegen} points of Spirit. {hpPart}"
            : hpPart;
    }

    private static string SlotIconUrl(string icon) =>
        $"https://wow.zamimg.com/images/wow/icons/large/inventoryslot_{icon}.jpg";

    private static readonly string EmptyBagSlotIconUrl = SlotIconUrl("bag");

    // Erklärtext zu Taschenplätzen – aus den Konstanten erzeugt, damit er bei Balancing-Änderungen stimmt
    private static readonly string BagUnlockHint =
        $"Bag slots unlock at levels {string.Join(", ", Models.Inventory.BagUnlockLevels[..^1])} and {Models.Inventory.BagUnlockLevels[^1]}. " +
        $"Every bag has {Models.Inventory.BaseBagSlotSize} slots, plus one more for every 5 character levels.";

    // ─── Paperdoll-Slots ───

    /// <summary>Das in einem Paperdoll-Slot getragene Rüstungsteil (null wenn Slot leer oder nicht ausrüstbar).</summary>
    private ArmorItem? EquippedIn(EquipSlot slot) =>
        slot.Armor is { } armor && _engine.Player.EquippedArmor.TryGetValue(armor, out var item)
            ? item
            : null;

    /// <summary>Im Slot des Bag-Items getragenes Teil – Vergleichsziel für den Tooltip.</summary>
    private ArmorItem? EquippedArmorFor(ArmorItem item) =>
        _engine.Player.EquippedArmor.TryGetValue(item.Slot, out var equipped) ? equipped : null;

    private static string SlotImage(EquipSlot slot, ArmorItem? gear) =>
        gear?.IconUrl ?? SlotIconUrl(slot.Icon);

    private static string SlotCss(ArmorItem? gear) =>
        gear is not null ? "equipped-item" : "";

    private static string? SlotHint(EquipSlot slot, ArmorItem? gear) =>
        gear is not null
            ? null // ausgerüstete Teile haben einen WoW-Tooltip – kein schwebender Systemtooltip
            : slot.Armor is null ? slot.Name : $"{slot.Name} (drag item here)";

    private static string SlotQuality(ArmorItem? gear) =>
        gear is not null ? ItemQualityColors.Hex(gear.Quality) : "#4a3d22";

    /// <summary>Nur Slots mit Rüstungsplatz nehmen gezogene Items an.</summary>
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

    /// <summary>Rechtsklick auf einen Taschenplatz: bei offenem Händler verkaufen, sonst anlegen bzw. benutzen.</summary>
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

    // Browser-Kontextmenü nur dort zulassen, wo Rechtsklick nichts bewirkt
    private bool PreventContextMenu(Item item) =>
        _merchantOpen || item is not TradeItem || item is ConsumableItem { Kind: ConsumableKind.Costume };

    private string? TradeHint(TradeItem item) =>
        _merchantOpen ? SellHint
        : item is ConsumableItem { Kind: ConsumableKind.Costume } ? UseHint
        : null;

    // ─── Stapel teilen (Shift-Linksklick) ───
    private (int bagIndex, int slotIndex)? _split;
    private int _splitAmount = 1;
    private string? _splitError;

    private string? _useMessage;
    private bool _useFailed;

    // Rechtsklick: manuell benutzbare Verbrauchsgüter (Verkleidung) anwenden, Ergebnis als Meldung anzeigen
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

    // Taschenplätze, die bereits belegt waren – neu hinzugekommene werden automatisch geöffnet
    private readonly HashSet<int> _knownBags = new();

    protected override void OnInitialized()
    {
        _engine.TickUpdated += OnTick;
        // Backpack und alle vorhandenen Taschen beim Laden öffnen
        OpenContainers.Add(BackpackIndex);
        SyncBags();
    }

    /// <summary>
    /// Öffnet Taschen, die seit dem letzten Render freigeschaltet wurden (Level-Up, Charakterwechsel),
    /// und vergisst weggefallene Plätze, damit sie beim erneuten Freischalten wieder aufgehen.
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
