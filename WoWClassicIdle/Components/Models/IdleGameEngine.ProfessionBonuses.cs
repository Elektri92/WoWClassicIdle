using Models;

// Profession bonuses: account-wide collection (never in the inventory) and 3 slots per profession.
public partial class IdleGameEngine
{
    private readonly HashSet<ProfessionBonus> _ownedProfessionBonuses = new();
    private readonly Dictionary<Profession, ProfessionBonus?[]> _equippedProfessionBonuses = new();

    public bool OwnsProfessionBonus(ProfessionBonus bonus)
    {
        lock (_simLock) return _ownedProfessionBonuses.Contains(bonus);
    }

    public int OwnedProfessionBonusCount
    {
        get { lock (_simLock) return _ownedProfessionBonuses.Count; }
    }

    public IReadOnlyList<ProfessionBonus> OwnedProfessionBonuses
    {
        get { lock (_simLock) return _ownedProfessionBonuses.ToList(); }
    }

    /// <summary>Copy of the slots of a profession (empty slots = <c>null</c>).</summary>
    public ProfessionBonus?[] EquippedProfessionBonuses(Profession profession)
    {
        lock (_simLock) return (ProfessionBonus?[])BonusSlots(profession).Clone();
    }

    /// <summary>
    /// Puts an owned bonus into a slot. Only one version per kind may be equipped,
    /// so another rarity of the same kind leaves its slot.
    /// </summary>
    public void EquipProfessionBonus(int slot, ProfessionBonus bonus)
    {
        lock (_simLock)
        {
            if (slot is < 0 or >= ProfessionBonusRegistry.SlotCount || !_ownedProfessionBonuses.Contains(bonus)) return;
            var slots = BonusSlots(bonus.Profession);
            for (int i = 0; i < slots.Length; i++)
                if (slots[i]?.Kind == bonus.Kind) slots[i] = null;
            slots[slot] = bonus;
        }
        ProfessionUpdated?.Invoke();
    }

    public void UnequipProfessionBonus(Profession profession, int slot)
    {
        lock (_simLock)
        {
            if (slot is < 0 or >= ProfessionBonusRegistry.SlotCount) return;
            BonusSlots(profession)[slot] = null;
        }
        ProfessionUpdated?.Invoke();
    }

    /// <summary>Can an empty slot be filled or the bonus in the slot be replaced by a higher rarity?</summary>
    public bool CanImproveBonusSlot(Profession profession, int slot)
    {
        lock (_simLock)
        {
            var slots = BonusSlots(profession);
            if (slots[slot] is { } current)
                return BestOwnedBonus(profession, current.Kind)?.Quality > current.Quality;
            return ProfessionBonusRegistry.Kinds.Any(kind =>
                BestOwnedBonus(profession, kind) is not null && slots.All(b => b?.Kind != kind));
        }
    }

    public bool CanImproveBonuses(Profession profession) =>
        Enumerable.Range(0, ProfessionBonusRegistry.SlotCount).Any(slot => CanImproveBonusSlot(profession, slot));

    public bool CanImproveAnyBonuses => Professions.All.Any(def => CanImproveBonuses(def.Profession));

    /// <summary>Skill-up chance of a pass including all boni (UI).</summary>
    public double ProfessionSkillUpChance(ProfessionDefinition def) =>
        (def.IsGathering ? Professions.PassiveSkillUpChance : Professions.SkillUpChance + ProfessionSkillUpBonus)
        + ProfessionBonusValue(def.Profession, ProfessionBonusKind.SkillChance);

    /// <summary>Time factor of a pass: 30 % faster = 30 % less time per pass.</summary>
    public double ProfessionSpeedFactor(Profession profession) =>
        1 / (1 - ProfessionBonusValue(profession, ProfessionBonusKind.Speed));

    private double ProfessionBonusValue(Profession profession, ProfessionBonusKind kind)
    {
        lock (_simLock) return BonusSlots(profession).FirstOrDefault(b => b?.Kind == kind)?.Value ?? 0;
    }

