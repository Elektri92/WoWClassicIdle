// Combat/Enemy.cs
using Models;

public class Enemy
{
    public Stats Stats { get; }
    public string Name { get; set; } = "";
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

    public void TakeDamage(int amount)
    {
        CurrentHp = Math.Max(0, CurrentHp - amount);
        DamageTaken?.Invoke(amount);
    }
}
