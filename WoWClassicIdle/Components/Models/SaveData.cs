// Models/SaveData.cs
namespace Models;

public class SaveData
{
    public int Version { get; set; } = 4;
    public int? ActiveSlotId { get; set; }
    public int NextSlotId { get; set; }
    public int TotalKills { get; set; }
    public int TotalDeaths { get; set; }
    public int TotalRareKills { get; set; }
    public int TotalItemsSold { get; set; }
    public int TotalUncommonLooted { get; set; }
    public int TotalRareItemsLooted { get; set; }
    public List<SlotSaveData> Roster { get; set; } = new();
    public List<AchievementSaveData> Achievements { get; set; } = new();
    // Shared cross-class inventory – backpack (from version 2)
    public List<ItemStackSaveData> SharedInventoryItems { get; set; } = new();
    // Bag slots 0-3 (from version 3)
    public List<BagItemSaveData> SharedBagItems { get; set; } = new();
    // Account-wide professions (gathering + fishing) and passive profession slots (from version 4; name kept for compatibility)
    public List<ProfessionSaveData> SecondaryProfessions { get; set; } = new();
    public List<PassiveAssignmentSaveData> PassiveAssignments { get; set; } = new();
    // Profession.ToString() → last chosen zone/recipe
    public Dictionary<string, string> LastProfessionTargets { get; set; } = new();
    // Feature intros already shown; null = legacy save without this field
    public List<string>? SeenFeatureIntros { get; set; }
    // Merchant stock with restock time (real time); null = legacy save, stock full
    public List<MerchantStockSaveData>? MerchantStock { get; set; }
    // Page shown when the game was last saved; null = legacy save → start page
    public string? LastRoute { get; set; }
}

public class MerchantStockSaveData
{
    public int ItemId { get; set; }
    public int Count { get; set; }
    public DateTime? RestockAtUtc { get; set; }
}

public class SlotSaveData
{
    public int Id { get; set; }
    public CharacterClass Class { get; set; }
    // Missing in legacy saves → empty, display falls back to race + class
    public string? Name { get; set; }
    public Faction Faction { get; set; }
    public Race Race { get; set; }
    public Gender Gender { get; set; }
    public PlayerSaveData Player { get; set; } = new();
    // Combat zone (null = not chosen yet) and ongoing travel
    public string? CombatZone { get; set; }
    public string? TravelTarget { get; set; }
    public double TravelElapsed { get; set; }
    // Discovered zones (null = legacy save → only the current zone) and progress of the zone objectives
    public List<string>? DiscoveredZones { get; set; }
    public Dictionary<string, int>? ZoneObjectives { get; set; }
}

public class PlayerSaveData
{
    public CharacterClass ActiveClass { get; set; }
    public long Copper { get; set; }
    // Current health and resource (mana/rage/energy); null = legacy save → full as after a fresh start
    public int? CurrentHp { get; set; }
    public int? CurrentMana { get; set; }
    // Keys are CharacterClass.ToString() for JSON compatibility
    public Dictionary<string, ClassProgressSaveData> ClassProgress { get; set; } = new();
    public Dictionary<string, int?> EquippedWeaponIdByClass { get; set; } = new();
    // ArmorSlot.ToString() → ItemId of the worn armor
    public Dictionary<string, int> EquippedArmorIdBySlot { get; set; } = new();
    public List<ItemStackSaveData> BackpackItems { get; set; } = new();
    public Dictionary<string, List<string>> LoadoutAbilityIdsByClass { get; set; } = new();
    // TalentId → invested ranks
    public Dictionary<string, int> TalentInvestedPoints { get; set; } = new();
    // BuffKeys of consumable kinds with auto-use off (missing in legacy saves → all on)
    public List<string> DisabledAutoUseConsumables { get; set; } = new();
    // Main profession (from version 4; older saves also contain gathering/secondary professions here → taken over account-wide)
    public List<ProfessionSaveData> Professions { get; set; } = new();
}

public class ProfessionSaveData
{
    // Profession.ToString()
    public string Profession { get; set; } = "";
    public int Skill { get; set; } = 1;
    public int RankIndex { get; set; }
    // Recipe IDs learned from the trainer; null = legacy save (all recipes up to the skill count as learned)
    public List<string>? KnownRecipes { get; set; }
}

/// <summary>A passive profession slot filled with a character.</summary>
public class PassiveAssignmentSaveData
{
    public string Profession { get; set; } = "";
    public string Group { get; set; } = "";
    public int Index { get; set; }
    public int CharacterId { get; set; }
    public string? Target { get; set; }
    public double Elapsed { get; set; }
}

public class ClassProgressSaveData
{
    public int Level { get; set; } = 1;
    public int Xp { get; set; }
}

public class ItemStackSaveData
{
    public int ItemId { get; set; }
    public int Count { get; set; }
    public int Slot { get; set; }
    // 0 = use the default value from the registry; >0 overrides RequiredLevel (e.g. for drops)
    public int RequiredLevel { get; set; }
}

public class AchievementSaveData
{
    public string Id { get; set; } = "";
    public DateTime EarnedAt { get; set; }
}

/// <summary>An item stack in one of the 4 bag slots (BagIndex 0–3).</summary>
public class BagItemSaveData : ItemStackSaveData
{
    public int BagIndex { get; set; }
}
