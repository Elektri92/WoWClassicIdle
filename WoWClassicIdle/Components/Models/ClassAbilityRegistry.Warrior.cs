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
            Cooldown = 0,       // Classic: no CD – replaces the next auto-attack
            ManaCost = 15,      // 15 Rage
            Damage = 10,        // +10 Waffenschaden (-10% nerf)
            AddsWeaponDamage = true,
            ReplacesAutoAttack = true,
            MinResourceToCast = 60, // only from 60 rage, so rage remains for other abilities
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
            Damage = 11,        // +10 % compared to Rank 1
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
            CastTime = 1.0,
            ManaCost = 10,          // 10 Rage
            BuffDuration = 30,      // 30 s
            BuffDamageBonus = 0.05, // +5 % damage dealt
            LevelRequired = 2,
            Rank = 1,
        },
        new Ability
        {
            Id = "Battle_Shout_r2",
            Name = "Battle Shout",
            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/ability_warrior_battleshout.jpg",
            Cooldown = 0,
            CastTime = 1.0,
            ManaCost = 10,
            BuffDuration = 30,
            BuffDamageBonus = 0.07, // ×1.3 compared to Rank 1
            LevelRequired = 12,
            Rank = 2,
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
            Id = "Thunderclap_r2",
            Name = "Thunder Clap",
            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/spell_nature_thunderclap.jpg",
            Cooldown = 30,
            ManaCost = 20,
            DebuffDuration = 10,
            DebuffAttackSpeedReduction = 0.26,  // ×1.3 compared to Rank 1
            LevelRequired = 14,
            Rank = 2,
        },
        new Ability
        {
            Id = "Execute_r1",
            Name = "Execute",
            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/inv_sword_48.jpg",
            Cooldown = 120,
            ManaCost = 20,                          // 20 Rage
            Damage = 50,
            RequiresEnemyBelowHpPct = 0.20,         // only below 20 % enemy HP
            LevelRequired = 8,
            Rank = 1,
        },
        new Ability
        {
            Id = "Execute_r2",
            Name = "Execute",
            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/inv_sword_48.jpg",
            Cooldown = 120,
            ManaCost = 20,
            Damage = 75,                            // ×1.5 compared to Rank 1
            RequiresEnemyBelowHpPct = 0.20,
            LevelRequired = 18,
            Rank = 2,
        },
        new Ability
        {
            Id = "Rend_r1",
            Name = "Rend",
            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/ability_gouge.jpg",
            Cooldown = 30,
            ManaCost = 10,                      // 10 Rage
            DebuffDuration = 12,                // Bleed DoT over 12 s
            DebuffDotDamage = 18,               // 4 ticks of 18 = 72 damage total
            DebuffDotTickInterval = 3,
            LevelRequired = 14,
            Rank = 1,
        },
        new Ability
        {
            Id = "Battle_Stance",
            Form = DruidForm.BattleStance,   // Stance: prerequisite for stance abilities (Overpower, Flurry …)
            IsPassive = true,          // Aura – applied while equipped
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
            Form = DruidForm.BerserkerStance,   // Stance: prerequisite for stance abilities (Overpower, Flurry …)
            IsPassive = true,          // Aura – applied while equipped
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
            Form = DruidForm.DefensiveStance,   // Stance: prerequisite for stance abilities (Overpower, Flurry …)
            IsPassive = true,          // Aura – applied while equipped
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

        // ─── Specialization abilities (talent row 3, from Lv 20) ───
        new Ability
        {
            Id = "Overpower_r1",
            Name = "Overpower",
            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/ability_meleedamage.jpg",
            Cooldown = 12,
            ManaCost = 10,                      // 10 Rage
            Damage = 4,          // was regular Lv 18 (+5) – as an Arms talent with +8 / CD 8 it gave +26 % XP/h
            AddsWeaponDamage = true,
            BonusCritChance = 0.25,             // Improved Overpower
            RequiredForm = DruidForm.BattleStance,
            TalentUnlockOnly = true,
            SpecHint = 0,
            LevelRequired = 20,
            Rank = 1,
        },
        new Ability
        {
            Id = "Intimidating_Shout_r1",
            Name = "Intimidating Shout",
            IconUrl = "https://wow.zamimg.com/images/wow/icons/large/ability_golemstormbolt.jpg",
            Cooldown = 40,
            ManaCost = 10,                      // 10 Rage
            DebuffDuration = 6,                 // Enemy flees: no attacks for 6 s
            DebuffIsStun = true,
            RequiredForm = DruidForm.DefensiveStance,
            TalentUnlockOnly = true,
            SpecHint = 2,
            LevelRequired = 20,
            Rank = 1,
        },
    };
}
