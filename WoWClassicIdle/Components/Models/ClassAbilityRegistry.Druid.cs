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
            Cooldown = 1.5,
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
            Cooldown = 1.5,
            ManaCost = 20,
            Damage = 15,        // +10 % gegenüber Rank 1 (15.4 → 15)
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
            Damage = 5,
            DebuffDuration = 20,
            DebuffDotDamage = 5,
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
            ManaCost = 35,
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
    };
}
