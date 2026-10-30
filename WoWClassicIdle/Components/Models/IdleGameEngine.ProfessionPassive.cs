using Models;

// Passive profession slots (gathering professions + fishing, per zone): filled with characters,
// each character practices at most one profession passively.
public partial class IdleGameEngine
{
    private readonly List<PassiveAssignment> _passiveAssignments = new();

    public IReadOnlyList<PassiveAssignment> PassiveAssignments => _passiveAssignments;

    /// <summary>Display name of a character for slots and log (legacy saves without a name: race + class).</summary>
    public static string CharacterName(CharacterSlot slot) =>
        string.IsNullOrWhiteSpace(slot.Name) ? $"{slot.Race} {slot.Class}" : slot.Name;

    /// <summary>Display name of the active character (headers of combat, inventory, talents, loadout).</summary>
    public string ActiveCharacterName => ActiveCharacter is { } slot ? CharacterName(slot) : "Character";

    public CharacterSlot? GetCharacter(int id) => _roster.FirstOrDefault(s => s.Id == id);

    /// <summary>The character currently being played (null without a roster).</summary>
    public CharacterSlot? ActiveCharacter => _activeSlotId is { } id ? GetCharacter(id) : null;

    public PassiveAssignment? GetPassiveAssignment(Profession profession, string zoneId, int index) =>
        _passiveAssignments.FirstOrDefault(a => a.Matches(profession, zoneId, index));

    public PassiveAssignment? PassiveAssignmentOf(int characterId) =>
        _passiveAssignments.FirstOrDefault(a => a.CharacterId == characterId);

    /// <summary>Occupied passive slots of the profession (across all zones), max. <see cref="Professions.MaxPassiveWorkers"/>.</summary>
    public int PassiveWorkerCount(Profession profession) =>
        _passiveAssignments.Count(a => a.Profession == profession);

    /// <summary>
    /// Is there still a passive spot free in the profession for the character? Its own slot and a replaced slot don't count
    /// (moving/swapping is always allowed at the limit).
    /// </summary>
    public bool HasPassiveCapacity(Profession profession, int characterId, PassiveAssignment? replaced = null) =>
        _passiveAssignments.Count(a => a.Profession == profession && a.CharacterId != characterId && a != replaced)
        < Professions.MaxPassiveWorkers;

    /// <summary>May the character practice the profession passively at all? (gathering profession/fishing, from level 5)</summary>
    public static bool CanWorkPassive(CharacterSlot slot, Profession profession) =>
        Professions.Get(profession).HasPassiveSlots
        && slot.Player.Level >= Professions.MinCharacterLevel;

    /// <summary>Reason why the character cannot be assigned (UI text) – <c>null</c> if allowed.</summary>
    public string? PassiveAssignBlocker(CharacterSlot slot, Profession profession)
    {
        if (!Professions.Get(profession).HasPassiveSlots) return "No passive slots";
        if (slot.Player.Level < Professions.MinCharacterLevel) return $"Requires level {Professions.MinCharacterLevel}";
        // Every class may gather, including the character being played
        return null;
    }

    /// <summary>
    /// Fills a passive slot with a character (<c>null</c> = clear the slot).
    /// A character already working passively elsewhere moves into this slot.
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
            // Character deleted, profession without passive slots or zone locked (legacy) → slot free
            if (GetCharacter(assignment.CharacterId) is not { } character || !CanWorkPassive(character, assignment.Profession)
                || !IsZoneUnlocked(assignment.Profession, assignment.Group))
            {
                _passiveAssignments.Remove(assignment);
                continue;
            }
            if (assignment.Target is null) continue;

            // Account-wide progress
            var progress = _sharedProfessions.GetValueOrDefault(assignment.Profession)
                           ?? new ProfessionProgress(assignment.Profession);

            assignment.Elapsed += dt * ProfessionSpeedFactor(assignment.Profession);
            while (assignment.Elapsed >= Professions.PassiveDurationSeconds)
            {
                assignment.Elapsed -= Professions.PassiveDurationSeconds;
                ExecuteProfession(progress, assignment.Target, Professions.PassiveSkillUpChance, source: CharacterName(character));
            }
        }
    }
}
