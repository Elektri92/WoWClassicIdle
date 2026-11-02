// Models/ClassAbilityRegistry.Ranks.cs
namespace Models;

// Higher ranks are generated from the highest hand-written rank of every ability with flat values:
// one rank every 10 levels plus one at Lv 60. Percent effects (forms, stances, most buffs) don't get ranks.
public static partial class ClassAbilityRegistry
{
    private const int RankLevelInterval = 10;
    private const int FinalRankLevel = 60;

    // Talent abilities without their own level are listed at Lv 1 but can't be skilled before the first talent point
    private const int FirstTalentLevel = 10;

    private static IReadOnlyList<Ability> WithGeneratedRanks(CharacterClass cls, IEnumerable<Ability> handWritten)
    {
        var abilities = handWritten.ToList();
        foreach (var family in abilities.GroupBy(a => a.Name).ToList())
        {
            var top = family.MaxBy(a => a.Rank)!;
            if (!HasFlatValues(top)) continue;

            // Some abilities with a single rank don't state one
            if (top.Rank < 1)
            {
                int index = abilities.IndexOf(top);
                top = top with { Rank = 1 };
                abilities[index] = top;
            }
            abilities.AddRange(GenerateRanks(cls, top));
        }
        return abilities;
    }

    private static IEnumerable<Ability> GenerateRanks(CharacterClass cls, Ability top)
    {
        int firstLevel = top.TalentUnlockOnly ? Math.Max(top.LevelRequired, FirstTalentLevel) : top.LevelRequired;
        var levels = RankLevelsAfter(firstLevel);
        if (levels.Count == 0) yield break;

        int topReference = ReferenceLevel(firstLevel, levels[0]);
        string idBase = StripRankSuffix(top.Id);
        var previous = top;
        for (int i = 0; i < levels.Count; i++)
        {
            int reference = i + 1 < levels.Count ? ReferenceLevel(levels[i], levels[i + 1]) : levels[i];
            previous = ScaleRank(cls, top, previous, topReference, reference) with
            {
                Id            = $"{idBase}_r{previous.Rank + 1}",
                Rank          = previous.Rank + 1,
                LevelRequired = levels[i],
            };
            yield return previous;
        }
    }

    private static List<int> RankLevelsAfter(int level)
    {
        var levels = new List<int>();
        for (int l = level + RankLevelInterval; l < FinalRankLevel; l += RankLevelInterval)
            levels.Add(l);
        if (level < FinalRankLevel) levels.Add(FinalRankLevel);
        return levels;
    }

    // A rank should be on curve halfway to the next one, so fights take equally long in every rank's level band
    private static int ReferenceLevel(int rankLevel, int nextRankLevel) => (rankLevel + nextRankLevel) / 2;

    private static Ability ScaleRank(CharacterClass cls, Ability top, Ability previous, int topReference, int reference)
    {
        double damage = (double)EnemyScaling.BaseHpAt(reference) / EnemyScaling.BaseHpAt(topReference);
        double health = PlayerHealthCurve(reference) / PlayerHealthCurve(topReference);
        double mana   = PlayerManaCurve(reference) / PlayerManaCurve(topReference);
        // Rage and energy pools don't grow with the level
        bool costsMana = ShapeshiftForms.ResourceFor(cls, top.RequiredForm) == ResourceKind.Mana;

        return top with
        {
            Damage                = Scale(top.Damage, previous.Damage, damage),
            DebuffDotDamage       = Scale(top.DebuffDotDamage, previous.DebuffDotDamage, damage),
            BuffRetaliationDamage = Scale(top.BuffRetaliationDamage, previous.BuffRetaliationDamage, damage),
            Heal                  = Scale(top.Heal, previous.Heal, health),
            HotTotalHeal          = Scale(top.HotTotalHeal, previous.HotTotalHeal, health),
            BuffHealOnHit         = Scale(top.BuffHealOnHit, previous.BuffHealOnHit, health),
            BuffIntellectBonus    = Scale(top.BuffIntellectBonus, previous.BuffIntellectBonus, health),
            BuffSpiritBonus       = Scale(top.BuffSpiritBonus, previous.BuffSpiritBonus, health),
            LifeTapHpCost         = Scale(top.LifeTapHpCost, previous.LifeTapHpCost, health),
            LifeTapManaGain       = Scale(top.LifeTapManaGain, previous.LifeTapManaGain, mana),
            ManaCost              = costsMana ? Scale(top.ManaCost, previous.ManaCost, mana) : top.ManaCost,
        };
    }

    // Every rank is at least one point stronger, even where rounding would swallow the step
    private static int Scale(int topValue, int previousValue, double factor) =>
        topValue == 0 ? 0 : Math.Max(previousValue + 1, (int)Math.Round(topValue * factor));

    private static bool HasFlatValues(Ability a) =>
        a.Damage > 0 || a.Heal > 0 || a.HotTotalHeal > 0 || a.DebuffDotDamage > 0 || a.BuffRetaliationDamage > 0
        || a.BuffHealOnHit > 0 || a.BuffIntellectBonus > 0 || a.BuffSpiritBonus > 0 || a.IsLifeTap;

    private static string StripRankSuffix(string id)
    {
        int index = id.LastIndexOf("_r", StringComparison.Ordinal);
        return index > 0 && int.TryParse(id.AsSpan(index + 2), out _) ? id[..index] : id;
    }

    // Measured with the green gear of the simulator (all classes, Lv 20–60)
    private static double PlayerHealthCurve(int level) => 40 + 8.5 * level;
    private static double PlayerManaCurve(int level) => 300 + 18.0 * level;

    /// <summary>Highest rank of the ability's family that is learned at <paramref name="level"/>.</summary>
    public static Ability HighestRank(Ability ability, int level) =>
        Abilities.Values
            .FirstOrDefault(list => list.Any(a => a.Id == ability.Id))?
            .Where(a => a.Name == ability.Name && a.LevelRequired <= level)
            .MaxBy(a => a.Rank) ?? ability;

    /// <summary>IDs of all ranks of the ability with <paramref name="id"/>; a talent that grants the ability grants all of them.</summary>
    public static IEnumerable<string> AllRankIds(string id)
    {
        foreach (var list in Abilities.Values)
            if (list.FirstOrDefault(a => a.Id == id) is { } ability)
                return list.Where(a => a.Name == ability.Name).Select(a => a.Id);
        return [id];
    }
}
