// Debug/SimStarterGear.cs – only compiled in the DEBUG build (see .csproj)
namespace DebugTools;

using Models;

/// <summary>
/// Equips a simulation character with the best green (Uncommon) gear available at its level:
/// real items from the green loot tables and the crafted sets of its class. Weapon: best green tier weapon.
/// The simulator has no gear drops, so the gear only changes through this class.
/// </summary>
internal static class SimStarterGear
{
    private static readonly string[] StatNames = ["Strength", "Agility", "Stamina", "Spirit", "Intellect"];

    /// <summary>Stat priorities measured per character: each single stat and each pair of two stats.</summary>
    private static readonly PrimaryStats[] Priorities =
    [
        .. Enumerable.Range(0, StatNames.Length).Select(a => StatsOf(a)),
        .. Enumerable.Range(0, StatNames.Length)
            .SelectMany(a => Enumerable.Range(a + 1, StatNames.Length - a - 1).Select(b => StatsOf(a, b))),
    ];

    /// <summary>Equips the best green gear of the level with the spec's usual stats (starting point before talents and loadout).</summary>
    public static PrimaryStats Equip(Player player, CharacterClass cls, int specIndex, int level, DruidForm? form = null)
    {
        // Bear tanks like a Protection Warrior → Str+Sta instead of the default Feral combination
        var priority = form == DruidForm.Bear
            ? new PrimaryStats { Strength = 1, Stamina = 1 }
            : DefaultPriority(cls, specIndex);
        EquipFor(player, cls, level, priority);
        return priority;
    }

    /// <summary>Equips the green gear of the level that yields the most XP/h with the current loadout.</summary>
    public static void EquipBest(Player player, CharacterClass cls, int level, CancellationToken ct)
    {
        var ids = player.GetCurrentLoadout().EquippedAbilityIds.ToList();
        var candidates = Priorities.Select(p => BuildSet(cls, level, p)).DistinctBy(SetKey).ToList();

        IReadOnlyList<ArmorItem> best = candidates[0];
        double bestScore = double.MinValue;
        foreach (var set in candidates)
        {
            Apply(player, cls, level, set);
            double score = SimLoadoutOptimizer.Evaluate(player, ids, ct);
            if (score > bestScore)
            {
                bestScore = score;
                best      = set;
            }
        }
        Apply(player, cls, level, best);
    }

    /// <summary>Changes when a new level offers new green items – otherwise the gear does not need to be measured again.</summary>
    public static string AvailabilityKey(CharacterClass cls, int level) =>
        $"{AvailableItems(cls, level).Count}/{WeaponRegistry.GetTierWeaponForLevel(cls, level)?.Id}";

    private static void EquipFor(Player player, CharacterClass cls, int level, PrimaryStats priority) =>
        Apply(player, cls, level, BuildSet(cls, level, priority));

    private static void Apply(Player player, CharacterClass cls, int level, IReadOnlyList<ArmorItem> set)
    {
        if (WeaponRegistry.GetTierWeaponForLevel(cls, level) is { } weapon)
            player.ForceEquipWeapon(weapon);

        player.EquippedArmor.Clear();
        foreach (var item in set)
            player.ForceEquipArmor(item);
    }

    /// <summary>Best item per slot for the priority: most prioritized stats, then most stats in total, then the widest spread.</summary>
    private static IReadOnlyList<ArmorItem> BuildSet(CharacterClass cls, int level, PrimaryStats priority) =>
        AvailableItems(cls, level)
            .GroupBy(i => i.Slot)
            .Select(slot => slot
                .OrderByDescending(i => Score(i.Bonuses, priority))
                .ThenByDescending(i => Sum(i.Bonuses))
                .ThenByDescending(i => Covered(i.Bonuses, priority))
                .ThenBy(i => i.Id)
                .First())
            .ToList();

    private static List<ArmorItem> AvailableItems(CharacterClass cls, int level)
    {
        var drops = Enumerable.Range(1, Math.Max(1, level))
            .Select(RareLootRegistry.GetLootTable)
            .Distinct()
            .SelectMany(table => table);
        var crafted = CraftedGearRegistry.Items.Where(i => i.RequiredClass == cls);
        return drops.Concat(crafted)
            .Where(i => i.Quality == ItemQuality.Uncommon && i.RequiredLevel <= level)
            .ToList();
    }

    private static string SetKey(IReadOnlyList<ArmorItem> set) =>
        string.Join(",", set.Select(i => i.Id).Order());

    private static int Score(PrimaryStats b, PrimaryStats p) =>
        b.Strength * p.Strength + b.Agility * p.Agility + b.Stamina * p.Stamina + b.Spirit * p.Spirit + b.Intelligence * p.Intelligence;

    private static int Sum(PrimaryStats b) =>
        b.Strength + b.Agility + b.Stamina + b.Spirit + b.Intelligence;

    private static int Covered(PrimaryStats b, PrimaryStats p) =>
        (b.Strength > 0 && p.Strength > 0 ? 1 : 0) + (b.Agility > 0 && p.Agility > 0 ? 1 : 0) + (b.Stamina > 0 && p.Stamina > 0 ? 1 : 0)
        + (b.Spirit > 0 && p.Spirit > 0 ? 1 : 0) + (b.Intelligence > 0 && p.Intelligence > 0 ? 1 : 0);

    private static PrimaryStats StatsOf(params int[] stats) => new()
    {
        Strength     = stats.Contains(0) ? 1 : 0,
        Agility      = stats.Contains(1) ? 1 : 0,
        Stamina      = stats.Contains(2) ? 1 : 0,
        Spirit       = stats.Contains(3) ? 1 : 0,
        Intelligence = stats.Contains(4) ? 1 : 0,
    };

    // Spec-appropriate 2-stat combination as the starting point
    private static PrimaryStats DefaultPriority(CharacterClass cls, int specIndex) =>
        (cls, specIndex) switch
        {
            // Warrior: Arms(0)/Fury(1) → Str+Agi; Protection(2) → Str+Sta
            (CharacterClass.Warrior, 2) => new PrimaryStats { Strength = 1, Stamina      = 1 },
            (CharacterClass.Warrior, _) => new PrimaryStats { Strength = 1, Agility      = 1 },

            // Paladin: Holy(0) → Int+Spi; Protection(1) → Str+Sta; Retribution(2) → Str+Agi
            (CharacterClass.Paladin, 0) => new PrimaryStats { Intelligence = 1, Spirit   = 1 },
            (CharacterClass.Paladin, 1) => new PrimaryStats { Strength = 1, Stamina      = 1 },
            (CharacterClass.Paladin, 2) => new PrimaryStats { Strength = 1, Agility      = 1 },

            // Hunter: Beast Mastery(0)/Marksmanship(1)/Survival(2) → Agi+Sta
            (CharacterClass.Hunter,  _) => new PrimaryStats { Agility  = 1, Stamina      = 1 },

            // Rogue: all specs → Agi+Str
            (CharacterClass.Rogue,   _) => new PrimaryStats { Agility  = 1, Strength     = 1 },

            // Shaman: Enhancement(1) → Str+Sta (before Windfury it casts like a caster without stamina)
            (CharacterClass.Shaman,  1) => new PrimaryStats { Strength = 1, Stamina      = 1 },

            // Druid: Feral Combat(1) → Agi+Str
            (CharacterClass.Druid,   1) => new PrimaryStats { Agility  = 1, Strength     = 1 },

            // Casters and the remaining specs → Int+Spi
            _ => new PrimaryStats { Intelligence = 1, Spirit = 1 },
        };
}
