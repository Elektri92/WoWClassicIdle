using Models;

public partial class IdleGameEngine
{
    /// <summary>Current death penalty of the active character.</summary>
    public DeathXpPenalty CurrentDeathXpPenalty
    {
        get
        {
            var player = Player;
            int milestone = DeathXpPenalty.MilestoneFor(player.Level);
            return new DeathXpPenalty(
                player.Level >= Player.DeathXpPenaltyMinLevel && player.XpToNextLevel > 0,
                milestone,
                _roster.Count(s => s.Player.Level >= milestone),
                player.Racial.DeathPenaltyMultiplier);
        }
    }
}
