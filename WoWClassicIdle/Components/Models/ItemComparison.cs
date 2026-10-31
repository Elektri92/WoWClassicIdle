// Models/ItemComparison.cs
namespace Models
{
    /// <summary>A single stat change when swapping an item (positive = gain).</summary>
    public record StatDelta(string Label, double Amount, string Format = "0")
    {
        public bool IsGain => Amount > 0;

        public string Text => $"{(Amount > 0 ? "+" : "")}{Amount.ToString(Format)} {Label}";
    }

    /// <summary>
    /// Calculates the stat differences between a candidate and the currently
    /// equipped item (candidate − equipped). Zero values are omitted.
    /// </summary>
    public static class ItemComparison
    {
        public static double Dps(WeaponItem weapon) =>
            (weapon.MinDamage + weapon.MaxDamage) / 2.0 / weapon.Speed;

        public static IReadOnlyList<StatDelta> Compare(ArmorItem candidate, ArmorItem equipped)
        {
            var deltas = PrimaryDeltas(candidate.Bonuses, equipped.Bonuses).ToList();

            double crit = Math.Round(((candidate.Effect?.CritChance ?? 0) - (equipped.Effect?.CritChance ?? 0)) * 100, 2);
            if (crit != 0) deltas.Add(new("% Critical Strike", crit, "0.##"));
            return deltas;
        }

        public static IReadOnlyList<StatDelta> Compare(WeaponItem candidate, WeaponItem equipped)
        {
            var deltas = new List<StatDelta>();

            // Round to two decimal places so rounding noise doesn't show up as a change
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
