using Models;

// Effect resolution of individual abilities (resource, cooldown and GCD are handled by IdleGameEngine.Casting.cs).
public partial class IdleGameEngine
{
    /// <summary>Triggers the effect of an ability. Returns the damage dealt to the enemy.</summary>
    private int ExecuteAbility(Ability ability)
    {
        var enemy = CurrentEnemy!;
        int dealt = 0;
        AbilityTriggered?.Invoke(ability);

        // Life Tap: HP in Mana umwandeln
        if (ability.IsLifeTap)
        {
            Player.TakeDamage(ability.LifeTapHpCost); // Absorb return value ignored (no shield meter for Life Tap)
            Player.RegenMana(ability.LifeTapManaGain);
            CombatTextRaised?.Invoke(new CombatText(CombatTextTarget.Player, CombatTextKind.AutoAttack, ability.LifeTapHpCost));
            LastLog = $"✦ {ability.Name} → -{ability.LifeTapHpCost} HP, +{ability.LifeTapManaGain} Mana";
            Player.TriggerAbilityUsed(ability.Name, 0);
            return 0;
        }

        // Summon or revive a pet (Hunter pets, Warlock demons)
        if (ability.IsPetSummon)
        {
            SummonPet(ability);
            Player.TriggerAbilityUsed(ability.Name, 0);
            return 0;
        }

        // Talent and form bonuses (e.g. Improved Moonfire, Moonkin Form)
        var effective = Player.GetEffectiveAbility(ability);

        // Damaging abilities with a buff (Ghostly Strike) go through the damage branch
        if (ability.IsBuff && !ability.IsDamage)
        {
            Player.Buffs.Apply(effective, Player.Stats.MaxMana, Player.Stats.MaxHp);
            LastLog = $"✦ {ability.Name} active ({ability.BuffDuration:0}s)";
            if (ability.IsHeal)   // z. B. Regrowth: Direktheilung + HoT
                ApplyDirectHeal(ability, effective.Heal);
        }
        else if (ability.IsDamage)
        {
            double abilityMult = Player.Buffs.DamageMultiplier
                * (ability.AddsWeaponDamage ? Player.Buffs.WeaponDamageMultiplier : 1.0)
                * enemy.Debuffs.DamageTakenMultiplier
                * Player.Buffs.ConsumeEmpower(ability.Name);   // z. B. Mongoose Bite → Raptor Strike
            // Cold Blood: guaranteed crit for all hits of this ability
            double extraCrit = Player.GetAbilityCritBonus(ability) + (Player.Buffs.ConsumeGuaranteedCrit() ? 1.0 : 0.0);
            // Synergy with a debuff on the target (Lava Burst → Flame Shock, Mutilate → Deadly Poison, Shatter)
            if (IsSynergyActive(ability))
            {
                abilityMult *= 1.0 + ability.SynergyDamageBonus;
                extraCrit   += ability.SynergyCritBonus;
            }
            // Multiple hits (Chain Lightning): each hit rolls its own crit
            int  abilityDmg  = 0;
            bool abilityCrit = false;
            for (int hit = 0; hit < ability.HitCount && enemy.IsAlive; hit++)
            {
                var (hitDmg, hitCrit) = RollPlayerCrit(
                    CombatEngine.CalculateAbilityDamage(effective, Player.Stats, _rng, abilityMult),
                    extraCrit);
                Meter.RecordAbility(ability, CombatActionKind.Damage, enemy.TakeDamage(hitDmg));
                CombatTextRaised?.Invoke(new CombatText(CombatTextTarget.Enemy, CombatTextKind.Ability, hitDmg, hitCrit));
                abilityDmg  += hitDmg;
                abilityCrit |= hitCrit;
            }
            dealt += abilityDmg;
            LastLog = ability.HitCount > 1
                ? $"⚔ {ability.Name} → {abilityDmg} DMG ({ability.HitCount} hits)!"
                : $"⚔ {ability.Name} → {abilityDmg} DMG!";

            if (ability.IsDebuff)   // e.g. Moonfire: direct hit + DoT
                enemy.Debuffs.Apply(effective);

            if (ability.IsHeal)     // e.g. Holy Nova: damage + healing at the same time
                ApplyDirectHeal(ability, effective.Heal);

            if (ability.IsBuff)     // z. B. Ghostly Strike: Schaden + Schild
                Player.Buffs.Apply(effective, Player.Stats.MaxMana, Player.Stats.MaxHp);

            ApplyDamageTalentProcs(ability, abilityDmg, abilityCrit);
        }
        else if (ability.IsDebuff)  // reiner Debuff (Hunter's Mark, Curse of Agony)
        {
            enemy.Debuffs.Apply(effective);
            LastLog = $"✦ {ability.Name} on {enemy.Name} ({ability.DebuffDuration:0}s)";
        }
        else
        {
            ApplyDirectHeal(ability, effective.Heal);
        }

        ApplyAbilityAfterEffects(ability);
        Player.TriggerAbilityUsed(ability.Name, ability.IsDamage ? ability.Damage : ability.Heal);
        return dealt;
    }

