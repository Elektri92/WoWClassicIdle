// Level-Up: goldene Lichtsäule um den Spieler-Frame plus Banner über dem Schlachtfeld.
// Darstellung in wwwroot/css/level-up.css.
using Models;

namespace WoWClassicIdle.Components.Pages;

public partial class IdleFighter
{
    private static readonly TimeSpan LevelUpDuration = TimeSpan.FromSeconds(3.4); // = längste CSS-Animation
    private Character? _levelWatchPlayer;
    private CharacterClass _levelWatchClass;
    private int _lastSeenLevel;
    private DateTime _levelUpAt = DateTime.MinValue;
    private int _levelUpLevel;

    private bool ShowLevelUp => DateTime.UtcNow - _levelUpAt < LevelUpDuration;

    // Timer-Thread! Nur ein Anstieg beim selben Charakter/derselben Klasse zählt –
    // Charakter- oder Klassenwechsel setzen lediglich die Vergleichsbasis neu.
    private void DetectLevelUp()
    {
        var player = _engine.Player;
        int level = player.Level;
        bool sameSource = player == _levelWatchPlayer && player.ActiveClass == _levelWatchClass;
        if (sameSource && level > _lastSeenLevel)
        {
            _levelUpLevel = level;
            _levelUpAt = DateTime.UtcNow;
        }
        _levelWatchPlayer = player;
        _levelWatchClass = player.ActiveClass;
        _lastSeenLevel = level;
    }

    // Funken der Lichtsäule: (horizontale Position %, Verzögerung ms, Größe px)
    private static readonly (int X, int Delay, int Size)[] LevelUpSparks =
    [
        (8, 0, 4), (18, 380, 3), (27, 120, 5), (36, 620, 3), (44, 260, 4), (52, 40, 6),
        (60, 520, 3), (68, 180, 5), (76, 700, 3), (84, 300, 4), (92, 90, 3), (14, 820, 4),
        (40, 900, 3), (64, 980, 4), (88, 760, 5), (30, 1100, 3),
    ];
}
