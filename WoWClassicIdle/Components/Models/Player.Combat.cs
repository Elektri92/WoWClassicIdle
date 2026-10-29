// Models/Player.Combat.cs
namespace Models
{
    // Create new systems as their own partial file; keep files below 300 lines where possible.
    public partial class Player
    {
        /// <summary>
        /// Sets the level of the active class directly (e.g. for debug purposes).
        /// </summary>
        public void SetLevel(int level)
        {
            ActiveProgress.SetLevel(level);
            RecalculateStats();
        }

        /// <summary>Returns a talent- and form-modified copy of the ability (incl. mana cost).</summary>
        public Ability GetEffectiveAbility(Ability ability) =>
            ApplyFormBonus(ApplyExtendedTalentBonus(GetTalentModifiedAbility(ability)));

        private Ability GetTalentModifiedAbility(Ability ability)
        {
            if (ability.Id.StartsWith("Arcane_Intellect"))
            {
                double bonus = TalentState.GetTotalBonus(ActiveClass, TalentEffectType.ArcaneIntellectBonus);
                if (bonus > 0)
                    return ability with
                    {
                        BuffIntellectBonus = (int)Math.Round(ability.BuffIntellectBonus * (1.0 + bonus)),
                        BuffSpiritBonus    = (int)Math.Round(ability.BuffSpiritBonus    * (1.0 + bonus)),
                    };
            }
            if (ability.Id.StartsWith("Conjure_Water"))
            {
                double bonus = TalentState.GetTotalBonus(ActiveClass, TalentEffectType.ConjureWaterManaRegen);
                if (bonus > 0)
                    return ability with { MotManaRestorePct = ability.MotManaRestorePct * (1.0 + bonus) };
            }
            if (ability.Id.StartsWith("Fireball"))
            {
                int reduction = (int)TalentState.GetTotalBonus(ActiveClass, TalentEffectType.FireballManaCost);
                if (reduction > 0)
                    return ability with { ManaCost = Math.Max(0, ability.ManaCost - reduction) };
            }
            if (ability.Id.StartsWith("Frostbolt"))
            {
                double speedBonus    = TalentState.GetTotalBonus(ActiveClass, TalentEffectType.FrostboltAttackSpeedDebuff);
                int    costReduction = (int)TalentState.GetTotalBonus(ActiveClass, TalentEffectType.FrostboltManaCost);
                double dmgBonus      = TalentState.GetTotalBonus(ActiveClass, TalentEffectType.FrostboltDamage);
                if (speedBonus > 0 || costReduction > 0 || dmgBonus > 0)
                    return ability with
                    {
                        Damage     = dmgBonus > 0 ? (int)Math.Round(ability.Damage * (1.0 + dmgBonus)) : ability.Damage,
                        ManaCost   = Math.Max(0, ability.ManaCost - costReduction),
                        DebuffAttackSpeedReduction = ability.DebuffAttackSpeedReduction + speedBonus,
                    };
            }
            if (ability.Id.StartsWith("Arcane_Missiles"))
            {
                double bonus = TalentState.GetTotalBonus(ActiveClass, TalentEffectType.ArcaneMissilesDamage);
                if (bonus > 0)
                    return ability.WithDamageBonus(bonus);   // Arcane Missiles is now direct damage, no longer a DoT
            }
            if (ability.Id.StartsWith("Fire_Blast"))
            {
                int red = (int)TalentState.GetTotalBonus(ActiveClass, TalentEffectType.FireBlastManaCost);
                if (red > 0)
                    return ability with { ManaCost = Math.Max(0, ability.ManaCost - red) };
            }
            if (ability.Id.StartsWith("Heroic_Strike"))
            {
                double dmgBonus = TalentState.GetTotalBonus(ActiveClass, TalentEffectType.HeroicStrikeDamage);
                int    costRed  = (int)TalentState.GetTotalBonus(ActiveClass, TalentEffectType.HeroicStrikeManaCost);
                if (dmgBonus > 0 || costRed > 0)
                    return ability.WithDamageBonus(dmgBonus) with
                    {
                        ManaCost = Math.Max(0, ability.ManaCost - costRed),
                    };
            }
            if (ability.Id.StartsWith("Execute"))
            {
                double cdRed = TalentState.GetTotalBonus(ActiveClass, TalentEffectType.ExecuteCooldown);
                if (cdRed > 0)
                    return ability with { Cooldown = Math.Max(1, ability.Cooldown - cdRed) };
            }
            if (ability.Id.StartsWith("Thunderclap"))
            {
                double bonus = TalentState.GetTotalBonus(ActiveClass, TalentEffectType.ThunderclapBonus);
                if (bonus > 0)
                    return ability with { DebuffAttackSpeedReduction = ability.DebuffAttackSpeedReduction + bonus };
            }
            if (ability.Id.StartsWith("Shadow_Word_Pain"))
            {
                double bonus = TalentState.GetTotalBonus(ActiveClass, TalentEffectType.ShadowWordPainDamage);
                if (bonus > 0)
                    return ability with { DebuffDotDamage = (int)Math.Round(ability.DebuffDotDamage * (1.0 + bonus)) };
            }
            if (ability.Id.StartsWith("Sinester_Strike") || ability.Id.StartsWith("Sinister_Strike"))
            {
                double bonus = TalentState.GetTotalBonus(ActiveClass, TalentEffectType.SinisterStrikeDamage);
                if (bonus > 0)
                    return ability.WithDamageBonus(bonus);
            }
            if (ability.Id.StartsWith("Deadly_Poison"))
            {
                double bonus = TalentState.GetTotalBonus(ActiveClass, TalentEffectType.DeadlyPoisonProcChance);
                if (bonus > 0)
                    return ability with { ProcChance = ability.ProcChance + bonus };
            }
            if (ability.Id.StartsWith("Kidney_Shot"))
            {
                double bonus = TalentState.GetTotalBonus(ActiveClass, TalentEffectType.KidneyShotDuration);
                if (bonus > 0)
                    return ability with { DebuffDuration = ability.DebuffDuration * (1.0 + bonus) };
            }
            if (ability.Id.StartsWith("Holy_Light"))
            {
                double pct = TalentState.GetTotalBonus(ActiveClass, TalentEffectType.HolyLightManaCost);
                if (pct > 0)
                    return ability with { ManaCost = Math.Max(0, (int)Math.Round(ability.ManaCost * (1.0 - pct))) };
            }
            if (ability.Id.StartsWith("Holy_Strike"))
            {
                double pct = TalentState.GetTotalBonus(ActiveClass, TalentEffectType.HolyStrikeManaCost);
                if (pct > 0)
                    return ability with { ManaCost = Math.Max(0, (int)Math.Round(ability.ManaCost * (1.0 - pct))) };
            }
            if (ability.Id.StartsWith("Divine_Shield"))
            {
                double bonus = TalentState.GetTotalBonus(ActiveClass, TalentEffectType.DivineshieldDuration);
                if (bonus > 0)
                    return ability with { BuffDuration = ability.BuffDuration + bonus };
            }
            if (ability.Id.StartsWith("Raptor_Strike"))
            {
                int red = (int)TalentState.GetTotalBonus(ActiveClass, TalentEffectType.RaptorStrikeManaCost);
                if (red > 0)
                    return ability with { ManaCost = Math.Max(0, ability.ManaCost - red) };
            }
            if (ability.Id.StartsWith("Serpent_Sting"))
            {
                int red = (int)TalentState.GetTotalBonus(ActiveClass, TalentEffectType.SerpentStingManaCost);
                if (red > 0)
                    return ability with { ManaCost = Math.Max(0, ability.ManaCost - red) };
            }
            if (ability.Id.StartsWith("Hunters_Mark"))
            {
                double bonus = TalentState.GetTotalBonus(ActiveClass, TalentEffectType.HuntersMarkBonus);
                if (bonus > 0)
                    return ability with { DebuffDamageTakenBonus = ability.DebuffDamageTakenBonus + bonus };
            }
            if (ability.Id.StartsWith("Curse_of_Agony"))
            {
                double bonus = TalentState.GetTotalBonus(ActiveClass, TalentEffectType.CurseOfAgonyDuration);
                if (bonus > 0)
                    return ability with { DebuffDuration = ability.DebuffDuration + bonus };
            }
            if (ability.Id.StartsWith("Drain_Life"))
            {
                int bonus = (int)TalentState.GetTotalBonus(ActiveClass, TalentEffectType.DrainLifeTickDamage);
                if (bonus > 0)
                    return ability with { DebuffDotDamage = ability.DebuffDotDamage + bonus };
            }
            if (ability.Id.StartsWith("Life_Tap"))
            {
                int red = (int)TalentState.GetTotalBonus(ActiveClass, TalentEffectType.LifeTapHpCostReduction);
                if (red > 0)
                    return ability with { LifeTapHpCost = Math.Max(1, ability.LifeTapHpCost - red) };
            }
            if (ability.Id.StartsWith("Shdow_bolt") || ability.Id.StartsWith("Shadow_Bolt"))
            {
                int red = (int)TalentState.GetTotalBonus(ActiveClass, TalentEffectType.ShadowBoltManaCost);
                if (red > 0)
                    return ability with { ManaCost = Math.Max(0, ability.ManaCost - red) };
            }
            if (ability.Id.StartsWith("Lightning_bolt") || ability.Id.StartsWith("Lightning_Bolt"))
            {
                int dmgBonus = (int)TalentState.GetTotalBonus(ActiveClass, TalentEffectType.LightningBoltDamage);
                int costRed  = (int)TalentState.GetTotalBonus(ActiveClass, TalentEffectType.LightningBoltManaCost);
                if (dmgBonus > 0 || costRed > 0)
                    return ability with
                    {
                        Damage   = ability.Damage + dmgBonus,
                        ManaCost = Math.Max(0, ability.ManaCost - costRed),
                    };
            }
            if (ability.Id.StartsWith("Lightning_Shield"))
            {
                double bonus = TalentState.GetTotalBonus(ActiveClass, TalentEffectType.LightningShieldDamage);
                if (bonus > 0)
                    return ability with { BuffRetaliationDamage = (int)Math.Round(ability.BuffRetaliationDamage * (1.0 + bonus)) };
            }
            if (ability.Id.StartsWith("Flame_Shock"))
            {
                double bonus = TalentState.GetTotalBonus(ActiveClass, TalentEffectType.FlameShockDamage);
                if (bonus > 0)
                    return ability with
                    {
                        Damage          = (int)Math.Round(ability.Damage * (1.0 + bonus)),
                        DebuffDotDamage = (int)Math.Round(ability.DebuffDotDamage * (1.0 + bonus)),
                    };
            }
            if (ability.Id.StartsWith("Healing_Wave"))
            {
                double bonus = TalentState.GetTotalBonus(ActiveClass, TalentEffectType.HealingWaveHealing);
                if (bonus > 0)
                    return ability with { Heal = (int)Math.Round(ability.Heal * (1.0 + bonus)) };
            }
            if (ability.Id.StartsWith("Earthbind_Totem"))
            {
                double red = TalentState.GetTotalBonus(ActiveClass, TalentEffectType.EarthbindCooldown);
                if (red > 0)
                    return ability with { Cooldown = Math.Max(1, ability.Cooldown - red) };
            }
            if (ability.Id.StartsWith("Moonfire"))
            {
                double bonus = TalentState.GetTotalBonus(ActiveClass, TalentEffectType.MoonfireDamage);
                if (bonus > 0)
                    return ability with
                    {
                        Damage          = (int)Math.Round(ability.Damage * (1.0 + bonus)),
                        DebuffDotDamage = (int)Math.Round(ability.DebuffDotDamage * (1.0 + bonus)),
                    };
            }
            if (ability.Id.StartsWith("Maul"))
            {
                double bonus = TalentState.GetTotalBonus(ActiveClass, TalentEffectType.MaulDamage);
                if (bonus > 0)
                    return ability.WithDamageBonus(bonus);
            }
            if (ability.Id.StartsWith("Claw"))
            {
                double bonus = TalentState.GetTotalBonus(ActiveClass, TalentEffectType.ClawDamage);
                if (bonus > 0)
                    return ability.WithDamageBonus(bonus);
            }
            if (ability.Id.StartsWith("Smite"))
            {
                double bonus = TalentState.GetTotalBonus(ActiveClass, TalentEffectType.SmiteDamage);
                if (bonus > 0)
                    return ability with { Damage = (int)Math.Round(ability.Damage * (1.0 + bonus)) };
            }
            if (ability.Id.StartsWith("Power_Word_Fortitude"))
            {
                double bonus = TalentState.GetTotalBonus(ActiveClass, TalentEffectType.PowerWordFortitudeBonus);
                if (bonus > 0)
                    return ability with { BuffDamageTakenReduction = ability.BuffDamageTakenReduction + bonus };
            }
            if (ability.Id.StartsWith("Renew"))
            {
                double healBonus = TalentState.GetTotalBonus(ActiveClass, TalentEffectType.RenewHealing);
                double costPct   = TalentState.GetTotalBonus(ActiveClass, TalentEffectType.RenewManaCost);
                if (healBonus > 0 || costPct > 0)
                    return ability with
                    {
                        HotTotalHeal = (int)Math.Round(ability.HotTotalHeal * (1.0 + healBonus)),
                        ManaCost     = Math.Max(0, (int)Math.Round(ability.ManaCost * (1.0 - costPct))),
                    };
            }
            if (ability.Id.StartsWith("Mind_Blast"))
            {
                double pct = TalentState.GetTotalBonus(ActiveClass, TalentEffectType.MindBlastManaCost);
                if (pct > 0)
                    return ability with { ManaCost = Math.Max(0, (int)Math.Round(ability.ManaCost * (1.0 - pct))) };
            }
            if (ability.Id.StartsWith("Drain_Mana"))
            {
                double bonus = TalentState.GetTotalBonus(ActiveClass, TalentEffectType.DrainManaBonus);
                if (bonus > 0)
                    return ability with { MotManaRestorePct = ability.MotManaRestorePct * (1.0 + bonus) };
            }
            if (ability.Id.StartsWith("Regrowth"))
            {
                double bonus = TalentState.GetTotalBonus(ActiveClass, TalentEffectType.RegrowthHealing);
                if (bonus > 0)
                    return ability with
                    {
                        Heal         = (int)Math.Round(ability.Heal * (1.0 + bonus)),
                        HotTotalHeal = (int)Math.Round(ability.HotTotalHeal * (1.0 + bonus)),
                    };
            }
            return ability;
        }

