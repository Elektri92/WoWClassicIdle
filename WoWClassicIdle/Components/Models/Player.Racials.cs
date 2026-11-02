// Models/Player.Racials.cs
namespace Models
{
    // Passive racial abilities: data in RacialRegistry, effects outside of the stats in IdleGameEngine.Racials.cs.
    public partial class Player
    {
        /// <summary>Simulator only: measures specs independently of the race.</summary>
        public bool RacialsDisabled { get; private set; }

        public RacialBonus Racial => RacialsDisabled ? RacialBonus.None : RacialRegistry.BonusFor(Race);

        internal void SetRacialsDisabled(bool disabled)
        {
            RacialsDisabled = disabled;
            RecalculateStatsKeepResources();
        }

        // Additional attributes from the total values, so they scale with gear like in WoW
        private PrimaryStats RacialPrimaryBonus(PrimaryStats total) => new()
        {
            Spirit       = (int)Math.Round(total.Spirit * (Racial.SpiritMultiplier - 1)),
            Intelligence = (int)Math.Round(total.Intelligence * (Racial.IntellectMultiplier - 1)),
        };

        private Stats ApplyRacialStats(Stats stats) => stats with
        {
            MaxHp       = (int)Math.Round(stats.MaxHp * Racial.HpMultiplier),
            HpRegenPer5 = (int)Math.Round(stats.HpRegenPer5 * Racial.HpRegenMultiplier),
        };
    }
}
