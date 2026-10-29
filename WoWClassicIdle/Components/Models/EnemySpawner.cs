// Combat/EnemySpawner.cs

using Models;

public class EnemySpawner
{
    private readonly Random _rng;
    private readonly IReadOnlyList<EnemyTemplate> _extraTemplates;

    /// <param name="extraTemplates">Zusätzliche Templates für jede Zone (Debug-Monster), sofern ihr Levelbereich passt.</param>
    public EnemySpawner(IReadOnlyList<EnemyTemplate>? extraTemplates = null, Random? rng = null)
    {
        _extraTemplates = extraTemplates ?? [];
        _rng = rng ?? new Random();
    }

    public Enemy Spawn(int playerLevel, CombatZone zone)
    {
        // 1) Gegner-Level: Spieler-Level ± 2, auf den Levelbereich der Zone begrenzt
        //    (über dem Maxlevel der Zone spawnen nur noch Gegner der Maximalstufe)
        var enemyLevel = Math.Clamp(playerLevel + _rng.Next(-2, 3), zone.MinLevel, zone.MaxLevel);

        // 2) Gegnerarten der Zone (+ passende Extra-Templates)
        var candidates = CombatZoneRegistry.TemplatesFor(zone)
            .Concat(_extraTemplates.Where(t => enemyLevel >= t.MinLevel && enemyLevel <= t.MaxLevel))
            .ToList();

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

        // 10 % Chance auf Rare – nur wenn sowohl Spieler als auch Gegner mind. Level 3
        bool isRare = playerLevel >= 3 && enemyLevel >= 3 && _rng.NextDouble() < 0.10;

        int baseHp      = template.GetMaxHp(enemyLevel);
        int baseAttack  = template.GetAttack(enemyLevel);
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
            MobName  = template.Name,
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
