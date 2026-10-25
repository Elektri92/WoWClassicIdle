// Models/ActivePet.cs
namespace Models;

/// <summary>Laufzeit-Zustand eines beschworenen Begleittiers (lebt nur im Speicher, wird nicht gespeichert).</summary>
public sealed class ActivePet
{
    public PetDefinition Definition { get; }

    /// <summary>Beschwörungs-Fähigkeit – bekommt nach dem Tod den Wiederbelebungs-Cooldown.</summary>
    public Ability SummonAbility { get; }

    /// <summary>Eigene Meter-Zeile für den Pet-Schaden (Name und Icon des Pets).</summary>
    public Ability MeterSource { get; }

    public int MaxHp { get; }
    public int CurrentHp { get; private set; }
    public bool IsAlive => CurrentHp > 0;

    /// <summary>Vergangene Zeit seit dem letzten Angriff.</summary>
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
