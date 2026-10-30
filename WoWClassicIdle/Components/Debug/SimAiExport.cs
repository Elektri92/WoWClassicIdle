// Debug/SimAiExport.cs – only compiled in the DEBUG build (see .csproj)
namespace DebugTools;

using System.Globalization;
using System.Text;
using Models;

/// <summary>
/// Markdown export of the simulation results for AI chats: real pipe tables, numbers formatted invariantly,
/// builds (talents with measured impact, loadout) and damage breakdown per run.
/// </summary>
public static class SimAiExport
{
    private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

    private const string MethodNote =
        "Talents and loadout are chosen per run by measured XP/h (10-minute trial fights, averaged over 3 fixed seeds; " +
        "XP/h combines damage, survival, mana and the death XP penalty). Talent impact = XP/h with the point vs. without it. " +
        "Gear: best green items available at the level, measured by XP/h; no gear drops.";

    // ─── Level Benchmark ─────────────────────────────────────────────────────

    public static string Benchmark(IReadOnlyList<SimulationResult> results, double minutesPerRun, int outlierPercent, IReadOnlyList<string> outlierLines)
    {
        if (results.Count == 0) return "";
        var sb     = new StringBuilder();
        var levels = results.Select(r => r.Level).Distinct().OrderBy(l => l).ToList();
        var sorted = results.OrderBy(r => r.Class.ToString()).ThenBy(r => r.SpecIndex).ThenBy(r => r.Level).ToList();

        sb.AppendLine("# WoW Classic Idle – Simulation: Level Benchmark");
        sb.AppendLine($"- Classes: {string.Join(", ", sorted.Select(r => r.Class).Distinct())}");
        sb.AppendLine($"- Levels: {levels.First()}–{levels.Last()} (each level played through from 0 XP with the death XP penalty, repeated up to at least {Num(minutesPerRun, "0")} min; – under Min to level = never finished within 50 h) · outlier threshold: ±{outlierPercent}% vs. level median");
        sb.AppendLine($"- Method: {MethodNote}");
        sb.AppendLine();

        sb.AppendLine("## Key metrics");
        Table(sb, ["Class", "Spec", "Lv", "DPS", "Auto %", "Ability %", "DTPS", "HPS", "Kills/min", "Deaths/h", "K/D", "XP/h", "Copper/h", "Min to level"],
            sorted.Select(r => new[]
            {
                r.Class.ToString(), r.SpecName, r.Level.ToString(Inv),
                Num(r.Dps, "0.00"), Pct(r.Dps > 0 ? r.AutoDps / r.Dps : 0), Pct(r.Dps > 0 ? r.AbilityDps / r.Dps : 0),
                Num(r.Dtps, "0.00"), Num(r.Hps, "0.00"), Num(r.KillsPerMin, "0.00"), Num(r.DeathsPerHour, "0.0"),
                Num(r.KillsPerDeath, "0.0"), Num(r.XpPerHour, "0"), Num(r.CopperPerHour, "0"), Num(r.MinutesToLevel, "0.0"),
            }));
        sb.AppendLine();

        sb.AppendLine($"## Outliers (≥ {outlierPercent}% from level median; + = better)");
        if (outlierLines.Count == 0) sb.AppendLine("None.");
        foreach (var line in outlierLines) sb.AppendLine($"- {line}");
        sb.AppendLine();

        // Builds only at the highest level per spec – below that they are subsets of the same tree
        sb.AppendLine("## Builds (highest simulated level per spec, best XP/h first)");
        var builds = BestBuilds(results);
        for (int i = 0; i < builds.Count; i++)
        {
            var r = builds[i];
            sb.AppendLine($"### {i + 1}. {r.Class} – {r.SpecName} (Lv {r.Level}) · XP/h {Num(r.XpPerHour, "0")} · DPS {Num(r.Dps, "0.00")} · Deaths/h {Num(r.DeathsPerHour, "0.0")}");
            sb.AppendLine($"- Loadout: {AbilityNames(r.LoadoutIds)}");
            sb.AppendLine($"- Talents: {r.TalentSummary}");
            sb.AppendLine($"- Talent pick order (impact): {PickOrder(r.TalentPicks, withLevels: false)}");
        }
        sb.AppendLine();

        sb.AppendLine("## Damage / healing share per run");
        foreach (var r in sorted)
            sb.AppendLine($"- {r.Class} ({r.SpecName}) Lv {r.Level}: {BreakdownLine(r.Breakdown, r.TotalDamage, r.TotalHealing, r.CombatSeconds)}");

        return sb.ToString();
    }

