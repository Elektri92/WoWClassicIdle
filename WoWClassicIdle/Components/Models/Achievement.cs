namespace Models;

public enum AchievementCategory
{
    General,
    Classes,
    Kills,
    Deaths,
    Loot,
    Exploration,
    Inventory,
    Professions
}

public class AchievementDefinition
{
    public string Id { get; init; } = "";
    public string Title { get; init; } = "";
    public string Description { get; init; } = "";
    public AchievementCategory Category { get; init; }
    public int Points { get; init; } = 10;
    public string IconUrl { get; init; } = "";
    public CharacterClass? Class { get; init; }
}

public class EarnedAchievement
{
    public string AchievementId { get; init; } = "";
    public DateTime EarnedAt { get; init; }
}

public record AchievementProgress(long Current, long Target)
{
    public double Percent => Target > 0 ? Current * 100.0 / Target : 0;
}
