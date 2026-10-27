// Models/ZoneTravel.cs
namespace Models;

public enum TravelMode { Foot, Form, Mount, Flight, Ship, Zeppelin }

/// <summary>Ein Reiseabschnitt; <see cref="Theme"/> = Farbwelt des Ziels (CSS zone-theme-*).</summary>
public record TravelLeg(TravelMode Mode, string From, string To, double Seconds, string Theme);

/// <summary>Komplette Route; <see cref="Seconds"/> = Summe aller Abschnitte.</summary>
public record TravelRoute(IReadOnlyList<TravelLeg> Legs)
{
    public double Seconds => Legs.Sum(l => l.Seconds);
}

/// <summary>Laufende Reise eines Charakters (Fortschritt in Spielsekunden).</summary>
public class ZoneTravelState
{
    public required string FromZoneId { get; init; }
    public required string TargetZoneId { get; init; }
    public required TravelRoute Route { get; init; }
    public double Elapsed { get; internal set; }

    public double Duration  => Route.Seconds;
    public double Remaining => Math.Max(0, Duration - Elapsed);
    public double Progress  => Duration > 0 ? Math.Clamp(Elapsed / Duration, 0, 1) : 1;
    public bool IsArrived   => Elapsed >= Duration;

    /// <summary>Aktueller Abschnitt und Fortschritt darin (0–1).</summary>
    public (TravelLeg Leg, double Progress)? CurrentLeg
    {
        get
        {
            double start = 0;
            foreach (var leg in Route.Legs)
            {
                if (Elapsed < start + leg.Seconds || leg == Route.Legs[^1])
                    return (leg, leg.Seconds > 0 ? Math.Clamp((Elapsed - start) / leg.Seconds, 0, 1) : 1);
                start += leg.Seconds;
            }
            return null;
        }
    }
}

/// <summary>Wie sich der Charakter am Boden fortbewegt (Laufen, Klassen-Gestalt oder Reittier).</summary>
public record GroundTravel(TravelMode Mode, string Name, string? IconUrl, double SpeedMultiplier);

/// <summary>
/// Reisezeiten zwischen Kampfzonen, angelehnt an echte WoW-Classic-Wege: auf derselben Landmasse laufen/reiten
/// oder Greifen/Windreiter fliegen (das Schnellere), zwischen Landmassen per Schiff oder Zeppelin (je nach Fraktion).
/// </summary>
public static class ZoneTravel
{
    private const double RunSpeed       = 7.0;   // Yards/s zu Fuß (100 %)
    private const double GroundDetour   = 1.25;  // Wege sind keine Luftlinie
    private const double FlightSpeed    = 32.0;  // Yards/s Flugroute
    private const double FlightDetour   = 1.15;
    private const double FlightOverhead = 15.0;  // Start und Landung
    private const double ZoneApproach   = 350;   // Yards vom Jagdgebiet zum Flugmeister der Zone (Häfen haben ihn direkt am Dock)

    private static string Icon(string name) => $"https://wow.zamimg.com/images/wow/icons/large/{name}.jpg";

    /// <summary>Hafen bzw. Zeppelinturm; <see cref="ZoneId"/> = Zone, in der er liegt (für Farbe/Beschriftung).</summary>
    private record Port(string Name, string ZoneId, Landmass Landmass, double X, double Y);

    private static readonly Port Menethil  = new("Menethil Harbor",  "wetlands",           Landmass.EasternKingdoms, -3700,  -600);
    private static readonly Port BootyBay  = new("Booty Bay",        "stranglethorn_vale", Landmass.EasternKingdoms, -14300,  500);
    private static readonly Port GromGol   = new("Grom'gol",         "stranglethorn_vale", Landmass.EasternKingdoms, -12400,  200);
    private static readonly Port Undercity = new("Undercity",        "tirisfal_glades",    Landmass.EasternKingdoms,  2060,   290);
    private static readonly Port Theramore = new("Theramore",        "dustwallow_marsh",   Landmass.Kalimdor,       -3800, -4500);
    private static readonly Port Auberdine = new("Auberdine",        "darkshore",          Landmass.Kalimdor,        6400,   500);
    private static readonly Port Ratchet   = new("Ratchet",          "barrens",            Landmass.Kalimdor,        -950, -3700);
    private static readonly Port Orgrimmar = new("Orgrimmar",        "durotar",            Landmass.Kalimdor,        1350, -4650);
    private static readonly Port Ruttheran = new("Rut'theran Village", "teldrassil",       Landmass.Teldrassil,      8550,  1000);

