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

    /// <summary>
    /// Nur die *ausgerüsteten* Abilities der aktuellen Klasse.
    /// </summary>
    public IReadOnlyList<Ability> EquippedAbilities
    {
        get
        {
            var pool = ClassAbilityRegistry.GetAbilitiesFor(CurrentClass);
            var poolIds = new HashSet<string>(pool.Select(a => a.Id));

            return _loadout.EquippedAbilityIds
                .Where(id => id != null && poolIds.Contains(id))
                .Select(id => pool.First(a => a.Id == id))
                .ToList();
        }
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
    }

    
    protected Character(Stats stats)
    {
        Stats = stats;
        CurrentHp = stats.MaxHp;
        CurrentMana = stats.MaxMana;
    }

    /// <returns>Tatsächlich erlittener Schaden nach Rüstung.</returns>
    public int TakeDamage(int rawDamage)
    {
        var mitigated = Math.Max(1, rawDamage - Stats.Defense);
        CurrentHp = Math.Max(0, CurrentHp - mitigated);
        DamageTaken?.Invoke(mitigated);
        return mitigated;
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
