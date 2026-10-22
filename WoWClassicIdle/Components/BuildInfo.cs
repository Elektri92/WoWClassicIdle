namespace WoWClassicIdle.Components;

// Wird in Program.cs via Initialize() gesetzt – basiert auf IWebHostEnvironment, nicht auf der Build-Konfiguration.
public static class BuildInfo
{
    public static bool IsDebug { get; private set; }

    public static void Initialize(Microsoft.AspNetCore.Hosting.IWebHostEnvironment env)
        => IsDebug = env.IsDevelopment();
}
