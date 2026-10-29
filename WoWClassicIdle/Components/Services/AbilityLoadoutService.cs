// Services/AbilityLoadoutService.cs
namespace Services;

using Models;

/// <summary>
/// In-memory implementation. In production replace it with a DB- or storage-based
/// implementation (e.g. JSON file, SQLite, etc.).
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
                // Initial selection: equip all abilities available for class + level
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
