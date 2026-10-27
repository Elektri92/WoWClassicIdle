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
    // Gemeinsames klassenübergreifendes Inventar – Rucksack (ab Version 2)
    public List<ItemStackSaveData> SharedInventoryItems { get; set; } = new();
    // Taschenplätze 0-3 (ab Version 3)
    public List<BagItemSaveData> SharedBagItems { get; set; } = new();
    // Accountweite Berufe (Sammeln + Angeln) und passive Berufs-Slots (ab Version 4; Name aus Kompatibilitätsgründen)
    public List<ProfessionSaveData> SecondaryProfessions { get; set; } = new();
    public List<PassiveAssignmentSaveData> PassiveAssignments { get; set; } = new();
    // Profession.ToString() → zuletzt gewählte Zone/Rezept
    public Dictionary<string, string> LastProfessionTargets { get; set; } = new();
    // Bereits gezeigte Feature-Intros; null = Altstand ohne dieses Feld
    public List<string>? SeenFeatureIntros { get; set; }
}

public class SlotSaveData
{
    public int Id { get; set; }
    public CharacterClass Class { get; set; }
    public Faction Faction { get; set; }
    public Race Race { get; set; }
    public Gender Gender { get; set; }
    public PlayerSaveData Player { get; set; } = new();
    // Kampfgebiet (null = noch nicht gewählt) und laufende Reise
    public string? CombatZone { get; set; }
    public string? TravelTarget { get; set; }
    public double TravelElapsed { get; set; }
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
    // Hauptberuf (ab Version 4; ältere Stände enthalten hier auch Sammel-/Sekundärberufe → werden accountweit übernommen)
    public List<ProfessionSaveData> Professions { get; set; } = new();
}

public class ProfessionSaveData
{
    // Profession.ToString()
    public string Profession { get; set; } = "";
    public int Skill { get; set; } = 1;
    public int RankIndex { get; set; }
}

/// <summary>Ein mit einem Charakter belegter passiver Berufs-Slot.</summary>
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
