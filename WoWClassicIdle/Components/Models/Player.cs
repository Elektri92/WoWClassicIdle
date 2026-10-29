// Models/Player.cs
namespace Models
{
    // Create new systems as their own partial file; keep files below 300 lines where possible.
    // Equipment → Player.Equipment.cs | Talente → Player.Talents.cs | Stats/Kampfhilfen → Player.Combat.cs
    public partial class Player : Character
    {
        public Faction Faction { get; private set; }
        public Race Race { get; private set; }

        /// <summary>The currently "equipped" class – XP flows into it.</summary>
        public CharacterClass ActiveClass { get; private set; }

        /// <summary>Progression of all allowed classes of this race.</summary>
        public Dictionary<CharacterClass, ClassProgress> ClassProgress { get; }

        /// <summary>Total money in copper (displayed as gold/silver/copper via <see cref="Money"/>).</summary>
        public long Copper { get; set; }

        /// <summary>Backpack + 4 bag slots (set to the shared inventory by the engine).</summary>
        public Inventory Inventory { get; internal set; } = new();

        /// <summary>Talent points and invested ranks per class.</summary>
        public TalentState TalentState { get; } = new();

        /// <summary>Learned professions (one crafting profession; gathering/secondary professions are account-wide).</summary>
        public ProfessionState ProfessionState { get; } = new();

        /// <summary>Currently equipped weapon. Auto-set to class starter on creation.</summary>
        public WeaponItem? EquippedWeapon { get; private set; }

        /// <summary>Equipped armor pieces per slot.</summary>
        public Dictionary<ArmorSlot, ArmorItem> EquippedArmor { get; } = new();

        // Equipped weapon saved per class so switching back restores the right weapon.
        private readonly Dictionary<CharacterClass, WeaponItem?> _equippedByClass = new();

        internal IReadOnlyDictionary<CharacterClass, WeaponItem?> EquippedWeaponsByClass => _equippedByClass;

        // ─── Convenience-Properties (delegieren an aktive Klasse) ───
        public int Level         => ActiveProgress.Level;
        public int Xp            => ActiveProgress.Xp;
        public int XpToNextLevel => ActiveProgress.XpToNextLevel;

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

            // Every class allowed for the race gets an entry (level 1).
            ClassProgress = new Dictionary<CharacterClass, ClassProgress>();
            foreach (var cls in RaceClassInfo.GetAvailableClasses(race))
            {
                ClassProgress[cls] = new ClassProgress(cls);
            }

            EquippedWeapon = WeaponRegistry.GetStarter(characterClass);
            _equippedByClass[characterClass] = EquippedWeapon;
        }

        /// <summary>
        /// XP is credited to the active class.
        /// On level-up: stats are recalculated.
        /// </summary>
        public int GainXp(int amount)
        {
            int levelsGained = ActiveProgress.GainXp(amount);
            if (levelsGained > 0)
            {
                RecalculateStats();
                Inventory.UpdateBagsForLevel(Level);
            }
            return levelsGained;
        }

        /// <summary>
        /// Death penalty: the active class loses a share of its current XP (no level loss).
        /// </summary>
        public int LoseXpPercent(double percent) => ActiveProgress.LoseXpPercent(percent);

        /// <summary>
        /// Updates race/faction and ensures that
        /// ClassProgress entries exist for the new race.
        /// </summary>
        public void ChangeCharacter(Faction newFaction, Race newRace)
        {
            Faction = newFaction;
            Race = newRace;

            // Add new classes of the race (without deleting existing ones)
            foreach (var cls in RaceClassInfo.GetAvailableClasses(newRace))
            {
                if (!ClassProgress.ContainsKey(cls))
                    ClassProgress[cls] = new ClassProgress(cls);
            }
        }

        /// <summary>
        /// Switches the active class (must be available for the race).
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
        /// Checks whether the active character meets the level and class requirements of an item.
        /// </summary>
        public bool MeetsItemRequirements(Item item) =>
            ItemRequirements.MatchesLevel(item, Level) && ItemRequirements.MatchesClass(item, ActiveClass);
    }
}
