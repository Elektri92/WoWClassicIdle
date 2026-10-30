// Models/ClassAbilityRegistry.LevelSpec.cs
namespace Models;

// Spec-themed abilities learned automatically at Lv 11–15 (no talent needed). Every spec can use all three;
// the matching row 3 talents live in TalentRegistry.LevelAbilities.cs.
public static partial class ClassAbilityRegistry
{
    private const string IconBase = "https://wow.zamimg.com/images/wow/icons/large/";

    private static IEnumerable<Ability> LevelSpecAbilities(CharacterClass cls) => cls switch
    {
        CharacterClass.Warrior =>
        [
            new Ability
            {
                Id = "Slam_r1", Name = "Slam", IconUrl = IconBase + "ability_warrior_decisivestrike.jpg",
                CastTime = 1.0, Cooldown = 8, ManaCost = 15, Damage = 8, AddsWeaponDamage = true,
                SpecHint = 0, LevelRequired = 11, Rank = 1,
            },
            new Ability
            {
                Id = "Death_Wish_r1", Name = "Death Wish", IconUrl = IconBase + "spell_shadow_deathpact.jpg",
                Cooldown = 60, ManaCost = 10, BuffDuration = 15, BuffDamageBonus = 0.15,
                SpecHint = 1, LevelRequired = 13, Rank = 1,
            },
            new Ability
            {
                Id = "Shield_Block_r1", Name = "Shield Block", IconUrl = IconBase + "ability_defend.jpg",
                Cooldown = 20, ManaCost = 10, BuffDuration = 10, BuffHitCharges = 3, BuffDamageTakenReduction = 0.40,
                SpecHint = 2, LevelRequired = 15, Rank = 1,
            },
        ],
        CharacterClass.Paladin =>
        [
            new Ability
            {
                Id = "Flash_of_Light_r1", Name = "Flash of Light", IconUrl = IconBase + "spell_holy_flashheal.jpg",
                CastTime = 1.5, Cooldown = 8, ManaCost = 20, Heal = 22,
                SpecHint = 0, LevelRequired = 11, Rank = 1,
            },
            new Ability
            {
                Id = "Blessing_of_Sanctuary_r1", Name = "Blessing of Sanctuary", IconUrl = IconBase + "spell_holy_greaterblessingofsanctuary.jpg",
                ManaCost = 15, BuffDuration = 60, BuffDamageTakenReduction = 0.05, BuffRetaliationDamage = 3,
                SpecHint = 1, LevelRequired = 12, Rank = 1,
            },
            new Ability
            {
                Id = "Crusader_Strike_r1", Name = "Crusader Strike", IconUrl = IconBase + "spell_holy_crusaderstrike.jpg",
                Cooldown = 8, ManaCost = 20, Damage = 6, AddsWeaponDamage = true,
                SpecHint = 2, LevelRequired = 13, Rank = 1,
            },
        ],
        CharacterClass.Hunter =>
        [
            new Ability
            {
                Id = "Rapid_Fire_r1", Name = "Rapid Fire", IconUrl = IconBase + "ability_hunter_runningshot.jpg",
                Cooldown = 40, ManaCost = 15, BuffDuration = 15, BuffAttackSpeedBonus = 0.30,
                SpecHint = 1, LevelRequired = 11, Rank = 1,
            },
            new Ability
            {
                Id = "Wyvern_Sting_r1", Name = "Wyvern Sting", IconUrl = IconBase + "inv_spear_02.jpg",
                Cooldown = 30, ManaCost = 20, DebuffDuration = 6, DebuffIsStun = true, DebuffDotDamage = 8, DebuffDotTickInterval = 2,
                SpecHint = 2, LevelRequired = 12, Rank = 1,
            },
            new Ability
            {
                // The small personal bonus keeps it useful for specs without a pet
                Id = "Bestial_Wrath_r1", Name = "Bestial Wrath", IconUrl = IconBase + "ability_druid_ferociousbite.jpg",
                Cooldown = 30, ManaCost = 20, BuffDuration = 15, BuffPetDamageBonus = 0.50, BuffDamageBonus = 0.10,
                SpecHint = 0, LevelRequired = 15, Rank = 1,
            },
        ],
        CharacterClass.Rogue =>
        [
            new Ability
            {
                Id = "Rupture_r1", Name = "Rupture", IconUrl = IconBase + "ability_rogue_rupture.jpg",
                Cooldown = 12, ManaCost = 25, DebuffDuration = 12, DebuffDotDamage = 5, DebuffDotTickInterval = 2,
                SpecHint = 0, LevelRequired = 11, Rank = 1,
            },
            new Ability
            {
                Id = "Riposte_r1", Name = "Riposte", IconUrl = IconBase + "ability_warrior_challange.jpg",
                Cooldown = 10, ManaCost = 25, Damage = 6, AddsWeaponDamage = true, DebuffDuration = 6, DebuffAttackSpeedReduction = 0.20,
                SpecHint = 1, LevelRequired = 13, Rank = 1,
            },
            new Ability
            {
                Id = "Hemorrhage_r1", Name = "Hemorrhage", IconUrl = IconBase + "spell_shadow_lifedrain.jpg",
                Cooldown = 15, ManaCost = 30, Damage = 3, AddsWeaponDamage = true, DebuffDuration = 15, DebuffDamageTakenBonus = 0.05,
                SpecHint = 2, LevelRequired = 15, Rank = 1,
            },
        ],
        CharacterClass.Priest =>
        [
            new Ability
            {
                Id = "Power_Infusion_r1", Name = "Power Infusion", IconUrl = IconBase + "spell_holy_powerinfusion.jpg",
                Cooldown = 60, ManaCost = 25, BuffDuration = 15, BuffDamageBonus = 0.20,
                SpecHint = 0, LevelRequired = 11, Rank = 1,
            },
            new Ability
            {
                Id = "Devouring_Plague_r1", Name = "Devouring Plague", IconUrl = IconBase + "spell_shadow_devouringplague.jpg",
                Cooldown = 30, ManaCost = 25, DebuffDuration = 24, DebuffDotDamage = 7, DebuffDotTickInterval = 3, DebuffDotLifesteal = 0.5,
                SpecHint = 2, LevelRequired = 13, Rank = 1,
            },
            new Ability
            {
                Id = "Holy_Fire_r1", Name = "Holy Fire", IconUrl = IconBase + "spell_holy_searinglight.jpg",
                CastTime = 3.0, Cooldown = 12, ManaCost = 30, Damage = 28, DebuffDuration = 8, DebuffDotDamage = 4, DebuffDotTickInterval = 2,
                SpecHint = 1, LevelRequired = 15, Rank = 1,
            },
        ],
        CharacterClass.Shaman =>
        [
            new Ability
            {
                Id = "Earth_Shock_r1", Name = "Earth Shock", IconUrl = IconBase + "spell_nature_earthshock.jpg",
                Cooldown = 8, ManaCost = 15, Damage = 17,
                SpecHint = 0, LevelRequired = 11, Rank = 1,
            },
            new Ability
            {
                Id = "Healing_Stream_Totem_r1", Name = "Healing Stream Totem", IconUrl = IconBase + "inv_spear_04.jpg",
                Cooldown = 30, ManaCost = 15, BuffDuration = 12, HotTotalHeal = 36, HotTickInterval = 3,
                SpecHint = 2, LevelRequired = 12, Rank = 1,
            },
            new Ability
            {
                Id = "Lava_Lash_r1", Name = "Lava Lash", IconUrl = IconBase + "ability_shaman_lavalash.jpg",
                Cooldown = 8, ManaCost = 20, Damage = 6, AddsWeaponDamage = true,
                SpecHint = 1, LevelRequired = 13, Rank = 1,
            },
        ],
        CharacterClass.Mage =>
        [
            new Ability
            {
                Id = "Arcane_Explosion_r1", Name = "Arcane Explosion", IconUrl = IconBase + "spell_nature_wispsplode.jpg",
                Cooldown = 6, ManaCost = 12, Damage = 14,
                SpecHint = 0, LevelRequired = 11, Rank = 1,
            },
            new Ability
            {
                Id = "Cone_of_Cold_r1", Name = "Cone of Cold", IconUrl = IconBase + "spell_frost_glacier.jpg",
                Cooldown = 10, ManaCost = 15, Damage = 16, DebuffDuration = 8, DebuffAttackSpeedReduction = 0.20,
                SpecHint = 2, LevelRequired = 13, Rank = 1,
            },
            new Ability
            {
                Id = "Pyroblast_r1", Name = "Pyroblast", IconUrl = IconBase + "spell_fire_fireball02.jpg",
                CastTime = 4.0, Cooldown = 20, ManaCost = 40, Damage = 38, DebuffDuration = 12, DebuffDotDamage = 4, DebuffDotTickInterval = 3,
                SpecHint = 1, LevelRequired = 15, Rank = 1,
            },
        ],
        CharacterClass.Warlock =>
        [
            new Ability
            {
                Id = "Drain_Soul_r1", Name = "Drain Soul", IconUrl = IconBase + "spell_shadow_haunting.jpg",
                CastTime = 2.0, Cooldown = 10, ManaCost = 15, Damage = 28, RequiresEnemyBelowHpPct = 0.25,
                SpecHint = 0, LevelRequired = 11, Rank = 1,
            },
            new Ability
            {
                Id = "Demon_Armor_r1", Name = "Demon Armor", IconUrl = IconBase + "spell_shadow_ragingscream.jpg",
                ManaCost = 15, BuffDuration = 60, BuffDamageTakenReduction = 0.08,
                SpecHint = 1, LevelRequired = 13, Rank = 1,
            },
            new Ability
            {
                Id = "Searing_Pain_r1", Name = "Searing Pain", IconUrl = IconBase + "spell_fire_soulburn.jpg",
                Cooldown = 8, ManaCost = 20, Damage = 20,
                SpecHint = 2, LevelRequired = 15, Rank = 1,
            },
        ],
        CharacterClass.Druid =>
        [
            new Ability
            {
                Id = "Entangling_Roots_r1", Name = "Entangling Roots", IconUrl = IconBase + "spell_nature_stranglevines.jpg",
                CastTime = 1.5, Cooldown = 30, ManaCost = 15, DebuffDuration = 6, DebuffIsStun = true, DebuffDotDamage = 4, DebuffDotTickInterval = 2,
                SpecHint = 0, LevelRequired = 11, Rank = 1,
            },
            new Ability
            {
                Id = "Swiftmend_r1", Name = "Swiftmend", IconUrl = IconBase + "inv_relics_idolofrejuvenation.jpg",
                Cooldown = 30, ManaCost = 25, Heal = 40,
                SpecHint = 2, LevelRequired = 12, Rank = 1,
            },
            new Ability
            {
                Id = "Swipe_r1", Name = "Swipe", IconUrl = IconBase + "ability_druid_swipe.jpg",
                Cooldown = 8, ManaCost = 25, Damage = 6, AddsWeaponDamage = true,
                FormCosts = new Dictionary<DruidForm, int> { [DruidForm.Bear] = 20, [DruidForm.Cat] = 25 },
                SpecHint = 1, LevelRequired = 13, Rank = 1,
            },
        ],
        _ => [],
    };
}
