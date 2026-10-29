using Models;

// Talent and ability after-effects from row 2: shield after Holy Light, empower buffs (Mongoose Bite, Incinerating Shadow).
public partial class IdleGameEngine
{
    /// <summary>Duration of the Holy Barrier shield after Holy Light.</summary>
    private const double HolyBarrierDuration = 15;

    /// <summary>Duration of the Incinerating Shadow empowerment after Shadow Bolt.</summary>
    private const double IncineratingShadowDuration = 15;

    /// <summary>Effects that apply after an ability is triggered (healing and damage alike).</summary>
    private void ApplyAbilityAfterEffects(Ability ability)
    {
        // Mongoose Bite: a hit empowers the next ability (Raptor Strike)
        if (ability.IsEmpowering && !ability.IsBuff)
            ApplyEmpowerBuff($"empower_{ability.Id}", ability.Name, ability.IconUrl,
                ability.EmpowersAbilityName, ability.EmpowerDamageBonus, ability.EmpowerDuration);

        ApplyEclipse(ability);   // Wrath ↔ Starfire (IdleGameEngine.SpecAbilities.cs)

        switch (ability.Name)
        {
            // Holy Barrier: Holy Light additionally applies a small shield
            case "Holy Light":
                double shieldPct = Player.TalentState.GetTotalBonus(Player.ActiveClass, TalentEffectType.HolyLightShield);
                if (shieldPct > 0)
                    Player.Buffs.Apply(new Ability
                    {
                        Id            = "talent_holy_barrier",
                        Name          = "Holy Barrier",
                        IconUrl       = "https://wow.zamimg.com/images/wow/icons/large/spell_holy_powerwordshield.jpg",
                        BuffDuration  = HolyBarrierDuration,
                        BuffShieldPct = shieldPct,
                    }, Player.Stats.MaxMana, Player.Stats.MaxHp);
                break;

            // Incinerating Shadow: Shadow Bolt empowers the next Incinerate
            case "Shadow Bolt":
                double bonus = Player.TalentState.GetTotalBonus(Player.ActiveClass, TalentEffectType.IncinerateEmpower);
                if (bonus > 0)
                    ApplyEmpowerBuff("talent_incinerating_shadow", "Incinerating Shadow",
                        "https://wow.zamimg.com/images/wow/icons/large/spell_shadow_shadowbolt.jpg",
                        "Incinerate", bonus, IncineratingShadowDuration);
                break;
        }
    }

    /// <summary>Applies a buff that empowers the next ability <paramref name="target"/> by <paramref name="bonus"/>.</summary>
    private void ApplyEmpowerBuff(string id, string name, string iconUrl, string target, double bonus, double duration) =>
        Player.Buffs.Apply(new Ability
        {
            Id                  = id,
            Name                = name,
            IconUrl             = iconUrl,
            BuffDuration        = duration,
            EmpowersAbilityName = target,
            EmpowerDamageBonus  = bonus,
        });
}
