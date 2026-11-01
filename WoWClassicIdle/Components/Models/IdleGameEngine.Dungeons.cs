using System.Collections.Immutable;
using Models;

// Dungeons: fixed runs of trash groups and bosses, boss loot, wipes and the lockout per character.
public partial class IdleGameEngine
{
    public DungeonRunState? CurrentDungeonRun => ActiveCharacter?.DungeonRun;

    /// <summary>Free dungeon runs of the active character within the lockout window.</summary>
    public int DungeonRunsLeft => ActiveCharacter is { } slot ? DungeonRunsLeftFor(slot) : 0;

    /// <summary>Time until the next run is free; null while runs are left.</summary>
    public TimeSpan? DungeonLockoutRemaining
    {
        get
        {
            if (ActiveCharacter is not { } slot || DungeonRunsLeftFor(slot) > 0) return null;
            return ActiveRunStarts(slot).Min() + DungeonRules.LockoutWindow - DateTime.UtcNow;
        }
    }

    /// <summary>Completed runs of the dungeon by the active character.</summary>
    public int DungeonClears(CombatZone zone) => ActiveCharacter?.DungeonClears.GetValueOrDefault(zone.Id) ?? 0;

    /// <summary>Reason why the lockout blocks the dungeon (null = not locked).</summary>
    public string? DungeonLockoutBlocker(CombatZone zone) =>
        zone.IsDungeon && DungeonLockoutRemaining is { } remaining
            ? $"Lockout: next run in {FormatLockout(remaining)}"
            : null;

    private static string FormatLockout(TimeSpan remaining)
    {
        int minutes = Math.Max(1, (int)Math.Ceiling(remaining.TotalMinutes));
        return $"{minutes / 60}h {minutes % 60}m";
    }

    private static IEnumerable<DateTime> ActiveRunStarts(CharacterSlot slot)
    {
        var now = DateTime.UtcNow;
        return slot.DungeonRunStartsUtc.Where(start => now - start < DungeonRules.LockoutWindow);
    }

    private static int DungeonRunsLeftFor(CharacterSlot slot) =>
        Math.Max(0, DungeonRules.RunsPerWindow - ActiveRunStarts(slot).Count());

    /// <summary>Starts a new run and takes up a lockout slot – counting on start keeps leaving and re-entering from bypassing it.</summary>
    private static void BeginDungeonRun(CharacterSlot slot)
    {
        slot.DungeonRunStartsUtc = [.. ActiveRunStarts(slot), DateTime.UtcNow];
        slot.DungeonRun = new DungeonRunState { Party = BuildPartyStates(slot), SelfRole = SelfRoleOf(slot) };
    }

    /// <summary>Arrival at a dungeon. Without a free run the character stays outside.</summary>
    private void EnterDungeon(CharacterSlot slot, CombatZone zone, string fromZoneId)
    {
        if (DungeonRunsLeftFor(slot) == 0)
        {
            slot.CombatZoneId = zone.Dungeon!.EntranceZoneId;
            LastLog = $"🔒 {zone.Name} is locked – returning to the entrance";
            return;
        }
        // Coming directly from another dungeon keeps the original way back
        if (CombatZoneRegistry.Get(fromZoneId) is { IsDungeon: false })
            slot.DungeonReturnZoneId = fromZoneId;
        BeginDungeonRun(slot);
        LastLog = $"⚔ {slot.Name} entered {zone.Name}";
    }

    /// <summary>
    /// End of a run, successful or not: the character leaves through the entrance and travels back to the zone it came from.
    /// Returns the name of that zone.
    /// </summary>
    private string LeaveDungeonRun(CharacterSlot slot, DungeonDefinition dungeon)
    {
        ClearPartyBuffs(slot);
        slot.DungeonRun = null;
        slot.CombatZoneId = dungeon.EntranceZoneId;
        var entrance = CombatZoneRegistry.Get(dungeon.EntranceZoneId)!;
        DiscoverZone(slot, entrance, grantXp: true);

        var target = CombatZoneRegistry.Get(slot.DungeonReturnZoneId) ?? entrance;
        slot.DungeonReturnZoneId = null;
        if (target.Id == entrance.Id) return entrance.Name;

        var ground = ZoneTravel.GroundFor(slot.Class, slot.Player.Level, MountOf(slot));
        slot.Travel = new ZoneTravelState
        {
            FromZoneId   = entrance.Id,
            TargetZoneId = target.Id,
            Route        = ZoneTravel.Route(entrance, target, slot.Faction, ground, slot.DiscoveredZoneIds).Faster(DungeonRules.ReturnTravelSpeed),
            IsDungeonReturn = true,
        };
        return target.Name;
    }

