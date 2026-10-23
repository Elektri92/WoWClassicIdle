namespace Models;

public static partial class ClassAbilityRegistry
{
    private static IReadOnlyList<Ability> WarriorAbilities => new[]
    {
        new Ability
        {
            Id = "Heroic_Strike_r1",
            Name = "Heroic Strike",
            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/ability_rogue_ambush.jpg",
            Cooldown = 0,       // Classic: kein CD – ersetzt die nächste Auto-Attack
            ManaCost = 15,      // 15 Rage
            Damage = 10,        // +10 Waffenschaden (-10% nerf)
            AddsWeaponDamage = true,
            ReplacesAutoAttack = true,
            MinResourceToCast = 60, // erst ab 60 Rage, damit Rage für andere Fähigkeiten bleibt
            LevelRequired = 1,
            Rank = 1,
        },
        new Ability
        {
            Id = "Heroic_Strike_r2",
            Name = "Heroic Strike",
            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/ability_rogue_ambush.jpg",
            Cooldown = 0,
            ManaCost = 15,
            Damage = 11,        // +10 % gegenüber Rank 1
            AddsWeaponDamage = true,
            ReplacesAutoAttack = true,
            MinResourceToCast = 60,
            LevelRequired = 10,
            Rank = 2,
        },
        new Ability
        {
            Id = "Battle_Shout_r1",
            Name = "Battle Shout",
            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/ability_warrior_battleshout.jpg",
            Cooldown = 0,
            ManaCost = 10,          // 10 Rage
            BuffDuration = 30,      // 30 s
            BuffDamageBonus = 0.05, // +5 % verursachter Schaden
            LevelRequired = 2,
            Rank = 1,
        },
        new Ability
        {
            Id = "Thunderclap_r1",
            Name = "Thunder Clap",
            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/spell_nature_thunderclap.jpg",
            Cooldown = 30,
            ManaCost = 20,                      // 20 Rage
            DebuffDuration = 10,
            DebuffAttackSpeedReduction = 0.20,  // −20 % Angriffstempo
            LevelRequired = 4,
            Rank = 1,
        },
        new Ability
        {
            Id = "Execute_r1",
            Name = "Execute",
            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/inv_sword_48.jpg",
            Cooldown = 120,
            ManaCost = 20,                          // 20 Rage
            Damage = 50,
            RequiresEnemyBelowHpPct = 0.20,         // nur unter 20 % Gegner-HP
            LevelRequired = 8,
            Rank = 1,
        },
        new Ability
        {
            Id = "Battle_Stance",
            Name = "Battle Stance",
            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/ability_warrior_offensivestance.jpg",
            Cooldown = 0,
            ManaCost = 0,
            BuffDuration = 9999,
            BuffRageGenBonus = 0.10,
            ExclusiveGroup = "warrior_stance",
            TalentUnlockOnly = true,
            LevelRequired = 1,
            Rank = 1,
        },
        new Ability
        {
            Id = "Berserker_Stance",
            Name = "Berserker Stance",
            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/ability_racial_avatar.jpg",
            Cooldown = 0,
            ManaCost = 0,
            BuffDuration = 9999,
            BuffDamageBonus = 0.05,
            BuffCritChanceBonus = 0.03,
            ExclusiveGroup = "warrior_stance",
            TalentUnlockOnly = true,
            LevelRequired = 1,
            Rank = 1,
        },
        new Ability
        {
            Id = "Defensive_Stance",
            Name = "Defensive Stance",
            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/ability_warrior_defensivestance.jpg",
            Cooldown = 0,
            ManaCost = 0,
            BuffDuration = 9999,
            BuffDamageTakenReduction = 0.10,
            ExclusiveGroup = "warrior_stance",
            TalentUnlockOnly = true,
            LevelRequired = 1,
            Rank = 1,
        },
    };
}
