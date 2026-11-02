// Boss entrance in /idle: the screen darkens, the boss slams in and says its line (css/boss-intro.css).
using System.Globalization;

namespace WoWClassicIdle.Components.Pages;

public partial class IdleFighter
{
    private Enemy? _introEnemy;
    private string _introTiming = "";

    /// <summary>Boss whose entrance is visible right now.</summary>
    private Enemy? IntroEnemy =>
        !ZoneMenuVisible && !_engine.IsTraveling && !_engine.IsAtSpiritHealer && _engine.CurrentEnemy is { IsAlive: true, IsInIntro: true } enemy
            ? enemy
            : null;

    /// <summary>Inline variables of the entrance timeline, which runs in step with the engine.</summary>
    private string BossIntroStyle(Enemy enemy)
    {
        // Fixed once per boss: a changed duration or delay would restart the animations
        if (!ReferenceEquals(enemy, _introEnemy))
        {
            _introEnemy = enemy;
            double elapsed = enemy.IntroSeconds - enemy.IntroRemaining;
            _introTiming = string.Create(CultureInfo.InvariantCulture,
                $"--bi: {enemy.IntroSeconds / SimRate:0.###}s; --bi-delay: -{elapsed / SimRate:0.###}s;");
        }
        return _running ? _introTiming : _introTiming + " --bi-state: paused;";
    }
}
