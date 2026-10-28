// Models/IdleGameEngine.Merchant.cs
using Models;

/// <summary>Händler: Verkaufen aus dem Inventar und Kauf von Angeboten mit Echtzeit-Vorrat.</summary>
public partial class IdleGameEngine
{
    private sealed class MerchantStockState
    {
        public int Count;
        // Zeitpunkt der nächsten vollen Auffüllung; null = Vorrat ist voll
        public DateTime? RestockAtUtc;
    }

    private readonly Dictionary<int, MerchantStockState> _merchantStock = new();

    /// <summary>Wie viele verkaufte Stapel der Händler zum Rückkauf bereithält.</summary>
    public const int MerchantBuybackSlots = 6;

    // Zuletzt verkaufte Stapel, neueste zuerst – nur für die laufende Sitzung (wie im Original)
    private readonly List<MerchantBuybackEntry> _buyback = new();

    /// <summary>Rückkaufliste (neueste zuerst), Kopie für die UI.</summary>
    public IReadOnlyList<MerchantBuybackEntry> MerchantBuyback
    {
        get { lock (_simLock) return _buyback.ToList(); }
    }

    /// <summary>Aktueller Vorrat eines Angebots (füllt sich nach Ablauf der Echtzeit-Frist komplett auf).</summary>
    public int MerchantStockOf(MerchantOffer offer)
    {
        lock (_simLock) return StockState(offer).Count;
    }

    /// <summary>Restzeit bis zur nächsten Auffüllung; null wenn der Vorrat voll ist.</summary>
    public TimeSpan? MerchantRestockRemaining(MerchantOffer offer)
    {
        lock (_simLock)
        {
            var state = StockState(offer);
            return state.RestockAtUtc is { } at ? at - DateTime.UtcNow : null;
        }
    }

    /// <summary>Kauft ein Stück. Gibt bei Misserfolg eine englische Fehlermeldung zurück, sonst null.</summary>
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

    /// <summary>Verkauft einen ganzen Stapel an den Händler. Gibt den verkauften Stapel und den Erlös zurück.</summary>
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
    /// Kauft einen verkauften Stapel zum Verkaufspreis zurück.
    /// Gibt bei Misserfolg eine englische Fehlermeldung zurück, sonst den zurückgekauften Eintrag.
    /// </summary>
    public (MerchantBuybackEntry? Entry, string? Error) BuybackFromMerchant(int index)
    {
        lock (_simLock)
        {
            if (index < 0 || index >= _buyback.Count) return (null, "That item is no longer available.");
            var entry = _buyback[index];
            if (Player.Copper < entry.Price) return (null, "You don't have enough money.");

            var item = entry.Stack.Item;
            // Nicht stapelbare Items brauchen einen freien Platz; Stapel werden bei Platzmangel zurückgerollt
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

    // Liefert den Vorratszustand und wendet eine fällige Auffüllung an (Aufruf nur unter _simLock)
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

/// <summary>Ein an den Händler verkaufter Stapel mit Rückkaufpreis (= Verkaufserlös).</summary>
public record MerchantBuybackEntry(ItemStack Stack, long Price);
