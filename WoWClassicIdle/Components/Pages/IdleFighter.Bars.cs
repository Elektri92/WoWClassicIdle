// Time-based progress bars (cast, swing timer): the browser animates them itself via CSS animation.
// Blazor only resets duration/start offset at cycle start, on speed/pause changes or drift.
// Per-tick values with a transition visibly jump back in WebKitGTK when retargeting.
using System.Globalization;

namespace WoWClassicIdle.Components.Pages;

public partial class IdleFighter
{
    private readonly BarClock _castClock = new();
    private readonly BarClock _swingClock = new();
    private readonly BarClock _enemySwingClock = new();

    /// <summary>Game seconds per real-time second – the engine computes whole sub-steps per tick.</summary>
    private double SimRate => Math.Max(1, Math.Round(_engine.SpeedMultiplier));

    /// <summary>Combat is at a standstill (pause, spirit healer, travel, no enemy) – swing timers don't advance then.</summary>
    private bool CombatFrozen =>
        !_running || _engine.IsAtSpiritHealer || _engine.IsTraveling || _engine.CurrentEnemy is not { IsAlive: true };

    /// <summary>Inline style of a bar: animation timing, or a static transform when paused.</summary>
    private static string BarStyle(BarClock clock, string staticTransform) =>
        clock.Animating ? clock.Timing : $"transform: {staticTransform}";

    private sealed class BarClock
    {
        // Deviation between animation and engine from which a resync happens
        private const double MaxDriftSeconds = 0.25;

        private double _lastProgress;
        private double _realDuration;
        private DateTime _start;

        /// <summary>Changes on every restart – as @key it forces a fresh element and thus an animation start.</summary>
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
                || progress < _lastProgress - 1e-6                    // new cycle
                || Math.Abs(realDuration - _realDuration) > 1e-3;    // Speed/interval changed
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