    /// <summary>Schiffs-/Zeppelinlinie; Dauer inkl. durchschnittlicher Wartezeit. Faction null = neutral.</summary>
    private record Transport(Port A, Port B, TravelMode Mode, double Seconds, Faction? Faction);

    private static readonly Transport[] Transports =
    [
        new(Menethil,  Theramore, TravelMode.Ship,     150, Faction.Alliance),
        new(Menethil,  Auberdine, TravelMode.Ship,     180, Faction.Alliance),
        new(Auberdine, Ruttheran, TravelMode.Ship,      60, null),
        new(BootyBay,  Ratchet,   TravelMode.Ship,     150, null),
        new(Orgrimmar, Undercity, TravelMode.Zeppelin, 150, Faction.Horde),
        new(Orgrimmar, GromGol,   TravelMode.Zeppelin, 150, Faction.Horde),
    ];

    private static readonly Port[] Ports = [Menethil, BootyBay, GromGol, Undercity, Theramore, Auberdine, Ratchet, Orgrimmar, Ruttheran];

    // ─── Bodenreise je Charakter ───

    private static string RaceMountIcon(Race race) => race switch
    {
        Race.Dwarf    => Icon("ability_mount_mountainram"),
        Race.Gnome    => Icon("ability_mount_mechastrider"),
        Race.NightElf => Icon("ability_mount_whitetiger"),
        Race.Orc      => Icon("ability_mount_blackdirewolf"),
        Race.Troll    => Icon("ability_mount_raptor"),
        Race.Tauren   => Icon("ability_mount_kodo_03"),
        Race.Undead   => Icon("ability_mount_undeadhorse"),
        _             => Icon("ability_mount_ridinghorse"),
    };

    /// <summary>
    /// Schnellste Bodenreise des Charakters: Reittier ab 40 (60 %, episch ab 60 mit 100 %), Paladin/Hexenmeister
    /// mit Klassen-Reittier, davor Reisegestalt (Druide 30), Geisterwolf (Schamane 20) bzw. Aspekt des Geparden (Jäger 20).
    /// </summary>
    public static GroundTravel GroundFor(CharacterClass cls, Race race, int level)
    {
        if (level >= 60)
            return cls switch
            {
                CharacterClass.Paladin => new(TravelMode.Mount, "Charger", Icon("ability_mount_charger"), 2.0),
                CharacterClass.Warlock => new(TravelMode.Mount, "Dreadsteed", Icon("ability_mount_dreadsteed"), 2.0),
                _                      => new(TravelMode.Mount, "Epic mount", RaceMountIcon(race), 2.0),
            };
        if (level >= 40)
            return cls switch
            {
                CharacterClass.Paladin => new(TravelMode.Mount, "Warhorse", Icon("spell_nature_swiftness"), 1.6),
                CharacterClass.Warlock => new(TravelMode.Mount, "Felsteed", Icon("spell_nature_swiftness"), 1.6),
                _                      => new(TravelMode.Mount, "Riding mount", RaceMountIcon(race), 1.6),
            };
        if (cls == CharacterClass.Druid && level >= 30)
            return new(TravelMode.Form, "Travel Form", Icon("ability_druid_travelform"), 1.4);
        if (cls == CharacterClass.Shaman && level >= 20)
            return new(TravelMode.Form, "Ghost Wolf", Icon("spell_nature_spiritwolf"), 1.4);
        if (cls == CharacterClass.Hunter && level >= 20)
            return new(TravelMode.Form, "Aspect of the Cheetah", Icon("ability_mount_jungletiger"), 1.3);
        return new(TravelMode.Foot, "On foot", null, 1.0);
    }

    public static string FlightIcon(Faction faction) =>
        faction == Faction.Horde ? Icon("ability_mount_wyvern_01") : Icon("ability_mount_gryphon_01");

    // ─── Routensuche ───

    private record Node(string Name, string Theme, Landmass Landmass, double X, double Y, double Approach);

