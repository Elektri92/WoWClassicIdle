// Models/AbilityText.Special.cs
namespace Models;

// Tooltip texts for mechanics from talent row 2: pets, hit charges, guaranteed crit, empowerments, damage + shield.
public static partial class AbilityText
{
    /// <summary>Yellow effect text for special mechanics; null = use default text.</summary>
    private static string? SpecialDescription(Ability ability)
    {
        // Specialization abilities (row 3) – see AbilityText.SpecAbilities.cs
        if (SpecDescription(ability) is { } spec)
            return spec;

        if (ability.IsPetSummon && PetRegistry.Get(ability.SummonsPet) is { } pet)
            return $"Summons your {pet.Name} ({pet.Role.ToLowerInvariant()}). It has {Num(pet.HpPctOfOwner * 100)}% of your maximum health,"
                 + $" attacks every {Num(pet.AttackInterval)} sec and takes {Num(PetRegistry.DamageShare * 100)}% of all damage dealt to you."
                 + $" If it dies, it is revived automatically after {Num(PetRegistry.DeathCooldown)} sec. Only one companion at a time.";

        if (ability.IsBuff && ability.BuffGuaranteesCrit)
            return $"Your next damaging ability is a guaranteed critical strike. Lasts {Num(ability.BuffDuration)} sec or until used.";

        if (ability.IsBuff && ability.BuffHitCharges > 0)
        {
            string s = $"Reduces the damage of the next {ability.BuffHitCharges} attacks against you by {Num(ability.BuffDamageTakenReduction * 100)}%";
            if (ability.BuffRetaliationDamage > 0)
                s += $" and deals {ability.BuffRetaliationDamage} damage to the attacker each time";
            return s + $". Lasts {Num(ability.BuffDuration)} sec.";
        }

        // Empower buff itself (Mongoose Bite, Incinerating Shadow) – appears in the buff bar
        if (ability.IsBuff && ability.IsEmpowering)
            return $"Your next {ability.EmpowersAbilityName} deals {Num(ability.EmpowerDamageBonus * 100)}% more damage."
                 + $" Lasts {Num(ability.BuffDuration)} sec.";

        if (ability.IsDamage && ability.IsEmpowering)
            return $"An instant strike that deals {DamageText(ability)}. Your next {ability.EmpowersAbilityName}"
                 + $" within {Num(ability.EmpowerDuration)} sec deals {Num(ability.EmpowerDamageBonus * 100)}% more damage.";

        if (ability.IsDamage && ability.IsShield)
            return $"An instant strike that deals {DamageText(ability)}. Also shields you for"
                 + $" {Num(ability.BuffShieldPct * 100)}% of your maximum health for {Num(ability.BuffDuration)} sec.";

        // Seal of Command: Waffenschaden + Zusatzschlag-Chance
        if (ability.IsBuff && ability.BuffWeaponDamageBonus > 0 && ability.BuffExtraAttackChance > 0)
            return $"Increases your auto-attack damage by {Num(ability.BuffWeaponDamageBonus * 100)}% and gives each auto attack"
                 + $" a {Num(ability.BuffExtraAttackChance * 100)}% chance to strike an additional time for {Num(ability.BuffDuration)} sec."
                 + " Recast automatically when it expires.";

        // Blade Flurry: every auto-attack hits multiple times
        if (ability.IsBuff && ability.BuffExtraAttackChance >= 1.0)
            return $"Your auto attacks strike {1 + ability.BuffExtraAttacks} times for {Num(ability.BuffDuration)} sec.";

        return null;
    }

    /// <summary>One-liner for special mechanics; null = use default text.</summary>
    private static string? SpecialShortSummary(Ability ability)
    {
        if (SpecShortSummary(ability) is { } spec)
            return spec;
        if (ability.IsPetSummon && PetRegistry.Get(ability.SummonsPet) is { } pet)
            return $"Summon {pet.Name} · {pet.Role}";
        if (ability.IsBuff && ability.BuffGuaranteesCrit)
            return "Next ability crits";
        if (ability.IsBuff && ability.BuffHitCharges > 0)
            return $"-{Num(ability.BuffDamageTakenReduction * 100)}% DMG for next {ability.BuffHitCharges} hits";
        if (ability.IsBuff && ability.IsEmpowering)
            return $"Next {ability.EmpowersAbilityName} +{Num(ability.EmpowerDamageBonus * 100)}% DMG";
        if (ability.IsDamage && ability.IsEmpowering)
            return $"Weapon +{ability.Damage} DMG · next {ability.EmpowersAbilityName} +{Num(ability.EmpowerDamageBonus * 100)}%";
        if (ability.IsDamage && ability.IsShield)
            return $"Weapon +{ability.Damage} DMG + {Num(ability.BuffShieldPct * 100)}% HP shield";
        if (ability.IsBuff && ability.BuffWeaponDamageBonus > 0 && ability.BuffExtraAttackChance > 0)
            return $"+{Num(ability.BuffWeaponDamageBonus * 100)}% Melee DMG · {Num(ability.BuffExtraAttackChance * 100)}% extra attack";
        if (ability.IsBuff && ability.BuffExtraAttackChance >= 1.0)
            return $"Auto attacks hit {1 + ability.BuffExtraAttacks}× for {Num(ability.BuffDuration)}s";
        return null;
    }

    private static string DamageText(Ability ability) =>
        ability.AddsWeaponDamage ? $"weapon damage plus {ability.Damage}{MultiplierText(ability)}" : $"{ability.Damage} damage";
}
