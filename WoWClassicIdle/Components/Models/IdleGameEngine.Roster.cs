using Models;

// Create new systems as their own partial file; keep files below 300 lines where possible.
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

            // Update the existing player (progress is preserved!)
            Player.ChangeCharacter(faction, race);
            Player.SetActiveClass(characterClass);
            Player.SetClass(characterClass);   // ← ensures that Character.CurrentClass + loadout are updated
            Player.EquipStarterWeapon(characterClass);

            // Combat stats and swing timer belong to the old character
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
    public CharacterSlot CreateCharacter(string name, Faction faction, Race race, CharacterClass characterClass, Gender gender)
    {
        if (CharacterNames.Validate(name) is { } nameError)
            throw new ArgumentException(nameError);
        name = CharacterNames.Normalize(name);
        if (!RaceClassInfo.IsRaceValidForFaction(race, faction))
            throw new ArgumentException($"Race '{race}' does not belong to faction '{faction}'.");
        if (!RaceClassInfo.IsClassAvailableForRace(race, characterClass))
            throw new ArgumentException($"Class '{characterClass}' is not available for race '{race}'.");

        // One slot per class – update the existing one instead of duplicating
        var existing = _roster.FirstOrDefault(s => s.Class == characterClass);

        // Names are unique in the roster (excluding the own slot when editing)
        if (_roster.Any(s => s != existing && string.Equals(s.Name, name, StringComparison.OrdinalIgnoreCase)))
            throw new ArgumentException($"The name '{name}' is already taken by another character.");

        if (existing is not null)
        {
            lock (_simLock)
            {
                existing.Name    = name;
                existing.Player.Name = name;
                existing.Faction = faction;
                existing.Race    = race;
                existing.Gender  = gender;
                // An already chosen zone is kept; otherwise the starting zone of the (new) race
                existing.CombatZoneId ??= CombatZoneRegistry.StartZoneFor(race)?.Id;
                if (CombatZoneRegistry.Get(existing.CombatZoneId) is { } zone)
                    DiscoverZone(existing, zone, grantXp: false);
                existing.Player.ChangeCharacter(faction, race);
                existing.Player.SetActiveClass(characterClass); // recalculates stats for new race
            }
            ActivateSlot(existing);
            return existing;
        }

        var stats = StatsFactory.GetBaseStats(characterClass, race);
        var newPlayer = new Player(stats, faction, race, characterClass) { Name = name };
        newPlayer.Inventory = _sharedInventory;
        var slot = new CharacterSlot
        {
            Id = _nextSlotId++,
            Name = name,
            Faction = faction,
            Race = race,
            Gender = gender,
            Class = characterClass,
            Player = newPlayer,
            CombatZoneId = CombatZoneRegistry.StartZoneFor(race)?.Id
        };
        // The starting zone is known but gives no discovery XP
        if (CombatZoneRegistry.Get(slot.CombatZoneId) is { } startZone)
            DiscoverZone(slot, startZone, grantXp: false);
        lock (_simLock) _roster.Add(slot);
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

    // Character played before a switch in the profession windows – restored when fighting
    private int? _characterBeforeProfessions;

    /// <summary>
    /// Switch from the profession windows (click in the character list, main profession of another character):
    /// remembers the original character on the first switch, see <see cref="RestoreCharacterAfterProfessions"/>.
    /// </summary>
    public void SelectCharacterForProfessions(int id)
    {
        int? original = _characterBeforeProfessions ?? _activeSlotId;
        SelectCharacter(id);
        _characterBeforeProfessions = original != id ? original : null;
    }

    /// <summary>Back to fighting: select the originally played character again (if it still exists).</summary>
    public void RestoreCharacterAfterProfessions()
    {
        if (_characterBeforeProfessions is not { } id) return;
        _characterBeforeProfessions = null;
        if (id != _activeSlotId && _roster.Any(s => s.Id == id))
            SelectCharacter(id);
    }

    private void ActivateSlot(CharacterSlot slot)
    {
        // Any other selection (character selection, new character) is a deliberate decision → restore nothing
        _characterBeforeProfessions = null;
        lock (_simLock)
        {
            _activeSlotId = slot.Id;
            Faction = slot.Faction;
            Race = slot.Race;
            Gender = slot.Gender;
            PlayerClass = slot.Class;
            Player = slot.Player;

            // Update virtual bags for the new active character
            Player.Inventory.UpdateBagsForLevel(Player.Level);

            Meter.Reset();
            _playerAttackAcc = 0;
            _enemyAttackAcc  = 0;
            _rogueManaAcc    = 0;
            _manaRegenAcc    = 0;
            _hpRegenAcc      = 0;
            ResetCastState();
            DismissPet();
            _pendingRareEnemy = null;
            CurrentEnemy = SpawnEnemy();
        }
        TickUpdated?.Invoke();
    }

    /// <summary>
    /// Debug: sets all characters in the roster to the same level and spawns a matching enemy.
    /// </summary>
    public void SetAllCharactersLevel(int level)
    {
        level = Math.Clamp(level, 1, 60);
        lock (_simLock)
        {
            foreach (var slot in _roster)
                slot.Player.SetLevel(level);

            Player.Inventory.UpdateBagsForLevel(Player.Level);
            CurrentEnemy = SpawnEnemy();
            LastLog = $"All characters set to level {level}";
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
