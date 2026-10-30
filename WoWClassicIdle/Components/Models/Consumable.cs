// Models/Consumable.cs
namespace Models
{
    /// <summary>Kind of consumable – determines when and how it is used automatically in combat.</summary>
    public enum ConsumableKind
    {
        HealthPotion,   // restores health (at low health, own cooldown)
        ManaPotion,     // restores mana (at low mana, own cooldown)
        Elixir,         // stat buff (Alchemy), 3 min
        Flask,          // stat buff (Alchemy), 1 h, persists through death
        Food,           // stat buff (Cooking), 3 min
        Bandage,        // shortens the spirit healer wait by Amount %
        Explosive,      // Engineering: Amount damage to the enemy, own cooldown
        Costume,        // manual only: transforms the character model (Savory Deviate Delight), renews itself automatically
    }

    /// <summary>Disguise from Savory Deviate Delight (50/50).</summary>
    public enum Costume { None, Pirate, Ninja }

    public static class Costumes
    {
        public static string Name(Costume costume) => costume.ToString();

        /// <summary>Portrait of the disguise; null = none.</summary>
        public static string? PortraitUrl(Costume costume) => costume switch
        {
            Costume.Pirate => "https://wow.zamimg.com/images/wow/icons/large/inv_helmet_66.jpg",
            Costume.Ninja  => "https://wow.zamimg.com/images/wow/icons/large/ability_rogue_masterofsubtlety.jpg",
            _              => null,
        };
    }

    /// <summary>
    /// Consumable (category "Consumable"): stackable profession item that is used automatically from the inventory in combat while its auto-use toggle is on.
    /// <paramref name="Bonuses"/> = stat buff (elixir/flask/food) for <paramref name="DurationSeconds"/> combat seconds,
    /// <paramref name="Amount"/> = healing/mana or percent faster resurrection (bandage).
    /// </summary>
    public record ConsumableItem(
        int Id,
        string Name,
        string Icon,
        ConsumableKind Kind,
        PrimaryStats? Bonuses = null,
        int Amount = 0,
        double DurationSeconds = 0,
        int RequiredLevel = 1,
        ItemQuality Quality = ItemQuality.Common,
        long SellPrice = 5)
        : TradeItem(Id, Name, Icon, IsCraftingReagent: false, Quality, SellPrice)
    {
        public bool IsBuff => Kind is ConsumableKind.Elixir or ConsumableKind.Flask or ConsumableKind.Food;

        /// <summary>
        /// Buff slot: the same kind with the same stats shares a slot (stronger ones overwrite weaker ones),
        /// different kinds (e.g. elixir + food + flask) run in parallel.
        /// </summary>
        public string BuffKey => $"{Kind}:{StatSignature(Bonuses ?? PrimaryStats.Zero)}";

        /// <summary>Strength for comparison within a slot.</summary>
        public int Potency => Bonuses is { } b
            ? b.Strength + b.Agility + b.Stamina + b.Intelligence + b.Spirit
            : Amount;

        /// <summary>Description of the effect for tooltips (English).</summary>
        public string EffectText => Kind switch
        {
            ConsumableKind.HealthPotion => $"Restores {Amount} health.",
            ConsumableKind.ManaPotion   => $"Restores {Amount} mana.",
            ConsumableKind.Bandage      => $"On death: the Spirit Healer resurrects you {Amount}% faster.",
            ConsumableKind.Explosive    => $"Deals {Amount} damage to your enemy.",
            ConsumableKind.Costume      => $"Turns you into a Pirate or a Ninja for {FormatDuration(DurationSeconds)}.",
            _ => $"{string.Join(", ", (Bonuses ?? PrimaryStats.Zero).DescribeBonuses())} for {FormatDuration(DurationSeconds)}.",
        };

        public string KindLabel => Kind switch
        {
            ConsumableKind.HealthPotion or ConsumableKind.ManaPotion => "Potion",
            _ => Kind.ToString(),
        };

        private static string StatSignature(PrimaryStats b) => string.Concat(
            b.Strength > 0 ? "S" : "", b.Agility > 0 ? "A" : "", b.Stamina > 0 ? "T" : "",
            b.Intelligence > 0 ? "I" : "", b.Spirit > 0 ? "P" : "");

        public static string FormatDuration(double seconds) =>
            seconds >= 3600 ? $"{seconds / 3600:0.#} h" : seconds >= 60 ? $"{seconds / 60:0.#} min" : $"{seconds:0} sec";
    }

    /// <summary>Active consumable buff with remaining duration (combat seconds).</summary>
    public record ActiveConsumable(ConsumableItem Item, double Remaining);
}
