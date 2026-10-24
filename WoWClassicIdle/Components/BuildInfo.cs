using System.Reflection;

namespace WoWClassicIdle.Components;

// Wird in Program.cs via Initialize() gesetzt – basiert auf IWebHostEnvironment, nicht auf der Build-Konfiguration.
public static class BuildInfo
{
    public static bool IsDebug { get; private set; }

    // App-Version aus Assembly-Metadaten (gesetzt über <Version> in der .csproj)
    public static Version AppVersion =>
        Assembly.GetEntryAssembly()?.GetName().Version ?? new Version(0, 0, 0);

    public static void Initialize(Microsoft.AspNetCore.Hosting.IWebHostEnvironment env)
        => IsDebug = env.IsDevelopment();
}
