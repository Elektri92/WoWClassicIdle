// Models/TalentDefinition.cs
namespace Models;

public enum TalentEffectType
{
    CritChance,
    ArcaneIntellectBonus,       // +x% Stärke von Arcane Intellect
    ConjureWaterManaRegen,      // +x% Manaregg-Wert von Conjure Water
    AutoAttackDamage,           // +x% Auto-Angriff-Schaden
    FireballManaCost,           // -x Mana Kosten von Fireball (Flat)
    FireballDot,                // Fireball-DoT-Schaden pro Rang
    FireballCrit,               // +x% Crit-Chance für Fireball
    FrostboltManaCost,          // -x Mana Kosten von Frostbolt (Flat)
    FrostboltAttackSpeedDebuff, // +x% Angriffstempo-Debuff von Frostbolt
    FrostboltCrit,              // +x% Crit-Chance für Frostbolt
}

public class TalentDefinition
{
    public string Id { get; init; } = "";
    public string Name { get; init; } = "";
    public string Description { get; init; } = "";
    public CharacterClass Class { get; init; }
    public int TreeIndex { get; init; }   // 0, 1, 2
    public int Row { get; init; }         // 0–6
    public int Column { get; init; }      // 0–3
    public int MaxRanks { get; init; } = 1;
    public string? RequiresTalentId { get; init; }
    public TalentEffectType EffectType { get; init; }
    public double EffectPerRank { get; init; }
    public string IconUrl { get; init; } = "";

    /// <summary>Beschreibung des Effekts für einen bestimmten Rang.</summary>
    public string EffectDescription(int ranks) => EffectType switch
    {
        TalentEffectType.CritChance =>
            $"+{(EffectPerRank * ranks * 100):0.##}% Critical Strike Chance",
        TalentEffectType.ArcaneIntellectBonus =>
            $"+{(EffectPerRank * ranks * 100):0.##}% Arcane Intellect strength",
        TalentEffectType.ConjureWaterManaRegen =>
            $"+{(EffectPerRank * ranks * 100):0.##}% Conjure Water mana regen",
        TalentEffectType.AutoAttackDamage =>
            $"+{(EffectPerRank * ranks * 100):0.##}% auto-attack damage",
        TalentEffectType.FireballManaCost =>
            $"-{(EffectPerRank * ranks):0.##} Fireball mana cost",
        TalentEffectType.FireballDot =>
            $"Fireball DoT: {(EffectPerRank * ranks):0.##} DMG over 5s",
        TalentEffectType.FireballCrit =>
            $"+{(EffectPerRank * ranks * 100):0.##}% Fireball crit chance",
        TalentEffectType.FrostboltManaCost =>
            $"-{(EffectPerRank * ranks):0.##} Frostbolt mana cost",
        TalentEffectType.FrostboltAttackSpeedDebuff =>
            $"+{(EffectPerRank * ranks * 100):0.##}% Frostbolt attack slow",
        TalentEffectType.FrostboltCrit =>
            $"+{(EffectPerRank * ranks * 100):0.##}% Frostbolt crit chance",
        _ => ""
    };
}
