// Models/TalentRegistry.Improved.cs
namespace Models;

// Talent row 4 (Row = 3, 15 points in the tree): "Improved …" talents that strengthen the 1/1 ability talent of row 2.
// Each sits in the same column as the talent it builds on, so the requirement arrow runs straight down.
// Where a row 3 talent already depends on the row 2 talent (stances, Shadowform, Druid forms) the chain continues through it.
public static partial class TalentRegistry
{
    private const int ImprovedRow      = 3;
    private const int ImprovedMaxRanks = 2;

    private static IEnumerable<TalentDefinition> BuildImprovedTalents(CharacterClass cls) => cls switch
    {
        CharacterClass.Warrior =>
        [
            ImproveAbility(cls, 0, "warrior_arms_improved_battle_stance", "Improved Battle Stance", "Battle Stance",
                "warrior_arms_overpower", "warrior_arms_battle_stance",
                r => $"Battle Stance generates {Pct(0.05 * r)} more rage from auto-attacks.",
                (a, r) => a with { BuffRageGenBonus = a.BuffRageGenBonus + 0.05 * r }),
            ImproveAbility(cls, 1, "warrior_fury_improved_berserker_stance", "Improved Berserker Stance", "Berserker Stance",
                "warrior_fury_flurry", "warrior_fury_berserker_stance",
                r => $"Berserker Stance also increases your critical strike chance by {Pct(0.015 * r)}.",
                (a, r) => a with { BuffCritChanceBonus = a.BuffCritChanceBonus + 0.015 * r }),
            ImproveAbility(cls, 2, "warrior_prot_improved_defensive_stance", "Improved Defensive Stance", "Defensive Stance",
                "warrior_prot_intimidating_shout", "warrior_def_defensive_stance",
                r => $"Defensive Stance reduces damage taken by an additional {Pct(0.03 * r)}.",
                (a, r) => a with { BuffDamageTakenReduction = a.BuffDamageTakenReduction + 0.03 * r }),
        ],
        CharacterClass.Mage =>
        [
            ImproveAbility(cls, 0, "mage_arcane_improved_evocation", "Improved Evocation", "Evocation",
                "mage_arcane_evocation", "mage_arcane_evocation",
                r => $"Evocation increases your damage by an additional {Pct(0.05 * r)}.",
                (a, r) => a with { BuffDamageBonus = a.BuffDamageBonus + 0.05 * r }),
            ImprovedEffect(cls, 1, "mage_fire_improved_ignite", "Improved Ignite", "mage_fire_ignite",
                TalentEffectType.IgniteDamage, 0.15,
                r => $"Ignite deals {Pct(0.15 * r)} more damage."),
            ImproveAbility(cls, 2, "mage_frost_improved_ice_barrier", "Improved Ice Barrier", "Ice Barrier",
                "mage_frost_ice_barrier", "mage_frost_ice_barrier",
                r => $"Ice Barrier absorbs an additional {Pct(0.05 * r)} of your maximum health.",
                (a, r) => a with { BuffShieldPct = a.BuffShieldPct + 0.05 * r }),
        ],
        CharacterClass.Priest =>
        [
            ImproveAbility(cls, 0, "priest_disc_improved_divine_shield", "Improved Divine Shield", "Divine Shield",
                "priest_disc_divine_shield", "priest_disc_divine_shield",
                r => $"Divine Shield absorbs an additional {Pct(0.05 * r)} of your maximum health.",
                (a, r) => a with { BuffShieldPct = a.BuffShieldPct + 0.05 * r }),
            ImproveAbility(cls, 1, "priest_holy_improved_holy_nova", "Improved Holy Nova", "Holy Nova",
                "priest_holy_holy_nova", "priest_holy_holy_nova",
                r => $"Increases the damage and healing of Holy Nova by {Pct(0.15 * r)}.",
                (a, r) => DamageAndHeal(a, 0.15 * r)),
            ImproveForm(cls, 2, "priest_shadow_improved_shadowform", "Improved Shadowform", DruidForm.Shadow,
                "priest_shadow_mind_flay", "priest_shadow_shadowform",
                r => $"Shadowform increases the damage of Smite, Mind Blast and Shadow Word: Pain by an additional {Pct(0.05 * r)}.",
                (f, r) => f with { BoostedAbilityDamageBonus = f.BoostedAbilityDamageBonus + 0.05 * r }),
        ],
        CharacterClass.Rogue =>
        [
            ImproveAbility(cls, 0, "rogue_assa_improved_cold_blood", "Improved Cold Blood", "Cold Blood",
                "rogue_ass_cold_blood", "rogue_ass_cold_blood",
                r => $"Reduces the cooldown of Cold Blood by {5 * r} sec.",
                (a, r) => a with { Cooldown = Math.Max(0, a.Cooldown - 5 * r) }),
            ImproveAbility(cls, 1, "rogue_combat_improved_blade_flurry", "Improved Blade Flurry", "Blade Flurry",
                "rogue_combat_blade_flurry", "rogue_combat_blade_flurry",
                r => $"Blade Flurry lasts {2 * r} sec longer.",
                (a, r) => a with { BuffDuration = a.BuffDuration + 2 * r }),
            ImproveAbility(cls, 2, "rogue_sub_improved_ghostly_strike", "Improved Ghostly Strike", "Ghostly Strike",
                "rogue_sub_ghostly_strike", "rogue_sub_ghostly_strike",
                r => $"Increases the damage of Ghostly Strike by {Pct(0.15 * r)} and its shield by {Pct(0.025 * r)} of your maximum health.",
                (a, r) => a.WithDamageBonus(0.15 * r) with { BuffShieldPct = a.BuffShieldPct + 0.025 * r }),
        ],
        CharacterClass.Paladin =>
        [
            ImproveAbility(cls, 0, "paladin_holy_improved_holy_shock", "Improved Holy Shock", "Holy Shock",
                "paladin_holy_holy_shock", "paladin_holy_holy_shock",
                r => $"Increases the damage and healing of Holy Shock by {Pct(0.15 * r)}.",
                (a, r) => DamageAndHeal(a, 0.15 * r)),
            ImproveAbility(cls, 1, "paladin_prot_improved_holy_shield", "Improved Holy Shield", "Holy Shield",
                "paladin_prot_holy_shield", "paladin_prot_holy_shield",
                r => $"Holy Shield lasts for {r} additional attack{(r == 1 ? "" : "s")}.",
                (a, r) => a with { BuffHitCharges = a.BuffHitCharges + r }),
            ImproveAbility(cls, 2, "paladin_ret_improved_seal_of_command", "Improved Seal of Command", "Seal of Command",
                "paladin_ret_seal_of_command", "paladin_ret_seal_of_command",
                r => $"Increases the chance of Seal of Command to grant an extra attack by {Pct(0.05 * r)}.",
                (a, r) => a with { BuffExtraAttackChance = a.BuffExtraAttackChance + 0.05 * r }),
        ],
        CharacterClass.Hunter =>
        [
            ImprovedEffect(cls, 0, "hunter_bm_improved_tame_pet", "Improved Tame Pet", "hunter_bm_tame_pet",
                TalentEffectType.PetDamageBonus, 0.05,
                r => $"Your pet deals {Pct(0.05 * r)} more damage."),
            ImproveAbility(cls, 1, "hunter_mm_improved_sniper_shot", "Improved Sniper Shot", "Sniper Shot",
                "hunter_mm_sniper_shot", "hunter_mm_sniper_shot",
                r => $"Increases the damage of Sniper Shot by {Pct(0.10 * r)}.",
                (a, r) => a.WithDamageBonus(0.10 * r)),
            ImproveAbility(cls, 2, "hunter_surv_improved_mongoose_bite", "Improved Mongoose Bite", "Mongoose Bite",
                "hunter_surv_mongoose_bite", "hunter_surv_mongoose_bite",
                r => $"Increases the damage of Mongoose Bite by {Pct(0.10 * r)} and its bonus to your next Raptor Strike by {Pct(0.10 * r)}.",
                (a, r) => a.WithDamageBonus(0.10 * r) with { EmpowerDamageBonus = a.EmpowerDamageBonus + 0.10 * r }),
        ],
        CharacterClass.Warlock =>
        [
            ImproveAbility(cls, 0, "warlock_affli_improved_siphon_life", "Improved Siphon Life", "Siphon Life",
                "warlock_aff_siphon_life", "warlock_aff_siphon_life",
                r => $"Increases the damage of Siphon Life by {Pct(0.20 * r)}.",
                (a, r) => a with { DebuffDotDamage = Percent(a.DebuffDotDamage, 0.20 * r) }),
            ImprovedEffect(cls, 1, "warlock_demo_improved_summon_demon", "Improved Summon Demon", "warlock_demo_summon_demon",
                TalentEffectType.PetHealthBonus, 0.05,
                r => $"Your Voidwalker gains health equal to {Pct(0.05 * r)} of your maximum health."),
            ImproveAbility(cls, 2, "warlock_destro_improved_incinerate", "Improved Incinerate", "Incinerate",
                "warlock_dest_incinerate", "warlock_dest_incinerate",
                r => $"Increases the damage of Incinerate by {Pct(0.10 * r)}.",
                (a, r) => a.WithDamageBonus(0.10 * r)),
        ],
        CharacterClass.Shaman =>
        [
            ImproveAbility(cls, 0, "shaman_ele_improved_chain_lightning", "Improved Chain Lightning", "Chain Lightning",
                "shaman_ele_chain_lightning", "shaman_ele_chain_lightning",
                r => $"Increases the damage of Chain Lightning by {Pct(0.10 * r)}.",
                (a, r) => a.WithDamageBonus(0.10 * r)),
            ImproveAbility(cls, 1, "shaman_enh_improved_windfury_weapon", "Improved Windfury Weapon", "Windfury Weapon",
                "shaman_enh_windfury_weapon", "shaman_enh_windfury_weapon",
                r => $"Increases the chance of Windfury Weapon to trigger by {Pct(0.05 * r)}.",
                (a, r) => a with { BuffExtraAttackChance = a.BuffExtraAttackChance + 0.05 * r }),
            ImproveAbility(cls, 2, "shaman_resto_improved_mana_tide_totem", "Improved Mana Tide Totem", "Mana Tide Totem",
                "shaman_rest_mana_tide", "shaman_rest_mana_tide",
                r => $"Mana Tide Totem restores an additional {Pct(0.04 * r)} of your maximum mana.",
                (a, r) => a with { MotManaRestorePct = a.MotManaRestorePct + 0.04 * r }),
        ],
        CharacterClass.Druid =>
        [
            ImproveForm(cls, 0, "druid_bal_improved_moonkin_form", "Improved Moonkin Form", DruidForm.Moonkin,
                "druid_bal_moonkin_form", "druid_bal_moonkin_form",
                r => $"Moonkin Form increases the damage of Wrath, Starfire and Moonfire by an additional {Pct(0.05 * r)}.",
                (f, r) => f with { BoostedAbilityDamageBonus = f.BoostedAbilityDamageBonus + 0.05 * r }),
            ImproveForm(cls, 1, "druid_feral_improved_bear_form", "Improved Bear Form", DruidForm.Bear,
                "druid_feral_faerie_fire_bear", "druid_feral_bear_form",
                r => $"Bear Form reduces damage taken by an additional {Pct(0.03 * r)}.",
                (f, r) => f with { DamageTakenReduction = f.DamageTakenReduction + 0.03 * r }, column: 0),
            ImproveForm(cls, 1, "druid_feral_improved_cat_form", "Improved Cat Form", DruidForm.Cat,
                "druid_feral_faerie_fire_cat", "druid_feral_cat_form",
                r => $"Cat Form increases your critical strike chance by {Pct(0.02 * r)}.",
                (f, r) => f with { CritChanceBonus = f.CritChanceBonus + 0.02 * r }, column: 3),
            ImproveForm(cls, 2, "druid_resto_improved_tree_form", "Improved Tree Form", DruidForm.Tree,
                "druid_rest_tree_form", "druid_rest_tree_form",
                r => $"Tree Form increases all healing done by an additional {Pct(0.05 * r)}.",
                (f, r) => f with { HealingBonus = f.HealingBonus + 0.05 * r }),
        ],
        _ => [],
    };

