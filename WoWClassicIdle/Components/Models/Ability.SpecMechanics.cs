// Models/Ability.SpecMechanics.cs
namespace Models;

// Mechanics of the specialization abilities (talent row 3, from Lv 20): auras, synergy debuffs,
// free/instant casts, healing on hits. Pure talent passives (Flurry, Nightfall …) see TalentEffectType.
public sealed partial record Ability
{
    /// <summary>More healing done while the buff is active (0.20 = +20 %, e.g. Holy Aura).</summary>
    public double BuffHealingBonus { get; init; }

    /// <summary>Faster mana or energy regeneration (1.0 = twice as fast, e.g. Adrenaline Rush).</summary>
    public double BuffResourceRegenBonus { get; init; }

    /// <summary>More pet damage (0.50 = +50 %, e.g. Aspect of the Owl).</summary>
    public double BuffPetDamageBonus { get; init; }

    /// <summary>Additional share of enemy damage absorbed by the pet (0.20 = +20 percentage points, Soul Link).</summary>
    public double BuffPetDamageShareBonus { get; init; }

    /// <summary>The next N abilities cost no resource while the buff lasts (Vanish, Spirit of Redemption).</summary>
    public int BuffFreeCasts { get; init; }

    /// <summary>Heals the player by this amount on every hit taken (Earth Shield, consumes <see cref="BuffHitCharges"/>).</summary>
    public int BuffHealOnHit { get; init; }

    /// <summary>The next ability with this name is cast without cast time and consumes the buff (Shadow Trance).</summary>
    public string BuffInstantCastName { get; init; } = "";

    /// <summary>
    /// Synergy: if a debuff with this name is on the enemy, the ability gains
    /// <see cref="SynergyDamageBonus"/> and <see cref="SynergyCritBonus"/> (Lava Burst → Flame Shock).
    /// </summary>
    public string SynergyDebuffName { get; init; } = "";
    public double SynergyDamageBonus { get; init; }
    public double SynergyCritBonus { get; init; }

    public bool HasSynergy => SynergyDebuffName.Length > 0;

    /// <summary>Aura only works while a pet is alive (Soul Link).</summary>
    public bool RequiresActivePet { get; init; }

    /// <summary>
    /// Passive aura (Paladin auras, Hunter aspects, Warrior stances, Soul Link): the buff is applied permanently
    /// as long as the ability is equipped and usable in the current form – no cast, no GCD.
    /// </summary>
    public bool IsAura => IsPassive && IsBuff;
}
