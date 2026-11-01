// Models/DungeonPartyRegistry.cs
namespace Models;

public enum DungeonRole { Tank, Healer, Damage }

public enum NpcSpellKind { Damage, Heal, Mitigation }

/// <summary>
/// Spell of an NPC party member, cast automatically every <paramref name="Interval"/> seconds.
/// <paramref name="Reduction"/>/<paramref name="Duration"/> only apply to <see cref="NpcSpellKind.Mitigation"/>.
/// </summary>
public record NpcSpell(string Name, string Icon, NpcSpellKind Kind, double Interval, double Reduction = 0, double Duration = 0, bool Melee = false)
{
    public string IconUrl => $"https://wow.zamimg.com/images/wow/icons/large/{Icon}.jpg";

    /// <summary>Time the animation flies from the companion to its target.</summary>
    public int FlightMs => Melee ? 240 : 420;

    public string Description => Kind switch
    {
        NpcSpellKind.Damage => $"Strikes the enemy every {Interval:0.0} s",
        NpcSpellKind.Heal   => $"Restores {HealShare * 100:0.#}% of your health every {Interval:0.0} s when you are hurt",
        _                   => $"Reduces the damage you take by {Reduction * 100:0}% for {Duration:0} s, every {Interval:0} s",
    };

    public double HealShare => DungeonRules.HealerHpPerSecond * Interval;

    /// <summary>Damage of one cast at the enemy level, before variance.</summary>
    public int DamageAt(int level) =>
        (int)Math.Round(EnemyScaling.Hp(level, Archetypes.Humanoid) * DungeonRules.NpcDamagePerSecond * Interval);
}

/// <summary>Drawn NPC party member with the spell it casts on its own.</summary>
public record DungeonPartyMember(string Name, Race Race, Gender Gender, CharacterClass Class, DungeonRole Role, NpcSpell Spell);

/// <summary>Chosen group: own role plus one class per NPC slot, in the order of <see cref="DungeonPartyRegistry.NpcSlots"/>.</summary>
public record DungeonPartySetup(DungeonRole SelfRole, IReadOnlyList<CharacterClass> Companions);

/// <summary>Runtime state of an NPC during a run (memory only, rebuilt on load).</summary>
public sealed class DungeonPartyMemberState
{
    public DungeonPartyMember Member { get; }

    /// <summary>The spell as an ability: own meter row per NPC, for tanks also the buff on the player.</summary>
    public Ability SpellAbility { get; }

    public double Cooldown { get; internal set; }
    public double MitigationRemaining { get; internal set; }
    /// <summary>Seconds left of the cast highlight on the party card.</summary>
    public double CastFlash { get; internal set; }

    private int _hpLevel;
    private int _hp;

    /// <summary>Health the companion adds to the shared bar: the base health of its class and race at that level.</summary>
    public int MaxHpAt(int level)
    {
        if (level != _hpLevel)
        {
            _hp = StatsFactory.GetBaseStats(Member.Class, Member.Race, level).MaxHp;
            _hpLevel = level;
        }
        return _hp;
    }

    public DungeonPartyMemberState(DungeonPartyMember member, double initialCooldown)
    {
        Member      = member;
        Cooldown    = initialCooldown;
        SpellAbility   = new Ability
        {
            Id = $"npc_{member.Name}",
            Name = $"{member.Spell.Name} ({member.Name})",
            IconUrl = member.Spell.IconUrl,
            BuffDuration = member.Spell.Duration,
            BuffDamageTakenReduction = member.Spell.Reduction,
        };
    }
}

