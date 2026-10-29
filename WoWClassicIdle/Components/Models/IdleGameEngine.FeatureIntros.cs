using Models;

// Create new systems as their own partial file; keep files below 300 lines where possible.
public partial class IdleGameEngine
{
    // ─── One-time feature introductions (e.g. first visit of the Idle Fighter) ───

    // Account-wide: intro IDs already shown (stored in the save game)
    private readonly HashSet<string> _seenFeatureIntros = new();

    // Requested intros not yet shown – bridges the case where the popup initializes after the page
    private readonly Queue<GameFeature> _pendingFeatureIntros = new();

    /// <summary>Fires as soon as an intro was requested for the first time. Collect via <see cref="TakePendingFeatureIntros"/>.</summary>
    public event Action? FeatureIntroRequested;

    internal IReadOnlyCollection<string> SeenFeatureIntros => _seenFeatureIntros;

    /// <summary>Requests the intro – only the very first time, a no-op afterwards.</summary>
    public void RequestFeatureIntro(string id)
    {
        if (GameFeatureRegistry.GetIntro(id) is not { } feature) return;
        lock (_simLock)
        {
            if (!_seenFeatureIntros.Add(id)) return;
            _pendingFeatureIntros.Enqueue(feature);
        }
        FeatureIntroRequested?.Invoke();
    }

    /// <summary>
    /// Should the level-up popup be shown? Account-wide features only for the first character:
    /// not if already seen or another character already reached the level (legacy saves).
    /// </summary>
    public bool ShouldShowUnlock(GameFeature feature)
    {
        if (!feature.OncePerAccount) return true;
        lock (_simLock)
        {
            if (_seenFeatureIntros.Contains(feature.Id)) return false;
            bool reachedBefore = _roster.Any(s => s.Id != _activeSlotId && s.Player.Level >= feature.UnlockLevel);
            _seenFeatureIntros.Add(feature.Id);
            return !reachedBefore;
        }
    }

    public List<GameFeature> TakePendingFeatureIntros()
    {
        lock (_simLock)
        {
            var list = _pendingFeatureIntros.ToList();
            _pendingFeatureIntros.Clear();
            return list;
        }
    }

    private void RestoreFeatureIntros(SaveData save)
    {
        _seenFeatureIntros.Clear();
        _pendingFeatureIntros.Clear();

        if (save.SeenFeatureIntros is { } seen)
            _seenFeatureIntros.UnionWith(seen);
        // Legacy save without the field: anyone who has already fought knows the Idle Fighter
        else if (save.TotalKills > 0)
            _seenFeatureIntros.Add(GameFeatureRegistry.IdleFighterIntroId);
    }
}
