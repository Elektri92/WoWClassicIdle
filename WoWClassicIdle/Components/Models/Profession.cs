// Models/Profession.cs
namespace Models
{
    /// <summary>Alle Berufe: 8 Herstellungsberufe (je Charakter) sowie 3 Sammel- und 1 Sekundärberuf (accountweit).</summary>
    public enum Profession
    {
        // Herstellungsberufe (Hauptberufe, je Charakter – inkl. Kochen und Erste Hilfe)
        Alchemy,
        Engineering,
        Leatherworking,
        Blacksmithing,
        Tailoring,
        Enchanting,
        // Sammelberufe (accountweit)
        Mining,
        Herbalism,
        Skinning,
        // Sekundärberufe (accountweit)
        Fishing,
        FirstAid,
        Cooking,
    }

    public enum ProfessionCategory { Crafting, Gathering, Secondary }

    /// <summary>Statische Beschreibung eines Berufs.</summary>
    /// <param name="IsGathering">true = Durchläufe liefern Zonen-Drops, false = Durchläufe stellen Rezepte her.</param>
    public record ProfessionDefinition(
        Profession Profession,
        string Name,
        string Icon,
        ProfessionCategory Category,
        bool IsGathering)
    {
        public string IconUrl => $"https://wow.zamimg.com/images/wow/icons/large/{Icon}.jpg";
        /// <summary>Hauptberuf = Herstellungsberuf, max. einer je Charakter. Sammel- und Sekundärberufe sind accountweit geteilt.</summary>
        public bool IsPrimary => Category == ProfessionCategory.Crafting;

        /// <summary>Alle Berufe mit Zonen (Sammeln + Angeln) haben passive Slots (Anzahl je Zone, <see cref="GatheringZone.PassiveSlotCount"/>).</summary>
        public bool HasPassiveSlots => IsGathering;

        /// <summary>URL-Segment der Unterseite, z. B. "first-aid".</summary>
        public string Slug => Profession == Profession.FirstAid ? "first-aid" : Profession.ToString().ToLowerInvariant();
    }

    /// <summary>Ausbildungsstufe: hebt das Skill-Cap an, lernbar ab einem Charakterlevel und Mindest-Skill.</summary>
    public record ProfessionRank(string Name, int MaxSkill, int RequiredLevel, int RequiredSkill = 1);

    public static class Professions
    {
        public const int MinCharacterLevel = 5;
        public const int MaxSkill = 300;

        /// <summary>Dauer eines Durchlaufs im aktiven Slot (Sekunden).</summary>
        public const double ActiveDurationSeconds = 3.0;

        /// <summary>Dauer eines Durchlaufs je passivem Slot (Sekunden).</summary>
        public const double PassiveDurationSeconds = 120.0;

        /// <summary>Max. passiv belegte Slots je Beruf – über alle Zonen zusammen.</summary>
        public const int MaxPassiveWorkers = 3;

        /// <summary>Chance auf +1 Skill je aktivem Durchlauf.</summary>
        public const double SkillUpChance = 0.01;

        /// <summary>Apprentice wird beim Erlernen automatisch vergeben.</summary>
        public static readonly IReadOnlyList<ProfessionRank> Ranks =
        [
            new("Apprentice",  75, 5),
            new("Journeyman", 150, 15, RequiredSkill: 50),
            new("Expert",     225, 25, RequiredSkill: 125),
            new("Artisan",    300, 35, RequiredSkill: 200),
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
