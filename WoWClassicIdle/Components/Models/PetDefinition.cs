// Models/PetDefinition.cs
namespace Models;

public enum PetKind
{
    None,
    Cat,         // Hunter: Schadens-Pet
    Bear,        // Hunter: Tank-Pet
    Voidwalker,  // Warlock: tank demon
}

/// <summary>
/// Static values of a pet. Health = share of the player HP,
/// damage = <see cref="BaseDamage"/> + <see cref="DamagePerLevel"/> × player level every <see cref="AttackInterval"/> seconds.
/// </summary>
public sealed record PetDefinition(
    PetKind Kind,
    string Name,
    string IconUrl,
    double HpPctOfOwner,
    double AttackInterval,
    double BaseDamage,
    double DamagePerLevel,
    string Role);

public static class PetRegistry
{
    /// <summary>Share of enemy damage a living pet absorbs instead of the player (0.10 = 10 %).</summary>
    public const double DamageShare = 0.10;

    /// <summary>Cooldown of the summon after the pet's death in seconds.</summary>
    public const double DeathCooldown = 10;

    private static readonly Dictionary<PetKind, PetDefinition> _pets = new()
    {
        [PetKind.Cat] = new(PetKind.Cat, "Cat",
            "https://wow.zamimg.com/images/wow/icons/large/ability_hunter_pet_cat.jpg",
            HpPctOfOwner: 0.25, AttackInterval: 2.0, BaseDamage: 1, DamagePerLevel: 0.10, Role: "Damage"),
        [PetKind.Bear] = new(PetKind.Bear, "Bear",
            "https://wow.zamimg.com/images/wow/icons/large/ability_hunter_pet_bear.jpg",
            HpPctOfOwner: 0.45, AttackInterval: 2.5, BaseDamage: 1, DamagePerLevel: 0.12, Role: "Tank"),
        [PetKind.Voidwalker] = new(PetKind.Voidwalker, "Voidwalker",
            "https://wow.zamimg.com/images/wow/icons/large/spell_shadow_summonvoidwalker.jpg",
            HpPctOfOwner: 0.50, AttackInterval: 2.5, BaseDamage: 1, DamagePerLevel: 0.12, Role: "Tank"),
    };

    public static PetDefinition? Get(PetKind kind) => _pets.GetValueOrDefault(kind);

    /// <summary>Average hit of the pet at a player level (without variance).</summary>
    public static int AverageHit(PetDefinition def, int ownerLevel) =>
        Math.Max(1, (int)Math.Round(def.BaseDamage + def.DamagePerLevel * ownerLevel));
}
