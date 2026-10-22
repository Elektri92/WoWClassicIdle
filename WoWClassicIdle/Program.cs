using Services;
using WoWClassicIdle.Components;
using Photino.NET;

// Muss vor jeder WebKit-Initialisierung gesetzt werden
if (OperatingSystem.IsLinux())
{
    // Verhindert DMA-BUF-Rendering in WebKit2GTK (leerer WebView unter Wayland)
    Environment.SetEnvironmentVariable("WEBKIT_DISABLE_DMABUF_RENDERER", "1");
}

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddSingleton<IdleGameEngine>();
builder.Services.AddSingleton<IAbilityLoadoutService, AbilityLoadoutService>();
builder.Services.AddSingleton<IAchievementService, AchievementService>();
builder.Services.AddSingleton<ISaveGameService, SaveGameService>();

var app = builder.Build();

BuildInfo.Initialize(app.Environment);

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

app.MapStaticAssets();
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
        .SetIconFile(iconPath)
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
