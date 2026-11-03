using Models;

// NPC group in a dungeon run: spells, group buffs, trash and boss strengths, the warrior's rage and once-per-run rescues.
public partial class IdleGameEngine
{
    // Enemy the group is fighting and for how long: drives the rage of the warrior tank and the boss pull
    private Enemy? _partyFightEnemy;
    private double _partyFightSeconds;

    private double PartyDamageReduction =>
        CurrentEnemy is { FromDungeon: true } && CurrentDungeonSelfRole == DungeonRole.Tank ? DungeonRules.SelfTankReduction : 0;

    // Damage the tank prevented counts as healing, like absorbs of the player's own shields
    private void RecordPartyMitigation(int enemyDamage)
    {
        if (CurrentEnemy is not { FromDungeon: true }) return;
        foreach (var state in CurrentDungeonParty)
            if (Player.Buffs.IsActive(state.SpellAbility.Id))
                Meter.RecordAbility(state.SpellAbility, CombatActionKind.Heal, (int)Math.Round(enemyDamage * state.CurrentMitigation));
    }

    /// <summary>A companion casts its spell – the UI plays the animation. Fired before the matching combat number.</summary>
    public event Action<DungeonPartyMemberState>? PartySpellCast;

    /// <summary>The NPCs cast their spells on their own. Returns the damage dealt to the enemy.</summary>
    private int UpdateDungeonParty(double dt, double enemyDmgTakenMult)
    {
        if (CurrentEnemy is not { FromDungeon: true, IsAlive: true } enemy) return 0;

        UpdateSelfHealing(dt);
        TrackPartyFight(enemy, dt);
        ApplyPartyAuras();

        int dealt = 0;
        foreach (var state in CurrentDungeonParty)
        {
            state.CastFlash = Math.Max(0, state.CastFlash - dt);
            state.MitigationRemaining = Math.Max(0, state.MitigationRemaining - dt);
            state.Cooldown = Math.Max(0, state.Cooldown - dt);
            dealt += UpdateExtraSpell(state, enemy, dt, enemyDmgTakenMult);
            if (state.Cooldown > 0 || !enemy.IsAlive) continue;

            var spell = state.Member.Spell;
            switch (spell.Kind)
            {
                case NpcSpellKind.Damage:
                    // Filling the tank or healer slot: three companion damage dealers share the damage of two
                    double roleFactor = CurrentDungeonSelfRole == DungeonRole.Damage ? 1.0 : DungeonRules.CompanionDamageWithoutSelfDps;
                    PartySpellCast?.Invoke(state);
                    dealt += PartyHit(state.SpellAbility, spell, enemy, enemyDmgTakenMult * roleFactor * state.Member.Trait.ScaleAgainst(enemy));
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
                    state.SetMitigation(spell.Mitigation * (state.Member.Trait.Ramp?.FactorAt(_partyFightSeconds) ?? 1));
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

    // One hit of a companion spell on the enemy, with ±15% variance
    private int PartyHit(Ability source, NpcSpell spell, Enemy enemy, double multiplier)
    {
        double variance = 0.85 + _rng.NextDouble() * 0.30;
        int dmg = Math.Max(1, (int)Math.Round(spell.DamageAt(enemy.Stats.Level) * variance * multiplier));
        Meter.RecordAbility(source, CombatActionKind.Damage, enemy.TakeDamage(dmg));
        CombatTextRaised?.Invoke(new CombatText(CombatTextTarget.Enemy, CombatTextKind.Ability, dmg, FlightMs: spell.FlightMs));
        return dmg;
    }

    // Consecration (only trash) or Revenge (only bosses) of a tank: not part of the damage dealers' share
    private int UpdateExtraSpell(DungeonPartyMemberState state, Enemy enemy, double dt, double enemyDmgTakenMult)
    {
        if (state.Member.Trait.ExtraSpell is not { } spell || state.ExtraSpellAbility is not { } ability) return 0;
        state.ExtraSpellCooldown = Math.Max(0, state.ExtraSpellCooldown - dt);
        if (enemy.IsBoss != state.Member.Trait.ExtraSpellOnBosses || state.ExtraSpellCooldown > 0 || !enemy.IsAlive) return 0;
        state.ExtraSpellCooldown = spell.Interval;
        return PartyHit(ability, spell, enemy, enemyDmgTakenMult);
    }

    // A new enemy starts a new fight: rage starts over, a boss pull gets the priest's shield
    private void TrackPartyFight(Enemy enemy, double dt)
    {
        if (ReferenceEquals(enemy, _partyFightEnemy))
        {
            _partyFightSeconds += dt;
            return;
        }
        _partyFightEnemy = enemy;
        _partyFightSeconds = 0;
        if (!enemy.IsBoss) return;
        foreach (var state in CurrentDungeonParty)
        {
            if (state.OpenerAbility is not { } shield) continue;
            Player.Buffs.Apply(shield, maxHp: Player.Stats.MaxHp);
            state.CastFlash = 0.4;
        }
    }

    // Group buffs: set again whenever they are missing (death, form change) and switched when the resource changes
    private void ApplyPartyAuras()
    {
        var resource = Player.Resource;
        foreach (var state in CurrentDungeonParty)
        {
            if (state.Member.Trait.Aura is not { } aura) continue;
            var wanted = aura.For(resource);
            // A spell the character casts itself wins: both share a name, so they would keep replacing each other
            if (wanted is not null && Player.EquippedAbilities.Any(a => a.Name == wanted.Name)) wanted = null;
            foreach (var variant in aura.Variants)
                if (variant != wanted) Player.Buffs.Remove(variant.Id);
            if (wanted is not null && !Player.Buffs.IsActive(wanted.Id))
                Player.Buffs.Apply(wanted.Build(Player.Level));
        }
    }

    /// <summary>Rebirth or Soulstone: a companion with an unused rescue brings the character back instead of a wipe.</summary>
    private void TryPartyRescue()
    {
        if (CurrentEnemy is not { FromDungeon: true }) return;
        var rescuer = CurrentDungeonParty.FirstOrDefault(s => s.Member.Trait.Rescue != NpcRescue.None && !s.RescueUsed);
        if (rescuer is null) return;

        rescuer.RescueUsed = true;
        rescuer.CastFlash = 0.4;
        Player.CurrentHp = Math.Max(1, (int)Math.Round(Player.Stats.MaxHp * DungeonRules.RescueHpShare));
        CombatTextRaised?.Invoke(new CombatText(CombatTextTarget.Player, CombatTextKind.Heal, Player.CurrentHp));
        LastLog = $"✦ {NpcTrait.RescueName(rescuer.Member.Trait.Rescue)} ({rescuer.Member.Name}) – death averted!";
    }

    /// <summary>Removes everything the companions put on the character: spell buffs, shields and group buffs.</summary>
    private static void ClearPartyBuffs(CharacterSlot slot)
    {
        foreach (var state in slot.DungeonRun?.Party ?? [])
        {
            slot.Player.Buffs.Remove(state.SpellAbility.Id);
            if (state.OpenerAbility is { } shield) slot.Player.Buffs.Remove(shield.Id);
            foreach (var variant in state.Member.Trait.Aura?.Variants ?? [])
                slot.Player.Buffs.Remove(variant.Id);
        }
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
}
