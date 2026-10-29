// Models/AbilityText.cs
using System.Globalization;

namespace Models;

/// <summary>WoW-style tooltip texts for abilities, generated from the ability values.</summary>
public static partial class AbilityText
{
    private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

    public static string ResourceName(CharacterClass? cls) => cls switch
    {
        CharacterClass.Warrior => "Rage",
        CharacterClass.Rogue   => "Energy",
        _                      => "Mana",
    };

    /// <summary>Resource of an ability – form-bound ones (Maul, Claw) use the resource of their form, stances that of the class.</summary>
    public static string ResourceName(Ability ability) =>
        ability.RequiredForm != DruidForm.None
            ? ShapeshiftForms.ResourceName(ShapeshiftForms.ResourceFor(
                ClassAbilityRegistry.ClassOf(ability) ?? CharacterClass.Druid, ability.RequiredForm))
            : ResourceName(ClassAbilityRegistry.ClassOf(ability));

    /// <summary>Cost line – form-dependent costs are appended (e.g. "14 Mana / 10 Rage / 25 Energy").</summary>
    public static string CostText(Ability ability)
    {
        if (ability.ManaCost <= 0) return "";
        var parts = new List<string> { $"{ability.ManaCost} {ResourceName(ability)}" };
        foreach (var (form, cost) in ability.FormCosts)
            parts.Add($"{cost} {ShapeshiftForms.ResourceName(ShapeshiftForms.ResourceFor(CharacterClass.Druid, form))}");
        return string.Join(" / ", parts);
    }

    /// <summary>Cost in a specific form (e.g. Rejuvenation in Cat Form → "25 Energy").</summary>
    public static string CostText(Ability ability, DruidForm form) =>
        ability.FormCosts.TryGetValue(form, out int cost)
            ? $"{cost} {ShapeshiftForms.ResourceName(ShapeshiftForms.ResourceFor(CharacterClass.Druid, form))}"
            : ability.ManaCost > 0 ? $"{ability.ManaCost} {ResourceName(ability)}" : "";

    /// <summary>Right side of the cost line: "Passive", "Next melee" or cooldown.</summary>
    public static string TimingText(Ability ability)
    {
        if (ability.IsForm)             return ShapeshiftForms.Get(ability.Form) is { IsStance: true } ? "Stance" : "Shapeshift";
        if (ability.IsAura)             return "Aura";
        if (ability.IsPassive)          return "Passive";
        if (ability.ReplacesAutoAttack)
            return ability.Cooldown > 0 ? $"Next melee · {Num(ability.Cooldown)} sec cooldown" : "Next melee";
        if (ability.HasCastTime)
            return ability.Cooldown > 0
                ? $"{Num(ability.CastTime)} sec cast · {Num(ability.Cooldown)} sec cooldown"
                : $"{Num(ability.CastTime)} sec cast";
        if (ability.Cooldown > 0)       return $"{Num(ability.Cooldown)} sec cooldown";
        return "Instant";
    }

    /// <summary>Yellow effect text like in the Classic tooltip – incl. synergy and form hint.</summary>
    public static string Description(Ability ability) =>
        CoreDescription(ability) + SynergyText(ability) + RequiredFormText(ability);

