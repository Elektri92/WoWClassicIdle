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
            ActiveProfessionRun = null;
            _professionLog.Clear();
            _sharedProfessions.Clear();
            _passiveAssignments.Clear();
            _lastProfessionTargets.Clear();
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

            RestoreProfessions(save);

            // Gemeinsames Inventar befüllen – Rucksack
            foreach (var itemData in save.SharedInventoryItems ?? [])
            {
                Item? item = ResolveSavedItem(itemData.ItemId);
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
                Item? item = ResolveSavedItem(itemData.ItemId);
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

    /// <summary>Accountweite Berufe und passive Slots wiederherstellen (nach dem Roster).</summary>
    private void RestoreProfessions(SaveData save)
    {
        foreach (var profData in save.SecondaryProfessions ?? [])
        {
            if (Enum.TryParse<Profession>(profData.Profession, out var profession) && !Professions.Get(profession).IsPrimary)
                RestoreSharedProfession(profession, profData.Skill, profData.RankIndex);
        }

        foreach (var data in save.PassiveAssignments ?? [])
        {
            if (!Enum.TryParse<Profession>(data.Profession, out var profession)) continue;
            if (_passiveAssignments.Any(a => a.CharacterId == data.CharacterId || a.Matches(profession, data.Group, data.Index))) continue;
            if (!HasPassiveCapacity(profession, data.CharacterId)) continue; // Altstand mit mehr Slots je Beruf als erlaubt
            _passiveAssignments.Add(new PassiveAssignment
            {
                Profession  = profession,
                Group       = data.Group,
                Index       = data.Index,
                CharacterId = data.CharacterId,
                Target      = data.Target,
                Elapsed     = data.Elapsed,
            });
        }

        foreach (var (name, target) in save.LastProfessionTargets ?? [])
        {
            if (Enum.TryParse<Profession>(name, out var profession))
                _lastProfessionTargets[profession] = target;
        }

        UnlockSharedProfessions();
    }

    /// <summary>Übernimmt einen accountweiten Beruf – bei mehrfachen Altständen gewinnt der höchste Skill.</summary>
    private void RestoreSharedProfession(Profession profession, int skill, int rankIndex)
    {
        var current = _sharedProfessions.GetValueOrDefault(profession);
        if (current is not null && current.Skill >= skill && current.RankIndex >= rankIndex) return;
        var progress = new ProfessionProgress(profession);
        progress.Restore(Math.Max(skill, current?.Skill ?? 1), Math.Max(rankIndex, current?.RankIndex ?? 0));
        _sharedProfessions[profession] = progress;
    }

    private static Item? ResolveSavedItem(int itemId) =>
        WeaponRegistry.GetById(itemId)
        ?? (Item?)RareLootRegistry.GetById(itemId)
        ?? (Item?)CraftedGearRegistry.GetById(itemId)
        ?? (Item?)EnchantingRegistry.GetById(itemId)
        ?? ProfessionRegistry.GetById(itemId);

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
                && (RareLootRegistry.GetById(itemId) ?? CraftedGearRegistry.GetById(itemId)
                    ?? EnchantingRegistry.GetById(itemId)) is { } armor)
            {
                armorBySlot[slot] = armor;
            }
        }
        if (armorBySlot.Count > 0)
            player.RestoreEquippedArmor(armorBySlot);

        // Talente wiederherstellen (vor SetActiveClass, damit CritBonus in Stats einfließt)
        foreach (var (talentId, ranks) in p.TalentInvestedPoints)
            player.TalentState.RestoreRanks(talentId, ranks);

        // Berufe wiederherstellen
        foreach (var profData in p.Professions ?? [])
        {
            if (!Enum.TryParse<Profession>(profData.Profession, out var profession)) continue;
            if (Professions.Get(profession).IsPrimary)
                player.ProfessionState.Restore(profession, profData.Skill, profData.RankIndex);
            else
                RestoreSharedProfession(profession, profData.Skill, profData.RankIndex); // Altstand migrieren
        }

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
