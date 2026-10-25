// Models/WeaponRegistry.cs
namespace Models;

/// <summary>
/// Starter-Waffen (Level 1) + klassenspezifische Tier-Waffen (grün Level 5–60, blau Level 10–60).
/// Grün: 25 % Dropchance. Blau: 10 % Dropchance, doppelte Stats, +10 % Schaden.
/// </summary>
public static class WeaponRegistry
{
    public static readonly WeaponItem WarriorStarter = new(5001, "Worn Shortsword",   "inv_sword_04",      1, 3, 2.3, WeaponType.Sword, SellPrice: 1, RequiredClass: CharacterClass.Warrior);
    public static readonly WeaponItem PaladinStarter = new(5002, "Battleworn Hammer", "inv_hammer_05",     2, 4, 2.9, WeaponType.Mace,  SellPrice: 1, RequiredClass: CharacterClass.Paladin);
    public static readonly WeaponItem HunterStarter  = new(5003, "Worn Shortbow",     "inv_weapon_bow_07", 1, 3, 3.4, WeaponType.Bow,   SellPrice: 1, RequiredClass: CharacterClass.Hunter);
    public static readonly WeaponItem RogueStarter   = new(5004, "Short Sword",       "inv_sword_04",      1, 3, 1.8, WeaponType.Sword, SellPrice: 1, RequiredClass: CharacterClass.Rogue);
    public static readonly WeaponItem PriestStarter  = new(5005, "Worn Wand",         "inv_wand_05",       1, 3, 1.9, WeaponType.Wand,  SellPrice: 1, RequiredClass: CharacterClass.Priest);
    public static readonly WeaponItem MageStarter    = new(5006, "Arcane Wand",       "inv_wand_05",       2, 4, 2.9, WeaponType.Wand,  SellPrice: 1, RequiredClass: CharacterClass.Mage);
    public static readonly WeaponItem WarlockStarter = new(5007, "Fel-Touched Wand",  "inv_wand_05",       1, 3, 1.6, WeaponType.Wand,  SellPrice: 1, RequiredClass: CharacterClass.Warlock);
    public static readonly WeaponItem ShamanStarter  = new(5008, "Worn Mace",         "inv_hammer_05",     1, 3, 1.8, WeaponType.Mace,  SellPrice: 1, RequiredClass: CharacterClass.Shaman);
    public static readonly WeaponItem DruidStarter   = new(5009, "Worn Mace",         "inv_hammer_05",     1, 3, 1.9, WeaponType.Mace,  SellPrice: 1, RequiredClass: CharacterClass.Druid);

    // Grüne Präfixe für Level 5, 10, 15 … 60 (12 Stufen)
    private static readonly string[] GreenPrefixes =
    [
        "Simple", "Fine", "Solid", "Reinforced", "Battle-Worn", "Honored",
        "Revered", "Exalted", "Champion's", "Warlord's", "Marshal's", "Grand Marshal's",
    ];

    // Blaue Präfixe für Level 10, 15 … 60 (11 Stufen, Tier 2–12)
    private static readonly string[] BluePrefixes =
    [
        "Gleaming", "Radiant", "Brilliant", "Lustrous", "Resplendent",
        "Illustrious", "Magnificent", "Exquisite", "Mythical", "Timeless", "Prismatic",
    ];

    private record ClassConfig(
        CharacterClass Class,
        WeaponItem Starter,
        string WeaponName,
        string Icon,
        WeaponType Type,
        Func<int, int, PrimaryStats> StatsForTier);

