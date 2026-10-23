namespace Models;

public static partial class ClassAbilityRegistry
{
    private static IReadOnlyList<Ability> PriestAbilities => new[]
    {
        new Ability
        {
            Id = "Smite_r1",
            Name = "Smite",
            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/spell_holy_holysmite.jpg",
            Cooldown = 1.5,
            ManaCost = 18,      // -10% from 20
            Damage = 13,        // -3 (was 16)
            LevelRequired = 1,
            Rank = 1,
        },
        new Ability
        {
            Id = "Smite_r2",
            Name = "Smite",
            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/spell_holy_holysmite.jpg",
            Cooldown = 1.5,
            ManaCost = 18,
            Damage = 14,        // +10 % gegenüber Rank 1
            LevelRequired = 10,
            Rank = 2,
        },
        new Ability
        {
            Id = "Renew_r1",
            Name = "Renew",
            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/spell_holy_renew.jpg",
            Cooldown = 60,
            ManaCost = 30,
            BuffDuration = 20,      // HoT über 20 s
            HotTotalHeal = 40,      // 40 Heilung gesamt
            HotTickInterval = 4,    // 5 Ticks à 8
            LevelRequired = 3,
            Rank = 1,
        },
        new Ability
        {
            Id = "Shadow_Word_Pain_r1",
            Name = "Shadow Word: Pain",
            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/spell_shadow_shadowwordpain.jpg",
            Cooldown = 60,
            ManaCost = 20,
            DebuffDuration = 30,        // DoT über 30 s
            DebuffDotDamage = 12,       // verdoppelt (war 6), 6 Ticks à 12 = 72 DMG
            DebuffDotTickInterval = 5,
            LevelRequired = 5,
            Rank = 1,
        },
        new Ability
        {
            Id = "Drain_Mana_r1",
            Name = "Drain Mana",
            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/spell_shadow_siphonmana.jpg",
            Cooldown = 30,
            ManaCost = 0,
            BuffDuration = 20,              // MoT über 20 s
            MotManaRestorePct = 0.08,       // 8 % maximales Mana
            MotTickInterval = 5.0,          // 4 Ticks à 2 %
            LevelRequired = 7,
            Rank = 1,
        },
    };
}
