// Models/ItemComparison.cs
namespace Models
{
    /// <summary>Eine einzelne Stat-Änderung beim Tausch eines Items (positiv = Gewinn).</summary>
    public record StatDelta(string Label, double Amount, string Format = "0")
    {
        public bool IsGain => Amount > 0;

        public string Text => $"{(Amount > 0 ? "+" : "")}{Amount.ToString(Format)} {Label}";
    }

    /// <summary>
    /// Berechnet die Stat-Differenzen zwischen einem Kandidaten und dem aktuell
    /// ausgerüsteten Item (Kandidat − ausgerüstet). Nullwerte werden weggelassen.
    /// </summary>
    public static class ItemComparison
    {
        public static double Dps(WeaponItem weapon) =>
            (weapon.MinDamage + weapon.MaxDamage) / 2.0 / weapon.Speed;

        public static IReadOnlyList<StatDelta> Compare(ArmorItem candidate, ArmorItem equipped) =>
            PrimaryDeltas(candidate.Bonuses, equipped.Bonuses).ToList();

        public static IReadOnlyList<StatDelta> Compare(WeaponItem candidate, WeaponItem equipped)
        {
            var deltas = new List<StatDelta>();

            // Auf zwei Nachkommastellen runden, damit Rundungsrauschen nicht als Änderung erscheint
            double dps = Math.Round(Dps(candidate) - Dps(equipped), 2);
            if (dps != 0) deltas.Add(new("Damage per second", dps, "0.00"));

            deltas.AddRange(PrimaryDeltas(candidate.Bonuses ?? PrimaryStats.Zero, equipped.Bonuses ?? PrimaryStats.Zero));
            return deltas;
        }

        private static IEnumerable<StatDelta> PrimaryDeltas(PrimaryStats candidate, PrimaryStats equipped)
        {
            var pairs = new (string Label, int Amount)[]
            {
                ("Strength",  candidate.Strength     - equipped.Strength),
                ("Agility",   candidate.Agility      - equipped.Agility),
                ("Stamina",   candidate.Stamina      - equipped.Stamina),
                ("Intellect", candidate.Intelligence - equipped.Intelligence),
                ("Spirit",    candidate.Spirit       - equipped.Spirit),
            };

            foreach (var (label, amount) in pairs)
            {
                if (amount != 0) yield return new(label, amount);
            }
        }
    }
}
