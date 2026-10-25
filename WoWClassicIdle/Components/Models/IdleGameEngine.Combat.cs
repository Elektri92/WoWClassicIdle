using Models;

// Neue Systeme als eigene Partial-Datei anlegen; Dateien möglichst < 300 Zeilen halten.
public partial class IdleGameEngine
{
    private void OnTick(object? _, System.Timers.ElapsedEventArgs __)
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
        // Aktiver Debuff blockiert nur reine Debuffs – Schadenszauber mit Debuff (z. B. Frostbolt) werden weiter gewirkt und frischen ihn auf
        && !(ability.IsDebuff && !ability.IsDamage && CurrentEnemy is { } e && e.Debuffs.IsActive(ability.Id))
        && !(ability.RequiresEnemyBelowHpPct > 0
             && (CurrentEnemy is null || CurrentEnemy.CurrentHp > CurrentEnemy.Stats.MaxHp * ability.RequiresEnemyBelowHpPct));

    /// <summary>
    /// HP unter der Schwelle und eine ausgerüstete, bereite Heilung scheitert nur am Mana:
    /// dann werden keine anderen Fähigkeiten gewirkt, bis genug Mana da ist.
    /// </summary>
    private bool IsSavingManaForHeal() =>
        PlayerNeedsHeal
        && Player.EquippedAbilities.Any(a => a.IsHealing && IsReadyToCast(a) && Player.CurrentMana < Player.GetEffectiveManaCost(a));

    /// <summary>Simulierte Spielzeit pro Schritt in Sekunden.</summary>
    public double StepSeconds => _baseDt;

    /// <summary>
    /// Würfelt Crit für einen vom Spieler verursachten Wert und verdoppelt ihn gegebenenfalls.
    /// Gilt für allen Schaden, jede Heilung und (sobald vorhanden) jeden Schild.
    /// </summary>
    private (int Amount, bool IsCrit) RollPlayerCrit(int amount, double extraCrit = 0)
    {
        bool isCrit = _rng.NextDouble() < (Player.Stats.CritChance + extraCrit + Player.Buffs.CritChanceBonus);
        return (CombatEngine.ApplyCrit(amount, isCrit), isCrit);
    }

