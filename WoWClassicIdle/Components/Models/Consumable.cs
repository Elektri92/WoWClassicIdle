// Models/Consumable.cs
namespace Models
{
    /// <summary>Art eines Verbrauchsguts – bestimmt, wann und wie es im Kampf automatisch benutzt wird.</summary>
    public enum ConsumableKind
    {
        HealthPotion,   // stellt Leben wieder her (bei niedrigem Leben, gemeinsame Trank-Abklingzeit)
        ManaPotion,     // stellt Mana wieder her (bei niedrigem Mana, gemeinsame Trank-Abklingzeit)
        Elixir,         // Stat-Buff (Alchemie), 1 min
        Flask,          // Stat-Buff (Alchemie), 1 h, bleibt nach dem Tod erhalten
        Food,           // Stat-Buff (Kochkunst), 1 min
        Bandage,        // verkürzt die Wartezeit beim Geistheiler um Amount %
        Explosive,      // Ingenieurskunst: Amount Schaden am Gegner, eigene Abklingzeit
        Costume,        // nur manuell: verwandelt das Charaktermodell (Savory Deviate Delight), erneuert sich automatisch
    }

    /// <summary>Verkleidung durch Savory Deviate Delight (50/50).</summary>
    public enum Costume { None, Pirate, Ninja }

    public static class Costumes
    {
        public static string Name(Costume costume) => costume.ToString();

        /// <summary>Portrait der Verkleidung; null = keine.</summary>
        public static string? PortraitUrl(Costume costume) => costume switch
        {
            Costume.Pirate => "https://wow.zamimg.com/images/wow/icons/large/inv_helmet_66.jpg",
            Costume.Ninja  => "https://wow.zamimg.com/images/wow/icons/large/ability_rogue_masterofsubtlety.jpg",
            _              => null,
        };
    }

    /// <summary>
    /// Verbrauchsgut (Kategorie "Consumable"): stapelbares Berufs-Item, das im Kampf automatisch aus dem Inventar benutzt wird.
    /// <paramref name="Bonuses"/> = Stat-Buff (Elixier/Fläschchen/Essen) für <paramref name="DurationSeconds"/> Kampfsekunden,
    /// <paramref name="Amount"/> = Heilung/Mana bzw. Prozent schnellere Wiederbelebung (Verband).
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
        /// Buff-Slot: gleiche Art mit denselben Stats teilt sich einen Slot (stärkere überschreiben schwächere),
        /// verschiedene Arten (z. B. Elixier + Essen + Fläschchen) laufen parallel.
        /// </summary>
        public string BuffKey => $"{Kind}:{StatSignature(Bonuses ?? PrimaryStats.Zero)}";

        /// <summary>Stärke für den Vergleich innerhalb eines Slots.</summary>
        public int Potency => Bonuses is { } b
            ? b.Strength + b.Agility + b.Stamina + b.Intelligence + b.Spirit
            : Amount;

        /// <summary>Beschreibung des Effekts für Tooltips (englisch).</summary>
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

    /// <summary>Aktiver Verbrauchsgut-Buff mit verbleibender Laufzeit (Kampfsekunden).</summary>
    public record ActiveConsumable(ConsumableItem Item, double Remaining);
}
