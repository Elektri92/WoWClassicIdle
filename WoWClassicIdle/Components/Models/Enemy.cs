// Combat/Enemy.cs
using Models;

public class Enemy
{
    public Stats Stats { get; }
    public string Name { get; set; } = "";
    /// <summary>Name of the enemy type without level/rare marker (for zone objectives).</summary>
    public string MobName { get; init; } = "";
    public string Type { get; set; } = "";
    public string? ImageUrl { get; set; }
    public bool IsRare { get; init; }

    public int CurrentHp { get; private set; }
    public bool IsAlive => CurrentHp > 0;

    public DebuffTracker Debuffs { get; } = new();

    public event Action<int>? DamageTaken;

    public Enemy(Stats stats)
    {
        Stats     = stats;
        CurrentHp = stats.MaxHp;
    }

    /// <summary>Subtracts damage and returns the value actually subtracted (without overkill).</summary>
    public int TakeDamage(int amount)
    {
        int effective = Math.Min(amount, CurrentHp);
        CurrentHp -= effective;
        DamageTaken?.Invoke(amount);
        return effective;
    }

    // Resets HP (e.g. when the player dies and the rare is fought again)
    public void ResetHp()
    {
        CurrentHp = Stats.MaxHp;
        Debuffs.Reset();
    }
}
