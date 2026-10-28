// Models/MerchantRegistry.cs
namespace Models;

/// <summary>Ein Angebot des Händlers mit begrenztem Vorrat, der sich nach Echtzeit wieder auffüllt.</summary>
public record MerchantOffer(int ItemId, long Price, int MaxStock, TimeSpan RestockInterval)
{
    public Item Item => ConsumableRegistry.GetById(ItemId)
        ?? throw new InvalidOperationException($"Unknown merchant item {ItemId}");
}

/// <summary>Statische Händlerdaten (Gibzig, Goblin-Händler im Inventar).</summary>
public static class MerchantRegistry
{
    public const string Name     = "Gibzig";
    public const string Title    = "Goblin Merchant";
    public const string Greeting = "\"A deal's a deal, mon!\"";
    public const string PortraitUrl = "https://wow.zamimg.com/images/wow/icons/large/race_goblin_male.jpg";
    public const string BadgeUrl    = "https://wow.zamimg.com/images/wow/icons/large/inv_misc_head_goblin_01.jpg";

    // Sprechblasen-Sprüche je Anlass (zufällig gewählt, nie zweimal hintereinander derselbe)
    public static readonly IReadOnlyList<string> GreetingLines =
    [
        "Time is money, friend!",
        "Welcome, welcome! Everything's for sale!",
        "You got the gold? I got the goods!",
        "Ah, a customer! My favorite kind of person.",
        "Buying? Selling? Gibzig does both — at a profit!",
    ];

    public static readonly IReadOnlyList<string> FarewellLines =
    [
        "Come back when you're a little... richer!",
        "Don't let the door hit your coin purse!",
        "Leaving already? My prices only go up, you know!",
        "Time is money — and you just wasted mine!",
        "Pleasure doing business! Mostly mine.",
    ];

    public static readonly IReadOnlyList<string> BuyLines =
    [
        "A fine choice! No refunds.",
        "Ka-ching! Drink responsibly... or don't.",
        "Sold! That potion's barely expired.",
        "Excellent! Tell your friends. Rich friends.",
    ];

    public static readonly IReadOnlyList<string> SellLines =
    [
        "Hmm... I'll take it. Probably worthless.",
        "Junk? Treasure? Who cares — it's mine now!",
        "Here's your coin. Don't spend it all in one place!",
        "Another fine addition to my... collection.",
    ];

    public static readonly IReadOnlyList<string> BuybackLines =
    [
        "Changed your mind? Gibzig always profits!",
        "Back so soon? I kept it warm for you.",
        "Seller's remorse? Happens to the best of 'em.",
    ];

    public static readonly IReadOnlyList<string> RefuseLines =
    [
        "No gold, no goods, friend!",
        "What, you think I run a charity?",
        "Gibzig doesn't do credit!",
    ];

    // Minor Healing Potion (sonst nur per Alchemie): 5 Stück, volle Auffüllung 5 min nach dem ersten Kauf
    public static readonly IReadOnlyList<MerchantOffer> Offers =
    [
        new(9103, Price: 25, MaxStock: 5, RestockInterval: TimeSpan.FromMinutes(5)),
    ];
}
