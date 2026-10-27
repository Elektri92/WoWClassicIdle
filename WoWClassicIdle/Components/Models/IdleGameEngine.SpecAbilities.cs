using Models;

// Spezialisierungs-Fähigkeiten (Talent-Reihe 3, ab Lv 20): passive Auren, Gratis- und Sofort-Zauber,
// Synergie-Debuffs, Earth Shield sowie die Talent-Passive Eclipse, Nightfall und Spirit of Redemption.
public partial class IdleGameEngine
{
    /// <summary>Eclipse: Wrath verstärkt das nächste Starfire …</summary>
    public const double LunarEclipseBonus = 0.30;

    /// <summary>… und Starfire das nächste Wrath – Wrath ist deutlich schwächer und braucht mehr, damit sich der Wechsel immer lohnt.</summary>
    public const double SolarEclipseBonus = 0.75;

    private const double EclipseDuration = 15;

    /// <summary>Spirit of Redemption greift höchstens alle 2 Minuten.</summary>
    private const double SpiritOfRedemptionCooldown = 120;

    /// <summary>Geistgestalt nach einem abgewendeten Tod: immun, Zauber gratis, mehr Heilung.</summary>
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

    /// <summary>Nightfall-Proc: das nächste Shadow Bolt ist sofort.</summary>
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

    /// <summary>Tempo der Mana- und Energie-Regeneration (Adrenaline Rush, Holy Aura, Aspect of the Owl).</summary>
    private double ResourceRegenMultiplier => 1.0 + Player.Buffs.ResourceRegenBonus;

    /// <summary>Level erreicht und – bei Talent-Fähigkeiten – das freischaltende Talent geskillt.</summary>
    private bool IsAbilityAvailable(Ability ability) =>
        Player.Level >= ability.LevelRequired
        && (!ability.TalentUnlockOnly || Player.HasTalentAbility(ability.Id));

    /// <summary>
    /// Passive Auren (Paladin-Auren, Aspekte, Haltungen, Soul Link) liegen an, solange sie ausgerüstet
    /// und nutzbar sind – ohne Cast und GCD. Nach dem Tod (Buff-Reset) werden sie sofort erneuert.
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

    /// <summary>Kosten beim Wirken – eine Gratis-Ladung (Vanish, Spirit of Redemption) macht sie 0.</summary>
    private int CastCost(Ability ability) => Player.Buffs.HasFreeCast ? 0 : Player.GetEffectiveManaCost(ability);

    /// <summary>Zieht die Kosten ab bzw. verbraucht eine Gratis-Ladung (nur bei Fähigkeiten, die etwas kosten).</summary>
    private bool PayCastCost(Ability ability)
    {
        int cost = Player.GetEffectiveManaCost(ability);
        if (cost > 0 && Player.Buffs.ConsumeFreeCast()) return true;
        return Player.ConsumeMana(cost);
    }

    /// <summary>Liegt der Synergie-Debuff der Fähigkeit auf dem Gegner (Flame Shock für Lava Burst, Immolate für Conflagrate …)?</summary>
    private bool IsSynergyActive(Ability ability) =>
        ability.HasSynergy && CurrentEnemy is { } enemy && enemy.Debuffs.HasDebuffNamed(ability.SynergyDebuffName);

    /// <summary>
    /// Erwarteter Schadensfaktor für die Cast-Auswahl: offene Verstärkungen (Eclipse, Arcane Blast) und Synergien
    /// (ein Crit verdoppelt, +100 % Crit-Chance ≈ ×2).
    /// </summary>
    private double SelectionDamageFactor(Ability ability)
    {
        double factor = Player.Buffs.PeekEmpower(ability.Name);
        if (IsSynergyActive(ability))
            factor *= (1.0 + ability.SynergyDamageBonus) * (1.0 + ability.SynergyCritBonus * (CombatEngine.CritMultiplier - 1.0));
        return factor;
    }

    /// <summary>Eclipse-Talent: Wrath und Starfire verstärken sich abwechselnd.</summary>
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

    /// <summary>Nightfall-Talent: jeder DoT-Tick kann Shadow Trance auslösen (nächstes Shadow Bolt sofort).</summary>
    private void TryNightfallProc()
    {
        double chance = Player.TalentState.GetTotalBonus(Player.ActiveClass, TalentEffectType.Nightfall);
        if (chance > 0 && _rng.NextDouble() < chance)
            Player.Buffs.Apply(ShadowTrance);
    }

    /// <summary>Earth Shield: ein erlittener Treffer heilt den Spieler (Ladung verbraucht ConsumeHitCharges).</summary>
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

    /// <summary>Spirit-of-Redemption-Talent: ein tödlicher Treffer lässt den Spieler mit 1 HP als unverwundbaren Geist weiterkämpfen.</summary>
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
