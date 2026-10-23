namespace Models;

public static partial class ClassAbilityRegistry
{
    private static IReadOnlyList<Ability> PaladinAbilities => new[]
    {
        new Ability
        {
            Id = "Judgement_r1",
            Name = "Judgement",
            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/spell_holy_righteousfury.jpg",
            Cooldown = 8,
            ManaCost = 20,
            Damage = 19,        // +1 (was 18)
            LevelRequired = 1,
            Rank = 1,
        },
        new Ability
        {
            Id = "Judgement_r2",
            Name = "Judgement",
            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/spell_holy_righteousfury.jpg",
            Cooldown = 8,
            ManaCost = 20,
            Damage = 21,        // +10 % gegenüber Rank 1 (20.9 → 21)
            LevelRequired = 10,
            Rank = 2,
        },
        new Ability
        {
            Id = "Holy_Light_r1",
            Name = "Holy Light",
            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/spell_holy_holybolt.jpg",
            Cooldown = 10,
            ManaCost = 35,
            Heal = 19,          // -5% from 20
            LevelRequired = 3,
            Rank = 1,
        },
        new Ability
        {
            Id = "Holy_Strike_r1",
            Name = "Holy Strike",
            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/spell_holy_excorcism_02.jpg",
            Cooldown = 10,
            ManaCost = 15,
            Damage = 16,                // Waffenschaden + 16
            AddsWeaponDamage = true,
            ReplacesAutoAttack = true,
            LevelRequired = 4,
            Rank = 1,
        },
        new Ability
        {
            Id = "Divine_Shield_r1",
            Name = "Divine Shield",
            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/spell_holy_divineshield.jpg",
            Cooldown = 60,
            ManaCost = 0,
            BuffDuration = 6,           // 6 s Immunität
            BuffGrantsImmunity = true,
            LevelRequired = 7,
            Rank = 1,
        },
    };
}
