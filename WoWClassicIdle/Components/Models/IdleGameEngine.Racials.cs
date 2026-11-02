using Models;

// Racial passives that act in the game loop; stat effects are in Player.Racials.cs.
public partial class IdleGameEngine
{
    /// <summary>Simulator: no racial passives, so specs are compared independently of the race.</summary>
    public bool DisableRacials
    {
        get => Player.RacialsDisabled;
        set => Player.SetRacialsDisabled(value);
    }

    /// <summary>Price of a merchant offer for the active character; never below what the item sells for.</summary>
    public long MerchantPrice(MerchantOffer offer) =>
        Math.Max(offer.Item.SellPrice, DiscountedPrice(offer.Price));

    /// <summary>Price of a riding rank for the active character.</summary>
    public long RidingCost(RidingSkill skill) => DiscountedPrice(skill.Cost);

    private long DiscountedPrice(long price) =>
        (long)Math.Ceiling(price * (1 - Player.Racial.VendorDiscount));

    // Blood Fury and Beast Slaying; applied per hit and not in the enemy multiplier, which also reaches the dungeon group
    private double RacialDamageFactor(Enemy enemy)
    {
        var racial = Player.Racial;
        return (1 + racial.DamageBonus) * (RacialRegistry.IsBeast(enemy.Type) ? 1 + racial.BeastDamageBonus : 1);
    }

    /// <summary>Rounds randomly up or down, so small bonuses on small amounts (DoT ticks, copper) still count on average.</summary>
    private int RoundByChance(double amount)
    {
        int whole = (int)Math.Floor(amount);
        return _rng.NextDouble() < amount - whole ? whole + 1 : whole;
    }

    private int RacialDotTick(int amount, Enemy enemy)
    {
        double factor = RacialDamageFactor(enemy);
        return factor == 1 ? amount : RoundByChance(amount * factor);
    }

    private int RacialKillCopper(Enemy enemy)
    {
        int copper = enemy.Stats.CopperReward;
        double bonus = Player.Racial.CopperBonus;
        return bonus == 0 ? copper : RoundByChance(copper * (1 + bonus));
    }

    // Cannibalize
    private void ApplyRacialKillHeal()
    {
        double share = Player.Racial.KillHealShare;
        if (share == 0 || !Player.IsAlive) return;

        int before = Player.CurrentHp;
        Player.RegenHp((int)Math.Round(Player.Stats.MaxHp * share));
        int healed = Player.CurrentHp - before;
        if (healed > 0)
            CombatTextRaised?.Invoke(new CombatText(CombatTextTarget.Player, CombatTextKind.Heal, healed));
    }
}
