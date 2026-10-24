using Models;

// Neue Systeme als eigene Partial-Datei anlegen; Dateien möglichst < 300 Zeilen halten.
public partial class IdleGameEngine
{
    // ─── Spielstand-Wiederherstellung ───

    internal void RestoreFromSave(SaveData save)
    {
        bool wasRunning = IsGameRunning;
        Stop();

        lock (_simLock)
        {
            _sharedInventory = new Inventory();

            _roster.Clear();
            TotalKills           = save.TotalKills;
            TotalDeaths          = save.TotalDeaths;
            TotalRareKills       = save.TotalRareKills;
            TotalItemsSold       = save.TotalItemsSold;
            TotalUncommonLooted  = save.TotalUncommonLooted;
            TotalRareItemsLooted = save.TotalRareItemsLooted;
            SpeedMultiplier = 1.0;
            _nextSlotId     = save.NextSlotId;

            foreach (var slotData in save.Roster)
                _roster.Add(RestoreSlot(slotData));

            // Gemeinsames Inventar befüllen – Rucksack
            foreach (var itemData in save.SharedInventoryItems ?? [])
            {
                Item? item = WeaponRegistry.GetById(itemData.ItemId) ?? (Item?)RareLootRegistry.GetById(itemData.ItemId);
                if (item is null) continue;
                if (itemData.RequiredLevel > 0)
                {
                    if (item is ArmorItem ai) item = ai with { RequiredLevel = itemData.RequiredLevel };
                    else if (item is WeaponItem wi) item = wi with { RequiredLevel = itemData.RequiredLevel };
                }
                if (itemData.Slot < _sharedInventory.Backpack.SlotCount)
                    _sharedInventory.Backpack.Put(itemData.Slot, new ItemStack(item, Math.Max(1, itemData.Count)));
            }

            _activeSlotId = save.ActiveSlotId;

            var activeSlot = _activeSlotId.HasValue
                ? _roster.FirstOrDefault(s => s.Id == _activeSlotId.Value)
                : null;

            // Virtuelle Taschen auf Basis des aktiven Charakters erzeugen, dann Items einfüllen
            if (activeSlot is not null)
                _sharedInventory.UpdateBagsForLevel(activeSlot.Player.Level);

            foreach (var itemData in save.SharedBagItems ?? [])
            {
                if (itemData.BagIndex < 0 || itemData.BagIndex >= Inventory.BagSlotCount) continue;
                Item? item = WeaponRegistry.GetById(itemData.ItemId) ?? (Item?)RareLootRegistry.GetById(itemData.ItemId);
                if (item is null) continue;
                if (itemData.RequiredLevel > 0)
                {
                    if (item is ArmorItem ai) item = ai with { RequiredLevel = itemData.RequiredLevel };
                    else if (item is WeaponItem wi) item = wi with { RequiredLevel = itemData.RequiredLevel };
                }
                var targetBag = _sharedInventory.Bags[itemData.BagIndex];
                if (targetBag is not null && itemData.Slot < targetBag.SlotCount)
                    targetBag.Put(itemData.Slot, new ItemStack(item, Math.Max(1, itemData.Count)));
                else
                    _sharedInventory.TryAdd(item);  // Fallback: Rucksack
            }

            if (activeSlot is not null)
            {
                Faction     = activeSlot.Faction;
                Race        = activeSlot.Race;
                Gender      = activeSlot.Gender;
                PlayerClass = activeSlot.Class;
                Player      = activeSlot.Player;
                CurrentEnemy = _spawner.Spawn(Player.Stats.Level, 0);
            }
        }

        if (wasRunning) Start();
        RosterChanged?.Invoke();
        TickUpdated?.Invoke();
    }

    private CharacterSlot RestoreSlot(SlotSaveData data)
    {
        var stats  = StatsFactory.GetBaseStats(data.Class, data.Race);
        var player = new Player(stats, data.Faction, data.Race, data.Class);
        player.Inventory = _sharedInventory;

        var p = data.Player;

        player.Copper = p.Copper;

        // ClassProgress wiederherstellen
        foreach (var (clsName, progressData) in p.ClassProgress)
        {
            if (Enum.TryParse<CharacterClass>(clsName, out var cls)
                && player.ClassProgress.TryGetValue(cls, out var progress))
            {
                progress.Restore(progressData.Level, progressData.Xp);
            }
        }

        // Ausgerüstete Waffen pro Klasse wiederherstellen
        var weaponsByClass = new Dictionary<CharacterClass, WeaponItem?>();
        foreach (var (clsName, weaponId) in p.EquippedWeaponIdByClass)
        {
            if (Enum.TryParse<CharacterClass>(clsName, out var cls))
                weaponsByClass[cls] = weaponId.HasValue ? WeaponRegistry.GetById(weaponId.Value) : null;
        }
        player.RestoreEquippedWeapons(weaponsByClass);

        // Getragene Rüstung wiederherstellen
        var armorBySlot = new Dictionary<ArmorSlot, ArmorItem>();
        foreach (var (slotName, itemId) in p.EquippedArmorIdBySlot)
        {
            if (Enum.TryParse<ArmorSlot>(slotName, out var slot)
                && RareLootRegistry.GetById(itemId) is { } armor)
            {
                armorBySlot[slot] = armor;
            }
        }
        if (armorBySlot.Count > 0)
            player.RestoreEquippedArmor(armorBySlot);

        // Talente wiederherstellen (vor SetActiveClass, damit CritBonus in Stats einfließt)
        foreach (var (talentId, ranks) in p.TalentInvestedPoints)
            player.TalentState.RestoreRanks(talentId, ranks);

        // Aktive Klasse setzen (berechnet Stats neu, stellt Waffe wieder her)
        player.SetClass(p.ActiveClass);
        player.SetActiveClass(p.ActiveClass);

        // Loadout der aktiven Klasse in den Charakter laden
        if (p.LoadoutAbilityIdsByClass.TryGetValue(p.ActiveClass.ToString(), out var activeIds))
            player.ApplyLoadout(new ClassLoadout { Class = p.ActiveClass, EquippedAbilityIds = activeIds.ToList() });

        return new CharacterSlot
        {
            Id      = data.Id,
            Class   = data.Class,
            Faction = data.Faction,
            Race    = data.Race,
            Gender  = data.Gender,
            Player  = player,
        };
    }
}