    private static string CoreDescription(Ability ability)
    {
        // Mechanics from talent row 2 (pets, charges, empowerments …) – see AbilityText.Special.cs
        if (SpecialDescription(ability) is { } special)
            return special;

        // Druid form – check before IsPassive (forms are passive but have no proc)
        if (ability.IsForm)
            return FormDescription(ability);

        // Passive proc (e.g. Deadly Poison) – check first, since IsDebuffDot can also be true here
        if (ability.IsPassive)
        {
            string s = $"Passive. Each of your attacks has a {Num(ability.ProcChance * 100)}% chance to apply"
                     + $" {ability.Name} to the target";
            if (ability.DebuffMaxStacks > 1)
                s += $", stacking up to {ability.DebuffMaxStacks} times";
            s += ".";
            if (ability.IsDebuffDot)
                s += $" Each stack deals {ability.DebuffDotDamage} damage every {Num(ability.DebuffDotTickInterval)} sec.";
            return s;
        }

        // Mana over time (e.g. Conjure Water, Drain Mana) – check before general IsBuff, because IsMot ⊂ IsBuff
        if (ability.IsMot)
            return $"Restores {Num(ability.MotManaRestorePct * 100)}% of your maximum mana over"
                 + $" {Num(ability.BuffDuration)} sec ({ability.MotTickCount} ticks, every {Num(ability.MotTickInterval)} sec)."
                 + (ability.BuffDamageBonus > 0 ? $" Increases all damage you deal by {Num(ability.BuffDamageBonus * 100)}% during that time." : "")
                 + " Recast automatically when it expires.";

        // Percentage HoT (e.g. Conjure Food) – check before general IsHot
        if (ability.IsHot && ability.HotHealPct > 0)
            return $"Restores {Num(ability.HotHealPct * 100)}% of your maximum health over"
                 + $" {Num(ability.BuffDuration)} sec ({ability.HotTickCount} ticks, every {Num(ability.HotTickInterval)} sec)."
                 + " Recast automatically when it expires.";

        // HoT – also check before general IsBuff
        if (ability.IsHot)
            return $"Heals you for {ability.HotTotalHeal} over {Num(ability.BuffDuration)} sec"
                 + $" ({ability.HotTickCount} ticks, every {Num(ability.HotTickInterval)} sec).";

        if (ability.IsBuff)
            return BuffDescription(ability);

        // On-next-swing (z. B. Heroic Strike, Holy Strike)
        if (ability.ReplacesAutoAttack)
            return $"A powerful strike dealing weapon damage plus {ability.Damage}{MultiplierText(ability)}. Replaces your next auto attack.";

        // Nahkampf-Sofortschlag (z. B. Sinister Strike, Raptor Strike)
        if (ability.AddsWeaponDamage)
            return $"An instant strike that deals weapon damage plus {ability.Damage}{MultiplierText(ability)}.";

        // Direct damage – possibly with execute condition and attached DoT (e.g. Moonfire)
        if (ability.IsDamage)
        {
            string desc = ability.HitCount > 1
                ? $"Strikes the enemy {ability.HitCount} times, dealing {ability.Damage} damage each ({ability.Damage * ability.HitCount} total)."
                : $"Deals {ability.Damage} damage to the enemy.";
            if (ability.BonusCritChance > 0)
                desc += $" Has a {Num(ability.BonusCritChance * 100)}% increased critical strike chance.";
            if (ability.RequiresEnemyBelowHpPct > 0)
                desc += $" Only usable on enemies below {Num(ability.RequiresEnemyBelowHpPct * 100)}% health.";
            if (ability.IsDebuffDot)
            {
                int total = ability.DebuffDotDamage * ability.DebuffDotTickCount;
                desc += $" Also afflicts the target, dealing {ability.DebuffDotDamage} damage every"
                      + $" {Num(ability.DebuffDotTickInterval)} sec for {Num(ability.DebuffDuration)} sec ({total} total).";
            }
            if (ability.IsDebuff && ability.DebuffAttackSpeedReduction > 0)
                desc += $" Slows the target's attack speed by {Num(ability.DebuffAttackSpeedReduction * 100)}% for {Num(ability.DebuffDuration)} sec.";
            if (ability.IsHeal)
                desc += $" At the same time heals you for {ability.Heal}.";
            return desc;
        }

        // Angriffstempo-Slow (z. B. Thunder Clap, Earthbind Totem)
        if (ability.IsDebuff && ability.DebuffAttackSpeedReduction > 0)
            return $"Reduces the target's attack speed by {Num(ability.DebuffAttackSpeedReduction * 100)}%"
                 + $" for {Num(ability.DebuffDuration)} sec.";

        // Stun (e.g. Kidney Shot)
        if (ability.IsDebuff && ability.DebuffIsStun)
            return $"Stuns the target for {Num(ability.DebuffDuration)} sec.";

        // Pure DoT – possibly with lifesteal (e.g. Curse of Agony, Drain Life, Shadow Word: Pain)
        if (ability.IsDebuffDot)
        {
            int total = ability.DebuffDotDamage * ability.DebuffDotTickCount;
            string desc = $"Afflicts the target, dealing {ability.DebuffDotDamage} damage every"
                        + $" {Num(ability.DebuffDotTickInterval)} sec for {Num(ability.DebuffDuration)} sec ({total} total).";
            if (ability.DebuffDotLifesteal > 0)
                desc += $" Heals you for {Num(ability.DebuffDotLifesteal * 100)}% of all damage dealt.";
            return desc;
        }

        // Debuff: increased damage (e.g. Hunter's Mark)
        if (ability.IsDebuff && ability.DebuffDamageTakenBonus > 0)
            return $"Marks the target, causing it to take {Num(ability.DebuffDamageTakenBonus * 100)}% more damage"
                 + $" for {Num(ability.DebuffDuration)} sec.";

        if (ability.IsDebuff)
            return $"Applies a debuff to the target for {Num(ability.DebuffDuration)} sec.";

        if (ability.IsHeal)
            return $"Heals you for {ability.Heal}.";

        // Life Tap (z. B. Warlock: HP → Mana)
        if (ability.IsLifeTap)
            return $"Converts {ability.LifeTapHpCost} health into {ability.LifeTapManaGain} mana.";

        return "";
    }

