using Models;

// Verbrauchsgüter im Kampf: automatischer Verbrauch aus dem Inventar (Buffs, Tränke) und Verbände beim Tod
public partial class IdleGameEngine
{
    private const double ConsumableCheckInterval = 1.0;   // Kampfsekunden zwischen den Buff-Prüfungen
    private const double PotionCooldownSeconds   = 60;    // gemeinsame Abklingzeit für Heil- und Manatränke
    private const double HealthPotionThreshold   = 0.35;
    private const double ManaPotionThreshold     = 0.25;
    public  const double ExplosiveCooldownSeconds = 60;   // eigene Abklingzeit für Dynamit

    private double _consumableCheckAcc = ConsumableCheckInterval;
    private double _potionCooldown;
    private double _explosiveCooldown;

    /// <summary>Ein Verbrauchsgut wurde automatisch benutzt (Sim-Thread!).</summary>
    public event Action<ConsumableItem>? ConsumableUsed;

    public double PotionCooldownRemaining => _potionCooldown;
    public double ExplosiveCooldownRemaining => _explosiveCooldown;
    public const double PotionCooldownTotal = PotionCooldownSeconds;

    // Eigener RNG für die Verkleidung, damit die Kampf-Zufallsfolge (Simulator-Seed) unverändert bleibt
    private readonly Random _costumeRng = new();

    /// <summary>Je Kampfschritt: Buffs ablaufen lassen, bei Bedarf Tränke trinken, leere/schwächere Buff-Slots auffüllen.</summary>
    private void UpdateConsumables(double dt)
    {
        Player.TickConsumables(dt);
        // Abgelaufene Verkleidung erneuert sich automatisch, solange noch welche im Inventar sind
        if (Player.TickCostume(dt) && UsableConsumables().FirstOrDefault(c => c.Kind == ConsumableKind.Costume) is { } next)
            ApplyCostume(next);
        _potionCooldown = Math.Max(0, _potionCooldown - dt);
        _explosiveCooldown = Math.Max(0, _explosiveCooldown - dt);

        // Tränke reagieren sofort, Buffs werden im Sekundentakt geprüft (Inventar-Scan)
        TryDrinkPotion();
        TryThrowExplosive();
        _consumableCheckAcc += dt;
        if (_consumableCheckAcc < ConsumableCheckInterval) return;
        _consumableCheckAcc = 0;
        RefreshConsumableBuffs();
    }

    /// <summary>Verbrauchsgüter im Inventar, deren Levelanforderung der aktive Charakter erfüllt.</summary>
    private IEnumerable<ConsumableItem> UsableConsumables() =>
        Player.Inventory.AllContainers
            .SelectMany(b => b.Slots)
            .Select(s => s?.Item)
            .OfType<ConsumableItem>()
            .Where(c => c.RequiredLevel <= Player.Level)
            .Distinct();

    private void RefreshConsumableBuffs()
    {
        // Nur Stats, die der Charakter auch nutzt (kein Intellekt für Schurken, keine Stärke für Magier)
        foreach (var group in UsableConsumables().Where(c => c.IsBuff && Player.CanUseConsumable(c)).GroupBy(c => c.BuffKey))
        {
            var best = group.MaxBy(c => c.Potency)!;
            // Leerer Slot → benutzen; schwächerer aktiv → stärkerer überschreibt; gleich stark → erst nach Ablauf
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

    /// <summary>Dynamit: sobald die Abklingzeit bereit ist, das stärkste nutzbare auf den Gegner werfen.</summary>
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
    /// Manuelle Benutzung (Inventar/Kampfleiste) – bisher nur Verkleidungen. Gibt einen Fehlertext zurück oder <c>null</c>.
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
    /// Beim Tod: Elixiere/Essen fallen weg (Fläschchen bleiben), der beste Verband im Inventar
    /// verkürzt die Wartezeit beim Geistheiler um seinen Prozentwert.
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
