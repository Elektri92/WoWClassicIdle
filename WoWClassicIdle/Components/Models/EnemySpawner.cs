// Combat/EnemySpawner.cs

using Models;

public class EnemySpawner
{
    private readonly Random _rng = new();
    private readonly IReadOnlyList<EnemyTemplate> _templates;

    public EnemySpawner()
        : this(DefaultEnemyTemplates.Default) { }

    public EnemySpawner(IReadOnlyList<EnemyTemplate> templates)
    {
        _templates = templates;
    }

    public Enemy Spawn(int playerLevel, double elapsedSeconds)
    {
        // 1) Gegner-Level: Spieler-Level ± 2 (random)
        var enemyLevel = Math.Max(1, playerLevel + _rng.Next(-2, 3));

        // 2) Nur Templates, deren LevelRange den enemyLevel abdeckt
        var candidates = _templates
            .Where(t => enemyLevel >= t.MinLevel && enemyLevel <= t.MaxLevel)
            .ToList();

        if (candidates.Count == 0)
            throw new InvalidOperationException(
                $"No enemy type configured for level {enemyLevel}.");

        // 3) Weighted Random Pick
        var totalWeight = candidates.Sum(c => c.Weight);
        var roll        = _rng.Next(totalWeight);
        int cumulative  = 0;

        var template = candidates[0];
        foreach (var t in candidates)
        {
            cumulative += t.Weight;
            if (roll < cumulative)
            {
                template = t;
                break;
            }
        }

        // 4) Stats skalieren: linear mit Level + leichte Zeit-Bonus
        var scale = 1; // + (elapsedSeconds / 60.0) * 0.05; // +5 % / Minute

        // 10 % Chance auf Rare – nur wenn sowohl Spieler als auch Gegner mind. Level 3
        bool isRare = playerLevel >= 3 && enemyLevel >= 3 && _rng.NextDouble() < 0.10;

        int baseHp      = template.GetMaxHp(enemyLevel);
        int baseAttack  = template.BaseAttack;
        int baseCopper  = RollCopper(template, enemyLevel);

        var stats = new Stats
        {
            MaxHp        = isRare ? baseHp * 2 : baseHp,
            Attack       = isRare ? (int)Math.Ceiling(baseAttack * 1.5) : baseAttack,
            Defense      = (int)(template.BaseDefense),
            Speed        = template.BaseSpeed,
            Level        = enemyLevel,
            CopperReward = baseCopper,
            XpReward     = (int)(StaticHelpers.GetXpForLevel(enemyLevel))
        };

        string name = isRare ? $"★ {template.Name} (Lv.{enemyLevel})" : $"{template.Name} (Lv.{enemyLevel})";

        return new Enemy(stats)
        {
            Name     = name,
            Type     = template.Type,
            ImageUrl = template.ImageUrl,
            IsRare   = isRare,
        };
    }

    // Wenige Kupfer pro Kill: Spanne aus dem Template, +1 Kupfer je Level über MinLevel
    private int RollCopper(EnemyTemplate template, int enemyLevel)
    {
        int min = Math.Max(0, template.MinCopperReward);
        int max = Math.Max(min, template.MaxCopperReward);
        return _rng.Next(min, max + 1) + Math.Max(0, enemyLevel - template.MinLevel);
    }
}
