// Services/SaveGameService.cs
using System.Text.Json;
using Models;

namespace Services;

public class SaveGameService : ISaveGameService, IDisposable
{
    private readonly IdleGameEngine _engine;
    private readonly IAchievementService _achievements;
    private readonly IAbilityLoadoutService _loadout;

    private readonly object _saveFileLock = new();
    private int _tickCount;
    private const int AutoSaveEveryTicks = 600; // ~1 minute at 100 ms interval

    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public bool SaveExists => File.Exists(DefaultSavePath);

    public string? LastRoute { get; set; }

    private static string DefaultSavePath
    {
        get
        {
            var dir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "WoWClassicIdle");
            Directory.CreateDirectory(dir);
#if DEBUG
            return Path.Combine(dir, "save_debug.json");
#else
            return Path.Combine(dir, "save.json");
#endif
        }
    }

    public SaveGameService(IdleGameEngine engine, IAchievementService achievements, IAbilityLoadoutService loadout)
    {
        _engine = engine;
        _achievements = achievements;
        _loadout = loadout;
        _engine.TickUpdated += OnTick;
    }

    private void OnTick()
    {
        if (++_tickCount >= AutoSaveEveryTicks)
        {
            _tickCount = 0;
            Save();
        }
    }

    public void Save(string? path = null)
    {
        path ??= DefaultSavePath;
        try
        {
            var saveData = BuildSaveData();
            var json = JsonSerializer.Serialize(saveData, JsonOptions);

            // Autosave (timer thread) and the updater can save concurrently, and the updater exits the process
            // right after – write to a temp file and swap it in so save.json is never left truncated
            lock (_saveFileLock)
            {
                var tempPath = path + ".tmp";
                File.WriteAllText(tempPath, json);
                File.Move(tempPath, path, overwrite: true);
            }
        }
        catch { /* Speicherfehler still ignorieren */ }
    }

    public bool TryLoad(string? path = null)
    {
        path ??= DefaultSavePath;
        if (!File.Exists(path)) return false;

        try
        {
            var json = File.ReadAllText(path);
            var save = JsonSerializer.Deserialize<SaveData>(json, JsonOptions);
            if (save is null) return false;

            _engine.RestoreFromSave(save);

            // Write the loadouts of all classes into the service (for the UI)
            foreach (var slotData in save.Roster)
            {
                foreach (var (clsName, abilityIds) in slotData.Player.LoadoutAbilityIdsByClass)
                {
                    if (Enum.TryParse<CharacterClass>(clsName, out var cls))
                        _loadout.SaveLoadout(new ClassLoadout { Class = cls, EquippedAbilityIds = abilityIds.ToList() });
                }
            }

            if (_achievements is AchievementService achievementService)
                achievementService.RestoreFromSave(save.Achievements, _engine);

            LastRoute = IsLocalRoute(save.LastRoute) ? save.LastRoute : null;

            return true;
        }
        catch
        {
            return false;
        }
    }

    // The save file is user-editable and the route ends up in the window's URL – it must stay on our server
    private static bool IsLocalRoute(string? route) =>
        route is not null && route.StartsWith('/') && !route.StartsWith("//") && !route.Contains('\\');

    public bool TryAutoLoad() => TryLoad();

    /// <summary>Deletes the save file and resets engine, loadouts and achievements to an empty state.</summary>
    public void DeleteSave()
    {
        // Clear the in-memory state first, otherwise autosave/exit writes the old state back
        _tickCount = 0;
        var empty = new SaveData { SeenFeatureIntros = new() };
        _engine.RestoreFromSave(empty);

        foreach (var cls in Enum.GetValues<CharacterClass>())
            _loadout.ClearLoadout(cls);

        if (_achievements is AchievementService achievementService)
            achievementService.RestoreFromSave(empty.Achievements, _engine);

        try
        {
            if (File.Exists(DefaultSavePath))
                File.Delete(DefaultSavePath);
        }
        catch { /* Silently ignore delete errors */ }
    }

    private SaveData BuildSaveData()
    {
        var save = new SaveData
        {
            ActiveSlotId = _engine.CombatSlotId,
            NextSlotId   = _engine.NextSlotId,
            TotalKills           = _engine.TotalKills,
            TotalDeaths          = _engine.TotalDeaths,
            TotalRareKills       = _engine.TotalRareKills,
            TotalItemsSold       = _engine.TotalItemsSold,
            TotalUncommonLooted  = _engine.TotalUncommonLooted,
            TotalRareItemsLooted = _engine.TotalRareItemsLooted,
            LastRoute            = LastRoute,
        };

        foreach (var slot in _engine.CharacterRoster)
            save.Roster.Add(BuildSlotSaveData(slot));

        // Save the shared inventory once – backpack
        var backpack = _engine.SharedInventory.Backpack;
        for (int i = 0; i < backpack.SlotCount; i++)
        {
            var stack = backpack[i];
            if (stack is null) continue;
            save.SharedInventoryItems.Add(new ItemStackSaveData
            {
                ItemId        = stack.Item.Id,
                Count         = stack.Count,
                Slot          = i,
                RequiredLevel = ItemRequirements.LevelOf(stack.Item),
            });
        }

        // Bag slots 0-3
        var bags = _engine.SharedInventory.Bags;
        for (int b = 0; b < bags.Count; b++)
        {
            var bag = bags[b];
            if (bag is null) continue;
            for (int i = 0; i < bag.SlotCount; i++)
            {
                var stack = bag[i];
                if (stack is null) continue;
                save.SharedBagItems.Add(new BagItemSaveData
                {
                    BagIndex      = b,
                    ItemId        = stack.Item.Id,
                    Count         = stack.Count,
                    Slot          = i,
                    RequiredLevel = ItemRequirements.LevelOf(stack.Item),
                });
            }
        }

        // Accountweite Berufe (Sammeln + Angeln) + passive Slots
        foreach (var progress in _engine.SharedProfessions)
            save.SecondaryProfessions.Add(BuildProfessionSaveData(progress));
        foreach (var a in _engine.PassiveAssignments)
        {
            save.PassiveAssignments.Add(new PassiveAssignmentSaveData
            {
                Profession  = a.Profession.ToString(),
                Group       = a.Group,
                Index       = a.Index,
                CharacterId = a.CharacterId,
                Target      = a.Target,
                Elapsed     = a.Elapsed,
            });
        }

        foreach (var (profession, target) in _engine.LastProfessionTargets)
            save.LastProfessionTargets[profession.ToString()] = target;

        save.CraftingQueue = _engine.CraftingQueue
            .Select(e => new CraftingQueueSaveData
            {
                RecipeId = e.Recipe?.Id ?? "",
                Remaining = e.Remaining,
                DisenchantItemId = e.IsDisenchant ? e.Item.Id : null,
                DisenchantRequiredLevel = e.IsDisenchant ? ItemRequirements.LevelOf(e.Item) : 0,
            })
            .ToList();

        save.ProfessionBonuses = _engine.BuildOwnedBonusSaveData();
        save.EquippedProfessionBonuses = _engine.BuildEquippedBonusSaveData();

        save.SeenFeatureIntros = _engine.SeenFeatureIntros.ToList();
        save.MerchantStock = _engine.BuildMerchantSaveData();

        foreach (var earned in _achievements.EarnedAchievements)
            save.Achievements.Add(new AchievementSaveData { Id = earned.AchievementId, EarnedAt = earned.EarnedAt });

        return save;
    }

    private PlayerSaveData BuildPlayerSaveData(Player p)
    {
        var data = new PlayerSaveData
        {
            ActiveClass = p.ActiveClass,
            Copper      = p.Copper,
            CurrentHp   = p.CurrentHp,
            CurrentMana = p.CurrentMana,
            DisabledAutoUseConsumables = p.DisabledAutoUse.ToList(),
        };

        foreach (var (cls, progress) in p.ClassProgress)
        {
            data.ClassProgress[cls.ToString()] = new ClassProgressSaveData
            {
                Level = progress.Level,
                Xp    = progress.Xp,
            };
        }

        foreach (var (cls, weapon) in p.EquippedWeaponsByClass)
            data.EquippedWeaponIdByClass[cls.ToString()] = weapon?.Id;

        // Worn armor (slots belong to the character, not the class)
        foreach (var (slot, item) in p.EquippedArmor)
            data.EquippedArmorIdBySlot[slot.ToString()] = item.Id;

        // Loadout: active class from Character._loadout, others from AbilityLoadoutService
        foreach (var cls in p.ClassProgress.Keys)
        {
            var loadout = cls == p.ActiveClass
                ? p.GetCurrentLoadout()
                : _loadout.GetLoadout(cls, p.ClassProgress[cls].Level);
            data.LoadoutAbilityIdsByClass[cls.ToString()] = loadout.EquippedAbilityIds.ToList();
        }

        // Talente
        foreach (var (talentId, ranks) in p.TalentState.Invested)
            data.TalentInvestedPoints[talentId] = ranks;

        // Hauptberuf
        if (p.ProfessionState.Primary is { } primary)
            data.Professions.Add(BuildProfessionSaveData(primary));

        return data;
    }

    private static ProfessionSaveData BuildProfessionSaveData(ProfessionProgress progress) => new()
    {
        Profession = progress.Profession.ToString(),
        Skill      = progress.Skill,
        RankIndex  = progress.RankIndex,
        KnownRecipes = progress.Definition.IsGathering ? null : progress.KnownRecipes.ToList(),
    };

    private SlotSaveData BuildSlotSaveData(CharacterSlot slot) => new()
    {
        Id      = slot.Id,
        Class   = slot.Class,
        Name    = slot.Name,
        Faction = slot.Faction,
        Race    = slot.Race,
        Gender  = slot.Gender,
        Player  = BuildPlayerSaveData(slot.Player),
        CombatZone    = slot.CombatZoneId,
        TravelTarget  = slot.Travel?.TargetZoneId,
        TravelElapsed = slot.Travel?.Elapsed ?? 0,
        DiscoveredZones = slot.DiscoveredZoneIds.Order().ToList(),
        ZoneObjectives  = slot.ZoneObjectiveProgress.ToDictionary(),
    };

    public void Dispose()
    {
        _engine.TickUpdated -= OnTick;
    }
}
