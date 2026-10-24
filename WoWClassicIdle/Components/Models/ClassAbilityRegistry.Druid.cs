namespace Models;

public static partial class ClassAbilityRegistry
{
    private static IReadOnlyList<Ability> DruidAbilities => new[]
    {
        new Ability
        {
            Id = "Wrath_r1",
            Name = "Wrath",
            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/spell_nature_abolishmagic.jpg",
            Cooldown = 0,
            CastTime = 2.5,     // 2,5 s Cast-Zeit
            ManaCost = 20,
            Damage = 14,
            LevelRequired = 1,
            Rank = 1,
        },
        new Ability
        {
            Id = "Wrath_r2",
            Name = "Wrath",
            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/spell_nature_abolishmagic.jpg",
            Cooldown = 0,
            CastTime = 2.5,     // 2,5 s Cast-Zeit
            ManaCost = 20,
            Damage = 15,
            LevelRequired = 10,
            Rank = 2,
        },
        new Ability
        {
            Id = "Moonfire_r1",
            Name = "Moonfire",
            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/spell_nature_starfall.jpg",
            Cooldown = 20,
            ManaCost = 5,
            Damage = 6,             // +20 % Buff
            DebuffDuration = 20,
            DebuffDotDamage = 6,    // +20 % Buff
            DebuffDotTickInterval = 5,
            LevelRequired = 3,
            Rank = 1,
        },
        new Ability
        {
            Id = "Rejuvenation_r1",
            Name = "Rejuvenation",
            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/spell_nature_rejuvenation.jpg",
            Cooldown = 10,
            ManaCost = 9,               // nochmals −50 % (war 18)
            BuffDuration = 10,          // HoT über 10 s
            HotTotalHeal = 20,          // 5 Ticks à 4 HP = 20 HP
            HotTickInterval = 2,
            LevelRequired = 5,
            Rank = 1,
        },
        new Ability
        {
            Id = "Thorns_r1",
            Name = "Thorns",
            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/spell_nature_thorns.jpg",
            Cooldown = 0,
            ManaCost = 10,
            BuffDuration = 30,              // Buff läuft 30 s
            BuffRetaliationDamage = 5,      // 5 DMG wenn der Spieler getroffen wird
            LevelRequired = 7,
            Rank = 1,
        },
        new Ability
        {
            Id = "Regrowth_r1",
            Name = "Regrowth",
            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/spell_nature_resistnature.jpg",
            Cooldown = 12,
            CastTime = 2.5,     // 2,5 s Cast-Zeit
            ManaCost = 20,
            Heal = 12,                          // direkter Sofort-Heal
            BuffDuration = 12,                  // HoT über 12 s
            HotTotalHeal = 20,                  // 6 Ticks à ~3 = 20 HP gesamt
            HotTickInterval = 2,
            LevelRequired = 14,
            Rank = 1,
        },
        new Ability
        {
            Id = "Insect_Swarm_r1",
            Name = "Insect Swarm",
            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/spell_nature_insectswarm.jpg",
            Cooldown = 20,
            ManaCost = 10,
            DebuffDuration = 15,                // Naturdot + Tempo-Debuff
            DebuffDotDamage = 6,                // 5 Ticks à 6 = 30 DMG gesamt
            DebuffDotTickInterval = 3,
            DebuffAttackSpeedReduction = 0.10,  // −10 % Angriffstempo
            LevelRequired = 18,
            Rank = 1,
        },
        new Ability
        {
            Id = "Starfire_r1",
            Name = "Starfire",
            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/spell_arcane_starfire.jpg",
            Cooldown = 0,
            CastTime = 3.0,     // 3,0 s Cast-Zeit
            ManaCost = 30,
            Damage = 28,
            LevelRequired = 24,
            Rank = 1,
        },
    };
}
