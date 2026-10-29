// Models/IdleGameEngine.Merchant.cs
using Models;

/// <summary>Merchant: selling from the inventory and buying offers with real-time stock.</summary>
public partial class IdleGameEngine
{
    private sealed class MerchantStockState
    {
        public int Count;
        // Time of the next full restock; null = stock is full
        public DateTime? RestockAtUtc;
    }

    private readonly Dictionary<int, MerchantStockState> _merchantStock = new();

    /// <summary>How many sold stacks the merchant keeps for buyback.</summary>
    public const int MerchantBuybackSlots = 6;

    // Most recently sold stacks, newest first – only for the current session (like in the original)
    private readonly List<MerchantBuybackEntry> _buyback = new();

    /// <summary>Buyback list (newest first), copy for the UI.</summary>
    public IReadOnlyList<MerchantBuybackEntry> MerchantBuyback
    {
        get { lock (_simLock) return _buyback.ToList(); }
    }

    /// <summary>Current stock of an offer (fully restocks after the real-time period expires).</summary>
    public int MerchantStockOf(MerchantOffer offer)
    {
        lock (_simLock) return StockState(offer).Count;
    }

    /// <summary>Remaining time until the next restock; null if the stock is full.</summary>
    public TimeSpan? MerchantRestockRemaining(MerchantOffer offer)
    {
        lock (_simLock)
        {
            var state = StockState(offer);
            return state.RestockAtUtc is { } at ? at - DateTime.UtcNow : null;
        }
    }

    /// <summary>Buys one piece. Returns an English error message on failure, otherwise null.</summary>
    public string? BuyFromMerchant(MerchantOffer offer)
    {
        lock (_simLock)
        {
            var state = StockState(offer);
            if (state.Count <= 0) return "Sold out.";
            if (Player.Copper < offer.Price) return "You don't have enough money.";
            if (_sharedInventory.TryAdd(offer.Item) > 0) return "Your bags are full.";

            Player.Copper -= offer.Price;
            state.Count--;
            state.RestockAtUtc ??= DateTime.UtcNow + offer.RestockInterval;
            return null;
        }
    }

    /// <summary>Sells a whole stack to the merchant. Returns the sold stack and the proceeds.</summary>
    public (ItemStack Stack, long Earned)? SellToMerchant(Bag bag, int index)
    {
        lock (_simLock)
        {
            if (bag.Take(index) is not { } stack) return null;
            long earned = stack.Item.SellPrice * stack.Count;
            Player.Copper += earned;
            TotalItemsSold += stack.Count;

            _buyback.Insert(0, new MerchantBuybackEntry(stack, earned));
            if (_buyback.Count > MerchantBuybackSlots)
                _buyback.RemoveAt(_buyback.Count - 1);
            return (stack, earned);
        }
    }

    /// <summary>
    /// Buys back a sold stack at its selling price.
    /// Returns an English error message on failure, otherwise the bought-back entry.
    /// </summary>
    public (MerchantBuybackEntry? Entry, string? Error) BuybackFromMerchant(int index)
    {
        lock (_simLock)
        {
            if (index < 0 || index >= _buyback.Count) return (null, "That item is no longer available.");
            var entry = _buyback[index];
            if (Player.Copper < entry.Price) return (null, "You don't have enough money.");

            var item = entry.Stack.Item;
            // Non-stackable items need a free slot; stacks are rolled back if there is not enough space
            if (!item.IsStackable && _sharedInventory.FreeSlots == 0) return (null, "Your bags are full.");
            int leftover = _sharedInventory.TryAdd(item, entry.Stack.Count);
            if (leftover > 0)
            {
                _sharedInventory.Remove(item.Id, entry.Stack.Count - leftover);
                return (null, "Your bags are full.");
            }

            Player.Copper -= entry.Price;
            _buyback.RemoveAt(index);
            return (entry, null);
        }
    }

    // Returns the stock state and applies a due restock (call only under _simLock)
    private MerchantStockState StockState(MerchantOffer offer)
    {
        if (!_merchantStock.TryGetValue(offer.ItemId, out var state))
            _merchantStock[offer.ItemId] = state = new MerchantStockState { Count = offer.MaxStock };
        if (state.RestockAtUtc is { } at && DateTime.UtcNow >= at)
        {
            state.Count = offer.MaxStock;
            state.RestockAtUtc = null;
        }
        return state;
    }

    internal List<MerchantStockSaveData> BuildMerchantSaveData()
    {
        lock (_simLock)
        {
            return _merchantStock
                .Select(kv => new MerchantStockSaveData { ItemId = kv.Key, Count = kv.Value.Count, RestockAtUtc = kv.Value.RestockAtUtc })
                .ToList();
        }
    }

    private void RestoreMerchantStock(SaveData save)
    {
        _merchantStock.Clear();
        _buyback.Clear();
        foreach (var entry in save.MerchantStock ?? [])
        {
            var offer = MerchantRegistry.Offers.FirstOrDefault(o => o.ItemId == entry.ItemId);
            if (offer is null) continue;
            _merchantStock[entry.ItemId] = new MerchantStockState
            {
                Count = Math.Clamp(entry.Count, 0, offer.MaxStock),
                RestockAtUtc = entry.RestockAtUtc,
            };
        }
    }
}

/// <summary>A stack sold to the merchant with buyback price (= sale proceeds).</summary>
public record MerchantBuybackEntry(ItemStack Stack, long Price);
