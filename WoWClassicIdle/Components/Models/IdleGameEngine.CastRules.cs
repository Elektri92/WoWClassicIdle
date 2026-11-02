using Models;

// Auto-cast conditions from the Ability Loadout; without a rule the default behavior of the ability applies.
public partial class IdleGameEngine
{
    private AbilityCastRule? CastRuleOf(Ability ability) => Player.GetCurrentLoadout().RuleFor(ability.Name);

    /// <summary>Resource condition: the player's rule replaces the built-in limits of the ability.</summary>
    private bool MeetsResourceCondition(Ability ability)
    {
        if (CastRuleOf(ability) is { ResourcePct: { } pct } rule)
        {
            long current = Player.CurrentMana * 100L;
            long limit   = pct * (long)(Player.Stats.MaxMana + Player.Buffs.BonusMaxMana);
            return rule.ResourceComparison == ResourceComparison.Below ? current < limit : current > limit;
        }

        if (ability.CastWhenManaBelowPct > 0
            && Player.CurrentMana >= Player.Stats.MaxMana * ability.CastWhenManaBelowPct) return false;
        if (ability.IsLifeTap && Player.CurrentMana >= Player.Stats.MaxMana / 2) return false;
        return Player.CurrentMana >= ability.MinResourceToCast;
    }

    /// <summary>Health condition: the player's rule replaces the heal threshold; shields have no default limit.</summary>
    private bool MeetsHpCondition(Ability ability)
    {
        if (CastRuleOf(ability)?.HpPct is { } pct)
            return Player.CurrentHp * 100L <= pct * (long)Player.Stats.MaxHp;
        return !ability.IsHealing || PlayerNeedsHeal;
    }
}
