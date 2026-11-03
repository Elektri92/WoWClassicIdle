// Models/DungeonRegistry.cs
namespace Models;

/// <summary>
/// All dungeons as data. They are combat zones (travel, routes, battlefield) but deliberately not part of
/// <see cref="CombatZoneRegistry.All"/>, which also feeds profession bonuses, objectives and exploration.
/// </summary>
public static class DungeonRegistry
{
    public const string DeadminesId        = "deadmines";
    public const string WailingCavernsId   = "wailing_caverns";
    public const string ShadowfangKeepId   = "shadowfang_keep";
    public const string BlackfathomDeepsId = "blackfathom_deeps";

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
        new(ShadowfangKeepId, "Shadowfang Keep", 22, 27, Landmass.EasternKingdoms, -11000, 1000, "shadowfang_keep",
            "A cursed keep haunted by the worgen of Arugal",
            [
                Mob("Wailing Guardsman", Archetypes.Undead,   "spell_shadow_raisedead"),
                Mob("Bleak Worg",        Archetypes.Wolf,     "ability_hunter_pet_wolf"),
                Mob("Haunted Servitor",  Archetypes.Humanoid, "spell_shadow_antishadow", 2),
            ],
            new DungeonDefinition("silverpine_forest", Encounters(
                Mob("Rethilgore",             Archetypes.Undead,   "spell_shadow_animatedead"),
                Mob("Razorclaw the Butcher",  Archetypes.Wolf,     "ability_creature_disease_02"),
                Mob("Baron Silverlaine",       Archetypes.Humanoid, "spell_shadow_possession"),
                Mob("Commander Springvale",    Archetypes.Humanoid, "inv_shield_06"),
                Mob("Archmage Arugal",         Archetypes.Humanoid, "spell_shadow_teleport")))),
        new(BlackfathomDeepsId, "Blackfathom Deeps", 26, 32, Landmass.Kalimdor, -4200, -700, "blackfathom_deeps",
            "Ancient ruins beneath Ashenvale, corrupted by the Old God Aku'mai",
            [
                Mob("Naga Siren",         Archetypes.Humanoid, "spell_frost_frostnova"),
                Mob("Murkshallow Crawler", Archetypes.Crocolisk, "ability_hunter_pet_crocolisk"),
                Mob("Twilight Acolyte",   Archetypes.Humanoid, "spell_shadow_shadowwordpain", 2),
            ],
            new DungeonDefinition("ashenvale", Encounters(
                Mob("Ghamoo-ra",      Archetypes.Crocolisk, "ability_hunter_pet_crocolisk"),
                Mob("Lady Sarevess",  Archetypes.Humanoid,  "spell_frost_frostbolt"),
                Mob("Gelihast",       Archetypes.Murloc,    "inv_misc_head_murloc_01"),
                Mob("Old Serra'kis",  Archetypes.Crocolisk, "inv_misc_monsterscales_15"),
                Mob("Aku'mai",        Archetypes.Demon,     "spell_shadow_demonbreath")))),
    ];
}
