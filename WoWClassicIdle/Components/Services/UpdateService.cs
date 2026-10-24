using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Services;

// Prüft beim App-Start auf eine neue Version und ersetzt die Binary automatisch.
// Kehrt bei erfolgreichem Update NICHT zurück – App beendet sich via Environment.Exit(0),
// ein Neustart-Skript übernimmt das Ersetzen und den Wiederstart.
public static class UpdateService
{
    // Raw-URL zur version.json im GitHub-Repo (main-Branch)
    private const string ManifestUrl = "https://raw.githubusercontent.com/Elektri92/WoWClassicIdle/master/version.json";

    // Struktur der version.json auf dem Server
    private sealed record VersionManifest(
        [property: JsonPropertyName("version")]     string  Version,
        [property: JsonPropertyName("windows_url")] string? WindowsUrl,
        [property: JsonPropertyName("linux_url")]   string? LinuxUrl,
        [property: JsonPropertyName("mac_url")]     string? MacUrl
    );

    // Prüft auf Update und wendet es an. Gibt false zurück wenn kein Update nötig oder ein Fehler aufgetreten ist.
    // Fehler blockieren NIE den App-Start – sie werden nur auf die Konsole geschrieben.
    public static async Task<bool> CheckAndApplyAsync()
    {
        try
        {
            var localVersion = Assembly.GetEntryAssembly()?.GetName().Version ?? new Version(0, 0, 0);
            Console.WriteLine($"[Update] Checking for updates... (local: v{localVersion.Major}.{localVersion.Minor}.{localVersion.Build}, url: {ManifestUrl})");

            using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(15) };

            var json     = await http.GetStringAsync(ManifestUrl);
            var manifest = JsonSerializer.Deserialize<VersionManifest>(json);

            if (manifest is null)
            {
                Console.WriteLine("[Update] Invalid manifest – skipping.");
                return false;
            }

            if (!Version.TryParse(manifest.Version, out var remoteVersion))
            {
                Console.WriteLine($"[Update] Cannot parse remote version '{manifest.Version}' – skipping.");
                return false;
            }

            if (remoteVersion <= localVersion)
            {
                Console.WriteLine($"[Update] Up to date (v{localVersion}).");
                return false;
            }

            Console.WriteLine($"[Update] New version {remoteVersion} available (local: {localVersion}).");

            // Plattformspezifische Download-URL wählen
            var downloadUrl = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? manifest.WindowsUrl
                            : RuntimeInformation.IsOSPlatform(OSPlatform.OSX)     ? manifest.MacUrl
                            :                                                        manifest.LinuxUrl;

            if (string.IsNullOrEmpty(downloadUrl))
            {
                Console.WriteLine("[Update] No download URL for this platform – skipping.");
                return false;
            }

            // Staging-Verzeichnis im App-Ordner
            var appPath    = Environment.ProcessPath!;
            var appDir     = Path.GetDirectoryName(appPath)!;
            var stagingDir = Path.Combine(appDir, "_update");
            Directory.CreateDirectory(stagingDir);

            var stagedBinary = Path.Combine(stagingDir, Path.GetFileName(appPath));

            Console.WriteLine($"[Update] Downloading to {stagedBinary} ...");

            using var response = await http.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();

            var totalBytes = response.Content.Headers.ContentLength ?? -1;
            var downloaded = 0L;
            var lastReport = 0L;

            await using (var srcStream  = await response.Content.ReadAsStreamAsync())
            await using (var destStream = File.Create(stagedBinary))
            {
                var buffer = new byte[81920];
                int read;
                while ((read = await srcStream.ReadAsync(buffer)) > 0)
                {
                    await destStream.WriteAsync(buffer.AsMemory(0, read));
                    downloaded += read;
                    // Alle 5 MB Fortschritt melden
                    if (downloaded - lastReport >= 5 * 1024 * 1024)
                    {
                        lastReport = downloaded;
                        var mb = downloaded / 1024.0 / 1024.0;
                        var info = totalBytes > 0 ? $"{mb:F1} MB / {totalBytes / 1024.0 / 1024.0:F1} MB" : $"{mb:F1} MB";
                        Console.WriteLine($"[Update] Downloading... {info}");
                    }
                }
            }

            Console.WriteLine($"[Update] Download complete ({downloaded / 1024.0 / 1024.0:F1} MB). Launching restart...");

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                LaunchWindowsRestart(stagedBinary, appPath);
            else
                LaunchUnixRestart(stagedBinary, appPath, appDir);

            // App beendet sich – Neustart-Skript übernimmt
            Environment.Exit(0);
            return true; // Wird nie erreicht
        }
        catch (Exception ex)
        {
            // Update-Fehler dürfen den Spielstart nicht blockieren
            Console.Error.WriteLine($"[Update] Error: {ex.Message}");
            return false;
        }
    }

    // Erzeugt ein temporäres .bat-Skript, das nach kurzer Wartezeit die neue Binary überschreibt und neu startet.
    private static void LaunchWindowsRestart(string stagedBinary, string appPath)
    {
        var bat = Path.Combine(Path.GetTempPath(), $"wowcidle_update_{Guid.NewGuid():N}.bat");

        File.WriteAllText(bat,
            $"""
             @echo off
             timeout /t 3 /nobreak > nul
             move /y "{stagedBinary}" "{appPath}"
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

    // Erzeugt ein temporäres Shell-Skript, das nach kurzer Wartezeit die neue Binary kopiert und neu startet.
    private static void LaunchUnixRestart(string stagedBinary, string appBin, string appDir)
    {
        var launcher = appBin;

        var sh = Path.Combine(Path.GetTempPath(), $"wowcidle_update_{Guid.NewGuid():N}.sh");

        File.WriteAllText(sh,
            $"""
             #!/bin/bash
             sleep 2
             cp -f "{stagedBinary}" "{appBin}"
             chmod +x "{appBin}"
             "{launcher}" &
             rm -f "$0"
             """);

        // Skript ausführbar machen und im Hintergrund starten
        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
        {
            FileName  = "chmod",
            ArgumentList = { "+x", sh },
            UseShellExecute = false
        })?.WaitForExit();
        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
        {
            FileName        = "bash",
            ArgumentList    = { sh },
            UseShellExecute = false
        });
    }
}
