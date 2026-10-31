// Services/ISaveGameService.cs
namespace Services;

public interface ISaveGameService
{
    bool SaveExists { get; }
    /// <summary>Page the app was last showing; reopened on the next start. Null = start page.</summary>
    string? LastRoute { get; set; }
    /// <summary>Newest changelog entry the player has opened. Null = never opened.</summary>
    string? LastSeenChangelogVersion { get; set; }
    void Save(string? path = null);
    bool TryLoad(string? path = null);
    bool TryAutoLoad();
    void DeleteSave();
}
