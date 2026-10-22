// Services/ISaveGameService.cs
namespace Services;

public interface ISaveGameService
{
    bool SaveExists { get; }
    void Save(string? path = null);
    bool TryLoad(string? path = null);
    bool TryAutoLoad();
}
