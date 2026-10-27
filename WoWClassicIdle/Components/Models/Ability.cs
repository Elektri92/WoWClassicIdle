// Models/Ability.cs
namespace Models;

public sealed partial record Ability
{
    public string Id { get; init; } = "";
    public string Name { get; init; } = "";
    public string IconUrl { get; init; } = "";

    /// <summary>Abilities mit derselben nicht-leeren Gruppe schließen sich gegenseitig aus (z. B. Fireball vs. Frostbolt).</summary>
    public string ExclusiveGroup { get; init; } = "";
    public double Cooldown { get; init; }   // Sekunden
    public int ManaCost { get; init; }
    public int Damage { get; init; }       // > 0 → macht Schaden
    public int Heal { get; init; }         
    public int Rank { get; init; }
    public int LevelRequired { get; init; }

    /// <summary>Zauberzeit in Sekunden. &gt; 0 → Effekt erst nach Abschluss des Casts, löst keinen GCD aus.</summary>
    public double CastTime { get; init; }
    public bool HasCastTime => CastTime > 0;

    /// <summary>Nahkampf-Schlag: Schaden = Waffenschlag (inkl. AP) + <see cref="Damage"/>. Sonst fixer Zauberschaden.</summary>
    public bool AddsWeaponDamage { get; init; }

    /// <summary>„On next swing“ wie Heroic Strike: ersetzt die nächste Auto-Attack statt über den Cooldown zu feuern.</summary>
    public bool ReplacesAutoAttack { get; init; }

    /// <summary>Mindest-Ressource (Mana/Rage), ab der die Fähigkeit eingesetzt wird. 0 = nur <see cref="ManaCost"/> zählt.</summary>
    public int MinResourceToCast { get; init; }

    /// <summary>Buff-Laufzeit in Sekunden. &gt; 0 → Fähigkeit ist ein Selbst-Buff, der automatisch neu gecastet wird, sobald er ausläuft.</summary>
    public double BuffDuration { get; init; }

    /// <summary>Schadensbonus des Buffs auf allen verursachten Schaden (0.05 = +5 %).</summary>
    public double BuffDamageBonus { get; init; }

    /// <summary>Heal over Time: Gesamtheilung über die <see cref="BuffDuration"/>, verteilt auf Ticks.</summary>
    public int HotTotalHeal { get; init; }

    /// <summary>Sekunden zwischen zwei HoT-Ticks.</summary>
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

    /// <summary>Anteil des DoT-Schadens, der den Spieler heilt (1.0 = 100 %, z. B. Drain Life).</summary>
    public double DebuffDotLifesteal { get; init; }

    /// <summary>Verringert das Angriffstempo des Gegners um diesen Anteil (0.20 = −20 %, z. B. Thunderclap).</summary>
    public double DebuffAttackSpeedReduction { get; init; }

    /// <summary>Passiv: Fähigkeit wird nicht aktiv eingesetzt, sondern per Proc ausgelöst.</summary>
    public bool IsPassive { get; init; }

    /// <summary>Proc-Chance pro Auto-Attack (0.05 = 5 %).</summary>
    public double ProcChance { get; init; }

    /// <summary>Maximale Stack-Anzahl für stapelbare Debuffs (0 = kein Stacking → 1 Stack).</summary>
    public int DebuffMaxStacks { get; init; }

    /// <summary>Anteil des maximalen Manas, der über <see cref="BuffDuration"/> Sekunden regeneriert wird (0.10 = 10 %).</summary>
    public double MotManaRestorePct { get; init; }

    /// <summary>Sekunden zwischen zwei Mana-over-Time-Ticks.</summary>
    public double MotTickInterval { get; init; } = 5.0;

    public bool IsMot => MotManaRestorePct > 0;

    /// <summary>Gesamtzahl der MoT-Ticks über die <see cref="BuffDuration"/>.</summary>
    public int MotTickCount => IsMot ? Math.Max(1, (int)Math.Round(BuffDuration / MotTickInterval)) : 0;

