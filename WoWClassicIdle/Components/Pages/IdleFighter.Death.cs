// Death & spirit healer: ghost world overlay while the player is dead, flash of light on resurrection.
// Darstellung in wwwroot/css/spirit-healer.css.
namespace WoWClassicIdle.Components.Pages;

public partial class IdleFighter
{
    private static readonly TimeSpan ResurrectFlashDuration = TimeSpan.FromSeconds(1.6); // = CSS-Animation
    private bool _wasDead;
    private DateTime _resurrectedAt = DateTime.MinValue;

    private bool ShowResurrectFlash => DateTime.UtcNow - _resurrectedAt < ResurrectFlashDuration;

    // Timer thread! The transition dead → alive starts the resurrection flash.
    private void DetectResurrection()
    {
        bool dead = _engine.IsAtSpiritHealer;
        if (_wasDead && !dead)
            _resurrectedAt = DateTime.UtcNow;
        _wasDead = dead;
    }
}
