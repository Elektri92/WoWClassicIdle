// Combat effects (wwwroot/js/spellfx/*): abilities fly from the player to the enemy, auto-attacks of player
// and enemy are shown as a lunge/projectile with a weapon- or creature-dependent hit.
// Buffs light up on the player, debuffs visibly land on the enemy; both icons fly into their bar.
// The corresponding damage number only appears on impact.
using Models;
using Microsoft.JSInterop;

namespace WoWClassicIdle.Components.Pages;

public partial class IdleFighter
{
    /// <summary>An effect for spellFx.play – field names are serialized to JS as camelCase.</summary>
    private sealed class PendingFx
    {
        public string Type { get; init; } = "spell";       // spell | swing | buff
        public string? Attacker { get; init; }              // swing: player | enemy
        public string? Style { get; init; }                 // swing: slash | chop | stab | blunt | claw | splash | arrow | wand
        public string? Icon { get; init; }
        public string? Name { get; init; }                  // spell/buff: matched against the icon in the buff/debuff bar
        public string? Kind { get; init; }                  // buff: buff | heal | shield | form
        public bool Debuff { get; init; }                   // spell: applies a debuff to the enemy
        public bool DebuffOnly { get; init; }               // spell: pure debuff without damage
        public bool Stun { get; init; }
        public bool Melee { get; init; }
        public bool Crit { get; set; }
        public int Duration { get; init; }                  // ms until the hit
    }

    private const int SpellFlightMs = 450;
    private const int MeleeFlightMs = 240;
    private const int SwingImpactMs = 110;      // lunge until the hit
    private const int ArrowFlightMs = 260;
    private const int WandFlightMs = 320;
    private const int MaxFxPerTick = 3;         // at high game speed only show the latest effects
    private static readonly TimeSpan MinFlushInterval = TimeSpan.FromMilliseconds(80);
    private readonly List<PendingFx> _pendingFx = new();
    private PendingFx? _activeSpellFx;          // ability just triggered whose hits are still being reported
    private DateTime _lastFlush = DateTime.MinValue;
    private bool _lastEnemyHitCrit;
    private bool _lifeTapPending;               // Life Tap reports its HP cost as a "hit" on the player

    private static bool IsOffensive(Ability a) =>
        (a.IsDamage || a.IsDebuff) && !a.IsPetSummon && !a.IsLifeTap;

    private void AddFx(PendingFx fx)
    {
        lock (_pendingFx)
            _pendingFx.Add(fx);
    }

    // Timer thread! Called from OnAbilityTriggered.
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
        // On-next-swing (Heroic Strike): the hit was already reported before the trigger → don't delay
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

    /// <summary>Buff, heal, shield and form effect on the player.</summary>
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
    /// Derives the matching effect from a combat number and returns the delay (ms)
    /// with which the number should appear so it coincides with the impact.
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

        if (ct.Target == CombatTextTarget.Enemy)   // Auto-attack of the player
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

        // Hit of the enemy on the player
        AddFx(new PendingFx { Type = "swing", Attacker = "enemy", Style = EnemySwingStyle(), Crit = ct.IsCrit, Duration = SwingImpactMs });
        return SwingImpactMs;
    }

    /// <summary>Presentation of the player auto-attack depending on the weapon or Druid form.</summary>
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
            _                 => ("blunt", SwingImpactMs),   // unarmed: punch
        };
    }

    /// <summary>Presentation of the enemy attack depending on the creature type.</summary>
    private string EnemySwingStyle() => _engine.CurrentEnemy?.Type switch
    {
        "Bear" or "Wolf" or "Dragon" or "Cat" or "Raptor" or "Hyena" or "Bird" or "Demon" => "claw",
        "Scorpid" or "Spider" or "Insect" or "Basilisk" => "stab",
        "Boar" or "Kodo" or "Gorilla" or "Crocolisk" or "Elemental" => "blunt",
        "Ooze" => "splash",
        _ => "slash",
    };

    /// <summary>Icon of the enemy swing timer matching the attack type.</summary>
    private string EnemySwingIconUrl() => "https://wow.zamimg.com/images/wow/icons/large/" + EnemySwingStyle() switch
    {
        "claw"   => "ability_druid_rake.jpg",
        "stab"   => "ability_backstab.jpg",
        "blunt"  => "inv_mace_01.jpg",
        "splash" => "inv_misc_slime_01.jpg",
        _        => "inv_sword_04.jpg",
    };


    /// <summary>Inline style that keeps the combat number invisible until impact.</summary>
    private static string FloatDelayStyle(CombatFloat f) =>
        f.DelayMs > 0 ? $" animation-delay: {f.DelayMs}ms; animation-fill-mode: both;" : "";

    // At the end of each engine tick: send the collected effects to the browser in one call.
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
                // The effect is purely cosmetic – ignore errors
            }
        });
    }
}
