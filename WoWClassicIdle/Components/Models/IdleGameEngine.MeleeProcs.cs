using Models;

// Nahkampf-Procs rund um die Auto-Attack: Flurry (Tempo nach Crit) und Windfury Weapon (Zusatzschläge).
public partial class IdleGameEngine
{
    /// <summary>Anzahl Schläge, die nach einem Nahkampf-Crit beschleunigt sind (wie Classic-Flurry).</summary>
    private const int FlurryChargesPerCrit = 3;

    private int _flurryCharges;

    private double FlurryBonus => Player.TalentState.GetTotalBonus(Player.ActiveClass, TalentEffectType.FlurryAttackSpeed);

    /// <summary>Tempo-Multiplikator durch Flurry (1.0 = kein Bonus aktiv).</summary>
    private double FlurryAttackSpeedMultiplier => _flurryCharges > 0 ? 1.0 + FlurryBonus : 1.0;

    /// <summary>Nach jedem regulären Schlag: eine Ladung verbrauchen, bei Crit neu aufladen.</summary>
    private void UpdateFlurry(bool isCrit)
    {
        if (_flurryCharges > 0) _flurryCharges--;
        if (isCrit && FlurryBonus > 0) _flurryCharges = FlurryChargesPerCrit;
    }

    /// <summary>
    /// Windfury Weapon: mit <see cref="Ability.BuffExtraAttackChance"/> folgen dem Schlag
    /// <see cref="Ability.BuffExtraAttacks"/> zusätzliche Auto-Attacks. Gibt den Zusatzschaden zurück.
    /// </summary>
    private int TryWindfuryAttacks(double autoMult)
    {
        var source = Player.Buffs.GetExtraAttackSource();
        if (source is null || !CurrentEnemy!.IsAlive || _rng.NextDouble() >= source.BuffExtraAttackChance)
            return 0;

        int  total   = 0;
        bool anyCrit = false;
        for (int i = 0; i < source.BuffExtraAttacks && CurrentEnemy.IsAlive; i++)
        {
            var (dmg, crit) = RollPlayerCrit(CombatEngine.CalculateAutoAttack(Player.Stats, _rng, autoMult));
            Meter.RecordAbility(source, CombatActionKind.Damage, CurrentEnemy.TakeDamage(dmg));
            CombatTextRaised?.Invoke(new CombatText(CombatTextTarget.Enemy, CombatTextKind.Ability, dmg, crit));
            total   += dmg;
            anyCrit |= crit;
        }

        // Crit eines Zusatzschlags lädt Flurry auf, verbraucht aber keine Ladung
        if (anyCrit && FlurryBonus > 0) _flurryCharges = FlurryChargesPerCrit;
        LastLog = $"⚔ {source.Name} → +{total} DMG ({source.BuffExtraAttacks} extra attacks)!";
        return total;
    }
}
