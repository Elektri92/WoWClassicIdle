// Models/AbilityCooldownTracker.cs
namespace Models;

public class AbilityCooldownTracker
{
    private readonly Dictionary<string, double> _remaining = new();

    public bool IsReady(string abilityId) =>
        !_remaining.ContainsKey(abilityId) || _remaining[abilityId] <= 0;

    public void StartCooldown(string abilityId, double cooldownSeconds) =>
        _remaining[abilityId] = cooldownSeconds;

    public void Tick(double dt)
    {
        foreach (var key in _remaining.Keys.ToList())
        {
            _remaining[key] -= dt;
            if (_remaining[key] <= 0)
                _remaining.Remove(key);
        }
    }

    public void Reset() => _remaining.Clear();

    /// <summary>Verbleibende Sekunden (0 = ready).</summary>
    public double GetRemaining(string abilityId) =>
        _remaining.TryGetValue(abilityId, out var v) ? Math.Max(0, v) : 0;
}