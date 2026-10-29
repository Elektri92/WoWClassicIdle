// Models/TalentRegistry.SpecAbilities.cs
namespace Models;

// Talent row 3 (Row = 2, from Lv 20, 10 points in the tree): one point per tree for the specialization ability
// or – for pure passives (Flurry, Spirit of Redemption, Nightfall, Eclipse) – for the talent effect.
public static partial class TalentRegistry
{
    /// <summary>Minimum level of all specialization talents.</summary>
    public const int SpecAbilityLevel = 20;

    private static IEnumerable<TalentDefinition> BuildSpecAbilityTalents(CharacterClass cls) => cls switch
    {
        CharacterClass.Mage =>
        [
            Spec(cls, 0, "mage_arcane_arcane_blast", "Arcane Blast", "Arcane_Blast_r1",
                "Teaches you Arcane Blast: a fast arcane nuke that empowers your next Arcane Missiles by 30%."),
            Spec(cls, 1, "mage_fire_flamestrike", "Flamestrike", "Flamestrike_r1",
                "Teaches you Flamestrike: scorches the enemy and leaves burning ground behind. Its critical strikes trigger Ignite."),
            Spec(cls, 2, "mage_frost_frost_nova", "Frost Nova", "Frost_Nova_r1",
                "Teaches you Frost Nova: damages and freezes the enemy for 5 sec. Frostbolt has +50% critical strike chance against frozen targets (Shatter)."),
        ],
        CharacterClass.Warrior =>
        [
            Spec(cls, 0, "warrior_arms_overpower", "Overpower", "Overpower_r1",
                "Teaches you Overpower: a quick weapon strike with +25% critical strike chance. Only usable in Battle Stance.",
                requires: "warrior_arms_battle_stance"),
            Passive(cls, 1, "warrior_fury_flurry", "Flurry", TalentEffectType.BerserkerFlurry, 0.30, "ability_ghoulfrenzy",
                "Passive, like the Shaman talent: after a melee critical strike your next 3 swings are 30% faster. Only active in Berserker Stance.",
                requires: "warrior_fury_berserker_stance"),
            Spec(cls, 2, "warrior_prot_intimidating_shout", "Intimidating Shout", "Intimidating_Shout_r1",
                "Teaches you Intimidating Shout: the enemy flees in terror and cannot attack for 6 sec. Only usable in Defensive Stance.",
                requires: "warrior_def_defensive_stance"),
        ],
        CharacterClass.Priest =>
        [
            Spec(cls, 0, "priest_disc_penance", "Penance", "Penance_r1",
                "Teaches you Penance: channels 3 holy bolts at the enemy and heals you at the same time."),
            Passive(cls, 1, "priest_holy_spirit_of_redemption", "Spirit of Redemption", TalentEffectType.SpiritOfRedemption, 1.0,
                "inv_enchant_essenceeternallarge",
                "Passive. A killing blow turns you into a Spirit of Redemption instead: for 8 sec you are immune, your next 10 spells cost no mana and your healing is increased by 50%. Can occur once every 2 min."),
            Spec(cls, 2, "priest_shadow_mind_flay", "Mind Flay", "Mind_Flay_r1",
                "Teaches you Mind Flay: a channeled shadow spell that damages and slows the enemy. Only usable in Shadowform.",
                requires: "priest_shadow_shadowform"),
        ],
        CharacterClass.Rogue =>
        [
            Spec(cls, 0, "rogue_assa_mutilate", "Mutilate", "Mutilate_r1",
                "Teaches you Mutilate: strikes with both weapons at once, dealing 25% more damage to poisoned targets."),
            Spec(cls, 1, "rogue_combat_adrenaline_rush", "Adrenaline Rush", "Adrenaline_Rush_r1",
                "Teaches you Adrenaline Rush: doubles your energy regeneration for 15 sec."),
            Spec(cls, 2, "rogue_sub_vanish", "Vanish", "Vanish_r1",
                "Teaches you Vanish: you take no damage for 4 sec and your next ability costs no energy."),
        ],
        CharacterClass.Paladin =>
        [
            Spec(cls, 0, "paladin_holy_holy_aura", "Holy Aura", "Holy_Aura",
                "Teaches you Holy Aura (passive): +20% healing done, +25% mana regeneration and +5% damage. Only one aura can be equipped at a time."),
            Spec(cls, 1, "paladin_prot_protection_aura", "Protection Aura", "Protection_Aura",
                "Teaches you Protection Aura (passive): reduces all damage taken by 10%. Only one aura can be equipped at a time."),
            Spec(cls, 2, "paladin_ret_retribution_aura", "Retribution Aura", "Retribution_Aura",
                "Teaches you Retribution Aura (passive): deals holy damage to every attacker. Only one aura can be equipped at a time."),
        ],
        CharacterClass.Hunter =>
        [
            Spec(cls, 0, "hunter_bm_aspect_owl", "Aspect of the Owl", "Aspect_of_the_Owl",
                "Teaches you Aspect of the Owl (passive): your pet deals 50% more damage and your mana regenerates 30% faster. Only one aspect can be equipped at a time."),
            Spec(cls, 1, "hunter_mm_aspect_hawk", "Aspect of the Hawk", "Aspect_of_the_Hawk",
                "Teaches you Aspect of the Hawk (passive): +5% damage and +3% critical strike chance. Only one aspect can be equipped at a time."),
            Spec(cls, 2, "hunter_surv_aspect_monkey", "Aspect of the Monkey", "Aspect_of_the_Monkey",
                "Teaches you Aspect of the Monkey (passive): -15% damage taken and +8% attack speed. Only one aspect can be equipped at a time."),
        ],
        CharacterClass.Warlock =>
        [
            Passive(cls, 0, "warlock_affli_nightfall", "Nightfall", TalentEffectType.Nightfall, 0.10, "spell_shadow_twilight",
                "Passive. Each tick of your damage over time spells has a 10% chance to grant Shadow Trance: your next Shadow Bolt is instant."),
            Spec(cls, 1, "warlock_demo_soul_link", "Soul Link", "Soul_Link",
                "Teaches you Soul Link (passive): while your demon lives, it takes an additional 20% of the damage dealt to you and you deal 5% more damage."),
            Spec(cls, 2, "warlock_destro_conflagrate", "Conflagrate", "Conflagrate_r1",
                "Teaches you Conflagrate: instant fire damage, 50% more against targets afflicted by Immolate."),
        ],
        CharacterClass.Shaman =>
        [
            Spec(cls, 0, "shaman_ele_lava_burst", "Lava Burst", "Lava_Burst_r1",
                "Teaches you Lava Burst: hurls molten lava at the enemy. Always critically strikes if Flame Shock is on the target."),
            Spec(cls, 1, "shaman_enh_stormstrike", "Stormstrike", "Stormstrike_r1",
                "Teaches you Stormstrike: strikes twice with your weapon and makes the target take 5% more damage for 10 sec."),
            Spec(cls, 2, "shaman_resto_earth_shield", "Earth Shield", "Earth_Shield_r1",
                "Teaches you Earth Shield: protects you with 6 charges of earth. Each attack against you consumes a charge and heals you."),
        ],
        CharacterClass.Druid =>
        [
            Passive(cls, 0, "druid_bal_eclipse", "Eclipse", TalentEffectType.Eclipse, 1.0, "ability_druid_eclipse",
                "Passive. Casting Wrath increases the damage of your next Starfire by 30%, casting Starfire increases the damage of your next Wrath by 75% – alternating always pays off."),
            Spec(cls, 1, "druid_feral_faerie_fire_bear", "Faerie Fire (Bear)", "Faerie_Fire_Bear",
                "Teaches you Faerie Fire (Bear): damages the enemy, slows its attacks by 10% and increases its damage taken by 5%. Only usable in Bear Form.",
                requires: "druid_feral_bear_form", column: 0),
            Spec(cls, 1, "druid_feral_faerie_fire_cat", "Faerie Fire (Cat)", "Faerie_Fire_Cat",
                "Teaches you Faerie Fire (Cat): lowers the enemy's armor, increasing its damage taken by 8%. Only usable in Cat Form.",
                requires: "druid_feral_cat_form", column: 2),
            Spec(cls, 2, "druid_resto_tranquility", "Tranquility", "Tranquility_r1",
                "Teaches you Tranquility: channels waves of nature that damage the enemy and heal you at the same time."),
        ],
        _ => [],
    };

