namespace Models
{
    // Lines of the dungeon bosses on their entrance (speech bubble in /idle)
    public static class BossQuotes
    {
        private static readonly string[] Fallback = ["You should not have come here!", "Your journey ends now!"];

        private static readonly Dictionary<string, string[]> Quotes = new()
        {
            // Wailing Caverns
            ["Lady Anacondra"]       = ["None can stand against the Serpent Lords!", "The serpents will feast on your flesh!"],
            ["Lord Cobrahn"]         = ["You will never wake the dreamer!", "I shall shed this skin and devour you!"],
            ["Lord Pythas"]          = ["The coils of death will crush you!", "This nightmare is your grave!"],
            ["Lord Serpentis"]       = ["I am the serpent king, I can do anything!", "Kneel before the Fanglords!"],
            ["Mutanus the Devourer"] = ["The Nightmare consumes all!", "Naralex sleeps... and you will sleep forever!"],

            // The Deadmines
            ["Rhahk'Zor"]      = ["VanCleef pay big for your heads!", "Me smash you good!"],
            ["Sneed"]          = ["Into the shredder with you!", "Nobody touches my machines!"],
            ["Gilnid"]         = ["More metal for the smelter!", "You'll make fine scrap!"],
            ["Mr. Smite"]      = ["We're under attack! Avast, ye swabs! Repel the invaders!", "You landlubbers are tougher than I thought!"],
            ["Edwin VanCleef"] = ["None may challenge the Brotherhood!", "Lapdogs, all of you!"],
        };

        public static string Pick(string bossName, Random rng)
        {
            var quotes = Quotes.GetValueOrDefault(bossName, Fallback);
            return quotes[rng.Next(quotes.Length)];
        }
    }
}