    private static string Pct(double value) => $"{value * 100:0.#}%";

    private static Ability DamageAndHeal(Ability ability, double bonus) =>
        ability.WithDamageBonus(bonus) with { Heal = Percent(ability.Heal, bonus) };

    /// <summary>Icon of the base talent – the improved talent visibly belongs to it.</summary>
    private static string BaseIcon(CharacterClass cls, string baseTalentId) =>
        BuildTalents(cls).FirstOrDefault(t => t.Id == baseTalentId)?.IconUrl ?? "";

    /// <summary>Improves the ability <paramref name="abilityName"/> via the effective ability copy.</summary>
    private static TalentDefinition ImproveAbility(
        CharacterClass cls, int tree, string id, string name, string abilityName,
        string requires, string baseTalentId, Func<int, string> rankText, Func<Ability, int, Ability> modifier,
        int column = 0) => new()
    {
        Id                  = id,
        Name                = name,
        Description         = PerRankDescription(rankText),
        Class               = cls,
        TreeIndex           = tree,
        Row                 = ImprovedRow,
        Column              = column,
        MaxRanks            = ImprovedMaxRanks,
        RequiresTalentId    = requires,
        EffectType          = TalentEffectType.ModifyAbility,
        EffectPerRank       = 1.0,
        ModifiedAbilityName = abilityName,
        AbilityModifier     = modifier,
        RankText            = rankText,
        IconUrl             = BaseIcon(cls, baseTalentId),
    };

