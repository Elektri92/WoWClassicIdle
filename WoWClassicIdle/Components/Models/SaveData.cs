// Models/SaveData.cs
namespace Models;

public class SaveData
{
    public int Version { get; set; } = 1;
    public int? ActiveSlotId { get; set; }
    public int NextSlotId { get; set; }
    public int TotalKills { get; set; }
    public int TotalDeaths { get; set; }
    public List<SlotSaveData> Roster { get; set; } = new();
    public List<AchievementSaveData> Achievements { get; set; } = new();
}

public class SlotSaveData
{
    public int Id { get; set; }
    public CharacterClass Class { get; set; }
    public Faction Faction { get; set; }
    public Race Race { get; set; }
    public Gender Gender { get; set; }
    public PlayerSaveData Player { get; set; } = new();
}

public class PlayerSaveData
{
    public CharacterClass ActiveClass { get; set; }
    public long Copper { get; set; }
    // Schlüssel sind CharacterClass.ToString() für JSON-Kompatibilität
    public Dictionary<string, ClassProgressSaveData> ClassProgress { get; set; } = new();
    public Dictionary<string, int?> EquippedWeaponIdByClass { get; set; } = new();
    // ArmorSlot.ToString() → ItemId der getragenen Rüstung
    public Dictionary<string, int> EquippedArmorIdBySlot { get; set; } = new();
    public List<ItemStackSaveData> BackpackItems { get; set; } = new();
    public Dictionary<string, List<string>> LoadoutAbilityIdsByClass { get; set; } = new();
    // TalentId → investierte Ränge
    public Dictionary<string, int> TalentInvestedPoints { get; set; } = new();
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
}

public class AchievementSaveData
{
    public string Id { get; set; } = "";
    public DateTime EarnedAt { get; set; }
}