        /// <summary>Mana cost after the first-row cost talents (or rage/energy of the active form).</summary>
        private int GetTalentManaCost(Ability ability)
        {
            // Form-specific costs (e.g. Rejuvenation in Bear Form → rage) replace the mana cost
            if (ability.FormCosts.TryGetValue(ActiveForm, out int formCost))
                return formCost;
            if (ability.Id.StartsWith("Fireball"))
            {
                int reduction = (int)TalentState.GetTotalBonus(ActiveClass, TalentEffectType.FireballManaCost);
                return Math.Max(0, ability.ManaCost - reduction);
            }
            if (ability.Id.StartsWith("Frostbolt"))
            {
                int reduction = (int)TalentState.GetTotalBonus(ActiveClass, TalentEffectType.FrostboltManaCost);
                return Math.Max(0, ability.ManaCost - reduction);
            }
            if (ability.Id.StartsWith("Fire_Blast"))
            {
                int red = (int)TalentState.GetTotalBonus(ActiveClass, TalentEffectType.FireBlastManaCost);
                return Math.Max(0, ability.ManaCost - red);
            }
            if (ability.Id.StartsWith("Heroic_Strike"))
            {
                int red = (int)TalentState.GetTotalBonus(ActiveClass, TalentEffectType.HeroicStrikeManaCost);
                return Math.Max(0, ability.ManaCost - red);
            }
            if (ability.Id.StartsWith("Holy_Light"))
            {
                double pct = TalentState.GetTotalBonus(ActiveClass, TalentEffectType.HolyLightManaCost);
                return Math.Max(0, (int)Math.Round(ability.ManaCost * (1.0 - pct)));
            }
            if (ability.Id.StartsWith("Holy_Strike"))
            {
                double pct = TalentState.GetTotalBonus(ActiveClass, TalentEffectType.HolyStrikeManaCost);
                return Math.Max(0, (int)Math.Round(ability.ManaCost * (1.0 - pct)));
            }
            if (ability.Id.StartsWith("Raptor_Strike"))
            {
                int red = (int)TalentState.GetTotalBonus(ActiveClass, TalentEffectType.RaptorStrikeManaCost);
                return Math.Max(0, ability.ManaCost - red);
            }
            if (ability.Id.StartsWith("Serpent_Sting"))
            {
                int red = (int)TalentState.GetTotalBonus(ActiveClass, TalentEffectType.SerpentStingManaCost);
                return Math.Max(0, ability.ManaCost - red);
            }
            if (ability.Id.StartsWith("Shdow_bolt") || ability.Id.StartsWith("Shadow_Bolt"))
            {
                int red = (int)TalentState.GetTotalBonus(ActiveClass, TalentEffectType.ShadowBoltManaCost);
                return Math.Max(0, ability.ManaCost - red);
            }
            if (ability.Id.StartsWith("Lightning_bolt") || ability.Id.StartsWith("Lightning_Bolt"))
            {
                int red = (int)TalentState.GetTotalBonus(ActiveClass, TalentEffectType.LightningBoltManaCost);
                return Math.Max(0, ability.ManaCost - red);
            }
            if (ability.Id.StartsWith("Renew"))
            {
                double pct = TalentState.GetTotalBonus(ActiveClass, TalentEffectType.RenewManaCost);
                return Math.Max(0, (int)Math.Round(ability.ManaCost * (1.0 - pct)));
            }
            if (ability.Id.StartsWith("Mind_Blast"))
            {
                double pct = TalentState.GetTotalBonus(ActiveClass, TalentEffectType.MindBlastManaCost);
                return Math.Max(0, (int)Math.Round(ability.ManaCost * (1.0 - pct)));
            }
            return ability.ManaCost;
        }

