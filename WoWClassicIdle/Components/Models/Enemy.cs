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

    /// <summary>Zieht Schaden ab und gibt den tatsächlich abgezogenen Wert zurück (ohne Overkill).</summary>
    public int TakeDamage(int amount)
    {
        int effective = Math.Min(amount, CurrentHp);
        CurrentHp -= effective;
        DamageTaken?.Invoke(amount);
        return effective;
    }

    // Setzt HP zurück (z. B. wenn Spieler stirbt und Rare erneut bekämpft wird)
    public void ResetHp()
    {
        CurrentHp = Stats.MaxHp;
        Debuffs.Reset();
    }
}
