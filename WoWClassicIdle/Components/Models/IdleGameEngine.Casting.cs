using Models;

// Cast times, global cooldown and selection of the next ability.
public partial class IdleGameEngine
{
    /// <summary>Global cooldown in seconds, triggered by every ability without cast time.</summary>
    public const double GlobalCooldown = 0.5;

    /// <summary>Ability currently being cast (null = no cast).</summary>
    public Ability? CurrentCast { get; private set; }

    /// <summary>Bisher vergangene Cast-Zeit in Sekunden.</summary>
    public double CastElapsed { get; private set; }

    /// <summary>Total duration of the current cast (cast time, shortened by attack speed).</summary>
    public double CastDuration { get; private set; }

    /// <summary>Verbleibender Global Cooldown in Sekunden (0 = bereit).</summary>
    public double GcdRemaining { get; private set; }

    public double CastProgress => CurrentCast is null || CastDuration <= 0 ? 0 : Math.Min(1, CastElapsed / CastDuration);

    /// <summary>Aktuelles Auto-Attack-Intervall in Sekunden (inkl. Attack-Speed).</summary>
    public double SwingInterval { get; private set; } = 1;

    /// <summary>Progress of the swing timer (0..1).</summary>
    public double SwingProgress => SwingInterval <= 0 ? 0 : Math.Clamp(_playerAttackAcc / SwingInterval, 0, 1);

    /// <summary>Current attack interval of the enemy in seconds (incl. slow debuffs).</summary>
    public double EnemySwingInterval { get; private set; } = 1;

    /// <summary>Progress of the enemy swing timer (0..1).</summary>
    public double EnemySwingProgress => EnemySwingInterval <= 0 ? 0 : Math.Clamp(_enemyAttackAcc / EnemySwingInterval, 0, 1);

    /// <summary>Spells with mana cost take precedence: the auto-attack waits during their cast.</summary>
    public bool IsSwingHeld => CurrentCast is { ManaCost: > 0 };

    /// <summary>Cancels a running cast (no mana spent, no cooldown).</summary>
    private void CancelCast()
    {
        CurrentCast  = null;
        CastElapsed  = 0;
        CastDuration = 0;
    }

    /// <summary>Reset cast and GCD (death, character switch).</summary>
    private void ResetCastState()
    {
        CancelCast();
        GcdRemaining = 0;
    }

    /// <summary>
    /// Advances the running cast and GCD and starts the next ability if applicable.
    /// Returns the damage dealt to the enemy in the process.
    /// </summary>
    private int UpdateAbilities(double dt, double attackSpeed)
    {
        int dealt = 0;
        // Epsilon: 0.5 - 5 × 0.1 in double is slightly > 0 and would extend the GCD by one step
        GcdRemaining = GcdRemaining - dt > 1e-9 ? GcdRemaining - dt : 0;

        if (CurrentCast is { } casting)
        {
            CastElapsed += dt;
            if (CastDuration - CastElapsed > 1e-9) return 0;

            CancelCast();
            dealt += FinishCast(casting);
            if (!CurrentEnemy!.IsAlive) return dealt;
            // No GCD after a cast → the next ability may start immediately
        }

        if (GcdRemaining > 0) return dealt;

        var next = SelectNextAbility();
        if (next is null) return dealt;

        // Shadow Trance (Nightfall): a spell with cast time is cast instantly
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

    /// <summary>Cast completed: only now deduct mana, then trigger the effect.</summary>
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
    /// Picks the next ability: player buffs first, then heal/shield,
    /// then the damaging ability with the highest estimated DPS.
    /// </summary>
    private Ability? SelectNextAbility()
    {
        Ability? best = null;
        double bestDps = -1;
        foreach (var ability in Player.SortedEquippedAbilities)
        {
            if (!CanStartAbility(ability)) continue;
            // List is sorted by group → the first ready buff/heal ability wins
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

    /// <summary>All conditions for starting an ability incl. sufficient resource.</summary>
    private bool CanStartAbility(Ability ability)
    {
        if (!IsReadyToCast(ability)) return false;
        if (!Player.CanUseInCurrentForm(ability)) return false;
        if (ability.IsPetSummon && Pet is not null) return false;   // Pet is already alive
        if (ability.CastWhenManaBelowPct > 0
            && Player.CurrentMana >= Player.Stats.MaxMana * ability.CastWhenManaBelowPct) return false;
        if (ability.IsHealing && !PlayerNeedsHeal) return false;
        if (!ability.IsHealing && ability.ManaCost > 0 && IsSavingManaForHeal()) return false;
        // Life Tap: only at low mana and with sufficient HP
        if (ability.IsLifeTap
            && (Player.CurrentMana >= Player.Stats.MaxMana / 2 || PlayerNeedsHeal || Player.CurrentHp <= ability.LifeTapHpCost))
            return false;
        return Player.CurrentMana >= CastCost(ability);
    }
}
