// Models/Money.cs
namespace Models
{
    /// <summary>
    /// WoW Classic currency: everything is stored internally in copper.
    /// 100 Kupfer = 1 Silber, 100 Silber = 1 Gold.
    /// </summary>
    public static class Money
    {
        public const long CopperPerSilver = 100;
        public const long CopperPerGold   = CopperPerSilver * 100;

        public const string GoldIconUrl   = "https://wow.zamimg.com/images/icons/money-gold.gif";
        public const string SilverIconUrl = "https://wow.zamimg.com/images/icons/money-silver.gif";
        public const string CopperIconUrl = "https://wow.zamimg.com/images/icons/money-copper.gif";

        public static (long Gold, long Silver, long Copper) Split(long totalCopper)
        {
            totalCopper = Math.Max(0, totalCopper);
            return (totalCopper / CopperPerGold,
                    totalCopper % CopperPerGold / CopperPerSilver,
                    totalCopper % CopperPerSilver);
        }

        /// <summary>Text form like "1g 23s 45c" – leading zero values are omitted.</summary>
        public static string Format(long totalCopper)
        {
            var (g, s, c) = Split(totalCopper);
            if (g > 0) return $"{g}g {s}s {c}c";
            if (s > 0) return $"{s}s {c}c";
            return $"{c}c";
        }
    }
}