    private static string FormDescription(Ability ability)
    {
        var form = ShapeshiftForms.Get(ability.Form);
        if (form is null) return "";
        if (form.IsStance)
            return $"Enter {form.Name} while equipped: {AuraEffectText(ability)}. Unlocks abilities that require this stance."
                 + " Only one stance can be equipped at a time.";

        string desc = $"Shapeshift into {form.Name} while equipped.";
        if (form.Resource != ResourceKind.Mana)
            desc += $" You use {ShapeshiftForms.ResourceName(form.Resource)} instead of Mana and cannot equip spells that cost other resources.";
        if (form.BoostedAbilityDamageBonus > 0)
            desc += $" Increases the damage of {string.Join(" and ", form.BoostedAbilityNames)} by {Num(form.BoostedAbilityDamageBonus * 100)}%.";
        if (form.DamageTakenReduction > 0)
            desc += $" Reduces all damage taken by {Num(form.DamageTakenReduction * 100)}%.";
        if (form.HealingBonus > 0)
            desc += $" Increases all healing done by {Num(form.HealingBonus * 100)}%.";
        if (form.SpiritManaRegenBonus > 0)
            desc += $" Increases mana regeneration from Spirit by {Num(form.SpiritManaRegenBonus * 100)}%.";
        return desc + " Only one form can be equipped at a time.";
    }

    private static string BuffDescription(Ability ability)
    {
        // "Recast automatically" only makes sense if the buff doesn't expire before the cooldown
        bool autoRecast = ability.Cooldown == 0 || ability.Cooldown <= ability.BuffDuration;
        string suffix = autoRecast
            ? $" for {Num(ability.BuffDuration)} sec. Recast automatically when it expires."
            : $" for {Num(ability.BuffDuration)} sec.";

        if (ability.BuffDamageBonus > 0)
            return $"Increases all damage you deal by {Num(ability.BuffDamageBonus * 100)}%{suffix}";

        if (ability.BuffAttackSpeedBonus > 0)
            return $"Increases your attack speed by {Num(ability.BuffAttackSpeedBonus * 100)}%{suffix}";

        if (ability.BuffWeaponDamageBonus > 0)
            return $"Increases your auto-attack damage by {Num(ability.BuffWeaponDamageBonus * 100)}%{suffix}";

        if (ability.BuffIntellectBonus > 0)
        {
            string text = $"Increases Intellect by {ability.BuffIntellectBonus}";
            if (ability.BuffSpiritBonus > 0)
                text += $" and Spirit by {ability.BuffSpiritBonus}";
            return text + ", granting additional mana and mana regeneration" + suffix;
        }

        if (ability.BuffDamageTakenReduction > 0)
            return $"Reduces all damage taken by {Num(ability.BuffDamageTakenReduction * 100)}%{suffix}";

        if (ability.BuffGrantsImmunity)
            return $"Makes you immune to all attacks and spells{suffix}";

        if (ability.BuffRetaliationDamage > 0)
            return $"Deals {ability.BuffRetaliationDamage} damage to attackers{suffix}";

        if (ability.BuffShieldPct > 0)
            return $"Absorbs damage equal to {Num(ability.BuffShieldPct * 100)}% of your maximum health{suffix}";

        if (ability.BuffExtraAttackChance > 0)
            return $"Each auto attack has a {Num(ability.BuffExtraAttackChance * 100)}% chance to strike"
                 + $" {ability.BuffExtraAttacks} additional times{suffix}";

        return $"Buffs you{suffix}";
    }

