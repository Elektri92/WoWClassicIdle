// Models/Player.cs
namespace Models
{
    public class Player : Character
    {
        public Faction Faction { get; private set; }
        public Race Race { get; private set; }

        /// <summary>Die aktuell "ausgestattete" Klasse – XP fließt hier hinein.</summary>
        public CharacterClass ActiveClass { get; private set; }

        /// <summary>Progression aller erlaubten Klassen dieser Rasse.</summary>
        public Dictionary<CharacterClass, ClassProgress> ClassProgress { get; }

        /// <summary>Gesamtes Geld in Kupfer (Anzeige als Gold/Silber/Kupfer über <see cref="Money"/>).</summary>
        public long Copper { get; set; }

        /// <summary>Rucksack + 4 Taschenplätze.</summary>
        public Inventory Inventory { get; } = new();

        /// <summary>Talent-Punkte und investierte Ränge je Klasse.</summary>
        public TalentState TalentState { get; } = new();

        /// <summary>Currently equipped weapon. Auto-set to class starter on creation.</summary>
        public WeaponItem? EquippedWeapon { get; private set; }

        /// <summary>Ausgerüstete Rüstungsteile je Slot.</summary>
        public Dictionary<ArmorSlot, ArmorItem> EquippedArmor { get; } = new();

        // Equipped weapon saved per class so switching back restores the right weapon.
        private readonly Dictionary<CharacterClass, WeaponItem?> _equippedByClass = new();

        internal IReadOnlyDictionary<CharacterClass, WeaponItem?> EquippedWeaponsByClass => _equippedByClass;

        internal void RestoreEquippedWeapons(Dictionary<CharacterClass, WeaponItem?> weapons)
        {
            _equippedByClass.Clear();
            foreach (var (cls, weapon) in weapons)
                _equippedByClass[cls] = weapon;
            EquippedWeapon = _equippedByClass.TryGetValue(ActiveClass, out var w) ? w : null;
        }

        // ─── Convenience-Properties (delegieren an aktive Klasse) ───
        public int Level          => ActiveProgress.Level;
        public int Xp             => ActiveProgress.Xp;
        public int XpToNextLevel  => ActiveProgress.XpToNextLevel;

        private ClassProgress ActiveProgress => ClassProgress[ActiveClass];

        public Player(Stats stats, Faction faction, Race race, CharacterClass characterClass)
            : base(stats)
        {
            if (!RaceClassInfo.IsRaceValidForFaction(race, faction))
                throw new ArgumentException($"Race '{race}' does not belong to faction '{faction}'.");

            if (!RaceClassInfo.IsClassAvailableForRace(race, characterClass))
                throw new ArgumentException($"Class '{characterClass}' is not available for race '{race}'.");

            Faction = faction;
            Race = race;
            ActiveClass = characterClass;

            CurrentMana = characterClass == CharacterClass.Warrior ? 0 : stats.MaxMana;

            // Jede für die Rasse erlaubte Klasse bekommt einen Eintrag (Level 1).
            ClassProgress = new Dictionary<CharacterClass, ClassProgress>();
            foreach (var cls in RaceClassInfo.GetAvailableClasses(race))
            {
                ClassProgress[cls] = new ClassProgress(cls);
            }

            EquippedWeapon = WeaponRegistry.GetStarter(characterClass);
            _equippedByClass[characterClass] = EquippedWeapon;
        }

        /// <summary>
        /// XP wird der aktiven Klasse gutgeschrieben.
        /// Bei Level-Up: Stats werden neu berechnet.
        /// </summary>
        public int GainXp(int amount)
        {
            int levelsGained = ActiveProgress.GainXp(amount);

            if (levelsGained > 0)
                RecalculateStats();

            return levelsGained;
        }

        /// <summary>
        /// Death penalty: Die aktive Klasse verliert einen Anteil ihrer aktuellen XP (kein Level-Verlust).
        /// </summary>
        public int LoseXpPercent(double percent) => ActiveProgress.LoseXpPercent(percent);

