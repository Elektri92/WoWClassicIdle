using Services;
using WoWClassicIdle.Components;
using Photino.NET;

// Must be set before any WebKit initialization
// On Linux the GDK/WebKit env vars must be set BEFORE GTK is initialized by the native
// libraries. Since the single-file bootstrapper loads native libs before managed code,
// we restart the process with the correct vars if needed.
// Not in Development (no Photino there) and not with a debugger – otherwise the debugger hangs on the exited parent process.
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
    // DMA-BUF buffers show an empty window under NVIDIA/XWayland. FORCE_SHM keeps rendering on the GPU and
    // only transfers the finished image via shared memory – DISABLE_DMABUF_RENDERER fell back to software rendering
    // (measured: ~12 instead of ~58 FPS in combat).
    psi.Environment["WEBKIT_DMABUF_RENDERER_FORCE_SHM"] = "1";
    System.Diagnostics.Process.Start(psi);
    return;
}

// Publish bundles wwwroot into the single-file binary and extracts it to AppContext.BaseDirectory.
// The current wwwroot lives there – not the outdated one next to the binary (auto-update only replaces the binary).
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

// Professions run in real time independent of the combat timer
app.Services.GetRequiredService<IdleGameEngine>().StartProfessionTimer();
// Zone travel continues outside /idle; combat does not
app.Services.GetRequiredService<IdleGameEngine>().StartTravelTimer();

#if DEBUG
// Debug start: create preset characters for all classes
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

// Static files have no fingerprinted URLs (no MapStaticAssets) – without no-cache
// WebKit/Photino shows old CSS/JS from the cache after an update/rebuild. Revalidation via ETag (304).
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
    // On Linux check whether libwebkit2gtk-4.1 is present (Photino dependency)
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

            // Try a native dialog (zenity → kdialog → xmessage → console)
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

    // Deliberately synchronous: an await would turn Main into an async Main and move the continuation to a
    // thread pool thread – but Photino needs the main thread on macOS and an STA thread on Windows.
    app.Start();

    // Read the actually bound URL — app.Urls still contains port 0, IServerAddressesFeature the real port
    var serverUrl = app.Services
        .GetRequiredService<Microsoft.AspNetCore.Hosting.Server.IServer>()
        .Features.Get<Microsoft.AspNetCore.Hosting.Server.Features.IServerAddressesFeature>()!
        .Addresses.First();

    // Load the save game on startup, if present
    var saveGame = app.Services.GetRequiredService<ISaveGameService>();
    saveGame.TryAutoLoad();

    var iconPath = Path.Combine(AppContext.BaseDirectory, "wwwroot", "images", "wow_idle_game_logo.png");

    void RunWindow()
    {
        var window = new PhotinoWindow()
            .SetTitle("WoW Classic Idle")
            .SetIconFile(File.Exists(iconPath) ? iconPath : null!)
            .SetMaximized(true)
            .SetResizable(true)
            .RegisterWindowClosingHandler((_, _) =>
            {
                // Save the game on close
                saveGame.Save();
                return false; // false = allow closing
            })
            // Reopen the page that was open when the game was closed
            .Load(new Uri(new Uri(serverUrl), saveGame.LastRoute ?? "/"));

        window.WaitForClose();
    }

    // WebView2 (Windows) only runs on an STA thread – on the MTA main thread the window stays white.
    // [STAThread] is not possible with top-level statements, hence a dedicated thread.
    if (OperatingSystem.IsWindows())
    {
        var uiThread = new Thread(RunWindow);
        uiThread.SetApartmentState(ApartmentState.STA);
        uiThread.Start();
        uiThread.Join();
    }
    else
    {
        RunWindow();
    }

    app.StopAsync().GetAwaiter().GetResult();
}
