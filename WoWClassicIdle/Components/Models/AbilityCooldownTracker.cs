// Models/AbilityCooldownTracker.cs
namespace Models;

public class AbilityCooldownTracker
{
    private readonly Dictionary<string, double> _remaining = new();
    private readonly List<string> _expiredCooldowns = new();
    private readonly List<(string Id, double Value)> _cooldownUpdates = new();

    public bool IsReady(string abilityId) =>
        !_remaining.ContainsKey(abilityId) || _remaining[abilityId] <= 0;

    public void StartCooldown(string abilityId, double cooldownSeconds) =>
        _remaining[abilityId] = cooldownSeconds;

    public void Tick(double dt)
    {
        _expiredCooldowns.Clear();
        _cooldownUpdates.Clear();

        foreach (var kvp in _remaining)
        {
            double newVal = kvp.Value - dt;
            if (newVal <= 0)
                _expiredCooldowns.Add(kvp.Key);
            else
                _cooldownUpdates.Add((kvp.Key, newVal));
        }

        foreach (var k in _expiredCooldowns)        _remaining.Remove(k);
        foreach (var (k, v) in _cooldownUpdates)    _remaining[k] = v;
    }

    public void Reset() => _remaining.Clear();

    /// <summary>Verbleibende Sekunden (0 = ready).</summary>
    public double GetRemaining(string abilityId) =>
        _remaining.TryGetValue(abilityId, out var v) ? Math.Max(0, v) : 0;
}