        /// <summary>Additional crit bonus of a specific spell from talents.</summary>
        public double GetAbilityCritBonus(Ability ability)
        {
            if (ability.Id.StartsWith("Fireball"))
                return ability.BonusCritChance + TalentState.GetTotalBonus(ActiveClass, TalentEffectType.FireballCrit);
            if (ability.Id.StartsWith("Frostbolt"))
                return ability.BonusCritChance + TalentState.GetTotalBonus(ActiveClass, TalentEffectType.FrostboltCrit);
            return ability.BonusCritChance;
        }

        private void RecalculateStats()
        {
            var old = Stats;
            int level = ActiveProgress.Level;

            var weaponOverride = EquippedWeapon is not null
                ? new WeaponProfile(EquippedWeapon.Name, EquippedWeapon.MinDamage, EquippedWeapon.MaxDamage, EquippedWeapon.Speed)
                : null;

            var baseStats = StatsFactory.GetBaseStats(ActiveClass, Race, level, weaponOverride);

            double talentCrit        = TalentState.GetTotalCritBonus(ActiveClass);
            double autoAtkBonus      = TalentState.GetTotalBonus(ActiveClass, TalentEffectType.AutoAttackDamage);
            double autoAtkSpeedBonus = TalentState.GetTotalBonus(ActiveClass, TalentEffectType.AutoAttackSpeed);
            double damageTakenRed    = TalentState.GetTotalBonus(ActiveClass, TalentEffectType.DamageTaken);
            double healingBonus      = TalentState.GetTotalBonus(ActiveClass, TalentEffectType.HealingBonus);
            double spiritRegenBonus  = TalentState.GetTotalBonus(ActiveClass, TalentEffectType.SpiritManaRegen);

            // Add up the armor and weapon bonuses of all equipped pieces
            var armorBonus = PrimaryStats.Zero;
            foreach (var armor in EquippedArmor.Values)
                armorBonus = armorBonus + armor.Bonuses;
            if (EquippedWeapon?.Bonuses is { } weaponBonus)
                armorBonus = armorBonus + weaponBonus;
            // Consumables (elixirs, flasks, food) work like gear bonuses
            armorBonus = armorBonus + ConsumableBonus;

            var augPrimary = baseStats.Primary + armorBonus;

            // Melee formulas: Druid forms scale like Rogue (Cat) or Warrior (Bear)
            var meleeClass = MeleeScalingClass;

            Stats = ApplyFormStats(baseStats with
            {
                CopperReward               = old.CopperReward,
                XpReward                   = old.XpReward,
                CritChance                 = StatScaling.CritChance(meleeClass) + talentCrit,
                MeleeDamageMultiplier      = StatScaling.MeleeDamageMultiplier(meleeClass),
                AutoAttackDamageMultiplier = StatScaling.AutoAttackDamageMultiplier(meleeClass) * (1.0 + autoAtkBonus),
                AttackSpeedBonus           = StatScaling.AttackSpeedBonus(meleeClass) + autoAtkSpeedBonus,
                DamageTakenReduction       = Math.Clamp(damageTakenRed, 0, 0.99),
                HealingMultiplier          = 1.0 + healingBonus,
                Primary                    = augPrimary,
                MaxHp                      = baseStats.MaxHp   + StatScaling.BonusHp(armorBonus) + StaminaTalentHp(augPrimary),
                MaxMana                    = baseStats.MaxMana  + StatScaling.BonusMana(ActiveClass, armorBonus),
                ManaRegenPer5              = ManaRegenWithSpiritBonus(StatScaling.ManaRegenPer5(ActiveClass, augPrimary), spiritRegenBonus),
                HpRegenPer5                = StatScaling.HpRegenPer5(meleeClass, augPrimary),
                AttackPower                = StatScaling.AttackPower(meleeClass, level, augPrimary),
            });

            CurrentHp = Stats.MaxHp;
            CurrentMana = StartingResource;
        }

        /// <summary>Recalculates stats but keeps current health/mana (clamped to the new maximums).</summary>
        private void RecalculateStatsKeepResources()
        {
            int hp = CurrentHp, mana = CurrentMana;
            RecalculateStats();
            CurrentHp = Math.Clamp(hp, 1, Stats.MaxHp);
            CurrentMana = Math.Clamp(mana, 0, Stats.MaxMana);
        }

        /// <summary>Only increases the Spirit part of mana regeneration (base value stays), e.g. Restoration Mana.</summary>
        private static int ManaRegenWithSpiritBonus(int regen, double bonus)
        {
            int spiritPart = Math.Max(0, regen - StatScaling.BaseManaRegenPer5);
            return regen + (int)Math.Round(spiritPart * bonus);
        }
    }
}