    /// <summary>One point in row 3 with a pure passive effect without ability/slot (like Shaman Flurry).</summary>
    private static TalentDefinition Passive(
        CharacterClass cls, int tree, string id, string name, TalentEffectType effect, double effectPerRank,
        string icon, string description, string? requires = null) => new()
    {
        Id               = id,
        Name             = name,
        Description      = description,
        Class            = cls,
        TreeIndex        = tree,
        Row              = 2,
        Column           = 1,
        MaxRanks         = 1,
        LevelRequired    = SpecAbilityLevel,
        RequiresTalentId = requires,
        EffectType       = effect,
        EffectPerRank    = effectPerRank,
        IconUrl          = $"https://wow.zamimg.com/images/wow/icons/medium/{icon}.jpg",
    };

    /// <summary>One point in row 3, unlocks <paramref name="abilityId"/>; icon = ability icon.</summary>
    private static TalentDefinition Spec(
        CharacterClass cls, int tree, string id, string name, string abilityId, string description,
        string? requires = null, int column = 1) => new()
    {
        Id               = id,
        Name             = name,
        Description      = description,
        Class            = cls,
        TreeIndex        = tree,
        Row              = 2,
        Column           = column,
        MaxRanks         = 1,
        LevelRequired    = SpecAbilityLevel,
        RequiresTalentId = requires,
        EffectType       = TalentEffectType.GrantSpecAbility,
        EffectPerRank    = 1.0,
        GrantsAbilityId  = abilityId,
        IconUrl          = (ClassAbilityRegistry.FindById(abilityId)?.IconUrl ?? "").Replace("/large/", "/medium/"),
    };
}
