// Models/SaveData.cs
namespace Models;

public class SaveData
{
    public int Version { get; set; } = 3;
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
    // Gemeinsames klassenübergreifendes Inventar – Rucksack (ab Version 2)
    public List<ItemStackSaveData> SharedInventoryItems { get; set; } = new();
    // Taschenplätze 0-3 (ab Version 3)
    public List<BagItemSaveData> SharedBagItems { get; set; } = new();
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
    // 0 = Standardwert aus Registry verwenden; >0 überschreibt RequiredLevel (z. B. für Drops)
    public int RequiredLevel { get; set; }
}

public class AchievementSaveData
{
    public string Id { get; set; } = "";
    public DateTime EarnedAt { get; set; }
}

/// <summary>Ein Item-Stack in einem der 4 Taschenplätze (BagIndex 0–3).</summary>
public class BagItemSaveData : ItemStackSaveData
{
    public int BagIndex { get; set; }
}
