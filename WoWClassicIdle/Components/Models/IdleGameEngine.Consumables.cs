using Models;

// Consumables in combat: automatic use from the inventory (buffs, potions) and bandages on death
public partial class IdleGameEngine
{
    private const double ConsumableCheckInterval = 1.0;   // Combat seconds between buff checks
    private const double PotionCooldownSeconds   = 60;    // shared cooldown for healing and mana potions
    private const double HealthPotionThreshold   = 0.35;
    private const double ManaPotionThreshold     = 0.25;
    public  const double ExplosiveCooldownSeconds = 60;   // own cooldown for dynamite

    private double _consumableCheckAcc = ConsumableCheckInterval;
    private double _potionCooldown;
    private double _explosiveCooldown;

    /// <summary>A consumable was used automatically (sim thread!).</summary>
    public event Action<ConsumableItem>? ConsumableUsed;

    public double PotionCooldownRemaining => _potionCooldown;
    public double ExplosiveCooldownRemaining => _explosiveCooldown;
    public const double PotionCooldownTotal = PotionCooldownSeconds;

    // Own RNG for the disguise so the combat random sequence (simulator seed) stays unchanged
    private readonly Random _costumeRng = new();

    /// <summary>Per combat step: let buffs expire, drink potions if needed, fill empty/weaker buff slots.</summary>
    private void UpdateConsumables(double dt)
    {
        Player.TickConsumables(dt);
        // An expired disguise renews itself automatically as long as some are left in the inventory
        if (Player.TickCostume(dt) && UsableConsumables().FirstOrDefault(c => c.Kind == ConsumableKind.Costume) is { } next)
            ApplyCostume(next);
        _potionCooldown = Math.Max(0, _potionCooldown - dt);
        _explosiveCooldown = Math.Max(0, _explosiveCooldown - dt);

        // Potions react immediately, buffs are checked every second (inventory scan)
        TryDrinkPotion();
        TryThrowExplosive();
        _consumableCheckAcc += dt;
        if (_consumableCheckAcc < ConsumableCheckInterval) return;
        _consumableCheckAcc = 0;
        RefreshConsumableBuffs();
    }

    /// <summary>Consumables in the inventory whose level requirement the active character meets.</summary>
    private IEnumerable<ConsumableItem> UsableConsumables() =>
        Player.Inventory.AllContainers
            .SelectMany(b => b.Slots)
            .Select(s => s?.Item)
            .OfType<ConsumableItem>()
            .Where(c => c.RequiredLevel <= Player.Level)
            .Distinct();

    private void RefreshConsumableBuffs()
    {
        // Only stats the character actually uses (no Intellect for Rogues, no Strength for Mages)
        foreach (var group in UsableConsumables().Where(c => c.IsBuff && Player.CanUseConsumable(c)).GroupBy(c => c.BuffKey))
        {
            var best = group.MaxBy(c => c.Potency)!;
            // Empty slot → use; weaker one active → stronger one overwrites; equally strong → only after expiry
            if (Player.GetActiveConsumable(group.Key) is { } active && active.Item.Potency >= best.Potency) continue;
            Consume(best);
            Player.ApplyConsumable(best);
        }
    }

    private void TryDrinkPotion()
    {
        if (_potionCooldown > 0) return;

        if (Player.CurrentHp < Player.Stats.MaxHp * HealthPotionThreshold
            && BestConsumable(ConsumableKind.HealthPotion) is { } heal)
        {
            int before = Player.CurrentHp;
            Consume(heal);
            Player.RegenHp(heal.Amount);
            CombatTextRaised?.Invoke(new CombatText(CombatTextTarget.Player, CombatTextKind.Heal, Player.CurrentHp - before));
            _potionCooldown = PotionCooldownSeconds;
        }
        else if (Player.Resource == ResourceKind.Mana && StatScaling.UsesMana(Player.ActiveClass)
                 && Player.CurrentMana < Player.Stats.MaxMana * ManaPotionThreshold
                 && BestConsumable(ConsumableKind.ManaPotion) is { } mana)
        {
            Consume(mana);
            Player.RegenMana(mana.Amount);
            _potionCooldown = PotionCooldownSeconds;
        }
    }

    /// <summary>Dynamite: as soon as the cooldown is ready, throw the strongest usable one at the enemy.</summary>
    private void TryThrowExplosive()
    {
        if (_explosiveCooldown > 0 || CurrentEnemy is not { IsAlive: true } enemy) return;
        if (BestConsumable(ConsumableKind.Explosive) is not { } explosive) return;

        Consume(explosive);
        int dealt = enemy.TakeDamage(explosive.Amount);
        Meter.RecordItem(explosive, CombatActionKind.Damage, dealt);
        CombatTextRaised?.Invoke(new CombatText(CombatTextTarget.Enemy, CombatTextKind.Ability, dealt));
        _explosiveCooldown = ExplosiveCooldownSeconds;
    }

    /// <summary>
    /// Manual use (inventory/combat bar) – so far only disguises. Returns an error text or <c>null</c>.
    /// </summary>
    public string? UseConsumableManually(ConsumableItem item)
    {
        lock (_simLock)
        {
            if (item.Kind != ConsumableKind.Costume) return "Used automatically in combat.";
            if (item.RequiredLevel > Player.Level) return $"Requires level {item.RequiredLevel}";
            if (Player.Inventory.CountOf(item.Id) == 0) return "None left.";
            ApplyCostume(item);
        }
        TickUpdated?.Invoke();
        return null;
    }

    private void ApplyCostume(ConsumableItem item)
    {
        Consume(item);
        var costume = _costumeRng.Next(2) == 0 ? Costume.Pirate : Costume.Ninja;
        Player.SetCostume(costume, item.DurationSeconds);
        LastLog = $"{item.Name}: you turn into a {Costumes.Name(costume)}!";
    }

    private ConsumableItem? BestConsumable(ConsumableKind kind) =>
        UsableConsumables().Where(c => c.Kind == kind).MaxBy(c => c.Amount);

    private void Consume(ConsumableItem item)
    {
        Player.Inventory.Remove(item.Id, 1);
        ConsumableUsed?.Invoke(item);
    }

    /// <summary>
    /// On death: elixirs/food are removed (flasks stay), the best bandage in the inventory
    /// shortens the wait at the spirit healer by its percentage.
    /// </summary>
    private void HandleConsumablesOnDeath()
    {
        Player.ClearConsumablesOnDeath();
        _potionCooldown = 0;

        if (UsableConsumables().Where(c => c.Kind == ConsumableKind.Bandage).MaxBy(c => c.Amount) is not { } bandage) return;
        Consume(bandage);
        RespawnDuration *= 1 - Math.Clamp(bandage.Amount, 0, 90) / 100.0;
        RespawnRemaining = RespawnDuration;
        LastLog = $"Defeated! {bandage.Name} speeds up the Spirit Healer by {bandage.Amount}%…";
    }
}
