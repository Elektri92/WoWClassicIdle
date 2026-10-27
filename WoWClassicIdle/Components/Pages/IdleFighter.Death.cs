// Tod & Spirit Healer: Geisterwelt-Overlay solange der Spieler tot ist, Lichtblitz bei der Wiederbelebung.
// Darstellung in wwwroot/css/spirit-healer.css.
namespace WoWClassicIdle.Components.Pages;

public partial class IdleFighter
{
    private static readonly TimeSpan ResurrectFlashDuration = TimeSpan.FromSeconds(1.6); // = CSS-Animation
    private bool _wasDead;
    private DateTime _resurrectedAt = DateTime.MinValue;

    private bool ShowResurrectFlash => DateTime.UtcNow - _resurrectedAt < ResurrectFlashDuration;

    // Timer-Thread! Übergang tot → lebendig startet den Wiederbelebungs-Blitz.
    private void DetectResurrection()
    {
        bool dead = _engine.IsAtSpiritHealer;
        if (_wasDead && !dead)
            _resurrectedAt = DateTime.UtcNow;
        _wasDead = dead;
    }
}
