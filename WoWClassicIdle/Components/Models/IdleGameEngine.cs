using System.Timers;
using System.Diagnostics;
using Models;
using Timer = System.Timers.Timer;

public class IdleGameEngine
{
    private Timer _timer;
    private readonly Stopwatch _stopwatch = new();
#if DEBUG
    private readonly EnemySpawner _spawner = new(
        DefaultEnemyTemplates.Default.Concat(DebugEnemyTemplates.All).ToList());
#else
    private readonly EnemySpawner _spawner = new();
#endif
    private readonly Random _rng = new();

    private double _playerAttackAcc;
    private double _enemyAttackAcc;
    private double _rogueManaAcc;
    private double _manaRegenAcc;

    public double SpeedMultiplier { get; private set; } = 1.0;
    private const double MinSpeed = 0.1;
    private const double MaxSpeed = 10000.0;
    private readonly double _baseDt; // fester Zeitschritt pro Sub-Step in Sekunden

    // ─── Roster ───
    private readonly List<CharacterSlot> _roster = new();
    private int? _activeSlotId;
    private int _nextSlotId;

    public IReadOnlyList<CharacterSlot> CharacterRoster => _roster;
    public bool HasActiveCharacter => _activeSlotId.HasValue;

    internal int? ActiveSlotId => _activeSlotId;
    internal int NextSlotId    => _nextSlotId;

    // ─── Charakterauswahl ───
    public Faction Faction { get; private set; } = Faction.Alliance;
    public Race Race { get; private set; } = Race.Human;
    public Gender Gender { get; private set; } = Gender.Male;
    public CharacterClass PlayerClass { get; private set; } = CharacterClass.Warrior;
    public bool IsGameRunning { get; private set; }

    public Player Player { get; private set; } = null!;

    public Enemy? CurrentEnemy { get; set; }
    public int TotalKills { get; private set; }
    public int TotalDeaths { get; private set; }
    public string? LastLog { get; private set; }

    /// <summary>Verbleibende Sekunden beim Geistheiler, bevor der nächste Gegner spawnt.</summary>
    public double RespawnRemaining { get; private set; }
    /// <summary>Gesamtdauer des aktuellen Geistheiler-Aufenthalts (10 s + 1 s pro Spielerlevel).</summary>
    public double RespawnDuration { get; private set; }
    public bool IsAtSpiritHealer => RespawnRemaining > 0;
    public CombatMeter Meter { get; } = new();

    // Serialisiert Simulationsschritte mit Charakterwechseln (Timer-Thread vs. UI-Thread)
    private readonly object _simLock = new();

    public event Action? TickUpdated;

    /// <summary>Wird ausgelöst wenn ein neuer Charakter zum Roster hinzugefügt wird.</summary>
    public event Action? RosterChanged;

    /// <summary>Wird für jeden Treffer/jede Heilung ausgelöst (Timer-Thread!).</summary>
    public event Action<CombatText>? CombatTextRaised;

    /// <summary>Fires once per ability that becomes available on level-up (Timer-Thread!).</summary>
    public event Action<Ability>? AbilityUnlocked;

    /// <summary>Wird ausgelöst wenn ein Seltenes Item von einem Rare-Gegner gedroppt wird (Timer-Thread!).</summary>
    public event Action<ArmorItem>? ItemDropped;

    /// <summary>
    /// true = gedroppte Rüstung wird automatisch angelegt wenn sie eine Verbesserung ist
    /// (genutzt vom Debug-Simulator). Normalerweise false: Items landen nur im Inventar.
    /// </summary>
    public bool AutoEquipDrops { get; set; }

