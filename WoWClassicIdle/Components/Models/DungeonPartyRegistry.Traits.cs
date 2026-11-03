// Models/DungeonPartyRegistry.Traits.cs
namespace Models;

public enum NpcRescue { None, Rebirth, Soulstone }

/// <summary>Tag on the party cards: where a companion shines.</summary>
public enum NpcProfile { Support, Boss, Trash, Safety, Damage }

/// <summary>One form of a group buff; <paramref name="Resources"/> = resources of the player that profit from it.</summary>
public sealed record NpcAuraVariant(string Name, string Icon, ResourceKind[] Resources, string Text, Func<Ability, int, Ability> Effect)
{
    public string Id => $"npc_aura_{Name.Replace(' ', '_').ToLowerInvariant()}";

    /// <summary>The buff at the player's level; it lasts the whole run and is set again after a death.</summary>
    public Ability Build(int level) => Effect(new Ability
    {
        Id = Id,
        Name = Name,
        IconUrl = NpcTrait.IconUrl(Icon),
        BuffDuration = DungeonRules.PartyAuraSeconds,
    }, level);
}

/// <summary>Group buff of a companion; the first variant that fits the player's resource is used, none = no use.</summary>
public sealed record NpcAura(params NpcAuraVariant[] Variants)
{
    public NpcAuraVariant? For(ResourceKind resource) => Variants.FirstOrDefault(v => v.Resources.Contains(resource));
}

/// <summary>Mitigation grows from <paramref name="StartFactor"/> to <paramref name="MaxFactor"/> of the spell within <paramref name="Seconds"/> of a fight.</summary>
public sealed record NpcRamp(double StartFactor, double MaxFactor, double Seconds)
{
    public double FactorAt(double fightSeconds) =>
        StartFactor + (MaxFactor - StartFactor) * Math.Clamp(fightSeconds / Seconds, 0, 1);
}

/// <summary>
/// What sets a companion apart besides its spell. Utility is paid with <see cref="OutputScale"/>:
/// the more a companion brings, the less it deals, heals or blocks.
/// </summary>
public sealed record NpcTrait
{
    public double OutputScale { get; init; } = 1;
    /// <summary>Damage factors of the spell against trash and bosses.</summary>
    public double TrashScale { get; init; } = 1;
    public double BossScale  { get; init; } = 1;
    public NpcAura? Aura { get; init; }
    public NpcRescue Rescue { get; init; }
    public NpcRamp? Ramp { get; init; }
    /// <summary>Second spell that only hits bosses (<see cref="ExtraSpellOnBosses"/>) or only trash.</summary>
    public NpcSpell? ExtraSpell { get; init; }
    public bool ExtraSpellOnBosses { get; init; }
    /// <summary>Shield on every boss pull as a share of the player's health.</summary>
    public double BossOpenerShieldPct { get; init; }

    public double ScaleAgainst(Enemy enemy) => enemy.IsBoss ? BossScale : TrashScale;

    public NpcProfile Profile =>
        Rescue != NpcRescue.None                                   ? NpcProfile.Safety
        : ExtraSpellOnBosses || BossScale > 1 || BossOpenerShieldPct > 0 ? NpcProfile.Boss
        : ExtraSpell is not null || TrashScale > 1                 ? NpcProfile.Trash
        : Aura is null                                             ? NpcProfile.Damage
        : NpcProfile.Support;

    public static string IconUrl(string icon) => $"https://wow.zamimg.com/images/wow/icons/large/{icon}.jpg";

    public static string RescueName(NpcRescue rescue) => rescue == NpcRescue.Soulstone ? "Soulstone" : "Rebirth";
    public static string RescueIconUrl(NpcRescue rescue) =>
        IconUrl(rescue == NpcRescue.Soulstone ? "spell_shadow_soulgem" : "spell_nature_reincarnation");
}

