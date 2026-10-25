// Debug/SimLoadoutOptimizer.cs – wird nur im DEBUG-Build kompiliert (siehe .csproj)
namespace DebugTools;

using Models;

/// <summary>
/// Verbessert den Simulator-Loadout empirisch: Statt einer festen Heuristik werden Fähigkeiten
/// in Probe-Kämpfen ausgetauscht und behalten, wenn sie mehr XP pro Stunde bringen.
/// XP/h bewertet Schaden, Überleben (Wiederbelebungszeit, verlorene Gegner) und Mana (Leerlauf) gemeinsam.
/// </summary>
public static class SimLoadoutOptimizer
{
    /// <summary>
    /// Simulierte Sekunden pro Probe-Kampf – lang genug, dass Manaknappheit und Tode sichtbar werden
    /// (bei 4 Minuten zeigten Mana- und Überlebens-Talente durchweg 0 %).
    /// </summary>
    private const double TrialSeconds = 600;

    /// <summary>Ein Tausch muss mindestens so viel besser sein, damit Zufallsrauschen keine Wechsel auslöst.</summary>
    private const double MinImprovement = 1.03;

    private const int MaxPasses = 2;

    /// <summary>
    /// Jede Probe läuft mit diesen festen Seeds und wird gemittelt: gleiche Gegner und Würfe für alle Varianten,
    /// aber kein Zufallstreffer eines einzelnen Kampfverlaufs (ein Seed allein ergab z. B. +13,8 % für +2 % Tempo).
    /// </summary>
    private static readonly int[] TrialSeeds = [1337, 7331, 4242];

    /// <summary>
    /// Kandidaten-Pool des Spielers: pro Fähigkeit der höchste verfügbare Rang (inkl. Talent-Fähigkeiten),
    /// ohne Gestalten – die bleiben aus <paramref name="seedIds"/> fest. Dient auch als Cache-Schlüssel.
    /// </summary>
    public static IReadOnlyList<Ability> CandidatePool(Player player, IReadOnlyList<string> seedIds)
    {
        var cls      = player.ActiveClass;
        var unlocked = ClassSimulator.GetUnlockedTalentAbilityIds(player, cls);
        var form     = ShapeshiftForms.FormOf(seedIds.Select(ClassAbilityRegistry.FindById).OfType<Ability>());

        return ClassAbilityRegistry.GetAbilitiesFor(cls)
            .Where(a => a.LevelRequired <= player.Level)
            .Where(a => !a.TalentUnlockOnly || unlocked.Contains(a.Id))
            .Where(a => !a.IsForm && ShapeshiftForms.IsUsableIn(a, form))
            .GroupBy(a => a.Name)
            .Select(g => g.OrderByDescending(a => a.Rank).First())
            .ToList();
    }

    /// <summary>Lesbarer Schlüssel des Pools – ändert er sich nicht, muss nicht neu optimiert werden.</summary>
    public static string PoolKey(IReadOnlyList<Ability> pool) =>
        string.Join(",", pool.Select(a => a.Id).OrderBy(id => id));

    /// <summary>
    /// Startet beim Heuristik-Loadout <paramref name="seedIds"/> und tauscht Fähigkeiten gegen Pool-Kandidaten,
    /// solange ein Tausch die XP pro Stunde spürbar erhöht (Hill-Climbing, erster Treffer gewinnt).
    /// </summary>
    public static List<string> Optimize(Player player, IReadOnlyList<string> seedIds, CancellationToken ct = default)
    {
        var pool  = CandidatePool(player, seedIds);
        var fixedIds = seedIds.Where(id => ClassAbilityRegistry.FindById(id) is { IsForm: true }).ToList();
        // Ausgerüstete Fähigkeiten auf den höchsten verfügbaren Rang heben; nicht mehr nutzbare fallen raus
        var current  = seedIds.Except(fixedIds)
            .Select(id => ClassAbilityRegistry.FindById(id) is { } a ? pool.FirstOrDefault(c => c.Name == a.Name)?.Id : null)
            .OfType<string>()
            .Distinct()
            .Take(ClassAbilityRegistry.MaxSlotsPerClass - fixedIds.Count)
            .ToList();
        int freeSlots = ClassAbilityRegistry.MaxSlotsPerClass - fixedIds.Count;

        double best = Evaluate(player, fixedIds.Concat(current).ToList(), ct);

        for (int pass = 0; pass < MaxPasses; pass++)
        {
            bool improved = false;
            foreach (var candidate in pool.Where(c => !current.Contains(c.Id)))
            {
                // Freier Slot: einfach ergänzen, sonst jeden belegten Slot als Tauschziel probieren
                var trials = current.Count < freeSlots
                    ? [current.Append(candidate.Id).ToList()]
                    : Enumerable.Range(0, current.Count)
                        .Select(i => current.Select((id, j) => j == i ? candidate.Id : id).ToList())
                        .ToList();

                List<string>? bestTrial = null;
                double bestTrialScore = best * MinImprovement;
                foreach (var trial in trials.Where(t => !HasExclusiveConflict(t)))
                {
                    double score = Evaluate(player, fixedIds.Concat(trial).ToList(), ct);
                    if (score > bestTrialScore)
                    {
                        bestTrial      = trial;
                        bestTrialScore = score;
                    }
                }

                if (bestTrial is null) continue;
                current  = bestTrial;
                best     = bestTrialScore;
                improved = true;
            }
            if (!improved) break;
        }

        return fixedIds.Concat(current).ToList();
    }

