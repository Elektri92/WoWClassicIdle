using Models;

/// <summary>Disenchantable item in a bag slot of the shared inventory.</summary>
public record DisenchantCandidate(Bag Bag, int Index, Item Item);

// Enchanting: destroy green items from the inventory → dust/essence by item level (EnchantingRegistry).
// Runs as an active slot with a queue, 1 s cast time per item.
public partial class IdleGameEngine
{
    /// <summary>Target identifier of the active slot when disenchanting (no recipe).</summary>
    public const string DisenchantTarget = "disenchant";

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

    /// <summary>Reason why the active character cannot disenchant the item (UI text) – <c>null</c> if allowed.</summary>
    public string? DisenchantBlocker(Item item)
    {
        if (Player.ProfessionState.Get(Profession.Enchanting) is not { } progress) return "You have not learned Enchanting";
        if (!EnchantingRegistry.CanDisenchant(item)) return "Only uncommon gear can be disenchanted";
        int required = EnchantingRegistry.BandFor(item).RequiredSkill;
        return progress.Skill < required ? $"Requires Enchanting ({required})" : null;
    }

    /// <summary>Running disenchant queue (empty if nothing is being disenchanted).</summary>
    public IReadOnlyList<Item> DisenchantQueue
    {
        get { lock (_simLock) return ActiveProfessionRun?.DisenchantQueue?.ToList() ?? []; }
    }

    /// <summary>
    /// Queues items for disenchanting; if a disenchant queue is already running, they are appended,
    /// otherwise it replaces a running pass (like crafting).
    /// </summary>
    public void QueueDisenchant(IEnumerable<Item> items)
    {
        lock (_simLock)
        {
            if (Player.ProfessionState.Get(Profession.Enchanting) is null) return;
            var valid = items.Where(i => DisenchantBlocker(i) is null).ToList();
            if (valid.Count == 0) return;

            if (ActiveProfessionRun is { DisenchantQueue: { } queue } run && run.Player == Player)
            {
                foreach (var item in valid) queue.Enqueue(item);
                run.Remaining = queue.Count;
            }
            else
            {
                ActiveProfessionRun = new ActiveProfessionRun
                {
                    Player = Player, Profession = Profession.Enchanting, Target = DisenchantTarget,
                    Duration = DisenchantCastSeconds, DisenchantQueue = new Queue<Item>(valid), Remaining = valid.Count,
                };
            }
        }
        ProfessionUpdated?.Invoke();
    }

    /// <summary>
    /// Disenchants the next still existing item of the queue (vanished/locked ones are skipped).
    /// Only under <c>_simLock</c> from the profession tick.
    /// </summary>
    private void ExecuteNextDisenchant(ProfessionProgress progress, Queue<Item> queue)
    {
        while (queue.TryDequeue(out var item))
        {
            if (DisenchantBlocker(item) is not null || FindInBags(item) is not { } slot) continue;

            var (bag, index) = slot;
            var (material, count) = EnchantingRegistry.RollDisenchant(item, _professionRng);

            // Remove first – the freed slot is always enough for the material
            bag.Take(index);
            int leftover = _sharedInventory.TryAdd(material, count);
            if (leftover == count)
            {
                bag.Put(index, new ItemStack(item));
                LogProfession(Profession.Enchanting, "Inventory is full", ProfessionLogKind.Error);
                queue.Clear();
                return;
            }

            LogProfession(Profession.Enchanting, material.Name, ProfessionLogKind.Loot, material, count - leftover);
            RollSkillUp(progress, EnchantingRegistry.BandFor(item).RequiredSkill);
            return;
        }
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