public static partial class DungeonPartyRegistry
{
    private static readonly ResourceKind[] AnyResource = [ResourceKind.Mana, ResourceKind.Rage, ResourceKind.Energy];
    private static readonly ResourceKind[] Physical    = [ResourceKind.Rage, ResourceKind.Energy];
    private static readonly ResourceKind[] ManaOnly    = [ResourceKind.Mana];

    private static readonly NpcAura BattleShout = new(
        new NpcAuraVariant("Battle Shout", "ability_warrior_battleshout", Physical, "+10% auto-attack damage",
            (a, _) => a with { BuffWeaponDamageBonus = 0.10 }));

    private static readonly NpcAura Blessing = new(
        new NpcAuraVariant("Blessing of Might", "spell_holy_fistofjustice", Physical, "+10% auto-attack damage",
            (a, _) => a with { BuffWeaponDamageBonus = 0.10 }),
        new NpcAuraVariant("Blessing of Wisdom", "spell_holy_sealofwisdom", ManaOnly, "+25% mana regeneration, +2% damage dealt",
            (a, _) => a with { BuffResourceRegenBonus = 0.25, BuffDamageBonus = 0.02 }));

    private static readonly NpcAura MarkOfTheWild = new(
        new NpcAuraVariant("Mark of the Wild", "spell_nature_regeneration", AnyResource, "+3% damage dealt",
            (a, _) => a with { BuffDamageBonus = 0.03 }));

    private static readonly NpcAura DivineSpirit = new(
        new NpcAuraVariant("Divine Spirit", "spell_holy_divinespirit", ManaOnly, "+20% mana regeneration, +4% damage dealt",
            (a, _) => a with { BuffResourceRegenBonus = 0.20, BuffDamageBonus = 0.04 }));

    private static readonly NpcAura Totems = new(
        new NpcAuraVariant("Windfury Totem", "spell_nature_windfury", Physical, "15% chance on auto-attack for an extra attack",
            (a, _) => a with { BuffExtraAttackChance = 0.15, BuffExtraAttacks = 1 }),
        new NpcAuraVariant("Mana Spring Totem", "spell_nature_manaregentotem", ManaOnly, "+25% mana regeneration, +2% damage dealt",
            (a, _) => a with { BuffResourceRegenBonus = 0.25, BuffDamageBonus = 0.02 }));

    private static readonly NpcAura ArcaneIntellect = new(
        new NpcAuraVariant("Arcane Intellect", "spell_holy_magicalsentry", ManaOnly, "+Intellect for a larger mana pool, +3% critical strike chance",
            (a, level) => a with { BuffIntellectBonus = 3 + level / 2, BuffCritChanceBonus = 0.03 }));

    private static readonly NpcAura HuntersMark = new(
        new NpcAuraVariant("Hunter's Mark", "ability_hunter_snipershot", AnyResource, "+4% damage dealt",
            (a, _) => a with { BuffDamageBonus = 0.04 }));

    private static readonly NpcSpell Consecration = new("Consecration", "spell_holy_innerfire", NpcSpellKind.Damage, 2) { Scale = 0.25 };
    private static readonly NpcSpell Revenge      = new("Revenge", "ability_warrior_revenge", NpcSpellKind.Damage, 2, Melee: true) { Scale = 0.30 };