    private void ApplyCritHeal(bool isCrit)
    {
        if (!isCrit) return;
        double critHeal = Player.TalentState.GetTotalBonus(Player.ActiveClass, TalentEffectType.CritHeal);
        if (critHeal > 0)
            Player.RegenHp((int)Math.Round(critHeal));
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
            if (_pendingRareEnemy != null)
            {
                // Spieler ist gegen diesen Rare gestorben – Rare mit verbleibendem HP erneut bekämpfen
                CurrentEnemy    = _pendingRareEnemy;
                _pendingRareEnemy = null;
            }
            else
            {
                CurrentEnemy = _spawner.Spawn(Player.Stats.Level, _stopwatch.Elapsed.TotalSeconds);
            }
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
                int scaledHot = (int)Math.Round(tick.Amount * Player.Stats.HealingMultiplier);
                var (hotHeal, hotCrit) = RollPlayerCrit(scaledHot);
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
            Meter.RecordAbility(dot.Source, CombatActionKind.Damage, CurrentEnemy.TakeDamage(dotDmg));
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

        // ── Energie (Rogue, Cat Form): Regeneration alle 3 Sekunden ──
        var resource = Player.Resource;
        if (resource == ResourceKind.Energy)
        {
            _rogueManaAcc += dt;
            while (_rogueManaAcc >= 3.0)
            {
                _rogueManaAcc -= 3.0;
                Player.RegenMana(20);
            }
        }
        else if (resource == ResourceKind.Mana)
        {
            _manaRegenAcc += dt;
            while (_manaRegenAcc >= 5.0)
            {
                _manaRegenAcc -= 5.0;
                Player.RegenMana(Player.Stats.ManaRegenPer5);
            }
        }

        // ── Passive HP-Regeneration aus Spirit alle 5 Sekunden (alle Klassen) ──
        _hpRegenAcc += dt;
        while (_hpRegenAcc >= 5.0)
        {
            _hpRegenAcc -= 5.0;
            int hpBefore = Player.CurrentHp;
            Player.RegenHp(Player.Stats.HpRegenPer5);
            int regenHealed = Player.CurrentHp - hpBefore;
            // Nur tatsächlich geheilte HP zählen – bei vollem Leben kein Meter-Eintrag
            if (regenHealed > 0)
                Meter.RecordSpiritRegen(regenHealed);
        }

        // Attack-Speed aus Charakter-Stat und Buffs – beschleunigt Auto-Attacks und Fähigkeiten-Cooldowns
        double attackSpeed    = Player.Stats.AttackSpeedMultiplier * Player.Buffs.AttackSpeedMultiplier * FlurryAttackSpeedMultiplier;
        double playerInterval = Math.Max(0.1, Player.Stats.Speed / attackSpeed);
        double enemyInterval  = Math.Max(0.1, CurrentEnemy.Stats.Speed / CurrentEnemy.Debuffs.AttackSpeedMultiplier);
        double enemyDmgTakenMult = CurrentEnemy.Debuffs.DamageTakenMultiplier;

        _playerAttackAcc += dt;
        _enemyAttackAcc  += dt;
        SwingInterval = playerInterval;

        // ── Abilities (Cooldown-basiert, mit Cast-Zeit und Global Cooldown) + Pet-Angriffe ──
        Player.Cooldowns.Tick(dt * attackSpeed);
        int abilityDmg = UpdateAbilities(dt, attackSpeed) + UpdatePet(dt, enemyDmgTakenMult);
        if (abilityDmg > 0)
        {
            totalPlayerDmg += abilityDmg;
            playerAttacked = true;
        }

        // ── Kill-Check nach Abilities (Spells werden vor der Auto-Attack ausgewertet) ──
        if (!CurrentEnemy.IsAlive)
        {
            HandleEnemyDeath();
            return;
        }

        // ── Spielerangriffe ──
        var nextSwingRaw = Player.EquippedAbilities
            .FirstOrDefault(a => a.ReplacesAutoAttack && Player.Level >= a.LevelRequired && Player.CanUseInCurrentForm(a));
        var nextSwingAbility = nextSwingRaw is null ? null : Player.GetEffectiveAbility(nextSwingRaw);

        // Spell mit Manakosten wird gerade gewirkt → Swing wartet (Timer bleibt voll geladen)
        if (IsSwingHeld)
            _playerAttackAcc = Math.Min(_playerAttackAcc, playerInterval);

        while (!IsSwingHeld && _playerAttackAcc >= playerInterval && CurrentEnemy.IsAlive)
        {
            _playerAttackAcc -= playerInterval;
            playerAttacked = true;

            // On-next-swing (Heroic Strike): ersetzt den Schlag, erzeugt wie in Classic keine Rage
            if (nextSwingAbility is not null
                && Player.Cooldowns.IsReady(nextSwingAbility.Id)   // Holy Strike: 10 s CD; Heroic Strike/Maul haben keinen
                && !IsSavingManaForHeal()
                && Player.CurrentMana >= nextSwingAbility.MinResourceToCast
                && Player.ConsumeMana(nextSwingAbility.ManaCost))
            {
                double swingMult = Player.Buffs.DamageMultiplier * Player.Buffs.WeaponDamageMultiplier * enemyDmgTakenMult;
                var (swingDmg, swingCrit) = RollPlayerCrit(
                    CombatEngine.CalculateAbilityDamage(nextSwingAbility, Player.Stats, _rng, swingMult));
                Meter.RecordAbility(nextSwingAbility, CombatActionKind.Damage, CurrentEnemy.TakeDamage(swingDmg));
                CombatTextRaised?.Invoke(new CombatText(CombatTextTarget.Enemy, CombatTextKind.Ability, swingDmg, swingCrit));
                totalPlayerDmg += swingDmg;
                LastLog = $"⚔ {nextSwingAbility.Name} → {swingDmg} DMG!";
                Player.TriggerAbilityUsed(nextSwingAbility.Name, nextSwingAbility.Damage);
                AbilityTriggered?.Invoke(nextSwingAbility);
                if (nextSwingAbility.Cooldown > 0)
                    Player.Cooldowns.StartCooldown(nextSwingAbility.Id, nextSwingAbility.Cooldown);
                UpdateFlurry(swingCrit);

                // Passive Procs (z. B. Deadly Poison) – auch bei On-Next-Swing
                foreach (var passive in Player.PassiveAbilities)
                    if (CurrentEnemy.IsAlive && _rng.NextDouble() < passive.ProcChance)
                        CurrentEnemy.Debuffs.ApplyStack(passive);

                continue;
            }

            double autoMult = Player.Buffs.DamageMultiplier * Player.Buffs.WeaponDamageMultiplier * enemyDmgTakenMult;
            double autoAttackCritBonus = Player.TalentState.GetTotalBonus(Player.ActiveClass, TalentEffectType.AutoAttackCrit);
            var (playerDmg, autoCrit) = RollPlayerCrit(
                CombatEngine.CalculateAutoAttack(Player.Stats, _rng, autoMult), autoAttackCritBonus);
            Meter.RecordAutoAttack(CurrentEnemy.TakeDamage(playerDmg), Player.EquippedWeapon?.IconUrl);
            CombatTextRaised?.Invoke(new CombatText(CombatTextTarget.Enemy, CombatTextKind.AutoAttack, playerDmg, autoCrit));
            totalPlayerDmg += playerDmg;

            ApplyCritHeal(autoCrit);
            UpdateFlurry(autoCrit);
            totalPlayerDmg += TryWindfuryAttacks(autoMult);

            // Rage (Warrior, Bear Form): +8 Rage pro Auto-Attack (durch Talente und Stances erhöhbar)
            if (resource == ResourceKind.Rage)
            {
                double rageMult = 1.0
                    + Player.TalentState.GetTotalBonus(Player.ActiveClass, TalentEffectType.RageGenBonus)
                    + Player.Buffs.RageGenBonus;
                Player.RegenMana((int)Math.Round(8 * rageMult));
            }

            // Passive Procs (z. B. Deadly Poison)
            foreach (var passive in Player.PassiveAbilities)
                if (CurrentEnemy.IsAlive && _rng.NextDouble() < passive.ProcChance)
                    CurrentEnemy.Debuffs.ApplyStack(passive);
        }

        if (!CurrentEnemy.IsAlive)
        {
            HandleEnemyDeath();
            return;
        }

        // ── Gegnerangriffe ──
        while (_enemyAttackAcc >= enemyInterval && Player.IsAlive && CurrentEnemy.IsAlive)
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
            double dmgReduction = Math.Min(0.99, Player.Stats.DamageTakenReduction + Player.Buffs.DamageTakenReduction);
            int reducedDmg = Math.Max(1, (int)Math.Round(enemyDmg * (1.0 - dmgReduction)));
            reducedDmg = SplitDamageWithPet(reducedDmg);   // Pet fängt einen Teil des Treffers ab
            var (taken, absorbs) = Player.TakeDamage(reducedDmg);
            CombatTextRaised?.Invoke(new CombatText(CombatTextTarget.Player, CombatTextKind.AutoAttack, taken));
            foreach (var (src, amt) in absorbs)
                Meter.RecordAbility(src, CombatActionKind.Heal, amt);
            totalEnemyDmg += reducedDmg;

            // RageOnHit: Warrior generiert bei Schaden-Einnahme extra Rage
            if (resource == ResourceKind.Rage)
            {
                double rageOnHit = Player.TalentState.GetTotalBonus(Player.ActiveClass, TalentEffectType.RageOnHit);
                if (rageOnHit > 0)
                    Player.RegenMana((int)Math.Round(rageOnHit));
            }

            // Vergeltungsschaden (Thorns / Lightning Shield)
            foreach (var retAbility in Player.Buffs.GetRetaliationSources())
            {
                if (!CurrentEnemy.IsAlive) break;
                int retDmg = retAbility.BuffRetaliationDamage;
                Meter.RecordAbility(retAbility, CombatActionKind.Damage, CurrentEnemy.TakeDamage(retDmg));
                CombatTextRaised?.Invoke(new CombatText(CombatTextTarget.Enemy, CombatTextKind.Ability, retDmg, false));
                totalPlayerDmg += retDmg;
                playerAttacked = true;
            }

            // Ladungs-Buffs (Holy Shield) verbrauchen pro erlittenem Treffer eine Ladung
            Player.Buffs.ConsumeHitCharges();
        }

