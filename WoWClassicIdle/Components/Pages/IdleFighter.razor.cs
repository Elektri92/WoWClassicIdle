// Neue Systeme als eigene Partial-Datei anlegen; Dateien möglichst < 300 Zeilen halten.
using Models;
using Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace WoWClassicIdle.Components.Pages;

public partial class IdleFighter
{
    private bool _running;
    private double _speedMultiplier = 1.0;
    private HashSet<string> _multiRankNames = new();
    private double _gameElapsedSeconds = 0.0;
    private int _debugTargetLevel = 1;
    private DateTime _lastRealTick = DateTime.UtcNow;
    private System.Timers.Timer? _uiTimer;
    private DotNetObjectReference<IdleFighter>? _selfRef;
    private IJSObjectReference? _visibilityHandle;

    private record CombatFloat(int Id, CombatTextKind Kind, bool OnPlayer, int Amount, int XOffset, int YOffset, DateTime Created, bool IsCrit = false);
    private record AbilityNotification(int Id, Ability Ability, DateTime Created);

    private static readonly TimeSpan NotificationLifetime = TimeSpan.FromSeconds(8);
    private readonly List<AbilityNotification> _abilityNotifications = new();
    private int _nextNotifId;

    private const string SpiritHealerIconUrl = "https://wow.zamimg.com/images/wow/icons/large/spell_holy_resurrection.jpg";
    private const int MaxFloatsPerFrame = 12; // bei hoher Spielgeschwindigkeit nicht das Frame zuschütten
    private static readonly TimeSpan FloatLifetime = TimeSpan.FromSeconds(1.7);
    private readonly List<CombatFloat> _playerFloats = new();
    private readonly List<CombatFloat> _enemyFloats = new();
    private int _nextFloatId;
    private static readonly Random _rng = new();
    private Character? _subscribedPlayer;
    private CharacterClass _lastSeenClass = default;

    protected override void OnInitialized()
    {
        _engine.TickUpdated += OnTick;
        _engine.CombatTextRaised += OnCombatText;
        _engine.AbilityUnlocked += OnAbilityUnlocked;
        ApplySavedLoadout();
        _running = _engine.IsGameRunning;
        StartUiTimer();
    }

