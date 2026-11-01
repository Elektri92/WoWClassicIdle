// Models/DungeonRegistry.cs
namespace Models;

/// <summary>
/// All dungeons as data. They are combat zones (travel, routes, battlefield) but deliberately not part of
/// <see cref="CombatZoneRegistry.All"/>, which also feeds profession bonuses, objectives and exploration.
/// </summary>
public static class DungeonRegistry
{
    public const string DeadminesId      = "deadmines";
    public const string WailingCavernsId = "wailing_caverns";

    private const int TrashPerGroup = 2;

    private static string Icon(string name) => $"https://wow.zamimg.com/images/wow/icons/large/{name}.jpg";

    private static ZoneMob Mob(string name, EnemyArchetype archetype, string icon, int weight = 3) =>
        new(name, archetype, Icon(icon), weight);

    // A trash group in front of every boss
    private static IReadOnlyList<DungeonEncounter> Encounters(params ZoneMob[] bosses) =>
        bosses.SelectMany((boss, index) => new[]
        {
            new DungeonEncounter(TrashPerGroup),
            new DungeonEncounter(1, boss, index),
        }).ToList();

    public static IReadOnlyList<CombatZone> All { get; } =
    [
        new(WailingCavernsId, "Wailing Caverns", 17, 24, Landmass.Kalimdor, -750, -2200, "wailing",
            "Poisoned caves where the Druids of the Fang twist nature",
            [
                Mob("Druid of the Fang", Archetypes.Humanoid,  "spell_nature_regeneration"),
                Mob("Deviate Viper",     Archetypes.Spider,    "spell_nature_nullifypoison"),
                Mob("Deviate Crocolisk", Archetypes.Crocolisk, "ability_hunter_pet_crocolisk", 2),
            ],
            new DungeonDefinition("barrens", Encounters(
                Mob("Lady Anacondra",       Archetypes.Humanoid, "spell_nature_starfall"),
                Mob("Lord Cobrahn",         Archetypes.Humanoid, "ability_hunter_pet_windserpent"),
                Mob("Lord Pythas",          Archetypes.Humanoid, "spell_nature_lightning"),
                Mob("Lord Serpentis",       Archetypes.Humanoid, "spell_nature_thorns"),
                Mob("Mutanus the Devourer", Archetypes.Murloc,   "spell_shadow_mindsteal")))),
        new(DeadminesId, "The Deadmines", 18, 23, Landmass.EasternKingdoms, -11200, 1650, "deadmines",
            "The hidden shipyard of the Defias Brotherhood",
            [
                Mob("Defias Miner",    Archetypes.Humanoid, "inv_pick_01"),
                Mob("Goblin Engineer", Archetypes.Goblin,   "inv_misc_gear_01"),
                Mob("Defias Pirate",   Archetypes.Humanoid, "ability_rogue_shadowstep", 2),
            ],
            new DungeonDefinition("westfall", Encounters(
                Mob("Rhahk'Zor",      Archetypes.Ogre,     "inv_hammer_05"),
                Mob("Sneed",          Archetypes.Goblin,   "inv_misc_gear_02"),
                Mob("Gilnid",         Archetypes.Goblin,   "inv_misc_gear_03"),
                Mob("Mr. Smite",      Archetypes.Humanoid, "inv_hammer_10"),
                Mob("Edwin VanCleef", Archetypes.Humanoid, "ability_rogue_shadowstep")))),
    ];
}
