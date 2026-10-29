namespace Models;

/// <summary>
/// Display names of the <see cref="Ability.ExclusiveGroup"/>s – only one spell per group can be equipped at a time.
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

    /// <summary>Display name of the group, null if the ability belongs to no group.</summary>
    public static string? LabelOf(Ability ability) =>
        string.IsNullOrEmpty(ability.ExclusiveGroup) ? null
        : Labels.TryGetValue(ability.ExclusiveGroup, out var label) ? label
        : "Exclusive";
}
