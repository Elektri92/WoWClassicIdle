// Debug/DebugCharacterPresets.cs – wird nur im DEBUG-Build kompiliert (siehe .csproj)
namespace DebugTools;

using Models;

/// <summary>
/// Legt beim Start im Debug-Modus für jede Klasse einen vordefinierten Charakter an,
/// damit nicht jedes Mal manuell über die Charakterauswahl erstellt werden muss.
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
        // Nur auf leerem Roster anwenden – vorhandene Charaktere nie überschreiben
        if (engine.CharacterRoster.Count > 0) return;

        foreach (var (name, race, cls, gender) in Presets)
            engine.CreateCharacter(name, RaceClassInfo.RaceToFaction[race], race, cls, gender);

        // Ersten Preset-Charakter (Mage) aktiv setzen
        engine.SelectCharacter(engine.CharacterRoster[0].Id);
    }
}
