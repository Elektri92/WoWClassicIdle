using Models;

// Melee procs around the auto-attack: Flurry (haste after crit) and Windfury Weapon (extra swings).
public partial class IdleGameEngine
{
    /// <summary>Number of swings that are sped up after a melee crit (like Classic Flurry).</summary>
    private const int FlurryChargesPerCrit = 3;

    private int _flurryCharges;

    // Shaman talent + Warrior Flurry (only in Berserker Stance)
    private double FlurryBonus =>
        Player.TalentState.GetTotalBonus(Player.ActiveClass, TalentEffectType.FlurryAttackSpeed)
        + (Player.ActiveForm == DruidForm.BerserkerStance
            ? Player.TalentState.GetTotalBonus(Player.ActiveClass, TalentEffectType.BerserkerFlurry)
            : 0);

    /// <summary>Haste multiplier from Flurry (1.0 = no bonus active).</summary>
    private double FlurryAttackSpeedMultiplier => _flurryCharges > 0 ? 1.0 + FlurryBonus : 1.0;

    /// <summary>After every regular swing: consume one charge, recharge on crit.</summary>
    private void UpdateFlurry(bool isCrit)
    {
        if (_flurryCharges > 0) _flurryCharges--;
        if (isCrit && FlurryBonus > 0) _flurryCharges = FlurryChargesPerCrit;
    }

    /// <summary>Passive proc (Deadly Poison) – with talent bonuses (proc chance, stacks, tick rate).</summary>
    private void TryProcPassive(Ability passive)
    {
        var effective = Player.GetEffectiveAbility(passive);
        if (CurrentEnemy!.IsAlive && _rng.NextDouble() < effective.ProcChance)
            CurrentEnemy.Debuffs.ApplyStack(effective);
    }

    /// <summary>
    /// Windfury Weapon: with <see cref="Ability.BuffExtraAttackChance"/> the swing is followed by
    /// <see cref="Ability.BuffExtraAttacks"/> additional auto-attacks. Returns the extra damage.
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

        // A crit of an extra swing charges Flurry but consumes no charge
        if (anyCrit && FlurryBonus > 0) _flurryCharges = FlurryChargesPerCrit;
        LastLog = $"⚔ {source.Name} → +{total} DMG ({source.BuffExtraAttacks} extra attacks)!";
        return total;
    }
}
