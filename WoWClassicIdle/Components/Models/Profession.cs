// Models/Profession.cs
namespace Models
{
    /// <summary>All professions: 8 crafting professions (per character) plus 3 gathering and 1 secondary profession (account-wide).</summary>
    public enum Profession
    {
        // Crafting professions (main professions, per character – incl. Cooking and First Aid)
        Alchemy,
        Engineering,
        Leatherworking,
        Blacksmithing,
        Tailoring,
        Enchanting,
        // Gathering professions (account-wide)
        Mining,
        Herbalism,
        Skinning,
        // Secondary professions (account-wide)
        Fishing,
        FirstAid,
        Cooking,
    }

    public enum ProfessionCategory { Crafting, Gathering, Secondary }

    /// <summary>Static description of a profession.</summary>
    /// <param name="IsGathering">true = passes yield zone drops, false = passes craft recipes.</param>
    public record ProfessionDefinition(
        Profession Profession,
        string Name,
        string Icon,
        ProfessionCategory Category,
        bool IsGathering)
    {
        public string IconUrl => $"https://wow.zamimg.com/images/wow/icons/large/{Icon}.jpg";
        /// <summary>Main profession = crafting profession, max. one per character. Gathering and secondary professions are shared account-wide.</summary>
        public bool IsPrimary => Category == ProfessionCategory.Crafting;

        /// <summary>All professions with zones (gathering + fishing) have passive slots (count per zone, <see cref="GatheringZone.PassiveSlotCount"/>).</summary>
        public bool HasPassiveSlots => IsGathering;

        /// <summary>URL segment of the subpage, e.g. "first-aid".</summary>
        public string Slug => Profession == Profession.FirstAid ? "first-aid" : Profession.ToString().ToLowerInvariant();
    }

    /// <summary>Training tier: raises the skill cap, learnable from the trainer from a character level and minimum skill for <see cref="Cost"/> copper.</summary>
    public record ProfessionRank(string Name, int MaxSkill, int RequiredLevel, int RequiredSkill = 1, long Cost = 0);

    public static class Professions
    {
        public const int MinCharacterLevel = 5;
        public const int MaxSkill = 300;

        /// <summary>Duration of a craft in the crafting queue (seconds).</summary>
        public const double ActiveDurationSeconds = 3.0;

        /// <summary>Duration of a pass per passive slot (seconds).</summary>
        public const double PassiveDurationSeconds = 90.0;

        /// <summary>Max. passively occupied slots per profession – across all zones combined.</summary>
        public const int MaxPassiveWorkers = 3;

        /// <summary>Chance for +1 skill per active pass (crafting, disenchanting).</summary>
        public const double SkillUpChance = 0.01;

        /// <summary>Chance for +1 skill per passive pass (gathering – the only source of gathering skill).</summary>
        public const double PassiveSkillUpChance = 0.05;

        /// <summary>Added to the crafting and disenchanting skill-up chance per full 10 levels summed across all characters.</summary>
        public const double SkillUpChancePerTenLevels = 0.002;

        public static double SkillUpBonus(int totalCharacterLevels) => totalCharacterLevels / 10 * SkillUpChancePerTenLevels;

        /// <summary>Chance as percent text for the UI, e.g. "1.2".</summary>
        public static string FormatChance(double chance) =>
            (chance * 100).ToString("0.##", System.Globalization.CultureInfo.InvariantCulture);

        /// <summary>Apprentice is granted automatically when learning, higher tiers are bought from the trainer.</summary>
        public static readonly IReadOnlyList<ProfessionRank> Ranks =
        [
            new("Apprentice",  75, 5),
            new("Journeyman", 150, 15, RequiredSkill: 50,  Cost: 100),     // 1 Silber
            new("Expert",     225, 25, RequiredSkill: 125, Cost: 1_000),   // 10 Silber
            new("Artisan",    300, 35, RequiredSkill: 200, Cost: 5_000),   // 50 Silber
        ];

        public static readonly IReadOnlyList<ProfessionDefinition> All =
        [
            new(Profession.Alchemy,        "Alchemy",        "trade_alchemy",              ProfessionCategory.Crafting,  false),
            new(Profession.Blacksmithing,  "Blacksmithing",  "trade_blacksmithing",        ProfessionCategory.Crafting,  false),
            new(Profession.Enchanting,     "Enchanting",     "trade_engraving",            ProfessionCategory.Crafting,  false),
            new(Profession.Engineering,    "Engineering",    "trade_engineering",          ProfessionCategory.Crafting,  false),
            new(Profession.Leatherworking, "Leatherworking", "inv_misc_armorkit_17",       ProfessionCategory.Crafting,  false),
            new(Profession.Tailoring,      "Tailoring",      "trade_tailoring",            ProfessionCategory.Crafting,  false),
            new(Profession.Herbalism,      "Herbalism",      "spell_nature_naturetouchgrow", ProfessionCategory.Gathering, true),
            new(Profession.Mining,         "Mining",         "trade_mining",               ProfessionCategory.Gathering, true),
            new(Profession.Skinning,       "Skinning",       "inv_misc_pelt_wolf_01",      ProfessionCategory.Gathering, true),
            new(Profession.Cooking,        "Cooking",        "inv_misc_food_15",           ProfessionCategory.Crafting,  false),
            new(Profession.FirstAid,       "First Aid",      "spell_holy_sealofsacrifice", ProfessionCategory.Crafting,  false),
            new(Profession.Fishing,        "Fishing",        "trade_fishing",              ProfessionCategory.Secondary, true),
        ];

        public static ProfessionDefinition Get(Profession profession) =>
            All.First(d => d.Profession == profession);

        public static ProfessionDefinition? FromSlug(string? slug) =>
            All.FirstOrDefault(d => string.Equals(d.Slug, slug, StringComparison.OrdinalIgnoreCase));
    }
}
