// Zeitbasierte Fortschrittsbalken (Cast, Swing-Timer): Der Browser animiert sie per CSS-Animation selbst.
// Blazor setzt Dauer/Startversatz nur bei Zyklusbeginn, Tempo-/Pausenwechsel oder Drift neu.
// Pro-Tick-Werte mit Transition springen in WebKitGTK beim Umzielen sichtbar zurück.
using System.Globalization;

namespace WoWClassicIdle.Components.Pages;

public partial class IdleFighter
{
    private readonly BarClock _castClock = new();
    private readonly BarClock _swingClock = new();
    private readonly BarClock _enemySwingClock = new();

    /// <summary>Spielsekunden pro Echtzeitsekunde – die Engine rechnet ganze Sub-Steps pro Tick.</summary>
    private double SimRate => Math.Max(1, Math.Round(_engine.SpeedMultiplier));

    /// <summary>Kampf steht still (Pause, Geistheiler, Reise, kein Gegner) – Swing-Timer laufen dann nicht weiter.</summary>
    private bool CombatFrozen =>
        !_running || _engine.IsAtSpiritHealer || _engine.IsTraveling || _engine.CurrentEnemy is not { IsAlive: true };

    /// <summary>Inline-Style eines Balkens: Animations-Timing, oder statischer transform wenn pausiert.</summary>
    private static string BarStyle(BarClock clock, string staticTransform) =>
        clock.Animating ? clock.Timing : $"transform: {staticTransform}";

    private sealed class BarClock
    {
        // Abweichung zwischen Animation und Engine, ab der neu synchronisiert wird
        private const double MaxDriftSeconds = 0.25;

        private double _lastProgress;
        private double _realDuration;
        private DateTime _start;

        /// <summary>Wechselt bei jedem Neustart – als @key erzwingt es ein frisches Element und damit einen Animationsstart.</summary>
        public int Key { get; private set; }
        public bool Animating { get; private set; }
        public double Progress { get; private set; }
        public string Timing { get; private set; } = "";

        public void Update(double progress, double gameSeconds, double rate, bool paused)
        {
            var now = DateTime.UtcNow;
            double realDuration = gameSeconds / rate;
            bool animate = !paused && realDuration > 0 && progress < 1;

            bool restart = animate != Animating
                || progress < _lastProgress - 1e-6                    // neuer Zyklus
                || Math.Abs(realDuration - _realDuration) > 1e-3;    // Tempo/Intervall geändert
            if (!restart && animate)
                restart = Math.Abs((now - _start).TotalSeconds - progress * realDuration) > MaxDriftSeconds;

            _lastProgress = progress;
            Progress = progress;
            if (!restart) return;

            Key++;
            Animating = animate;
            _realDuration = realDuration;
            double offset = progress * realDuration;
            _start = now - TimeSpan.FromSeconds(offset);
            Timing = animate
                ? string.Create(CultureInfo.InvariantCulture, $"animation-duration: {realDuration:0.###}s; animation-delay: -{offset:0.###}s")
                : "";
        }
    }
}