    public IdleGameEngine(int intervalMs = 100)
    {
        _baseDt = intervalMs / 1000.0;
        Player = CreatePlayerInternal();

        _timer = new Timer(intervalMs) { AutoReset = true };
        _timer.Elapsed += OnTick;
        CurrentEnemy = _spawner.Spawn(1, 0);
    }

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
        var slot = new CharacterSlot
        {
            Id = _nextSlotId++,
            Faction = faction,
            Race = race,
            Gender = gender,
            Class = characterClass,
            Player = new Player(stats, faction, race, characterClass)
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

            Meter.Reset();
            _playerAttackAcc = 0;
            _enemyAttackAcc  = 0;
            _rogueManaAcc    = 0;
            _manaRegenAcc    = 0;
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

    // ─── Spielsteuerung ───
    public void Start()
    {
        IsGameRunning = true;
        _stopwatch.Restart();
        _timer.Start();
    }

    public void Stop()
    {
        IsGameRunning = false;
        _timer.Stop();
        _stopwatch.Stop();
    }

    /// <summary>
    /// Setzt den Geschwindigkeitsmultiplikator und passt die Timer-Intervall entsprechend an.
    /// Wert wird auf [0.1, 20.0] geklemmt.
    /// </summary>
    public void SetSpeed(double multiplier)
    {
        SpeedMultiplier = Math.Clamp(multiplier, MinSpeed, MaxSpeed);
    }

    /// <summary>
    /// Setzt das Level des aktiven Charakters direkt und spawnt sofort einen passenden Gegner.
    /// </summary>
    public void SetPlayerLevel(int level)
    {
        lock (_simLock)
        {
            Player.SetLevel(Math.Clamp(level, 1, 60));
            CurrentEnemy = _spawner.Spawn(Player.Stats.Level, _stopwatch.Elapsed.TotalSeconds);
        }
        TickUpdated?.Invoke();
    }

    private Player CreatePlayerInternal()
    {
        var stats = StatsFactory.GetBaseStats(PlayerClass, Race);
        return new Player(stats, Faction, Race, PlayerClass);
    }

    private void OnTick(object? _, ElapsedEventArgs __)
        {
            int subSteps = Math.Max(1, (int)Math.Round(SpeedMultiplier));

            RunSteps(subSteps);

            TickUpdated?.Invoke();
        }

        /// <summary>
        /// Führt <paramref name="count"/> Simulationsschritte à <c>intervalMs</c> synchron aus –
        /// ohne Timer, z. B. für Headless-Simulationen.
        /// </summary>
        public void RunSteps(int count)
        {
            lock (_simLock)
            {
                for (int step = 0; step < count; step++)
                {
                    SimulateOneStep();
                }
            }
        }

        private bool PlayerNeedsHeal =>
            Player.CurrentHp <= Player.Stats.MaxHp * Ability.HealThreshold;

        /// <summary>Level, Cooldown und (bei Buffs/Debuffs) Ablauf erlauben einen Cast – Mana wird nicht geprüft.</summary>
        private bool IsReadyToCast(Ability ability) =>
            Player.Level >= ability.LevelRequired
            && Player.Cooldowns.IsReady(ability.Id)
            && !(ability.IsBuff && Player.Buffs.IsActive(ability.Id))
            && !(ability.IsDebuff && CurrentEnemy is { } e && e.Debuffs.IsActive(ability.Id))
            && !(ability.RequiresEnemyBelowHpPct > 0
                 && (CurrentEnemy is null || CurrentEnemy.CurrentHp > CurrentEnemy.Stats.MaxHp * ability.RequiresEnemyBelowHpPct));

        /// <summary>
        /// HP unter der Schwelle und eine ausgerüstete, bereite Heilung scheitert nur am Mana:
        /// dann werden keine anderen Fähigkeiten gewirkt, bis genug Mana da ist.
        /// </summary>
        private bool IsSavingManaForHeal() =>
            PlayerNeedsHeal
            && Player.EquippedAbilities.Any(a => a.IsHealing && IsReadyToCast(a) && Player.CurrentMana < a.ManaCost);

        /// <summary>Simulierte Spielzeit pro Schritt in Sekunden.</summary>
        public double StepSeconds => _baseDt;

        /// <summary>
        /// Würfelt Crit für einen vom Spieler verursachten Wert und verdoppelt ihn gegebenenfalls.
        /// Gilt für allen Schaden, jede Heilung und (sobald vorhanden) jeden Schild.
        /// </summary>
        private (int Amount, bool IsCrit) RollPlayerCrit(int amount, double extraCrit = 0)
        {
            bool isCrit = _rng.NextDouble() < (Player.Stats.CritChance + extraCrit);
            return (CombatEngine.ApplyCrit(amount, isCrit), isCrit);
        }

        private void SimulateOneStep()
        {
            // ── Tot: Wartezeit beim Geistheiler ──
            if (RespawnRemaining > 0)
            {
                RespawnRemaining = Math.Max(0, RespawnRemaining - _baseDt);
                return;
            }

            if (CurrentEnemy is not { IsAlive: true })
            {
                CurrentEnemy = _spawner.Spawn(Player.Stats.Level, _stopwatch.Elapsed.TotalSeconds);
                LastLog = $"{CurrentEnemy.Name} appeared!";
                _playerAttackAcc = 0;
                _enemyAttackAcc  = 0;
                return;
            }

            double dt = _baseDt;
            Meter.AddCombatTime(dt);

            int totalPlayerDmg = 0;
            int totalEnemyDmg  = 0;
            bool playerAttacked = false;
            bool enemyAttacked  = false;

            // ── Player HoT / MoT ticks ──
            foreach (var tick in Player.Buffs.Tick(dt))
            {
                if (tick.IsMana)
                {
                    // Mana-over-Time (z. B. Conjure Water) – kein Crit, kein Combat-Text
                    Player.RegenMana(tick.Amount);
                }
                else
                {
                    var (hotHeal, hotCrit) = RollPlayerCrit(tick.Amount);
                    int hpBefore = Player.CurrentHp;
                    Player.RegenHp(hotHeal);
                    Meter.RecordAbility(tick.Source, CombatActionKind.Heal, Player.CurrentHp - hpBefore);
                    CombatTextRaised?.Invoke(new CombatText(CombatTextTarget.Player, CombatTextKind.Heal, hotHeal, hotCrit));
                }
            }

            // ── Enemy DoT ticks (debuffs) ──
            foreach (var dot in CurrentEnemy.Debuffs.Tick(dt))
            {
                var (dotDmg, dotCrit) = RollPlayerCrit(dot.Amount);
                CurrentEnemy.TakeDamage(dotDmg);
                Meter.RecordAbility(dot.Source, CombatActionKind.Damage, dotDmg);
                CombatTextRaised?.Invoke(new CombatText(CombatTextTarget.Enemy, CombatTextKind.Ability, dotDmg, dotCrit));
                totalPlayerDmg += dotDmg;
                playerAttacked = true;

                // Lifesteal (z. B. Drain Life)
                if (dot.Source.DebuffDotLifesteal > 0)
                {
                    int lifeHeal = (int)Math.Round(dotDmg * dot.Source.DebuffDotLifesteal);
                    int hpBefore = Player.CurrentHp;
                    Player.RegenHp(lifeHeal);
                    CombatTextRaised?.Invoke(new CombatText(CombatTextTarget.Player, CombatTextKind.Heal, Player.CurrentHp - hpBefore, dotCrit));
                }
            }

            if (!CurrentEnemy.IsAlive)
            {
                HandleEnemyDeath();
                return;
            }

            // ── Rogue: Energie-Regeneration alle 3 Sekunden ──
            if (Player.ActiveClass == CharacterClass.Rogue)
            {
                _rogueManaAcc += dt;
                while (_rogueManaAcc >= 3.0)
                {
                    _rogueManaAcc -= 3.0;
                    Player.RegenMana(20);
                }
            }
            else if (Player.ActiveClass != CharacterClass.Warrior)
            {
                _manaRegenAcc += dt;
                while (_manaRegenAcc >= 5.0)
                {
                    _manaRegenAcc -= 5.0;
                    Player.RegenMana(Player.Stats.ManaRegenPer5);
                }
            }

            // Attack-Speed aus Charakter-Stat und Buffs – beschleunigt Auto-Attacks und Fähigkeiten-Cooldowns
            double attackSpeed    = Player.Stats.AttackSpeedMultiplier * Player.Buffs.AttackSpeedMultiplier;
            double playerInterval = Math.Max(0.1, Player.Stats.Speed / attackSpeed);
            double enemyInterval  = Math.Max(0.1, CurrentEnemy.Stats.Speed / CurrentEnemy.Debuffs.AttackSpeedMultiplier);
            double enemyDmgTakenMult = CurrentEnemy.Debuffs.DamageTakenMultiplier;

            _playerAttackAcc += dt;
            _enemyAttackAcc  += dt;

            // ── Spielerangriffe ──
            var nextSwingAbility = Player.EquippedAbilities
                .FirstOrDefault(a => a.ReplacesAutoAttack && Player.Level >= a.LevelRequired);

            while (_playerAttackAcc >= playerInterval && CurrentEnemy.IsAlive)
            {
                _playerAttackAcc -= playerInterval;
                playerAttacked = true;

                // On-next-swing (Heroic Strike): ersetzt den Schlag, erzeugt wie in Classic keine Rage
                if (nextSwingAbility is not null
                    && !IsSavingManaForHeal()
                    && Player.CurrentMana >= nextSwingAbility.MinResourceToCast
                    && Player.ConsumeMana(nextSwingAbility.ManaCost))
                {
                    double swingMult = Player.Buffs.DamageMultiplier * Player.Buffs.WeaponDamageMultiplier * enemyDmgTakenMult;
                    var (swingDmg, swingCrit) = RollPlayerCrit(
                        CombatEngine.CalculateAbilityDamage(nextSwingAbility, Player.Stats, _rng, swingMult));
                    CurrentEnemy.TakeDamage(swingDmg);
                    Meter.RecordAbility(nextSwingAbility, CombatActionKind.Damage, swingDmg);
                    CombatTextRaised?.Invoke(new CombatText(CombatTextTarget.Enemy, CombatTextKind.Ability, swingDmg, swingCrit));
                    totalPlayerDmg += swingDmg;
                    LastLog = $"⚔ {nextSwingAbility.Name} → {swingDmg} DMG!";
                    Player.TriggerAbilityUsed(nextSwingAbility.Name, nextSwingAbility.Damage);

                    // Passive Procs (z. B. Deadly Poison) – auch bei On-Next-Swing
                    foreach (var passive in Player.EquippedAbilities)
                        if (passive.IsPassive && passive.ProcChance > 0 && CurrentEnemy.IsAlive && _rng.NextDouble() < passive.ProcChance)
                            CurrentEnemy.Debuffs.ApplyStack(passive);

                    continue;
                }

                double autoMult = Player.Buffs.DamageMultiplier * Player.Buffs.WeaponDamageMultiplier * enemyDmgTakenMult;
                var (playerDmg, autoCrit) = RollPlayerCrit(
                    CombatEngine.CalculateAutoAttack(Player.Stats, _rng, autoMult));
                CurrentEnemy.TakeDamage(playerDmg);
                Meter.RecordAutoAttack(playerDmg, Player.EquippedWeapon?.IconUrl);
                CombatTextRaised?.Invoke(new CombatText(CombatTextTarget.Enemy, CombatTextKind.AutoAttack, playerDmg, autoCrit));
                totalPlayerDmg += playerDmg;

                // Warrior: +8 Rage pro Auto-Attack
                if (Player.ActiveClass == CharacterClass.Warrior)
                    Player.RegenMana(8);

                // Passive Procs (z. B. Deadly Poison)
                foreach (var passive in Player.EquippedAbilities)
                    if (passive.IsPassive && passive.ProcChance > 0 && CurrentEnemy.IsAlive && _rng.NextDouble() < passive.ProcChance)
                        CurrentEnemy.Debuffs.ApplyStack(passive);
            }

            if (!CurrentEnemy.IsAlive)
            {
                HandleEnemyDeath();
                return;
            }

            // ── Gegnerangriffe ──
            while (_enemyAttackAcc >= enemyInterval && Player.IsAlive)
            {
                _enemyAttackAcc -= enemyInterval;

                // Betäubung: Gegner kann nicht angreifen
                if (CurrentEnemy.Debuffs.IsStunned)
                    continue;

                enemyAttacked = true;

                // Immunität (z. B. Divine Shield): Treffer landet nicht
                if (Player.Buffs.HasImmunity)
                    continue;

                int enemyDmg = CombatEngine.CalculateDamage(
                    CurrentEnemy.Stats.Attack, Player.Stats.Defense, _rng);
                int taken = Player.TakeDamage(enemyDmg);
                CombatTextRaised?.Invoke(new CombatText(CombatTextTarget.Player, CombatTextKind.AutoAttack, taken));
                totalEnemyDmg += enemyDmg;

                // Vergeltungsschaden (Thorns / Lightning Shield)
                foreach (var retAbility in Player.Buffs.GetRetaliationSources())
                {
                    if (!CurrentEnemy.IsAlive) break;
                    int retDmg = retAbility.BuffRetaliationDamage;
                    CurrentEnemy.TakeDamage(retDmg);
                    Meter.RecordAbility(retAbility, CombatActionKind.Damage, retDmg);
                    CombatTextRaised?.Invoke(new CombatText(CombatTextTarget.Enemy, CombatTextKind.Ability, retDmg, false));
                    totalPlayerDmg += retDmg;
                    playerAttacked = true;
                }
            }

            if (!Player.IsAlive)
            {
                TotalDeaths++;
                Player.Buffs.Reset();   // wie in WoW: Tod entfernt alle Buffs
                Player.CurrentHp = Player.Stats.MaxHp;
                CurrentEnemy = null;
                RespawnDuration  = 10 + Player.Level;
                RespawnRemaining = RespawnDuration;
                LastLog = "Defeated! Resurrecting at the Spirit Healer…";
                _playerAttackAcc = 0;
                _enemyAttackAcc  = 0;
                return;
            }

            // ── Abilities (Cooldown-basiert) ──
            Player.Cooldowns.Tick(dt * attackSpeed);

            // Heilungen zuerst, dann Buffs/Debuffs (hohe Prio), dann Schaden
            foreach (var ability in Player.EquippedAbilities.OrderBy(a => a.IsHealing ? 0 : (a.IsBuff || a.IsDebuff) ? 1 : 2))
            {
                if (ability.ReplacesAutoAttack) continue;
                if (ability.IsPassive) continue;
                if (!IsReadyToCast(ability)) continue;
                if (ability.IsHealing && !PlayerNeedsHeal) continue;
                if (!ability.IsHealing && ability.ManaCost > 0 && IsSavingManaForHeal()) continue;
                if (!Player.ConsumeMana(Player.GetEffectiveManaCost(ability))) continue;

                // Life Tap: HP in Mana umwandeln – nur bei niedrigem Mana und ausreichend HP
                if (ability.IsLifeTap)
                {
                    if (Player.CurrentMana >= Player.Stats.MaxMana / 2 || PlayerNeedsHeal || Player.CurrentHp <= ability.LifeTapHpCost)
                        continue;
                    Player.TakeDamage(ability.LifeTapHpCost);
                    Player.RegenMana(ability.LifeTapManaGain);
                    CombatTextRaised?.Invoke(new CombatText(CombatTextTarget.Player, CombatTextKind.AutoAttack, ability.LifeTapHpCost));
                    LastLog = $"✦ {ability.Name} → -{ability.LifeTapHpCost} HP, +{ability.LifeTapManaGain} Mana";
                    Player.Cooldowns.StartCooldown(ability.Id, ability.Cooldown);
                    Player.TriggerAbilityUsed(ability.Name, 0);
                    continue;
                }

                if (ability.IsBuff)
                {
                    Player.Buffs.Apply(Player.GetEffectiveAbility(ability), Player.Stats.MaxMana, Player.Stats.MaxHp);
                    LastLog = $"✦ {ability.Name} active ({ability.BuffDuration:0}s)";
                }
                else if (ability.IsDamage)
                {
                    double abilityMult = Player.Buffs.DamageMultiplier
                        * (ability.AddsWeaponDamage ? Player.Buffs.WeaponDamageMultiplier : 1.0)
                        * enemyDmgTakenMult;
                    var (abilityDmg, abilityCrit) = RollPlayerCrit(
                        CombatEngine.CalculateAbilityDamage(ability, Player.Stats, _rng, abilityMult),
                        Player.GetAbilityCritBonus(ability));
                    CurrentEnemy.TakeDamage(abilityDmg);
                    Meter.RecordAbility(ability, CombatActionKind.Damage, abilityDmg);
                    CombatTextRaised?.Invoke(new CombatText(CombatTextTarget.Enemy, CombatTextKind.Ability, abilityDmg, abilityCrit));
                    totalPlayerDmg += abilityDmg;
                    playerAttacked = true;
                    LastLog = $"⚔ {ability.Name} → {abilityDmg} DMG!";

                    if (ability.IsDebuff)   // e.g. Moonfire: direkter Treffer + DoT
                        CurrentEnemy.Debuffs.Apply(Player.GetEffectiveAbility(ability));

                    // Periodic Fire: Fireball-DoT durch Talent
                    double dotDmg = Player.TalentState.GetTotalBonus(Player.ActiveClass, TalentEffectType.FireballDot);
                    if (dotDmg > 0 && ability.Id.StartsWith("Fireball"))
                    {
                        var periodicFire = new Ability
                        {
                            Id                   = "talent_periodic_fire",
                            Name                 = "Periodic Fire",
                            DebuffDuration       = 5,
                            DebuffDotDamage      = (int)Math.Max(1, dotDmg),
                            DebuffDotTickInterval = 5.0,
                        };
                        CurrentEnemy.Debuffs.Apply(periodicFire);
                    }
                }
                else if (ability.IsDebuff)  // reiner Debuff (Hunter's Mark, Curse of Agony)
                {
                    CurrentEnemy.Debuffs.Apply(Player.GetEffectiveAbility(ability));
                    LastLog = $"✦ {ability.Name} on {CurrentEnemy.Name} ({ability.DebuffDuration:0}s)";
                }
                else
                {
                    var (healAmount, healCrit) = RollPlayerCrit(ability.Heal);
                    int hpBefore = Player.CurrentHp;
                    Player.RegenHp(healAmount);
                    int healed = Player.CurrentHp - hpBefore;
                    Meter.RecordAbility(ability, CombatActionKind.Heal, healed);
                    CombatTextRaised?.Invoke(new CombatText(CombatTextTarget.Player, CombatTextKind.Heal, healAmount, healCrit));
                    LastLog = $"✚ {ability.Name} → +{healed} HP";
                }

                Player.Cooldowns.StartCooldown(ability.Id, ability.Cooldown);
                Player.TriggerAbilityUsed(ability.Name, ability.IsDamage ? ability.Damage : ability.Heal);
            }

            // ── Kill-Check nach Abilities ──
            if (!CurrentEnemy.IsAlive)
            {
                HandleEnemyDeath();
                return;
            }

            // ── Log ──
            if (LastLog is null || !(LastLog.StartsWith("Defeated") || LastLog.Contains("defeated")))
            {
                if (playerAttacked && enemyAttacked)
                    LastLog = $"Hit {CurrentEnemy.Name} for {totalPlayerDmg}, took {totalEnemyDmg}.";
                else if (playerAttacked)
                    LastLog = $"Hit {CurrentEnemy.Name} for {totalPlayerDmg}.";
                else if (enemyAttacked)
                    LastLog = $"Took {totalEnemyDmg} from {CurrentEnemy.Name}.";
            }
        }

        private void HandleEnemyDeath()
        {
            var deadEnemy = CurrentEnemy!;
            Player.Copper += deadEnemy.Stats.CopperReward;

            // Rare-Loot: Item oder doppeltes Gold
            if (deadEnemy.IsRare)
            {
                var lootTable = RareLootRegistry.GetLootTable(deadEnemy.Stats.Level);
                if (lootTable.Count > 0)
                {
                    var droppedItem = lootTable[_rng.Next(lootTable.Count)];
                    if (AutoEquipDrops)
                    {
                        // Debug-Simulator: Items werden automatisch angelegt wenn sie ein Upgrade sind.
                        bool equipped = Player.EquipArmorIfUpgrade(droppedItem, out var displaced);
                        if (!equipped)
                            Player.Inventory.TryAdd(droppedItem);
                        else if (displaced is not null)
                            Player.Inventory.TryAdd(displaced);
                    }
                    else
                    {
                        // Normales Spiel: kein Auto-Equip – das Item liegt erst einmal im Inventar
                        // und wird vom Spieler selbst angelegt.
                        Player.Inventory.TryAdd(droppedItem);
                    }
                    ItemDropped?.Invoke(droppedItem);
                }
                else
                {
                    // Kein Loottable → doppeltes Gold statt Item
                    Player.Copper += deadEnemy.Stats.CopperReward;
                }
            }

            int xpReward = StaticHelpers.GetEnemyXpReward(deadEnemy.Stats.Level);
            int oldLevel = Player.Level;
            int levelsGained = Player.GainXp(xpReward);

            if (levelsGained > 0)
            {
                for (int l = oldLevel + 1; l <= Player.Level; l++)
                {
                    foreach (var ability in ClassAbilityRegistry.GetAbilitiesFor(Player.ActiveClass)
                                 .Where(a => a.LevelRequired == l))
                        AbilityUnlocked?.Invoke(ability);
                }
                LastLog = $"⬆ {Player.ActiveClass} → Lv. {Player.Level}";
            }
            else
                LastLog = $"💀 {deadEnemy.Name} defeated (+{xpReward} XP)";

            TotalKills++;
            CurrentEnemy = null;
            _playerAttackAcc = 0;
            _enemyAttackAcc  = 0;
        }

    // ─── Debug-Helfer ───

    /// <summary>
    /// Legt ein Rüstungsteil direkt am aktiven Charakter an und feuert das ItemDropped-Event (Debug-Seiten).
    /// </summary>
    public void DebugGiveArmor(ArmorItem item)
    {
        lock (_simLock)
        {
            Player.ForceEquipArmor(item);
        }
        ItemDropped?.Invoke(item);
        TickUpdated?.Invoke();
    }

    // ─── Talente ───

    /// <summary>Investiert einen Talent-Punkt für den aktiven Charakter. Gibt true zurück wenn erfolgreich.</summary>
    public bool InvestTalentPoint(string talentId)
    {
        var talent = TalentRegistry.GetTalents(Player.ActiveClass)
            .FirstOrDefault(t => t.Id == talentId);
        if (talent is null) return false;
        bool ok;
        lock (_simLock)
        {
            ok = Player.InvestTalent(talent);
        }
        if (ok) TickUpdated?.Invoke();
        return ok;
    }

    /// <summary>Setzt alle Talente der aktiven Klasse zurück (Respek).</summary>
    public void ResetTalents(CharacterClass cls)
    {
        lock (_simLock)
        {
            Player.ResetTalents(cls);
        }
        TickUpdated?.Invoke();
    }

    // ─── Spielstand-Wiederherstellung ───

    internal void RestoreFromSave(SaveData save)
    {
        bool wasRunning = IsGameRunning;
        Stop();

        lock (_simLock)
        {
            _roster.Clear();
            TotalKills      = save.TotalKills;
            TotalDeaths     = save.TotalDeaths;
            SpeedMultiplier = 1.0;
            _nextSlotId     = save.NextSlotId;

            foreach (var slotData in save.Roster)
                _roster.Add(RestoreSlot(slotData));

            _activeSlotId = save.ActiveSlotId;

            var activeSlot = _activeSlotId.HasValue
                ? _roster.FirstOrDefault(s => s.Id == _activeSlotId.Value)
                : null;

            if (activeSlot is not null)
            {
                Faction     = activeSlot.Faction;
                Race        = activeSlot.Race;
                Gender      = activeSlot.Gender;
                PlayerClass = activeSlot.Class;
                Player      = activeSlot.Player;
                CurrentEnemy = _spawner.Spawn(Player.Stats.Level, 0);
            }
        }

        if (wasRunning) Start();
        RosterChanged?.Invoke();
        TickUpdated?.Invoke();
    }

    private CharacterSlot RestoreSlot(SlotSaveData data)
    {
        var stats  = StatsFactory.GetBaseStats(data.Class, data.Race);
        var player = new Player(stats, data.Faction, data.Race, data.Class);

        var p = data.Player;

        player.Copper = p.Copper;

        // ClassProgress wiederherstellen
        foreach (var (clsName, progressData) in p.ClassProgress)
        {
            if (Enum.TryParse<CharacterClass>(clsName, out var cls)
                && player.ClassProgress.TryGetValue(cls, out var progress))
            {
                progress.Restore(progressData.Level, progressData.Xp);
            }
        }

        // Ausgerüstete Waffen pro Klasse wiederherstellen
        var weaponsByClass = new Dictionary<CharacterClass, WeaponItem?>();
        foreach (var (clsName, weaponId) in p.EquippedWeaponIdByClass)
        {
            if (Enum.TryParse<CharacterClass>(clsName, out var cls))
                weaponsByClass[cls] = weaponId.HasValue ? WeaponRegistry.GetById(weaponId.Value) : null;
        }
        player.RestoreEquippedWeapons(weaponsByClass);

        // Getragene Rüstung wiederherstellen
        var armorBySlot = new Dictionary<ArmorSlot, ArmorItem>();
        foreach (var (slotName, itemId) in p.EquippedArmorIdBySlot)
        {
            if (Enum.TryParse<ArmorSlot>(slotName, out var slot)
                && RareLootRegistry.GetById(itemId) is { } armor)
            {
                armorBySlot[slot] = armor;
            }
        }
        if (armorBySlot.Count > 0)
            player.RestoreEquippedArmor(armorBySlot);

        // Talente wiederherstellen (vor SetActiveClass, damit CritBonus in Stats einfließt)
        foreach (var (talentId, ranks) in p.TalentInvestedPoints)
            player.TalentState.RestoreRanks(talentId, ranks);

        // Aktive Klasse setzen (berechnet Stats neu, stellt Waffe wieder her)
        player.SetClass(p.ActiveClass);
        player.SetActiveClass(p.ActiveClass);

        // Loadout der aktiven Klasse in den Charakter laden
        if (p.LoadoutAbilityIdsByClass.TryGetValue(p.ActiveClass.ToString(), out var activeIds))
            player.ApplyLoadout(new ClassLoadout { Class = p.ActiveClass, EquippedAbilityIds = activeIds.ToList() });

        // Rucksack-Inhalt wiederherstellen (Waffen und Rüstungsteile)
        foreach (var itemData in p.BackpackItems)
        {
            Item? item = WeaponRegistry.GetById(itemData.ItemId) ?? (Item?)RareLootRegistry.GetById(itemData.ItemId);
            if (item is not null && itemData.Slot < player.Inventory.Backpack.SlotCount)
                player.Inventory.Backpack.Put(itemData.Slot, new ItemStack(item, Math.Max(1, itemData.Count)));
        }

        return new CharacterSlot
        {
            Id      = data.Id,
            Class   = data.Class,
            Faction = data.Faction,
            Race    = data.Race,
            Gender  = data.Gender,
            Player  = player,
        };
    }
}

