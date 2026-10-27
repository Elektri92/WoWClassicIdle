using Models;

// Talent- und Fähigkeits-Nachwirkungen ab Reihe 2: Schild nach Holy Light, Verstärkungs-Buffs (Mongoose Bite, Incinerating Shadow).
public partial class IdleGameEngine
{
    /// <summary>Laufzeit des Holy-Barrier-Schilds nach Holy Light.</summary>
    private const double HolyBarrierDuration = 15;

    /// <summary>Laufzeit der Incinerating-Shadow-Verstärkung nach Shadow Bolt.</summary>
    private const double IncineratingShadowDuration = 15;

    /// <summary>Effekte, die nach dem Auslösen einer Fähigkeit greifen (Heilung und Schaden gleichermaßen).</summary>
    private void ApplyAbilityAfterEffects(Ability ability)
    {
        // Mongoose Bite: Treffer verstärkt die nächste Fähigkeit (Raptor Strike)
        if (ability.IsEmpowering && !ability.IsBuff)
            ApplyEmpowerBuff($"empower_{ability.Id}", ability.Name, ability.IconUrl,
                ability.EmpowersAbilityName, ability.EmpowerDamageBonus, ability.EmpowerDuration);

        ApplyEclipse(ability);   // Wrath ↔ Starfire (IdleGameEngine.SpecAbilities.cs)

        switch (ability.Name)
        {
            // Holy Barrier: Holy Light legt zusätzlich einen kleinen Schild an
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

            // Incinerating Shadow: Shadow Bolt verstärkt das nächste Incinerate
            case "Shadow Bolt":
                double bonus = Player.TalentState.GetTotalBonus(Player.ActiveClass, TalentEffectType.IncinerateEmpower);
                if (bonus > 0)
                    ApplyEmpowerBuff("talent_incinerating_shadow", "Incinerating Shadow",
                        "https://wow.zamimg.com/images/wow/icons/large/spell_shadow_shadowbolt.jpg",
                        "Incinerate", bonus, IncineratingShadowDuration);
                break;
        }
    }

    /// <summary>Legt einen Buff an, der die nächste Fähigkeit <paramref name="target"/> um <paramref name="bonus"/> verstärkt.</summary>
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
