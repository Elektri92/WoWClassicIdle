// Models/TalentRegistry.LevelAbilities.cs
namespace Models;

// Talent row 3 (Row = 2, from Lv 20, 10 points in the tree): one point each to improve the spec abilities
// from ClassAbilityRegistry.LevelSpec.cs. Modifiers only change values read from the effective copy
// (damage, DoT, durations, buff/debuff strength, healing) – ExecuteAbility branches and cooldowns use the raw ability.
public static partial class TalentRegistry
{
    private static IEnumerable<TalentDefinition> BuildLevelAbilityTalents(CharacterClass cls) => cls switch
    {
        CharacterClass.Warrior =>
        [
            Improve(cls, 0, "warrior_arms_improved_slam", "Improved Slam", "Slam",
                "Increases the damage of Slam by 30%.", a => a.WithDamageBonus(0.30)),
            Improve(cls, 1, "warrior_fury_unbridled_wrath", "Unbridled Wrath", "Death Wish",
                "Death Wish also increases your critical strike chance by 10%.", a => a with { BuffCritChanceBonus = a.BuffCritChanceBonus + 0.10 }),
            Improve(cls, 2, "warrior_prot_improved_shield_block", "Improved Shield Block", "Shield Block",
                "Shield Block lasts for 2 additional attacks.", a => a with { BuffHitCharges = a.BuffHitCharges + 2 }),
        ],
        CharacterClass.Paladin =>
        [
            Improve(cls, 0, "paladin_holy_illumination", "Illumination", "Flash of Light",
                "Increases the healing of Flash of Light by 30%.", a => a with { Heal = Percent(a.Heal, 0.30) }),
            Improve(cls, 1, "paladin_prot_improved_sanctuary", "Improved Sanctuary", "Blessing of Sanctuary",
                "Blessing of Sanctuary reduces damage taken by an additional 3% and deals 2 more damage to attackers.",
                a => a with { BuffDamageTakenReduction = a.BuffDamageTakenReduction + 0.03, BuffRetaliationDamage = a.BuffRetaliationDamage + 2 }),
            Improve(cls, 2, "paladin_ret_vengeance", "Vengeance", "Crusader Strike",
                "Increases the damage of Crusader Strike by 30%.", a => a.WithDamageBonus(0.30)),
        ],
        CharacterClass.Hunter =>
        [
            Improve(cls, 0, "hunter_bm_ferocity", "Ferocity", "Bestial Wrath",
                "Bestial Wrath increases your pet's damage by an additional 25% and your own by an additional 5%.",
                a => a with { BuffPetDamageBonus = a.BuffPetDamageBonus + 0.25, BuffDamageBonus = a.BuffDamageBonus + 0.05 }),
            Improve(cls, 1, "hunter_mm_rapid_killing", "Rapid Killing", "Rapid Fire",
                "Rapid Fire increases your attack speed by an additional 10%.", a => a with { BuffAttackSpeedBonus = a.BuffAttackSpeedBonus + 0.10 }),
            Improve(cls, 2, "hunter_surv_improved_wyvern_sting", "Improved Wyvern Sting", "Wyvern Sting",
                "Increases the duration of Wyvern Sting by 2 sec.", a => a with { DebuffDuration = a.DebuffDuration + 2 }),
        ],
        CharacterClass.Rogue =>
        [
            Improve(cls, 0, "rogue_assa_serrated_blades", "Serrated Blades", "Rupture",
                "Increases the damage of Rupture by 40%.", a => a with { DebuffDotDamage = Percent(a.DebuffDotDamage, 0.40) }),
            Improve(cls, 1, "rogue_combat_improved_riposte", "Improved Riposte", "Riposte",
                "Increases the damage of Riposte by 30% and its attack speed slow by 10%.",
                a => a.WithDamageBonus(0.30) with { DebuffAttackSpeedReduction = a.DebuffAttackSpeedReduction + 0.10 }),
            Improve(cls, 2, "rogue_sub_improved_hemorrhage", "Improved Hemorrhage", "Hemorrhage",
                "Hemorrhage makes the target take an additional 5% damage.", a => a with { DebuffDamageTakenBonus = a.DebuffDamageTakenBonus + 0.05 }),
        ],
        CharacterClass.Priest =>
        [
            Improve(cls, 0, "priest_disc_improved_power_infusion", "Improved Power Infusion", "Power Infusion",
                "Increases the duration of Power Infusion by 10 sec.", a => a with { BuffDuration = a.BuffDuration + 10 }),
            Improve(cls, 1, "priest_holy_divine_fury", "Divine Fury", "Holy Fire",
                "Increases the direct and periodic damage of Holy Fire by 30%.", a => DirectAndDot(a, 0.30)),
            Improve(cls, 2, "priest_shadow_vampiric_embrace", "Vampiric Embrace", "Devouring Plague",
                "Devouring Plague heals you for 100% of the damage dealt instead of 50%.", a => a with { DebuffDotLifesteal = a.DebuffDotLifesteal + 0.50 }),
        ],
        CharacterClass.Shaman =>
        [
            Improve(cls, 0, "shaman_ele_concussion", "Concussion", "Earth Shock",
                "Increases the damage of Earth Shock by 30%.", a => a.WithDamageBonus(0.30)),
            Improve(cls, 1, "shaman_enh_elemental_weapons", "Elemental Weapons", "Lava Lash",
                "Increases the damage of Lava Lash by 30%.", a => a.WithDamageBonus(0.30)),
            Improve(cls, 2, "shaman_resto_restorative_totems", "Restorative Totems", "Healing Stream Totem",
                "Increases the healing of Healing Stream Totem by 25%.", a => a with { HotTotalHeal = Percent(a.HotTotalHeal, 0.25) }),
        ],
        CharacterClass.Mage =>
        [
            Improve(cls, 0, "mage_arcane_improved_arcane_explosion", "Improved Arcane Explosion", "Arcane Explosion",
                "Increases the damage of Arcane Explosion by 30%.", a => a.WithDamageBonus(0.30)),
            Improve(cls, 1, "mage_fire_improved_pyroblast", "Improved Pyroblast", "Pyroblast",
                "Increases the direct and periodic damage of Pyroblast by 30%.", a => DirectAndDot(a, 0.30)),
            Improve(cls, 2, "mage_frost_improved_cone_of_cold", "Improved Cone of Cold", "Cone of Cold",
                "Increases the damage of Cone of Cold by 30% and its attack speed slow by 10%.",
                a => a.WithDamageBonus(0.30) with { DebuffAttackSpeedReduction = a.DebuffAttackSpeedReduction + 0.10 }),
        ],
        CharacterClass.Warlock =>
        [
            Improve(cls, 0, "warlock_affli_improved_drain_soul", "Improved Drain Soul", "Drain Soul",
                "Increases the damage of Drain Soul by 40%.", a => a.WithDamageBonus(0.40)),
            Improve(cls, 1, "warlock_demo_demonic_aegis", "Demonic Aegis", "Demon Armor",
                "Demon Armor reduces damage taken by an additional 7%.", a => a with { BuffDamageTakenReduction = a.BuffDamageTakenReduction + 0.07 }),
            Improve(cls, 2, "warlock_destro_emberstorm", "Emberstorm", "Searing Pain",
                "Increases the damage of Searing Pain by 30%.", a => a.WithDamageBonus(0.30)),
        ],
        CharacterClass.Druid =>
        [
            Improve(cls, 0, "druid_bal_natures_grasp", "Nature's Grasp", "Entangling Roots",
                "Increases the duration of Entangling Roots by 2 sec.", a => a with { DebuffDuration = a.DebuffDuration + 2 }),
            // Column 1: Faerie Fire (Bear/Cat) occupy columns 0 and 2 of the Feral tree
            Improve(cls, 1, "druid_feral_savage_fury", "Savage Fury", "Swipe",
                "Increases the damage of Swipe by 30%.", a => a.WithDamageBonus(0.30), column: 1),
            Improve(cls, 2, "druid_resto_improved_swiftmend", "Improved Swiftmend", "Swiftmend",
                "Increases the healing of Swiftmend by 25%.", a => a with { Heal = Percent(a.Heal, 0.25) }),
        ],
        _ => [],
    };

    private static int Percent(int value, double bonus) => (int)Math.Round(value * (1.0 + bonus));

    private static Ability DirectAndDot(Ability ability, double bonus) =>
        ability.WithDamageBonus(bonus) with { DebuffDotDamage = Percent(ability.DebuffDotDamage, bonus) };

    /// <summary>One point in row 3 that improves the ability <paramref name="abilityName"/>; icon = ability icon.</summary>
    private static TalentDefinition Improve(
        CharacterClass cls, int tree, string id, string name, string abilityName, string description,
        Func<Ability, Ability> modifier, int column = 0) => new()
    {
        Id                  = id,
        Name                = name,
        Description         = description,
        Class               = cls,
        TreeIndex           = tree,
        Row                 = 2,
        Column              = column,
        MaxRanks            = 1,
        LevelRequired       = SpecAbilityLevel,
        EffectType          = TalentEffectType.ModifyAbility,
        EffectPerRank       = 1.0,
        ModifiedAbilityName = abilityName,
        AbilityModifier     = modifier,
        IconUrl             = (ClassAbilityRegistry.FindByName(cls, abilityName)?.IconUrl ?? "").Replace("/large/", "/medium/"),
    };
}
