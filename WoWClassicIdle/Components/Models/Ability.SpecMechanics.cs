// Models/Ability.SpecMechanics.cs
namespace Models;

// Mechaniken der Spezialisierungs-Fähigkeiten (Talent-Reihe 3, ab Lv 20): Auren, Synergie-Debuffs,
// Gratis-/Sofort-Zauber, Heilung bei Treffern. Reine Talent-Passive (Flurry, Nightfall …) siehe TalentEffectType.
public sealed partial record Ability
{
    /// <summary>Mehr verursachte Heilung, solange der Buff aktiv ist (0.20 = +20 %, z. B. Holy Aura).</summary>
    public double BuffHealingBonus { get; init; }

    /// <summary>Schnellere Mana- bzw. Energie-Regeneration (1.0 = doppelt so schnell, z. B. Adrenaline Rush).</summary>
    public double BuffResourceRegenBonus { get; init; }

    /// <summary>Mehr Schaden des Begleittiers (0.50 = +50 %, z. B. Aspect of the Owl).</summary>
    public double BuffPetDamageBonus { get; init; }

    /// <summary>Zusätzlicher Anteil des Gegnerschadens, den das Begleittier abfängt (0.20 = +20 Prozentpunkte, Soul Link).</summary>
    public double BuffPetDamageShareBonus { get; init; }

    /// <summary>Die nächsten N Fähigkeiten kosten keine Ressource, solange der Buff läuft (Vanish, Spirit of Redemption).</summary>
    public int BuffFreeCasts { get; init; }

    /// <summary>Heilt den Spieler bei jedem erlittenen Treffer um diesen Wert (Earth Shield, verbraucht <see cref="BuffHitCharges"/>).</summary>
    public int BuffHealOnHit { get; init; }

    /// <summary>Die nächste Fähigkeit mit diesem Namen wird ohne Zauberzeit gewirkt und verbraucht den Buff (Shadow Trance).</summary>
    public string BuffInstantCastName { get; init; } = "";

    /// <summary>
    /// Synergie: liegt ein Debuff mit diesem Namen auf dem Gegner, erhält die Fähigkeit
    /// <see cref="SynergyDamageBonus"/> und <see cref="SynergyCritBonus"/> (Lava Burst → Flame Shock).
    /// </summary>
    public string SynergyDebuffName { get; init; } = "";
    public double SynergyDamageBonus { get; init; }
    public double SynergyCritBonus { get; init; }

    public bool HasSynergy => SynergyDebuffName.Length > 0;

    /// <summary>Aura wirkt nur, solange ein Begleittier lebt (Soul Link).</summary>
    public bool RequiresActivePet { get; init; }

    /// <summary>
    /// Passive Aura (Paladin-Auren, Hunter-Aspekte, Warrior-Haltungen, Soul Link): der Buff liegt dauerhaft an,
    /// solange die Fähigkeit ausgerüstet und in der aktuellen Gestalt nutzbar ist – kein Cast, kein GCD.
    /// </summary>
    public bool IsAura => IsPassive && IsBuff;
}
