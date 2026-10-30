# WoW Classic Idle

An idle game inspired by **World of Warcraft Classic**, built as a Blazor Web App that runs in a native desktop window. Your characters automatically fight level-appropriate enemies across the zones of Azeroth. They level from 1 to 60, learn class abilities, pick up professions, collect loot and earn achievements.

## Features

### Characters
- **Character creation** that follows Classic rules: 8 races, 9 classes, Alliance and Horde with the classic race/class matrix, faction crests and race/class quotes
- **Character roster**: create several characters and switch between them, each with its own progress
- **Level 1–60 progression** with a Classic-style XP curve
- **Talent system** with a talent tree for each class (unlocks at level 10)
- **Ability loadout**: choose up to 6 abilities per class and set their priority. Includes ranks, buffs and debuffs, shapeshift forms, pets and mana management

### Combat & World
- **Automatic combat** with floating combat text, crits, dodges and resists, spell effects, a combat log and a damage meter
- **40 combat zones** in Eastern Kingdoms and Kalimdor, each with its own enemies. Enemies spawn at the player's level ± 2, limited to the zone's level range
- **Travel between zones** on foot, mounted, by flight path, ship or zeppelin, with travel times based on real map distances
- **Exploration**: you get discovery XP the first time you enter a zone and learn its flight path
- **Zone Mastery**: each zone has objectives (hunt its enemies, defeat a rare). Completing all of them rewards a guaranteed item
- **Rare enemies** with their own loot tables (from level 3)

### Items & Economy
- **Inventory and equipment** with bags that unlock as you level, item comparison and tooltips
- **Loot and money** in copper, silver and gold, scaled to enemy level
- **Goblin merchant** with live stock. Right-click an item to sell it
- **Consumables** such as food, potions and bandages, with a quick-use bar

### Professions
- **Gathering**: Herbalism, Mining, Skinning, Fishing
- **Crafting**: Alchemy, Blacksmithing, Enchanting (including disenchanting), Engineering, Leatherworking, Tailoring, Cooking, First Aid
- Profession trainers, recipes and skill-ups. Professions run in real time, separately from combat

### Meta
- **Achievements** with toast notifications, including exploration achievements
- **Feature unlock pop-ups** that introduce new systems as you level
- **Autosave** about once a minute, as JSON in `LocalApplicationData/WoWClassicIdle`
- **Automatic updates**: on startup the app checks for a newer release and replaces itself
- **Pauses when hidden**: the simulation stops while the window isn't visible

---

## Installation

Pre-built binaries are on the [Releases](https://github.com/Elektri92/WoWClassicIdle/releases) page. They are self-contained, so you don't need the .NET SDK to run them. After the first install, the game updates itself automatically.

### Windows

1. Download `WoWClassicIdle-win-x64.exe` from the latest release.
2. Run the `.exe` directly. No installation needed.
3. The first time, Windows may show a SmartScreen warning (unknown publisher). Click **More info → Run anyway**.

### Linux

1. Download `WoWClassicIdle-linux-x64.tar.gz` from the latest release.
2. Install the WebKit dependency for your distribution:
   - **Arch / CachyOS:** `sudo pacman -S webkit2gtk-4.1`
   - **Debian / Ubuntu:** `sudo apt install libwebkit2gtk-4.1-0`
   - **Fedora:** `sudo dnf install webkit2gtk4.1`
3. Extract the archive and start the game:
   ```bash
   mkdir WoWClassicIdle && tar -xzf WoWClassicIdle-linux-x64.tar.gz -C WoWClassicIdle
   ./WoWClassicIdle/WoWClassicIdle
   ```

The app sets the WebKit environment variables it needs on its own (`GDK_BACKEND=x11`, `WEBKIT_DMABUF_RENDERER_FORCE_SHM=1`). You don't need a launcher script.

### macOS

An Intel build (`WoWClassicIdle-osx-x64`) is attached to every release, but it isn't tested regularly. On Apple Silicon, see [Build from Source](#build-from-source).

---

## Build from Source

**Requirements:** [.NET 10 SDK](https://dotnet.microsoft.com/download)

```bash
git clone https://github.com/Elektri92/WoWClassicIdle.git
cd WoWClassicIdle

# Development mode (opens in the browser)
dotnet run --project WoWClassicIdle
# https://localhost:7243  or  http://localhost:5067
```

In **Development** mode the app runs in the browser. In **Release/Production** mode, Photino opens a native desktop window.

### Publish a self-contained binary

```bash
dotnet publish WoWClassicIdle -c Release -r win-x64      # Windows
dotnet publish WoWClassicIdle -c Release -r linux-x64    # Linux
dotnet publish WoWClassicIdle -c Release -r osx-x64      # macOS (Intel)
dotnet publish WoWClassicIdle -c Release -r osx-arm64    # macOS (Apple Silicon)
```

Static assets (`wwwroot`) are bundled into the single-file binary. That way an auto-update, which only replaces the binary, also delivers new CSS and images.

### Releases

Every push to `master` triggers a GitHub Actions workflow (`.github/workflows/release.yml`). It bumps the version in `version.json`, builds for Windows, Linux and macOS, publishes a GitHub release, and then deletes all older releases (their git tags stay). The auto-updater reads `version.json`.

---

## Tech Stack

- .NET 10 / C#, Blazor Web App (Interactive Server render mode), no JS framework (only small JS helpers for spell effects and window visibility)
- [Photino.NET](https://www.tryphotino.io/) is the only NuGet dependency. It wraps the app in a native WebView window
- Game loop: `System.Timers.Timer` at 100 ms on a thread-pool thread, with shared state protected by `lock`
- No database: the save game is a single JSON file, and services are in-memory singletons

## Project Structure

```
WoWClassicIdle/
├── Components/
│   ├── Models/          # Game logic and data: IdleGameEngine (partial classes per system),
│   │                    # CombatEngine, Player, Enemy, registries for abilities, talents,
│   │                    # zones, items, loot, professions, merchants
│   ├── Pages/           # UI: Home, Character, Idle (combat), Talents, Ability Loadout,
│   │                    # Inventory, Professions, Achievements
│   ├── Services/        # SaveGame, AbilityLoadout, Achievement and Update services (singletons)
│   ├── Debug/           # Class simulator and optimizers (Debug build only)
│   └── Pages/Debug/     # Debug pages: simulation, loot tables, enemy spawns, … (Debug build only)
├── wwwroot/             # Static assets (CSS, JS, images)
└── Program.cs           # DI registration, Kestrel and Photino desktop hosting
```

All game data (weapons, enemies, zones, abilities, talents, recipes) is hardcoded in C# on purpose. There are no external data files.

---

*Personal fan project, not affiliated with Blizzard Entertainment.*
