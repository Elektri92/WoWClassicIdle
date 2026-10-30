using Models;

/// <summary>Disenchantable item in a bag slot of the shared inventory.</summary>
public record DisenchantCandidate(Bag Bag, int Index, Item Item);

// Enchanting: destroy green items from the inventory → dust/essence by item level (EnchantingRegistry).
// Every item is its own position in the crafting queue, 1 s cast time per item.
public partial class IdleGameEngine
{
    public const double DisenchantCastSeconds = 1.0;

    /// <summary>All green armor pieces and weapons in the bags (equipped items don't count).</summary>
    public IReadOnlyList<DisenchantCandidate> DisenchantCandidates()
    {
        lock (_simLock)
        {
            var list = new List<DisenchantCandidate>();
            foreach (var bag in _sharedInventory.AllContainers)
                for (int i = 0; i < bag.SlotCount; i++)
                    if (bag[i] is { } stack && EnchantingRegistry.CanDisenchant(stack.Item))
                        list.Add(new DisenchantCandidate(bag, i, stack.Item));
            return list;
        }
    }

    /// <summary>Reason why the Enchanting owner cannot disenchant the item (UI text) – <c>null</c> if allowed.</summary>
    public string? DisenchantBlocker(Item item)
    {
        if (ProfessionOwner(Profession.Enchanting)?.Player.ProfessionState.Get(Profession.Enchanting) is not { } progress)
            return "You have not learned Enchanting";
        if (!EnchantingRegistry.CanDisenchant(item)) return "Only uncommon gear can be disenchanted";
        int required = EnchantingRegistry.BandFor(item).RequiredSkill;
        return progress.Skill < required ? $"Requires Enchanting ({required})" : null;
    }

    /// <summary>Items waiting in the crafting queue to be disenchanted.</summary>
    public IReadOnlyList<Item> DisenchantQueue
    {
        get { lock (_simLock) return _craftingQueue.Where(e => e.IsDisenchant).Select(e => e.Item).ToList(); }
    }

    /// <summary>Appends every disenchantable item as its own position to the crafting queue.</summary>
    public void QueueDisenchant(IEnumerable<Item> items)
    {
        lock (_simLock)
        {
            foreach (var item in items.Where(i => DisenchantBlocker(i) is null))
                _craftingQueue.Add(NewDisenchantEntry(item));
        }
        ProfessionUpdated?.Invoke();
    }

    /// <summary>The item must still be in the bags (it may have been sold or equipped since it was queued).</summary>
    private bool CanDisenchant(Item item) => DisenchantBlocker(item) is null && FindInBags(item) is not null;

    /// <summary>
    /// Disenchants the item (<see cref="CanDisenchant"/> checked); <c>false</c> = inventory full, the item stays.
    /// Only under <c>_simLock</c> from the profession tick.
    /// </summary>
    private bool ExecuteDisenchant(ProfessionProgress progress, Item item)
    {
        var (bag, index) = FindInBags(item)!.Value;
        var (material, count) = EnchantingRegistry.RollDisenchant(item, _professionRng);
        if (RollDoubleYield(Profession.Enchanting)) count *= 2;

        // Remove first – the freed slot is always enough for the material
        bag.Take(index);
        int leftover = _sharedInventory.TryAdd(material, count);
        if (leftover == count)
        {
            bag.Put(index, new ItemStack(item));
            LogProfession(Profession.Enchanting, "Inventory is full", ProfessionLogKind.Error);
            return false;
        }

        LogProfession(Profession.Enchanting, material.Name, ProfessionLogKind.Loot, material, count - leftover);
        RollSkillUp(progress, EnchantingRegistry.BandFor(item).RequiredSkill);
        return true;
    }

    private (Bag Bag, int Index)? FindInBags(Item item)
    {
        foreach (var bag in _sharedInventory.AllContainers)
            for (int i = 0; i < bag.SlotCount; i++)
                if (bag[i]?.Item == item)
                    return (bag, i);
        return null;
    }
}
