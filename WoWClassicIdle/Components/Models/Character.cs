// Models/Character.cs

using Models;

public abstract class Character
{
    public string Name { get; internal set; } = "";
    public Stats Stats { get; set; }
    public int CurrentHp { get; set; }
    public int CurrentMana { get; set; }
    public bool IsAlive => CurrentHp > 0;

    public CharacterClass CurrentClass { get; private set; }
    public AbilityCooldownTracker Cooldowns { get; } = new();
    public BuffTracker Buffs { get; } = new();
    private ClassLoadout _loadout = new() { Class = CharacterClass.Warrior };

    // Cached ability lists – reset on class or loadout change
    private Ability[]? _cachedEquipped;
    private Ability[]? _cachedSortedActive;  // without ReplacesAutoAttack/IsPassive, sorted by cast priority
    private Ability[]? _cachedPassives;       // only IsPassive && ProcChance > 0

    /// <summary>Only the equipped abilities of the current class (cached).</summary>
    public IReadOnlyList<Ability> EquippedAbilities => _cachedEquipped ??= BuildEquippedCache();

    /// <summary>Active cast abilities, pre-sorted by <see cref="Ability.CastPriorityGroup"/>: buff → heal/shield → damage.</summary>
    internal Ability[] SortedEquippedAbilities =>
        _cachedSortedActive ??= BuildSortedActiveCache();

    /// <summary>Passive abilities with ProcChance > 0 (e.g. Deadly Poison).</summary>
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
            .OrderBy(a => a.CastPriorityGroup)
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
    /// Publicly callable trigger for the <see cref="AbilityUsed"/> event.
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
    /// Loads a saved loadout (e.g.&nbsp;from the service) into the character.
    /// Call this after <see cref="SetClass"/> as soon as the persisted loadout is available.
    /// </summary>
    public void ApplyLoadout(ClassLoadout loadout)
    {
        if (loadout.Class != CurrentClass) return;
        _loadout = loadout;
        InvalidateAbilityCache();
        OnLoadoutChanged();
    }

    /// <summary>Hook after a loadout change (e.g. Druid form changes resource and stats).</summary>
    protected virtual void OnLoadoutChanged() { }

    
    protected Character(Stats stats)
    {
        Stats = stats;
        CurrentHp = stats.MaxHp;
        CurrentMana = stats.MaxMana;
    }

    /// <returns>Damage actually taken and amounts absorbed per source.</returns>
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
