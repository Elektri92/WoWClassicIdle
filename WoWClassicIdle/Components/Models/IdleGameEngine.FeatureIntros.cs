using Models;

// Neue Systeme als eigene Partial-Datei anlegen; Dateien möglichst < 300 Zeilen halten.
public partial class IdleGameEngine
{
    // ─── Einmalige Feature-Einführungen (z. B. erstes Betreten des Idle Fighters) ───

    // Accountweit: bereits gezeigte Intro-IDs (im Spielstand gespeichert)
    private readonly HashSet<string> _seenFeatureIntros = new();

    // Angeforderte, noch nicht angezeigte Intros – überbrückt, falls das Popup erst nach der Seite initialisiert
    private readonly Queue<GameFeature> _pendingFeatureIntros = new();

    /// <summary>Feuert, sobald ein Intro zum ersten Mal angefordert wurde. Abholen über <see cref="TakePendingFeatureIntros"/>.</summary>
    public event Action? FeatureIntroRequested;

    internal IReadOnlyCollection<string> SeenFeatureIntros => _seenFeatureIntros;

    /// <summary>Fordert das Intro an – nur beim allerersten Mal, danach No-Op.</summary>
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
        // Altstand ohne Feld: wer schon gekämpft hat, kennt den Idle Fighter bereits
        else if (save.TotalKills > 0)
            _seenFeatureIntros.Add(GameFeatureRegistry.IdleFighterIntroId);
    }
}
