// Kampf-Effekte (wwwroot/js/spellfx/*): Fähigkeiten fliegen vom Spieler zum Gegner, Auto-Attacks von Spieler
// und Gegner werden als Ausfallschritt/Geschoss mit waffen- bzw. kreaturabhängigem Treffer dargestellt.
// Buffs leuchten am Spieler auf, Debuffs landen sichtbar am Gegner; beide Icons fliegen in ihre Leiste.
// Die zugehörige Schadenszahl erscheint erst beim Einschlag.
using Models;
using Microsoft.JSInterop;

namespace WoWClassicIdle.Components.Pages;

public partial class IdleFighter
{
    /// <summary>Ein Effekt für spellFx.play – Feldnamen werden als camelCase an JS serialisiert.</summary>
    private sealed class PendingFx
    {
        public string Type { get; init; } = "spell";       // spell | swing | buff
        public string? Attacker { get; init; }              // swing: player | enemy
        public string? Style { get; init; }                 // swing: slash | chop | stab | blunt | claw | splash | arrow | wand
        public string? Icon { get; init; }
        public string? Name { get; init; }                  // spell/buff: Abgleich mit dem Icon in der Buff-/Debuff-Leiste
        public string? Kind { get; init; }                  // buff: buff | heal | shield | form
        public bool Debuff { get; init; }                   // spell: legt einen Debuff auf den Gegner
        public bool DebuffOnly { get; init; }               // spell: reiner Debuff ohne Schaden
        public bool Stun { get; init; }
        public bool Melee { get; init; }
        public bool Crit { get; set; }
        public int Duration { get; init; }                  // ms bis zum Treffer
    }

    private const int SpellFlightMs = 450;
    private const int MeleeFlightMs = 240;
    private const int SwingImpactMs = 110;      // Ausfallschritt bis zum Treffer
    private const int ArrowFlightMs = 260;
    private const int WandFlightMs = 320;
    private const int MaxFxPerTick = 3;         // bei hoher Spielgeschwindigkeit nur die letzten Effekte zeigen
    private static readonly TimeSpan MinFlushInterval = TimeSpan.FromMilliseconds(80);
    private readonly List<PendingFx> _pendingFx = new();
    private PendingFx? _activeSpellFx;          // gerade ausgelöste Fähigkeit, deren Treffer noch gemeldet werden
    private DateTime _lastFlush = DateTime.MinValue;
    private bool _lastEnemyHitCrit;
    private bool _lifeTapPending;               // Life Tap meldet seine HP-Kosten als „Treffer“ am Spieler

    private static bool IsOffensive(Ability a) =>
        (a.IsDamage || a.IsDebuff) && !a.IsPetSummon && !a.IsLifeTap;

    private void AddFx(PendingFx fx)
    {
        lock (_pendingFx)
            _pendingFx.Add(fx);
    }

    // Timer-Thread! Wird aus OnAbilityTriggered aufgerufen.
    private void QueueProjectile(Ability ability)
    {
        _activeSpellFx = null;
        _lifeTapPending = ability.IsLifeTap;
        if (_selfRef is null) return;
        if (!IsOffensive(ability))
        {
            QueueBuff(ability);
            return;
        }

        bool melee = ability.AddsWeaponDamage || ability.ReplacesAutoAttack;
        // On-Next-Swing (Heroic Strike): Treffer wurde bereits vor dem Trigger gemeldet → nicht verzögern
        bool hitAlreadyShown = ability.ReplacesAutoAttack;
        var fx = new PendingFx
        {
            Type = "spell", Icon = ability.IconUrl, Name = ability.Name, Melee = melee,
            Debuff = ability.IsDebuff, DebuffOnly = ability.IsDebuff && !ability.IsDamage, Stun = ability.DebuffIsStun,
            Duration = melee ? MeleeFlightMs : SpellFlightMs,
            Crit = hitAlreadyShown && _lastEnemyHitCrit,
        };
        AddFx(fx);
        if (!hitAlreadyShown)
            _activeSpellFx = fx;
    }

    /// <summary>Buff-, Heil-, Schild- und Gestalt-Effekt am Spieler.</summary>
    private void QueueBuff(Ability ability)
    {
        string? kind = ability switch
        {
            { IsPetSummon: true } or { IsLifeTap: true } => null,
            { IsShield: true }  => "shield",
            { IsHealing: true } => "heal",
            { IsForm: true }    => "form",
            { IsBuff: true }    => "buff",
            _ => null,
        };
        if (kind is null) return;
        AddFx(new PendingFx { Type = "buff", Kind = kind, Icon = ability.IconUrl, Name = ability.Name });
    }