        // ── Kill-Check nach Vergeltungsschaden – sonst ginge der Kill (XP, Geld, Loot) verloren ──
        // (Sterben beide im selben Schritt, gilt der Tod des Spielers)
        if (!CurrentEnemy.IsAlive && Player.IsAlive)
        {
            HandleEnemyDeath();
            return;
        }

        if (!Player.IsAlive)
        {
            TotalDeaths++;
            Player.Buffs.Reset();   // wie in WoW: Tod entfernt alle Buffs
            DismissPet();
            Player.CurrentHp = Player.Stats.MaxHp;
            // Rare-Gegner merken, damit er nach Respawn erneut erscheint (nur wenn aktiviert)
            if (EnableRareRequeue && CurrentEnemy is { IsRare: true })
                _pendingRareEnemy = CurrentEnemy;
            CurrentEnemy = null;
            RespawnDuration  = 10 + Player.Level;
            RespawnRemaining = RespawnDuration;
            LastLog = "Defeated! Resurrecting at the Spirit Healer…";
            ResetCastState();
            _playerAttackAcc = 0;
            _enemyAttackAcc  = 0;
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

    /// <summary>
    /// Verkauft ein Rüstungs- oder Waffenitem automatisch, wenn ein exaktes Duplikat
    /// (gleiche ID) bereits im Inventar liegt oder angelegt ist.
    /// </summary>
    private bool TryAutoSellIfDuplicate(Item item)
    {
        if (item is not ArmorItem and not WeaponItem) return false;

        bool hasDuplicate =
            Player.Inventory.CountOf(item.Id) > 0
            || (item is ArmorItem ai && Player.EquippedArmor.Values.Any(e => e.Id == ai.Id))
            || (item is WeaponItem w  && Player.EquippedWeapon?.Id == w.Id);

        if (!hasDuplicate) return false;

        Player.Copper += Math.Max(1, item.SellPrice);
        TotalItemsSold++;
        LastLog = $"Auto-sold duplicate: {item.Name} for {Money.Format(Math.Max(1, item.SellPrice))}";
        return true;
    }

    private void AddLootOrSell(Item item)
    {
        if (!TryAutoSellIfDuplicate(item))
        {
            // Loot-Qualität verfolgen (nur im Normalbetrieb, nicht im Debug-Simulator)
            if (!AutoEquipDrops)
            {
                if (item.Quality == ItemQuality.Uncommon) TotalUncommonLooted++;
                else if (item.Quality == ItemQuality.Rare) TotalRareItemsLooted++;
            }
            Player.Inventory.TryAdd(item);
            ItemDropped?.Invoke(item);
        }
    }

    /// <summary>Waffe droppen: im AutoEquip-Modus als Upgrade anlegen, sonst normal ins Inventar.</summary>
    private void AddWeaponLootOrSell(WeaponItem weapon)
    {
        if (AutoEquipDrops)
        {
            bool equipped = Player.EquipWeaponIfUpgrade(weapon, out var displaced);
            if (!equipped)
                Player.Inventory.TryAdd(weapon);
            else if (displaced is not null)
                Player.Inventory.TryAdd(displaced);
            ItemDropped?.Invoke(weapon);
        }
        else
        {
            AddLootOrSell(weapon);
        }
    }

    private void HandleEnemyDeath()
    {
        var deadEnemy = CurrentEnemy!;
        Player.Copper += deadEnemy.Stats.CopperReward;

        // Rare-Loot: Rüstung + 25 % Chance auf Klassenwaffe
        if (deadEnemy.IsRare)
        {
            var rawTable  = RareLootRegistry.GetLootTable(deadEnemy.Stats.Level);
            // Im Simulator nur klassenpassende Items droppen lassen
            IReadOnlyList<ArmorItem> lootTable = AutoEquipDrops
                ? rawTable.Where(i => RareLootRegistry.IsUsefulForClass(i.Bonuses, Player.ActiveClass)).ToList()
                : rawTable;

            if (lootTable.Count > 0)
            {
                var droppedItem = lootTable[_rng.Next(lootTable.Count)] with { RequiredLevel = deadEnemy.Stats.Level };
                if (AutoEquipDrops)
                {
                    // Debug-Simulator: Items werden automatisch angelegt wenn sie ein Upgrade sind.
                    bool equipped = Player.EquipArmorIfUpgrade(droppedItem, out var displaced);
                    if (!equipped)
                        Player.Inventory.TryAdd(droppedItem);
                    else if (displaced is not null)
                        Player.Inventory.TryAdd(displaced);
                    ItemDropped?.Invoke(droppedItem);
                }
                else
                {
                    AddLootOrSell(droppedItem);
                }
            }
            else
            {
                // Kein Loottable → doppeltes Gold statt Item
                Player.Copper += deadEnemy.Stats.CopperReward;
            }

            // 25 % Chance auf grüne Klassenwaffe
            if (_rng.NextDouble() < 0.25)
            {
                var classWeapon = WeaponRegistry.GetTierWeaponForLevel(Player.ActiveClass, deadEnemy.Stats.Level);
                if (classWeapon is not null)
                    AddWeaponLootOrSell(classWeapon);
            }

            // Blaue Drops nur im normalen Spiel – im Simulator wird kein blaues Loot gerollt
            if (!AutoEquipDrops)
            {
                // 15 % Chance auf blaue Rüstung (ab Level 11)
                if (_rng.NextDouble() < 0.15)
                {
                    var blueTable = RareLootRegistry.GetBlueLootTable(deadEnemy.Stats.Level);
                    if (blueTable.Count > 0)
                    {
                        var blueArmor = blueTable[_rng.Next(blueTable.Count)] with { RequiredLevel = deadEnemy.Stats.Level };
                        AddLootOrSell(blueArmor);
                    }
                }

                // 10 % Chance auf blaue Klassenwaffe (ab Level 10)
                if (_rng.NextDouble() < 0.10)
                {
                    var blueWeapon = WeaponRegistry.GetBlueTierWeaponForLevel(Player.ActiveClass, deadEnemy.Stats.Level);
                    if (blueWeapon is not null)
                        AddWeaponLootOrSell(blueWeapon);
                }
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
        if (deadEnemy.IsRare) TotalRareKills++;
        CurrentEnemy = null;
        CancelCast();   // Ziel tot → laufender Cast verfällt
        _playerAttackAcc = 0;
        _enemyAttackAcc  = 0;
    }
}
