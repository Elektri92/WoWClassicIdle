namespace Models;

public static partial class ClassAbilityRegistry
{
    private static IReadOnlyList<Ability> WarlockAbilities => new[]
    {
        new Ability
        {
            Id = "Shdow_bolt_r1",
            Name = "Shadow Bolt",
            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/spell_shadow_shadowbolt.jpg",
            Cooldown = 1.7,
            ManaCost = 25,
            Damage = 15,
            LevelRequired = 1,
            Rank = 1,
        },
        new Ability
        {
            Id = "Shadow_Bolt_r2",
            Name = "Shadow Bolt",
            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/spell_shadow_shadowbolt.jpg",
            Cooldown = 1.7,
            ManaCost = 25,
            Damage = 17,        // +10 % gegenüber Rank 1 (16.5 → 17)
            LevelRequired = 10,
            Rank = 2,
        },
        new Ability
        {
            Id = "Curse_of_Agony_r1",
            Name = "Curse of Agony",
            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/spell_shadow_curseofsargeras.jpg",
            Cooldown = 0,
            ManaCost = 10,
            DebuffDuration = 15,
            DebuffDotDamage = 8,        // verdoppelt (war 4)
            DebuffDotTickInterval = 3,
            LevelRequired = 3,
            Rank = 1,
        },
        new Ability
        {
            Id = "Drain_Life_r1",
            Name = "Drain Life",
            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/spell_shadow_lifedrain02.jpg",
            Cooldown = 60,
            ManaCost = 0,
            DebuffDuration = 12,        // DoT über 12 s
            DebuffDotDamage = 5,        // 4 Ticks à 5 = 20 DMG
            DebuffDotTickInterval = 3,
            DebuffDotLifesteal = 1.0,   // 100 % des Schadens heilt den Spieler
            LevelRequired = 5,
            Rank = 1,
        },
        new Ability
        {
            Id = "Drain_Life_r2",
            Name = "Drain Life",
            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/spell_shadow_lifedrain02.jpg",
            Cooldown = 60,
            ManaCost = 0,
            DebuffDuration = 12,
            DebuffDotDamage = 10,       // doppelter Schaden gegenüber Rank 1
            DebuffDotTickInterval = 3,
            DebuffDotLifesteal = 1.0,
            LevelRequired = 11,
            Rank = 2,
        },
        new Ability
        {
            Id = "Life_Tap_r1",
            Name = "Life Tap",
            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/spell_shadow_burningspirit.jpg",
            Cooldown = 30,
            ManaCost = 0,
            LifeTapHpCost = 10,         // kostet 10 HP
            LifeTapManaGain = 15,        // gibt 15 Mana
            LevelRequired = 7,
            Rank = 1,
        },
    };
}
