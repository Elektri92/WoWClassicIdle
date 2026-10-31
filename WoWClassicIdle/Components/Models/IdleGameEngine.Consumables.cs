using Models;

// Consumables in combat: automatic use from the inventory (buffs, potions) and bandages on death,
// each kind only while its auto-use toggle is on
public partial class IdleGameEngine
{
    private const double ConsumableCheckInterval = 1.0;   // Combat seconds between buff checks
    private const double PotionCooldownSeconds   = 60;    // separate cooldowns for healing and mana potions
    private const double HealthPotionThreshold   = 0.35;
    private const double ManaPotionThreshold     = 0.25;
    public  const double ExplosiveCooldownSeconds = 60;   // own cooldown for dynamite

    private double _consumableCheckAcc = ConsumableCheckInterval;
    private double _healthPotionCooldown;
    private double _manaPotionCooldown;
    private double _explosiveCooldown;

    /// <summary>A consumable was used automatically (sim thread!).</summary>
    public event Action<ConsumableItem>? ConsumableUsed;

    public double HealthPotionCooldownRemaining => _healthPotionCooldown;
    public double ManaPotionCooldownRemaining => _manaPotionCooldown;
    public double ExplosiveCooldownRemaining => _explosiveCooldown;
    public const double PotionCooldownTotal = PotionCooldownSeconds;

    // Own RNG for the disguise so the combat random sequence (simulator seed) stays unchanged
    private readonly Random _costumeRng = new();

    /// <summary>Turns automatic use of a consumable kind on or off for the active character.</summary>
    public void ToggleConsumableAutoUse(ConsumableItem item)
    {
        lock (_simLock) Player.ToggleAutoUse(item);
        TickUpdated?.Invoke();
    }

    /// <summary>Per combat step: let buffs expire, drink potions if needed, fill empty/weaker buff slots.</summary>
    private void UpdateConsumables(double dt)
    {
        Player.TickConsumables(dt);
        // An expired disguise renews itself automatically as long as some are left in the inventory
        if (Player.TickCostume(dt) && AutoUsableConsumables().FirstOrDefault(c => c.Kind == ConsumableKind.Costume) is { } next)
            ApplyCostume(next);
        _healthPotionCooldown = Math.Max(0, _healthPotionCooldown - dt);
        _manaPotionCooldown = Math.Max(0, _manaPotionCooldown - dt);
        _explosiveCooldown = Math.Max(0, _explosiveCooldown - dt);

        // Potions react immediately, buffs are checked every second (inventory scan)
        TryDrinkHealthPotion();
        TryDrinkManaPotion();
        TryThrowExplosive();
        _consumableCheckAcc += dt;
        if (_consumableCheckAcc < ConsumableCheckInterval) return;
        _consumableCheckAcc = 0;
        RefreshConsumableBuffs();
    }

    /// <summary>Consumables in the inventory that the active character may use on their own (level met, auto-use on).</summary>
    private IEnumerable<ConsumableItem> AutoUsableConsumables() =>
        Player.Inventory.AllContainers
            .SelectMany(b => b.Slots)
            .Select(s => s?.Item)
            .OfType<ConsumableItem>()
            .Where(c => c.RequiredLevel <= Player.Level && Player.IsAutoUseEnabled(c))
            .Distinct();

    private void RefreshConsumableBuffs()
    {
        // Only stats the character actually uses (no Intellect for Rogues, no Strength for Mages)
        foreach (var group in AutoUsableConsumables().Where(c => c.IsBuff && Player.CanUseConsumable(c)).GroupBy(c => c.BuffKey))
        {
            var best = group.MaxBy(c => c.Potency)!;
            // Empty slot → use; weaker one active → stronger one overwrites; equally strong → only after expiry
            if (Player.GetActiveConsumable(group.Key) is { } active && active.Item.Potency >= best.Potency) continue;
            Consume(best);
            Player.ApplyConsumable(best);
        }
    }

    private void TryDrinkHealthPotion()
    {
        if (_healthPotionCooldown > 0 || Player.CurrentHp >= Player.Stats.MaxHp * HealthPotionThreshold) return;
        if (BestConsumable(ConsumableKind.HealthPotion) is not { } heal) return;

        int before = Player.CurrentHp;
        Consume(heal);
        Player.RegenHp((int)Math.Round(heal.Amount * Player.PotionEffectMultiplier(heal)));
        Meter.RecordItem(heal, CombatActionKind.Heal, Player.CurrentHp - before);
        CombatTextRaised?.Invoke(new CombatText(CombatTextTarget.Player, CombatTextKind.Heal, Player.CurrentHp - before));
        _healthPotionCooldown = PotionCooldownSeconds;
    }

    private void TryDrinkManaPotion()
    {
        if (_manaPotionCooldown > 0 || Player.Resource != ResourceKind.Mana || !StatScaling.UsesMana(Player.ActiveClass)) return;
        if (Player.CurrentMana >= Player.Stats.MaxMana * ManaPotionThreshold) return;
        if (BestConsumable(ConsumableKind.ManaPotion) is not { } mana) return;

        Consume(mana);
        Player.RegenMana((int)Math.Round(mana.Amount * Player.PotionEffectMultiplier(mana)));
        _manaPotionCooldown = PotionCooldownSeconds;
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
        AutoUsableConsumables().Where(c => c.Kind == kind).MaxBy(c => c.Amount);

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
        _healthPotionCooldown = 0;
        _manaPotionCooldown = 0;

        if (AutoUsableConsumables().Where(c => c.Kind == ConsumableKind.Bandage).MaxBy(c => c.Amount) is not { } bandage) return;
        Consume(bandage);
        RespawnDuration *= 1 - Math.Clamp(bandage.Amount, 0, 90) / 100.0;
        RespawnRemaining = RespawnDuration;
        LastLog = $"Defeated! {bandage.Name} speeds up the Spirit Healer by {bandage.Amount}%…";
    }

    /// <summary>Right after resurrecting: the strongest stimpak raises Spirit.</summary>
    private void UseStimpakAfterResurrection()
    {
        if (BestConsumable(ConsumableKind.Stimpak) is not { } stimpak) return;
        Consume(stimpak);
        Player.ApplyConsumable(stimpak);
        LastLog = $"{stimpak.Name}: +{stimpak.Amount}% Spirit for {ConsumableItem.FormatDuration(stimpak.DurationSeconds)}.";
    }
}