    /// <summary>Direct heal incl. healing multiplier and crit.</summary>
    private void ApplyDirectHeal(Ability ability, int baseHeal)
    {
        int scaledHeal = (int)Math.Round(baseHeal * HealingMultiplier);
        var (healAmount, healCrit) = RollPlayerCrit(scaledHeal);
        int hpBefore = Player.CurrentHp;
        Player.RegenHp(healAmount);
        int healed = Player.CurrentHp - hpBefore;
        Meter.RecordAbility(ability, CombatActionKind.Heal, healed);
        CombatTextRaised?.Invoke(new CombatText(CombatTextTarget.Player, CombatTextKind.Heal, healAmount, healCrit));
        LastLog = $"✚ {ability.Name} → +{healed} HP";
    }

    /// <summary>Talent effects attached to specific damage spells (Smite heal, Periodic Fire, Ignite).</summary>
    private void ApplyDamageTalentProcs(Ability ability, int abilityDmg, bool abilityCrit)
    {
        var enemy = CurrentEnemy!;

        // SmiteHeal: Smite heals the Priest
        if (ability.Id.StartsWith("Smite"))
        {
            double smiteHealBonus = Player.TalentState.GetTotalBonus(Player.ActiveClass, TalentEffectType.SmiteHeal);
            if (smiteHealBonus > 0)
            {
                int sh = (int)Math.Round(smiteHealBonus * HealingMultiplier);
                var (smHealed, _) = RollPlayerCrit(sh);
                int hpSmBefore = Player.CurrentHp;
                Player.RegenHp(smHealed);
                CombatTextRaised?.Invoke(new CombatText(CombatTextTarget.Player, CombatTextKind.Heal, Player.CurrentHp - hpSmBefore, false));
            }
        }

        // Periodic Fire: Fireball DoT from a talent (share of the Fireball hit)
        double dotPct = Player.TalentState.GetTotalBonus(Player.ActiveClass, TalentEffectType.FireballDot);
        if (dotPct > 0 && ability.Id.StartsWith("Fireball"))
        {
            var periodicFire = new Ability
            {
                Id                   = "talent_periodic_fire",
                Name                 = "Periodic Fire",
                DebuffDuration       = 5,
                DebuffDotDamage      = (int)Math.Max(1, Math.Round(abilityDmg * dotPct)),
                DebuffDotTickInterval = 5.0,
            };
            enemy.Debuffs.Apply(periodicFire);
        }

        // Ignite: fire spell crit (Fireball, Fire Blast, Flamestrike) triggers a DoT (40 % of the damage over 3 ticks)
        if (abilityCrit
            && (ability.Id.StartsWith("Fireball") || ability.Id.StartsWith("Fire_Blast") || ability.Id.StartsWith("Flamestrike"))
            && Player.TalentState.GetTotalBonus(Player.ActiveClass, TalentEffectType.GrantIgnite) > 0
            && enemy.IsAlive)
        {
            int ignitePerTick = (int)Math.Max(1, Math.Round(abilityDmg * 0.40 / 3.0));
            var igniteDebuff = new Ability
            {
                Id                    = "talent_ignite",
                Name                  = "Ignite",
                DebuffDuration        = 3,
                DebuffDotDamage       = ignitePerTick,
                DebuffDotTickInterval = 1.0,
            };
            enemy.Debuffs.ApplyIgnite(igniteDebuff);
        }
    }
}
