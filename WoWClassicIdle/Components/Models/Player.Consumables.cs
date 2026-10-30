// Models/Player.Consumables.cs
using System.Collections.Immutable;

namespace Models
{
    // Active consumable buffs (elixirs, flasks, food): one slot per BuffKey, stronger ones overwrite weaker ones.
    public partial class Player
    {
        private readonly Dictionary<string, ActiveConsumable> _consumables = new();

        // Immutable: replaced on the UI thread under the sim lock, the UI reads without a lock
        /// <summary>BuffKeys of the consumable kinds this character must not use on their own.</summary>
        public ImmutableHashSet<string> DisabledAutoUse { get; private set; } = ImmutableHashSet<string>.Empty;

        public bool IsAutoUseEnabled(ConsumableItem item) => !DisabledAutoUse.Contains(item.BuffKey);

        public void ToggleAutoUse(ConsumableItem item) =>
            DisabledAutoUse = IsAutoUseEnabled(item)
                ? DisabledAutoUse.Add(item.BuffKey)
                : DisabledAutoUse.Remove(item.BuffKey);

        public void RestoreDisabledAutoUse(IEnumerable<string> buffKeys) =>
            DisabledAutoUse = buffKeys.ToImmutableHashSet();

        /// <summary>Active disguise (Savory Deviate Delight) – purely cosmetic.</summary>
        public Costume Costume { get; private set; }

        /// <summary>Remaining duration of the disguise (combat seconds).</summary>
        public double CostumeRemaining { get; private set; }

        public void SetCostume(Costume costume, double duration)
        {
            Costume = costume;
            CostumeRemaining = costume == Costume.None ? 0 : duration;
        }

        /// <summary>Advances the disguise; true = just expired (the engine renews it if applicable).</summary>
        public bool TickCostume(double dt)
        {
            if (Costume == Costume.None) return false;
            CostumeRemaining -= dt;
            if (CostumeRemaining > 0) return false;
            SetCostume(Costume.None, 0);
            return true;
        }

        /// <summary>
        /// Can the character make use of the stat buff? (see <see cref="StatScaling.IsUsefulStat"/>)
        /// Druids in a form additionally count as their melee class (Cat → Rogue: Agility).
        /// Consumables without stat bonuses (potions, bandages, dynamite) are always useful.
        /// </summary>
        public bool CanUseConsumable(ConsumableItem item) =>
            item.Bonuses is not { } bonus
            || StatScaling.IsUsefulStat(ActiveClass, bonus)
            || StatScaling.IsUsefulStat(MeleeScalingClass, bonus);

        /// <summary>Sum of the stat bonuses of all active consumables – flows into RecalculateStats.</summary>
        public PrimaryStats ConsumableBonus { get; private set; } = PrimaryStats.Zero;

        /// <summary>Active consumable buffs (copy for the UI).</summary>
        public IReadOnlyList<ActiveConsumable> ActiveConsumables
        {
            get { lock (_consumables) return _consumables.Values.OrderBy(c => c.Item.Kind).ToList(); }
        }

        public ActiveConsumable? GetActiveConsumable(string buffKey)
        {
            lock (_consumables) return _consumables.GetValueOrDefault(buffKey);
        }

        /// <summary>Activates a buff (replaces the slot of the same kind) and recalculates the stats.</summary>
        public void ApplyConsumable(ConsumableItem item)
        {
            lock (_consumables) _consumables[item.BuffKey] = new ActiveConsumable(item, item.DurationSeconds);
            RefreshConsumableStats();
        }

        /// <summary>Advances the buffs by <paramref name="dt"/> combat seconds; expired ones are removed.</summary>
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

        /// <summary>Death removes elixirs and food – flasks persist like in WoW.</summary>
        public void ClearConsumablesOnDeath()
        {
            lock (_consumables)
            {
                foreach (var key in _consumables.Where(kv => kv.Value.Item.Kind != ConsumableKind.Flask).Select(kv => kv.Key).ToList())
                    _consumables.Remove(key);
            }
            RefreshConsumableStats();
        }

        // Recalculate stats without refilling health/mana (RecalculateStats sets both to maximum)
        private void RefreshConsumableStats()
        {
            PrimaryStats bonus;
            lock (_consumables)
                bonus = _consumables.Values.Aggregate(PrimaryStats.Zero, (sum, c) => sum + (c.Item.Bonuses ?? PrimaryStats.Zero));
            if (bonus == ConsumableBonus) return;

            ConsumableBonus = bonus;
            RecalculateStatsKeepResources();
        }
    }
}
