// Services/IAbilityLoadoutService.cs
namespace Services;

using Models;

public interface IAbilityLoadoutService
{
    /// <summary>
    /// Liefert die Loadout der Klasse. Beim ersten Zugriff werden alle für
    /// <paramref name="level"/> verfügbaren Abilities automatisch ausgerüstet.
    /// </summary>
    ClassLoadout GetLoadout(CharacterClass cls, int level);
    void SaveLoadout(ClassLoadout loadout);
    void ClearLoadout(CharacterClass cls);
}
