// Merchant on /inventory: open by click, sell by right-click or drag and drop
using Models;

namespace WoWClassicIdle.Components.Pages;

public partial class Inventory
{
    // While the trade window is open, right-clicking a bag item sells it
    private bool _merchantOpen;
    private string? _merchantMessage;
    private bool _merchantMessageFailed;

    // Speech bubble of the merchant; counter as @key so the animation restarts on every line
    private string? _speech;
    private int _speechKey;

    // Buy/sell effect (flying coins or item); counter as @key as above
    private MerchantFx? _merchantFx;
    private int _fxKey;

    private void OpenMerchant()
    {
        _merchantOpen = true;
        Say(MerchantRegistry.GreetingLines);
    }

    private void CloseMerchant()
    {
        _merchantOpen = false;
        _merchantMessage = null;
        _merchantFx = null;
        Say(MerchantRegistry.FarewellLines);
    }

    private void Say(IReadOnlyList<string> lines)
    {
        // Never the same line twice in a row
        var options = lines.Where(l => l != _speech).ToList();
        _speech = options[Random.Shared.Next(options.Count)];
        _speechKey++;
    }

    private void ShowMerchantMessage(string text, bool failed) =>
        (_merchantMessage, _merchantMessageFailed) = (text, failed);

    private void ShowFx(MerchantFxKind kind, Item item, long copper)
    {
        _merchantFx = new MerchantFx(kind, item.Id, item.IconUrl, copper);
        _fxKey++;
    }

    private void BuyFromMerchant(MerchantOffer offer)
    {
        var error = _engine.BuyFromMerchant(offer);
        if (error is not null)
        {
            ShowMerchantMessage(error, true);
            if (_engine.Player.Copper < offer.Price) Say(MerchantRegistry.RefuseLines);
            return;
        }
        ShowMerchantMessage($"Bought: {offer.Item.Name} for {Money.Format(offer.Price)}", false);
        ShowFx(MerchantFxKind.Buy, offer.Item, offer.Price);
        Say(MerchantRegistry.BuyLines);
    }

    private void BuybackFromMerchant(int index)
    {
        var (entry, error) = _engine.BuybackFromMerchant(index);
        if (entry is null)
        {
            ShowMerchantMessage(error ?? "That item is no longer available.", true);
            if (_engine.MerchantBuyback.ElementAtOrDefault(index) is { } wanted && _engine.Player.Copper < wanted.Price)
                Say(MerchantRegistry.RefuseLines);
            return;
        }
        var count = entry.Stack.Count > 1 ? $" x{entry.Stack.Count}" : "";
        ShowMerchantMessage($"Bought back: {entry.Stack.Item.Name}{count} for {Money.Format(entry.Price)}", false);
        ShowFx(MerchantFxKind.Buyback, entry.Stack.Item, entry.Price);
        Say(MerchantRegistry.BuybackLines);
    }

    private void DropOnMerchant()
    {
        if (_dragSource is not { } src) return;
        _dragSource = null;
        SellSlot(src.bagIndex, src.slotIndex);
    }

    private void SellSlot(int bagIndex, int slotIndex)
    {
        if (!_merchantOpen || ContainerAt(bagIndex) is not { } bag) return;
        if (_engine.SellToMerchant(bag, slotIndex) is not { } sold) return;
        var (stack, earned) = sold;
        var count = stack.Count > 1 ? $" x{stack.Count}" : "";
        ShowMerchantMessage($"Sold: {stack.Item.Name}{count} for {Money.Format(Math.Max(1, earned))}", false);
        ShowFx(MerchantFxKind.Sell, stack.Item, earned);
        Say(MerchantRegistry.SellLines);
    }
}

public enum MerchantFxKind { Buy, Sell, Buyback }

/// <summary>One-time trade effect in the merchant window: item icon and amount.</summary>
public record MerchantFx(MerchantFxKind Kind, int ItemId, string IconUrl, long Copper);
