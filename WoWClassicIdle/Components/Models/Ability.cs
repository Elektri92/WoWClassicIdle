// Models/Ability.cs
namespace Models;

public sealed partial record Ability
{
    public string Id { get; init; } = "";
    public string Name { get; init; } = "";
    public string IconUrl { get; init; } = "";

    /// <summary>Abilities with the same non-empty group are mutually exclusive (e.g. Fireball vs. Frostbolt).</summary>
    public string ExclusiveGroup { get; init; } = "";
    public double Cooldown { get; init; }   // Sekunden
    public int ManaCost { get; init; }
    public int Damage { get; init; }       // > 0 → deals damage
    public int Heal { get; init; }         
    public int Rank { get; init; }
    public int LevelRequired { get; init; }

    /// <summary>Cast time in seconds. &gt; 0 → effect only after the cast completes, does not trigger a GCD.</summary>
    public double CastTime { get; init; }
    public bool HasCastTime => CastTime > 0;

    /// <summary>Melee strike: damage = weapon swing (incl. AP) + <see cref="Damage"/>. Otherwise fixed spell damage.</summary>
    public bool AddsWeaponDamage { get; init; }

    /// <summary>"On next swing" like Heroic Strike: replaces the next auto-attack instead of firing via its cooldown.</summary>
    public bool ReplacesAutoAttack { get; init; }

    /// <summary>Minimum resource (Mana/Rage) at which the ability is used. 0 = only <see cref="ManaCost"/> counts.</summary>
    public int MinResourceToCast { get; init; }

    /// <summary>Buff duration in seconds. &gt; 0 → ability is a self-buff that is recast automatically as soon as it expires.</summary>
    public double BuffDuration { get; init; }

    /// <summary>Damage bonus of the buff on all damage dealt (0.05 = +5 %).</summary>
    public double BuffDamageBonus { get; init; }

    /// <summary>Heal over Time: total healing over the <see cref="BuffDuration"/>, spread across ticks.</summary>
    public int HotTotalHeal { get; init; }

    /// <summary>Seconds between two HoT ticks.</summary>
    public double HotTickInterval { get; init; } = 3;

    /// <summary>+X% attack speed while buff is active (e.g. 0.2 = +20%). Divides swing interval by (1+bonus).</summary>
    public double BuffAttackSpeedBonus { get; init; }

    /// <summary>+X% auto-attack damage while buff is active (e.g. 0.1 = +10%).</summary>
    public double BuffWeaponDamageBonus { get; init; }

    /// <summary>Flat Intellect bonus while buff is active (+15 max mana per point).</summary>
    public int BuffIntellectBonus { get; init; }

    /// <summary>Flat Spirit bonus while buff is active (increases HP/Mana regen out of combat).</summary>
    public int BuffSpiritBonus { get; init; }

    // ─── Debuff (applied to enemy) ───

    /// <summary>Duration in seconds of the debuff placed on the enemy. &gt; 0 → ability is a debuff.</summary>
    public double DebuffDuration { get; init; }

    /// <summary>Fraction of additional damage the enemy takes from all sources (0.05 = +5%).</summary>
    public double DebuffDamageTakenBonus { get; init; }

    /// <summary>Damage dealt per DoT tick while the debuff is active.</summary>
    public int DebuffDotDamage { get; init; }

    /// <summary>Seconds between two DoT ticks.</summary>
    public double DebuffDotTickInterval { get; init; } = 3;

    /// <summary>Share of DoT damage that heals the player (1.0 = 100 %, e.g. Drain Life).</summary>
    public double DebuffDotLifesteal { get; init; }

    /// <summary>Reduces the enemy's attack speed by this share (0.20 = −20 %, e.g. Thunderclap).</summary>
    public double DebuffAttackSpeedReduction { get; init; }

    /// <summary>Passive: ability is not used actively but triggered by a proc.</summary>
    public bool IsPassive { get; init; }

    /// <summary>Proc chance per auto-attack (0.05 = 5 %).</summary>
    public double ProcChance { get; init; }

    /// <summary>Maximum stack count for stackable debuffs (0 = no stacking → 1 stack).</summary>
    public int DebuffMaxStacks { get; init; }

    /// <summary>Share of max mana regenerated over <see cref="BuffDuration"/> seconds (0.10 = 10 %).</summary>
    public double MotManaRestorePct { get; init; }

