using Models;

namespace Services;

// Profession bonus achievements: collecting (count, rarity, profession, zone) and equipping – account-wide like the bonuses
public partial class AchievementService
{
    private static readonly int[] BonusFoundMilestones = { 1, 10, 25, 50 };

    private static int BonusCount => ProfessionBonusRegistry.All.Count;
    private static int BonusesPerQuality(ItemQuality quality) => ProfessionBonusRegistry.All.Count(b => b.Quality == quality);
    private static int BonusesPerProfession(Profession profession) => ProfessionBonusRegistry.ForProfession(profession).Count();
    private static int BonusSlotTotal => Professions.All.Count * ProfessionBonusRegistry.SlotCount;

    private ProfessionBonus?[][] BonusSlots() =>
        Professions.All.Select(def => _engine.EquippedProfessionBonuses(def.Profession)).ToArray();

    private static bool IsEpic(ProfessionBonus? bonus) => bonus?.Quality == ItemQuality.Epic;

    private void CheckProfessionBonuses()
    {
        var owned = _engine.OwnedProfessionBonuses.ToHashSet();
        foreach (int milestone in BonusFoundMilestones)
            if (owned.Count >= milestone) TryAward($"bonus_found_{milestone}");
        if (owned.Count >= BonusCount) TryAward("bonus_found_all");

        foreach (var quality in ProfessionBonusRegistry.Qualities)
        {
            int count = owned.Count(b => b.Quality == quality);
            if (count >= 1) TryAward($"bonus_first_{quality}");
            if (count >= BonusesPerQuality(quality)) TryAward($"bonus_all_{quality}");
        }

        foreach (var def in Professions.All)
            if (owned.Count(b => b.Profession == def.Profession) >= BonusesPerProfession(def.Profession))
                TryAward($"bonus_prof_{def.Profession}");

        if (CombatZoneRegistry.All.Any(z => ProfessionBonusRegistry.ForZone(z).All(owned.Contains)))
            TryAward("bonus_zone_complete");

        var slots = BonusSlots();
        if (slots.Any(s => s.Any(b => b is not null))) TryAward("bonus_equip_first");
        if (slots.Any(s => s.All(b => b is not null))) TryAward("bonus_equip_full");
        if (slots.Any(s => s.All(IsEpic))) TryAward("bonus_equip_epic");
        if (slots.All(s => s.All(b => b is not null))) TryAward("bonus_equip_all");
        if (slots.All(s => s.All(IsEpic))) TryAward("bonus_equip_all_epic");
    }

    private void AddProfessionBonusTrackers(Dictionary<string, (Func<long>, long)> map)
    {
        foreach (int m in BonusFoundMilestones)
            map[$"bonus_found_{m}"] = (() => _engine.OwnedProfessionBonusCount, m);
        map["bonus_found_all"] = (() => _engine.OwnedProfessionBonusCount, BonusCount);

        foreach (var quality in ProfessionBonusRegistry.Qualities)
            map[$"bonus_all_{quality}"] = (() => _engine.OwnedProfessionBonuses.Count(b => b.Quality == quality), BonusesPerQuality(quality));

        foreach (var def in Professions.All)
            map[$"bonus_prof_{def.Profession}"] = (() => _engine.OwnedProfessionBonuses.Count(b => b.Profession == def.Profession), BonusesPerProfession(def.Profession));

        map["bonus_equip_full"] = (() => BonusSlots().Max(s => s.Count(b => b is not null)), ProfessionBonusRegistry.SlotCount);
        map["bonus_equip_epic"] = (() => BonusSlots().Max(s => s.Count(IsEpic)), ProfessionBonusRegistry.SlotCount);
        map["bonus_equip_all"] = (() => BonusSlots().Sum(s => s.Count(b => b is not null)), BonusSlotTotal);
        map["bonus_equip_all_epic"] = (() => BonusSlots().Sum(s => s.Count(IsEpic)), BonusSlotTotal);
    }

