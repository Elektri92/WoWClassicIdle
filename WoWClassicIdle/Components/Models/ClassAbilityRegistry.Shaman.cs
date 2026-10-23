namespace Models;

public static partial class ClassAbilityRegistry
{
    private static IReadOnlyList<Ability> ShamanAbilities => new[]
    {
        new Ability
        {
            Id = "Lightning_bolt_r1",
            Name = "Lightning Bolt",
            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/spell_nature_lightning.jpg",
            Cooldown = 1.5,
            ManaCost = 15,
            Damage = 10,        // -1 (was 11)
            LevelRequired = 1,
            Rank = 1,
        },
        new Ability
        {
            Id = "Lightning_Bolt_r2",
            Name = "Lightning Bolt",
            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/spell_nature_lightning.jpg",
            Cooldown = 1.5,
            ManaCost = 15,
            Damage = 11,        // +10 % gegenüber Rank 1
            LevelRequired = 10,
            Rank = 2,
        },
        new Ability
        {
            Id = "Rockbiter_Weapon_r1",
            Name = "Rockbiter Weapon",
            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/spell_nature_rockbiter.jpg",
            Cooldown = 0,
            ManaCost = 10,
            BuffDuration = 60,
            BuffWeaponDamageBonus = 0.08, // +8% auto-attack damage
            LevelRequired = 2,
            Rank = 1,
        },
        new Ability
        {
            Id = "Earthbind_Totem_r1",
            Name = "Earthbind Totem",
            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/spell_nature_earthbindtotem.jpg",
            Cooldown = 30,
            ManaCost = 5,
            DebuffDuration = 10,
            DebuffAttackSpeedReduction = 0.20,  // −20 % Angriffstempo
            LevelRequired = 4,
            Rank = 1,
        },
        new Ability
        {
            Id = "Lightning_Shield_r1",
            Name = "Lightning Shield",
            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/spell_nature_lightningshield.jpg",
            Cooldown = 0,
            ManaCost = 10,
            BuffDuration = 30,              // Buff läuft 30 s
            BuffRetaliationDamage = 4,      // 4 DMG wenn der Spieler getroffen wird
            LevelRequired = 7,
            Rank = 1,
        },
    };
}
