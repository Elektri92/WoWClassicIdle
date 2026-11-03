// Logic of the /talents page (markup in Talents.razor, styles in wwwroot/css/talents.css)
using Models;

namespace WoWClassicIdle.Components.Pages;

public partial class Talents
{
    private readonly record struct Tier(string CssClass, string Text, string Title);

    private string? _message;
    private bool _messagePositive;

    private CharacterClass ActiveClass => Engine.PlayerClass;
    private TalentState State         => Engine.Player.TalentState;
    private int AvailablePoints       => Engine.Player.GetAvailableTalentPoints(ActiveClass);
    private int TotalSpent            => State.TotalInvested(ActiveClass);
    private string[] TreeNames        => TalentRegistry.GetTreeNames(ActiveClass);

    // Background of the header: current zone, otherwise the starting zone of the race
    private string HeaderTheme =>
        Engine.CurrentCombatZone?.Theme ?? CombatZoneRegistry.StartZoneFor(Engine.Race)?.Theme ?? "grassland";

    protected override void OnInitialized() => Engine.TickUpdated += OnEngineTick;

    private void OnEngineTick() => InvokeAsync(StateHasChanged);

    /// <summary>Fill of the tier rail – reaches the badge of row N at N*5 points.</summary>
    private static double TrackFillPercent(int invested)
    {
        int max = (TalentRegistry.RowCount - 1) * TalentRegistry.PointsPerRow;
        return Math.Round(Math.Min(invested, max) * 100.0 / max, 2);
    }

    /// <summary>Badge of a row on the tier rail: unlocked, next to unlock (points still missing) or locked.</summary>
    private Tier TierInfo(int treeIndex, int row)
    {
        int required = row * TalentRegistry.PointsPerRow;
        int have     = State.PointsBeforeRow(ActiveClass, treeIndex, row);
        string tree  = TreeNames[treeIndex];
        if (have >= required)
            return new("tier-unlocked", required.ToString(), $"Tier {row + 1} unlocked ({required} points in {tree})");

        bool isNext = State.IsRowUnlocked(ActiveClass, treeIndex, row - 1);
        int missing = required - have;
        return isNext
            ? new("tier-next", missing.ToString(), $"{missing} more point{(missing == 1 ? "" : "s")} in {tree} to unlock tier {row + 1}")
            : new("tier-locked", required.ToString(), $"Tier {row + 1} requires {required} points in {tree}");
    }

    /// <summary>Requirement arrows inside the tree (prerequisite in the same column above).</summary>
    private static IEnumerable<(TalentDefinition From, TalentDefinition To)> Arrows(List<TalentDefinition> talents)
    {
        foreach (var talent in talents)
            if (talent.RequiresTalentId is { } reqId
                && talents.FirstOrDefault(t => t.Id == reqId) is { } required
                && required.Column == talent.Column && required.Row < talent.Row)
                yield return (required, talent);
    }

    private static string NodeClass(TalentDefinition talent, int ranks, bool hasMissing, bool canSpend) =>
        (ranks >= talent.MaxRanks ? "talent-maxed"
            : ranks > 0          ? "talent-partial"
            : hasMissing         ? "talent-locked"
            : "talent-empty")
        + (canSpend ? " talent-clickable" : "");

    private void OnTalentClick(TalentDefinition talent)
    {
        var blocker = State.InvestBlocker(talent, AvailablePoints, Engine.Player.Level);
        if (blocker is null && Engine.InvestTalentPoint(talent.Id))
            ShowMessage($"{talent.Name}: rank {State.GetRanks(talent.Id)}/{talent.MaxRanks}.", true);
        else
            ShowMessage(blocker ?? "Cannot learn this talent yet.", false);
    }

    private void OnTalentRightClick(TalentDefinition talent)
    {
        if (State.GetRanks(talent.Id) == 0) return;
        var error = Engine.RemoveTalentPoint(talent.Id);
        if (error is null)
            ShowMessage($"{talent.Name}: rank {State.GetRanks(talent.Id)}/{talent.MaxRanks}.", true);
        else
            ShowMessage(error, false);
    }

    private void OnReset()
    {
        if (TotalSpent == 0) return;
        Engine.ResetTalents(ActiveClass);
        ShowMessage($"All {ActiveClass} talents reset.", true);
    }

    private void ShowMessage(string message, bool positive)
    {
        _message         = message;
        _messagePositive = positive;
        StateHasChanged();
    }

    public void Dispose() => Engine.TickUpdated -= OnEngineTick;
}
