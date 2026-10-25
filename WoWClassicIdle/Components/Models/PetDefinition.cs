// Models/PetDefinition.cs
namespace Models;

public enum PetKind
{
    None,
    Cat,         // Hunter: Schadens-Pet
    Bear,        // Hunter: Tank-Pet
    Voidwalker,  // Warlock: Tank-Dämon
}

/// <summary>
/// Statische Werte eines Begleittiers. Leben = Anteil der Spieler-HP,
/// Schaden = <see cref="BaseDamage"/> + <see cref="DamagePerLevel"/> × Spielerlevel alle <see cref="AttackInterval"/> Sekunden.
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
    /// <summary>Anteil des Gegnerschadens, den ein lebendes Pet statt des Spielers abfängt (0.10 = 10 %).</summary>
    public const double DamageShare = 0.10;

    /// <summary>Cooldown der Beschwörung nach dem Tod des Pets in Sekunden.</summary>
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

    /// <summary>Durchschnittlicher Treffer des Pets auf einem Spielerlevel (ohne Varianz).</summary>
    public static int AverageHit(PetDefinition def, int ownerLevel) =>
        Math.Max(1, (int)Math.Round(def.BaseDamage + def.DamagePerLevel * ownerLevel));
}
