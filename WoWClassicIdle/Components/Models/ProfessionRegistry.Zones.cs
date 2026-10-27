// Models/ProfessionRegistry.Zones.cs
using System.Collections.Concurrent;

namespace Models
{
    // Farmgebiete: jede Zone gibt es für alle Sammelberufe + Angeln, mit eigener Drop-Tabelle je Beruf.
    // Chancen sind Gewichte – RollDrop normiert über alle Drops der Zone.
    public static partial class ProfessionRegistry
    {
        private record ZoneInfo(string Id, string Name, string Theme, string Description);

        // Theme → CSS-Klasse zone-theme-* in professions.css
        private static readonly ZoneInfo Mulgore      = new("mulgore",             "Mulgore",             "grassland", "Rolling green plains of the Tauren");
        private static readonly ZoneInfo Barrens      = new("barrens",             "The Barrens",         "savanna",   "Sun-scorched savanna of Kalimdor");
        private static readonly ZoneInfo Duskwood     = new("duskwood",            "Duskwood",            "duskwood",  "Eternal night shrouds these haunted woods");
        private static readonly ZoneInfo Stranglethorn = new("stranglethorn",      "Stranglethorn Vale",  "jungle",    "Sweltering jungle full of trolls and tigers");
        private static readonly ZoneInfo Feralas      = new("feralas",             "Feralas",             "feralas",   "Ancient forests of towering trees");
        private static readonly ZoneInfo Felwood      = new("felwood",             "Felwood",             "felwood",   "A forest twisted by demonic corruption");
        private static readonly ZoneInfo Plaguelands  = new("eastern_plaguelands", "Eastern Plaguelands", "plague",    "Blighted lands ravaged by the Scourge");
        private static readonly ZoneInfo Winterspring = new("winterspring",        "Winterspring",        "snow",      "Frozen valleys at the edge of the world");

        private static GatheringZone Zone(ZoneInfo info, params GatherDrop[] drops) =>
            new(info.Id, info.Name, drops, info.Theme, info.Description);

        private static readonly Dictionary<Profession, IReadOnlyList<GatheringZone>> Zones;

