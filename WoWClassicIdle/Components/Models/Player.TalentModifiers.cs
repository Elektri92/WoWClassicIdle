// Models/Player.TalentModifiers.cs
namespace Models
{
    // Talent modifiers from row 2 – run after GetTalentModifiedAbility so the bonuses of both rows add up.
    public partial class Player
    {
        private double TalentBonus(TalentEffectType type) => TalentState.GetTotalBonus(ActiveClass, type);

        /// <summary>Effective mana cost of an ability after all talent reductions (or rage/energy of the active form).</summary>
        public int GetEffectiveManaCost(Ability ability) => ApplyExtendedManaCost(ability, GetTalentManaCost(ability));

        private int ApplyExtendedManaCost(Ability ability, int cost)
        {
            switch (ability.Name)
            {
                case "Sinister Strike":
                    return Math.Max(0, cost - (int)TalentBonus(TalentEffectType.SinisterStrikeEnergyCost));
                case "Raptor Strike":
                    double pct = TalentBonus(TalentEffectType.RaptorStrikeBonus);
                    return pct > 0 ? Math.Max(0, (int)Math.Round(cost * (1.0 - pct))) : cost;
                default:
                    return cost;
            }
        }

        private Ability ApplyExtendedTalentBonus(Ability ability)
        {
            // The lifesteal talent applies to all abilities with life drain (Drain Life, Siphon Life)
            if (ability.DebuffDotLifesteal > 0)
            {
                double lifesteal = TalentBonus(TalentEffectType.LifestealBonus);
                if (lifesteal > 0)
                    ability = ability with { DebuffDotLifesteal = ability.DebuffDotLifesteal * (1.0 + lifesteal) };
            }

            // General ability damage (Shadow Power, Inner Focus, Arcane Instability)
            double dmgBonus = TalentBonus(TalentEffectType.DamageBonus);
            if (dmgBonus > 0)
            {
                if (ability.IsDamage)
                    ability = ability.WithDamageBonus(dmgBonus);
                if (ability.DebuffDotDamage > 0)
                    ability = ability with { DebuffDotDamage = (int)Math.Round(ability.DebuffDotDamage * (1.0 + dmgBonus)) };
            }

            switch (ability.Name)
            {
                case "Consecration":
                {
                    double bonus = TalentBonus(TalentEffectType.ConsecrationDamage);
                    return bonus > 0 ? ability with { DebuffDotDamage = (int)Math.Round(ability.DebuffDotDamage * (1.0 + bonus)) } : ability;
                }
                case "Holy Strike":
                {
                    return ability.WithDamageBonus(TalentBonus(TalentEffectType.HolyStrikeDamage));
                }
                case "Sinister Strike":
                {
                    int red = (int)TalentBonus(TalentEffectType.SinisterStrikeEnergyCost);
                    return red > 0 ? ability with { ManaCost = Math.Max(0, ability.ManaCost - red) } : ability;
                }
                case "Slice and Dice":
                {
                    double bonus = TalentBonus(TalentEffectType.SliceAndDiceBonus);
                    return bonus > 0 ? ability with { BuffAttackSpeedBonus = ability.BuffAttackSpeedBonus + bonus } : ability;
                }
                case "Expose Armor":
                {
                    double bonus = TalentBonus(TalentEffectType.ExposeArmorBonus);
                    return bonus > 0 ? ability with { DebuffDamageTakenBonus = ability.DebuffDamageTakenBonus * (1.0 + bonus) } : ability;
                }
                case "Thorns":
                {
                    double bonus = TalentBonus(TalentEffectType.ThornsDamage);
                    return bonus > 0 ? ability with { BuffRetaliationDamage = (int)Math.Round(ability.BuffRetaliationDamage * (1.0 + bonus)) } : ability;
                }
                case "Insect Swarm":
                {
                    double bonus = TalentBonus(TalentEffectType.InsectSwarmDamage);
                    return bonus > 0 ? ability with { DebuffDotDamage = (int)Math.Round(ability.DebuffDotDamage * (1.0 + bonus)) } : ability;
                }
                case "Deadly Poison":
                {
                    int    stacks = (int)TalentBonus(TalentEffectType.DeadlyPoisonStacks);
                    double faster = TalentBonus(TalentEffectType.DeadlyPoisonTickRate);
                    return ability with
                    {
                        DebuffMaxStacks       = ability.DebuffMaxStacks + stacks,
                        DebuffDotTickInterval = ability.DebuffDotTickInterval * (1.0 - faster),
                    };
                }
                case "Serpent Sting":
                {
                    double bonus = TalentBonus(TalentEffectType.SerpentStingDamage);
                    return bonus > 0 ? ability with { DebuffDotDamage = (int)Math.Round(ability.DebuffDotDamage * (1.0 + bonus)) } : ability;
                }
                case "Fireball":
                    return ability.WithDamageBonus(TalentBonus(TalentEffectType.FireballDamage));
                case "Wrath":
                case "Starfire":
                    return ability.WithDamageBonus(TalentBonus(TalentEffectType.WrathDamage));
                case "Fire Blast":
                    return ability.WithDamageBonus(TalentBonus(TalentEffectType.FireBlastDamage));
                case "Mind Blast":
                    return ability.WithDamageBonus(TalentBonus(TalentEffectType.MindBlastDamage));
                case "Shadow Bolt":
                    return ability.WithDamageBonus(TalentBonus(TalentEffectType.ShadowBoltDamage));
                case "Multi-Shot":
                {
                    return ability.WithDamageBonus(TalentBonus(TalentEffectType.MultiShotDamage));
                }
                case "Raptor Strike":
                {
                    double bonus = TalentBonus(TalentEffectType.RaptorStrikeBonus);
                    return bonus > 0
                        ? ability.WithDamageBonus(bonus) with
                        {
                            ManaCost = Math.Max(0, (int)Math.Round(ability.ManaCost * (1.0 - bonus))),
                        }
                        : ability;
                }
                default:
                    return ability;
            }
        }

        /// <summary>Row 3 talents that improve a single ability (TalentRegistry.LevelAbilities.cs).</summary>
        private Ability ApplyAbilityModifierTalents(Ability ability)
        {
            foreach (var talent in TalentRegistry.GetTalents(ActiveClass))
                if (talent.AbilityModifier is not null && talent.ModifiedAbilityName == ability.Name && TalentState.GetRanks(talent.Id) > 0)
                    ability = talent.AbilityModifier(ability);
            return ability;
        }

        /// <summary>Additional HP from Stamina through talents (Strong Life).</summary>
        private int StaminaTalentHp(PrimaryStats primary) =>
            (int)Math.Round(primary.Stamina * StatScaling.HpPerStamina * TalentBonus(TalentEffectType.StaminaHpBonus));

        /// <summary>Additional pet damage (Master Demonologist).</summary>
        public double PetDamageTalentBonus => TalentBonus(TalentEffectType.PetDamageBonus);

        /// <summary>Additional pet HP as a share of the player HP (Strong Pet, Strong Demon).</summary>
        public double PetHealthTalentBonus => TalentBonus(TalentEffectType.PetHealthBonus);
    }
}
