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

        // Namen sind im Roster eindeutig (der eigene Slot beim Bearbeiten ausgenommen)
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
                // Bereits gewählte Zone bleibt erhalten; sonst Startgebiet der (neuen) Rasse
                existing.CombatZoneId ??= CombatZoneRegistry.StartZoneFor(race)?.Id;
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

    // Charakter, der vor einem Wechsel in den Berufsfenstern gespielt wurde – wird beim Kämpfen wiederhergestellt
    private int? _characterBeforeProfessions;

    /// <summary>
    /// Wechsel aus den Berufsfenstern (Klick in der Charakterliste, Hauptberuf eines anderen Charakters):
    /// merkt sich beim ersten Wechsel den ursprünglichen Charakter, siehe <see cref="RestoreCharacterAfterProfessions"/>.
    /// </summary>
    public void SelectCharacterForProfessions(int id)
    {
        int? original = _characterBeforeProfessions ?? _activeSlotId;
        SelectCharacter(id);
        _characterBeforeProfessions = original != id ? original : null;
    }

    /// <summary>Zurück zum Kämpfen: ursprünglich gespielten Charakter wieder auswählen (falls noch vorhanden).</summary>
    public void RestoreCharacterAfterProfessions()
    {
        if (_characterBeforeProfessions is not { } id) return;
        _characterBeforeProfessions = null;
        if (id != _activeSlotId && _roster.Any(s => s.Id == id))
            SelectCharacter(id);
    }

    private void ActivateSlot(CharacterSlot slot)
    {
        // Jede andere Auswahl (Charakterauswahl, neuer Charakter) ist eine bewusste Entscheidung → nichts wiederherstellen
        _characterBeforeProfessions = null;
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
            _pendingRareEnemy = null;
            _travelRegenAcc  = 0;
            CurrentEnemy = SpawnEnemy();
        }
        TickUpdated?.Invoke();
    }

    /// <summary>
    /// Debug: Setzt alle Charaktere im Roster auf dasselbe Level und spawnt einen passenden Gegner.
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
