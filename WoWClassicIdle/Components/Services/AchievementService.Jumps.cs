using Models;

namespace Services;

// Jump achievements: Space on the Idle Fighter page, account-wide across all characters
public partial class AchievementService
{
    private static readonly int[] JumpMilestones = { 1, 10, 100, 500, 1_000, 5_000 };

    private void CheckJumps()
    {
        int jumps = _engine.TotalJumps;
        if (jumps <= _lastKnownJumps) return;

        foreach (int milestone in JumpMilestones)
        {
            if (_lastKnownJumps < milestone && jumps >= milestone)
                TryAward($"jumps_{milestone}");
        }
        _lastKnownJumps = jumps;
    }

    private static void AddJumpAchievements(List<AchievementDefinition> list)
    {
        static string Icon(string name) => $"https://wow.zamimg.com/images/wow/icons/large/{name}.jpg";
        var cat = AchievementCategory.General;

        list.Add(new() { Id = "jumps_1",     Category = cat, Points = 5,  Title = "Leap of Faith",     Description = "Jump for the first time.", IconUrl = Icon("inv_boots_05") });
        list.Add(new() { Id = "jumps_10",    Category = cat, Points = 10, Title = "Hop to It",         Description = "Jump 10 times.",           IconUrl = Icon("inv_misc_rabbit") });
        list.Add(new() { Id = "jumps_100",   Category = cat, Points = 10, Title = "Spring in Your Step", Description = "Jump 100 times.",        IconUrl = Icon("spell_magic_featherfall") });
        list.Add(new() { Id = "jumps_500",   Category = cat, Points = 15, Title = "Restless Legs",     Description = "Jump 500 times.",          IconUrl = Icon("ability_rogue_sprint") });
        list.Add(new() { Id = "jumps_1000",  Category = cat, Points = 25, Title = "Bunny Hopper",      Description = "Jump 1,000 times.",        IconUrl = Icon("inv_misc_rabbit_2") });
        list.Add(new() { Id = "jumps_5000",  Category = cat, Points = 50, Title = "Defying Gravity",   Description = "Jump 5,000 times.",        IconUrl = Icon("inv_gizmo_rocketboot_01") });
    }
}
