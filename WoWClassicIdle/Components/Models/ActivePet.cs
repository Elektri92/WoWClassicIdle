// Models/ActivePet.cs
namespace Models;

/// <summary>Runtime state of a summoned pet (lives in memory only, is not saved).</summary>
public sealed class ActivePet
{
    public PetDefinition Definition { get; }

    /// <summary>Summon ability – receives the revive cooldown after death.</summary>
    public Ability SummonAbility { get; }

    /// <summary>Own meter row for pet damage (name and icon of the pet).</summary>
    public Ability MeterSource { get; }

    public int MaxHp { get; }
    public int CurrentHp { get; private set; }
    public bool IsAlive => CurrentHp > 0;

    /// <summary>Elapsed time since the last attack.</summary>
    public double AttackAcc { get; set; }

    public ActivePet(PetDefinition definition, Ability summonAbility, int maxHp)
    {
        Definition    = definition;
        SummonAbility = summonAbility;
        MaxHp         = Math.Max(1, maxHp);
        CurrentHp     = MaxHp;
        MeterSource   = new Ability { Id = $"pet_{definition.Kind}", Name = definition.Name, IconUrl = definition.IconUrl };
    }

    public void TakeDamage(int amount) => CurrentHp = Math.Max(0, CurrentHp - amount);
}
