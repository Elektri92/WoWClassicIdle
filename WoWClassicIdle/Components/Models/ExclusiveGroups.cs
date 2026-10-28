namespace Models;

/// <summary>
/// Anzeigenamen der <see cref="Ability.ExclusiveGroup"/>s – pro Gruppe ist nur ein Zauber gleichzeitig ausrüstbar.
/// </summary>
public static class ExclusiveGroups
{
    private static readonly Dictionary<string, string> Labels = new()
    {
        ["mage_bolt"]             = "Main Spell",
        ["warrior_stance"]        = "Stance",
        ["druid_form"]            = "Form",
        ["priest_form"]           = "Form",
        ["paladin_seal"]          = "Seal",
        ["paladin_aura"]          = "Aura",
        ["hunter_aspect"]         = "Aspect",
        ["shaman_weapon_enchant"] = "Weapon Imbue",
    };

    /// <summary>Anzeigename der Gruppe, null wenn die Ability keiner Gruppe angehört.</summary>
    public static string? LabelOf(Ability ability) =>
        string.IsNullOrEmpty(ability.ExclusiveGroup) ? null
        : Labels.TryGetValue(ability.ExclusiveGroup, out var label) ? label
        : "Exclusive";
}
