using System.Net.Http.Headers;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Services;

public enum UpdateStatus { Idle, Checking, UpToDate, Downloading, Restarting, Error }

// Singleton – prüft beim App-Start auf eine neue Version und ersetzt die Binary automatisch.
// Fortschritt wird per Changed-Event an die UI gemeldet.
public class UpdateService
{
    private const string ManifestUrl = "https://raw.githubusercontent.com/Elektri92/WoWClassicIdle/master/version.json";

    private bool _hasStarted;

    public UpdateStatus Status      { get; private set; } = UpdateStatus.Idle;
    public long   DownloadedBytes   { get; private set; }
    public long   TotalBytes        { get; private set; }
    public string? RemoteVersion    { get; private set; }
    public string? ErrorMessage     { get; private set; }

    // Gibt die eingebettete Assembly-Version zurück (z.B. "0.0.15")
    public static string AppVersion
    {
        get
        {
            var v = Assembly.GetEntryAssembly()?.GetName().Version;
            return v is null ? "dev" : $"{v.Major}.{v.Minor}.{v.Build}";
        }
    }

    // Feuert bei jedem Zustandswechsel – UI-Komponenten abonnieren und rufen StateHasChanged auf
    public event Action? Changed;

    // Startet den Update-Check einmalig im Hintergrund
    public void StartCheckInBackground()
    {
        if (_hasStarted) return;
        _hasStarted = true;
        CleanupStagingDir();
        _ = Task.Run(CheckAndApplyAsync);
    }

    // Löscht übrig gebliebene Staging-Dateien vom letzten Update
    private static void CleanupStagingDir()
    {
        try
        {
            var stagingDir = Path.Combine(Path.GetDirectoryName(Environment.ProcessPath!)!, "_update");
            if (Directory.Exists(stagingDir))
            {
                Directory.Delete(stagingDir, recursive: true);
                Console.WriteLine("[Update] Cleaned up staging directory.");
            }
        }
        catch { }
    }

    private async Task CheckAndApplyAsync()
    {
        try
        {
            SetStatus(UpdateStatus.Checking);
            Console.WriteLine($"[Update] Checking for updates... (local: v{AppVersion}, url: {ManifestUrl})");

            using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(15) };
            http.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue { NoCache = true, NoStore = true };

            var json     = await http.GetStringAsync(ManifestUrl);
            var manifest = JsonSerializer.Deserialize<VersionManifest>(json);

            if (manifest is null)
            {
                Console.WriteLine("[Update] Invalid manifest – skipping.");
                SetStatus(UpdateStatus.UpToDate);
                return;
            }

            if (!Version.TryParse(manifest.Version, out var remoteVersion))
            {
                Console.WriteLine($"[Update] Cannot parse remote version '{manifest.Version}' – skipping.");
                SetStatus(UpdateStatus.UpToDate);
                return;
            }

            var localVersion = Assembly.GetEntryAssembly()?.GetName().Version ?? new Version(0, 0, 0);

            if (remoteVersion <= localVersion)
            {
                Console.WriteLine($"[Update] Up to date (v{localVersion}).");
                SetStatus(UpdateStatus.UpToDate);
                return;
            }

            RemoteVersion = manifest.Version;
            Console.WriteLine($"[Update] New version {remoteVersion} available (local: {localVersion}).");

            var downloadUrl = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? manifest.WindowsUrl
                            : RuntimeInformation.IsOSPlatform(OSPlatform.OSX)     ? manifest.MacUrl
                            :                                                        manifest.LinuxUrl;

            if (string.IsNullOrEmpty(downloadUrl))
            {
                Console.WriteLine("[Update] No download URL for this platform – skipping.");
                SetStatus(UpdateStatus.UpToDate);
                return;
            }

            var appPath    = Environment.ProcessPath!;
            var appDir     = Path.GetDirectoryName(appPath)!;
            var stagingDir = Path.Combine(appDir, "_update");
            Directory.CreateDirectory(stagingDir);
            var stagedBinary = Path.Combine(stagingDir, Path.GetFileName(appPath));

            Console.WriteLine($"[Update] Downloading to {stagedBinary} ...");
            SetStatus(UpdateStatus.Downloading);

            using var response = await http.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();

            TotalBytes      = response.Content.Headers.ContentLength ?? -1;
            DownloadedBytes = 0;
            Notify();

            await using (var srcStream  = await response.Content.ReadAsStreamAsync())
            await using (var destStream = File.Create(stagedBinary))
            {
                var buffer = new byte[81920];
                int read;
                while ((read = await srcStream.ReadAsync(buffer)) > 0)
                {
                    await destStream.WriteAsync(buffer.AsMemory(0, read));
                    DownloadedBytes += read;
                    Notify();
                }
            }

            Console.WriteLine($"[Update] Download complete ({DownloadedBytes / 1024.0 / 1024.0:F1} MB). Launching restart...");
            SetStatus(UpdateStatus.Restarting);

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                LaunchWindowsRestart(stagedBinary, appPath);
            else
                LaunchUnixRestart(stagedBinary, appPath, appDir);

            await Task.Delay(1500);
            Environment.Exit(0);
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            Console.Error.WriteLine($"[Update] Error: {ex.Message}");
            SetStatus(UpdateStatus.Error);
        }
    }

    private void SetStatus(UpdateStatus status) { Status = status; Notify(); }
    private void Notify() => Changed?.Invoke();

    private static void LaunchWindowsRestart(string stagedBinary, string appPath)
    {
        var bat = Path.Combine(Path.GetTempPath(), $"wowcidle_update_{Guid.NewGuid():N}.bat");
        var stagingDir = Path.GetDirectoryName(stagedBinary)!;
        File.WriteAllText(bat,
            $"""
             @echo off
             timeout /t 3 /nobreak > nul
             move /y "{stagedBinary}" "{appPath}"
             rmdir /s /q "{stagingDir}"
             start "" "{appPath}"
             del "%~f0"
             """);
        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
        {
            FileName        = "cmd.exe",
            Arguments       = $"/c \"{bat}\"",
            CreateNoWindow  = true,
            UseShellExecute = false
        });
    }

    private static void LaunchUnixRestart(string stagedBinary, string appBin, string appDir)
    {
        var sh = Path.Combine(Path.GetTempPath(), $"wowcidle_update_{Guid.NewGuid():N}.sh");
        var stagingDir = Path.GetDirectoryName(stagedBinary)!;
        File.WriteAllText(sh,
            $"""
             #!/bin/bash
             sleep 2
             cp -f "{stagedBinary}" "{appBin}"
             chmod +x "{appBin}"
             rm -rf "{stagingDir}"
             "{appBin}" &
             rm -f "$0"
             """);
        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
        {
            FileName        = "chmod",
            ArgumentList    = { "+x", sh },
            UseShellExecute = false
        })?.WaitForExit();
        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
        {
            FileName        = "bash",
            ArgumentList    = { sh },
            UseShellExecute = false
        });
    }

    private sealed record VersionManifest(
        [property: JsonPropertyName("version")]     string  Version,
        [property: JsonPropertyName("windows_url")] string? WindowsUrl,
        [property: JsonPropertyName("linux_url")]   string? LinuxUrl,
        [property: JsonPropertyName("mac_url")]     string? MacUrl
    );
}
