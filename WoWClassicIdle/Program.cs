using Services;
using WoWClassicIdle.Components;
using Photino.NET;

// Muss vor jeder WebKit-Initialisierung gesetzt werden
// Auf Linux müssen GDK/WebKit-Env-Vars gesetzt sein BEVOR GTK durch die nativen Bibliotheken
// initialisiert wird. Da der Single-File-Bootstrapper native Libs vor managed Code lädt,
// starten wir den Prozess bei Bedarf mit den korrekten Vars neu.
// Nicht in Development (dort kein Photino) und nicht mit Debugger – sonst hängt der Debugger am beendeten Elternprozess.
bool isDevelopment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";
if (OperatingSystem.IsLinux()
    && !isDevelopment
    && !System.Diagnostics.Debugger.IsAttached
    && Environment.GetEnvironmentVariable("GDK_BACKEND") != "x11")
{
    var psi = new System.Diagnostics.ProcessStartInfo(Environment.ProcessPath!)
    {
        UseShellExecute = false
    };
    foreach (System.Collections.DictionaryEntry entry in Environment.GetEnvironmentVariables())
        psi.Environment[(string)entry.Key] = entry.Value as string ?? "";
    psi.Environment["GDK_BACKEND"]                 = "x11";
    // DMA-BUF-Puffer zeigen unter NVIDIA/XWayland ein leeres Fenster. FORCE_SHM rendert weiter auf der GPU und
    // überträgt nur das fertige Bild per Shared Memory – DISABLE_DMABUF_RENDERER fiel auf Software-Rendering
    // zurück (gemessen: ~12 statt ~58 FPS im Kampf).
    psi.Environment["WEBKIT_DMABUF_RENDERER_FORCE_SHM"] = "1";
    System.Diagnostics.Process.Start(psi);
    return;
}

// Publish bündelt wwwroot in die Single-File-Binary und entpackt sie nach AppContext.BaseDirectory.
// Dort liegt dann das aktuelle wwwroot – nicht das veraltete neben der Binary (Auto-Update ersetzt nur die Binary).
var bundledWebRoot = Path.Combine(AppContext.BaseDirectory, "wwwroot");
var builder = Directory.Exists(bundledWebRoot)
    ? WebApplication.CreateBuilder(new WebApplicationOptions
    {
        Args            = args,
        ContentRootPath = AppContext.BaseDirectory,
        WebRootPath     = bundledWebRoot
    })
    : WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddSingleton<IdleGameEngine>();
builder.Services.AddSingleton<IAbilityLoadoutService, AbilityLoadoutService>();
builder.Services.AddSingleton<IAchievementService, AchievementService>();
builder.Services.AddSingleton<ISaveGameService, SaveGameService>();
builder.Services.AddSingleton<Services.UpdateService>();

var app = builder.Build();

BuildInfo.Initialize(app.Environment);

// Berufe laufen in Echtzeit unabhängig vom Kampf-Timer
app.Services.GetRequiredService<IdleGameEngine>().StartProfessionTimer();

#if DEBUG
// Debug-Start: Preset-Charaktere für alle Klassen anlegen
if (app.Environment.IsDevelopment())
    DebugTools.DebugCharacterPresets.Apply(app.Services.GetRequiredService<IdleGameEngine>());
#endif

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}
else
{
    app.UseHttpsRedirection();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);

app.UseAntiforgery();

// Statische Dateien haben keine Fingerprint-URLs (kein MapStaticAssets) – ohne no-cache zeigt
// WebKit/Photino nach einem Update/Rebuild alte CSS/JS aus dem Cache. Revalidierung per ETag (304).
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx => ctx.Context.Response.Headers.CacheControl = "no-cache"
});
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

if (app.Environment.IsDevelopment())
{
    app.Run();
}
else
{   
    // Auf Linux prüfen ob libwebkit2gtk-4.1 vorhanden ist (Photino-Abhängigkeit)
    if (OperatingSystem.IsLinux())
    {
        bool hasWebkit =
            System.Runtime.InteropServices.NativeLibrary.TryLoad("libwebkit2gtk-4.1.so.0", out _) ||
            System.Runtime.InteropServices.NativeLibrary.TryLoad("libwebkit2gtk-4.0.so.0", out _);

        if (!hasWebkit)
        {
            const string msg = "Missing dependency: libwebkit2gtk\n\n"
                + "Please install it first:\n"
                + "  Arch/CachyOS:   sudo pacman -S webkit2gtk-4.1\n"
                + "  Debian/Ubuntu:  sudo apt install libwebkit2gtk-4.1-0\n"
                + "  Fedora:         sudo dnf install webkit2gtk4.1";

            // Versuche nativen Dialog (zenity → kdialog → xmessage → Konsole)
            bool shown = false;
            foreach (var (bin, arguments) in new (string, string)[]
            {
                ("zenity",   $"--error --title=\"WoW Classic Idle\" --text=\"{msg}\" --no-wrap"),
                ("kdialog",  $"--error \"{msg}\" --title \"WoW Classic Idle\""),
                ("xmessage", $"-center \"{msg}\"")
            })
            {
                try
                {
                    var p = System.Diagnostics.Process.Start(bin, arguments);
                    if (p is not null) { p.WaitForExit(); shown = true; break; }
                }
                catch { }
            }

            if (!shown)
                Console.Error.WriteLine(msg);

            return;
        }
    }

    await app.StartAsync();

    // Tatsächlich gebundene URL lesen — app.Urls enthält noch Port 0, IServerAddressesFeature den echten Port
    var serverUrl = app.Services
        .GetRequiredService<Microsoft.AspNetCore.Hosting.Server.IServer>()
        .Features.Get<Microsoft.AspNetCore.Hosting.Server.Features.IServerAddressesFeature>()!
        .Addresses.First();

    // Spielstand beim Start laden, falls vorhanden
    app.Services.GetRequiredService<ISaveGameService>().TryAutoLoad();

    var iconPath = Path.Combine(AppContext.BaseDirectory, "wwwroot", "images", "wow_idle_game_logo.png");

    var window = new PhotinoWindow()
        .SetTitle("WoW Classic Idle")
        .SetIconFile(File.Exists(iconPath) ? iconPath : null!)
        .SetMaximized(true)
        .SetResizable(true)
        .RegisterWindowClosingHandler((_, _) =>
        {
            // Spielstand beim Schließen speichern
            app.Services.GetRequiredService<ISaveGameService>().Save();
            return false; // false = Schließen erlauben
        })
        .Load(new Uri(serverUrl));

    window.WaitForClose();
    await app.StopAsync();
}
