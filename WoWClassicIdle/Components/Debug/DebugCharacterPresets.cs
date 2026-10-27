// Debug/DebugCharacterPresets.cs – wird nur im DEBUG-Build kompiliert (siehe .csproj)
namespace DebugTools;

using Models;

/// <summary>
/// Legt beim Start im Debug-Modus für jede Klasse einen vordefinierten Charakter an,
/// damit nicht jedes Mal manuell über die Charakterauswahl erstellt werden muss.
/// </summary>
internal static class DebugCharacterPresets
{
    private static readonly (Race Race, CharacterClass Class, Gender Gender)[] Presets =
    [
        (Race.Undead,   CharacterClass.Mage,    Gender.Female),
        (Race.Orc,      CharacterClass.Warrior, Gender.Female),
        (Race.Gnome,    CharacterClass.Warlock, Gender.Female),
        (Race.NightElf, CharacterClass.Druid,   Gender.Female),
        (Race.Dwarf,    CharacterClass.Paladin, Gender.Female),
        (Race.Tauren,   CharacterClass.Hunter,  Gender.Female),
        (Race.Troll,    CharacterClass.Rogue,   Gender.Male),
        (Race.Orc,      CharacterClass.Shaman,  Gender.Male),
        (Race.Human,    CharacterClass.Priest,  Gender.Female),
    ];

    public static void Apply(IdleGameEngine engine)
    {
        // Nur auf leerem Roster anwenden – vorhandene Charaktere nie überschreiben
        if (engine.CharacterRoster.Count > 0) return;

        foreach (var (race, cls, gender) in Presets)
            engine.CreateCharacter(RaceClassInfo.RaceToFaction[race], race, cls, gender);

        // Ersten Preset-Charakter (Mage) aktiv setzen
        engine.SelectCharacter(engine.CharacterRoster[0].Id);
    }
}
