// Models/Character.cs

using Models;

public abstract class Character
{
    public string Name { get; init; } = "";
    public Stats Stats { get; set; }
    public int CurrentHp { get; set; }
    public int CurrentMana { get; set; }
    public bool IsAlive => CurrentHp > 0;

    public CharacterClass CurrentClass { get; private set; }
    public AbilityCooldownTracker Cooldowns { get; } = new();
    public BuffTracker Buffs { get; } = new();
    private ClassLoadout _loadout = new() { Class = CharacterClass.Warrior };

    // Gecachte Ability-Listen – werden bei Klassen- oder Loadout-Wechsel zurückgesetzt
    private Ability[]? _cachedEquipped;
    private Ability[]? _cachedSortedActive;  // ohne ReplacesAutoAttack/IsPassive, sortiert nach Cast-Priorität
    private Ability[]? _cachedPassives;       // nur IsPassive && ProcChance > 0

    /// <summary>Nur die ausgerüsteten Abilities der aktuellen Klasse (gecacht).</summary>
    public IReadOnlyList<Ability> EquippedAbilities => _cachedEquipped ??= BuildEquippedCache();

    /// <summary>Aktive Cast-Abilities, vorsortiert: Heilung → Buff/Debuff → Schaden.</summary>
    internal Ability[] SortedEquippedAbilities =>
        _cachedSortedActive ??= BuildSortedActiveCache();

    /// <summary>Passive Abilities mit ProcChance > 0 (z. B. Deadly Poison).</summary>
    internal Ability[] PassiveAbilities =>
        _cachedPassives ??= BuildPassivesCache();

    private Ability[] BuildEquippedCache()
    {
        var pool = ClassAbilityRegistry.GetAbilitiesFor(CurrentClass);
        var poolDict = new Dictionary<string, Ability>(pool.Count);
        foreach (var a in pool) poolDict[a.Id] = a;
        var result = new List<Ability>(_loadout.EquippedAbilityIds.Count);
        foreach (var id in _loadout.EquippedAbilityIds)
            if (id != null && poolDict.TryGetValue(id, out var ability))
                result.Add(ability);
        return result.ToArray();
    }

    private Ability[] BuildSortedActiveCache() =>
        (_cachedEquipped ??= BuildEquippedCache())
            .Where(a => !a.ReplacesAutoAttack && !a.IsPassive)
            .OrderBy(a => a.IsHealing ? 0 : (a.IsBuff || a.IsDebuff) ? 1 : 2)
            .ToArray();

    private Ability[] BuildPassivesCache() =>
        (_cachedEquipped ??= BuildEquippedCache())
            .Where(a => a.IsPassive && a.ProcChance > 0)
            .ToArray();

    private void InvalidateAbilityCache()
    {
        _cachedEquipped = null;
        _cachedSortedActive = null;
        _cachedPassives = null;
    }

    public event Action<int>? DamageTaken;
    public event Action<string, int>? AbilityUsed;   // (AbilityName, Schaden/Heilung)

    /// <summary>
    /// Öffentlich aufrufbarer Trigger für das <see cref="AbilityUsed"/>-Event.
    /// </summary>
    public void TriggerAbilityUsed(string abilityName, int value)
    {
        AbilityUsed?.Invoke(abilityName, value);
    }

    protected Character(Stats stats, CharacterClass initialClass = CharacterClass.Warrior)
    {
        Stats = stats;
        CurrentHp = stats.MaxHp;
        CurrentMana = initialClass == CharacterClass.Warrior ? 0 : stats.MaxMana;
        SetClass(initialClass);
    }

    public void SetClass(CharacterClass newClass)
    {
        CurrentClass = newClass;

        var newLoadout = new ClassLoadout { Class = newClass };
        var defaultAbilities = ClassAbilityRegistry.GetAbilitiesFor(newClass);
        newLoadout.EquippedAbilityIds = defaultAbilities.Select(a => a.Id).ToList();

        _loadout = newLoadout;
        InvalidateAbilityCache();
        Cooldowns.Reset();
        Buffs.Reset();
    }

    internal ClassLoadout GetCurrentLoadout() => _loadout;

    /// <summary>
    /// Lädt eine gespeicherte Loadout (z.&nbsp;B. aus dem Service) in den Charakter.
    /// Ruft dies nach <see cref="SetClass"/>, sobald die persistierte Loadout verfügbar ist.
    /// </summary>
    public void ApplyLoadout(ClassLoadout loadout)
    {
        if (loadout.Class != CurrentClass) return;
        _loadout = loadout;
        InvalidateAbilityCache();
    }

    
    protected Character(Stats stats)
    {
        Stats = stats;
        CurrentHp = stats.MaxHp;
        CurrentMana = stats.MaxMana;
    }

    /// <returns>Tatsächlich erlittener Schaden und per-Quelle absorbierte Mengen.</returns>
    public (int Effective, IReadOnlyList<(Ability Source, int Absorbed)> Absorbs) TakeDamage(int rawDamage)
    {
        var mitigated = Math.Max(1, rawDamage - Stats.Defense);
        var absorbs   = Buffs.AbsorbDamage(mitigated);
        var absorbed  = absorbs.Count > 0 ? absorbs.Sum(a => a.Absorbed) : 0;
        var effective = mitigated - absorbed;
        CurrentHp = Math.Max(0, CurrentHp - effective);
        DamageTaken?.Invoke(effective);
        return (effective, absorbs);
    }

    public bool ConsumeMana(int amount)
    {
        if (CurrentMana < amount) return false;
        CurrentMana -= amount;
        return true;
    }

    public void RegenMana(int amount)
    {
        CurrentMana = Math.Min(Stats.MaxMana + Buffs.BonusMaxMana, CurrentMana + amount);
    }

    public void RegenHp(int amount)
    {
        CurrentHp = Math.Min(Stats.MaxHp, CurrentHp + amount);
    }
}
