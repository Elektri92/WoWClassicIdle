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

    /// <summary>
    /// Auto-cast conditions per spell name. Replaced as a whole on every change,
    /// so the engine tick can read it without a lock while the UI edits a rule.
    /// </summary>
    public IReadOnlyDictionary<string, AbilityCastRule> CastRules { get; private set; } = NoCastRules;
    private static readonly IReadOnlyDictionary<string, AbilityCastRule> NoCastRules = new Dictionary<string, AbilityCastRule>();

    public AbilityCastRule? RuleFor(string spellName) => CastRules.GetValueOrDefault(spellName);

    public void SetCastRule(string spellName, AbilityCastRule? rule)
    {
        var rules = new Dictionary<string, AbilityCastRule>(CastRules);
        if (rule is null || rule.IsEmpty)
            rules.Remove(spellName);
        else
            rules[spellName] = rule;
        CastRules = rules;
    }

    public void SetCastRules(IReadOnlyDictionary<string, AbilityCastRule> rules) =>
        CastRules = new Dictionary<string, AbilityCastRule>(rules);

    public void Clear()
    {
        EquippedAbilityIds.Clear();
        CastRules = NoCastRules;
    }
}
