# WoW Classic Idle

An idle game inspired by **World of Warcraft Classic** — built as a Blazor Web App that runs as a native desktop window. Your character fights level-appropriate enemies automatically, levels from 1 to 60, learns class abilities, collects loot, and unlocks achievements.

## Features

- **Character creation** following Classic conventions: 8 races, 9 classes, Alliance/Horde factions with the classic race-class matrix
- **Automatic combat** with combat text, crits, dodge/resist, and a combat log
- **Abilities & cooldowns** (Heroic Strike, Fireball, Lightning Bolt, Sinister Strike, …) including buffs, debuffs, and mana management
- **Talent system** with a unique talent tree per class
- **Level 1–60 progression** with a Classic-oriented XP curve
- **Enemies** based on Classic archetypes (Wolf, Murloc & more), weighted by player level
- **Inventory, rare loot & gold** (loot tables scaled by enemy level)
- **Achievements** with toast notifications
- **Autosave** every ~1 minute as JSON in `LocalApplicationData/WoWClassicIdle`
- **Pause on inactivity**: simulation pauses when the window is not visible

---

## Installation

Pre-built binaries are available on the [Releases](https://github.com/Elektri92/WoWClassicIdle/releases) page. No .NET SDK required to run them — they are fully self-contained.

### Windows

1. Download `WoWClassicIdle-win-x64.exe` from the latest release.
2. Run the `.exe` directly — no installation needed.
3. Windows may show a SmartScreen warning the first time (unknown publisher). Click **More info → Run anyway**.

### Linux

1. Download `WoWClassicIdle-linux-x64` from the latest release.
2. Install the WebKit dependency for your distribution:
   - **Arch / CachyOS:** `sudo pacman -S webkit2gtk-4.1`
   - **Debian / Ubuntu:** `sudo apt install libwebkit2gtk-4.1-0`
   - **Fedora:** `sudo dnf install webkit2gtk4.1`
3. Make the binary executable and run it:
   ```bash
   chmod +x WoWClassicIdle-linux-x64
   ./WoWClassicIdle-linux-x64
   ```

### macOS

> macOS builds are not yet available as pre-built binaries. See [Build from Source](#build-from-source) below.

---

## Build from Source

**Requirements:** [.NET 10 SDK](https://dotnet.microsoft.com/download)

```bash
# Clone the repository
git clone https://github.com/Elektri92/WoWClassicIdle.git
cd WoWClassicIdle

# Run in development mode (opens in browser)
dotnet run --project WoWClassicIdle
# https://localhost:7243  or  http://localhost:5067
```

In **Development** mode the app runs in the browser. In **Release/Production** mode Photino opens a native desktop window.

### Publish a self-contained binary

```bash
# Windows
dotnet publish -c Release -r win-x64

# Linux
dotnet publish -c Release -r linux-x64

# macOS (Intel)
dotnet publish -c Release -r osx-x64

# macOS (Apple Silicon)
dotnet publish -c Release -r osx-arm64
```

On Linux the binary is automatically renamed to `WoWClassicIdle.bin` and a launcher script (`WoWClassicIdle`) with the required `WEBKIT_DISABLE_DMABUF_RENDERER` variable is placed alongside it.

---

## Tech Stack

- .NET 10 / C# 13, Blazor Web App (Interactive Server render mode), no JS framework
- [Photino.NET](https://www.tryphotino.io/) as the only NuGet dependency — wraps the app in a native WebView window for desktop use
- Game loop: `System.Timers.Timer` at 100 ms on a thread-pool thread, shared state guarded by `lock`
- No database: save state is a single JSON file; services are in-memory singletons

## Project Structure

```
WoWClassicIdle/
├── Components/
│   ├── Models/          # Game logic: IdleGameEngine, CombatEngine, Enemy, Player,
│   │                    # Abilities, Talents, Items, Achievements, Registries (hardcoded game data)
│   ├── Pages/           # UI: Character select, IdleFighter, Talents, Inventory, Achievements, Ability Loadout
│   ├── Services/        # SaveGame, AbilityLoadout & Achievement services (singletons)
│   └── Debug/           # Class simulator (Debug build only, excluded from Release via .csproj)
├── wwwroot/             # Static assets (CSS, JS)
└── Program.cs           # DI registration, Kestrel & Photino desktop hosting
```

All game data (weapons, enemies, class abilities, talents) is intentionally hardcoded in C# — there are no external data files.

---

*Personal fan project, not affiliated with Blizzard Entertainment.*
