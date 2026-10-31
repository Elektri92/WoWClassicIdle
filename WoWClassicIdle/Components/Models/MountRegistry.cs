// Models/MountRegistry.cs
namespace Models;

/// <summary>Learnable riding rank; <see cref="Rank"/> starts at 1 and every rank requires the previous one.</summary>
public sealed record RidingSkill(int Rank, string Name, int RequiredLevel, long Cost, double SpeedBonus, ItemQuality Quality)
{
    public int SpeedPercent => (int)Math.Round(SpeedBonus * 100);
}

/// <summary>Mount of a race for a riding rank.</summary>
public sealed record Mount(string Name, string IconUrl, RidingSkill Riding)
{
    public double SpeedMultiplier => 1 + Riding.SpeedBonus;
}

/// <summary>Riding trainer NPC of a race.</summary>
public sealed record RidingTrainer(string Name, string Title, Race Race, Gender Gender, string Greeting)
{
    public string PortraitUrl => IconUrls.RaceIconUrl(Race, Gender);
}

/// <summary>Riding ranks, trainers and race mounts. Mounts are derived from the race, so they follow a race change.</summary>
public static class MountRegistry
{
    /// <summary>From this level the riding trainer appears in the inventory.</summary>
    public const int UnlockLevel = 40;

    public static readonly IReadOnlyList<RidingSkill> Skills =
    [
        new(1, "Riding (60%)",  UnlockLevel, 100 * Money.CopperPerGold,  0.6, ItemQuality.Rare),
        new(2, "Riding (100%)", 60,          1000 * Money.CopperPerGold, 1.0, ItemQuality.Epic),
    ];

    private record RaceMounts(RidingTrainer Trainer, string IconUrl, string Riding, string Epic);

    private static string Icon(string name) => $"https://wow.zamimg.com/images/wow/icons/large/{name}.jpg";

    private static readonly Dictionary<Race, RaceMounts> ByRace = new()
    {
        [Race.Human]    = new(new("Randal Hunter", "Horse Riding Instructor", Race.Human, Gender.Male, "A good horse is worth every coin."),
                              Icon("ability_mount_ridinghorse"), "Brown Horse", "Swift Brown Steed"),
        [Race.Dwarf]    = new(new("Ultham Ironhorn", "Ram Riding Instructor", Race.Dwarf, Gender.Male, "Rams dinnae bite. Much."),
                              Icon("ability_mount_mountainram"), "Brown Ram", "Swift Brown Ram"),
        [Race.NightElf] = new(new("Jartsam", "Nightsaber Riding Instructor", Race.NightElf, Gender.Male, "The sabers will accept you in time."),
                              Icon("ability_mount_whitetiger"), "Striped Nightsaber", "Swift Frostsaber"),
        [Race.Gnome]    = new(new("Binjy Featherwhistle", "Mechanostrider Pilot", Race.Gnome, Gender.Male, "Fully tested! Well, mostly tested."),
                              Icon("ability_mount_mechastrider"), "Blue Mechanostrider", "Swift Green Mechanostrider"),
        [Race.Orc]      = new(new("Kildar", "Wolf Riding Instructor", Race.Orc, Gender.Male, "A wolf respects only strength."),
                              Icon("ability_mount_blackdirewolf"), "Timber Wolf", "Swift Timber Wolf"),
        [Race.Undead]   = new(new("Velma Warnam", "Undead Horse Riding Instructor", Race.Undead, Gender.Female, "They never tire. Neither do we."),
                              Icon("ability_mount_undeadhorse"), "Red Skeletal Horse", "Green Skeletal Warhorse"),
        [Race.Tauren]   = new(new("Kar Stormsinger", "Kodo Riding Instructor", Race.Tauren, Gender.Male, "The kodo walks with the Earth Mother."),
                              Icon("ability_mount_kodo_03"), "Brown Kodo", "Great Brown Kodo"),
        [Race.Troll]    = new(new("Xar'Ti", "Raptor Riding Instructor", Race.Troll, Gender.Female, "Keep your fingers away from da teeth, mon."),
                              Icon("ability_mount_raptor"), "Emerald Raptor", "Swift Blue Raptor"),
    };

    // Speech bubble lines of the riding trainers (random, never the same one twice in a row)
    public static readonly IReadOnlyList<string> GreetingLines =
        ["Ready to give your feet a rest?", "Every traveler needs a good mount.", "Steady now – it can smell fear."];
    public static readonly IReadOnlyList<string> TrainLines =
        ["Heels down, back straight. You'll do fine!", "Treat your mount well and it will carry you far.", "Now you're riding!"];
    public static readonly IReadOnlyList<string> RefuseLines =
        ["Riding lessons aren't free, friend.", "You are not ready for that yet.", "Come back when you are prepared."];
    public static readonly IReadOnlyList<string> FarewellLines =
        ["Safe travels!", "Ride well.", "Mind the low branches."];

    public static RidingTrainer TrainerFor(Race race) => ByRace[race].Trainer;

    /// <summary>Mount of the race for a rank from <see cref="Skills"/> (null for an unknown rank).</summary>
    public static Mount? Get(Race race, int rank)
    {
        if (Skills.FirstOrDefault(s => s.Rank == rank) is not { } skill) return null;
        var data = ByRace[race];
        return new Mount(rank == 1 ? data.Riding : data.Epic, data.IconUrl, skill);
    }

    public static IReadOnlyList<Mount> ForRace(Race race) =>
        Skills.Select(s => Get(race, s.Rank)!).ToList();
}
