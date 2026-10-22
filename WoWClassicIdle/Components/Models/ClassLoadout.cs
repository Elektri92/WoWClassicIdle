// Models/ClassLoadout.cs
namespace Models;

/// <summary>
/// Definiert, welche Abilities in den Slots einer Klasse ausgerüstet sind.
/// </summary>
public class ClassLoadout
{
    public CharacterClass Class { get; init; }

    /// <summary>
    /// Geordnete Liste von Ability-Ids (max. <see cref="ClassAbilityRegistry.MaxSlotsPerClass"/> Einträge).
    /// Leere Slots werden durch <see langword="null"/>-Einträge nicht representiert –
    /// die Liste enthält nur die tatsächlich ausgerüsteten Ids.
    /// </summary>
    public List<string> EquippedAbilityIds { get; set; } = new();

    public int Count => EquippedAbilityIds.Count;

    public bool IsSlotFree(int slotIndex) => slotIndex >= EquippedAbilityIds.Count;

    public bool IsEquipped(string abilityId) => EquippedAbilityIds.Contains(abilityId);

    public void SetSlot(int slotIndex, string abilityId)
    {
        if (slotIndex < EquippedAbilityIds.Count)
            EquippedAbilityIds[slotIndex] = abilityId;
        else if (slotIndex == EquippedAbilityIds.Count)
            EquippedAbilityIds.Add(abilityId);
    }

    public void RemoveSlot(int slotIndex)
    {
        if (slotIndex < EquippedAbilityIds.Count)
            EquippedAbilityIds.RemoveAt(slotIndex);
    }

    public void Clear() => EquippedAbilityIds.Clear();
}
