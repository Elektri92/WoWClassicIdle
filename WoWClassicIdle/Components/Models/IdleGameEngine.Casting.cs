using Models;

// Cast-Zeiten, Global Cooldown und Auswahl der nächsten Fähigkeit.
public partial class IdleGameEngine
{
    /// <summary>Global Cooldown in Sekunden, ausgelöst von jeder Fähigkeit ohne Zauberzeit.</summary>
    public const double GlobalCooldown = 0.5;

    /// <summary>Fähigkeit, die gerade gewirkt wird (null = kein Cast).</summary>
    public Ability? CurrentCast { get; private set; }

    /// <summary>Bisher vergangene Cast-Zeit in Sekunden.</summary>
    public double CastElapsed { get; private set; }

    /// <summary>Gesamtdauer des aktuellen Casts (Zauberzeit, durch Attack-Speed verkürzt).</summary>
    public double CastDuration { get; private set; }

    /// <summary>Verbleibender Global Cooldown in Sekunden (0 = bereit).</summary>
    public double GcdRemaining { get; private set; }

    public double CastProgress => CurrentCast is null || CastDuration <= 0 ? 0 : Math.Min(1, CastElapsed / CastDuration);

    /// <summary>Aktuelles Auto-Attack-Intervall in Sekunden (inkl. Attack-Speed).</summary>
    public double SwingInterval { get; private set; } = 1;

    /// <summary>Fortschritt des Swing-Timers (0..1).</summary>
    public double SwingProgress => SwingInterval <= 0 ? 0 : Math.Clamp(_playerAttackAcc / SwingInterval, 0, 1);

    /// <summary>Aktuelles Angriffsintervall des Gegners in Sekunden (inkl. Verlangsamungs-Debuffs).</summary>
    public double EnemySwingInterval { get; private set; } = 1;

    /// <summary>Fortschritt des Gegner-Swing-Timers (0..1).</summary>
    public double EnemySwingProgress => EnemySwingInterval <= 0 ? 0 : Math.Clamp(_enemyAttackAcc / EnemySwingInterval, 0, 1);

    /// <summary>Spells mit Manakosten haben Vorrang: während ihres Casts wartet die Auto-Attack.</summary>
    public bool IsSwingHeld => CurrentCast is { ManaCost: > 0 };

    /// <summary>Bricht einen laufenden Cast ab (kein Mana-Verbrauch, kein Cooldown).</summary>
    private void CancelCast()
    {
        CurrentCast  = null;
        CastElapsed  = 0;
        CastDuration = 0;
    }

    /// <summary>Cast und GCD zurücksetzen (Tod, Charakterwechsel).</summary>
    private void ResetCastState()
    {
        CancelCast();
        GcdRemaining = 0;
    }

    /// <summary>
    /// Schreitet laufenden Cast und GCD voran und startet ggf. die nächste Fähigkeit.
    /// Gibt den dabei verursachten Schaden am Gegner zurück.
    /// </summary>
    private int UpdateAbilities(double dt, double attackSpeed)
    {
        int dealt = 0;
        // Epsilon: 0.5 - 5 × 0.1 ergibt in double knapp > 0 und würde den GCD um einen Schritt verlängern
        GcdRemaining = GcdRemaining - dt > 1e-9 ? GcdRemaining - dt : 0;

        if (CurrentCast is { } casting)
        {
            CastElapsed += dt;
            if (CastDuration - CastElapsed > 1e-9) return 0;

            CancelCast();
            dealt += FinishCast(casting);
            if (!CurrentEnemy!.IsAlive) return dealt;
            // Kein GCD nach einem Cast → nächste Fähigkeit darf direkt starten
        }

        if (GcdRemaining > 0) return dealt;

        var next = SelectNextAbility();
        if (next is null) return dealt;

        // Shadow Trance (Nightfall): Zauber mit Cast-Zeit wird sofort gewirkt
        if (next.HasCastTime && !Player.Buffs.ConsumeInstantCast(next.Name))
        {
            CurrentCast  = next;
            CastElapsed  = 0;
            CastDuration = next.CastTime / attackSpeed;
            LastLog = $"Casting {next.Name}…";
            return dealt;
        }

        if (!PayCastCost(next)) return dealt;
        dealt += ExecuteAbility(next);
        Player.Cooldowns.StartCooldown(next.Id, next.Cooldown);
        GcdRemaining = GlobalCooldown;
        return dealt;
    }

    /// <summary>Cast abgeschlossen: Mana erst jetzt abziehen, dann Effekt auslösen.</summary>
    private int FinishCast(Ability ability)
    {
        if (!PayCastCost(ability))
        {
            LastLog = $"{ability.Name} failed – not enough {ShapeshiftForms.ResourceName(Player.Resource)}";
            return 0;
        }
        int dealt = ExecuteAbility(ability);
        Player.Cooldowns.StartCooldown(ability.Id, ability.Cooldown);
        return dealt;
    }

    /// <summary>
    /// Wählt die nächste Fähigkeit: zuerst Spieler-Buffs, dann Heilung/Schild,
    /// danach die Schadensfähigkeit mit dem höchsten geschätzten DPS.
    /// </summary>
    private Ability? SelectNextAbility()
    {
        Ability? best = null;
        double bestDps = -1;
        foreach (var ability in Player.SortedEquippedAbilities)
        {
            if (!CanStartAbility(ability)) continue;
            // Liste ist nach Gruppe sortiert → erste bereite Buff/Heal-Fähigkeit gewinnt
            if (ability.CastPriorityGroup < 2) return ability;

            double dps = CombatEngine.EstimateAbilityDamage(ability, Player.Stats) * SelectionDamageFactor(ability)
                         / Math.Max(ability.CastTime, GlobalCooldown);
            if (dps > bestDps)
            {
                best    = ability;
                bestDps = dps;
            }
        }
        return best;
    }

    /// <summary>Alle Bedingungen für den Start einer Fähigkeit inkl. ausreichender Ressource.</summary>
    private bool CanStartAbility(Ability ability)
    {
        if (!IsReadyToCast(ability)) return false;
        if (!Player.CanUseInCurrentForm(ability)) return false;
        if (ability.IsPetSummon && Pet is not null) return false;   // Pet lebt bereits
        if (ability.CastWhenManaBelowPct > 0
            && Player.CurrentMana >= Player.Stats.MaxMana * ability.CastWhenManaBelowPct) return false;
        if (ability.IsHealing && !PlayerNeedsHeal) return false;
        if (!ability.IsHealing && ability.ManaCost > 0 && IsSavingManaForHeal()) return false;
        // Life Tap: nur bei niedrigem Mana und ausreichend HP
        if (ability.IsLifeTap
            && (Player.CurrentMana >= Player.Stats.MaxMana / 2 || PlayerNeedsHeal || Player.CurrentHp <= ability.LifeTapHpCost))
            return false;
        return Player.CurrentMana >= CastCost(ability);
    }
}
