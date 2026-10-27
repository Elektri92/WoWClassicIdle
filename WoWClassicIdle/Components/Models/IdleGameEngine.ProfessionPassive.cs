using Models;

// Passive Berufs-Slots (Sammelberufe + Angeln, je Zone): werden mit Charakteren belegt,
// jeder Charakter übt höchstens einen Beruf passiv aus.
public partial class IdleGameEngine
{
    private readonly List<PassiveAssignment> _passiveAssignments = new();

    public IReadOnlyList<PassiveAssignment> PassiveAssignments => _passiveAssignments;

    /// <summary>Anzeigename eines Charakters (Rasse + Klasse) für Slots und Protokoll.</summary>
    public static string CharacterName(CharacterSlot slot) => $"{slot.Race} {slot.Class}";

    public CharacterSlot? GetCharacter(int id) => _roster.FirstOrDefault(s => s.Id == id);

    /// <summary>Der aktuell gespielte Charakter (null ohne Roster).</summary>
    public CharacterSlot? ActiveCharacter => _activeSlotId is { } id ? GetCharacter(id) : null;

    public PassiveAssignment? GetPassiveAssignment(Profession profession, string zoneId, int index) =>
        _passiveAssignments.FirstOrDefault(a => a.Matches(profession, zoneId, index));

    public PassiveAssignment? PassiveAssignmentOf(int characterId) =>
        _passiveAssignments.FirstOrDefault(a => a.CharacterId == characterId);

    /// <summary>Belegte passive Slots des Berufs (über alle Zonen), max. <see cref="Professions.MaxPassiveWorkers"/>.</summary>
    public int PassiveWorkerCount(Profession profession) =>
        _passiveAssignments.Count(a => a.Profession == profession);

    /// <summary>
    /// Ist für den Charakter noch ein passiver Platz im Beruf frei? Sein eigener Slot und ein ersetzter Slot zählen nicht mit
    /// (Verschieben/Tauschen bleibt beim Limit immer erlaubt).
    /// </summary>
    public bool HasPassiveCapacity(Profession profession, int characterId, PassiveAssignment? replaced = null) =>
        _passiveAssignments.Count(a => a.Profession == profession && a.CharacterId != characterId && a != replaced)
        < Professions.MaxPassiveWorkers;

    /// <summary>Darf der Charakter den Beruf grundsätzlich passiv ausüben? (Sammelberuf/Angeln, ab Level 5)</summary>
    public static bool CanWorkPassive(CharacterSlot slot, Profession profession) =>
        Professions.Get(profession).HasPassiveSlots
        && slot.Player.Level >= Professions.MinCharacterLevel;

    /// <summary>Grund, warum der Charakter nicht zugewiesen werden kann (UI-Text) – <c>null</c> wenn erlaubt.</summary>
    public string? PassiveAssignBlocker(CharacterSlot slot, Profession profession)
    {
        if (!Professions.Get(profession).HasPassiveSlots) return "No passive slots";
        if (slot.Player.Level < Professions.MinCharacterLevel) return $"Requires level {Professions.MinCharacterLevel}";
        // Der aktive Charakter arbeitet im aktiven Slot – nicht zusätzlich passiv
        if (slot.Id == _activeSlotId) return "Active character";
        return null;
    }

    /// <summary>
    /// Pausiert, solange derselbe Charakter diesen Beruf gerade aktiv ausübt
    /// (z. B. nach einem Charakterwechsel auf einen passiv eingesetzten Charakter).
    /// </summary>
    public bool IsPassivePaused(PassiveAssignment assignment) =>
        ActiveProfessionRun is { } run
        && run.Profession == assignment.Profession
        && GetCharacter(assignment.CharacterId)?.Player == run.Player;

    /// <summary>
    /// Belegt einen passiven Slot mit einem Charakter (<c>null</c> = Slot leeren).
    /// Ein Charakter, der schon woanders passiv arbeitet, wechselt in diesen Slot.
    /// </summary>
    public void AssignPassive(Profession profession, string zoneId, int index, int? characterId)
    {
        lock (_simLock)
        {
            var existing = GetPassiveAssignment(profession, zoneId, index);
            if (characterId is not { } id)
            {
                if (existing is not null) _passiveAssignments.Remove(existing);
            }
            else if (GetCharacter(id) is { } character && PassiveAssignBlocker(character, profession) is null
                     && HasPassiveCapacity(profession, id, existing) && IsZoneUnlocked(profession, zoneId))
            {
                if (existing?.CharacterId == id) return;
                _passiveAssignments.RemoveAll(a => a.CharacterId == id || a == existing);
                _passiveAssignments.Add(new PassiveAssignment
                {
                    Profession  = profession,
                    Group       = zoneId,
                    Index       = index,
                    CharacterId = id,
                    Target      = zoneId,
                });
            }
        }
        ProfessionUpdated?.Invoke();
    }

    private void AdvancePassiveSlots(double dt)
    {
        foreach (var assignment in _passiveAssignments.ToList())
        {
            // Charakter gelöscht, Beruf ohne passive Slots oder Zone gesperrt (Altstand) → Slot frei
            if (GetCharacter(assignment.CharacterId) is not { } character || !CanWorkPassive(character, assignment.Profession)
                || !IsZoneUnlocked(assignment.Profession, assignment.Group))
            {
                _passiveAssignments.Remove(assignment);
                continue;
            }
            if (assignment.Target is null || IsPassivePaused(assignment)) continue;

            // Accountweiter Fortschritt – passive Durchläufe geben ohnehin keine Skillpunkte
            var progress = _sharedProfessions.GetValueOrDefault(assignment.Profession)
                           ?? new ProfessionProgress(assignment.Profession);

            assignment.Elapsed += dt;
            while (assignment.Elapsed >= Professions.PassiveDurationSeconds)
            {
                assignment.Elapsed -= Professions.PassiveDurationSeconds;
                ExecuteProfession(progress, assignment.Target, isActive: false, source: CharacterName(character));
            }
        }
    }
}
