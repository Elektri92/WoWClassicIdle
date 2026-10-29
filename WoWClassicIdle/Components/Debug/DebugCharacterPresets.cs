// Debug/DebugCharacterPresets.cs – only compiled in the DEBUG build (see .csproj)
namespace DebugTools;

using Models;

/// <summary>
/// On startup in debug mode creates a predefined character for every class,
/// so they don't have to be created manually via character selection every time.
/// </summary>
internal static class DebugCharacterPresets
{
    private static readonly (string Name, Race Race, CharacterClass Class, Gender Gender)[] Presets =
    [
        ("Morwen",   Race.Undead,   CharacterClass.Mage,    Gender.Female),
        ("Garosha",  Race.Orc,      CharacterClass.Warrior, Gender.Female),
        ("Fizzwix",  Race.Gnome,    CharacterClass.Warlock, Gender.Female),
        ("Elunara",  Race.NightElf, CharacterClass.Druid,   Gender.Female),
        ("Brunhild", Race.Dwarf,    CharacterClass.Paladin, Gender.Female),
        ("Talaha",   Race.Tauren,   CharacterClass.Hunter,  Gender.Female),
        ("Zuljin",   Race.Troll,    CharacterClass.Rogue,   Gender.Male),
        ("Drekthar", Race.Orc,      CharacterClass.Shaman,  Gender.Male),
        ("Liliana",  Race.Human,    CharacterClass.Priest,  Gender.Female),
    ];

    public static void Apply(IdleGameEngine engine)
    {
        // Only apply to an empty roster – never overwrite existing characters
        if (engine.CharacterRoster.Count > 0) return;

        foreach (var (name, race, cls, gender) in Presets)
            engine.CreateCharacter(name, RaceClassInfo.RaceToFaction[race], race, cls, gender);

        // Make the first preset character (Mage) active
        engine.SelectCharacter(engine.CharacterRoster[0].Id);
    }
}