/// <summary>
/// Groups always have 1 tank, 1 healer and 3 damage dealers; the own role takes one of those slots.
/// Classes and races follow <see cref="RaceClassInfo"/>, names depend only on character and slot, so they stay stable.
/// </summary>
public static class DungeonPartyRegistry
{
    private static readonly Dictionary<DungeonRole, Dictionary<CharacterClass, NpcSpell>> Spells = new()
    {
        [DungeonRole.Tank] = new()
        {
            [CharacterClass.Warrior] = new("Shield Block",      "ability_defend",                  NpcSpellKind.Mitigation, 8,  0.40, 6),
            [CharacterClass.Paladin] = new("Holy Shield",       "spell_holy_blessingofprotection", NpcSpellKind.Mitigation, 8,  0.35, 7),
            [CharacterClass.Druid]   = new("Demoralizing Roar", "ability_druid_demoralizingroar",  NpcSpellKind.Mitigation, 10, 0.30, 10),
        },
        [DungeonRole.Healer] = new()
        {
            [CharacterClass.Priest]  = new("Flash Heal",   "spell_holy_flashheal",        NpcSpellKind.Heal, 2.5),
            [CharacterClass.Paladin] = new("Holy Light",   "spell_holy_holybolt",         NpcSpellKind.Heal, 5),
            [CharacterClass.Shaman]  = new("Healing Wave", "spell_nature_magicimmunity",  NpcSpellKind.Heal, 4),
            [CharacterClass.Druid]   = new("Regrowth",     "spell_nature_resistnature",   NpcSpellKind.Heal, 3),
        },
        [DungeonRole.Damage] = new()
        {
            [CharacterClass.Mage]    = new("Frostbolt",       "spell_frost_frostbolt02",        NpcSpellKind.Damage, 3),
            [CharacterClass.Warlock] = new("Shadow Bolt",     "spell_shadow_shadowbolt",        NpcSpellKind.Damage, 3.5),
            [CharacterClass.Hunter]  = new("Aimed Shot",      "inv_spear_07",                   NpcSpellKind.Damage, 3),
            [CharacterClass.Rogue]   = new("Sinister Strike", "spell_shadow_ritualofsacrifice", NpcSpellKind.Damage, 1.5, Melee: true),
            [CharacterClass.Warrior] = new("Heroic Strike",   "ability_rogue_ambush",           NpcSpellKind.Damage, 2.5, Melee: true),
            [CharacterClass.Paladin] = new("Holy Strike",     "ability_thunderbolt",            NpcSpellKind.Damage, 2.5, Melee: true),
            [CharacterClass.Priest]  = new("Mind Blast",      "spell_shadow_unholyfrenzy",      NpcSpellKind.Damage, 3),
            [CharacterClass.Shaman]  = new("Lightning Bolt",  "spell_nature_lightning",         NpcSpellKind.Damage, 2.5),
            [CharacterClass.Druid]   = new("Moonfire",        "spell_nature_starfall",          NpcSpellKind.Damage, 2),
        },
    };

    private static readonly Dictionary<Race, (string Name, Gender Gender)[]> Names = new()
    {
        [Race.Human]    = [("Aldric", Gender.Male), ("Mirelle", Gender.Female), ("Corwin", Gender.Male), ("Isolde", Gender.Female)],
        [Race.Dwarf]    = [("Thorgrim", Gender.Male), ("Brenna", Gender.Female), ("Durgan", Gender.Male), ("Helga", Gender.Female)],
        [Race.NightElf] = [("Faelar", Gender.Male), ("Shalis", Gender.Female), ("Ilthan", Gender.Male), ("Maelis", Gender.Female)],
        [Race.Gnome]    = [("Wizzlo", Gender.Male), ("Nixie", Gender.Female), ("Bimble", Gender.Male), ("Tinka", Gender.Female)],
        [Race.Orc]      = [("Grommak", Gender.Male), ("Zarga", Gender.Female), ("Durok", Gender.Male), ("Shagra", Gender.Female)],
        [Race.Undead]   = [("Mortis", Gender.Male), ("Velessa", Gender.Female), ("Corvane", Gender.Male), ("Lenore", Gender.Female)],
        [Race.Tauren]   = [("Hakoa", Gender.Male), ("Nayeli", Gender.Female), ("Tahoma", Gender.Male), ("Kaya", Gender.Female)],
        [Race.Troll]    = [("Zanjin", Gender.Male), ("Sen'ji", Gender.Female), ("Rokhan", Gender.Male), ("Zalia", Gender.Female)],
    };

    /// <summary>Roles the character itself can take.</summary>
    public static IReadOnlyList<DungeonRole> RolesFor(CharacterClass cls) =>
        Spells.Keys.Where(role => Spells[role].ContainsKey(cls)).ToList();

    /// <summary>The five slots of every group.</summary>
    public static IReadOnlyList<DungeonRole> GroupSlots { get; } =
        [DungeonRole.Tank, DungeonRole.Healer, DungeonRole.Damage, DungeonRole.Damage, DungeonRole.Damage];

    /// <summary>NPC slots that remain next to the own role.</summary>
    public static IReadOnlyList<DungeonRole> NpcSlots(DungeonRole selfRole)
    {
        var slots = GroupSlots.ToList();
        slots.Remove(selfRole);
        return slots;
    }

    /// <summary>Classes the faction can pick for a role.</summary>
    public static IReadOnlyList<CharacterClass> OptionsFor(DungeonRole role, Faction faction) =>
        Spells[role].Keys.Where(c => RacesFor(faction, c).Count > 0).ToList();

