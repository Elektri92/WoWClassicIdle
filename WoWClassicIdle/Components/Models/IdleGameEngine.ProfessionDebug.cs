using Models;

// Debug helpers for professions (UI only reachable via BuildInfo.IsDebug)
public partial class IdleGameEngine
{
    /// <summary>Time factor of the profession timer (active and passive slots); 1 = real time. Only the debug UI changes it.</summary>
    public double ProfessionSpeed { get; private set; } = 1.0;

    public void DebugSetProfessionSpeed(double speed)
    {
        lock (_simLock) ProfessionSpeed = Math.Clamp(speed, 1.0, 1000.0);
        ProfessionUpdated?.Invoke();
    }

    /// <summary>
    /// Sets all learned professions (shared + main professions of all characters) to <paramref name="skill"/>
    /// and the training tier whose cap allows this skill.
    /// </summary>
    public void DebugSetAllProfessionSkills(int skill)
    {
        lock (_simLock)
        {
            UnlockSharedProfessions();
            int rankIndex = Professions.Ranks.ToList().FindIndex(r => r.MaxSkill >= skill);
            if (rankIndex < 0) rankIndex = Professions.Ranks.Count - 1;

            var all = _sharedProfessions.Values
                .Concat(_roster.Select(s => s.Player.ProfessionState.Primary).OfType<ProfessionProgress>());
            foreach (var progress in all)
                progress.Restore(skill, rankIndex);
        }
        ProfessionUpdated?.Invoke();
    }

    /// <summary>
    /// Distributes all still free crafting professions to characters without a main profession (from level 5)
    /// and fills the passive slots of each gathering profession in its first zone. Returns a summary.
    /// </summary>
    public string DebugAssignAllProfessions()
    {
        int learned = 0, passive = 0;
        lock (_simLock)
        {
            UnlockSharedProfessions();
            var eligible = _roster.Where(s => s.Player.Level >= Professions.MinCharacterLevel).ToList();

            // Main professions: each profession only once, each character at most one
            var freeCrafting = new Queue<ProfessionDefinition>(
                Professions.All.Where(d => d.IsPrimary && ProfessionOwner(d.Profession) is null));
            foreach (var slot in eligible.Where(s => s.Player.ProfessionState.Primary is null))
            {
                if (!freeCrafting.TryDequeue(out var def)) break;
                if (slot.Player.ProfessionState.Learn(def.Profession, slot.Player.Level, slot.Player.ActiveClass) is not null) learned++;
            }

            // Passive slots in zone 1 – only characters not working passively anywhere yet (not the one being played).
            // Distribute evenly: round robin one character per profession, always the profession with the fewest workers first.
            var idle = new Queue<CharacterSlot>(
                eligible.Where(s => s.Id != _activeSlotId && PassiveAssignmentOf(s.Id) is null));
            var targets = Professions.All
                .Where(d => d.HasPassiveSlots)
                .Select(d => (Profession: d.Profession, Zone: ProfessionRegistry.GetZones(d.Profession).FirstOrDefault()))
                .Where(t => t.Zone is not null)
                .ToList();
            while (idle.Count > 0)
            {
                var open = targets
                    .Select(t => (t.Profession, Zone: t.Zone!, Index: FreeDebugPassiveIndex(t.Profession, t.Zone!)))
                    .Where(t => t.Index is not null && HasPassiveCapacity(t.Profession, -1))
                    .OrderBy(t => PassiveWorkerCount(t.Profession))
                    .FirstOrDefault();
                if (open.Index is not { } index) break;

                var slot = idle.Dequeue();
                _passiveAssignments.Add(new PassiveAssignment
                {
                    Profession  = open.Profession,
                    Group       = open.Zone.Id,
                    Index       = index,
                    CharacterId = slot.Id,
                    Target      = open.Zone.Id,
                });
                passive++;
            }
        }
        ProfessionUpdated?.Invoke();
        return $"Learned {learned} crafting professions, filled {passive} passive slots.";
    }

    private int? FreeDebugPassiveIndex(Profession profession, GatheringZone zone)
    {
        for (int i = 0; i < zone.PassiveSlotCount; i++)
            if (GetPassiveAssignment(profession, zone.Id, i) is null) return i;
        return null;
    }
}
