namespace Models;

public static partial class ClassAbilityRegistry
{
    private static IReadOnlyList<Ability> HunterAbilities => new[]
    {
        new Ability
        {
            Id = "Raptor_Strike_r1",
            Name = "Raptor Strike",
            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/ability_meleedamage.jpg",
            Cooldown = 6,
            ManaCost = 15,
            Damage = 5,
            AddsWeaponDamage = true,
            LevelRequired = 1,
            Rank = 1,
        },
        new Ability
        {
            Id = "Raptor_Strike_r2",
            Name = "Raptor Strike",
            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/ability_meleedamage.jpg",
            Cooldown = 6,
            ManaCost = 15,
            Damage = 6,         // +10 % gegenüber Rank 1 (5.5 → 6)
            AddsWeaponDamage = true,
            LevelRequired = 10,
            Rank = 2,
        },
        new Ability
        {
            Id = "Hunters_Mark_r1",
            Name = "Hunter's Mark",
            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/ability_hunter_snipershot.jpg",
            Cooldown = 20,
            ManaCost = 0,
            DebuffDuration = 10,
            DebuffDamageTakenBonus = 0.05, // +5% damage taken
            LevelRequired = 3,
            Rank = 1,
        },
        new Ability
        {
            Id = "Serpent_Sting_r1",
            Name = "Serpent Sting",
            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/ability_poisonsting.jpg",
            Cooldown = 20,
            ManaCost = 15,
            DebuffDuration = 10,        // DoT über 10 s
            DebuffDotDamage = 10,       // 2 Ticks à 10 = 20 DMG
            DebuffDotTickInterval = 5,
            LevelRequired = 4,
            Rank = 1,
        },
        new Ability
        {
            Id = "Arcane_Shot_r1",
            Name = "Arcane Shot",
            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/ability_impalingbolt.jpg",
            Cooldown = 6,
            ManaCost = 10,
            Damage = 10,
            LevelRequired = 7,
            Rank = 1,
        },
    };
}
