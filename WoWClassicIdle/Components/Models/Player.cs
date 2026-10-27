// Models/Player.cs
namespace Models
{
    // Neue Systeme als eigene Partial-Datei anlegen; Dateien möglichst < 300 Zeilen halten.
    // Equipment → Player.Equipment.cs | Talente → Player.Talents.cs | Stats/Kampfhilfen → Player.Combat.cs
    public partial class Player : Character
    {
        public Faction Faction { get; private set; }
        public Race Race { get; private set; }

        /// <summary>Die aktuell "ausgestattete" Klasse – XP fließt hier hinein.</summary>
        public CharacterClass ActiveClass { get; private set; }

        /// <summary>Progression aller erlaubten Klassen dieser Rasse.</summary>
        public Dictionary<CharacterClass, ClassProgress> ClassProgress { get; }

        /// <summary>Gesamtes Geld in Kupfer (Anzeige als Gold/Silber/Kupfer über <see cref="Money"/>).</summary>
        public long Copper { get; set; }

        /// <summary>Rucksack + 4 Taschenplätze (wird von der Engine auf das gemeinsame Inventar gesetzt).</summary>
        public Inventory Inventory { get; internal set; } = new();

        /// <summary>Talent-Punkte und investierte Ränge je Klasse.</summary>
        public TalentState TalentState { get; } = new();

        /// <summary>Erlernte Berufe (ein Herstellungsberuf; Sammel-/Sekundärberufe sind accountweit).</summary>
        public ProfessionState ProfessionState { get; } = new();

        /// <summary>Currently equipped weapon. Auto-set to class starter on creation.</summary>
        public WeaponItem? EquippedWeapon { get; private set; }

        /// <summary>Ausgerüstete Rüstungsteile je Slot.</summary>
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
            {
                RecalculateStats();
                Inventory.UpdateBagsForLevel(Level);
            }
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
        /// Prüft, ob der aktive Charakter die Level- und Klassen-Voraussetzungen eines Items erfüllt.
        /// </summary>
        public bool MeetsItemRequirements(Item item) =>
            ItemRequirements.MatchesLevel(item, Level) && ItemRequirements.MatchesClass(item, ActiveClass);
    }
}