    /// <summary>Anteil der maximalen HP, der über <see cref="BuffDuration"/> Sekunden regeneriert wird (0.12 = 12 %).</summary>
    public double HotHealPct { get; init; }

    /// <summary>Nur einsetzbar wenn der Gegner weniger als diesen Anteil seiner max. HP hat (0.20 = 20 %). 0 = keine Einschränkung.</summary>
    public double RequiresEnemyBelowHpPct { get; init; }

    /// <summary>Buff schützt den Spieler vor allen Treffern (Immunität) solange aktiv.</summary>
    public bool BuffGrantsImmunity { get; init; }

    /// <summary>HP-Kosten des Life Tap (wird vom Spieler abgezogen).</summary>
    public int LifeTapHpCost { get; init; }

    /// <summary>Mana-Gewinn des Life Tap.</summary>
    public int LifeTapManaGain { get; init; }

    /// <summary>Vergeltungsschaden wenn der Spieler während dieses Buffs getroffen wird (z. B. Thorns, Lightning Shield).</summary>
    public int BuffRetaliationDamage { get; init; }

    /// <summary>Waffen-Buff (Windfury): Chance pro Auto-Attack auf <see cref="BuffExtraAttacks"/> zusätzliche Schläge.</summary>
    public double BuffExtraAttackChance { get; init; }
    public int BuffExtraAttacks { get; init; }

    /// <summary>Anzahl Treffer des Direktschadens (Chain Lightning: 3). Jeder Treffer würfelt eigenen Crit.</summary>
    public int HitCount { get; init; } = 1;

    /// <summary>Zusätzliche Crit-Chance nur für diese Fähigkeit (0.20 = +20 %).</summary>
    public double BonusCritChance { get; init; }

    /// <summary>Kann nur über ein Talent freigeschaltet werden, erscheint nicht im Standard-Loadout.</summary>
    public bool TalentUnlockOnly { get; init; }

    /// <summary>
    /// Bevorzugter Talent-Baum (0/1/2) für die Simulation. −1 = keine Präferenz.
    /// Wird verwendet, wenn mehrere Abilities in derselben ExclusiveGroup konkurrieren (z. B. Fireball vs. Frostbolt).
    /// </summary>
    public int SpecHint { get; init; } = -1;
    /// <summary>+% Rage-Generierung durch Auto-Attacks während der Buff aktiv ist.</summary>
    public double BuffRageGenBonus { get; init; }
    /// <summary>+% Crit-Chance während der Buff aktiv ist.</summary>
    public double BuffCritChanceBonus { get; init; }
    /// <summary>-% erlittener Schaden während der Buff aktiv ist.</summary>
    public double BuffDamageTakenReduction { get; init; }

    /// <summary>Wird nur gewirkt wenn aktuelles Mana unter diesem Anteil von MaxMana liegt (0 = keine Bedingung).</summary>
    public double CastWhenManaBelowPct { get; init; }

    /// <summary>Buff absorbiert diesen Anteil der maximalen HP als Schild (0.30 = 30 %).</summary>
    public double BuffShieldPct { get; init; }

    /// <summary>Debuff betäubt den Gegner: keine Angriffe für <see cref="DebuffDuration"/> Sekunden.</summary>
    public bool DebuffIsStun { get; init; }

    /// <summary>Druiden-Gestalt: wirkt passiv, solange die Fähigkeit ausgerüstet ist (siehe <see cref="ShapeshiftForms"/>).</summary>
    public DruidForm Form { get; init; }

    /// <summary>Nur in dieser Gestalt ausrüst- und einsetzbar (z. B. Maul → Bear). None = keine Einschränkung.</summary>
    public DruidForm RequiredForm { get; init; }

    /// <summary>
    /// Alternative Kosten in Rage-/Energie-Gestalten (z. B. Rejuvenation: Bear → Rage, Cat → Energy).
    /// Eine Fähigkeit mit Eintrag für eine Gestalt ist dort trotz Manakosten nutzbar.
    /// </summary>
    public IReadOnlyDictionary<DruidForm, int> FormCosts { get; init; } = NoFormCosts;
    private static readonly IReadOnlyDictionary<DruidForm, int> NoFormCosts = new Dictionary<DruidForm, int>();

