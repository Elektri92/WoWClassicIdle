// Models/ProfessionBonus.cs
namespace Models;

public enum ProfessionBonusKind { SkillChance, Speed, DoubleYield }

/// <summary>
/// Account-wide bonus of one profession. Derives from <see cref="Item"/> only for the loot toast –
/// it never enters an inventory and is saved by <see cref="Key"/>.
/// </summary>
public record ProfessionBonus(
    int Id,
    string Name,
    string Icon,
    ItemQuality Quality,
    Profession Profession,
    ProfessionBonusKind Kind,
    int Rank)
    : Item(Id, Name, Icon, Quality)
{
    public string Key => $"{Profession}.{Kind}.{Quality}";

    /// <summary>Skill-up chance in percentage points as a fraction, otherwise speed or double chance as a fraction.</summary>
    public double Value => Kind == ProfessionBonusKind.SkillChance ? Rank * 0.01 : Rank * 0.10;

    public string Description
    {
        get
        {
            var def = Professions.Get(Profession);
            string activity = def.IsGathering ? "gathering" : "crafting";
            int percent = (int)Math.Round(Value * 100);
            string text = Kind switch
            {
                ProfessionBonusKind.SkillChance => $"+{Rank} percentage point{(Rank > 1 ? "s" : "")} skill-up chance in {def.Name}",
                ProfessionBonusKind.Speed       => $"{percent}% faster {activity} in {def.Name}",
                _                               => $"{percent}% chance for double items when {activity} in {def.Name}",
            };
            return Profession == Profession.Enchanting ? $"{text}, including disenchanting" : text;
        }
    }
}

/// <summary>All profession bonuses: 12 professions × 3 kinds × 3 rarities.</summary>
public static class ProfessionBonusRegistry
{
    public const int SlotCount = 3;
    /// <summary>Chance per kill for one missing bonus of the zone.</summary>
    public const double DropChance = 0.01;
    public const int RareZoneMinLevel = 20;
    public const int EpicZoneMinLevel = 40;

    private const int FirstId = 20000;

    public static readonly IReadOnlyList<ItemQuality> Qualities = [ItemQuality.Uncommon, ItemQuality.Rare, ItemQuality.Epic];
    public static readonly IReadOnlyList<ProfessionBonusKind> Kinds =
        [ProfessionBonusKind.SkillChance, ProfessionBonusKind.Speed, ProfessionBonusKind.DoubleYield];

    private static readonly Dictionary<ItemQuality, string> Prefixes = new()
    {
        [ItemQuality.Uncommon] = "Apprentice's",
        [ItemQuality.Rare]     = "Expert's",
        [ItemQuality.Epic]     = "Master's",
    };

    // Name and icon per kind, in the order of Kinds
    private static readonly Dictionary<Profession, (string Name, string Icon)[]> Themes = new()
    {
        [Profession.Alchemy]        = [("Formula Notes", "inv_scroll_02"), ("Quicksilver Vial", "inv_potion_27"), ("Bubbling Cauldron", "inv_misc_cauldron_nature")],
        [Profession.Blacksmithing]  = [("Forge Plans", "inv_scroll_03"), ("Balanced Hammer", "inv_hammer_20"), ("Golden Ingot Mold", "inv_ingot_03")],
        [Profession.Enchanting]     = [("Arcane Treatise", "inv_misc_book_11"), ("Swift Runed Rod", "inv_wand_07"), ("Pouch of Arcane Dust", "inv_enchant_dustarcane")],
        [Profession.Engineering]    = [("Schematic Notes", "inv_misc_note_01"), ("Clockwork Timer", "inv_gizmo_01"), ("Spare Gears", "inv_misc_gear_01")],
        [Profession.Leatherworking] = [("Tanning Recipes", "inv_scroll_05"), ("Supple Leather Strap", "inv_misc_leatherscrap_08"), ("Cutting Pattern", "inv_misc_leatherscrap_02")],
        [Profession.Tailoring]      = [("Pattern Book", "inv_scroll_08"), ("Enchanted Thread", "inv_misc_thread_01"), ("Bolt of Silk", "inv_fabric_silk_01")],
        [Profession.Herbalism]      = [("Herbarium", "inv_misc_book_06"), ("Swiftleaf Charm", "inv_misc_herb_11"), ("Lucky Clover", "inv_misc_herb_goldclover")],
        [Profession.Mining]         = [("Prospector's Survey", "inv_misc_note_03"), ("Steelhead Pick", "inv_pick_02"), ("Rich Vein Nugget", "inv_ore_truesilver_01")],
        [Profession.Skinning]       = [("Anatomy Guide", "inv_misc_book_07"), ("Razor Skinning Knife", "inv_weapon_shortblade_05"), ("Flawless Hide", "inv_misc_pelt_bear_03")],
        [Profession.Fishing]        = [("Angler's Logbook", "inv_misc_book_09"), ("Swift-Cast Rod", "inv_fishingpole_02"), ("Twin-Hook Lure", "inv_misc_fish_02")],
        [Profession.Cooking]        = [("Recipe Collection", "inv_misc_book_01"), ("Quick-Rise Yeast", "inv_misc_food_33"), ("Hearty Portions", "inv_misc_food_45")],
        [Profession.FirstAid]       = [("Field Medic's Manual", "inv_misc_book_02"), ("Quick-Wrap Bandage", "inv_misc_bandage_12"), ("Spare Dressings", "inv_misc_bandage_15")],
    };

