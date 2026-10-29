// Logic of the /achievements page (markup in Achievements.razor, styles in wwwroot/css/achievements.css)
using System.Globalization;
using Microsoft.AspNetCore.Components;
using Models;
using Services;

namespace WoWClassicIdle.Components.Pages;

public partial class Achievements : IDisposable
{
    private enum AchievementFilter { All, Completed, Incomplete }

    [Inject] private IAchievementService _achievements { get; set; } = default!;

    // null = overview (summary)
    private AchievementCategory? _category;
    // Subcategory within "Classes"
    private CharacterClass? _classFilter;
    private AchievementFilter _filter = AchievementFilter.All;
    private string _search = "";
    private Timer? _refreshTimer;

    private bool IsSearching => !string.IsNullOrWhiteSpace(_search);

    protected override void OnInitialized()
    {
        _achievements.AchievementEarned += OnAchievementEarned;
        // Progress bars keep up without re-rendering on every 100 ms tick
        _refreshTimer = new Timer(_ => InvokeAsync(StateHasChanged), null, 1000, 1000);
    }

    private void OnAchievementEarned(AchievementDefinition _) => InvokeAsync(StateHasChanged);

    private void ShowSummary()
    {
        _category = null;
        _classFilter = null;
        _search = "";
    }

    private void ShowCategory(AchievementCategory cat)
    {
        _category = cat;
        _classFilter = null;
        _search = "";
    }

    private void ShowClass(CharacterClass cls)
    {
        _category = AchievementCategory.Classes;
        _classFilter = cls;
        _search = "";
    }

    private (int Earned, int Total) CountFor(Func<AchievementDefinition, bool> predicate)
    {
        var defs = _achievements.AllAchievements.Where(predicate).ToList();
        return (defs.Count(d => _achievements.IsEarned(d.Id)), defs.Count);
    }

    private bool PassesFilter(AchievementDefinition def) => _filter switch
    {
        AchievementFilter.Completed  => _achievements.IsEarned(def.Id),
        AchievementFilter.Incomplete => !_achievements.IsEarned(def.Id),
        _                            => true,
    };

    // List of the current view; for classes grouped by class (key null = no heading)
    private List<IGrouping<CharacterClass?, AchievementDefinition>> GetVisibleGroups()
    {
        IEnumerable<AchievementDefinition> defs = _achievements.AllAchievements;

        if (IsSearching)
        {
            string q = _search.Trim();
            defs = defs.Where(d => d.Title.Contains(q, StringComparison.OrdinalIgnoreCase)
                                || d.Description.Contains(q, StringComparison.OrdinalIgnoreCase));
        }
        else
        {
            defs = defs.Where(d => d.Category == _category);
            if (_classFilter is not null)
                defs = defs.Where(d => d.Class == _classFilter);
        }

        defs = defs.Where(PassesFilter);

        bool groupByClass = !IsSearching && _category == AchievementCategory.Classes && _classFilter is null;
        return defs.GroupBy(d => groupByClass ? d.Class : null).ToList();
    }

    private List<(AchievementDefinition Def, EarnedAchievement Earned)> GetRecentEarned() =>
        _achievements.EarnedAchievements
            .OrderByDescending(e => e.EarnedAt)
            .Select(e => (Def: _achievements.AllAchievements.FirstOrDefault(a => a.Id == e.AchievementId), Earned: e))
            .Where(x => x.Def is not null)
            .Take(4)
            .Select(x => (x.Def!, x.Earned))
            .ToList();

    private static string Pct(double pct) => pct.ToString("0.#", CultureInfo.InvariantCulture) + "%";

    private static string Num(long n) => n.ToString("N0", CultureInfo.InvariantCulture);

    public void Dispose()
    {
        _achievements.AchievementEarned -= OnAchievementEarned;
        _refreshTimer?.Dispose();
    }
}