        private static Dictionary<Profession, IReadOnlyList<GatheringZone>> BuildZones() => new()
        {
            [Profession.Herbalism] =
            [
                Zone(Mulgore,       new(Silverleaf, 45), new(Peacebloom, 45), new(Earthroot, 10)),
                Zone(Barrens,       new(Silverleaf, 30), new(Earthroot, 20), new(Mageroyal, 25), new(Briarthorn, 25)),
                Zone(Duskwood,      new(WildSteelbloom, 40), new(Bruiseweed, 40), new(Mageroyal, 10), new(Briarthorn, 10)),
                Zone(Stranglethorn, new(WildSteelbloom, 20), new(Bruiseweed, 20), new(Kingsblood, 30), new(Liferoot, 30)),
                Zone(Feralas,       new(Sungrass, 50), new(Blindweed, 50)),
                Zone(Felwood,       new(Gromsblood, 40), new(GoldenSansam, 20), new(Dreamfoil, 20), new(MountainSilversage, 20)),
                Zone(Plaguelands,   new(Plaguebloom, 39), new(GoldenSansam, 20), new(Dreamfoil, 20), new(MountainSilversage, 20), new(BlackLotus, 1)),
                Zone(Winterspring,  new(Plaguebloom, 39), new(GoldenSansam, 20), new(Dreamfoil, 20), new(MountainSilversage, 20), new(BlackLotus, 1)),
            ],
            [Profession.Mining] =
            [
                Zone(Mulgore,       new(CopperOre, 60), new(RoughStone, 40)),
                // Bronze-Zone: Kupfer/Zinn/Grobstein grob im Rezeptverhältnis 2:1:1 (Grobstein etwas mehr für Dynamit)
                Zone(Barrens,       new(CopperOre, 40), new(TinOre, 20), new(CoarseStone, 35), new(SilverOre, 5)),
                Zone(Duskwood,      new(TinOre, 30), new(IronOre, 30), new(HeavyStone, 35), new(SilverOre, 5)),
                Zone(Stranglethorn, new(IronOre, 50), new(MithrilOre, 15), new(SolidStone, 10), new(HeavyStone, 25)),
                Zone(Feralas,       new(MithrilOre, 40), new(SolidStone, 40), new(IronOre, 20)),
                Zone(Felwood,       new(DenseStone, 50), new(ThoriumOre, 10), new(MithrilOre, 30), new(TruesilverOre, 10)),
                Zone(Plaguelands,   new(DenseStone, 50), new(ThoriumOre, 30), new(MithrilOre, 15), new(TruesilverOre, 5)),
                Zone(Winterspring,  new(DenseStone, 30), new(ThoriumOre, 50), new(MithrilOre, 15), new(TruesilverOre, 5)),
            ],
            [Profession.Skinning] =
            [
                Zone(Mulgore,       new(RuinedLeatherScraps, 60), new(LightLeather, 40)),
                Zone(Barrens,       new(LightLeather, 60), new(MediumLeather, 40)),
                Zone(Duskwood,      new(HeavyLeather, 40), new(MediumLeather, 60)),
                Zone(Stranglethorn, new(ThickLeather, 40), new(HeavyLeather, 60)),
                Zone(Feralas,       new(ThickLeather, 60), new(HeavyLeather, 40)),
                Zone(Felwood,       new(RuggedLeather, 20), new(ThickLeather, 80)),
                Zone(Plaguelands,   new(RuggedLeather, 50), new(ThickLeather, 50)),
                Zone(Winterspring,  new(RuggedLeather, 70), new(ThickLeather, 30)),
            ],
            [Profession.Fishing] =
            [
                Zone(Mulgore,       new GatherDrop(RawBrilliantSmallfish, 100)),
                Zone(Barrens,       new(DeviateFish, 10), new(RawBrilliantSmallfish, 45), new(RawBristleWhiskerCatfish, 45)),
                Zone(Duskwood,      new(RawGreaterSagefish, 50), new(RawLongjawMudSnapper, 50)),
                Zone(Stranglethorn, new(RawGlossyMightfish, 50), new(RawNightfinSnapper, 50)),
                Zone(Feralas,       new(RawRedgill, 50), new(RawSpottedYellowtail, 50)),
                Zone(Felwood,       new(RawSummerBass, 50), new(RawWhitescaleSalmon, 50)),
                Zone(Plaguelands,   new(StonescaleEel, 20), new(RawSummerBass, 40), new(RawRedgill, 40)),
                Zone(Winterspring,  new(WinterSquid, 20), new(RawWhitescaleSalmon, 40), new(StonescaleEel, 40)),
            ],
        };

        /// <summary>Ab diesem Anteil prägt ein Drop die Zone – seltene Beigaben (Silbererz, Black Lotus) zählen nicht.</summary>
        public const double ZoneGateMinShare = 0.15;

        private static readonly ConcurrentDictionary<GatheringZone, int> _zoneSkill = new(ReferenceEqualityComparer.Instance);

        /// <summary>
        /// Benötigter Sammel-Skill einer Zone, abgeleitet aus den Rezepten: für jeden prägenden Drop der
        /// niedrigste Rezept-Skill, der ihn verarbeitet – die Zone verlangt den höchsten dieser Werte.
        /// </summary>
        public static int ZoneRequiredSkill(GatheringZone zone) => _zoneSkill.GetOrAdd(zone, ComputeZoneSkill);

        private static int ComputeZoneSkill(GatheringZone zone)
        {
            double total = zone.Drops.Sum(d => d.Chance);
            int skill = 1;
            foreach (var drop in zone.Drops.Where(d => d.Chance / total >= ZoneGateMinShare))
            {
                var uses = Recipes.Where(r => r.Reagents.Any(g => g.Item.Id == drop.Item.Id)).Select(r => r.RequiredSkill).ToList();
                if (uses.Count > 0) skill = Math.Max(skill, uses.Min());
            }
            return skill;
        }

        public static IReadOnlyList<GatheringZone> GetZones(Profession profession) =>
            Zones.TryGetValue(profession, out var zones) ? zones : [];

        public static GatheringZone? GetZone(Profession profession, string? zoneId) =>
            GetZones(profession).FirstOrDefault(z => z.Id == zoneId);

        /// <summary>Zieht einen Drop gewichtet nach Chance – es droppt immer etwas.</summary>
        public static GatherDrop RollDrop(GatheringZone zone, Random rng)
        {
            double roll = rng.NextDouble() * zone.Drops.Sum(d => d.Chance);
            foreach (var drop in zone.Drops)
            {
                roll -= drop.Chance;
                if (roll < 0) return drop;
            }
            return zone.Drops[^1];
        }
    }
}
