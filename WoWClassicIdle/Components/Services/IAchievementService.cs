using Models;

namespace Services;

public interface IAchievementService
{
    IReadOnlyList<AchievementDefinition> AllAchievements { get; }
    IReadOnlyList<EarnedAchievement> EarnedAchievements { get; }
    int TotalPoints { get; }

    bool IsEarned(string achievementId);
    EarnedAchievement? GetEarned(string achievementId);
    AchievementProgress? GetProgress(string achievementId);

    event Action<AchievementDefinition> AchievementEarned;
}
