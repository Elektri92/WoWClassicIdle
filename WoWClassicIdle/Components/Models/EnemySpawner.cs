// Combat/EnemySpawner.cs

using Models;

public class EnemySpawner
{
    private readonly Random _rng;
    private readonly IReadOnlyList<EnemyTemplate> _extraTemplates;

    /// <param name="extraTemplates">Additional templates for every zone (debug monsters), provided their level range fits.</param>
    public EnemySpawner(IReadOnlyList<EnemyTemplate>? extraTemplates = null, Random? rng = null)
    {
        _extraTemplates = extraTemplates ?? [];
        _rng = rng ?? new Random();
    }

    public Enemy Spawn(int playerLevel, CombatZone zone)
    {
        // 1) Enemy level: player level ± 2, clamped to the level range of the zone
        //    (above the zone's max level only enemies of the max level spawn)
        var enemyLevel = Math.Clamp(playerLevel + _rng.Next(-2, 3), zone.MinLevel, zone.MaxLevel);

        // 2) Enemy types of the zone (+ matching extra templates)
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

        // 10 % chance for a rare – only if both player and enemy are at least level 3
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

    // A few copper per kill: range from the template, +1 copper per level above MinLevel
    private int RollCopper(EnemyTemplate template, int enemyLevel)
    {
        int min = Math.Max(0, template.MinCopperReward);
        int max = Math.Max(min, template.MaxCopperReward);
        return _rng.Next(min, max + 1) + Math.Max(0, enemyLevel - template.MinLevel);
    }
}
