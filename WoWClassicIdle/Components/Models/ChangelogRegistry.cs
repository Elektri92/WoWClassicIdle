using System.Text.Json;

namespace Models;

public sealed record ChangelogSection(string Category, List<string> Changes);

public sealed record ChangelogEntry(string Version, string Date, List<ChangelogSection> Sections);

// Release notes embedded in the binary, newest first. CI adds an entry per build (.github/scripts/update_changelog.py).
public static class ChangelogRegistry
{
    private static readonly Lazy<IReadOnlyList<ChangelogEntry>> _entries = new(Load);

    public static IReadOnlyList<ChangelogEntry> Entries => _entries.Value;

    // The seen version comes from the user-editable save file – anything unparsable counts as never seen
    public static bool IsNewerThan(string version, string? seenVersion) =>
        !Version.TryParse(seenVersion, out var seen) || !Version.TryParse(version, out var current) || current > seen;

    private static IReadOnlyList<ChangelogEntry> Load()
    {
        using var stream = typeof(ChangelogRegistry).Assembly.GetManifestResourceStream("changelog.json")
            ?? throw new InvalidOperationException("Embedded resource changelog.json is missing.");
        return JsonSerializer.Deserialize<List<ChangelogEntry>>(stream, new JsonSerializerOptions(JsonSerializerDefaults.Web))
            ?? throw new InvalidOperationException("Embedded resource changelog.json is empty.");
    }
}