    private bool RollDoubleYield(Profession profession) =>
        _professionRng.NextDouble() < ProfessionBonusValue(profession, ProfessionBonusKind.DoubleYield);

    private ProfessionBonus?[] BonusSlots(Profession profession)
    {
        if (!_equippedProfessionBonuses.TryGetValue(profession, out var slots))
            _equippedProfessionBonuses[profession] = slots = new ProfessionBonus?[ProfessionBonusRegistry.SlotCount];
        return slots;
    }

    private ProfessionBonus? BestOwnedBonus(Profession profession, ProfessionBonusKind kind) =>
        _ownedProfessionBonuses.Where(b => b.Profession == profession && b.Kind == kind).MaxBy(b => b.Quality);

    /// <summary>Kill in a combat zone: one missing bonus of this zone. Headless simulations have no zone slot and never drop any.</summary>
    private void RollProfessionBonusDrop()
    {
        if (CombatZoneRegistry.Get(ActiveCharacter?.CombatZoneId) is not { } zone) return;
        if (_professionRng.NextDouble() >= ProfessionBonusRegistry.DropChance(ProfessionBonusRegistry.QualityFor(zone))) return;

        var missing = ProfessionBonusRegistry.ForZone(zone)
            .Where(b => !_ownedProfessionBonuses.Contains(b))
            .ToList();
        if (missing.Count == 0) return;

        var bonus = missing[_professionRng.Next(missing.Count)];
        _ownedProfessionBonuses.Add(bonus);
        ItemDropped?.Invoke(bonus);
    }

    // ─── Save / Load ───

    internal List<string> BuildOwnedBonusSaveData()
    {
        lock (_simLock) return _ownedProfessionBonuses.Select(b => b.Key).ToList();
    }

    internal Dictionary<string, List<string?>> BuildEquippedBonusSaveData()
    {
        lock (_simLock)
            return _equippedProfessionBonuses.ToDictionary(e => e.Key.ToString(), e => e.Value.Select(b => b?.Key).ToList());
    }

    private void RestoreProfessionBonuses(SaveData save)
    {
        _ownedProfessionBonuses.Clear();
        _equippedProfessionBonuses.Clear();

        foreach (var key in save.ProfessionBonuses ?? [])
            if (ProfessionBonusRegistry.GetByKey(key) is { } bonus)
                _ownedProfessionBonuses.Add(bonus);

        foreach (var (name, keys) in save.EquippedProfessionBonuses ?? [])
        {
            if (!Enum.TryParse<Profession>(name, out var profession)) continue;
            var slots = BonusSlots(profession);
            for (int i = 0; i < keys.Count && i < slots.Length; i++)
                if (ProfessionBonusRegistry.GetByKey(keys[i]) is { } bonus
                    && bonus.Profession == profession && _ownedProfessionBonuses.Contains(bonus)
                    && slots.All(b => b?.Kind != bonus.Kind))
                    slots[i] = bonus;
        }
    }

    // ─── Debug (UI only reachable via BuildInfo.IsDebug) ───

    public void DebugGrantProfessionBonuses(ItemQuality quality)
    {
        lock (_simLock)
            foreach (var bonus in ProfessionBonusRegistry.All.Where(b => b.Quality == quality))
                _ownedProfessionBonuses.Add(bonus);
        ProfessionUpdated?.Invoke();
    }

    public void DebugDropProfessionBonus()
    {
        lock (_simLock)
        {
            var missing = ProfessionBonusRegistry.All.Where(b => !_ownedProfessionBonuses.Contains(b)).ToList();
            if (missing.Count == 0) return;
            var bonus = missing[_professionRng.Next(missing.Count)];
            _ownedProfessionBonuses.Add(bonus);
            ItemDropped?.Invoke(bonus);
        }
        ProfessionUpdated?.Invoke();
    }

    public void DebugClearProfessionBonuses()
    {
        lock (_simLock)
        {
            _ownedProfessionBonuses.Clear();
            _equippedProfessionBonuses.Clear();
        }
        ProfessionUpdated?.Invoke();
    }
}
