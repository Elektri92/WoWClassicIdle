// Models/GameFeatureRegistry.cs
namespace Models;

/// <summary>
/// Ein per Level freigeschaltetes Spielfeature samt Kurzerklärung für das Freischalt-Popup.
/// </summary>
public sealed record GameFeature(
    string Id,
    int UnlockLevel,
    string Title,
    string IconUrl,
    string Summary,
    IReadOnlyList<string> Points,
    string? Href = null,
    string? LinkLabel = null);

/// <summary>
/// Statische Liste aller Features, die beim Levelaufstieg freigeschaltet werden.
/// Level-Schwellen kommen aus den jeweiligen Systemen, damit sie nie auseinanderlaufen.
/// </summary>
public static class GameFeatureRegistry
{
    /// <summary>Ab diesem Level erhält der Charakter Talentpunkte (siehe Player.Talents).</summary>
    public const int TalentUnlockLevel = 10;

    /// <summary>Ab diesem Level können Rare-Gegner spawnen (siehe EnemySpawner).</summary>
    public const int RareEnemyUnlockLevel = 3;

    public const string IdleFighterIntroId = "idle_fighter";

    public static IReadOnlyList<GameFeature> All { get; } = Build();

    /// <summary>Einmalige Einführungen beim ersten Betreten einer Seite (UnlockLevel 0 = ohne Levelanzeige).</summary>
    private static readonly Dictionary<string, GameFeature> _intros = new()
    {
        [IdleFighterIntroId] = new(IdleFighterIntroId, 0, "Idle Fighter",
            "https://wow.zamimg.com/images/wow/icons/large/inv_sword_04.jpg",
            "Your character fights enemies on their own – earning experience, copper and loot while you watch.",
            [
                "Enemies within two levels of yours spawn one after another and are fought automatically.",
                $"Up to {ClassAbilityRegistry.MaxSlotsPerClass} abilities from your Ability Loadout are used on their own; auto attacks fill the gaps.",
                "Loot goes straight into your inventory; duplicate items are sold automatically.",
                "If you die, you resurrect at the Spirit Healer after a short delay and keep fighting.",
                "Combat only runs while this page is open and the window is visible.",
            ]),
    };

    public static GameFeature? GetIntro(string id) => _intros.GetValueOrDefault(id);

    /// <summary>Features, deren Schwelle beim Aufstieg von <paramref name="oldLevel"/> auf <paramref name="newLevel"/> überschritten wurde.</summary>
    public static IEnumerable<GameFeature> UnlockedBetween(int oldLevel, int newLevel) =>
        All.Where(f => f.UnlockLevel > oldLevel && f.UnlockLevel <= newLevel);

    private static List<GameFeature> Build()
    {
        var list = new List<GameFeature>
        {
            new("rare_enemies", RareEnemyUnlockLevel, "Rare Enemies",
                "https://wow.zamimg.com/images/wow/icons/large/inv_misc_head_dragon_01.jpg",
                "Every now and then a rare elite (marked with ★) appears instead of a regular enemy.",
                [
                    "10 % chance to spawn once you and the enemy are at least level 3.",
                    "Rares have double health and hit 50 % harder.",
                    "Defeating one guarantees an uncommon armor piece.",
                    "There is also a 25 % chance for a class weapon and 15 % for rare (blue) armor from level 11.",
                ]),

            new("professions", Professions.MinCharacterLevel, "Professions",
                "https://wow.zamimg.com/images/wow/icons/large/trade_blacksmithing.jpg",
                "Gather materials and craft your own gear, potions and food.",
                [
                    "Mining, Herbalism, Skinning and Fishing are learned automatically and shared across your account.",
                    "Each character can learn one primary crafting profession.",
                    "Assign idle characters to gathering zones to collect materials passively.",
                    "Consumables like potions and food are used automatically during combat.",
                ],
                "professions", "Open Professions"),

            new("talents", TalentUnlockLevel, "Talents",
                "https://wow.zamimg.com/images/wow/icons/large/ability_rogue_slicedice.jpg",
                "Specialise your character by spending talent points in three talent trees.",
                [
                    "You earn one talent point for every level from 10 onwards.",
                    "Investing points in a tree unlocks its deeper talent rows.",
                    "Some talents grant powerful new abilities for your loadout.",
                ],
                "talents", "Open Talents"),
        };

        // Taschenplätze: erste Freischaltung ausführlich, weitere nur kurz
        var bagLevels = Inventory.BagUnlockLevels;
        for (int i = 0; i < bagLevels.Length; i++)
        {
            list.Add(i == 0
                ? new GameFeature("bag_slot_1", bagLevels[i], "Bag Slots",
                    "https://wow.zamimg.com/images/wow/icons/large/inv_misc_bag_07.jpg",
                    "Your first bag slot is open, giving you extra inventory space.",
                    [
                        $"Further bag slots open at levels {string.Join(", ", bagLevels[1..])}.",
                        $"Each bag starts with {Inventory.BaseBagSlotSize} slots and grows by one every 5 levels.",
                        "More space means fewer full-bag interruptions while idling.",
                    ],
                    "inventory", "Open Inventory")
                : new GameFeature($"bag_slot_{i + 1}", bagLevels[i], "New Bag Slot",
                    "https://wow.zamimg.com/images/wow/icons/large/inv_misc_bag_07.jpg",
                    $"Bag slot {i + 1} of {Inventory.BagSlotCount} is now available.",
                    [$"You now have {Inventory.BaseBagSlotSize + bagLevels[i] / 5} additional inventory slots."],
                    "inventory", "Open Inventory"));
        }

        return list.OrderBy(f => f.UnlockLevel).ToList();
    }
}
