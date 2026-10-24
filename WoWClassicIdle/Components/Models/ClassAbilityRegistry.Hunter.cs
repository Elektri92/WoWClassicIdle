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
            // On-Next-Swing wie Heroic Strike: kein Cast, ersetzt den nächsten Schlag
            ManaCost = 15,
            Damage = 5,
            AddsWeaponDamage = true,
            ReplacesAutoAttack = true,
            LevelRequired = 1,
            Rank = 1,
        },
        new Ability
        {
            Id = "Raptor_Strike_r2",
            Name = "Raptor Strike",
            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/ability_meleedamage.jpg",
            ManaCost = 15,
            Damage = 6,
            AddsWeaponDamage = true,
            ReplacesAutoAttack = true,
            LevelRequired = 10,
            Rank = 2,
        },
        new Ability
        {
            Id = "Hunters_Mark_r1",
            Name = "Hunter's Mark",
            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/ability_hunter_snipershot.jpg",
            Cooldown = 20,
            CastTime = 1.0,
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
            DebuffDotDamage = 11,       // 2 Ticks à 11 = 22 DMG (+10 %, war 10)
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
            ManaCost = 0,       // kostenlos (−100 % Mana)
            Damage = 11,                // +10 % (war 10)
            LevelRequired = 7,
            Rank = 1,
        },
        new Ability
        {
            Id = "Multi_Shot_r1",
            Name = "Multi-Shot",
            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/ability_upgrademoonglaive.jpg",
            Cooldown = 10,
            CastTime = 1.5,
            ManaCost = 15,
            Damage = 24,
            LevelRequired = 14,
            Rank = 1,
        },
        new Ability
        {
            Id = "Disengage_r1",
            Name = "Disengage",
            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/ability_rogue_sprint.jpg",
            Cooldown = 30,
            ManaCost = 0,
            BuffDuration = 6,
            BuffDamageTakenReduction = 0.15,    // −15 % erlittener Schaden
            LevelRequired = 18,
            Rank = 1,
        },
        new Ability
        {
            Id = "Aimed_Shot_r1",
            Name = "Aimed Shot",
            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/ability_hunter_aimedshot.jpg",
            Cooldown = 12,
            CastTime = 3.0,
            ManaCost = 20,
            Damage = 38,
            LevelRequired = 24,
            Rank = 1,
        },
    };
}
