// Logic of the /character page (markup in CharacterSelection.razor, quotes in Models/CharacterQuotes.cs)
using Microsoft.AspNetCore.Components.Web;
using Models;

namespace WoWClassicIdle.Components.Pages;

public partial class CharacterSelection
{
    private Race selectedRace = Race.Human;
    private CharacterClass selectedClass = CharacterClass.Warrior;
    private Gender selectedGender = Gender.Male;
    private string characterName = "";
    private string? nameError;
    private string? errorMsg;
    private bool showCreation;

    // Character selected by click in the overview; without a click the active one
    private int? pickedSlotId;

    // Speech bubble: who speaks (race or class) and what; quoteKey recreates the bubble on every click
    private string? quoteText;
    private bool quoteFromClass;
    private int quoteKey;

    protected override void OnInitialized()
    {
        Engine.TickUpdated += OnEngineTick;
        showCreation = Engine.CharacterRoster.Count == 0;
    }

    public void Dispose()
    {
        Engine.TickUpdated -= OnEngineTick;
    }

    private void OnEngineTick() => InvokeAsync(StateHasChanged);

    private CharacterSlot? PickedSlot =>
        Engine.CharacterRoster.FirstOrDefault(s => s.Id == pickedSlotId) ?? Engine.ActiveCharacter;

    private void PickCharacter(CharacterSlot slot) => pickedSlotId = slot.Id;

    private void PlayCharacter(CharacterSlot slot)
    {
        if (Engine.IsGameRunning)
            Engine.Stop();
        Engine.SelectCharacter(slot.Id);
        Nav.NavigateTo("/idle");
    }

    private void EditCharacter(CharacterSlot slot)
    {
        selectedRace = slot.Race;
        selectedClass = slot.Class;
        selectedGender = slot.Gender;
        characterName = slot.Name;
        nameError = null;
        errorMsg = null;
        quoteText = null;
        showCreation = true;
    }

    private void ToggleCreation()
    {
        showCreation = !showCreation;
        characterName = "";
        nameError = null;
        errorMsg = null;
        quoteText = null;
    }

    // The preview already shows the name in its final spelling
    private string PreviewName =>
        string.IsNullOrWhiteSpace(characterName) ? "Unnamed" : CharacterNames.Normalize(characterName);

    private void OnNameChanged()
    {
        // Only show errors after input – an empty field at form start is not an error
        nameError = string.IsNullOrEmpty(characterName) ? null : CharacterNames.Validate(characterName);
        errorMsg = null;
    }

    private void OnNameKeyDown(KeyboardEventArgs e)
    {
        if (e.Key == "Enter") ConfirmAndPlay();
    }

    private Race[] AllianceRaces => new[]
    {
        Race.Human, Race.Dwarf, Race.NightElf, Race.Gnome
    };

    private Race[] HordeRaces => new[]
    {
        Race.Orc, Race.Undead, Race.Tauren, Race.Troll
    };

    // Starting zone per race – provides the background theme for the preview and characters without a chosen zone
    private static CombatZone? StartZone(Race race) => CombatZoneRegistry.StartZoneFor(race);

    private Faction GetFactionForRace(Race race) =>
        RaceClassInfo.RaceToFaction.TryGetValue(race, out var f) ? f : Faction.Alliance;

    private void SetRace(Race value)
    {
        selectedRace = value;
        var classes = RaceClassInfo.GetAvailableClasses(value);
        if (!classes.Contains(selectedClass))
            selectedClass = classes.First();
        errorMsg = null;
        Speak(CharacterQuotes.ForRace(value), fromClass: false);
    }

    private void SetClass(CharacterClass value)
    {
        selectedClass = value;
        errorMsg = null;
        Speak(CharacterQuotes.ForClass(value), fromClass: true);
    }

    private void Speak(string text, bool fromClass)
    {
        quoteText = text;
        quoteFromClass = fromClass;
        quoteKey++;
    }

    private void SetGender(Gender value) => selectedGender = value;

    private void ConfirmAndPlay()
    {
        nameError = CharacterNames.Validate(characterName);
        if (nameError is not null) return;
        try
        {
            var faction = GetFactionForRace(selectedRace);
            if (Engine.IsGameRunning)
                Engine.Stop();
            Engine.CreateCharacter(characterName, faction, selectedRace, selectedClass, selectedGender);
            Nav.NavigateTo("/idle");
        }
        catch (ArgumentException ex)
        {
            errorMsg = ex.Message;
        }
    }
}