    // Companions' buffs end with the group
    private static void ClearPartyBuffs(CharacterSlot slot)
    {
        foreach (var state in slot.DungeonRun?.Party ?? [])
            slot.Player.Buffs.Remove(state.SpellAbility.Id);
    }

    /// <summary>Next enemy of the run – null without a running run.</summary>
    private Enemy? SpawnDungeonEnemy(CombatZone zone)
    {
        if (ActiveCharacter?.DungeonRun is not { } run || zone.Dungeon is not { } dungeon) return null;
        if (run.EncounterIndex >= dungeon.Encounters.Count) return null;
        return DungeonEnemies.Build(zone, dungeon.Encounters[run.EncounterIndex], Player.Stats.Level, _rng);
    }

    /// <summary>Kill in a dungeon: boss loot, next encounter, end of the run.</summary>
    private void AdvanceDungeonRun(Enemy dead)
    {
        if (!dead.FromDungeon || ActiveCharacter is not { DungeonRun: { } run } slot
            || CurrentCombatZone is not { Dungeon: { } dungeon } zone) return;

        var encounter = dungeon.Encounters[run.EncounterIndex];
        if (encounter.IsBoss)
            DropBossLoot(zone, encounter, dead);

        run.KillsInEncounter++;
        if (run.KillsInEncounter < encounter.Kills) return;

        run.KillsInEncounter = 0;
        run.EncounterIndex++;
        if (run.EncounterIndex < dungeon.Encounters.Count) return;

        slot.DungeonClears = slot.DungeonClears.SetItem(zone.Id, slot.DungeonClears.GetValueOrDefault(zone.Id) + 1);
        LastLog = $"🏆 {zone.Name} cleared! Returning to {LeaveDungeonRun(slot, dungeon)}";
    }

    // Exactly one item from the table of the boss
    private void DropBossLoot(CombatZone zone, DungeonEncounter encounter, Enemy dead)
    {
        if (DisableGearDrops) return;
        var item = DungeonLootRegistry.RollBossDrop(zone.Id, encounter.BossIndex, Player.ActiveClass, _rng);
        if (item is null) return;
        LastLog = $"💀 {dead.MobName} defeated: {item.Name}";
        AddLootOrSell(item); // overwrites the log when a duplicate is sold
    }

    /// <summary>Death in a dungeon: the encounter starts over; the last allowed wipe ends the run after the resurrection.</summary>
    private void HandleDungeonDeath()
    {
        if (ActiveCharacter is not { DungeonRun: { } run }) return;
        run.Wipes++;
        run.KillsInEncounter = 0;
        LastLog = "Your group was defeated! Resurrecting at the Spirit Healer…";
    }

    /// <summary>Back from the Spirit Healer: a failed run sends the character home now, not while it is still a ghost.</summary>
    private void FinishFailedDungeonRun()
    {
        if (ActiveCharacter is not { DungeonRun: { Wipes: >= DungeonRules.MaxWipes } } slot
            || CurrentCombatZone is not { Dungeon: { } dungeon } zone) return;
        LastLog = $"☠ The run in {zone.Name} failed – returning to {LeaveDungeonRun(slot, dungeon)}";
    }

    // ─── NPC group ───

    // Headless simulation without a roster character
    private IReadOnlyList<DungeonPartyMemberState>? _simParty;

    /// <summary>NPC group of the running run (empty outside of dungeons).</summary>
    public IReadOnlyList<DungeonPartyMemberState> CurrentDungeonParty => ActiveCharacter?.DungeonRun?.Party ?? _simParty ?? [];

    // ─── Shared health bar ───
    // The bar adds up the health of all five members. The fight itself keeps running on the character's own health:
    // what the bar shows is that health multiplied by GroupHealthScale, and hits and heals grow by the same factor.
    // The ratio of damage to health therefore stays the same, also for percentage heals and shields.

    /// <summary>Health of the whole group: the own character plus all companions.</summary>
    public int GroupMaxHp => Player.Stats.MaxHp + CurrentDungeonParty.Sum(s => s.MaxHpAt(Player.Level));