        /// <summary>
        /// Aktualisiert Rasse/Fraktion und stellt sicher, dass
        /// ClassProgress-Einträge für die neue Rasse existieren.
        /// </summary>
        public void ChangeCharacter(Faction newFaction, Race newRace)
        {
            Faction = newFaction;
            Race = newRace;

            // Neue Klassen der Rasse ergänzen (ohne bestehende zu löschen)
            foreach (var cls in RaceClassInfo.GetAvailableClasses(newRace))
            {
                if (!ClassProgress.ContainsKey(cls))
                    ClassProgress[cls] = new ClassProgress(cls);
            }
        }

        /// <summary>
        /// Wechselt die aktive Klasse (muss für die Rasse verfügbar sein).
        /// Saves the current weapon for the outgoing class and restores the weapon
        /// for the incoming class (or gives the starter weapon the first time).
        /// </summary>
        public void SetActiveClass(CharacterClass newClass)
        {
            if (!RaceClassInfo.IsClassAvailableForRace(Race, newClass))
                throw new ArgumentException($"Class '{newClass}' is not available for race '{Race}'.");

            // Save current weapon under the outgoing class.
            _equippedByClass[ActiveClass] = EquippedWeapon;

            ActiveClass = newClass;

            if (_equippedByClass.TryGetValue(newClass, out var saved))
            {
                EquippedWeapon = saved;
            }
            else
            {
                // First time this class is activated: give the starter weapon.
                EquippedWeapon = WeaponRegistry.GetStarter(newClass);
                _equippedByClass[newClass] = EquippedWeapon;
            }

            RecalculateStats();
        }

        /// <summary>
        /// Equips a weapon from the given bag slot.
        /// The old weapon (if any) is returned to the inventory.
        /// </summary>
        public void EquipWeapon(WeaponItem weapon, Bag sourceBag, int sourceSlot)
        {
            sourceBag.Take(sourceSlot);
            if (EquippedWeapon is not null)
                Inventory.TryAdd(EquippedWeapon);
            EquippedWeapon = weapon;
            _equippedByClass[ActiveClass] = weapon;
            RecalculateStats();
        }

        /// <summary>
        /// Moves the equipped weapon back to the inventory.
        /// Returns false if the inventory is full.
        /// </summary>
        public bool UnequipWeapon()
        {
            if (EquippedWeapon is null) return true;
            if (Inventory.TryAdd(EquippedWeapon) > 0) return false;
            EquippedWeapon = null;
            _equippedByClass[ActiveClass] = null;
            RecalculateStats();
            return true;
        }

        /// <summary>
        /// Legt ein Rüstungsteil an wenn es eine Verbesserung ist (oder der Slot leer war).
        /// Nur für Auto-Equip (Debug-Simulator) – im normalen Spiel wird selbst angelegt.
        /// Gibt true zurück wenn angelegt wurde; das verdrängte Teil kommt in <paramref name="displaced"/>.
        /// </summary>
        public bool EquipArmorIfUpgrade(ArmorItem item, out ArmorItem? displaced)
        {
            EquippedArmor.TryGetValue(item.Slot, out var current);
            if (current is not null && SumPrimary(item.Bonuses) <= SumPrimary(current.Bonuses))
            {
                displaced = null;
                return false;
            }
            displaced = current;
            EquippedArmor[item.Slot] = item;
            RecalculateStats();
            return true;
        }

        /// <summary>
        /// Rüstet ein Rüstungsteil aus dem gegebenen Taschen-Slot aus.
        /// Das bisher getragene Teil (falls vorhanden) wandert zurück ins Inventar.
        /// </summary>
        public void EquipArmor(ArmorItem item, Bag sourceBag, int sourceSlot)
        {
            sourceBag.Take(sourceSlot);
            if (EquippedArmor.TryGetValue(item.Slot, out var current))
                Inventory.TryAdd(current);
            EquippedArmor[item.Slot] = item;
            RecalculateStats();
        }

