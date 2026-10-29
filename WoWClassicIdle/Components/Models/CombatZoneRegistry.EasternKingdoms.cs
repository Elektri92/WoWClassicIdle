// Models/CombatZoneRegistry.EasternKingdoms.cs
namespace Models;

// Eastern Kingdoms. Coordinates = approximate zone center in WoW world coordinates (X = north, Y = west).
// Methods only – static fields in partial files would have no fixed initialization order.
public static partial class CombatZoneRegistry
{
    private static IEnumerable<CombatZone> EasternKingdoms() =>
    [
        new("dun_morogh", "Dun Morogh", 1, 10, Landmass.EasternKingdoms, -5500, -400, "snow",
            "Snowy home of dwarves and gnomes",
            [
                Mob("Winter Wolf",      Archetypes.Wolf,  "ability_hunter_pet_wolf"),
                Mob("Rockjaw Trogg",    Archetypes.Humanoid, "inv_misc_monsterhead_02"),
                Mob("Frostmane Troll",  Archetypes.Troll, "inv_misc_head_troll_01", 2),
            ]),
        new("elwynn_forest", "Elwynn Forest", 1, 10, Landmass.EasternKingdoms, -9400, -100, "elwynn",
            "Peaceful woodlands around Stormwind",
            [
                Mob("Timber Wolf",      Archetypes.Wolf,     "ability_hunter_pet_wolf"),
                Mob("Kobold Miner",     Archetypes.Humanoid, "inv_misc_head_kobold_01"),
                Mob("Murloc Forager",   Archetypes.Murloc,   "inv_misc_head_murloc_01", 2),
            ]),
        new("tirisfal_glades", "Tirisfal Glades", 1, 10, Landmass.EasternKingdoms, 2200, 600, "tirisfal",
            "Gloomy glades above the Undercity",
            [
                Mob("Rotting Dead",     Archetypes.Undead,   "spell_shadow_animatedead"),
                Mob("Night Web Spider", Archetypes.Spider,   "ability_hunter_pet_spider"),
                Mob("Scarlet Zealot",   Archetypes.Humanoid, "inv_misc_head_human_01", 2),
            ]),
        new("loch_modan", "Loch Modan", 10, 20, Landmass.EasternKingdoms, -5300, -3100, "lochmodan",
            "Dwarven highlands behind the great dam",
            [
                Mob("Mountain Boar",       Archetypes.Boar,      "ability_hunter_pet_boar"),
                Mob("Stonesplinter Trogg", Archetypes.Humanoid,  "inv_misc_monsterhead_02"),
                Mob("Loch Crocolisk",      Archetypes.Crocolisk, "ability_hunter_pet_crocolisk", 2),
            ]),
        new("silverpine_forest", "Silverpine Forest", 10, 20, Landmass.EasternKingdoms, 300, 1300, "silverpine",
            "Misty pine forest haunted by worgen",
            [
                Mob("Moonrage Worgen", Archetypes.Humanoid, "ability_druid_ravage"),
                Mob("Rot Hide Gnoll",  Archetypes.Humanoid, "inv_misc_head_gnoll_01"),
                Mob("Mottled Worg",    Archetypes.Wolf,     "ability_hunter_pet_wolf", 2),
            ]),
        new("westfall", "Westfall", 10, 20, Landmass.EasternKingdoms, -10500, 1200, "westfall",
            "Abandoned farmland overrun by the Defias",
            [
                Mob("Defias Bandit",   Archetypes.Humanoid,  "ability_rogue_shadowstep"),
                Mob("Riverpaw Gnoll",  Archetypes.Humanoid,  "inv_misc_head_gnoll_01"),
                Mob("Harvest Watcher", Archetypes.Elemental, "inv_misc_monsterhead_04", 2),
            ]),
        new("redridge_mountains", "Redridge Mountains", 15, 25, Landmass.EasternKingdoms, -9300, -2300, "redridge",
            "Red cliffs besieged by Blackrock orcs",
            [
                Mob("Redridge Mystic",        Archetypes.Humanoid, "inv_misc_head_gnoll_01"),
                Mob("Blackrock Grunt",        Archetypes.Humanoid, "inv_misc_head_orc_01"),
                Mob("Black Dragon Whelp",     Archetypes.Dragon,   "inv_misc_head_dragon_black", 1),
            ]),
        new("duskwood", "Duskwood", 18, 30, Landmass.EasternKingdoms, -10600, -600, "duskwood",
            "Eternal night shrouds these haunted woods",
            [
                Mob("Nightbane Worgen", Archetypes.Humanoid, "ability_druid_ravage"),
                Mob("Skeletal Fiend",   Archetypes.Undead,   "spell_shadow_raisedead"),
                Mob("Venom Web Spider", Archetypes.Spider,   "ability_hunter_pet_spider", 2),
            ]),
        new("wetlands", "Wetlands", 20, 30, Landmass.EasternKingdoms, -3300, -2000, "wetlands",
            "Rain-soaked marshes around Menethil Harbor",
            [
                Mob("Mottled Raptor",  Archetypes.Raptor,   "ability_hunter_pet_raptor"),
                Mob("Bluegill Murloc", Archetypes.Murloc,   "inv_misc_head_murloc_01"),
                Mob("Dragonmaw Grunt", Archetypes.Humanoid, "inv_misc_head_orc_01", 2),
            ]),
        new("hillsbrad_foothills", "Hillsbrad Foothills", 20, 35, Landmass.EasternKingdoms, -500, -500, "hillsbrad",
            "Rolling hills between Southshore and Tarren Mill",
            [
                Mob("Syndicate Footpad",   Archetypes.Humanoid, "ability_rogue_disguise"),
                Mob("Gray Bear",           Archetypes.Bear,     "ability_hunter_pet_bear"),
                Mob("Elder Moss Creeper",  Archetypes.Spider,   "inv_misc_monsterspidercarapace_01", 2),
            ]),
        new("alterac_mountains", "Alterac Mountains", 30, 40, Landmass.EasternKingdoms, 400, 100, "snow",
            "Frozen peaks of a fallen kingdom",
            [
                Mob("Crushridge Ogre",      Archetypes.Ogre,     "ability_warstomp"),
                Mob("Alterac Yeti",         Archetypes.Gorilla,  "ability_hunter_pet_gorilla"),
                Mob("Syndicate Shadow Mage", Archetypes.Humanoid, "inv_staff_29", 2),
            ]),
        new("arathi_highlands", "Arathi Highlands", 30, 40, Landmass.EasternKingdoms, -1500, -2400, "arathi",
            "Windswept plains of ancient Stromgarde",
            [
                Mob("Witherbark Troll", Archetypes.Troll,  "inv_misc_head_troll_01"),
                Mob("Highland Raptor",  Archetypes.Raptor, "ability_hunter_pet_raptor"),
                Mob("Boulderfist Ogre", Archetypes.Ogre,   "inv_hammer_15", 2),
            ]),
        new("stranglethorn_vale", "Stranglethorn Vale", 30, 45, Landmass.EasternKingdoms, -12800, 0, "jungle",
            "Sweltering jungle full of trolls and tigers",
            [
                Mob("Stranglethorn Tiger", Archetypes.Cat,      "ability_hunter_pet_cat"),
                Mob("Bloodscalp Troll",    Archetypes.Troll,    "inv_misc_head_troll_01"),
                Mob("Bloodsail Buccaneer", Archetypes.Humanoid, "inv_helmet_66"),
            ]),
        new("badlands", "Badlands", 35, 45, Landmass.EasternKingdoms, -6700, -3000, "badlands",
            "Scorched red canyons scarred by dragonfire",
            [
                Mob("Dustbelcher Ogre", Archetypes.Ogre,   "ability_warstomp"),
                Mob("Ridge Stalker",    Archetypes.Cat,    "ability_hunter_pet_cat"),
                Mob("Scalding Whelp",   Archetypes.Dragon, "inv_misc_head_dragon_red", 1),
            ]),
        new("swamp_of_sorrows", "Swamp of Sorrows", 35, 45, Landmass.EasternKingdoms, -10300, -3400, "sorrows",
            "A drowned, mournful bog",
            [
                Mob("Sawtooth Crocolisk", Archetypes.Crocolisk, "ability_hunter_pet_crocolisk"),
                Mob("Swamp Jaguar",       Archetypes.Cat,       "ability_hunter_pet_cat"),
                Mob("Marsh Murloc",       Archetypes.Murloc,    "inv_misc_head_murloc_01", 2),
            ]),
        new("hinterlands", "The Hinterlands", 40, 50, Landmass.EasternKingdoms, 200, -3300, "hinterlands",
            "Lush wilds of the Wildhammer and Vilebranch",
            [
                Mob("Vilebranch Troll",   Archetypes.Troll, "inv_misc_head_troll_01"),
                Mob("Silvermane Stalker", Archetypes.Wolf,  "ability_hunter_pet_wolf"),
                Mob("Primitive Owlbeast", Archetypes.Bear,  "ability_eyeoftheowl", 2),
            ]),
        new("searing_gorge", "Searing Gorge", 45, 50, Landmass.EasternKingdoms, -6800, -1200, "volcanic",
            "A molten pit ruled by the Dark Iron",
            [
                Mob("Dark Iron Slaver",    Archetypes.DarkIron,  "inv_misc_head_dwarf_01"),
                Mob("Magma Elemental",     Archetypes.Elemental, "spell_fire_elemental_totem"),
                Mob("Searing Lava Spider", Archetypes.Spider,    "ability_hunter_pet_spider", 2),
            ]),
        new("blasted_lands", "Blasted Lands", 45, 55, Landmass.EasternKingdoms, -11600, -3000, "blasted",
            "Demon-blighted wastes before the Dark Portal",
            [
                Mob("Felhound",           Archetypes.Demon,    "spell_shadow_summonfelhunter"),
                Mob("Redstone Basilisk",  Archetypes.Basilisk, "inv_misc_monsterscales_02"),
                Mob("Snickerfang Hyena",  Archetypes.Hyena,    "ability_hunter_pet_hyena", 2),
            ]),
        new("burning_steppes", "Burning Steppes", 50, 58, Landmass.EasternKingdoms, -7900, -1800, "volcanic",
            "Ashen fields beneath Blackrock Mountain",
            [
                Mob("Blackrock Soldier",  Archetypes.Humanoid, "inv_misc_head_orc_01"),
                Mob("Firegut Brute",      Archetypes.Ogre,     "inv_hammer_15"),
                Mob("Black Dragonspawn",  Archetypes.Dragon,   "inv_misc_head_dragon_black", 1),
            ]),
        new("western_plaguelands", "Western Plaguelands", 51, 58, Landmass.EasternKingdoms, 1600, -1500, "plague",
            "Scourge-tainted farmlands of Lordaeron",
            [
                Mob("Scarlet Crusader", Archetypes.Humanoid, "inv_misc_head_human_01"),
                Mob("Skeletal Warder",  Archetypes.Undead,   "spell_shadow_raisedead"),
                Mob("Diseased Wolf",    Archetypes.Wolf,     "ability_creature_disease_02", 2),
            ]),
        new("eastern_plaguelands", "Eastern Plaguelands", 53, 60, Landmass.EasternKingdoms, 2000, -4000, "plague",
            "Blighted lands ravaged by the Scourge",
            [
                Mob("Cannibal Ghoul",      Archetypes.Undead, "inv_misc_head_scourge_01"),
                Mob("Crypt Fiend",         Archetypes.Spider, "inv_misc_monsterspidercarapace_01"),
                Mob("Stonespine Gargoyle", Archetypes.Undead, "ability_hunter_pet_bat", 2),
            ]),
        new("deadwind_pass", "Deadwind Pass", 55, 60, Landmass.EasternKingdoms, -10900, -1900, "deadwind",
            "A lifeless ravine in Karazhan's shadow",
            [
                Mob("Deadwind Brute", Archetypes.Ogre,   "ability_warstomp"),
                Mob("Restless Shade", Archetypes.Undead, "spell_shadow_antishadow"),
                Mob("Sky Shadow",     Archetypes.Bird,   "ability_hunter_pet_vulture", 2),
            ]),
    ];
}
