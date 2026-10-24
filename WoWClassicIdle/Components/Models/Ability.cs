// Models/Ability.cs
namespace Models;

public sealed record Ability
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

    /// <summary>Direktheilung oder HoT – wird erst unter <see cref="HealThreshold"/> gewirkt.</summary>
    public bool IsHealing => IsHeal || IsHot;

    public bool IsShield => IsBuff && BuffShieldPct > 0;

    /// <summary>Reiner Spieler-Buff (keine Heilung, kein Schild) – höchste Cast-Priorität.</summary>
    public bool IsPlayerBuff => IsBuff && !IsHealing && !IsShield;

    /// <summary>
    /// Cast-Priorität: 0 = Spieler-Buffs (+ Utility ohne Schaden wie Life Tap, Stuns, Marks),
    /// 1 = Heilung/Schild, 2 = Schaden (innerhalb der Gruppe nach geschätztem DPS).
    /// </summary>
    public int CastPriorityGroup =>
        IsPlayerBuff || IsLifeTap || (IsDebuff && !IsDamage && !IsDebuffDot) ? 0
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
