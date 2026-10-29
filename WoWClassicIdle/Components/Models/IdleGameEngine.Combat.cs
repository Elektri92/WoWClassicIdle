using Models;

// Create new systems as their own partial file; keep files below 300 lines where possible.
public partial class IdleGameEngine
{
    private void OnTick(object? _, System.Timers.ElapsedEventArgs __)
    {
        int subSteps = Math.Max(1, (int)Math.Round(SpeedMultiplier));
        RunSteps(subSteps);
        TickUpdated?.Invoke();
    }

    /// <summary>
    /// Runs <paramref name="count"/> simulation steps of <c>intervalMs</c> synchronously –
    /// without a timer, e.g. for headless simulations.
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

    /// <summary>Level, cooldown and (for buffs/debuffs) expiry allow a cast – mana is not checked.</summary>
    private bool IsReadyToCast(Ability ability) =>
        IsAbilityAvailable(ability)
        && Player.Cooldowns.IsReady(ability.Id)
        && !(ability.IsBuff && Player.Buffs.IsActive(ability.Id))
        // An active debuff only blocks pure debuffs – damage spells with a debuff (e.g. Frostbolt) keep being cast and refresh it
        && !(ability.IsDebuff && !ability.IsDamage && CurrentEnemy is { } e && e.Debuffs.IsActive(ability.Id))
        && !(ability.RequiresEnemyBelowHpPct > 0
             && (CurrentEnemy is null || CurrentEnemy.CurrentHp > CurrentEnemy.Stats.MaxHp * ability.RequiresEnemyBelowHpPct));

    /// <summary>
    /// HP below the threshold and an equipped, ready heal only fails because of mana:
    /// then no other abilities are cast until there is enough mana.
    /// </summary>
    private bool IsSavingManaForHeal() =>
        PlayerNeedsHeal
        && Player.EquippedAbilities.Any(a => a.IsHealing && IsReadyToCast(a) && Player.CurrentMana < CastCost(a));

    /// <summary>Simulated play time per step in seconds.</summary>
    public double StepSeconds => _baseDt;

    /// <summary>
    /// Rolls crit for a value caused by the player and doubles it if applicable.
    /// Applies to all damage, every heal and (once present) every shield.
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
        // ── Dead: waiting at the spirit healer ──
        if (RespawnRemaining > 0)
        {
            RespawnRemaining = Math.Max(0, RespawnRemaining - _baseDt);
            return;
        }

        // ── Traveling to another zone: no combat ──
        if (AdvanceTravel(_baseDt))
            return;

        if (CurrentEnemy is not { IsAlive: true })
        {
            if (_pendingRareEnemy != null)
            {
                // Player died against this rare – fight the rare again with its remaining HP
                CurrentEnemy    = _pendingRareEnemy;
                _pendingRareEnemy = null;
            }
            else if (SpawnEnemy() is { } spawned)
            {
                CurrentEnemy = spawned;
            }
            else
            {
                return; // no zone chosen yet
            }
            LastLog = $"{CurrentEnemy.Name} appeared!";
            _playerAttackAcc = 0;
            _enemyAttackAcc  = 0;
            return;
        }

        double dt = _baseDt;
        Meter.AddCombatTime(dt);
        MaintainPassiveAuras();
        UpdateConsumables(dt);

        // Dynamite can already kill the enemy
        if (!CurrentEnemy.IsAlive)
        {
            HandleEnemyDeath();
            return;
        }

        int totalPlayerDmg = 0;
        int totalEnemyDmg  = 0;
        bool playerAttacked = false;
        bool enemyAttacked  = false;

