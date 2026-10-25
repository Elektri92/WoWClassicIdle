// Models/Player.Forms.cs
namespace Models
{
    // Neue Systeme als eigene Partial-Datei anlegen; Dateien möglichst < 300 Zeilen halten.
    public partial class Player
    {
        // Gestalt, mit der die Stats zuletzt berechnet wurden
        private DruidForm _statsForm;

        /// <summary>Gestalt (Druide, Priester-Shadowform) – ergibt sich aus der ausgerüsteten Form-Fähigkeit.</summary>
        public DruidForm ActiveForm => ShapeshiftForms.FormOf(EquippedAbilities);

        /// <summary>Klasse, nach deren Formeln die Nahkampf-Werte skalieren (Katze → Schurke, Bär → Krieger).</summary>
        public CharacterClass MeleeScalingClass => StatScaling.MeleeScalingClass(ActiveClass, ActiveForm);

        /// <summary>Ressource, die die Fähigkeiten aktuell verbrauchen (Klasse bzw. Gestalt).</summary>
        public ResourceKind Resource => ShapeshiftForms.ResourceFor(ActiveClass, ActiveForm);

        /// <summary>Portrait der aktiven Gestalt; null = normales Rassen-Portrait.</summary>
        public string? FormPortraitUrl => ShapeshiftForms.Get(ActiveForm)?.PortraitUrl;

        /// <summary>Rage startet leer, Mana und Energie voll.</summary>
        private int StartingResource => Resource == ResourceKind.Rage ? 0 : Stats.MaxMana;

        /// <summary>Kann die Fähigkeit in der aktuellen Gestalt eingesetzt werden?</summary>
        public bool CanUseInCurrentForm(Ability ability) => ShapeshiftForms.IsUsableIn(ability, ActiveForm);

        /// <summary>Gestaltwechsel → Ressource und Stats neu berechnen (HP bleiben erhalten).</summary>
        protected override void OnLoadoutChanged()
        {
            if (ActiveForm == _statsForm) return;
            int hp = CurrentHp;
            RecalculateStats();
            CurrentHp = Math.Min(hp, Stats.MaxHp);
        }

        /// <summary>Wendet die passiven Gestalt-Boni auf frisch berechnete Stats an.</summary>
        private Stats ApplyFormStats(Stats stats)
        {
            _statsForm = ActiveForm;
            var form = ShapeshiftForms.Get(_statsForm);
            if (form is null) return stats;

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

        /// <summary>Gestalt-Schadensbonus auf bestimmte Zauber (z. B. Moonkin → Wrath, Moonfire).</summary>
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
