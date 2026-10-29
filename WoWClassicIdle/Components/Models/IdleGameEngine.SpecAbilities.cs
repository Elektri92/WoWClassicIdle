using Models;

// Specialization abilities (talent row 3, from Lv 20): passive auras, free and instant casts,
// synergy debuffs, Earth Shield and the talent passives Eclipse, Nightfall and Spirit of Redemption.
public partial class IdleGameEngine
{
    /// <summary>Eclipse: Wrath empowers the next Starfire …</summary>
    public const double LunarEclipseBonus = 0.30;

    /// <summary>… and Starfire the next Wrath – Wrath is considerably weaker and needs more so the switch always pays off.</summary>
    public const double SolarEclipseBonus = 0.75;

    private const double EclipseDuration = 15;

    /// <summary>Spirit of Redemption triggers at most every 2 minutes.</summary>
    private const double SpiritOfRedemptionCooldown = 120;

    /// <summary>Spirit form after an averted death: immune, free spells, more healing.</summary>
    public static readonly Ability SpiritOfRedemptionBuff = new()
    {
        Id                 = "talent_spirit_of_redemption",
        Name               = "Spirit of Redemption",
        IconUrl            = "https://wow.zamimg.com/images/wow/icons/large/inv_enchant_essenceeternallarge.jpg",
        BuffDuration       = 8,
        BuffGrantsImmunity = true,
        BuffFreeCasts      = 10,
        BuffHealingBonus   = 0.50,
    };

    /// <summary>Nightfall proc: the next Shadow Bolt is instant.</summary>
    private static readonly Ability ShadowTrance = new()
    {
        Id                  = "nightfall_shadow_trance",
        Name                = "Shadow Trance",
        IconUrl             = "https://wow.zamimg.com/images/wow/icons/large/spell_shadow_twilight.jpg",
        BuffDuration        = 10,
        BuffInstantCastName = "Shadow Bolt",
    };

    /// <summary>Heilungs-Multiplikator inkl. Buffs (Holy Aura, Spirit of Redemption).</summary>
    private double HealingMultiplier => Player.Stats.HealingMultiplier + Player.Buffs.HealingBonus;

    /// <summary>Rate of mana and energy regeneration (Adrenaline Rush, Holy Aura, Aspect of the Owl).</summary>
    private double ResourceRegenMultiplier => 1.0 + Player.Buffs.ResourceRegenBonus;

    /// <summary>Level reached and – for talent abilities – the unlocking talent skilled.</summary>
    private bool IsAbilityAvailable(Ability ability) =>
        Player.Level >= ability.LevelRequired
        && (!ability.TalentUnlockOnly || Player.HasTalentAbility(ability.Id));

    /// <summary>
    /// Passive auras (Paladin auras, aspects, stances, Soul Link) are applied as long as they are equipped
    /// and usable – without cast and GCD. After death (buff reset) they are renewed immediately.
    /// </summary>
    private void MaintainPassiveAuras()
    {
        foreach (var ability in Player.EquippedAbilities)
        {
            if (!ability.IsAura) continue;
            bool allowed = IsAbilityAvailable(ability)
                           && Player.CanUseInCurrentForm(ability)
                           && (!ability.RequiresActivePet || Pet is { IsAlive: true });
            bool active = Player.Buffs.IsActive(ability.Id);
            if (allowed && !active)
                Player.Buffs.Apply(Player.GetEffectiveAbility(ability), Player.Stats.MaxMana, Player.Stats.MaxHp);
            else if (!allowed && active)
                Player.Buffs.Remove(ability.Id);
        }
    }

    /// <summary>Cost when casting – a free charge (Vanish, Spirit of Redemption) makes it 0.</summary>
    private int CastCost(Ability ability) => Player.Buffs.HasFreeCast ? 0 : Player.GetEffectiveManaCost(ability);

