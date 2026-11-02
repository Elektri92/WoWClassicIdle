// Models/RacialRegistry.cs
using System.Globalization;

namespace Models;

/// <summary>Display data of a passive racial ability.</summary>
public sealed record RacialTrait(string Name, string Description, string IconUrl);

/// <summary>Numbers behind the racial passives of a race; the defaults change nothing.</summary>
public sealed record RacialBonus
{
    public static readonly RacialBonus None = new();

    /// <summary>Factor on total Spirit (incl. gear).</summary>
    public double SpiritMultiplier { get; init; } = 1;
    /// <summary>Factor on total Intellect (incl. gear).</summary>
    public double IntellectMultiplier { get; init; } = 1;
    public double HpMultiplier { get; init; } = 1;
    public double HpRegenMultiplier { get; init; } = 1;
    public double CritChance { get; init; }
    public double AttackSpeedBonus { get; init; }
    public double DamageTakenReduction { get; init; }
    public double DamageBonus { get; init; }
    public double BeastDamageBonus { get; init; }
    public double PetDamageBonus { get; init; }
    public double CopperBonus { get; init; }
    /// <summary>Lower prices at merchants and riding trainers.</summary>
    public double VendorDiscount { get; init; }
    /// <summary>Shorter wait at the Spirit Healer.</summary>
    public double RespawnReduction { get; init; }
    /// <summary>Factor on the XP lost per death.</summary>
    public double DeathPenaltyMultiplier { get; init; } = 1;
    /// <summary>Share of maximum health restored per kill.</summary>
    public double KillHealShare { get; init; }
    public double FoodDurationBonus { get; init; }
}

/// <summary>Two passive abilities per race: always active, without a loadout slot.</summary>
public static class RacialRegistry
{
    private static readonly RacialBonus Human    = new() { SpiritMultiplier = 1.03, VendorDiscount = 0.10 };
    private static readonly RacialBonus Dwarf    = new() { DamageTakenReduction = 0.02, CopperBonus = 0.10 };
    private static readonly RacialBonus NightElf = new() { AttackSpeedBonus = 0.015, RespawnReduction = 0.50 };
    private static readonly RacialBonus Gnome    = new() { CritChance = 0.01, IntellectMultiplier = 1.05 };
    private static readonly RacialBonus Orc      = new() { DamageBonus = 0.02, PetDamageBonus = 0.05 };
    private static readonly RacialBonus Undead   = new() { KillHealShare = 0.015, DeathPenaltyMultiplier = 0.75 };
    private static readonly RacialBonus Tauren   = new() { HpMultiplier = 1.08, FoodDurationBonus = 0.50 };
    private static readonly RacialBonus Troll    = new() { HpRegenMultiplier = 1.08, BeastDamageBonus = 0.03 };

    private static readonly Dictionary<Race, RacialBonus> Bonuses = new()
    {
        [Race.Human] = Human, [Race.Dwarf] = Dwarf, [Race.NightElf] = NightElf, [Race.Gnome] = Gnome,
        [Race.Orc] = Orc, [Race.Undead] = Undead, [Race.Tauren] = Tauren, [Race.Troll] = Troll,
    };

    private static readonly Dictionary<Race, RacialTrait[]> Traits = new()
    {
        [Race.Human] =
        [
            new("The Human Spirit", $"Increases your Spirit by {Pct(Human.SpiritMultiplier - 1)}.", Icon("inv_enchant_shardbrilliantsmall")),
            new("Diplomacy", $"Merchants and riding trainers charge you {Pct(Human.VendorDiscount)} less.", Icon("inv_misc_note_02")),
        ],
        [Race.Dwarf] =
        [
            new("Stoneform", $"Reduces all damage you take by {Pct(Dwarf.DamageTakenReduction)}.", Icon("spell_shadow_unholystrength")),
            new("Treasure Hunter", $"Enemies you slay yield {Pct(Dwarf.CopperBonus)} more copper.", Icon("racial_dwarf_findtreasure")),
        ],
        [Race.NightElf] =
        [
            new("Quickness", $"Increases your attack speed by {Pct(NightElf.AttackSpeedBonus)}.", Icon("ability_racial_shadowmeld")),
            new("Wisp Spirit", $"Reduces your wait at the Spirit Healer by {Pct(NightElf.RespawnReduction)}.", Icon("spell_nature_wispsplode")),
        ],
        [Race.Gnome] =
        [
            new("Engineering Specialist", $"Increases your critical strike chance by {Pct(Gnome.CritChance)}.", Icon("trade_engineering")),
            new("Expansive Mind", $"Increases your Intellect by {Pct(Gnome.IntellectMultiplier - 1)}.", Icon("inv_enchant_essenceeternallarge")),
        ],
        [Race.Orc] =
        [
            new("Blood Fury", $"Increases all damage you deal by {Pct(Orc.DamageBonus)}.", Icon("racial_orc_berserkerstrength")),
            new("Command", $"Increases the damage of your pets by {Pct(Orc.PetDamageBonus)}.", Icon("ability_warrior_warcry")),
        ],
        [Race.Undead] =
        [
            new("Cannibalize", $"Every kill restores {Pct(Undead.KillHealShare)} of your maximum health.", Icon("ability_racial_cannibalize")),
            new("Will of the Forsaken", $"Dying costs you {Pct(1 - Undead.DeathPenaltyMultiplier)} less XP.", Icon("spell_shadow_raisedead")),
        ],
        [Race.Tauren] =
        [
            new("Endurance", $"Increases your maximum health by {Pct(Tauren.HpMultiplier - 1)}.", Icon("spell_nature_unyeildingstamina")),
            new("Hearty Appetite", $"Food buffs last {Pct(Tauren.FoodDurationBonus)} longer.", Icon("inv_misc_food_15")),
        ],
        [Race.Troll] =
        [
            new("Regeneration", $"Increases your health regeneration by {Pct(Troll.HpRegenMultiplier - 1)}.", Icon("spell_nature_regenerate")),
            new("Beast Slaying", $"Increases your damage against beasts by {Pct(Troll.BeastDamageBonus)}.", Icon("inv_misc_pelt_bear_ruin_02")),
        ],
    };

    // Creature types from Archetypes that count as beasts
    private static readonly HashSet<string> BeastTypes =
    [
        "Wolf", "Boar", "Bear", "Cat", "Spider", "Scorpid", "Raptor", "Crocolisk",
        "Bird", "Hyena", "Gorilla", "Kodo", "Basilisk", "Insect",
    ];

    public static RacialBonus BonusFor(Race race) => Bonuses[race];

    public static IReadOnlyList<RacialTrait> TraitsFor(Race race) => Traits[race];

    public static bool IsBeast(string enemyType) => BeastTypes.Contains(enemyType);

    private static string Icon(string name) => $"https://wow.zamimg.com/images/wow/icons/large/{name}.jpg";

    private static string Pct(double share) => (share * 100).ToString("0.#", CultureInfo.InvariantCulture) + "%";
}
