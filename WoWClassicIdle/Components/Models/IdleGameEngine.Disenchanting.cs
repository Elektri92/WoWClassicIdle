using Models;

/// <summary>Entzauberbares Item in einem Taschenplatz des geteilten Inventars.</summary>
public record DisenchantCandidate(Bag Bag, int Index, Item Item);

// Verzauberkunst: grüne Items aus dem Inventar zerstören → Staub/Essenz nach Item-Level (EnchantingRegistry).
// Läuft als aktiver Slot mit Warteschlange, 1 s Zauberzeit je Item.
public partial class IdleGameEngine
{
    /// <summary>Ziel-Kennung des aktiven Slots beim Entzaubern (kein Rezept).</summary>
    public const string DisenchantTarget = "disenchant";

    public const double DisenchantCastSeconds = 1.0;

    /// <summary>Alle grünen Rüstungsteile und Waffen in den Taschen (ausgerüstete Items zählen nicht).</summary>
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

    /// <summary>Grund, warum der aktive Charakter das Item nicht entzaubern kann (UI-Text) – <c>null</c> wenn erlaubt.</summary>
    public string? DisenchantBlocker(Item item)
    {
        if (Player.ProfessionState.Get(Profession.Enchanting) is not { } progress) return "You have not learned Enchanting";
        if (!EnchantingRegistry.CanDisenchant(item)) return "Only uncommon gear can be disenchanted";
        int required = EnchantingRegistry.BandFor(item).RequiredSkill;
        return progress.Skill < required ? $"Requires Enchanting ({required})" : null;
    }

    /// <summary>Laufende Entzauber-Warteschlange (leer, wenn nicht entzaubert wird).</summary>
    public IReadOnlyList<Item> DisenchantQueue
    {
        get { lock (_simLock) return ActiveProfessionRun?.DisenchantQueue?.ToList() ?? []; }
    }

    /// <summary>
    /// Reiht Items zum Entzaubern ein; läuft bereits eine Entzauber-Warteschlange, wird angehängt,
    /// sonst ersetzt sie einen laufenden Durchlauf (wie Herstellen).
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
    /// Entzaubert das nächste noch vorhandene Item der Warteschlange (verschwundene/gesperrte werden übersprungen).
    /// Nur unter <c>_simLock</c> aus dem Berufs-Tick.
    /// </summary>
    private void ExecuteNextDisenchant(ProfessionProgress progress, Queue<Item> queue)
    {
        while (queue.TryDequeue(out var item))
        {
            if (DisenchantBlocker(item) is not null || FindInBags(item) is not { } slot) continue;

            var (bag, index) = slot;
            var (material, count) = EnchantingRegistry.RollDisenchant(item, _professionRng);

            // Erst entfernen – der frei gewordene Platz reicht immer für das Material
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
