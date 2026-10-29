using System.Reflection;

namespace WoWClassicIdle.Components;

// Set in Program.cs via Initialize() – based on IWebHostEnvironment, not on the build configuration.
public static class BuildInfo
{
    public static bool IsDebug { get; private set; }

    // App version from assembly metadata (set via <Version> in the .csproj)
    public static Version AppVersion =>
        Assembly.GetEntryAssembly()?.GetName().Version ?? new Version(0, 0, 0);

    public static void Initialize(Microsoft.AspNetCore.Hosting.IWebHostEnvironment env)
        => IsDebug = env.IsDevelopment();
}