    /// <summary>Deducts the cost or consumes a free charge (only for abilities that cost something).</summary>
    private bool PayCastCost(Ability ability)
    {
        int cost = Player.GetEffectiveManaCost(ability);
        if (cost > 0 && Player.Buffs.ConsumeFreeCast()) return true;
        return Player.ConsumeMana(cost);
    }

    /// <summary>Is the ability's synergy debuff on the enemy (Flame Shock for Lava Burst, Immolate for Conflagrate …)?</summary>
    private bool IsSynergyActive(Ability ability) =>
        ability.HasSynergy && CurrentEnemy is { } enemy && enemy.Debuffs.HasDebuffNamed(ability.SynergyDebuffName);

    /// <summary>
    /// Expected damage factor for cast selection: pending empowerments (Eclipse, Arcane Blast) and synergies
    /// (a crit doubles, +100 % crit chance ≈ ×2).
    /// </summary>
    private double SelectionDamageFactor(Ability ability)
    {
        double factor = Player.Buffs.PeekEmpower(ability.Name);
        if (IsSynergyActive(ability))
            factor *= (1.0 + ability.SynergyDamageBonus) * (1.0 + ability.SynergyCritBonus * (CombatEngine.CritMultiplier - 1.0));
        return factor;
    }

    /// <summary>Eclipse talent: Wrath and Starfire empower each other alternately.</summary>
    private void ApplyEclipse(Ability ability)
    {
        if (ability.Name is not ("Wrath" or "Starfire")) return;
        if (Player.TalentState.GetTotalBonus(Player.ActiveClass, TalentEffectType.Eclipse) <= 0) return;

        if (ability.Name == "Wrath")
            ApplyEmpowerBuff("eclipse_lunar", "Lunar Eclipse", "https://wow.zamimg.com/images/wow/icons/large/spell_arcane_starfire.jpg",
                "Starfire", LunarEclipseBonus, EclipseDuration);
        else
            ApplyEmpowerBuff("eclipse_solar", "Solar Eclipse", "https://wow.zamimg.com/images/wow/icons/large/spell_nature_abolishmagic.jpg",
                "Wrath", SolarEclipseBonus, EclipseDuration);
    }

    /// <summary>Nightfall talent: every DoT tick can trigger Shadow Trance (next Shadow Bolt instant).</summary>
    private void TryNightfallProc()
    {
        double chance = Player.TalentState.GetTotalBonus(Player.ActiveClass, TalentEffectType.Nightfall);
        if (chance > 0 && _rng.NextDouble() < chance)
            Player.Buffs.Apply(ShadowTrance);
    }

    /// <summary>Earth Shield: a hit taken heals the player (the charge is consumed by ConsumeHitCharges).</summary>
    private void ApplyHealOnHit()
    {
        var source = Player.Buffs.GetHealOnHitSource();
        if (source is null) return;
        var (heal, crit) = RollPlayerCrit((int)Math.Round(source.BuffHealOnHit * HealingMultiplier));
        int hpBefore = Player.CurrentHp;
        Player.RegenHp(heal);
        Meter.RecordAbility(source, CombatActionKind.Heal, Player.CurrentHp - hpBefore);
        CombatTextRaised?.Invoke(new CombatText(CombatTextTarget.Player, CombatTextKind.Heal, heal, crit));
    }

    /// <summary>Spirit of Redemption talent: a lethal hit lets the player keep fighting with 1 HP as an invulnerable spirit.</summary>
    private void TryCheatDeath()
    {
        if (Player.TalentState.GetTotalBonus(Player.ActiveClass, TalentEffectType.SpiritOfRedemption) <= 0) return;
        if (!Player.Cooldowns.IsReady(SpiritOfRedemptionBuff.Id)) return;

        Player.CurrentHp = 1;
        Player.Buffs.Apply(SpiritOfRedemptionBuff);
        Player.Cooldowns.StartCooldown(SpiritOfRedemptionBuff.Id, SpiritOfRedemptionCooldown);
        AbilityTriggered?.Invoke(SpiritOfRedemptionBuff);
        LastLog = $"✦ {SpiritOfRedemptionBuff.Name} – death averted!";
    }
}
