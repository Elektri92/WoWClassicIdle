// Models/Player.Combat.cs
namespace Models
{
    // Neue Systeme als eigene Partial-Datei anlegen; Dateien möglichst < 300 Zeilen halten.
    public partial class Player
    {
        /// <summary>
        /// Setzt das Level der aktiven Klasse direkt (z. B. für Debug-Zwecke).
        /// </summary>
        public void SetLevel(int level)
        {
            ActiveProgress.SetLevel(level);
            RecalculateStats();
        }

        /// <summary>Gibt eine talentmodifizierte Kopie der Fähigkeit zurück (inkl. Manakosten).</summary>
        public Ability GetEffectiveAbility(Ability ability)
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
                    return ability with { DebuffDotDamage = (int)Math.Round(ability.DebuffDotDamage * (1.0 + bonus)) };
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
                    return ability with
                    {
                        Damage   = (int)Math.Round(ability.Damage * (1.0 + dmgBonus)),
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
                    return ability with { Damage = (int)Math.Round(ability.Damage * (1.0 + bonus)) };
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
            if (ability.Id.StartsWith("Rockbiter_Weapon"))
            {
                double bonus = TalentState.GetTotalBonus(ActiveClass, TalentEffectType.RockbiterDmgBonus);
                if (bonus > 0)
                    return ability with { BuffWeaponDamageBonus = ability.BuffWeaponDamageBonus + bonus };
            }
            if (ability.Id.StartsWith("Earthbind_Totem"))
            {
                double red = TalentState.GetTotalBonus(ActiveClass, TalentEffectType.EarthbindCooldown);
                if (red > 0)
                    return ability with { Cooldown = Math.Max(1, ability.Cooldown - red) };
            }
            if (ability.Id.StartsWith("Wrath"))
            {
                int red = (int)TalentState.GetTotalBonus(ActiveClass, TalentEffectType.WrathManaCost);
                if (red > 0)
                    return ability with { ManaCost = Math.Max(0, ability.ManaCost - red) };
            }
            return ability;
        }

        /// <summary>Effektive Manakosten einer Fähigkeit nach Talent-Reduktionen.</summary>
        public int GetEffectiveManaCost(Ability ability)
        {
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
            if (ability.Id.StartsWith("Wrath"))
            {
                int red = (int)TalentState.GetTotalBonus(ActiveClass, TalentEffectType.WrathManaCost);
                return Math.Max(0, ability.ManaCost - red);
            }
            return ability.ManaCost;
        }

        /// <summary>Zusätzlicher Crit-Bonus eines bestimmten Zaubers durch Talente.</summary>
        public double GetAbilityCritBonus(Ability ability)
        {
            if (ability.Id.StartsWith("Fireball"))
                return TalentState.GetTotalBonus(ActiveClass, TalentEffectType.FireballCrit);
            if (ability.Id.StartsWith("Frostbolt"))
                return TalentState.GetTotalBonus(ActiveClass, TalentEffectType.FrostboltCrit);
            return 0;
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

            // Rüstungs- und Waffen-Boni aller ausgerüsteten Teile aufaddieren
            var armorBonus = PrimaryStats.Zero;
            foreach (var armor in EquippedArmor.Values)
                armorBonus = armorBonus + armor.Bonuses;
            if (EquippedWeapon?.Bonuses is { } weaponBonus)
                armorBonus = armorBonus + weaponBonus;

            var augPrimary = baseStats.Primary + armorBonus;

            Stats = baseStats with
            {
                CopperReward               = old.CopperReward,
                XpReward                   = old.XpReward,
                CritChance                 = baseStats.CritChance + talentCrit,
                AutoAttackDamageMultiplier = baseStats.AutoAttackDamageMultiplier * (1.0 + autoAtkBonus),
                AttackSpeedBonus           = baseStats.AttackSpeedBonus + autoAtkSpeedBonus,
                DamageTakenReduction       = Math.Clamp(damageTakenRed, 0, 0.99),
                HealingMultiplier          = 1.0 + healingBonus,
                Primary                    = augPrimary,
                MaxHp                      = baseStats.MaxHp   + StatScaling.BonusHp(armorBonus),
                MaxMana                    = baseStats.MaxMana  + StatScaling.BonusMana(ActiveClass, armorBonus),
                ManaRegenPer5              = StatScaling.ManaRegenPer5(ActiveClass, augPrimary),
                AttackPower                = StatScaling.AttackPower(ActiveClass, level, augPrimary),
            };

            CurrentHp = Stats.MaxHp;
            CurrentMana = ActiveClass == CharacterClass.Warrior ? 0 : Stats.MaxMana;
        }
    }
}
