// Spell scroll: most recently triggered abilities fly upwards as icons to the left of the player.
using Models;

namespace WoWClassicIdle.Components.Pages;

public partial class IdleFighter
{
    private record SpellFloat(int Id, Ability Ability, DateTime Created);

    private const string AutoAttackFallbackIcon = "https://wow.zamimg.com/images/wow/icons/large/inv_sword_04.jpg";
    private const int MaxSpellFloats = 4; // at high game speed only show the latest icons
    private static readonly TimeSpan SpellFloatLifetime = TimeSpan.FromSeconds(1.5); // = CSS-Animation
    private readonly List<SpellFloat> _spellFloats = new();
    private int _nextSpellFloatId;

    // Timer thread! Rendering happens via the UI timer.
    private void OnAbilityTriggered(Ability ability)
    {
        lock (_spellFloats)
        {
            _spellFloats.Add(new SpellFloat(Interlocked.Increment(ref _nextSpellFloatId), ability, DateTime.UtcNow));
            if (_spellFloats.Count > MaxSpellFloats)
                _spellFloats.RemoveRange(0, _spellFloats.Count - MaxSpellFloats);
        }
        QueueProjectile(ability);
    }

    private void CleanupOldSpellFloats()
    {
        var cutoff = DateTime.UtcNow - SpellFloatLifetime;
        lock (_spellFloats)
            _spellFloats.RemoveAll(s => s.Created < cutoff);
    }

    private SpellFloat[] SnapshotSpellFloats()
    {
        lock (_spellFloats) return _spellFloats.ToArray();
    }
}
