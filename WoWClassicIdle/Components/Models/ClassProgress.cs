// Models/ClassProgress.cs
namespace Models;

public class ClassProgress
{
    public CharacterClass Class { get; }
    public int Level { get; private set; } = 1;
    public int Xp { get; private set; }
    public int XpToNextLevel { get; private set; }

    public ClassProgress(CharacterClass characterClass)
    {
        Class = characterClass;
        XpToNextLevel = StaticHelpers.GetXpForLevel(1);
    }

    /// <summary>
    /// Fügt XP hinzu und levelt so oft auf, wie nötig.
    /// Gibt an, wie viele Level-Steigerungen erfolgt sind.
    /// </summary>
    public int GainXp(int amount)
    {
        Xp += amount;
        int levelsGained = 0;

        // XpToNextLevel == 0 → Max-Level erreicht
        while (XpToNextLevel > 0 && Xp >= XpToNextLevel)
        {
            Xp -= XpToNextLevel;
            Level++;
            XpToNextLevel = StaticHelpers.GetXpForLevel(Level);
            levelsGained++;
        }

        // Auf Max-Level sammeln sich keine XP an
        if (XpToNextLevel == 0) Xp = 0;

        return levelsGained;
    }

    /// <summary>
    /// Zieht einen Anteil der aktuellen XP ab (z. B. beim Tod).
    /// Ein Level-Verlust ist ausgeschlossen – XP fallen höchstens auf 0.
    /// Gibt die tatsächlich verlorenen XP zurück.
    /// </summary>
    public int LoseXpPercent(double percent)
    {
        int lost = (int)Math.Floor(Xp * percent);
        Xp = Math.Max(0, Xp - lost);
        return lost;
    }

    /// <summary>
    /// Setzt die Klasse direkt auf ein Level (XP = 0), z. B. für Simulationen.
    /// </summary>
    public void SetLevel(int level)
    {
        Level = Math.Max(1, level);
        Xp = 0;
        XpToNextLevel = StaticHelpers.GetXpForLevel(Level);
    }

    /// <summary>
    /// Setzt Level und XP direkt (für Spielstand-Wiederherstellung).
    /// </summary>
    internal void Restore(int level, int xp)
    {
        Level = Math.Max(1, level);
        XpToNextLevel = StaticHelpers.GetXpForLevel(Level);
        Xp = Math.Clamp(xp, 0, XpToNextLevel > 0 ? XpToNextLevel - 1 : 0);
    }

    /// <summary>
    /// Reset auf Level 1 (z. B. für eine nie gespielte Klasse).
    /// </summary>
    public void ResetToLevelOne()
    {
        Level = 1;
        Xp = 0;
        XpToNextLevel = StaticHelpers.GetXpForLevel(1);
    }
}