    public static NpcSpell SpellOf(DungeonRole role, CharacterClass cls) => Spells[role][cls];

    /// <summary>Damage dealer by default, otherwise the first role of the class.</summary>
    public static DungeonRole DefaultRole(CharacterClass cls) =>
        RolesFor(cls).Contains(DungeonRole.Damage) ? DungeonRole.Damage : RolesFor(cls)[0];

    /// <summary>Default classes for the slots: as varied as possible and without the own class.</summary>
    public static DungeonPartySetup DefaultSetup(int seed, Faction faction, CharacterClass own, DungeonRole selfRole)
    {
        var picked = new List<CharacterClass>();
        foreach (var role in NpcSlots(selfRole))
        {
            var options = OptionsFor(role, faction);
            var fresh = options.Where(c => c != own && !picked.Contains(c)).ToList();
            var pool = fresh.Count > 0 ? fresh : options;
            picked.Add(pool[(seed + picked.Count) % pool.Count]);
        }
        return new DungeonPartySetup(selfRole, picked);
    }

    /// <summary>The setup fits the class of the character, the group layout and the faction.</summary>
    public static bool IsValid(DungeonPartySetup setup, Faction faction, CharacterClass own)
    {
        if (!RolesFor(own).Contains(setup.SelfRole)) return false;
        var slots = NpcSlots(setup.SelfRole);
        if (setup.Companions.Count != slots.Count) return false;
        var chosen = slots.Zip(setup.Companions).ToList();
        return chosen.All(c => OptionsFor(c.First, faction).Contains(c.Second)) && chosen.Distinct().Count() == chosen.Count;
    }

    /// <summary>
    /// All characters to choose from: one fixed person per role and class. Name and race depend only on
    /// seed and faction, so a person stays the same in the selection, in the group and across runs.
    /// </summary>
    public static IReadOnlyList<DungeonPartyMember> Pool(int seed, Faction faction)
    {
        var combos = Spells.Keys.SelectMany(role => OptionsFor(role, faction).Select(cls => (Role: role, Class: cls))).ToList();
        var pool = new List<DungeonPartyMember>();

        // Classes with the fewest races pick first, otherwise the names of their races could already be taken
        foreach (var combo in combos.OrderBy(c => RacesFor(faction, c.Class).Count))
        {
            int offset = seed + pool.Count;
            var races = RacesFor(faction, combo.Class);
            var candidates =
                from r in Enumerable.Range(0, races.Count)
                let race = races[(offset + r) % races.Count]
                from n in Enumerable.Range(0, Names[race].Length)
                let entry = Names[race][(offset + n) % Names[race].Length]
                select (Race: race, entry.Name, entry.Gender);
            var pick = candidates.First(c => pool.All(m => m.Name != c.Name));
            pool.Add(new DungeonPartyMember(pick.Name, pick.Race, pick.Gender, combo.Class, combo.Role, SpellOf(combo.Role, combo.Class)));
        }
        return pool.OrderBy(m => combos.IndexOf((m.Role, m.Class))).ToList();
    }

    public static IReadOnlyList<DungeonPartyMember> Build(int seed, Faction faction, DungeonPartySetup setup)
    {
        var pool = Pool(seed, faction);
        return NpcSlots(setup.SelfRole).Zip(setup.Companions, (role, cls) => pool.First(m => m.Role == role && m.Class == cls)).ToList();
    }

    private static List<Race> RacesFor(Faction faction, CharacterClass cls) =>
        RaceClassInfo.RaceToClasses
            .Where(r => RaceClassInfo.RaceToFaction[r.Key] == faction && r.Value.Contains(cls))
            .Select(r => r.Key)
            .ToList();

    public static string RoleLabel(DungeonRole role) => role switch
    {
        DungeonRole.Tank   => "Tank",
        DungeonRole.Healer => "Healer",
        _                  => "DPS",
    };

    /// <summary>What filling the slot itself means for the character.</summary>
    public static string SelfRoleEffect(DungeonRole role) => role switch
    {
        DungeonRole.Tank   => $"You take the hits: {DungeonRules.SelfTankReduction * 100:0}% less damage taken, but your damage dealers hit a third softer",
        DungeonRole.Healer => $"You keep yourself up: {DungeonRules.HealerHpPerSecond * 100:0.#}% of your health restored per second, but your damage dealers hit a third softer",
        _                  => "You deal damage alongside two other damage dealers",
    };

    public static string RoleIconUrl(DungeonRole role) => $"images/role-{role.ToString().ToLowerInvariant()}.png";
}
