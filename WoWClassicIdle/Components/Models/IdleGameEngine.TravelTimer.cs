using System.Diagnostics;
using Timer = System.Timers.Timer;

// Background travel: zone travel keeps going while the combat timer is stopped (other pages, hidden tab).
// Combat itself still only runs in /idle.
public partial class IdleGameEngine
{
    private Timer? _travelTimer;
    private readonly Stopwatch _travelClock = new();
    private double _lastTravelTime;

    /// <summary>Starts the background travel timer (once from Program.cs – not in the headless simulator).</summary>
    public void StartTravelTimer()
    {
        if (_travelTimer is not null) return;
        _travelClock.Start();
        _travelTimer = new Timer(100) { AutoReset = true };
        _travelTimer.Elapsed += (_, _) => OnTravelTick();
        _travelTimer.Start();
    }

    private void OnTravelTick()
    {
        double now = _travelClock.Elapsed.TotalSeconds;
        // Same pace as the combat loop (whole sub-steps per tick)
        double dt = (now - _lastTravelTime) * Math.Max(1, Math.Round(SpeedMultiplier));
        _lastTravelTime = now;

        bool anyArrived = false;
        lock (_simLock)
        {
            foreach (var slot in _roster)
            {
                // While the fight runs, the active character travels inside the combat loop
                if (slot.Id == _activeSlotId && IsGameRunning) continue;
                if (AdvanceTravel(slot, dt) && slot.Travel is null)
                    anyArrived = true;
            }
        }
        if (anyArrived) TickUpdated?.Invoke();
    }
}
