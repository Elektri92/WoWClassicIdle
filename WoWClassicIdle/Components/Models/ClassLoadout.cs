// Models/ClassLoadout.cs
namespace Models;

/// <summary>
/// Defines which abilities are equipped in the slots of a class.
/// </summary>
public class ClassLoadout
{
    public CharacterClass Class { get; init; }

    /// <summary>
    /// Ordered list of ability IDs (max. <see cref="ClassAbilityRegistry.MaxSlotsPerClass"/> entries).
    /// Empty slots are not represented by <see langword="null"/> entries –
    /// the list only contains the IDs actually equipped.
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