    /// <summary>Buff endet nach so vielen erlittenen Gegnertreffern vorzeitig (Holy Shield: 3). 0 = nur Laufzeit.</summary>
    public int BuffHitCharges { get; init; }

    /// <summary>Buff macht die nächste Schadensfähigkeit zu einem garantierten Crit und wird dabei verbraucht (Cold Blood).</summary>
    public bool BuffGuaranteesCrit { get; init; }

    /// <summary>
    /// Verstärkt die nächste Fähigkeit mit diesem Namen um <see cref="EmpowerDamageBonus"/> (Mongoose Bite → Raptor Strike).
    /// Bei einer Schadensfähigkeit: Treffer legt den Verstärkungs-Buff an. Bei einem Buff: der Buff selbst ist die Verstärkung.
    /// </summary>
    public string EmpowersAbilityName { get; init; } = "";
    public double EmpowerDamageBonus { get; init; }

    /// <summary>Laufzeit des Verstärkungs-Buffs in Sekunden.</summary>
    public double EmpowerDuration { get; init; } = 10;

    public bool IsEmpowering => EmpowersAbilityName.Length > 0 && EmpowerDamageBonus > 0;

    /// <summary>Beschwört (bzw. belebt) dieses Begleittier (Hunter-Pets, Warlock-Dämonen).</summary>
    public PetKind SummonsPet { get; init; }

    public bool IsPetSummon => SummonsPet != PetKind.None;

    /// <summary>
    /// Multiplikator auf den gesamten Treffer inkl. Waffenschaden (Talente/Gestalten auf Waffenschläge).
    /// Prozent-Boni nur auf <see cref="Damage"/> würden bei Waffenschlägen fast verpuffen (4 × 1.02 → 4).
    /// </summary>
    public double DamageMultiplier { get; init; } = 1.0;

    /// <summary>
    /// Kopie mit +<paramref name="bonus"/> Schaden: Waffenschläge über <see cref="DamageMultiplier"/>,
    /// Zauber direkt auf <see cref="Damage"/>.
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
    /// Direktheilung oder HoT – wird erst unter <see cref="HealThreshold"/> gewirkt.
    /// Schadenszauber mit Nebenheilung (Holy Nova) zählen nicht dazu und werden normal eingesetzt.
    /// </summary>
    public bool IsHealing => (IsHeal && !IsDamage) || IsHot;

    public bool IsShield => IsBuff && BuffShieldPct > 0;

    /// <summary>Reiner Spieler-Buff (keine Heilung, kein Schild) – höchste Cast-Priorität.</summary>
    public bool IsPlayerBuff => IsBuff && !IsHealing && !IsShield;

    /// <summary>
    /// Cast-Priorität: 0 = Spieler-Buffs (+ Utility ohne Schaden wie Life Tap, Stuns, Marks, Pet-Beschwörung),
    /// 1 = Heilung/Schild, 2 = Schaden (innerhalb der Gruppe nach geschätztem DPS).
    /// </summary>
    public int CastPriorityGroup =>
        IsPlayerBuff || IsLifeTap || IsPetSummon || (IsDebuff && !IsDamage && !IsDebuffDot) ? 0
        : IsHealing || IsShield ? 1
        : 2;

    /// <summary>HP-Anteil, ab dem (und darunter) Heilzauber automatisch gewirkt werden.</summary>
    public const double HealThreshold = 0.5;

    /// <summary>Anzahl HoT-Ticks über die gesamte Laufzeit.</summary>
    public int HotTickCount => IsHot ? Math.Max(1, (int)Math.Round(BuffDuration / HotTickInterval)) : 0;

    /// <summary>Heilung pro Tick (Rest der Ganzzahl-Division landet auf dem letzten Tick).</summary>
    public int HotHealForTick(int tickIndex)
    {
        int perTick = HotTotalHeal / HotTickCount;
        return tickIndex == HotTickCount - 1 ? HotTotalHeal - perTick * (HotTickCount - 1) : perTick;
    }
}
