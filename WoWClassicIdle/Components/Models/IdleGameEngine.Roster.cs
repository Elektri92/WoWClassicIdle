using Models;

// Neue Systeme als eigene Partial-Datei anlegen; Dateien möglichst < 300 Zeilen halten.
public partial class IdleGameEngine
{
    // ─── Charakter-Wechsel ───
    public void SetCharacter(Faction faction, Race race, CharacterClass characterClass, Gender gender)
    {
        if (!RaceClassInfo.IsRaceValidForFaction(race, faction))
            throw new ArgumentException($"Race '{race}' does not belong to faction '{faction}'.");

        if (!RaceClassInfo.IsClassAvailableForRace(race, characterClass))
            throw new ArgumentException($"Class '{characterClass}' is not available for race '{race}'.");

        lock (_simLock)
        {
            Faction = faction;
            Race = race;
            PlayerClass = characterClass;
            Gender = gender;

            // Bestehenden Player aktualisieren (Progress bleibt erhalten!)
            Player.ChangeCharacter(faction, race);
            Player.SetActiveClass(characterClass);
            Player.SetClass(characterClass);   // ← stellt sicher, dass Character.CurrentClass + Loadout aktualisiert werden
            Player.EquipStarterWeapon(characterClass);

            // Kampfstatistik und Swing-Timer gehören zum alten Charakter
            Meter.Reset();
            _playerAttackAcc = 0;
            _enemyAttackAcc  = 0;
            _rogueManaAcc    = 0;
            _manaRegenAcc    = 0;
            _hpRegenAcc      = 0;
            ResetCastState();
            DismissPet();

            LastLog = $"Character changed: {race} {characterClass} ({faction})";
        }
        TickUpdated?.Invoke();
    }

    // ─── Roster-Verwaltung ───
    public CharacterSlot CreateCharacter(Faction faction, Race race, CharacterClass characterClass, Gender gender)
    {
        if (!RaceClassInfo.IsRaceValidForFaction(race, faction))
            throw new ArgumentException($"Race '{race}' does not belong to faction '{faction}'.");
        if (!RaceClassInfo.IsClassAvailableForRace(race, characterClass))
            throw new ArgumentException($"Class '{characterClass}' is not available for race '{race}'.");

        // One slot per class – update the existing one instead of duplicating
        var existing = _roster.FirstOrDefault(s => s.Class == characterClass);
        if (existing is not null)
        {
            lock (_simLock)
            {
                existing.Faction = faction;
                existing.Race    = race;
                existing.Gender  = gender;
                existing.Player.ChangeCharacter(faction, race);
                existing.Player.SetActiveClass(characterClass); // recalculates stats for new race
            }
            ActivateSlot(existing);
            return existing;
        }

        var stats = StatsFactory.GetBaseStats(characterClass, race);
        var newPlayer = new Player(stats, faction, race, characterClass);
        newPlayer.Inventory = _sharedInventory;
        var slot = new CharacterSlot
        {
            Id = _nextSlotId++,
            Faction = faction,
            Race = race,
            Gender = gender,
            Class = characterClass,
            Player = newPlayer
        };
        _roster.Add(slot);
        RosterChanged?.Invoke();
        ActivateSlot(slot);
        return slot;
    }

    public void SelectCharacter(int id)
    {
        var slot = _roster.FirstOrDefault(s => s.Id == id)
            ?? throw new ArgumentException($"Character {id} not found.");
        ActivateSlot(slot);
    }

    private void ActivateSlot(CharacterSlot slot)
    {
        lock (_simLock)
        {
            _activeSlotId = slot.Id;
            Faction = slot.Faction;
            Race = slot.Race;
            Gender = slot.Gender;
            PlayerClass = slot.Class;
            Player = slot.Player;

            // Virtuelle Taschen für den neuen aktiven Charakter aktualisieren
            Player.Inventory.UpdateBagsForLevel(Player.Level);

            Meter.Reset();
            _playerAttackAcc = 0;
            _enemyAttackAcc  = 0;
            _rogueManaAcc    = 0;
            _manaRegenAcc    = 0;
            _hpRegenAcc      = 0;
            ResetCastState();
            DismissPet();
            CurrentEnemy = _spawner.Spawn(Player.Stats.Level, 0);
        }
        TickUpdated?.Invoke();
    }

    public IReadOnlyList<Race> GetAvailableRaces(Faction faction) =>
        RaceClassInfo.RaceToFaction
            .Where(kv => kv.Value == faction)
            .Select(kv => kv.Key)
            .OrderBy(r => r.ToString())
            .ToList();

    public IReadOnlyList<CharacterClass> GetAvailableClasses() =>
        RaceClassInfo.GetAvailableClasses(Race);

    private Player CreatePlayerInternal()
    {
        var stats = StatsFactory.GetBaseStats(PlayerClass, Race);
        var p = new Player(stats, Faction, Race, PlayerClass);
        p.Inventory = _sharedInventory;
        return p;
    }
}