    private static readonly Dictionary<DungeonRole, Dictionary<CharacterClass, NpcTrait>> Traits = new()
    {
        [DungeonRole.Tank] = new()
        {
            // Rage builds up during a fight: weak on short trash pulls, the strongest tank on long boss fights
            [CharacterClass.Warrior] = new() { Ramp = new NpcRamp(0.5, 1.25, 20), ExtraSpell = Revenge, ExtraSpellOnBosses = true, Aura = BattleShout },
            [CharacterClass.Paladin] = new() { OutputScale = 0.85, ExtraSpell = Consecration, Aura = Blessing },
            [CharacterClass.Druid]   = new() { OutputScale = 0.90, Aura = MarkOfTheWild, Rescue = NpcRescue.Rebirth },
        },
        [DungeonRole.Healer] = new()
        {
            [CharacterClass.Priest]  = new() { OutputScale = 0.95, BossOpenerShieldPct = 0.15, Aura = DivineSpirit },
            [CharacterClass.Paladin] = new() { OutputScale = 0.95, Aura = Blessing },
            [CharacterClass.Shaman]  = new() { OutputScale = 0.90, Aura = Totems },
            [CharacterClass.Druid]   = new() { OutputScale = 0.85, Aura = MarkOfTheWild, Rescue = NpcRescue.Rebirth },
        },
        [DungeonRole.Damage] = new()
        {
            [CharacterClass.Rogue]   = new() { OutputScale = 1.10, BossScale = 1.20 },
            [CharacterClass.Mage]    = new() { OutputScale = 0.95, TrashScale = 1.25, Aura = ArcaneIntellect },
            [CharacterClass.Hunter]  = new() { OutputScale = 0.95, Aura = HuntersMark },
            [CharacterClass.Warrior] = new() { OutputScale = 0.95, Aura = BattleShout },
            [CharacterClass.Paladin] = new() { OutputScale = 0.90, Aura = Blessing },
            [CharacterClass.Shaman]  = new() { OutputScale = 0.90, Aura = Totems },
            [CharacterClass.Priest]  = new() { OutputScale = 0.95, Aura = DivineSpirit },
            [CharacterClass.Warlock] = new() { OutputScale = 0.85, Rescue = NpcRescue.Soulstone },
            [CharacterClass.Druid]   = new() { OutputScale = 0.85, Aura = MarkOfTheWild, Rescue = NpcRescue.Rebirth },
        },
    };

    public static NpcTrait TraitOf(DungeonRole role, CharacterClass cls) => Traits[role][cls];

    public static string ProfileLabel(NpcProfile profile) => profile switch
    {
        NpcProfile.Boss   => "Boss specialist",
        NpcProfile.Trash  => "Trash specialist",
        NpcProfile.Safety => "Safety",
        NpcProfile.Damage => "Raw power",
        _                 => "Support",
    };

    /// <summary>Tooltip lines about the strengths of a companion; <paramref name="resource"/> = the player's resource.</summary>
    public static IEnumerable<(string Text, bool Good)> TraitLines(DungeonPartyMember member, ResourceKind resource)
    {
        var trait = member.Trait;
        if (trait.Ramp is { } ramp)
            yield return ($"Builds rage: blocks {member.Spell.Mitigation * ramp.StartFactor * 100:0}% at the pull, up to {member.Spell.Mitigation * ramp.MaxFactor * 100:0}% after {ramp.Seconds:0} s – best on long boss fights", true);
        if (trait.BossScale > 1)
            yield return ($"+{(trait.BossScale - 1) * 100:0}% damage against bosses", true);
        if (trait.TrashScale > 1)
            yield return ($"+{(trait.TrashScale - 1) * 100:0}% damage against trash", true);
        if (trait.ExtraSpell is { } extra)
            yield return ($"{extra.Name}: also hits {(trait.ExtraSpellOnBosses ? "bosses" : "trash")} every {extra.Interval:0} s", true);
        if (trait.BossOpenerShieldPct > 0)
            yield return ($"Power Word: Shield on every boss pull, absorbing {trait.BossOpenerShieldPct * 100:0}% of your health", true);
        if (trait.Aura is { } aura)
            yield return aura.For(resource) is { } variant
                ? ($"{variant.Name}: {variant.Text}", true)
                : ($"{string.Join(" / ", aura.Variants.Select(v => v.Name))}: no use for your class", false);
        if (trait.Rescue != NpcRescue.None)
            yield return ($"{NpcTrait.RescueName(trait.Rescue)}: once per run, brings you back at {DungeonRules.RescueHpShare * 100:0}% health instead of a wipe", true);
        if (trait.OutputScale != 1)
            yield return ($"{(trait.OutputScale > 1 ? "Stronger" : "Weaker")} {RoleLabel(member.Role)}: {trait.OutputScale * 100:0}% of the usual output", trait.OutputScale > 1);
    }
}
