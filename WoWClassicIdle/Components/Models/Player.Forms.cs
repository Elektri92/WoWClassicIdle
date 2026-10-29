// Models/Player.Forms.cs
namespace Models
{
    // Create new systems as their own partial file; keep files below 300 lines where possible.
    public partial class Player
    {
        // Form the stats were last calculated with
        private DruidForm _statsForm;

        /// <summary>Form (Druid, Priest Shadowform) – derived from the equipped form ability.</summary>
        public DruidForm ActiveForm => ShapeshiftForms.FormOf(EquippedAbilities);

        /// <summary>Class whose formulas the melee values scale with (Cat → Rogue, Bear → Warrior).</summary>
        public CharacterClass MeleeScalingClass => StatScaling.MeleeScalingClass(ActiveClass, ActiveForm);

        /// <summary>Resource the abilities currently consume (class or form).</summary>
        public ResourceKind Resource => ShapeshiftForms.ResourceFor(ActiveClass, ActiveForm);

        /// <summary>Portrait of the active form; null = normal race portrait.</summary>
        public string? FormPortraitUrl => ShapeshiftForms.Get(ActiveForm) is { PortraitUrl.Length: > 0 } f ? f.PortraitUrl : null;

        /// <summary>Rage starts empty, mana and energy full.</summary>
        private int StartingResource => Resource == ResourceKind.Rage ? 0 : Stats.MaxMana;

        /// <summary>Can the ability be used in the current form?</summary>
        public bool CanUseInCurrentForm(Ability ability) => ShapeshiftForms.IsUsableIn(ability, ActiveForm);

        /// <summary>Form change → recalculate resource and stats (HP is preserved).</summary>
        protected override void OnLoadoutChanged()
        {
            // Unequipped auras (Paladin auras, aspects, stances) end immediately
            Buffs.RemoveAurasExcept(EquippedAbilities);
            if (ActiveForm == _statsForm) return;
            int hp = CurrentHp;
            RecalculateStats();
            CurrentHp = Math.Min(hp, Stats.MaxHp);
        }

        /// <summary>Applies the passive form bonuses to freshly calculated stats.</summary>
        private Stats ApplyFormStats(Stats stats)
        {
            _statsForm = ActiveForm;
            var form = ShapeshiftForms.Get(_statsForm);
            if (form is null || form.IsStance) return stats;   // Stances work via their aura buff

            if (form.Resource != ResourceKind.Mana)
                return stats with
                {
                    MaxMana              = ShapeshiftForms.NonManaPoolSize,
                    ManaRegenPer5        = 0,
                    DamageTakenReduction = Math.Clamp(stats.DamageTakenReduction + form.DamageTakenReduction, 0, 0.99),
                    HealingMultiplier    = stats.HealingMultiplier + form.HealingBonus,
                };

            int spiritRegen = stats.ManaRegenPer5 - StatScaling.BaseManaRegenPer5;
            return stats with
            {
                ManaRegenPer5        = stats.ManaRegenPer5 + (int)Math.Round(spiritRegen * form.SpiritManaRegenBonus),
                DamageTakenReduction = Math.Clamp(stats.DamageTakenReduction + form.DamageTakenReduction, 0, 0.99),
                HealingMultiplier    = stats.HealingMultiplier + form.HealingBonus,
            };
        }

        /// <summary>Form damage bonus on specific spells (e.g. Moonkin → Wrath, Moonfire).</summary>
        private Ability ApplyFormBonus(Ability ability)
        {
            var form = ShapeshiftForms.Get(ActiveForm);
            if (form is null || form.BoostedAbilityDamageBonus <= 0 || !form.BoostedAbilityNames.Contains(ability.Name))
                return ability;

            double mult = 1.0 + form.BoostedAbilityDamageBonus;
            return ability.WithDamageBonus(form.BoostedAbilityDamageBonus) with
            {
                DebuffDotDamage = (int)Math.Round(ability.DebuffDotDamage * mult),
            };
        }
    }
}