    // ─── Leveling Race ───────────────────────────────────────────────────────

    public static string Leveling(IReadOnlyList<LevelingResult> results, double maxMinutes, IReadOnlyList<int> milestones)
    {
        if (results.Count == 0) return "";
        var sb     = new StringBuilder();
        var ranked = results.OrderBy(r => r.ReachedTarget ? r.SimSecondsToTarget : double.MaxValue).ToList();
        var first  = ranked[0];

        sb.AppendLine("# WoW Classic Idle – Simulation: Leveling Race");
        sb.AppendLine($"- Classes: {string.Join(", ", results.Select(r => r.Class).Distinct().OrderBy(c => c.ToString()))}");
        sb.AppendLine($"- Levels: {first.LevelFrom} → {first.LevelTo} · cap: {Num(maxMinutes / 60, "0.#")} h game time per spec · DNF = target not reached within cap");
        sb.AppendLine($"- Method: {MethodNote} Talents are re-measured on every level-up.");
        sb.AppendLine();

        sb.AppendLine("## Standings");
        Table(sb, ["Rank", "Class", "Spec", "Result", "Min to target", "Kills", "Deaths", "K/D", "DPS", "Copper/h"],
            ranked.Select((r, i) => new[]
            {
                (i + 1).ToString(Inv), r.Class.ToString(), r.SpecName,
                r.ReachedTarget ? $"Lv {r.LevelTo}" : $"DNF (Lv {r.FinalLevel})",
                r.ReachedTarget ? Num(r.SimSecondsToTarget / 60, "0.0") : "–",
                r.TotalKills.ToString(Inv), r.TotalDeaths.ToString(Inv),
                r.TotalDeaths > 0 ? Num((double)r.TotalKills / r.TotalDeaths, "0.0") : "∞",
                Num(r.Dps, "0.00"), Num(r.CopperPerHour, "0"),
            }));
        sb.AppendLine();

        if (milestones.Count > 0)
        {
            sb.AppendLine("## Minutes of game time to reach level");
            Table(sb, ["Class", "Spec", .. milestones.Select(l => $"Lv {l}")],
                ranked.Select(r => new[] { r.Class.ToString(), r.SpecName }
                    .Concat(milestones.Select(l => r.LevelTimings.FirstOrDefault(t => t.Level == l) is { } t ? Num(t.SimSecondsToReach / 60, "0.0") : "–"))
                    .ToArray()));
            sb.AppendLine();
        }

        sb.AppendLine("## Builds per spec");
        foreach (var r in ranked)
        {
            sb.AppendLine($"### {r.Class} – {r.SpecName} (final Lv {r.FinalLevel})");
            sb.AppendLine($"- Talents: {r.TalentSummary}");
            sb.AppendLine($"- Talent picks (impact): {PickOrder(r.TalentPicks, withLevels: true)}");
            sb.AppendLine($"- Loadouts: {string.Join(" | ", r.LoadoutHistory.Select(l => $"Lv {l.LevelFrom}–{l.LevelTo}: {AbilityNames(l.AbilityIds)}"))}");
            sb.AppendLine($"- Damage / healing share: {BreakdownLine(r.FinalBreakdown, r.TotalDamage, r.TotalHealing, r.SimSecondsTotal)}");
        }

        return sb.ToString();
    }

    // ─── Helpers (also used by the page) ─────────────────────────────────

    /// <summary>Per class/spec the run at the highest level, sorted by XP/h (best first).</summary>
    public static List<SimulationResult> BestBuilds(IEnumerable<SimulationResult> results) =>
        results.GroupBy(r => (r.Class, r.SpecIndex))
            .Select(g => g.OrderByDescending(r => r.Level).First())
            .OrderByDescending(r => r.XpPerHour)
            .ToList();

    /// <summary>Condensed talent choice: consecutive ranks of the same talent with avg. measured impact (NaN = not measured).</summary>
    public sealed record PickGroup(string Text, double AvgGain)
    {
        public string GainText => double.IsNaN(AvgGain) ? "only option" : AvgGain.ToString("+0.0%;-0.0%;0.0%", Inv);
    }

    /// <summary>Chosen talents in order; consecutive ranks of the same talent are merged.</summary>
    public static List<PickGroup> GroupPicks(IReadOnlyList<TalentPick> picks, bool withLevels)
    {
        var groups = new List<PickGroup>();
        for (int i = 0; i < picks.Count; i++)
        {
            int start = i;
            while (i + 1 < picks.Count && picks[i + 1].TalentId == picks[i].TalentId) i++;
            var run   = picks.Skip(start).Take(i - start + 1).ToList();
            string ranks = run.Count == 1 ? $"{run[0].Rank}/{run[0].MaxRanks}" : $"{run[0].Rank}–{run[^1].Rank}/{run[0].MaxRanks}";
            string lv    = !withLevels ? "" : run[0].Level == run[^1].Level ? $"Lv {run[0].Level} " : $"Lv {run[0].Level}–{run[^1].Level} ";
            var gains    = run.Select(p => p.GainPct).Where(g => !double.IsNaN(g)).ToList();
            groups.Add(new PickGroup($"{lv}{run[0].Name} {ranks}", gains.Count > 0 ? gains.Average() : double.NaN));
        }
        return groups;
    }

    public static string PickOrder(IReadOnlyList<TalentPick> picks, bool withLevels) =>
        picks.Count == 0 ? "none" : string.Join(" → ", GroupPicks(picks, withLevels).Select(g => $"{g.Text} ({g.GainText})"));

    public static string AbilityNames(IEnumerable<string> ids) =>
        string.Join(", ", ids.Select(id => ClassAbilityRegistry.FindById(id) is { } a ? (a.Rank > 1 ? $"{a.Name} r{a.Rank}" : a.Name) : id));

    private static string BreakdownLine(IReadOnlyList<MeterRow> rows, long totalDamage, long totalHealing, double seconds) =>
        string.Join(" · ", rows.OrderByDescending(b => b.Total)
            .Select(b => (Row: b, Total: b.Kind == CombatActionKind.Heal ? totalHealing : totalDamage))
            .Where(x => x.Total > 0)
            .Select(x => $"{x.Row.Name} {Pct((double)x.Row.Total / x.Total)} {(x.Row.Kind == CombatActionKind.Heal ? "heal" : "dmg")}"
                       + (seconds > 0 ? $" ({Num(x.Row.Total / seconds, "0.0")}/s)" : "")));

    private static void Table(StringBuilder sb, IReadOnlyList<string> headers, IEnumerable<string[]> rows)
    {
        sb.AppendLine($"| {string.Join(" | ", headers)} |");
        sb.AppendLine($"|{string.Concat(headers.Select(_ => "---|"))}");
        foreach (var row in rows)
            sb.AppendLine($"| {string.Join(" | ", row)} |");
    }

    private static string Num(double v, string fmt) =>
        double.IsNaN(v) ? "–" : double.IsInfinity(v) ? "∞" : v.ToString(fmt, Inv);

    private static string Pct(double v) => v.ToString("0%", Inv);
}
