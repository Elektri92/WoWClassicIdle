// Models/DungeonDefinition.cs
namespace Models;

/// <summary>
/// One step of a dungeon run: a trash group (<paramref name="Kills"/> enemies from the mobs of the dungeon)
/// or a boss with its index into the loot tables.
/// </summary>
public record DungeonEncounter(int Kills, ZoneMob? Boss = null, int BossIndex = -1)
{
    public bool IsBoss => Boss is not null;
}

/// <summary>Run data of a dungeon; <paramref name="EntranceZoneId"/> = zone a character ends up in after leaving involuntarily.</summary>
public record DungeonDefinition(string EntranceZoneId, IReadOnlyList<DungeonEncounter> Encounters)
{
    public IEnumerable<ZoneMob> Bosses => Encounters.Where(e => e.IsBoss).Select(e => e.Boss!);
}

/// <summary>Progress of a running dungeon run of a character.</summary>
public class DungeonRunState
{
    public int EncounterIndex   { get; internal set; }
    public int KillsInEncounter { get; internal set; }
    public int Wipes            { get; internal set; }
    /// <summary>NPC group of this run; fixed when the run starts.</summary>
    public IReadOnlyList<DungeonPartyMemberState> Party { get; internal set; } = [];
    /// <summary>Slot the character itself fills in this run.</summary>
    public DungeonRole SelfRole { get; internal set; }
}

/// <summary>Rules and balance values shared by all dungeons.</summary>
public static class DungeonRules
{
    public const int RunsPerWindow = 3;
    public static readonly TimeSpan LockoutWindow = TimeSpan.FromHours(12);

    /// <summary>The run fails on this death – the character stands in the entrance zone again.</summary>
    public const int MaxWipes = 3;

    /// <summary>The journey home after a run is this many times as fast as a normal journey.</summary>
    public const double ReturnTravelSpeed = 2.0;

    // Average output of an NPC: damage dealers per second as a share of the default enemy HP, healers as a share of the player's HP
    public const double NpcDamagePerSecond = 0.01;
    public const double HealerHpPerSecond  = 0.012;

    // A character filling the tank or healer slot itself does that job: otherwise the group would lack it completely
    public const double SelfTankReduction = 0.30;

    // Filling the tank or healer slot leaves three companion damage dealers instead of two:
    // together they deal what two do, otherwise those runs would be faster and give more XP per hour
    public const double CompanionDamageWithoutSelfDps = 2.0 / 3.0;

    public const double TrashHpFactor     = 1.6;
    public const double TrashAttackFactor = 1.4;
    public const double BossHpFactor      = 3.2;
    public const double BossAttackFactor  = 2.0;

    // Kill XP depends only on the level; these factors keep the XP/h in line with open zones (measured in /debug/dungeon-sim)
    public const double TrashXpMultiplier = 0.70;
    public const double BossXpMultiplier  = 1.33;
}

/// <summary>Builds the enemies of a dungeon – used by the engine and the debug simulation alike.</summary>
public static class DungeonEnemies
{
    public static Enemy Build(CombatZone zone, DungeonEncounter encounter, int playerLevel, Random rng)
    {
        var mob = encounter.Boss ?? RollTrash(zone, rng);
        bool boss = encounter.IsBoss;
        int level = Math.Clamp(playerLevel + (boss ? 1 : 0), zone.MinLevel, zone.MaxLevel);

        double hpFactor     = boss ? DungeonRules.BossHpFactor : DungeonRules.TrashHpFactor;
        double attackFactor = boss ? DungeonRules.BossAttackFactor : DungeonRules.TrashAttackFactor;

        var stats = new Stats
        {
            MaxHp        = (int)Math.Round(EnemyScaling.Hp(level, mob.Archetype) * hpFactor),
            Attack       = (int)Math.Ceiling(EnemyScaling.Attack(level, mob.Archetype) * attackFactor),
            Defense      = mob.Archetype.DefenseAt(level),
            Speed        = mob.Archetype.Speed,
            Level        = level,
            CopperReward = rng.Next(EnemyScaling.MinCopper(level), EnemyScaling.MaxCopper(level) + 1),
            XpReward     = StaticHelpers.GetXpForLevel(level),
        };

        return new Enemy(stats)
        {
            Name         = mob.Name,
            MobName      = mob.Name,
            Type         = mob.Archetype.Type,
            ImageUrl     = mob.ImageUrl,
            IsBoss       = boss,
            FromDungeon  = true,
            XpMultiplier = boss ? DungeonRules.BossXpMultiplier : DungeonRules.TrashXpMultiplier,
        };
    }

    private static ZoneMob RollTrash(CombatZone zone, Random rng)
    {
        int roll = rng.Next(zone.Mobs.Sum(m => m.Weight));
        foreach (var mob in zone.Mobs)
        {
            roll -= mob.Weight;
            if (roll < 0) return mob;
        }
        return zone.Mobs[0];
    }
}
