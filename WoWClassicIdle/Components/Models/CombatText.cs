// Models/CombatText.cs
namespace Models;

public enum CombatTextTarget { Player, Enemy }

public enum CombatTextKind { AutoAttack, Ability, Heal }

/// <summary>
/// Ein Treffer bzw. eine Heilung für die schwebenden Kampfzahlen (Floating Combat Text).
/// <paramref name="IsCrit"/> markiert kritische Treffer (bereits verdoppelter <paramref name="Amount"/>).
/// </summary>
public sealed record CombatText(CombatTextTarget Target, CombatTextKind Kind, int Amount, bool IsCrit = false);
