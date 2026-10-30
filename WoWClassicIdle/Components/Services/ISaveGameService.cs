// Services/ISaveGameService.cs
namespace Services;

public interface ISaveGameService
{
    bool SaveExists { get; }
    /// <summary>Page the app was last showing; reopened on the next start. Null = start page.</summary>
    string? LastRoute { get; set; }
    void Save(string? path = null);
    bool TryLoad(string? path = null);
    bool TryAutoLoad();
    void DeleteSave();
}