    /// <summary>Zwei Fähigkeiten derselben ExclusiveGroup (z. B. zwei Seals oder Pets) sind nicht erlaubt.</summary>
    internal static bool HasExclusiveConflict(IEnumerable<string> ids) =>
        ids.Select(ClassAbilityRegistry.FindById)
            .Where(a => a is not null && !string.IsNullOrEmpty(a.ExclusiveGroup))
            .GroupBy(a => a!.ExclusiveGroup)
            .Any(g => g.Count() > 1);

    /// <summary>
    /// Probe-Kampf mit einer frischen Engine, die Level, Talente und Ausrüstung von <paramref name="template"/> übernimmt.
    /// Ergebnis: XP pro simulierter Sekunde. Der angeschlagene letzte Gegner zählt anteilig nach entferntem HP-Anteil,
    /// damit der Wert nicht in ganzen Kills springt.
    /// <paramref name="extraTalent"/> wird zusätzlich um einen Rang erhöht (Talent-Probe).
    /// </summary>
    internal static double Evaluate(Player template, List<string> ids, CancellationToken ct, TalentDefinition? extraTalent = null) =>
        TrialSeeds.Average(seed => EvaluateOnce(template, ids, seed, ct, extraTalent));

    private static double EvaluateOnce(Player template, List<string> ids, int seed, CancellationToken ct, TalentDefinition? extraTalent)
    {
        var cls   = template.ActiveClass;
        int level = template.Level;

        var engine = new IdleGameEngine(seed: seed);
        engine.SetCharacter(RaceClassInfo.RaceToFaction[template.Race], template.Race, cls, Gender.Male);
        engine.EnableRareRequeue = false;

        var player   = engine.Player;
        var progress = player.ClassProgress[cls];
        progress.SetLevel(level);
        player.SetActiveClass(cls);
        var talents = new Dictionary<string, int>(template.TalentState.Invested);
        if (extraTalent is not null)
            talents[extraTalent.Id] = talents.GetValueOrDefault(extraTalent.Id) + 1;
        player.RestoreTalentPoints(talents);
        foreach (var armor in template.EquippedArmor.Values)
            player.ForceEquipArmor(armor);
        if (template.EquippedWeapon is { } weapon)
            player.ForceEquipWeapon(weapon);
        player.ApplyLoadout(new ClassLoadout { Class = cls, EquippedAbilityIds = ids });

        engine.Meter.Reset();
        engine.CurrentEnemy = null;

        long xpGained = 0;
        int  steps    = (int)Math.Round(TrialSeconds / engine.StepSeconds);
        for (int i = 0; i < steps; i++)
        {
            if ((i & 1023) == 0) ct.ThrowIfCancellationRequested();
            try
            {
                engine.RunSteps(1);
            }
            catch (InvalidOperationException)
            {
                // Spawner hat kein Template für das gewürfelte Gegner-Level
            }

            // Level einfrieren, damit alle Proben unter gleichen Bedingungen laufen
            if (progress.Xp > 0 || progress.Level != level)
            {
                xpGained += progress.Xp;
                progress.SetLevel(level);
            }
        }

        double partialXp = engine.CurrentEnemy is { IsAlive: true } enemy && enemy.Stats.MaxHp > 0
            ? StaticHelpers.GetEnemyXpReward(enemy.Stats.Level) * (1.0 - (double)enemy.CurrentHp / enemy.Stats.MaxHp)
            : 0;
        return (xpGained + partialXp) / (steps * engine.StepSeconds);
    }
}
