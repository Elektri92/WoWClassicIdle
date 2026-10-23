# WoWClassicIdle – Kontext für den Coding-Agenten

WoW Classic–Idle-Game. .NET 10 Blazor Web App (Razor-Komponenten, Interactive Server Render Mode).
Kein NuGet, kein Test-Projekt, kein CI.

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
| `Models/EnemySpawner.cs` | Gegner-Spawn-Logik |
| `Models/DefaultEnemyTemplates.cs` | Mob-Rohdaten (hardcoded) |
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

## Konventionen

- **Partial classes**: Neue Systeme als eigene `partial`-Datei anlegen — keine Monolithen.
  Dateien möglichst < 300 Zeilen halten. Erst Grep/Outline, nie ganze Dateien am Stück lesen.
- Code-Kommentare auf **Deutsch**.
- UI-Text (Logs, Labels, Fehlermeldungen) auf **Englisch**.
- Keine Lint-Konfiguration – umgebenden Stil manuell anpassen.
- Statische Daten (Mobs, Items, Abilities) von Logik getrennt halten.