        /// <summary>
        /// Legt ein getragenes Rüstungsteil ins Inventar zurück.
        /// Gibt false zurück wenn das Inventar voll ist.
        /// </summary>
        public bool UnequipArmor(ArmorSlot slot)
        {
            if (!EquippedArmor.TryGetValue(slot, out var item)) return true;
            if (Inventory.TryAdd(item) > 0) return false;
            EquippedArmor.Remove(slot);
            RecalculateStats();
            return true;
        }

        /// <summary>Stellt getragene Rüstungsteile aus einem Spielstand wieder her.</summary>
        internal void RestoreEquippedArmor(IReadOnlyDictionary<ArmorSlot, ArmorItem> armor)
        {
            foreach (var (slot, item) in armor)
                EquippedArmor[slot] = item;
            RecalculateStats();
        }

        /// <summary>Legt ein Rüstungsteil ohne Upgrade-Prüfung an (z. B. für Debug-Zwecke).</summary>
        public void ForceEquipArmor(ArmorItem item)
        {
            EquippedArmor[item.Slot] = item;
            RecalculateStats();
        }

        private static int SumPrimary(PrimaryStats p) =>
            p.Strength + p.Agility + p.Stamina + p.Intelligence + p.Spirit;

        /// <summary>
        /// No-op — starter weapon assignment is now handled inside SetActiveClass.
        /// Kept so call-sites in IdleGameEngine don't need to be changed.
        /// </summary>
        public void EquipStarterWeapon(CharacterClass cls) { }

            /// <summary>
        /// Verfügbare Talent-Punkte für eine Klasse (1 pro Level ab Level 10, minus investierte).
        /// </summary>
        public int GetAvailableTalentPoints(CharacterClass cls)
        {
            if (!ClassProgress.TryGetValue(cls, out var progress)) return 0;
            int earned = Math.Max(0, progress.Level - 9);
            int spent  = TalentState.TotalInvested(cls);
            return Math.Max(0, earned - spent);
        }

        /// <summary>Investiert einen Talent-Punkt und berechnet Stats neu.</summary>
        public bool InvestTalent(TalentDefinition talent)
        {
            int available = GetAvailableTalentPoints(ActiveClass);
            bool ok = TalentState.Invest(talent, available);
            if (ok) RecalculateStats();
            return ok;
        }

        /// <summary>Setzt alle Talente der Klasse zurück und berechnet Stats neu.</summary>
        public void ResetTalents(CharacterClass cls)
        {
            TalentState.Reset(cls);
            RecalculateStats();
        }

        /// <summary>Stellt Talent-Punkte aus einem Spielstand wieder her.</summary>
        internal void RestoreTalentPoints(Dictionary<string, int> points)
        {
            foreach (var (talentId, ranks) in points)
                TalentState.RestoreRanks(talentId, ranks);
            RecalculateStats();
        }

        /// <summary>
        /// Setzt das Level der aktiven Klasse direkt (z. B. für Debug-Zwecke).
        /// </summary>
        public void SetLevel(int level)
        {
            ActiveProgress.SetLevel(level);
            RecalculateStats();
        }

        /// <summary>Gibt eine talentmodifizierte Kopie der Fähigkeit zurück (inkl. Manakosten).</summary>
        public Ability GetEffectiveAbility(Ability ability)
        {
            if (ability.Id.StartsWith("Arcane_Intellect"))
            {
                double bonus = TalentState.GetTotalBonus(ActiveClass, TalentEffectType.ArcaneIntellectBonus);
                if (bonus > 0)
                    return ability with
                    {
                        BuffIntellectBonus = (int)Math.Round(ability.BuffIntellectBonus * (1.0 + bonus)),
                        BuffSpiritBonus    = (int)Math.Round(ability.BuffSpiritBonus    * (1.0 + bonus)),
                    };
            }
            if (ability.Id.StartsWith("Conjure_Water"))
            {
                double bonus = TalentState.GetTotalBonus(ActiveClass, TalentEffectType.ConjureWaterManaRegen);
                if (bonus > 0)
                    return ability with { MotManaRestorePct = ability.MotManaRestorePct * (1.0 + bonus) };
            }
            if (ability.Id.StartsWith("Fireball"))
            {
                int reduction = (int)TalentState.GetTotalBonus(ActiveClass, TalentEffectType.FireballManaCost);
                if (reduction > 0)
                    return ability with { ManaCost = Math.Max(0, ability.ManaCost - reduction) };
            }
            if (ability.Id.StartsWith("Frostbolt"))
            {
                double speedBonus   = TalentState.GetTotalBonus(ActiveClass, TalentEffectType.FrostboltAttackSpeedDebuff);
                int    costReduction = (int)TalentState.GetTotalBonus(ActiveClass, TalentEffectType.FrostboltManaCost);
                if (speedBonus > 0 || costReduction > 0)
                    return ability with
                    {
                        ManaCost = Math.Max(0, ability.ManaCost - costReduction),
                        DebuffAttackSpeedReduction = ability.DebuffAttackSpeedReduction + speedBonus,
                    };
            }
            return ability;
        }

