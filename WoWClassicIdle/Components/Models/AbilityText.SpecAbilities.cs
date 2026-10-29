// Models/AbilityText.SpecAbilities.cs
namespace Models;

// Tooltip texts for the specialization abilities (talent row 3): auras, stances, synergies, special buffs.
public static partial class AbilityText
{
    /// <summary>Effect text of the specialization mechanics; null = check the next rule.</summary>
    private static string? SpecDescription(Ability ability)
    {
        if (ability.IsAura && !ability.IsForm)
        {
            string s = $"Passive aura: {AuraEffectText(ability)} while equipped";
            if (ability.RequiresActivePet) s += " and your pet is alive";
            s += ".";
            if (ability.ExclusiveGroup == "paladin_aura")  s += " Only one aura can be equipped at a time.";
            if (ability.ExclusiveGroup == "hunter_aspect") s += " Only one aspect can be equipped at a time.";
            return s;
        }

        if (ability.IsBuff && ability.BuffInstantCastName.Length > 0)
            return $"Your next {ability.BuffInstantCastName} is cast instantly. Lasts {Num(ability.BuffDuration)} sec.";

        if (ability.IsBuff && ability.BuffFreeCasts > 0)
            return (ability.BuffGrantsImmunity ? $"You take no damage for {Num(ability.BuffDuration)} sec." : "")
                 + (ability.BuffFreeCasts == 1
                     ? " Your next ability costs nothing."
                     : $" Your next {ability.BuffFreeCasts} abilities cost nothing.")
                 + (ability.BuffHealingBonus > 0 ? $" Healing done increased by {Num(ability.BuffHealingBonus * 100)}%." : "");

        if (ability.IsBuff && ability.BuffHealOnHit > 0)
            return $"Protects you with {ability.BuffHitCharges} charges of earth. Each attack against you consumes a charge"
                 + $" and heals you for {ability.BuffHealOnHit}. Lasts {Num(ability.BuffDuration)} sec. Recast automatically when it expires.";

        if (ability.IsBuff && ability.BuffResourceRegenBonus > 0)
            return $"Increases your {ResourceName(ability)} regeneration by {Num(ability.BuffResourceRegenBonus * 100)}%"
                 + $" for {Num(ability.BuffDuration)} sec.";

        return null;
    }

    /// <summary>One-liner for the specialization mechanics; null = default text.</summary>
    private static string? SpecShortSummary(Ability ability)
    {
        if (ability.IsAura && !ability.IsForm)
            return $"Aura · {AuraEffectText(ability)}";
        if (ability.IsBuff && ability.BuffInstantCastName.Length > 0)
            return $"Instant {ability.BuffInstantCastName}";
        if (ability.IsBuff && ability.BuffFreeCasts > 0)
            return (ability.BuffGrantsImmunity ? $"Immune {Num(ability.BuffDuration)}s · " : "") + "next ability free";
        if (ability.IsBuff && ability.BuffHealOnHit > 0)
            return $"{ability.BuffHitCharges} charges · +{ability.BuffHealOnHit} HP per hit taken";
        if (ability.IsBuff && ability.BuffResourceRegenBonus > 0)
            return $"+{Num(ability.BuffResourceRegenBonus * 100)}% {ResourceName(ability)} regen for {Num(ability.BuffDuration)}s";
        return null;
    }

    /// <summary>List of aura/stance effects, e.g. "+20% healing done, +25% mana regeneration".</summary>
    private static string AuraEffectText(Ability ability)
    {
        var parts = new List<string>();
        if (ability.BuffDamageBonus > 0)          parts.Add($"+{Num(ability.BuffDamageBonus * 100)}% damage");
        if (ability.BuffCritChanceBonus > 0)      parts.Add($"+{Num(ability.BuffCritChanceBonus * 100)}% crit chance");
        if (ability.BuffAttackSpeedBonus > 0)     parts.Add($"+{Num(ability.BuffAttackSpeedBonus * 100)}% attack speed");
        if (ability.BuffDamageTakenReduction > 0) parts.Add($"-{Num(ability.BuffDamageTakenReduction * 100)}% damage taken");
        if (ability.BuffRageGenBonus > 0)         parts.Add($"+{Num(ability.BuffRageGenBonus * 100)}% rage generation");
        if (ability.BuffHealingBonus > 0)         parts.Add($"+{Num(ability.BuffHealingBonus * 100)}% healing done");
        if (ability.BuffResourceRegenBonus > 0)   parts.Add($"+{Num(ability.BuffResourceRegenBonus * 100)}% {ResourceName(ability).ToLowerInvariant()} regeneration");
        if (ability.BuffPetDamageBonus > 0)       parts.Add($"+{Num(ability.BuffPetDamageBonus * 100)}% pet damage");
        if (ability.BuffPetDamageShareBonus > 0)  parts.Add($"your pet takes {Num(ability.BuffPetDamageShareBonus * 100)}% more of the damage dealt to you");
        if (ability.BuffRetaliationDamage > 0)    parts.Add($"deals {ability.BuffRetaliationDamage} damage to attackers");
        return parts.Count > 0 ? string.Join(", ", parts) : "no effect";
    }

    /// <summary>Synergie-Hinweis, z. B. „ Always critically strikes if Flame Shock is on the target."</summary>
    private static string SynergyText(Ability ability)
    {
        if (!ability.HasSynergy) return "";
        var effects = new List<string>();
        if (ability.SynergyDamageBonus > 0) effects.Add($"deals {Num(ability.SynergyDamageBonus * 100)}% more damage");
        if (ability.SynergyCritBonus >= 1.0) effects.Add("always critically strikes");
        else if (ability.SynergyCritBonus > 0) effects.Add($"has {Num(ability.SynergyCritBonus * 100)}% increased critical strike chance");
        return $" {Capitalize(string.Join(" and ", effects))} if {ability.SynergyDebuffName} is on the target.";
    }

    /// <summary>Gestalt-/Haltungs-Voraussetzung, z. B. „ Only usable in Battle Stance."</summary>
    private static string RequiredFormText(Ability ability) =>
        ShapeshiftForms.Get(ability.RequiredForm) is { } form
            ? (ability.IsPassive ? $" Only active in {form.Name}." : $" Only usable in {form.Name}.")
            : "";

    private static string Capitalize(string s) => s.Length == 0 ? s : char.ToUpperInvariant(s[0]) + s[1..];
}
