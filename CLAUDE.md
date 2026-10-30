# WoWClassicIdle – Kontext für den Coding-Agenten

WoW Classic–Idle-Game. .NET 10 Blazor Web App (Razor-Komponenten, Interactive Server Render Mode).
Kein NuGet, kein Test-Projekt. CI via GitHub Actions (`.github/workflows/release.yml`).

## Build / Run

```
dotnet build WoWClassicIdle          # Debug (einziger Check)
dotnet build WoWClassicIdle -c Release   # Pflicht nach Änderungen an shared code
dotnet run --project WoWClassicIdle  # Dev-Server https://localhost:7243
```

## Ordnerstruktur

```
WoWClassicIdle/Components/
  Models/             Spiellogik und Daten
  Pages/              Blazor-Seiten (UI)
  Services/           Singletons (AchievementService, SaveGameService, AbilityLoadoutService)
  Debug/              Nur Debug-Build (ClassSimulator.cs usw.)
  Pages/Debug/        Nur Debug-Build (Simulation.razor, LootTables.razor …)
```

## Wo liegt was?

| Datei | Inhalt |
|---|---|
| `Models/IdleGameEngine.cs` | Felder, Events, Konstruktor, Start/Stop/SetSpeed |
| `Models/IdleGameEngine.Combat.cs` | Tick-Loop, SimulateOneStep, HandleEnemyDeath |
| `Models/IdleGameEngine.Roster.cs` | Charakter-Verwaltung (CreateCharacter, SelectCharacter …) |
| `Models/IdleGameEngine.Talent.cs` | Talente, DebugGiveArmor |
| `Models/IdleGameEngine.SaveLoad.cs` | RestoreFromSave, RestoreSlot |
| `Models/ClassAbilityRegistry.cs` | Abilities-Dict + statische Methoden |
| `Models/ClassAbilityRegistry.Warrior.cs` … `.Druid.cs` | Ability-Daten je Klasse |
| `Models/EnemySpawner.cs` | Gegner-Spawn-Logik (Level = Spieler ± 2, auf Zonenbereich begrenzt) |
| `Models/CombatZoneRegistry*.cs` | Kampfzonen (40) mit je 3 Gegnern, Koordinaten, Theme |
| `Models/EnemyArchetype.cs` / `EnemyScaling.cs` | Stat-Profile je Gegnerart + HP/Attack/Kupfer-Formeln |
| `Models/ZoneTravel.cs` | Reisezeiten (Laufen/Reittier/Flug/Schiff/Zeppelin) |
| `Models/IdleGameEngine.Zones.cs` | Zonenwahl je Charakter, Reisen, SpawnEnemy |
| `Models/RareLootRegistry.cs` | Rare-Loottabellen |
| `Models/Player.cs` | Spieler-Zustand, Equip, XP |
| `Models/CombatEngine.cs` | Reine Kampfmathematik (keine Zustand) |

## Engine-Architektur

- `System.Timers.Timer` (100 ms) tickt auf Thread-Pool-Thread.
  Events (`TickUpdated`, `CombatTextRaised`, `AbilityUnlocked`, `ItemDropped`) feuern off-UI-Thread
  → UI-Handler braucht `InvokeAsync(StateHasChanged)`.
- Geteilter Zustand wird mit `lock(_simLock)` geschützt.
- `IdleGameEngine`, `AbilityLoadoutService`, `AchievementService` sind **Singletons** (`Program.cs`).
- Spielstand nur im Speicher; `SaveGameService` schreibt JSON nach `LocalApplicationData`.

## Debug-only Code

`Components/Debug/**` und `Components/Pages/Debug/**` werden nur im Debug-Build kompiliert (`.csproj`).
Nie Debug-Typen aus shared/Release-Code referenzieren. Gate über `BuildInfo.IsDebug`.

## Changelog

Die CI erzeugt bei jedem Push auf `master` den Eintrag in `WoWClassicIdle/changelog.json` aus den Commits seit dem letzten Release-Tag
(`.github/scripts/update_changelog.py`). Die Datei nie von Hand für neue Builds ergänzen.

**Jeder Commit mit spielerrelevanter Änderung braucht im Body eine Zeile `Changelog(<Kategorie>): <englischer Text für Spieler>`**
(mehrfach erlaubt, in eigenem Absatz vor `Co-Authored-By`). Beschreibt, was sich für Spieler ändert, nicht die Technik.
Rein interne Commits (`chore`, `ci`, `docs`, `refactor`, `style`, `test`, `build`) brauchen keine.
Fehlt sie bei anderen Typen, zeigt das Spiel nur „Minor fixes and improvements.“, und die CI gibt eine Warnung aus.

Kategorien (Reihenfolge im Spiel, definiert in `update_changelog.py`):

| Kategorie | Inhalt |
|---|---|
| `Classes` | Fähigkeiten, Talente, Ability Loadout |
| `Combat` | Kampfregeln, Gegner, Tod, XP |
| `Items` | Ausrüstung, Taschen, Verbrauchsgüter, Loot, Händler |
| `Professions` | Berufe, Rezepte, Sammeln, Herstellen |
| `World` | Zonen, Reisen |
| `Interface` | Menüs, Tooltips, Hotkeys, Bedienung |
| `System` | Speichern, Updates, Performance, Plattform |

Zeilen ohne oder mit unbekannter Kategorie landen unter `General`, und die CI gibt eine Warnung aus.

## Konventionen

- **Partial classes**: Neue Systeme als eigene `partial`-Datei anlegen — keine Monolithen.
  Dateien möglichst < 300 Zeilen halten. Erst Grep/Outline, nie ganze Dateien am Stück lesen.
- Code-Kommentare auf **Englisch**.
- UI-Text (Logs, Labels, Fehlermeldungen) auf **Englisch**.
- Keine Lint-Konfiguration – umgebenden Stil manuell anpassen.
- Statische Daten (Mobs, Items, Abilities) von Logik getrennt halten.