    /// <summary>Additional hint on the auto-cast logic (e.g. rage threshold, heal gate).</summary>
    public static string AutoCastNote(Ability ability)
    {
        if (ability.IsForm || ability.IsAura)
            return "Active as long as it is equipped — no cast required.";


        if (ability.IsPassive)
            return "Equipped passively. Triggers automatically on each attack — no cast required.";

        if (ability.IsHealing)
            return $"Auto-cast at or below {Num(Ability.HealThreshold * 100)}% health."
                 + " Damage spells wait until there is enough mana.";

        return ability.MinResourceToCast > 0
            ? $"Auto-cast only at {ability.MinResourceToCast}+ {ResourceName(ability)}."
            : "";
    }

    /// <summary>Single-line effect text for the pool view (without cooldown and resources).</summary>
    public static string ShortSummary(Ability ability)
    {
        if (SpecialShortSummary(ability) is { } special)
            return special;
        if (ability.IsForm && ShapeshiftForms.Get(ability.Form) is { IsStance: true })
            return $"Stance · {AuraEffectText(ability)}";
        if (ability.IsForm)
            return ShapeshiftForms.Get(ability.Form) is { Resource: not ResourceKind.Mana } f
                ? $"Shapeshift · uses {ShapeshiftForms.ResourceName(f.Resource)}"
                : "Shapeshift";
        if (ability.IsPassive)
        {
            string s = $"{Num(ability.ProcChance * 100)}% proc";
            if (ability.IsDebuffDot)
                s += $" · {ability.DebuffDotDamage} DMG/{Num(ability.DebuffDotTickInterval)}s";
            return s;
        }
        if (ability.IsMot)
            return $"{Num(ability.MotManaRestorePct * 100)}% Mana over {Num(ability.BuffDuration)}s";
        if (ability.IsHot)
            return $"{ability.HotTotalHeal} Healing over {Num(ability.BuffDuration)}s";
        if (ability.IsBuff)
        {
            if (ability.BuffDamageBonus > 0)
                return $"+{Num(ability.BuffDamageBonus * 100)}% DMG for {Num(ability.BuffDuration)}s";
            if (ability.BuffAttackSpeedBonus > 0)
                return $"+{Num(ability.BuffAttackSpeedBonus * 100)}% Attack Speed for {Num(ability.BuffDuration)}s";
            if (ability.BuffWeaponDamageBonus > 0)
                return $"+{Num(ability.BuffWeaponDamageBonus * 100)}% Melee DMG for {Num(ability.BuffDuration)}s";
            if (ability.BuffIntellectBonus > 0)
            {
                string t = $"+{ability.BuffIntellectBonus} Int";
                if (ability.BuffSpiritBonus > 0)
                    t += $", +{ability.BuffSpiritBonus} Spirit";
                return t + $" for {Num(ability.BuffDuration)}s";
            }
            if (ability.BuffDamageTakenReduction > 0)
                return $"-{Num(ability.BuffDamageTakenReduction * 100)}% DMG taken for {Num(ability.BuffDuration)}s";
            if (ability.BuffGrantsImmunity)
                return $"Immune for {Num(ability.BuffDuration)}s";
            if (ability.BuffRetaliationDamage > 0)
                return $"{ability.BuffRetaliationDamage} retaliation DMG for {Num(ability.BuffDuration)}s";
            if (ability.BuffShieldPct > 0)
                return $"Shield {Num(ability.BuffShieldPct * 100)}% HP for {Num(ability.BuffDuration)}s";
            if (ability.BuffExtraAttackChance > 0)
                return $"{Num(ability.BuffExtraAttackChance * 100)}% chance for +{ability.BuffExtraAttacks} attacks for {Num(ability.BuffDuration)}s";
            return $"Buff for {Num(ability.BuffDuration)}s";
        }
        if (ability.AddsWeaponDamage)
        {
            string s = (ability.Damage > 0 ? $"Weapon +{ability.Damage} DMG" : "Weapon DMG") + MultiplierText(ability, compact: true);
            if (ability.IsDebuffDot)
            {
                int total = ability.DebuffDotDamage * ability.DebuffDotTickCount;
                s += $" + {ability.DebuffDotDamage} DMG/{Num(ability.DebuffDotTickInterval)}s ({total} total)";
            }
            return s;
        }
        if (ability.IsDamage)
        {
            string s = ability.HitCount > 1 ? $"{ability.HitCount}× {ability.Damage} DMG" : $"{ability.Damage} DMG";
            if (ability.BonusCritChance > 0)
                s += $" (+{Num(ability.BonusCritChance * 100)}% crit)";
            if (ability.IsDebuffDot)
            {
                int total = ability.DebuffDotDamage * ability.DebuffDotTickCount;
                s += $" + {ability.DebuffDotDamage} DMG/{Num(ability.DebuffDotTickInterval)}s ({total} total)";
            }
            if (ability.IsDebuff && ability.DebuffAttackSpeedReduction > 0)
                s += $" + -{Num(ability.DebuffAttackSpeedReduction * 100)}% Atk Speed {Num(ability.DebuffDuration)}s";
            if (ability.IsHeal)
                s += $" + {ability.Heal} Healing";
            return s;
        }
        if (ability.IsDebuffDot)
        {
            int total = ability.DebuffDotDamage * ability.DebuffDotTickCount;
            string s = $"{ability.DebuffDotDamage} DMG/{Num(ability.DebuffDotTickInterval)}s ({total} total)";
            if (ability.DebuffDotLifesteal > 0)
                s += " + Lifesteal";
            return s;
        }
        if (ability.IsDebuff)
        {
            if (ability.DebuffAttackSpeedReduction > 0)
                return $"-{Num(ability.DebuffAttackSpeedReduction * 100)}% Attack Speed for {Num(ability.DebuffDuration)}s";
            if (ability.DebuffDamageTakenBonus > 0)
                return $"+{Num(ability.DebuffDamageTakenBonus * 100)}% DMG taken for {Num(ability.DebuffDuration)}s";
            if (ability.DebuffIsStun)
                return $"Stun for {Num(ability.DebuffDuration)}s";
        }
        if (ability.IsHeal)
            return $"{ability.Heal} Healing";
        if (ability.IsLifeTap)
            return $"-{ability.LifeTapHpCost} HP / +{ability.LifeTapManaGain} Mana";
        return "";
    }

    /// <summary>CSS class for ShortSummary: "wow-dmg", "wow-heal" or empty.</summary>
    public static string ShortSummaryClass(Ability ability)
    {
        if (ability.IsHealing || ability.IsMot) return "wow-heal";
        if (ability.IsDamage || ability.IsDebuffDot || ability.AddsWeaponDamage || ability.IsPassive) return "wow-dmg";
        return "";
    }

    private static string Num(double v) => v.ToString("0.#", Inv);

    /// <summary>Talent/form bonus on the entire weapon strike, e.g. " (+10% damage)".</summary>
    private static string MultiplierText(Ability ability, bool compact = false)
    {
        if (Math.Abs(ability.DamageMultiplier - 1.0) < 1e-9) return "";
        string pct = ((ability.DamageMultiplier - 1.0) * 100).ToString("+0.#;-0.#", Inv);
        return compact ? $" ({pct}%)" : $" ({pct}% total damage)";
    }
}