    /// <summary>Seconds between two mana-over-time ticks.</summary>
    public double MotTickInterval { get; init; } = 5.0;

    public bool IsMot => MotManaRestorePct > 0;

    /// <summary>Total number of MoT ticks over the <see cref="BuffDuration"/>.</summary>
    public int MotTickCount => IsMot ? Math.Max(1, (int)Math.Round(BuffDuration / MotTickInterval)) : 0;

    /// <summary>Share of max HP regenerated over <see cref="BuffDuration"/> seconds (0.12 = 12 %).</summary>
    public double HotHealPct { get; init; }

    /// <summary>Only usable when the enemy has less than this share of its max HP (0.20 = 20 %). 0 = no restriction.</summary>
    public double RequiresEnemyBelowHpPct { get; init; }

    /// <summary>Buff protects the player from all hits (immunity) while active.</summary>
    public bool BuffGrantsImmunity { get; init; }

    /// <summary>HP cost of Life Tap (deducted from the player).</summary>
    public int LifeTapHpCost { get; init; }

    /// <summary>Mana gain of Life Tap.</summary>
    public int LifeTapManaGain { get; init; }

    /// <summary>Retaliation damage when the player is hit during this buff (e.g. Thorns, Lightning Shield).</summary>
    public int BuffRetaliationDamage { get; init; }

    /// <summary>Weapon buff (Windfury): chance per auto-attack for <see cref="BuffExtraAttacks"/> additional swings.</summary>
    public double BuffExtraAttackChance { get; init; }
    public int BuffExtraAttacks { get; init; }

    /// <summary>Number of hits of the direct damage (Chain Lightning: 3). Each hit rolls its own crit.</summary>
    public int HitCount { get; init; } = 1;

    /// <summary>Additional crit chance for this ability only (0.20 = +20 %).</summary>
    public double BonusCritChance { get; init; }

    /// <summary>Can only be unlocked via a talent, does not appear in the default loadout.</summary>
    public bool TalentUnlockOnly { get; init; }

    /// <summary>
    /// Preferred talent tree (0/1/2) for the simulation. −1 = no preference.
    /// Used when several abilities compete in the same ExclusiveGroup (e.g. Fireball vs. Frostbolt).
    /// </summary>
    public int SpecHint { get; init; } = -1;
    /// <summary>+% rage generation from auto-attacks while the buff is active.</summary>
    public double BuffRageGenBonus { get; init; }
    /// <summary>+% crit chance while the buff is active.</summary>
    public double BuffCritChanceBonus { get; init; }
    /// <summary>-% damage taken while the buff is active.</summary>
    public double BuffDamageTakenReduction { get; init; }

    /// <summary>Only cast when current mana is below this share of MaxMana (0 = no condition).</summary>
    public double CastWhenManaBelowPct { get; init; }

    /// <summary>Buff absorbs this share of max HP as a shield (0.30 = 30 %).</summary>
    public double BuffShieldPct { get; init; }

    /// <summary>Debuff stuns the enemy: no attacks for <see cref="DebuffDuration"/> seconds.</summary>
    public bool DebuffIsStun { get; init; }

    /// <summary>Druid form: works passively as long as the ability is equipped (see <see cref="ShapeshiftForms"/>).</summary>
    public DruidForm Form { get; init; }

    /// <summary>Only equippable and usable in this form (e.g. Maul → Bear). None = no restriction.</summary>
    public DruidForm RequiredForm { get; init; }

    /// <summary>
    /// Alternative Kosten in Rage-/Energie-Gestalten (z. B. Rejuvenation: Bear → Rage, Cat → Energy).
    /// An ability with an entry for a form is usable there despite its mana cost.
    /// </summary>
    public IReadOnlyDictionary<DruidForm, int> FormCosts { get; init; } = NoFormCosts;
    private static readonly IReadOnlyDictionary<DruidForm, int> NoFormCosts = new Dictionary<DruidForm, int>();

    /// <summary>Buff ends early after this many enemy hits taken (Holy Shield: 3). 0 = duration only.</summary>
    public int BuffHitCharges { get; init; }

    /// <summary>Buff turns the next damaging ability into a guaranteed crit and is consumed by it (Cold Blood).</summary>
    public bool BuffGuaranteesCrit { get; init; }

