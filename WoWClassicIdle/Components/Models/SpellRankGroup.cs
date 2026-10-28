namespace Models;

/// <summary>
/// Alle sichtbaren Ränge eines Zaubers (gleicher Name) für das Zauberbuch in /ability-loadout.
/// </summary>
/// <param name="Ranks">Sichtbare Ränge, aufsteigend nach Rang.</param>
/// <param name="Unlocked">Davon bereits nutzbare Ränge (Level erreicht bzw. per Talent freigeschaltet).</param>
/// <param name="Equipped">Aktuell ausgerüsteter Rang oder null.</param>
public sealed record SpellRankGroup(string Name, IReadOnlyList<Ability> Ranks, IReadOnlyList<Ability> Unlocked, Ability? Equipped)
{
    /// <summary>Höchster nutzbarer Rang.</summary>
    public Ability? Best => Unlocked.Count > 0 ? Unlocked[^1] : null;

    /// <summary>Angezeigter Rang: ausgerüstet → bester nutzbarer → erster gesperrter.</summary>
    public Ability Display => Equipped ?? Best ?? Ranks[0];

    public bool MultiRank => Ranks.Count > 1;
    public bool IsLocked => Unlocked.Count == 0;
    public bool CanUpgrade => Equipped is not null && Best is not null && Best.Rank > Equipped.Rank;
}