    /// <summary>Order is fixed by <see cref="Professions.All"/>, <see cref="Kinds"/> and <see cref="Qualities"/>.</summary>
    public static readonly IReadOnlyList<ProfessionBonus> All = Build();

    private static readonly Dictionary<string, ProfessionBonus> ByKey = All.ToDictionary(b => b.Key);

    // Each bonus drops in exactly one zone
    private static readonly Dictionary<ProfessionBonus, CombatZone> Zones = AssignZones();
    private static readonly ILookup<string, ProfessionBonus> ByZone = Zones.ToLookup(e => e.Value.Id, e => e.Key);

    private static List<ProfessionBonus> Build()
    {
        var list = new List<ProfessionBonus>();
        int id = FirstId;
        foreach (var def in Professions.All)
            foreach (var (kind, kindIndex) in Kinds.Select((k, i) => (k, i)))
                foreach (var (quality, qualityIndex) in Qualities.Select((q, i) => (q, i)))
                {
                    var (name, icon) = Themes[def.Profession][kindIndex];
                    list.Add(new ProfessionBonus(id++, $"{Prefixes[quality]} {name}", icon, quality, def.Profession, kind, qualityIndex + 1));
                }
        return list;
    }

    /// <summary>
    /// Distributes each rarity evenly over its zones. The large step sends the bonuses of one profession
    /// to zones far apart in the list, so collecting them means real travel. Only depends on the static data.
    /// </summary>
    private static Dictionary<ProfessionBonus, CombatZone> AssignZones()
    {
        var zones = new Dictionary<ProfessionBonus, CombatZone>();
        foreach (var quality in Qualities)
        {
            var pool = CombatZoneRegistry.All.Where(z => QualityFor(z) == quality).ToList();
            // Coprime with the zone count, otherwise some zones would never be visited
            int step = Enumerable.Range(pool.Count / 3, pool.Count).First(s => Gcd(s, pool.Count) == 1);
            foreach (var (bonus, index) in All.Where(b => b.Quality == quality).Select((b, i) => (b, i)))
                zones[bonus] = pool[index * step % pool.Count];
        }
        return zones;
    }

    private static int Gcd(int a, int b) => b == 0 ? a : Gcd(b, a % b);

    public static CombatZone ZoneOf(ProfessionBonus bonus) => Zones[bonus];

    public static IEnumerable<ProfessionBonus> ForZone(CombatZone zone) => ByZone[zone.Id];

    public static ProfessionBonus? GetByKey(string? key) =>
        key is not null && ByKey.TryGetValue(key, out var bonus) ? bonus : null;

    public static IEnumerable<ProfessionBonus> ForProfession(Profession profession) =>
        All.Where(b => b.Profession == profession);

    /// <summary>Rarity of a zone's bonuses: start zones and their follow-ups, then mid-level zones, then high-level zones.</summary>
    public static ItemQuality QualityFor(CombatZone zone) =>
        zone.MinLevel < RareZoneMinLevel ? ItemQuality.Uncommon
        : zone.MinLevel < EpicZoneMinLevel ? ItemQuality.Rare
        : ItemQuality.Epic;
}
