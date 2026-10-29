// Services/IAbilityLoadoutService.cs
namespace Services;

using Models;

public interface IAbilityLoadoutService
{
    /// <summary>
    /// Returns the loadout of the class. On first access all abilities available for
    /// <paramref name="level"/> are equipped automatically.
    /// </summary>
    ClassLoadout GetLoadout(CharacterClass cls, int level);
    void SaveLoadout(ClassLoadout loadout);
    void ClearLoadout(CharacterClass cls);
}