    /// <summary>Improves a shapeshift form via its form data.</summary>
    private static TalentDefinition ImproveForm(
        CharacterClass cls, int tree, string id, string name, DruidForm form,
        string requires, string baseTalentId, Func<int, string> rankText,
        Func<ShapeshiftFormInfo, int, ShapeshiftFormInfo> modifier, int column = 0) => new()
    {
        Id               = id,
        Name             = name,
        Description      = PerRankDescription(rankText),
        Class            = cls,
        TreeIndex        = tree,
        Row              = ImprovedRow,
        Column           = column,
        MaxRanks         = ImprovedMaxRanks,
        RequiresTalentId = requires,
        EffectType       = TalentEffectType.ImprovedForm,
        EffectPerRank    = 1.0,
        ModifiedForm     = form,
        FormModifier     = modifier,
        RankText         = rankText,
        IconUrl          = BaseIcon(cls, baseTalentId),
    };

    /// <summary>Improves via a generic per-rank talent effect (pet damage, Ignite …).</summary>
    private static TalentDefinition ImprovedEffect(
        CharacterClass cls, int tree, string id, string name, string baseTalentId,
        TalentEffectType effect, double effectPerRank, Func<int, string> rankText) => new()
    {
        Id               = id,
        Name             = name,
        Description      = PerRankDescription(rankText),
        Class            = cls,
        TreeIndex        = tree,
        Row              = ImprovedRow,
        Column           = 0,
        MaxRanks         = ImprovedMaxRanks,
        RequiresTalentId = baseTalentId,
        EffectType       = effect,
        EffectPerRank    = effectPerRank,
        RankText         = rankText,
        IconUrl          = BaseIcon(cls, baseTalentId),
    };

    /// <summary>"Rank 1: … Rank 2: …" – the tooltip shows the current and next rank separately.</summary>
    private static string PerRankDescription(Func<int, string> rankText) =>
        string.Join(" ", Enumerable.Range(1, ImprovedMaxRanks).Select(r => $"Rank {r}: {rankText(r)}"));
}