    /// <summary>
    /// Leitet aus einer Kampfzahl den passenden Effekt ab und liefert die Verzögerung (ms),
    /// mit der die Zahl erscheinen soll, damit sie mit dem Einschlag zusammenfällt.
    /// </summary>
    private int TakeHitDelay(CombatText ct)
    {
        if (ct.Kind == CombatTextKind.Heal) return 0;

        if (ct.Target == CombatTextTarget.Enemy && ct.Kind == CombatTextKind.Ability)
        {
            _lastEnemyHitCrit = ct.IsCrit;
            if (_activeSpellFx is not { } spell) return 0;
            lock (_pendingFx)
                spell.Crit |= ct.IsCrit;
            return spell.Duration;
        }

        if (_selfRef is null) return 0;

        if (ct.Target == CombatTextTarget.Enemy)   // Auto-Attack des Spielers
        {
            _lastEnemyHitCrit = ct.IsCrit;
            var (style, duration) = PlayerSwingStyle();
            AddFx(new PendingFx { Type = "swing", Attacker = "player", Style = style, Crit = ct.IsCrit, Duration = duration });
            return duration;
        }

        if (_lifeTapPending)
        {
            _lifeTapPending = false;
            return 0;
        }

        // Treffer des Gegners am Spieler
        AddFx(new PendingFx { Type = "swing", Attacker = "enemy", Style = EnemySwingStyle(), Crit = ct.IsCrit, Duration = SwingImpactMs });
        return SwingImpactMs;
    }

    /// <summary>Darstellung der Spieler-Auto-Attack je nach Waffe bzw. Druidengestalt.</summary>
    private (string Style, int DurationMs) PlayerSwingStyle()
    {
        if (_engine.Player.ActiveForm is DruidForm.Bear or DruidForm.Cat)
            return ("claw", SwingImpactMs);

        return _engine.Player.EquippedWeapon?.Type switch
        {
            WeaponType.Bow    => ("arrow", ArrowFlightMs),
            WeaponType.Wand   => ("wand", WandFlightMs),
            WeaponType.Axe    => ("chop", SwingImpactMs),
            WeaponType.Dagger => ("stab", SwingImpactMs),
            WeaponType.Mace or WeaponType.Staff => ("blunt", SwingImpactMs),
            WeaponType.Sword  => ("slash", SwingImpactMs),
            _                 => ("blunt", SwingImpactMs),   // unbewaffnet: Faustschlag
        };
    }

    /// <summary>Darstellung des Gegnerangriffs je nach Kreaturtyp.</summary>
    private string EnemySwingStyle() => _engine.CurrentEnemy?.Type switch
    {
        "Bear" or "Wolf" or "Dragon" or "Cat" or "Raptor" or "Hyena" or "Bird" or "Demon" => "claw",
        "Scorpid" or "Spider" or "Insect" or "Basilisk" => "stab",
        "Boar" or "Kodo" or "Gorilla" or "Crocolisk" or "Elemental" => "blunt",
        "Ooze" => "splash",
        _ => "slash",
    };

    /// <summary>Icon des Gegner-Swing-Timers passend zur Angriffsart.</summary>
    private string EnemySwingIconUrl() => "https://wow.zamimg.com/images/wow/icons/large/" + EnemySwingStyle() switch
    {
        "claw"   => "ability_druid_rake.jpg",
        "stab"   => "ability_backstab.jpg",
        "blunt"  => "inv_mace_01.jpg",
        "splash" => "inv_misc_slime_01.jpg",
        _        => "inv_sword_04.jpg",
    };


    /// <summary>Inline-Style, der die Kampfzahl bis zum Einschlag unsichtbar hält.</summary>
    private static string FloatDelayStyle(CombatFloat f) =>
        f.DelayMs > 0 ? $" animation-delay: {f.DelayMs}ms; animation-fill-mode: both;" : "";

    // Am Ende jedes Engine-Ticks: gesammelte Effekte in einem Aufruf an den Browser schicken.
    private void FlushProjectiles()
    {
        _activeSpellFx = null;
        _lifeTapPending = false;
        PendingFx[] batch;
        lock (_pendingFx)
        {
            if (_pendingFx.Count == 0) return;
            batch = _pendingFx.TakeLast(MaxFxPerTick).ToArray();
            _pendingFx.Clear();
        }

        var now = DateTime.UtcNow;
        if (!_running || now - _lastFlush < MinFlushInterval) return;
        _lastFlush = now;

        _ = InvokeAsync(async () =>
        {
            try
            {
                await _js.InvokeVoidAsync("spellFx.play", (object)batch);
            }
            catch (Exception e) when (e is JSDisconnectedException or JSException or TaskCanceledException)
            {
                // Effekt ist rein kosmetisch – Fehler ignorieren
            }
        });
    }
}
