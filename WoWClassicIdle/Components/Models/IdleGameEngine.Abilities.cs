using Models;

// Effekt-Auflösung einzelner Fähigkeiten (Ressource, Cooldown und GCD regelt IdleGameEngine.Casting.cs).
public partial class IdleGameEngine
{
    /// <summary>Löst den Effekt einer Fähigkeit aus. Gibt den am Gegner verursachten Schaden zurück.</summary>
    private int ExecuteAbility(Ability ability)
    {
        var enemy = CurrentEnemy!;
        int dealt = 0;
        AbilityTriggered?.Invoke(ability);

        // Life Tap: HP in Mana umwandeln
        if (ability.IsLifeTap)
        {
            Player.TakeDamage(ability.LifeTapHpCost); // Absorb-Rückgabe ignoriert (kein Schild-Meter für Life Tap)
            Player.RegenMana(ability.LifeTapManaGain);
            CombatTextRaised?.Invoke(new CombatText(CombatTextTarget.Player, CombatTextKind.AutoAttack, ability.LifeTapHpCost));
            LastLog = $"✦ {ability.Name} → -{ability.LifeTapHpCost} HP, +{ability.LifeTapManaGain} Mana";
            Player.TriggerAbilityUsed(ability.Name, 0);
            return 0;
        }

        if (ability.IsBuff)
        {
            Player.Buffs.Apply(Player.GetEffectiveAbility(ability), Player.Stats.MaxMana, Player.Stats.MaxHp);
            LastLog = $"✦ {ability.Name} active ({ability.BuffDuration:0}s)";
        }
        else if (ability.IsDamage)
        {
            double abilityMult = Player.Buffs.DamageMultiplier
                * (ability.AddsWeaponDamage ? Player.Buffs.WeaponDamageMultiplier : 1.0)
                * enemy.Debuffs.DamageTakenMultiplier;
            var (abilityDmg, abilityCrit) = RollPlayerCrit(
                CombatEngine.CalculateAbilityDamage(ability, Player.Stats, _rng, abilityMult),
                Player.GetAbilityCritBonus(ability));
            enemy.TakeDamage(abilityDmg);
            Meter.RecordAbility(ability, CombatActionKind.Damage, abilityDmg);
            CombatTextRaised?.Invoke(new CombatText(CombatTextTarget.Enemy, CombatTextKind.Ability, abilityDmg, abilityCrit));
            dealt += abilityDmg;
            LastLog = $"⚔ {ability.Name} → {abilityDmg} DMG!";

            if (ability.IsDebuff)   // e.g. Moonfire: direkter Treffer + DoT
                enemy.Debuffs.Apply(Player.GetEffectiveAbility(ability));

            ApplyDamageTalentProcs(ability, abilityDmg, abilityCrit);
        }
        else if (ability.IsDebuff)  // reiner Debuff (Hunter's Mark, Curse of Agony)
        {
            enemy.Debuffs.Apply(Player.GetEffectiveAbility(ability));
            LastLog = $"✦ {ability.Name} on {enemy.Name} ({ability.DebuffDuration:0}s)";
        }
        else
        {
            int scaledHeal = (int)Math.Round(ability.Heal * Player.Stats.HealingMultiplier);
            var (healAmount, healCrit) = RollPlayerCrit(scaledHeal);
            int hpBefore = Player.CurrentHp;
            Player.RegenHp(healAmount);
            int healed = Player.CurrentHp - hpBefore;
            Meter.RecordAbility(ability, CombatActionKind.Heal, healed);
            CombatTextRaised?.Invoke(new CombatText(CombatTextTarget.Player, CombatTextKind.Heal, healAmount, healCrit));
            LastLog = $"✚ {ability.Name} → +{healed} HP";
        }

        Player.TriggerAbilityUsed(ability.Name, ability.IsDamage ? ability.Damage : ability.Heal);
        return dealt;
    }

    /// <summary>Talent-Effekte, die an bestimmten Schadenszaubern hängen (Smite-Heal, Periodic Fire, Ignite).</summary>
    private void ApplyDamageTalentProcs(Ability ability, int abilityDmg, bool abilityCrit)
    {
        var enemy = CurrentEnemy!;

        // SmiteHeal: Smite heilt den Priester
        if (ability.Id.StartsWith("Smite"))
        {
            double smiteHealBonus = Player.TalentState.GetTotalBonus(Player.ActiveClass, TalentEffectType.SmiteHeal);
            if (smiteHealBonus > 0)
            {
                int sh = (int)Math.Round(smiteHealBonus * Player.Stats.HealingMultiplier);
                var (smHealed, _) = RollPlayerCrit(sh);
                int hpSmBefore = Player.CurrentHp;
                Player.RegenHp(smHealed);
                CombatTextRaised?.Invoke(new CombatText(CombatTextTarget.Player, CombatTextKind.Heal, Player.CurrentHp - hpSmBefore, false));
            }
        }

        // Periodic Fire: Fireball-DoT durch Talent
        double dotDmg = Player.TalentState.GetTotalBonus(Player.ActiveClass, TalentEffectType.FireballDot);
        if (dotDmg > 0 && ability.Id.StartsWith("Fireball"))
        {
            var periodicFire = new Ability
            {
                Id                   = "talent_periodic_fire",
                Name                 = "Periodic Fire",
                DebuffDuration       = 5,
                DebuffDotDamage      = (int)Math.Max(1, dotDmg),
                DebuffDotTickInterval = 5.0,
            };
            enemy.Debuffs.Apply(periodicFire);
        }

        // Ignite: Feuerzauber-Crit löst DoT aus (15 % des Schadens über 3 Ticks)
        if (abilityCrit
            && (ability.Id.StartsWith("Fireball") || ability.Id.StartsWith("Fire_Blast"))
            && Player.TalentState.GetTotalBonus(Player.ActiveClass, TalentEffectType.GrantIgnite) > 0
            && enemy.IsAlive)
        {
            int ignitePerTick = (int)Math.Max(1, Math.Round(abilityDmg * 0.15 / 3.0));
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