    /// <summary>Factor from the character's own health to the shared bar; 1 outside of dungeons.</summary>
    public double GroupHealthScale
    {
        get
        {
            int own = Player.Stats.MaxHp;
            return own > 0 && CurrentDungeonParty.Count > 0 ? (double)GroupMaxHp / own : 1.0;
        }
    }

    public int ToGroupHealth(int amount) => (int)Math.Round(amount * GroupHealthScale);

    /// <summary>Stores the group for the next run of the active character; invalid setups are ignored.</summary>
    public void SetDungeonParty(DungeonPartySetup setup)
    {
        lock (_simLock)
        {
            if (ActiveCharacter is { } slot && DungeonPartyRegistry.IsValid(setup, slot.Faction, slot.Class))
                slot.DungeonPartySetup = setup;
        }
    }

    /// <summary>Group of a simulation run (debug only).</summary>
    public void DebugSetSimParty(IReadOnlyList<DungeonPartyMember> members, DungeonRole selfRole)
    {
        _simParty = ToStates(members);
        _simSelfRole = selfRole;
    }

    private DungeonRole? _simSelfRole;
    private double _selfHealAcc;

    /// <summary>Slot the active character fills in the running run; null outside of dungeons.</summary>
    public DungeonRole? CurrentDungeonSelfRole => ActiveCharacter?.DungeonRun?.SelfRole ?? _simSelfRole;

    private static DungeonRole SelfRoleOf(CharacterSlot slot) =>
        slot.DungeonPartySetup?.SelfRole ?? DungeonPartyRegistry.DefaultRole(slot.Class);

    private static IReadOnlyList<DungeonPartyMemberState> BuildPartyStates(CharacterSlot slot)
    {
        var setup = slot.DungeonPartySetup
                    ?? DungeonPartyRegistry.DefaultSetup(slot.Id, slot.Faction, slot.Class, DungeonPartyRegistry.DefaultRole(slot.Class));
        return ToStates(DungeonPartyRegistry.Build(slot.Id, slot.Faction, setup));
    }

    // The first casts are staggered so the group does not act in lockstep
    private static IReadOnlyList<DungeonPartyMemberState> ToStates(IReadOnlyList<DungeonPartyMember> members) =>
        members.Select((member, i) => new DungeonPartyMemberState(member, member.Spell.Interval * (i + 1) / (members.Count + 1))).ToList();

    private double PartyDamageReduction =>
        CurrentEnemy is { FromDungeon: true } && CurrentDungeonSelfRole == DungeonRole.Tank ? DungeonRules.SelfTankReduction : 0;

    // Damage the tank prevented counts as healing, like absorbs of the player's own shields
    private void RecordPartyMitigation(int enemyDamage)
    {
        if (CurrentEnemy is not { FromDungeon: true }) return;
        foreach (var state in CurrentDungeonParty)
            if (Player.Buffs.IsActive(state.SpellAbility.Id))
                Meter.RecordAbility(state.SpellAbility, CombatActionKind.Heal, (int)Math.Round(enemyDamage * state.Member.Spell.Reduction));
    }

    /// <summary>A companion casts its spell – the UI plays the animation. Fired before the matching combat number.</summary>
    public event Action<DungeonPartyMemberState>? PartySpellCast;

