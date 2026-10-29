// Models/CombatText.cs
namespace Models;

public enum CombatTextTarget { Player, Enemy }

public enum CombatTextKind { AutoAttack, Ability, Heal }

/// <summary>
/// A hit or heal for the floating combat numbers (Floating Combat Text).
/// <paramref name="IsCrit"/> marks critical hits (<paramref name="Amount"/> already doubled).
/// </summary>
public sealed record CombatText(CombatTextTarget Target, CombatTextKind Kind, int Amount, bool IsCrit = false);