    private static void AddProfessionBonusAchievements(List<AchievementDefinition> list)
    {
        static string Icon(string name) => $"https://wow.zamimg.com/images/wow/icons/large/{name}.jpg";
        var cat = AchievementCategory.Professions;

        // ── Found bonuses ──
        list.Add(new() { Id = "bonus_found_1",   Category = cat, Points = 10, Title = "A Useful Trick",      Description = "Find your first profession bonus.", IconUrl = Icon("inv_misc_bag_07") });
        list.Add(new() { Id = "bonus_found_10",  Category = cat, Points = 10, Title = "Tricks of the Trade", Description = "Find 10 profession bonuses.",       IconUrl = Icon("inv_misc_bag_08") });
        list.Add(new() { Id = "bonus_found_25",  Category = cat, Points = 15, Title = "Bag of Tricks",       Description = "Find 25 profession bonuses.",       IconUrl = Icon("inv_misc_bag_10") });
        list.Add(new() { Id = "bonus_found_50",  Category = cat, Points = 25, Title = "Traveling Craftsman", Description = "Find 50 profession bonuses.",       IconUrl = Icon("inv_misc_bag_17") });
        list.Add(new() { Id = "bonus_found_all", Category = cat, Points = 50, Title = "Master of All Trades", Description = $"Find all {BonusCount} profession bonuses.", IconUrl = Icon("inv_misc_bag_enchantedmageweave") });
        list.Add(new() { Id = "bonus_zone_complete", Category = cat, Points = 15, Title = "Nothing Left Here", Description = "Find every profession bonus of a single zone.", IconUrl = Icon("inv_misc_map_01") });

        // ── Rarities ──
        list.Add(new() { Id = "bonus_first_Uncommon", Category = cat, Points = 10, Title = "Apprentice's Secret", Description = "Find your first uncommon profession bonus.", IconUrl = Icon("inv_misc_note_01") });
        list.Add(new() { Id = "bonus_first_Rare",     Category = cat, Points = 15, Title = "Expert Advice",       Description = "Find your first rare profession bonus.",     IconUrl = Icon("inv_scroll_03") });
        list.Add(new() { Id = "bonus_first_Epic",     Category = cat, Points = 25, Title = "Master Class",        Description = "Find your first epic profession bonus.",     IconUrl = Icon("inv_scroll_05") });
        list.Add(new() { Id = "bonus_all_Uncommon", Category = cat, Points = 25, Title = "Apprentice's Collection", Description = $"Find all {BonusesPerQuality(ItemQuality.Uncommon)} uncommon profession bonuses.", IconUrl = Icon("inv_misc_book_01") });
        list.Add(new() { Id = "bonus_all_Rare",     Category = cat, Points = 35, Title = "Expert's Collection",     Description = $"Find all {BonusesPerQuality(ItemQuality.Rare)} rare profession bonuses.",         IconUrl = Icon("inv_misc_book_06") });
        list.Add(new() { Id = "bonus_all_Epic",     Category = cat, Points = 50, Title = "Master's Collection",     Description = $"Find all {BonusesPerQuality(ItemQuality.Epic)} epic profession bonuses.",         IconUrl = Icon("inv_misc_book_11") });

        // ── Complete set per profession ──
        foreach (var def in Professions.All)
            list.Add(new() { Id = $"bonus_prof_{def.Profession}", Category = cat, Points = 25, Title = $"{def.Name} Secrets", Description = $"Find all {BonusesPerProfession(def.Profession)} {def.Name} bonuses.", IconUrl = def.IconUrl });

        // ── Equipped bonuses ──
        list.Add(new() { Id = "bonus_equip_first",    Category = cat, Points = 5,  Title = "Tools of the Trade", Description = "Equip a profession bonus.",                                     IconUrl = Icon("inv_gizmo_01") });
        list.Add(new() { Id = "bonus_equip_full",     Category = cat, Points = 10, Title = "Fully Prepared",     Description = "Fill all bonus slots of a profession.",                         IconUrl = Icon("inv_hammer_20") });
        list.Add(new() { Id = "bonus_equip_epic",     Category = cat, Points = 25, Title = "Masterwork Bench",   Description = "Fill all bonus slots of a profession with epic bonuses.",       IconUrl = Icon("inv_ingot_03") });
        list.Add(new() { Id = "bonus_equip_all",      Category = cat, Points = 35, Title = "Workshop in Order",  Description = "Fill all bonus slots of every profession.",                     IconUrl = Icon("inv_misc_gear_01") });
        list.Add(new() { Id = "bonus_equip_all_epic", Category = cat, Points = 50, Title = "Pinnacle of Craft",  Description = "Fill all bonus slots of every profession with epic bonuses.",   IconUrl = Icon("inv_jewelry_talisman_07") });
    }
}
