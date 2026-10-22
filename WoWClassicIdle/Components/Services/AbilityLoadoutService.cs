// Services/AbilityLoadoutService.cs
namespace Services;

using Models;

/// <summary>
/// In-Memory-Implementation. In Produktion durch eine DB- oder Storage-basierte
/// Implementation ersetzen (z. B. JSON-Datei, SQLite, etc.).
/// </summary>
public class AbilityLoadoutService : IAbilityLoadoutService
{
    private readonly Dictionary<CharacterClass, ClassLoadout> _store = new();
    private readonly object _lock = new();

    public ClassLoadout GetLoadout(CharacterClass cls, int level)
    {
        lock (_lock)
        {
            if (!_store.TryGetValue(cls, out var loadout))
            {
                // Erstauswahl: alle für Klasse + Level verfügbaren Abilities ausrüsten
                loadout = new ClassLoadout
                {
                    Class = cls,
                    EquippedAbilityIds = ClassAbilityRegistry.GetDefaultLoadoutIds(cls, level)
                };
                _store[cls] = loadout;
            }
            return loadout;
        }
    }

    public void SaveLoadout(ClassLoadout loadout)
    {
        lock (_lock)
        {
            _store[loadout.Class] = loadout;
        }
    }

    public void ClearLoadout(CharacterClass cls)
    {
        lock (_lock)
        {
            if (_store.TryGetValue(cls, out var loadout))
                loadout.Clear();
        }
    }
}
