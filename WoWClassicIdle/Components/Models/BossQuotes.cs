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

            // Shadowfang Keep
            ["Rethilgore"]            = ["Arugal will feast on your bones!", "None shall leave this keep!"],
            ["Razorclaw the Butcher"] = ["*Razorclaw sharpens his cleaver*", "Fresh meat for the larder!"],
            ["Baron Silverlaine"]     = ["My house is cursed... and so are you!", "Leave my halls, intruders!"],
            ["Commander Springvale"]  = ["The Light has abandoned this place!", "Ash'ari will never be redeemed!"],
            ["Archmage Arugal"]       = ["You, too, shall serve!", "Release your rage!"],

            // Blackfathom Deeps
            ["Ghamoo-ra"]      = ["*Ghamoo-ra snaps its massive jaws*", "*The great turtle rises from the depths*"],
            ["Lady Sarevess"]  = ["You will be the first to die!", "The deeps will claim you!"],
            ["Gelihast"]       = ["Mrglrglrgl!", "Mrrrgl! Rrglmrrgl!"],
            ["Old Serra'kis"]  = ["*Old Serra'kis circles hungrily*", "*Something huge moves beneath the water*"],
            ["Aku'mai"]        = ["*Aku'mai roars from the darkness*", "*The beast of the Old Gods awakens*"],

            // Razorfen Kraul
            ["Aggem Thorncurse"]     = ["The thorns will drink your blood!", "Agamaggan's brambles will bind you!"],
            ["Death Speaker Jargba"] = ["Your bones will join the Death's Head!", "The spirits hunger for you!"],
            ["Overlord Ramtusk"]     = ["Razorfen will crush the intruders!", "No one enters the Kraul and lives!"],
            ["Agathelos the Raging"] = ["*Agathelos snorts and paws the ground*", "*The great boar charges with a furious roar*"],
            ["Charlga Razorflank"]   = ["Agamaggan will be avenged!", "Your blood will feed the brambles!"],

            // Scarlet Monastery
            ["Interrogator Vishas"]        = ["Tell me... tell me everything!", "Naughty secrets!"],
            ["Arcanist Doan"]              = ["You will not defile these mysteries!", "Burn in righteous fire!"],
            ["Herod"]                      = ["Ah, I've been waiting for a real challenge!", "Blades of Light!"],
            ["Scarlet Commander Mograine"] = ["Infidels! They must be purified!", "The Light shall burn you!"],
            ["High Inquisitor Whitemane"]  = ["Mograine has fallen? You shall pay for this treachery!", "The Crusade shall destroy all who carry the Scourge's taint!"],
        };

        public static string Pick(string bossName, Random rng)
        {
            var quotes = Quotes.GetValueOrDefault(bossName, Fallback);
            return quotes[rng.Next(quotes.Length)];
        }
    }
}
