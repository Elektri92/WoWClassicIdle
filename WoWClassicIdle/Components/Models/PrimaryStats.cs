// Models/PrimaryStats.cs
namespace Models
{
    /// <summary>
    /// The five primary attributes like in WoW Classic.
    /// </summary>
    public record PrimaryStats
    {
        public int Strength     { get; init; }
        public int Agility      { get; init; }
        public int Stamina      { get; init; }
        public int Intelligence { get; init; }
        public int Spirit       { get; init; }

        public static readonly PrimaryStats Zero = new();

        public static PrimaryStats operator +(PrimaryStats a, PrimaryStats b) => new()
        {
            Strength     = a.Strength     + b.Strength,
            Agility      = a.Agility      + b.Agility,
            Stamina      = a.Stamina      + b.Stamina,
            Intelligence = a.Intelligence + b.Intelligence,
            Spirit       = a.Spirit       + b.Spirit,
        };

        /// <summary>Readable bonus lines for tooltips (non-empty values only).</summary>
        public IEnumerable<string> DescribeBonuses()
        {
            if (Strength     > 0) yield return $"+{Strength} Strength";
            if (Agility      > 0) yield return $"+{Agility} Agility";
            if (Stamina      > 0) yield return $"+{Stamina} Stamina";
            if (Intelligence > 0) yield return $"+{Intelligence} Intellect";
            if (Spirit       > 0) yield return $"+{Spirit} Spirit";
        }
    }

    /// <summary>
    /// Growth of the primary attributes per level (decimal values so that
    /// "slow" attributes also grow over several levels).
    /// </summary>
    public record StatGrowth(double Strength, double Agility, double Stamina, double Intelligence, double Spirit);

    /// <summary>
    /// Starting values (level 1) and growth per level of a class.
    /// </summary>
    public record ClassStatProfile(PrimaryStats Base, StatGrowth PerLevel);
}