    private static readonly ClassConfig[] ClassConfigs =
    [
        new(CharacterClass.Warrior, WarriorStarter, "Shortsword",    "inv_sword_04",
            WeaponType.Sword, (s, sta) => new PrimaryStats { Strength = s, Agility = s, Stamina = sta }),

        new(CharacterClass.Paladin, PaladinStarter, "Hammer",        "inv_hammer_05",
            WeaponType.Mace,  (s, sta) => new PrimaryStats { Strength = s, Intelligence = s, Spirit = s, Stamina = sta }),

        new(CharacterClass.Hunter,  HunterStarter,  "Shortbow",      "inv_weapon_bow_07",
            WeaponType.Bow,   (s, sta) => new PrimaryStats { Agility = s, Intelligence = s, Stamina = sta }),

        new(CharacterClass.Rogue,   RogueStarter,   "Short Sword",   "inv_sword_04",
            WeaponType.Sword, (s, sta) => new PrimaryStats { Strength = s, Agility = s, Stamina = sta }),

        new(CharacterClass.Priest,  PriestStarter,  "Spirit Wand",   "inv_wand_05",
            WeaponType.Wand,  (s, sta) => new PrimaryStats { Intelligence = s, Spirit = s, Stamina = sta }),

        new(CharacterClass.Mage,    MageStarter,    "Arcane Wand",   "inv_wand_05",
            WeaponType.Wand,  (s, sta) => new PrimaryStats { Intelligence = s, Spirit = s, Stamina = sta }),

        new(CharacterClass.Warlock, WarlockStarter, "Fel Wand",      "inv_wand_05",
            WeaponType.Wand,  (s, sta) => new PrimaryStats { Intelligence = s, Spirit = s, Stamina = sta }),

        new(CharacterClass.Shaman,  ShamanStarter,  "Shaman's Mace", "inv_hammer_05",
            WeaponType.Mace,  (s, sta) => new PrimaryStats { Agility = s, Intelligence = s, Spirit = s, Stamina = sta }),

        new(CharacterClass.Druid,   DruidStarter,   "Druid's Mace",  "inv_hammer_05",
            WeaponType.Mace,  (s, sta) => new PrimaryStats { Strength = s, Intelligence = s, Spirit = s, Stamina = sta }),
    ];

    private static readonly Dictionary<int, WeaponItem> _byId;
    private static readonly Dictionary<CharacterClass, List<WeaponItem>> _tierByClass    = new();
    private static readonly Dictionary<CharacterClass, List<WeaponItem>> _blueTierByClass = new();

    static WeaponRegistry()
    {
        _byId = new()
        {
            [WarriorStarter.Id] = WarriorStarter,
            [PaladinStarter.Id] = PaladinStarter,
            [HunterStarter.Id]  = HunterStarter,
            [RogueStarter.Id]   = RogueStarter,
            [PriestStarter.Id]  = PriestStarter,
            [MageStarter.Id]    = MageStarter,
            [WarlockStarter.Id] = WarlockStarter,
            [ShamanStarter.Id]  = ShamanStarter,
            [DruidStarter.Id]   = DruidStarter,
        };

        // Grüne Tier-Waffen: 9 Klassen × 12 Stufen, IDs ab 5101
        int id = 5101;
        foreach (var cfg in ClassConfigs)
        {
            var list = new List<WeaponItem>();
            for (int tier = 0; tier < 12; tier++)
            {
                int reqLevel   = (tier + 1) * 5;
                double mult    = Math.Pow(1.1, tier + 1);
                int minDmg     = Math.Max(1, (int)Math.Round(cfg.Starter.MinDamage * mult));
                int maxDmg     = Math.Max(minDmg + 1, (int)Math.Round(cfg.Starter.MaxDamage * mult));
                int statVal    = 2 + tier;
                int stamVal    = 4 + tier * 2;
                long sell      = (long)(tier + 1) * 100;

                var w = new WeaponItem(id++, $"{GreenPrefixes[tier]} {cfg.WeaponName}", cfg.Icon,
                    minDmg, maxDmg, cfg.Starter.Speed, cfg.Type,
                    ItemQuality.Uncommon, sell, reqLevel, cfg.Class, cfg.StatsForTier(statVal, stamVal));
                list.Add(w);
                _byId[w.Id] = w;
            }
            _tierByClass[cfg.Class] = list;
        }

        // Blaue Tier-Waffen: 9 Klassen × 11 Stufen (Level 10–60), IDs ab 5301
        // Doppelte Stats, +10 % Schaden gegenüber der grünen Stufe, 10 % Dropchance
        id = 5301;
        foreach (var cfg in ClassConfigs)
        {
            var greenList = _tierByClass[cfg.Class];
            var blueList  = new List<WeaponItem>();

            // tier 0 = grüne Stufe 2 (Level 10) bis tier 10 = grüne Stufe 12 (Level 60)
            for (int blueIdx = 0; blueIdx < 11; blueIdx++)
            {
                var green  = greenList[blueIdx + 1];   // grüne Waffe für Level 10, 15 … 60
                int minDmg = Math.Max(1, (int)Math.Round(green.MinDamage * 1.1));
                int maxDmg = Math.Max(minDmg + 1, (int)Math.Round(green.MaxDamage * 1.1));
                var blueBonuses = green.Bonuses is { } b
                    ? new PrimaryStats { Strength = b.Strength * 2, Agility = b.Agility * 2,
                                         Stamina  = b.Stamina  * 2, Intelligence = b.Intelligence * 2,
                                         Spirit   = b.Spirit   * 2 }
                    : null;
                long sell = green.SellPrice * 4;

                var w = new WeaponItem(id++,
                    $"{BluePrefixes[blueIdx]} {cfg.WeaponName}", cfg.Icon,
                    minDmg, maxDmg, cfg.Starter.Speed, cfg.Type,
                    ItemQuality.Rare, sell, green.RequiredLevel, cfg.Class, blueBonuses);
                blueList.Add(w);
                _byId[w.Id] = w;
            }
            _blueTierByClass[cfg.Class] = blueList;
        }
    }