        // ── Player HoT / MoT ticks ──
        foreach (var tick in Player.Buffs.Tick(dt))
        {
            if (tick.IsMana)
            {
                // Mana over time (e.g. Conjure Water) – no crit, no combat text
                Player.RegenMana(tick.Amount);
            }
            else
            {
                int scaledHot = (int)Math.Round(tick.Amount * HealingMultiplier);
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
            TryNightfallProc();

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

        // ── Energy (Rogue, Cat Form): regeneration every 3 seconds ──
        var resource = Player.Resource;
        if (resource == ResourceKind.Energy)
        {
            _rogueManaAcc += dt;
            while (_rogueManaAcc >= 3.0)
            {
                _rogueManaAcc -= 3.0;
                Player.RegenMana((int)Math.Round(20 * ResourceRegenMultiplier));
            }
        }
        else if (resource == ResourceKind.Mana)
        {
            _manaRegenAcc += dt;
            while (_manaRegenAcc >= 5.0)
            {
                _manaRegenAcc -= 5.0;
                Player.RegenMana((int)Math.Round(Player.Stats.ManaRegenPer5 * ResourceRegenMultiplier));
            }
        }

        // ── Passive HP regeneration from Spirit every 5 seconds (all classes) ──
        _hpRegenAcc += dt;
        while (_hpRegenAcc >= 5.0)
        {
            _hpRegenAcc -= 5.0;
            int hpBefore = Player.CurrentHp;
            Player.RegenHp(Player.Stats.HpRegenPer5);
            int regenHealed = Player.CurrentHp - hpBefore;
            // Only HP actually healed counts – no meter entry at full health
            if (regenHealed > 0)
                Meter.RecordSpiritRegen(regenHealed);
        }

        // Attack speed from character stat and buffs – speeds up auto-attacks and ability cooldowns
        double attackSpeed    = Player.Stats.AttackSpeedMultiplier * Player.Buffs.AttackSpeedMultiplier * FlurryAttackSpeedMultiplier;
        double playerInterval = Math.Max(0.1, Player.Stats.Speed / attackSpeed);
        double enemyInterval  = Math.Max(0.1, CurrentEnemy.Stats.Speed / CurrentEnemy.Debuffs.AttackSpeedMultiplier);
        double enemyDmgTakenMult = CurrentEnemy.Debuffs.DamageTakenMultiplier;

        _playerAttackAcc += dt;
        _enemyAttackAcc  += dt;
        SwingInterval = playerInterval;
        EnemySwingInterval = enemyInterval;

        // ── Abilities (cooldown-based, with cast time and global cooldown) + pet attacks ──
        Player.Cooldowns.Tick(dt * attackSpeed);
        int abilityDmg = UpdateAbilities(dt, attackSpeed) + UpdatePet(dt, enemyDmgTakenMult);
        if (abilityDmg > 0)
        {
            totalPlayerDmg += abilityDmg;
            playerAttacked = true;
        }

        // ── Kill check after abilities (spells are evaluated before the auto-attack) ──
        if (!CurrentEnemy.IsAlive)
        {
            HandleEnemyDeath();
            return;
        }

        // ── Spielerangriffe ──
        var nextSwingRaw = Player.EquippedAbilities
            .FirstOrDefault(a => a.ReplacesAutoAttack && IsAbilityAvailable(a) && Player.CanUseInCurrentForm(a));
        var nextSwingAbility = nextSwingRaw is null ? null : Player.GetEffectiveAbility(nextSwingRaw);

        // A spell with mana cost is being cast → the swing waits (timer stays fully charged)
        if (IsSwingHeld)
            _playerAttackAcc = Math.Min(_playerAttackAcc, playerInterval);

        while (!IsSwingHeld && _playerAttackAcc >= playerInterval && CurrentEnemy.IsAlive)
        {
            _playerAttackAcc -= playerInterval;
            playerAttacked = true;

            // On-next-swing (Heroic Strike): replaces the swing, generates no rage like in Classic
            if (nextSwingAbility is not null
                && Player.Cooldowns.IsReady(nextSwingAbility.Id)   // Holy Strike: 10 s CD; Heroic Strike/Maul have none
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

                // Passive procs (e.g. Deadly Poison) – also on on-next-swing
                foreach (var passive in Player.PassiveAbilities)
                    TryProcPassive(passive);

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

            // Rage (Warrior, Bear Form): +8 rage per auto-attack (can be increased by talents and stances)
            if (resource == ResourceKind.Rage)
            {
                double rageMult = 1.0
                    + Player.TalentState.GetTotalBonus(Player.ActiveClass, TalentEffectType.RageGenBonus)
                    + Player.Buffs.RageGenBonus;
                Player.RegenMana((int)Math.Round(8 * rageMult));
            }

            // Passive Procs (z. B. Deadly Poison)
            foreach (var passive in Player.PassiveAbilities)
                TryProcPassive(passive);
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

            // Stun: enemy cannot attack
            if (CurrentEnemy.Debuffs.IsStunned)
                continue;

            enemyAttacked = true;

            // Immunity (e.g. Divine Shield): the hit doesn't land
            if (Player.Buffs.HasImmunity)
                continue;

            int enemyDmg = CombatEngine.CalculateDamage(
                CurrentEnemy.Stats.Attack, Player.Stats.Defense, _rng);
            double dmgReduction = Math.Min(0.99, Player.Stats.DamageTakenReduction + Player.Buffs.DamageTakenReduction);
            int reducedDmg = Math.Max(1, (int)Math.Round(enemyDmg * (1.0 - dmgReduction)));
            reducedDmg = SplitDamageWithPet(reducedDmg);   // Pet absorbs part of the hit
            var (taken, absorbs) = Player.TakeDamage(reducedDmg);
            CombatTextRaised?.Invoke(new CombatText(CombatTextTarget.Player, CombatTextKind.AutoAttack, taken));
            if (!Player.IsAlive)
                TryCheatDeath();   // Spirit of Redemption
            else
                ApplyHealOnHit();  // Earth Shield
            foreach (var (src, amt) in absorbs)
                Meter.RecordAbility(src, CombatActionKind.Heal, amt);
            totalEnemyDmg += reducedDmg;

            // RageOnHit: Warrior generates extra rage when taking damage
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

            // Charge buffs (Holy Shield) consume one charge per hit taken
            Player.Buffs.ConsumeHitCharges();
        }

        // ── Kill check after retaliation damage – otherwise the kill (XP, money, loot) would be lost ──
        // (If both die in the same step, the player's death counts)
        if (!CurrentEnemy.IsAlive && Player.IsAlive)
        {
            HandleEnemyDeath();
            return;
        }

        if (!Player.IsAlive)
        {
            TotalDeaths++;
            Player.Buffs.Reset();   // like in WoW: death removes all buffs
            DismissPet();
            Player.CurrentHp = Player.Stats.MaxHp;
            // Remember the rare enemy so it reappears after respawn (only if enabled)
            if (EnableRareRequeue && CurrentEnemy is { IsRare: true })
                _pendingRareEnemy = CurrentEnemy;
            CurrentEnemy = null;
            RespawnDuration  = 10 + Player.Level;
            RespawnRemaining = RespawnDuration;
            LastLog = "Defeated! Resurrecting at the Spirit Healer…";
            HandleConsumablesOnDeath();
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
    /// Automatically sells an armor or weapon item if an exact duplicate
    /// (same ID) is already in the inventory or equipped.
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
            // Track loot quality (only in normal mode, not in the debug simulator)
            if (!AutoEquipDrops)
            {
                if (item.Quality == ItemQuality.Uncommon) TotalUncommonLooted++;
                else if (item.Quality == ItemQuality.Rare) TotalRareItemsLooted++;
            }
            Player.Inventory.TryAdd(item);
            ItemDropped?.Invoke(item);
        }
    }

    /// <summary>Drop a weapon: equip it as an upgrade in AutoEquip mode, otherwise put it into the inventory normally.</summary>
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

    /// <summary>Credit XP and report unlocked abilities; return value = levels gained.</summary>
    private int GrantXp(int amount)
    {
        int oldLevel = Player.Level;
        int levelsGained = Player.GainXp(amount);
        for (int l = oldLevel + 1; l <= Player.Level; l++)
        {
            // Talent abilities are reported when skilling (InvestTalentPoint), not on level-up
            foreach (var ability in ClassAbilityRegistry.GetAbilitiesFor(Player.ActiveClass)
                         .Where(a => a.LevelRequired == l && !a.TalentUnlockOnly))
                AbilityUnlocked?.Invoke(ability);
        }
        return levelsGained;
    }

    private void HandleEnemyDeath()
    {
        var deadEnemy = CurrentEnemy!;
        Player.Copper += deadEnemy.Stats.CopperReward;
        RollCombatTradeDrops(deadEnemy);

        // Rare loot: armor + 25 % chance for a class weapon
        if (deadEnemy.IsRare)
        {
            var rawTable  = RareLootRegistry.GetLootTable(deadEnemy.Stats.Level);
            // In the simulator only drop class-appropriate items
            IReadOnlyList<ArmorItem> lootTable = AutoEquipDrops
                ? rawTable.Where(i => RareLootRegistry.IsUsefulForClass(i.Bonuses, Player.ActiveClass)).ToList()
                : rawTable;

            if (lootTable.Count > 0)
            {
                var droppedItem = lootTable[_rng.Next(lootTable.Count)] with { RequiredLevel = deadEnemy.Stats.Level };
                if (AutoEquipDrops)
                {
                    // Debug simulator: items are equipped automatically if they are an upgrade.
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
                // No loot table → double gold instead of an item
                Player.Copper += deadEnemy.Stats.CopperReward;
            }

            // 25 % chance for a green class weapon
            if (_rng.NextDouble() < 0.25)
            {
                var classWeapon = WeaponRegistry.GetTierWeaponForLevel(Player.ActiveClass, deadEnemy.Stats.Level);
                if (classWeapon is not null)
                    AddWeaponLootOrSell(classWeapon);
            }

            // Blue drops only in the normal game – no blue loot is rolled in the simulator
            if (!AutoEquipDrops)
            {
                // 15 % chance for blue armor (from level 11)
                if (_rng.NextDouble() < 0.15)
                {
                    var blueTable = RareLootRegistry.GetBlueLootTable(deadEnemy.Stats.Level);
                    if (blueTable.Count > 0)
                    {
                        var blueArmor = blueTable[_rng.Next(blueTable.Count)] with { RequiredLevel = deadEnemy.Stats.Level };
                        AddLootOrSell(blueArmor);
                    }
                }

                // 10 % chance for a blue class weapon (from level 10)
                if (_rng.NextDouble() < 0.10)
                {
                    var blueWeapon = WeaponRegistry.GetBlueTierWeaponForLevel(Player.ActiveClass, deadEnemy.Stats.Level);
                    if (blueWeapon is not null)
                        AddWeaponLootOrSell(blueWeapon);
                }
            }
        }

        // Enemies that are too low (3+ levels below the player) give heavily reduced or no XP
        int xpReward = StaticHelpers.GetKillXp(Player.Level, deadEnemy.Stats.Level);

        if (GrantXp(xpReward) > 0)
            LastLog = $"⬆ {Player.ActiveClass} → Lv. {Player.Level}";
        else
            LastLog = xpReward > 0
                ? $"💀 {deadEnemy.Name} defeated (+{xpReward} XP)"
                : $"💀 {deadEnemy.Name} defeated (no XP – too low level)";

        // Count zone objectives (overwrites LastLog when an objective was completed)
        TrackZoneObjectives(deadEnemy);

        TotalKills++;
        if (deadEnemy.IsRare) TotalRareKills++;
        CurrentEnemy = null;
        CancelCast();   // Target dead → the running cast is discarded
        _playerAttackAcc = 0;
        _enemyAttackAcc  = 0;
    }
}
