// Models/AbilityCastRule.cs
using System.Text.Json.Serialization;

namespace Models;

public enum ResourceComparison { Below, Above }

/// <summary>
/// Player-defined auto-cast conditions of a spell, stored per spell name so they survive rank changes.
/// A condition that is not set falls back to the default behavior of the ability.
/// </summary>
public sealed record AbilityCastRule
{
    /// <summary>Starting value when a condition is switched on.</summary>
    public const int DefaultPercent = 50;

    public ResourceComparison ResourceComparison { get; init; }

    /// <summary>Limit in % of the maximum resource (mana, rage, energy). Null = default behavior.</summary>
    public int? ResourcePct { get; init; }

    /// <summary>Cast at or below this share of the player's max HP. Null = default behavior.</summary>
    public int? HpPct { get; init; }

    [JsonIgnore]
    public bool IsEmpty => ResourcePct is null && HpPct is null;

    /// <summary>Spells that are cast actively and spend or restore a resource.</summary>
    public static bool SupportsResource(Ability a) =>
        IsCast(a) && (a.ManaCost > 0 || a.FormCosts.Count > 0 || a.MinResourceToCast > 0
                      || a.CastWhenManaBelowPct > 0 || a.IsLifeTap || a.IsMot);

    /// <summary>Heals, shields and other defensive buffs that react to the player's health.</summary>
    public static bool SupportsHp(Ability a) =>
        IsCast(a) && (a.IsHealing || a.IsShield || a.BuffGrantsImmunity || a.BuffHealOnHit > 0
                      || a.BuffDamageTakenReduction > 0 || a.BuffRetaliationDamage > 0);

    private static bool IsCast(Ability a) => !a.IsPassive && !a.IsForm;
}