    /// <summary>
    /// Empowers the next ability with this name by <see cref="EmpowerDamageBonus"/> (Mongoose Bite → Raptor Strike).
    /// For a damaging ability: a hit applies the empower buff. For a buff: the buff itself is the empowerment.
    /// </summary>
    public string EmpowersAbilityName { get; init; } = "";
    public double EmpowerDamageBonus { get; init; }

    /// <summary>Duration of the empower buff in seconds.</summary>
    public double EmpowerDuration { get; init; } = 10;

    public bool IsEmpowering => EmpowersAbilityName.Length > 0 && EmpowerDamageBonus > 0;

    /// <summary>Summons (or revives) this pet (Hunter pets, Warlock demons).</summary>
    public PetKind SummonsPet { get; init; }

    public bool IsPetSummon => SummonsPet != PetKind.None;

    /// <summary>
    /// Multiplier on the entire hit incl. weapon damage (talents/forms on weapon strikes).
    /// Percentage bonuses on <see cref="Damage"/> only would almost vanish on weapon strikes (4 × 1.02 → 4).
    /// </summary>
    public double DamageMultiplier { get; init; } = 1.0;

    /// <summary>
    /// Copy with +<paramref name="bonus"/> damage: weapon strikes via <see cref="DamageMultiplier"/>,
    /// spells directly on <see cref="Damage"/>.
    /// </summary>
    public Ability WithDamageBonus(double bonus) =>
        bonus == 0 ? this
        : AddsWeaponDamage ? this with { DamageMultiplier = DamageMultiplier * (1.0 + bonus) }
        : this with { Damage = (int)Math.Round(Damage * (1.0 + bonus)) };

    public bool IsForm => Form != DruidForm.None;

    public bool IsLifeTap => LifeTapHpCost > 0 && LifeTapManaGain > 0;

    public bool IsDamage => Damage > 0;
    public bool IsHeal => Heal > 0;
    public bool IsBuff => BuffDuration > 0;
    public bool IsHot => IsBuff && (HotTotalHeal > 0 || HotHealPct > 0);

    /// <summary>Applies a debuff to the enemy. True even when the ability also deals direct damage (e.g. Moonfire).</summary>
    public bool IsDebuff => DebuffDuration > 0;
    public bool IsDebuffDot => IsDebuff && DebuffDotDamage > 0;

    /// <summary>Total number of DoT ticks over <see cref="DebuffDuration"/>.</summary>
    public int DebuffDotTickCount => IsDebuffDot ? Math.Max(1, (int)Math.Round(DebuffDuration / DebuffDotTickInterval)) : 0;

    /// <summary>
    /// Direct heal or HoT – only cast below <see cref="HealThreshold"/>.
    /// Damage spells with incidental healing (Holy Nova) don't count and are used normally.
    /// </summary>
    public bool IsHealing => (IsHeal && !IsDamage) || IsHot;

    public bool IsShield => IsBuff && BuffShieldPct > 0;

    /// <summary>Pure player buff (no heal, no shield) – highest cast priority.</summary>
    public bool IsPlayerBuff => IsBuff && !IsHealing && !IsShield;

    /// <summary>
    /// Cast priority: 0 = player buffs (+ utility without damage such as Life Tap, stuns, marks, pet summons),
    /// 1 = heal/shield, 2 = damage (within the group by estimated DPS).
    /// </summary>
    public int CastPriorityGroup =>
        IsPlayerBuff || IsLifeTap || IsPetSummon || (IsDebuff && !IsDamage && !IsDebuffDot) ? 0
        : IsHealing || IsShield ? 1
        : 2;

    /// <summary>HP share at (and below) which healing spells are cast automatically.</summary>
    public const double HealThreshold = 0.5;

    /// <summary>Number of HoT ticks over the entire duration.</summary>
    public int HotTickCount => IsHot ? Math.Max(1, (int)Math.Round(BuffDuration / HotTickInterval)) : 0;

    /// <summary>Healing per tick (the remainder of the integer division lands on the last tick).</summary>
    public int HotHealForTick(int tickIndex)
    {
        int perTick = HotTotalHeal / HotTickCount;
        return tickIndex == HotTickCount - 1 ? HotTotalHeal - perTick * (HotTickCount - 1) : perTick;
    }
}