    // Fight only runs while this page is open and the browser tab is visible.
    // Started here (not in OnInitialized) so prerendering doesn't kick off the engine.
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender) return;
        _selfRef = DotNetObjectReference.Create(this);
        try
        {
            _visibilityHandle = await _js.InvokeAsync<IJSObjectReference>("pageVisibility.register", _selfRef);
            SetEngineRunning(await _js.InvokeAsync<bool>("pageVisibility.isVisible"));
        }
        catch (JSDisconnectedException)
        {
        }
    }

    [JSInvokable]
    public void OnVisibilityChanged(bool visible)
    {
        SetEngineRunning(visible);
        StateHasChanged();
    }

    private void SetEngineRunning(bool run)
    {
        if (run && !_engine.IsGameRunning)
        {
            _engine.Start();
            _lastRealTick = DateTime.UtcNow;
        }
        else if (!run && _engine.IsGameRunning)
        {
            _engine.Stop();
        }

        _running = _engine.IsGameRunning;
    }

    private void DetectClassChange()
    {
        var current = _engine.Player.ActiveClass;
        if (current != _lastSeenClass)
        {
            _lastSeenClass = current;
            _engine.Player.SetClass(current); // erzwingt frisches Loadout auf Character-Basis
            ApplySavedLoadout();
        }
    }

    private void ApplySavedLoadout()
    {
        var cls = _engine.Player.ActiveClass;
        var loadout = _loadoutService.GetLoadout(cls, _engine.Player.Level);
        _engine.Player.ApplyLoadout(loadout);
        _multiRankNames = ClassAbilityRegistry.GetAbilitiesFor(cls)
            .GroupBy(a => a.Name)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToHashSet();
    }

    private void StartUiTimer()
    {
        _uiTimer = new System.Timers.Timer(100);
        _uiTimer.Elapsed += (_, _) =>
        {
            var now = DateTime.UtcNow;
            var delta = (now - _lastRealTick).TotalSeconds;
            _lastRealTick = now;
            if (_running)
                _gameElapsedSeconds += delta * _speedMultiplier;
            CleanupOldFloats();
            _ = InvokeAsync(StateHasChanged);
        };
        _uiTimer.AutoReset = true;
        _uiTimer.Start();
    }

    private void OnTick()
    {
        DetectPlayerChange();
        DetectClassChange();
        _ = InvokeAsync(StateHasChanged);
    }

    private void DetectPlayerChange()
    {
        if (_engine.Player != _subscribedPlayer)
        {
            _subscribedPlayer = _engine.Player;
            ApplySavedLoadout();
        }
    }

    private void OnAbilityUnlocked(Ability ability)
    {
        lock (_abilityNotifications)
            _abilityNotifications.Add(new AbilityNotification(
                Interlocked.Increment(ref _nextNotifId), ability, DateTime.UtcNow));

        _ = InvokeAsync(() =>
        {
            ApplySavedLoadout();
            StateHasChanged();
        });
    }

    private void EquipAndDismiss(AbilityNotification n)
    {
        var cls = _engine.Player.ActiveClass;
        var loadout = _loadoutService.GetLoadout(cls, _engine.Player.Level);
        if (!loadout.IsEquipped(n.Ability.Id))
        {
            var pool = ClassAbilityRegistry.GetAbilitiesFor(cls);
            // Gleicher Spell-Name (anderer Rank): vorhandenen Rank in-place ersetzen
            var sameSpell = pool.FirstOrDefault(a => a.Name == n.Ability.Name
                                                  && a.Id != n.Ability.Id
                                                  && loadout.IsEquipped(a.Id));
            if (sameSpell != null)
            {
                int idx = loadout.EquippedAbilityIds.IndexOf(sameSpell.Id);
                loadout.SetSlot(idx, n.Ability.Id);
            }
            else if (!string.IsNullOrEmpty(n.Ability.ExclusiveGroup))
            {
                // Exklusiv-Gruppe: vorhandene Ability derselben Gruppe in-place ersetzen
                var conflict = pool.FirstOrDefault(a => a.ExclusiveGroup == n.Ability.ExclusiveGroup
                                                      && loadout.IsEquipped(a.Id));
                if (conflict != null)
                {
                    int idx = loadout.EquippedAbilityIds.IndexOf(conflict.Id);
                    loadout.SetSlot(idx, n.Ability.Id);
                }
                else if (loadout.Count < ClassAbilityRegistry.MaxSlotsPerClass)
                {
                    loadout.EquippedAbilityIds.Add(n.Ability.Id);
                }
            }
            else if (loadout.Count < ClassAbilityRegistry.MaxSlotsPerClass)
            {
                loadout.EquippedAbilityIds.Add(n.Ability.Id);
            }
            _loadoutService.SaveLoadout(loadout);
            _engine.Player.ApplyLoadout(loadout);
        }
        lock (_abilityNotifications)
            _abilityNotifications.RemoveAll(x => x.Id == n.Id);
    }

    private void DismissNotification(int id)
    {
        lock (_abilityNotifications)
            _abilityNotifications.RemoveAll(n => n.Id == id);
    }

    private AbilityNotification[] SnapshotNotifications()
    {
        lock (_abilityNotifications) return _abilityNotifications.ToArray();
    }

    private void OnCombatText(CombatText ct)
    {
        bool onPlayer = ct.Target == CombatTextTarget.Player;
        var list = onPlayer ? _playerFloats : _enemyFloats;
        lock (list)
        {
            list.Add(new CombatFloat(
                Interlocked.Increment(ref _nextFloatId), ct.Kind, onPlayer, ct.Amount,
                _rng.Next(-45, 46), _rng.Next(0, 22), DateTime.UtcNow, ct.IsCrit));
            if (list.Count > MaxFloatsPerFrame)
                list.RemoveRange(0, list.Count - MaxFloatsPerFrame);
        }
    }

    private void CleanupOldFloats()
    {
        var cutoff = DateTime.UtcNow - FloatLifetime;
        lock (_playerFloats)
            _playerFloats.RemoveAll(f => f.Created < cutoff);
        lock (_enemyFloats)
            _enemyFloats.RemoveAll(f => f.Created < cutoff);

        var notifCutoff = DateTime.UtcNow - NotificationLifetime;
        lock (_abilityNotifications)
            _abilityNotifications.RemoveAll(n => n.Created < notifCutoff);
    }

    private static CombatFloat[] SnapshotFloats(List<CombatFloat> list)
    {
        lock (list) return list.ToArray();
    }

    private static string FloatCssClass(CombatFloat f)
    {
        var baseClass = f.Kind switch
        {
            CombatTextKind.Heal => "ct-heal",
            _ when f.OnPlayer => "ct-taken",
            CombatTextKind.Ability => "ct-ability",
            _ => "ct-auto",
        };
        return f.IsCrit ? $"{baseClass} ct-crit" : baseClass;
    }

    private static string FloatText(CombatFloat f)
    {
        var text = f.Kind switch
        {
            CombatTextKind.Heal => $"+{f.Amount}",
            _ when f.OnPlayer => $"-{f.Amount}",
            _ => f.Amount.ToString(),
        };
        return f.IsCrit ? $"{text}!" : text;
    }

    private void SetSpeed(double s)
    {
        _speedMultiplier = s;
        _engine.SetSpeed(s);
        _lastRealTick = DateTime.UtcNow;
    }

    private void SetDebugLevel()
    {
        _engine.SetPlayerLevel(_debugTargetLevel);
        ApplySavedLoadout();
    }

    private static string FormatElapsed(double totalSeconds)
    {
        var ts = TimeSpan.FromSeconds(totalSeconds);
        return $"{(int)ts.TotalHours:00}:{ts.Minutes:00}:{ts.Seconds:00}";
    }

    private void ResetMeter() => _engine.Meter.Reset();

    private static string FormatNumber(long value) => value switch
    {
        >= 1_000_000 => $"{value / 1_000_000.0:0.##}M",
        >= 10_000 => $"{value / 1_000.0:0.#}k",
        _ => value.ToString()
    };

    private static string FormatBuffTime(double seconds) =>
        seconds >= 60 ? $"{(int)seconds / 60}m" : $"{Math.Ceiling(seconds):0}s";

    private static int CalcPct(int current, int max) =>
        max <= 0 ? 0 : Math.Clamp(current * 100 / max, 0, 100);

    public async ValueTask DisposeAsync()
    {
        _engine.TickUpdated -= OnTick;
        _uiTimer?.Stop();
        _uiTimer?.Dispose();
        _engine.CombatTextRaised -= OnCombatText;
        _engine.AbilityUnlocked -= OnAbilityUnlocked;

        // Leaving the page stops the fight. Prerendered instances never started it.
        if (_selfRef != null)
            _engine.Stop();

        if (_visibilityHandle != null)
        {
            try
            {
                await _visibilityHandle.InvokeVoidAsync("dispose");
                await _visibilityHandle.DisposeAsync();
            }
            catch (JSDisconnectedException)
            {
            }
        }

        _selfRef?.Dispose();
    }
}
