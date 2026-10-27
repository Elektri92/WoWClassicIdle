// Models/Player.Talents.cs
namespace Models
{
    // Neue Systeme als eigene Partial-Datei anlegen; Dateien möglichst < 300 Zeilen halten.
    public partial class Player
    {
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

        /// <summary>Ist eine Talent-Fähigkeit durch ein geskilltes Talent freigeschaltet?</summary>
        public bool HasTalentAbility(string abilityId) =>
            TalentRegistry.GetGrantingTalents(ActiveClass, abilityId).Any(t => TalentState.GetRanks(t.Id) > 0);

        /// <summary>Investiert einen Talent-Punkt und berechnet Stats neu.</summary>
        public bool InvestTalent(TalentDefinition talent)
        {
            int available = GetAvailableTalentPoints(ActiveClass);
            bool ok = TalentState.Invest(talent, available, Level);
            if (ok) RecalculateStats();
            return ok;
        }

        /// <summary>Setzt alle Talente der Klasse zurück und berechnet Stats neu.</summary>
        public void ResetTalents(CharacterClass cls)
        {
            TalentState.Reset(cls);

            // Talent-exklusive Abilities aus Loadout und aktiven Buffs entfernen
            if (cls == ActiveClass)
            {
                var talentOnlyIds = ClassAbilityRegistry.GetAbilitiesFor(cls)
                    .Where(a => a.TalentUnlockOnly)
                    .Select(a => a.Id)
                    .ToHashSet();

                var loadout = GetCurrentLoadout();
                loadout.EquippedAbilityIds.RemoveAll(id => talentOnlyIds.Contains(id));
                ApplyLoadout(loadout); // Ability-Cache und Gestalt aktualisieren

                foreach (var id in talentOnlyIds)
                    Buffs.Remove(id);
            }

            RecalculateStats();
        }

        /// <summary>Stellt Talent-Punkte aus einem Spielstand wieder her.</summary>
        internal void RestoreTalentPoints(Dictionary<string, int> points)
        {
            foreach (var (talentId, ranks) in points)
                TalentState.RestoreRanks(talentId, ranks);
            RecalculateStats();
        }
    }
}
