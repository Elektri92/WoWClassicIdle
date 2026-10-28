// Services/SaveGameService.cs
using System.Text.Json;
using Models;

namespace Services;

public class SaveGameService : ISaveGameService, IDisposable
{
    private readonly IdleGameEngine _engine;
    private readonly IAchievementService _achievements;
    private readonly IAbilityLoadoutService _loadout;

    private int _tickCount;
    private const int AutoSaveEveryTicks = 600; // ~1 Minute bei 100 ms Intervall

    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public bool SaveExists => File.Exists(DefaultSavePath);

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
            File.WriteAllText(path, json);
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

            // Loadouts für alle Klassen in den Service schreiben (für UI)
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

            return true;
        }
        catch
        {
            return false;
        }
    }

    public bool TryAutoLoad() => TryLoad();

    private SaveData BuildSaveData()
    {
        var save = new SaveData
        {
            ActiveSlotId = _engine.ActiveSlotId,
            NextSlotId   = _engine.NextSlotId,
            TotalKills           = _engine.TotalKills,
            TotalDeaths          = _engine.TotalDeaths,
            TotalRareKills       = _engine.TotalRareKills,
            TotalItemsSold       = _engine.TotalItemsSold,
            TotalUncommonLooted  = _engine.TotalUncommonLooted,
            TotalRareItemsLooted = _engine.TotalRareItemsLooted,
        };

        foreach (var slot in _engine.CharacterRoster)
            save.Roster.Add(BuildSlotSaveData(slot));

        // Gemeinsames Inventar einmal speichern – Rucksack
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

        // Taschenplätze 0-3
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

        // Getragene Rüstung (Slots gehören zum Charakter, nicht zur Klasse)
        foreach (var (slot, item) in p.EquippedArmor)
            data.EquippedArmorIdBySlot[slot.ToString()] = item.Id;

        // Loadout: aktive Klasse aus Character._loadout, andere aus AbilityLoadoutService
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
    };

    public void Dispose()
    {
        _engine.TickUpdated -= OnTick;
    }
}
