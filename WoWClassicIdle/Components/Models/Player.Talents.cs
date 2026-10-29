// Models/Player.Talents.cs
namespace Models
{
    // Create new systems as their own partial file; keep files below 300 lines where possible.
    public partial class Player
    {
        /// <summary>
        /// Available talent points for a class (1 per level from level 10, minus invested ones).
        /// </summary>
        public int GetAvailableTalentPoints(CharacterClass cls)
        {
            if (!ClassProgress.TryGetValue(cls, out var progress)) return 0;
            int earned = Math.Max(0, progress.Level - 9);
            int spent  = TalentState.TotalInvested(cls);
            return Math.Max(0, earned - spent);
        }

        /// <summary>Is a talent ability unlocked by a skilled talent?</summary>
        public bool HasTalentAbility(string abilityId) =>
            TalentRegistry.GetGrantingTalents(ActiveClass, abilityId).Any(t => TalentState.GetRanks(t.Id) > 0);

        /// <summary>Invests a talent point and recalculates the stats.</summary>
        public bool InvestTalent(TalentDefinition talent)
        {
            int available = GetAvailableTalentPoints(ActiveClass);
            bool ok = TalentState.Invest(talent, available, Level);
            if (ok) RecalculateStats();
            return ok;
        }

        /// <summary>Resets all talents of the class and recalculates the stats.</summary>
        public void ResetTalents(CharacterClass cls)
        {
            TalentState.Reset(cls);

            // Remove talent-exclusive abilities from the loadout and active buffs
            if (cls == ActiveClass)
            {
                var talentOnlyIds = ClassAbilityRegistry.GetAbilitiesFor(cls)
                    .Where(a => a.TalentUnlockOnly)
                    .Select(a => a.Id)
                    .ToHashSet();

                var loadout = GetCurrentLoadout();
                loadout.EquippedAbilityIds.RemoveAll(id => talentOnlyIds.Contains(id));
                ApplyLoadout(loadout); // Update the ability cache and form

                foreach (var id in talentOnlyIds)
                    Buffs.Remove(id);
            }

            RecalculateStats();
        }

        /// <summary>Restores talent points from a save game.</summary>
        internal void RestoreTalentPoints(Dictionary<string, int> points)
        {
            foreach (var (talentId, ranks) in points)
                TalentState.RestoreRanks(talentId, ranks);
            RecalculateStats();
        }
    }
}
