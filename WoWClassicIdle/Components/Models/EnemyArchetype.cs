// Models/EnemyArchetype.cs
namespace Models;

/// <summary>
/// Stat profile of an enemy type (wolf, bear, troll …) – multipliers and fixed values from the former enemy scaling.
/// The actual level values are calculated by <see cref="EnemyScaling"/>.
/// </summary>
/// <param name="Type">Creature type for combat animation and cloth drops ("Humanoid" → cloth).</param>
/// <param name="HpMultiplier">Factor on the default HP curve.</param>
/// <param name="AttackBonus">Bonus on the default attack formula.</param>
/// <param name="Defense">Armor up to level 34.</param>
/// <param name="DefenseLate">Armor from level 35 (thicker fur, older dragons).</param>
/// <param name="Speed">Angriffsintervall in Sekunden.</param>
public record EnemyArchetype(string Type, double HpMultiplier, int AttackBonus, int Defense, int DefenseLate, double Speed)
{
    public int DefenseAt(int level) => level >= 35 ? DefenseLate : Defense;
}

/// <summary>All enemy types (values see the enemy scaling table).</summary>
public static class Archetypes
{
    // ── Tiere ──
    public static readonly EnemyArchetype Wolf      = new("Wolf",      1.00,  0, 0, 0, 3.3);
    public static readonly EnemyArchetype Boar      = new("Boar",      1.00, -1, 0, 0, 3.0);
    public static readonly EnemyArchetype Bear      = new("Bear",      1.15,  1, 1, 3, 2.5);
    public static readonly EnemyArchetype Cat       = new("Cat",       0.95,  1, 0, 0, 2.2);
    public static readonly EnemyArchetype Spider    = new("Spider",    1.00,  0, 0, 1, 2.2);
    public static readonly EnemyArchetype Scorpid   = new("Scorpid",   1.00,  0, 1, 1, 2.0);
    public static readonly EnemyArchetype Raptor    = new("Raptor",    1.00,  1, 1, 1, 2.4);
    public static readonly EnemyArchetype Crocolisk = new("Crocolisk", 1.10,  0, 2, 2, 2.8);
    public static readonly EnemyArchetype Bird      = new("Bird",      0.90,  0, 0, 0, 2.0);
    public static readonly EnemyArchetype Hyena     = new("Hyena",     1.00,  0, 0, 0, 2.4);
    public static readonly EnemyArchetype Gorilla   = new("Gorilla",   1.15,  1, 1, 1, 2.6);
    public static readonly EnemyArchetype Kodo      = new("Kodo",      1.20,  1, 1, 2, 3.2);
    public static readonly EnemyArchetype Basilisk  = new("Basilisk",  1.10,  0, 2, 2, 2.6);
    public static readonly EnemyArchetype Insect    = new("Insect",    1.00,  0, 2, 2, 2.2);
    public static readonly EnemyArchetype Murloc    = new("Murloc",    0.93,  0, 0, 0, 2.6);

    // ── Humanoids (drop cloth) ──
    public static readonly EnemyArchetype Humanoid  = new("Humanoid",  1.00,  0, 1, 1, 2.4);
    public static readonly EnemyArchetype Goblin    = new("Humanoid",  0.95,  0, 0, 0, 2.2);
    public static readonly EnemyArchetype Troll     = new("Humanoid",  1.15,  0, 1, 1, 2.5);
    public static readonly EnemyArchetype Ogre      = new("Humanoid",  1.20,  2, 1, 1, 3.0);
    public static readonly EnemyArchetype DarkIron  = new("Humanoid",  1.05,  0, 4, 4, 2.8);

    // ── Sonstige ──
    public static readonly EnemyArchetype Undead    = new("Undead",    1.05,  0, 1, 1, 2.6);
    public static readonly EnemyArchetype Demon     = new("Demon",     1.10,  1, 2, 2, 2.4);
    public static readonly EnemyArchetype Elemental = new("Elemental", 1.10,  0, 2, 2, 2.7);
    public static readonly EnemyArchetype Dragon    = new("Dragon",    1.20,  2, 2, 4, 2.0);
    public static readonly EnemyArchetype Ooze      = new("Ooze",      1.10,  0, 2, 2, 3.1);
}
