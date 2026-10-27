// Models/Player.Consumables.cs
namespace Models
{
    // Aktive Verbrauchsgut-Buffs (Elixiere, Fläschchen, Essen): je BuffKey ein Slot, stärkere überschreiben schwächere.
    public partial class Player
    {
        private readonly Dictionary<string, ActiveConsumable> _consumables = new();

        /// <summary>Aktive Verkleidung (Savory Deviate Delight) – rein optisch.</summary>
        public Costume Costume { get; private set; }

        /// <summary>Restlaufzeit der Verkleidung (Kampfsekunden).</summary>
        public double CostumeRemaining { get; private set; }

        public void SetCostume(Costume costume, double duration)
        {
            Costume = costume;
            CostumeRemaining = costume == Costume.None ? 0 : duration;
        }

        /// <summary>Läuft die Verkleidung ab; true = gerade abgelaufen (Engine erneuert ggf.).</summary>
        public bool TickCostume(double dt)
        {
            if (Costume == Costume.None) return false;
            CostumeRemaining -= dt;
            if (CostumeRemaining > 0) return false;
            SetCostume(Costume.None, 0);
            return true;
        }

        /// <summary>
        /// Kann der Charakter mit dem Stat-Buff etwas anfangen? (siehe <see cref="StatScaling.IsUsefulStat"/>)
        /// Druiden in Gestalt zählen zusätzlich wie ihre Nahkampf-Klasse (Katze → Schurke: Beweglichkeit).
        /// Verbrauchsgüter ohne Stat-Boni (Tränke, Verbände, Dynamit) sind immer nützlich.
        /// </summary>
        public bool CanUseConsumable(ConsumableItem item) =>
            item.Bonuses is not { } bonus
            || StatScaling.IsUsefulStat(ActiveClass, bonus)
            || StatScaling.IsUsefulStat(MeleeScalingClass, bonus);

        /// <summary>Summe der Stat-Boni aller aktiven Verbrauchsgüter – fließt in RecalculateStats ein.</summary>
        public PrimaryStats ConsumableBonus { get; private set; } = PrimaryStats.Zero;

        /// <summary>Aktive Verbrauchsgut-Buffs (Kopie für die UI).</summary>
        public IReadOnlyList<ActiveConsumable> ActiveConsumables
        {
            get { lock (_consumables) return _consumables.Values.OrderBy(c => c.Item.Kind).ToList(); }
        }

        public ActiveConsumable? GetActiveConsumable(string buffKey)
        {
            lock (_consumables) return _consumables.GetValueOrDefault(buffKey);
        }

        /// <summary>Aktiviert einen Buff (ersetzt den Slot derselben Art) und berechnet die Stats neu.</summary>
        public void ApplyConsumable(ConsumableItem item)
        {
            lock (_consumables) _consumables[item.BuffKey] = new ActiveConsumable(item, item.DurationSeconds);
            RefreshConsumableStats();
        }

        /// <summary>Läuft die Buffs um <paramref name="dt"/> Kampfsekunden ab; abgelaufene entfallen.</summary>
        public void TickConsumables(double dt)
        {
            bool expired = false;
            lock (_consumables)
            {
                foreach (var key in _consumables.Keys.ToList())
                {
                    var active = _consumables[key];
                    double remaining = active.Remaining - dt;
                    if (remaining <= 0)
                    {
                        _consumables.Remove(key);
                        expired = true;
                    }
                    else
                        _consumables[key] = active with { Remaining = remaining };
                }
            }
            if (expired) RefreshConsumableStats();
        }

        /// <summary>Tod entfernt Elixiere und Essen – Fläschchen bleiben wie in WoW erhalten.</summary>
        public void ClearConsumablesOnDeath()
        {
            lock (_consumables)
            {
                foreach (var key in _consumables.Where(kv => kv.Value.Item.Kind != ConsumableKind.Flask).Select(kv => kv.Key).ToList())
                    _consumables.Remove(key);
            }
            RefreshConsumableStats();
        }

        // Stats neu berechnen, ohne Leben/Mana aufzufüllen (RecalculateStats setzt beides auf Maximum)
        private void RefreshConsumableStats()
        {
            PrimaryStats bonus;
            lock (_consumables)
                bonus = _consumables.Values.Aggregate(PrimaryStats.Zero, (sum, c) => sum + (c.Item.Bonuses ?? PrimaryStats.Zero));
            if (bonus == ConsumableBonus) return;

            int hp = CurrentHp, mana = CurrentMana;
            ConsumableBonus = bonus;
            RecalculateStats();
            CurrentHp = Math.Clamp(hp, 1, Stats.MaxHp);
            CurrentMana = Math.Clamp(mana, 0, Stats.MaxMana);
        }
    }
}
