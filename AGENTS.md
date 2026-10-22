# AGENTS.md

WoW Classic–style idle game. Single-project .NET 10 (`net10.0`) Blazor Web App (Razor components, interactive server render mode). No NuGet dependencies, no test project, no CI.

## Commands

- `dotnet build` — verify after changes (the only available check; there are no tests)
- `dotnet run --project WoWClassicIdle` — dev server, https://localhost:7243 (or http://localhost:5067)

## Architecture

- `WoWClassicIdle/Components/Models/IdleGameEngine.cs` is the whole game loop: a `System.Timers.Timer` (100 ms) ticks the simulation on a **thread-pool thread**. Its events (`TickUpdated`, `CombatTextRaised`, `AbilityUnlocked`) fire off the UI thread — any UI handler must dispatch via `InvokeAsync(StateHasChanged)`. Shared mutable state is guarded with `lock` objects.
- `IdleGameEngine`, `AbilityLoadoutService`, `AchievementService` are **singletons** (see `Program.cs`). Game state is **in-memory only** — nothing persists across restarts; services are documented as in-memory placeholders for a future DB.
- All game data is hardcoded in C# under `Components/Models/` (`WeaponRegistry`, `StarterWeapons`, `DefaultEnemyTemplates`, `ClassAbilityRegistry`, `ClassStatRegistry`, `StatsFactory`). There are no data files to regenerate.
- `wwwroot/js/visibility.js` + `IdleFighter.razor` (`OnVisibilityChanged`) pause the engine while the browser tab is hidden and resume it on show — do not assume the engine always ticks.
- `app.UseHttpsRedirection()` is on; keep the https launch profile when running.

## Debug-only code

`Components/Debug/**` and `Components/Pages/Debug/**` are **excluded from compilation in non-Debug builds** (condition in `WoWClassicIdle.csproj`). The class simulator (`/debug/simulation`) and debug UI only exist in Debug.

- Never reference Debug-only types from shared/Release-compiled code — Release builds will break. Gate UI with `Components/BuildInfo.cs` (`BuildInfo.IsDebug`).
- Build in Release (`dotnet build -c Release`) when touching shared code, to catch accidental references to Debug types.

## Conventions

- Code comments are in **German**; keep that when adding comments.
- No lint/format tooling is configured — match surrounding style manually.
