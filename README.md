# WoW Classic Idle

Ein Idle-Spiel im Stil von **World of Warcraft Classic** — als Blazor-Web-App gebaut, die im Desktop-Modus als natives Fenster läuft. Der Charakter kämpft automatisch gegen stufenangepasste Gegner, steigt von Stufe 1 auf 60 auf, lernt Klassenfähigkeiten, sammelt Beute und schaltet Erfolge frei.

## Features

- **Charaktererstellung** nach Classic-Vorbild: 8 Rennen, 9 Klassen, Fraktionen Allianz/Horde mit klassischer Rennen-Klassen-Matrix
- **Automatischer Kampf** mit Kampftext, Crits, Ausweichen/Widerstand und Combat-Log
- **Fähigkeiten & Cooldowns** (Heroic Strike, Fireball, Lightning Bolt, Sinister Strike, …) inkl. Buffs, Debuffs und Mana-Management
- **Talentsystem** mit eigenem Talentbaum pro Klasse
- **Stufenaufstieg 1–60** mit Classic-orientierter XP-Kurve und Level-Ups
- **Gegner** nach Classic-Vorlagen (Wolf, Murloc & Co.), gewichtet nach Spielerstufe
- **Inventar, seltene Beute & Gold** (Beutetabellen nach Gegnerstufe)
- **Erfolge** mit Toast-Benachrichtigungen
- **Autosave** alle ~1 Minute als JSON unter `LocalApplicationData/WoWClassicIdle` (Debug-Nutzung nutzt eine eigene Save-Datei)
- **Pause bei Inaktivität**: Die Simulation pausiert, wenn der Browser-Tab bzw. das Fenster nicht sichtbar ist
- **Debug-Simulator** (`/debug/simulation`, nur im Debug-Build) zum Vergleich der Klassen

## Tech-Stack

- .NET 10 / C# 13, Blazor Web App (Render-Modus: Interactive Server), ohne JS-Framework
- Photino.NET 4.0.16 als einziger NuGet-Package: im Release/Production-Modus öffnet sich ein natives WebView-Fenster statt eines Browsers
- Game-Loop: `System.Timers.Timer` mit 100 ms Intervall auf einem Thread-Pool-Thread (`Components/Models/IdleGameEngine.cs`), geteilter Zustand über `lock`
- Keine Datenbank: Spielstand liegt als JSON-Datei auf der Festplatte, Services sind In-Memory-Singletons

## Voraussetzungen

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- Für den Desktop-Modus unter Linux zusätzlich `webkit2gtk`:
  - Arch/CachyOS: `sudo pacman -S webkit2gtk-4.1`
  - Debian/Ubuntu: `sudo apt install libwebkit2gtk-4.1-0`
  - Fedora: `sudo dnf install webkit2gtk4.1`

## Start

```bash
# Build
dotnet build

# Entwicklung (Playground im Browser)
dotnet run --project WoWClassicIdle
# https://localhost:7243  bzw.  http://localhost:5067
```

Im **Development**-Modus läuft die App im Browser; ab **Production/Release** öffnet Photino ein eigenes Desktop-Fenster.

## Publish (Desktop-Builds)

Vorhandene Publish-Profile: `linux-x64`, `win-x64`, `osx-x64` (jeweils self-contained, Single-File).

```bash
dotnet publish -c Release -r linux-x64
dotnet publish -c Release -r win-x64
```

Unter Linux wird die Binary nach dem Publish automatisch in `WoWClassicIdle.bin` umbenannt und ein Launcher-Skript (`WoWClassicIdle`) mit gesetzter `WEBKIT_DISABLE_DMABUF_RENDERER`-Variable abgelegt.

## Projektstruktur

```
WoWClassicIdle/
├── Components/
│   ├── Models/          # Spiellogik: IdleGameEngine, CombatEngine, Enemy, Player,
│   │                    # Abilities, Talents, Items, Achievements, Registries (hartcodierte Spieldaten)
│   ├── Pages/           # UI: Charakterwahl, IdleFighter, Talents, Inventar, Erfolge, Ability-Loadout
│   ├── Services/        # SaveGame-, AbilityLoadout- & Achievement-Services (Singletons)
│   └── Debug/           # Klassen-Simulator (nur Debug-Build, per csproj aus Release ausgeschlossen)
├── wwwroot/             # statische Assets (CSS, JS)
└── Program.cs           # DI-Registrierung, Kestrel & Photino-Desktop-Hosting
```

Alle Spieldaten (Waffen, Gegner, Klassenfähigkeiten, Talente) sind bewusst in C# hartcodiert — es gibt keine externen Datendateien.

## Hinweise

- Debug-only-Code (`Components/Debug/**`, `Components/Pages/Debug/**`) ist in Release-Builds nicht kompiliert; shared Code darf diese Typen nicht referenzieren (UI-Gating über `BuildInfo.IsDebug`).
- Persönliches Fan-Projekt ohne Verbindung zu Blizzard; die verwendeten Icon-URLs zeigen auf öffentliche WoW-Icon-CDNs.