        /// <summary>Effektive Manakosten einer Fähigkeit nach Talent-Reduktionen.</summary>
        public int GetEffectiveManaCost(Ability ability)
        {
            if (ability.Id.StartsWith("Fireball"))
            {
                int reduction = (int)TalentState.GetTotalBonus(ActiveClass, TalentEffectType.FireballManaCost);
                return Math.Max(0, ability.ManaCost - reduction);
            }
            if (ability.Id.StartsWith("Frostbolt"))
            {
                int reduction = (int)TalentState.GetTotalBonus(ActiveClass, TalentEffectType.FrostboltManaCost);
                return Math.Max(0, ability.ManaCost - reduction);
            }
            return ability.ManaCost;
        }

        /// <summary>Zusätzlicher Crit-Bonus eines bestimmten Zaubers durch Talente.</summary>
        public double GetAbilityCritBonus(Ability ability)
        {
            if (ability.Id.StartsWith("Fireball"))
                return TalentState.GetTotalBonus(ActiveClass, TalentEffectType.FireballCrit);
            if (ability.Id.StartsWith("Frostbolt"))
                return TalentState.GetTotalBonus(ActiveClass, TalentEffectType.FrostboltCrit);
            return 0;
        }

        private void RecalculateStats()
            {
                var old = Stats;
                int level = ActiveProgress.Level;

                var weaponOverride = EquippedWeapon is not null
                    ? new WeaponProfile(EquippedWeapon.Name, EquippedWeapon.MinDamage, EquippedWeapon.MaxDamage, EquippedWeapon.Speed)
                    : null;

                var baseStats = StatsFactory.GetBaseStats(ActiveClass, Race, level, weaponOverride);

                double talentCrit   = TalentState.GetTotalCritBonus(ActiveClass);
                double autoAtkBonus = TalentState.GetTotalBonus(ActiveClass, TalentEffectType.AutoAttackDamage);

                // Rüstungs-Boni aller ausgerüsteten Teile aufaddieren
                var armorBonus = PrimaryStats.Zero;
                foreach (var armor in EquippedArmor.Values)
                    armorBonus = armorBonus + armor.Bonuses;

                var augPrimary = baseStats.Primary + armorBonus;

                Stats = baseStats with
                {
                    CopperReward               = old.CopperReward,
                    XpReward                   = old.XpReward,
                    CritChance                 = baseStats.CritChance + talentCrit,
                    AutoAttackDamageMultiplier = baseStats.AutoAttackDamageMultiplier * (1.0 + autoAtkBonus),
                    Primary                    = augPrimary,
                    MaxHp                      = baseStats.MaxHp   + StatScaling.BonusHp(armorBonus),
                    MaxMana                    = baseStats.MaxMana  + StatScaling.BonusMana(ActiveClass, armorBonus),
                    ManaRegenPer5              = StatScaling.ManaRegenPer5(ActiveClass, augPrimary),
                    AttackPower                = StatScaling.AttackPower(ActiveClass, level, augPrimary),
                };

                CurrentHp = Stats.MaxHp;
                CurrentMana = ActiveClass == CharacterClass.Warrior ? 0 : Stats.MaxMana;
            }
    }
}
