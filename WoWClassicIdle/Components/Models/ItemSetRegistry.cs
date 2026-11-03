// Models/ItemSetRegistry.cs
namespace Models;

/// <summary>
/// Armor set with bonuses at 2, 3 and 5 worn pieces (cumulative). Only the listed classes can wear its pieces.
/// </summary>
public record ItemSet(
    string Id,
    string Name,
    IReadOnlyList<CharacterClass> Classes,
    PrimaryStats TwoPiece,
    PrimaryStats ThreePiece,
    double FivePieceCrit);

/// <summary>All item sets; the pieces themselves live in <see cref="DungeonLootRegistry"/>.</summary>
public static class ItemSetRegistry
{
    public const int PiecesPerSet = 5;

    public const string DeadminesCaster = "dm_caster";
    public const string DeadminesMelee  = "dm_melee";
    public const string DeadminesHybrid = "dm_hybrid";
    public const string WailingCaster   = "wc_caster";
    public const string WailingMelee    = "wc_melee";
    public const string WailingHybrid   = "wc_hybrid";
    public const string ScarletCaster   = "sm_caster";
    public const string ScarletMelee    = "sm_melee";
    public const string ScarletHybrid   = "sm_hybrid";

    private static readonly CharacterClass[] Casters = [CharacterClass.Mage, CharacterClass.Warlock, CharacterClass.Priest];
    private static readonly CharacterClass[] Melees  = [CharacterClass.Warrior, CharacterClass.Rogue, CharacterClass.Hunter];
    private static readonly CharacterClass[] Hybrids = [CharacterClass.Shaman, CharacterClass.Paladin, CharacterClass.Druid];

    private static readonly PrimaryStats IntTwo   = new() { Intelligence = 8 };
    private static readonly PrimaryStats SpiThree = new() { Spirit = 8, Stamina = 8 };

    // Scarlet Monastery drops around level 35–40, so its bonuses are half as large again
    private static readonly PrimaryStats ScarletIntTwo   = new() { Intelligence = 12 };
    private static readonly PrimaryStats ScarletSpiThree = new() { Spirit = 12, Stamina = 12 };

    private const double FiveCrit = 0.02;
    private const double ScarletFiveCrit = 0.03;

    private static readonly Dictionary<string, ItemSet> ById = new ItemSet[]
    {
        new(DeadminesCaster, "Vestments of the Defias Conjurer", Casters, IntTwo, SpiThree, FiveCrit),
        new(DeadminesMelee,  "Blackened Defias Armor",           Melees,  new() { Strength = 8 }, new() { Agility = 8, Stamina = 8 }, FiveCrit),
        new(DeadminesHybrid, "Bulwark of Moonbrook",             Hybrids, IntTwo, SpiThree, FiveCrit),
        new(WailingCaster,   "Raiment of the Fanglord",          Casters, IntTwo, SpiThree, FiveCrit),
        new(WailingMelee,    "Deviate Hunter's Scales",          Melees,  new() { Agility = 8 }, new() { Strength = 8, Stamina = 8 }, FiveCrit),
        new(WailingHybrid,   "Embrace of the Viper",             Hybrids, IntTwo, SpiThree, FiveCrit),
        new(ScarletCaster,   "Vestments of the Inquisition",     Casters, ScarletIntTwo, ScarletSpiThree, ScarletFiveCrit),
        new(ScarletMelee,    "Chain of the Scarlet Crusade",     Melees,  new() { Strength = 12 }, new() { Agility = 12, Stamina = 12 }, ScarletFiveCrit),
        new(ScarletHybrid,   "Battlegear of the Crimson Vanguard", Hybrids, ScarletIntTwo, ScarletSpiThree, ScarletFiveCrit),
    }.ToDictionary(s => s.Id);

    public static ItemSet? Get(string? id) => id is not null && ById.TryGetValue(id, out var set) ? set : null;

    public static IEnumerable<ItemSet> All => ById.Values;

    /// <summary>Stats and crit from all sets in the given worn armor.</summary>
    public static (PrimaryStats Stats, double Crit) ActiveBonus(IEnumerable<ArmorItem> worn)
    {
        var stats = PrimaryStats.Zero;
        double crit = 0;
        foreach (var group in worn.Where(a => a.SetId is not null).GroupBy(a => a.SetId))
        {
            if (Get(group.Key) is not { } set) continue;
            int count = group.Count();
            if (count >= 2) stats += set.TwoPiece;
            if (count >= 3) stats += set.ThreePiece;
            if (count >= 5) crit  += set.FivePieceCrit;
        }
        return (stats, crit);
    }
}