    /// <summary>Schnellste Route von <paramref name="from"/> nach <paramref name="to"/> (Dijkstra über Zonen + Häfen).</summary>
    public static TravelRoute Route(CombatZone from, CombatZone to, Faction faction, GroundTravel ground)
    {
        if (from.Id == to.Id) return new TravelRoute([]);

        var nodes = new List<Node>
        {
            new(from.Name, from.Theme, from.Landmass, from.X, from.Y, ZoneApproach),
            new(to.Name,   to.Theme,   to.Landmass,   to.X,   to.Y,   ZoneApproach),
        };
        foreach (var port in Ports)
            nodes.Add(new(port.Name, CombatZoneRegistry.Get(port.ZoneId)?.Theme ?? to.Theme, port.Landmass, port.X, port.Y, 0));

        var transports = Transports.Where(t => t.Faction is null || t.Faction == faction).ToList();
        int n = nodes.Count;
        var best = Enumerable.Repeat(double.MaxValue, n).ToArray();
        var prev = new IReadOnlyList<TravelLeg>?[n];
        var prevNode = Enumerable.Repeat(-1, n).ToArray();
        var done = new bool[n];
        best[0] = 0;

        for (int iter = 0; iter < n; iter++)
        {
            int u = -1;
            for (int i = 0; i < n; i++)
                if (!done[i] && best[i] < double.MaxValue && (u < 0 || best[i] < best[u])) u = i;
            if (u < 0 || u == 1) break;
            done[u] = true;

            for (int v = 0; v < n; v++)
            {
                if (done[v] || v == u) continue;
                var hop = Hop(nodes[u], nodes[v], transports, ground);
                if (hop is null) continue;
                double seconds = hop.Sum(l => l.Seconds);
                if (best[u] + seconds >= best[v]) continue;
                best[v] = best[u] + seconds;
                prev[v] = hop;
                prevNode[v] = u;
            }
        }

        var legs = new List<TravelLeg>();
        for (int v = 1; prevNode[v] >= 0; v = prevNode[v])
            legs.InsertRange(0, prev[v]!);
        return new TravelRoute(legs);
    }

    /// <summary>
    /// Direkter Weg zwischen zwei Knoten: auf derselben Landmasse Boden oder Flugroute (das Schnellere; zum Flugmeister
    /// und vom Landeplatz ins Jagdgebiet geht es zu Fuß bzw. per Reittier), sonst Schiff/Zeppelin zwischen zwei Häfen.
    /// </summary>
    private static IReadOnlyList<TravelLeg>? Hop(Node a, Node b, List<Transport> transports, GroundTravel ground)
    {
        if (a.Landmass == b.Landmass)
        {
            double distance = Math.Sqrt(Math.Pow(a.X - b.X, 2) + Math.Pow(a.Y - b.Y, 2));
            var direct = GroundLeg(a.Name, b.Name, distance, b.Theme, ground);

            var viaFlight = new List<TravelLeg>();
            if (a.Approach > 0) viaFlight.Add(GroundLeg(a.Name, FlightMaster, a.Approach, a.Theme, ground));
            viaFlight.Add(new TravelLeg(TravelMode.Flight, a.Name, b.Name, Math.Round(FlightOverhead + distance * FlightDetour / FlightSpeed), b.Theme));
            if (b.Approach > 0) viaFlight.Add(GroundLeg(FlightMaster, b.Name, b.Approach, b.Theme, ground));

            return direct.Seconds <= viaFlight.Sum(l => l.Seconds) ? [direct] : viaFlight;
        }

        var line = transports.FirstOrDefault(t => (t.A.Name == a.Name && t.B.Name == b.Name) || (t.A.Name == b.Name && t.B.Name == a.Name));
        return line is null ? null : [new TravelLeg(line.Mode, a.Name, b.Name, line.Seconds, b.Theme)];
    }

    private const string FlightMaster = "Flight master";

    private static TravelLeg GroundLeg(string from, string to, double yards, string theme, GroundTravel ground) =>
        new(ground.Mode, from, to, Math.Round(yards * GroundDetour / (RunSpeed * ground.SpeedMultiplier)), theme);

    public static string ModeLabel(TravelMode mode, GroundTravel ground) => mode switch
    {
        TravelMode.Flight   => "Flight path",
        TravelMode.Ship     => "Ship",
        TravelMode.Zeppelin => "Zeppelin",
        _                   => ground.Name,
    };

    public static string FormatDuration(double seconds)
    {
        int s = (int)Math.Ceiling(Math.Max(0, seconds));
        return s >= 60 ? $"{s / 60}m {s % 60:00}s" : $"{s}s";
    }
}