    /// <summary>The NPCs cast their spells on their own. Returns the damage dealt to the enemy.</summary>
    private int UpdateDungeonParty(double dt, double enemyDmgTakenMult)
    {
        if (CurrentEnemy is not { FromDungeon: true, IsAlive: true } enemy) return 0;

        UpdateSelfHealing(dt);

        int dealt = 0;
        foreach (var state in CurrentDungeonParty)
        {
            state.CastFlash = Math.Max(0, state.CastFlash - dt);
            state.MitigationRemaining = Math.Max(0, state.MitigationRemaining - dt);
            state.Cooldown = Math.Max(0, state.Cooldown - dt);
            if (state.Cooldown > 0 || !enemy.IsAlive) continue;

            var spell = state.Member.Spell;
            switch (spell.Kind)
            {
                case NpcSpellKind.Damage:
                    double variance = 0.85 + _rng.NextDouble() * 0.30;
                    double roleFactor = CurrentDungeonSelfRole == DungeonRole.Damage ? 1.0 : DungeonRules.CompanionDamageWithoutSelfDps;
                    int dmg = Math.Max(1, (int)Math.Round(spell.DamageAt(enemy.Stats.Level) * variance * enemyDmgTakenMult * roleFactor));
                    PartySpellCast?.Invoke(state);
                    Meter.RecordAbility(state.SpellAbility, CombatActionKind.Damage, enemy.TakeDamage(dmg));
                    CombatTextRaised?.Invoke(new CombatText(CombatTextTarget.Enemy, CombatTextKind.Ability, dmg, FlightMs: spell.FlightMs));
                    dealt += dmg;
                    break;

                case NpcSpellKind.Heal:
                    if (Player.CurrentHp >= Player.Stats.MaxHp) continue; // nobody hurt: the spell stays ready
                    int before = Player.CurrentHp;
                    Player.RegenHp((int)Math.Ceiling(Player.Stats.MaxHp * spell.HealShare));
                    int healed = Player.CurrentHp - before;
                    PartySpellCast?.Invoke(state);
                    Meter.RecordAbility(state.SpellAbility, CombatActionKind.Heal, healed);
                    CombatTextRaised?.Invoke(new CombatText(CombatTextTarget.Player, CombatTextKind.Heal, healed, FlightMs: spell.FlightMs));
                    break;

                default:
                    state.MitigationRemaining = spell.Duration;
                    Player.Buffs.Apply(state.SpellAbility); // shows up in the player's buff bar
                    PartySpellCast?.Invoke(state);
                    break;
            }
            state.Cooldown = spell.Interval;
            state.CastFlash = 0.4;
        }
        return dealt;
    }

    // Filling the healer slot: the character keeps itself up like an NPC healer would
    private void UpdateSelfHealing(double dt)
    {
        if (CurrentDungeonSelfRole != DungeonRole.Healer) return;
        _selfHealAcc += dt;
        if (_selfHealAcc < 1.0) return;
        _selfHealAcc -= 1.0;
        int before = Player.CurrentHp;
        Player.RegenHp((int)Math.Ceiling(Player.Stats.MaxHp * DungeonRules.HealerHpPerSecond));
        if (Player.CurrentHp > before)
            Meter.RecordTalentHeal("Healer role", Player.CurrentHp - before);
    }

    /// <summary>Lockout and clears from the save game (before the zone: it restores the run).</summary>
    private static void RestoreDungeons(CharacterSlot slot, SlotSaveData data)
    {
        if (data.DungeonSelfRole is { } role && data.DungeonCompanions is { } companions)
        {
            var setup = new DungeonPartySetup(role, companions);
            if (DungeonPartyRegistry.IsValid(setup, slot.Faction, slot.Class))
                slot.DungeonPartySetup = setup;
        }
        slot.DungeonReturnZoneId = CombatZoneRegistry.Get(data.DungeonReturnZone) is { IsDungeon: false } home ? home.Id : null;
        slot.DungeonRunStartsUtc = (data.DungeonRunStarts ?? []).ToImmutableList();
        if (data.DungeonClears is { } clears)
            slot.DungeonClears = clears.Where(c => c.Value > 0).ToImmutableDictionary();
    }

    /// <summary>Takes over the running run; a dungeon zone without valid progress puts the character at the entrance.</summary>
    private static void RestoreDungeonRun(CharacterSlot slot, DungeonDefinition dungeon, SlotSaveData data)
    {
        if (data.DungeonEncounter is { } index && index >= 0 && index < dungeon.Encounters.Count && data.DungeonWipes < DungeonRules.MaxWipes)
        {
            slot.DungeonRun = new DungeonRunState
            {
                EncounterIndex   = index,
                KillsInEncounter = Math.Max(0, data.DungeonEncounterKills),
                Wipes            = Math.Max(0, data.DungeonWipes),
                Party            = BuildPartyStates(slot),
                SelfRole         = SelfRoleOf(slot),
            };
            return;
        }
        slot.CombatZoneId = dungeon.EntranceZoneId;
    }

    /// <summary>Debug: frees all lockout slots of the active character (UI only reachable via BuildInfo.IsDebug).</summary>
    public void DebugResetDungeonLockout()
    {
        lock (_simLock)
        {
            if (ActiveCharacter is { } slot)
                slot.DungeonRunStartsUtc = ImmutableList<DateTime>.Empty;
        }
        TickUpdated?.Invoke();
    }
}