    public static WeaponItem? GetById(int id) => _byId.TryGetValue(id, out var w) ? w : null;

    /// <summary>Alle Waffen (grün + blau) aller Klassen mit genau diesem RequiredLevel (für Debug-Seiten).</summary>
    public static IReadOnlyList<WeaponItem> GetWeaponsAtLevel(int reqLevel)
    {
        var result = new List<WeaponItem>();
        foreach (var list in _tierByClass.Values)
        {
            var w = list.FirstOrDefault(x => x.RequiredLevel == reqLevel);
            if (w is not null) result.Add(w);
        }
        foreach (var list in _blueTierByClass.Values)
        {
            var w = list.FirstOrDefault(x => x.RequiredLevel == reqLevel);
            if (w is not null) result.Add(w);
        }
        return result;
    }

    public static WeaponItem GetStarter(CharacterClass cls) => cls switch
    {
        CharacterClass.Warrior => WarriorStarter,
        CharacterClass.Paladin => PaladinStarter,
        CharacterClass.Hunter  => HunterStarter,
        CharacterClass.Rogue   => RogueStarter,
        CharacterClass.Priest  => PriestStarter,
        CharacterClass.Mage    => MageStarter,
        CharacterClass.Warlock => WarlockStarter,
        CharacterClass.Shaman  => ShamanStarter,
        CharacterClass.Druid   => DruidStarter,
        _                      => WarriorStarter,
    };

    /// <summary>Höchste grüne Tier-Waffe mit RequiredLevel ≤ <paramref name="level"/>.</summary>
    public static WeaponItem? GetTierWeaponForLevel(CharacterClass cls, int level)
    {
        if (!_tierByClass.TryGetValue(cls, out var list)) return null;
        WeaponItem? best = null;
        foreach (var w in list)
            if (w.RequiredLevel <= level) best = w;
        return best;
    }

    /// <summary>Höchste blaue Tier-Waffe mit RequiredLevel ≤ <paramref name="level"/> (ab Level 10).</summary>
    public static WeaponItem? GetBlueTierWeaponForLevel(CharacterClass cls, int level)
    {
        if (!_blueTierByClass.TryGetValue(cls, out var list)) return null;
        WeaponItem? best = null;
        foreach (var w in list)
            if (w.RequiredLevel <= level) best = w;
        return best;
    }
}
