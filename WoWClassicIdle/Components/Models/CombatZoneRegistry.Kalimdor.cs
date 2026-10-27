// Models/CombatZoneRegistry.Kalimdor.cs
namespace Models;

// Kalimdor (+ Insel Teldrassil). Koordinaten = ungefähre Zonenmitte in WoW-Weltkoordinaten (X = Norden, Y = Westen).
// Nur Methoden – statische Felder in Partial-Dateien hätten keine feste Initialisierungsreihenfolge.
public static partial class CombatZoneRegistry
{
    private static IEnumerable<CombatZone> Kalimdor() =>
    [
        new("durotar", "Durotar", 1, 10, Landmass.Kalimdor, 300, -4800, "durotar",
            "Harsh red canyons of the orcish homeland",
            [
                Mob("Mottled Boar",        Archetypes.Boar,     "ability_hunter_pet_boar"),
                Mob("Clattering Scorpid",  Archetypes.Scorpid,  "ability_hunter_pet_scorpid"),
                Mob("Razormane Quilboar",  Archetypes.Humanoid, "inv_misc_head_quillboar_01", 2),
            ]),
        new("mulgore", "Mulgore", 1, 10, Landmass.Kalimdor, -2200, -300, "grassland",
            "Rolling green plains of the Tauren",
            [
                Mob("Plainstrider",    Archetypes.Bird,     "ability_hunter_pet_tallstrider"),
                Mob("Prairie Wolf",    Archetypes.Wolf,     "ability_hunter_pet_wolf"),
                Mob("Palemane Gnoll",  Archetypes.Humanoid, "inv_misc_head_gnoll_01", 2),
            ]),
        new("teldrassil", "Teldrassil", 1, 10, Landmass.Teldrassil, 9800, 1200, "teldrassil",
            "Twilight forests atop the World Tree",
            [
                Mob("Nightsaber",         Archetypes.Cat,      "ability_hunter_pet_cat"),
                Mob("Webwood Spider",     Archetypes.Spider,   "ability_hunter_pet_spider"),
                Mob("Gnarlpine Furbolg",  Archetypes.Humanoid, "ability_racial_bearform", 2),
            ]),
        new("darkshore", "Darkshore", 10, 20, Landmass.Kalimdor, 5800, 300, "darkshore",
            "Moonlit coast around Auberdine",
            [
                Mob("Thistle Bear",       Archetypes.Bear,      "ability_hunter_pet_bear"),
                Mob("Blackwood Furbolg",  Archetypes.Humanoid,  "ability_racial_bearform"),
                Mob("Darkshore Thresher", Archetypes.Crocolisk, "inv_misc_monsterscales_15", 2),
            ]),
        new("barrens", "The Barrens", 10, 25, Landmass.Kalimdor, -1500, -2600, "savanna",
            "Sun-scorched savanna of Kalimdor",
            [
                Mob("Savannah Prowler", Archetypes.Cat,      "ability_hunter_pet_cat"),
                Mob("Kolkar Centaur",   Archetypes.Humanoid, "inv_misc_head_centaur_01"),
                Mob("Barrens Kodo",     Archetypes.Kodo,     "ability_mount_kodo_01", 2),
            ]),
        new("stonetalon_mountains", "Stonetalon Mountains", 15, 27, Landmass.Kalimdor, 1500, 1300, "stonetalon",
            "Rocky peaks logged bare by the Venture Co.",
            [
                Mob("Venture Co. Logger", Archetypes.Goblin,   "inv_misc_bomb_07"),
                Mob("Deepmoss Spider",    Archetypes.Spider,   "ability_hunter_pet_spider"),
                Mob("Bloodfury Harpy",    Archetypes.Humanoid, "inv_feather_04", 2),
            ]),
        new("ashenvale", "Ashenvale", 18, 30, Landmass.Kalimdor, 2500, -800, "ashenvale",
            "Ancient night elf forest of eternal twilight",
            [
                Mob("Ashenvale Bear",    Archetypes.Bear,     "ability_hunter_pet_bear"),
                Mob("Foulweald Furbolg", Archetypes.Humanoid, "ability_racial_bearform"),
                Mob("Xavian Satyr",      Archetypes.Demon,    "spell_shadow_lifedrain02", 2),
            ]),
        new("thousand_needles", "Thousand Needles", 25, 35, Landmass.Kalimdor, -5300, -1800, "needles",
            "Towering stone spires above a dusty canyon",
            [
                Mob("Galak Centaur",     Archetypes.Humanoid, "inv_misc_head_centaur_01"),
                Mob("Pesterhide Hyena",  Archetypes.Hyena,    "ability_hunter_pet_hyena"),
                Mob("Grimtotem Brute",   Archetypes.Ogre,     "inv_misc_head_tauren_01", 2),
            ]),
        new("desolace", "Desolace", 30, 40, Landmass.Kalimdor, -300, 1700, "desolace",
            "Grey, barren wastes of warring centaur",
            [
                Mob("Gelkis Centaur",    Archetypes.Humanoid, "inv_misc_head_centaur_01"),
                Mob("Scorpashi Lasher",  Archetypes.Scorpid,  "ability_hunter_pet_scorpid"),
                Mob("Hatefury Satyr",    Archetypes.Demon,    "spell_shadow_lifedrain02", 2),
            ]),
        new("dustwallow_marsh", "Dustwallow Marsh", 35, 45, Landmass.Kalimdor, -3700, -3800, "swamp",
            "Murky swamp around Theramore Isle",
            [
                Mob("Drywallow Crocolisk", Archetypes.Crocolisk, "ability_hunter_pet_crocolisk"),
                Mob("Darkmist Spider",     Archetypes.Spider,    "ability_hunter_pet_spider"),
                Mob("Firemane Scalebane",  Archetypes.Dragon,    "inv_misc_head_dragon_red", 1),
            ]),
        new("feralas", "Feralas", 40, 50, Landmass.Kalimdor, -4500, 1500, "feralas",
            "Ancient forests of towering trees",
            [
                Mob("Ironfur Bear",    Archetypes.Bear,     "ability_hunter_pet_bear"),
                Mob("Gordunni Ogre",   Archetypes.Ogre,     "ability_warstomp"),
                Mob("Woodpaw Gnoll",   Archetypes.Humanoid, "inv_misc_head_gnoll_01", 2),
            ]),
        new("tanaris", "Tanaris", 40, 50, Landmass.Kalimdor, -7500, -3500, "tanaris",
            "Endless desert around Gadgetzan",
            [
                Mob("Wastewander Bandit",   Archetypes.Humanoid, "inv_misc_bandana_03"),
                Mob("Southsea Freebooter",  Archetypes.Humanoid, "inv_helmet_66"),
                Mob("Glasshide Basilisk",   Archetypes.Basilisk, "inv_misc_monsterscales_02", 2),
            ]),
        new("azshara", "Azshara", 45, 55, Landmass.Kalimdor, 3200, -4700, "azshara",
            "Autumn-hued ruins of a fallen empire",
            [
                Mob("Haldarr Satyr",           Archetypes.Demon,    "spell_shadow_lifedrain02"),
                Mob("Spitelash Naga",          Archetypes.Humanoid, "inv_misc_monstertail_03"),
                Mob("Thunderhead Hippogryph",  Archetypes.Bird,     "ability_druid_flightform", 2),
            ]),
        new("ungoro_crater", "Un'Goro Crater", 48, 55, Landmass.Kalimdor, -7000, -1200, "ungoro",
            "Primeval jungle crater teeming with dinosaurs",
            [
                Mob("Ravasaur Runner", Archetypes.Raptor,  "ability_hunter_pet_raptor"),
                Mob("Un'Goro Gorilla", Archetypes.Gorilla, "ability_hunter_pet_gorilla"),
                Mob("Tar Creeper",     Archetypes.Ooze,    "inv_misc_slime_01", 2),
            ]),
        new("felwood", "Felwood", 48, 55, Landmass.Kalimdor, 5000, -1000, "felwood",
            "A forest twisted by demonic corruption",
            [
                Mob("Jadefire Satyr",   Archetypes.Demon,    "spell_shadow_lifedrain02"),
                Mob("Deadwood Furbolg", Archetypes.Humanoid, "ability_racial_bearform"),
                Mob("Felpaw Ravager",   Archetypes.Wolf,     "ability_creature_disease_02", 2),
            ]),
        new("winterspring", "Winterspring", 53, 60, Landmass.Kalimdor, 6700, -4400, "snow",
            "Frozen valleys at the edge of the world",
            [
                Mob("Winterfall Furbolg", Archetypes.Humanoid, "ability_racial_bearform"),
                Mob("Frostsaber Stalker", Archetypes.Cat,      "ability_mount_whitetiger"),
                Mob("Chillwind Chimaera", Archetypes.Bird,     "ability_hunter_pet_windserpent", 2),
            ]),
        new("moonglade", "Moonglade", 55, 60, Landmass.Kalimdor, 7800, -2400, "moonglade",
            "Sacred druidic sanctuary beneath the moon",
            [
                Mob("Rabid Moonkin",      Archetypes.Bear,     "ability_eyeoftheowl"),
                Mob("Deadwood Shaman",    Archetypes.Humanoid, "ability_racial_bearform"),
                Mob("Emerald Dragonkin",  Archetypes.Dragon,   "inv_misc_head_dragon_green", 1),
            ]),
        new("silithus", "Silithus", 55, 60, Landmass.Kalimdor, -7000, 900, "silithus",
            "Insect-infested desert before Ahn'Qiraj",
            [
                Mob("Hive'Zora Wasp",    Archetypes.Insect,   "ability_hunter_pet_wasp"),
                Mob("Dredge Striker",    Archetypes.Insect,   "ability_hunter_pet_silithid"),
                Mob("Twilight Geolord",  Archetypes.Humanoid, "inv_staff_29", 2),
            ]),
    ];
}
