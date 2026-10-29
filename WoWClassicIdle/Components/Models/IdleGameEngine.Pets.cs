using Models;

// Pets (Hunter pets, Warlock demons): summoning, attacks, damage split, death and revival.
public partial class IdleGameEngine
{
    /// <summary>Currently summoned, living pet (null = none or dead).</summary>
    public ActivePet? Pet { get; private set; }

    /// <summary>Summons the ability's pet with full HP (share of the player HP + talents).</summary>
    private void SummonPet(Ability ability)
    {
        var def = PetRegistry.Get(ability.SummonsPet);
        if (def is null) return;
        int maxHp = (int)Math.Round(Player.Stats.MaxHp * (def.HpPctOfOwner + Player.PetHealthTalentBonus));
        Pet = new ActivePet(def, ability, maxHp);
        LastLog = $"✦ {def.Name} summoned ({Pet.MaxHp} HP)";
    }

    /// <summary>Pet attacks at its own pace. Returns the damage dealt to the enemy.</summary>
    private int UpdatePet(double dt, double enemyDmgTakenMult)
    {
        if (Pet is null) return 0;

        // Summon no longer equipped (loadout/class change, talent reset) → pet disappears
        if (!Player.EquippedAbilities.Any(a => a.Id == Pet.SummonAbility.Id))
        {
            DismissPet();
            return 0;
        }

        var enemy = CurrentEnemy;
        if (enemy is not { IsAlive: true }) return 0;

        int dealt = 0;
        var def   = Pet.Definition;
        Pet.AttackAcc += dt;
        while (Pet.AttackAcc >= def.AttackInterval && enemy.IsAlive)
        {
            Pet.AttackAcc -= def.AttackInterval;
            double variance = 0.85 + _rng.NextDouble() * 0.30;   // 85–115 % like enemy hits
            int dmg = Math.Max(1, (int)Math.Round(PetRegistry.AverageHit(def, Player.Level) * variance * enemyDmgTakenMult
                                                  * (1.0 + Player.PetDamageTalentBonus + Player.Buffs.PetDamageBonus)));
            Meter.RecordAbility(Pet.MeterSource, CombatActionKind.Damage, enemy.TakeDamage(dmg));
            CombatTextRaised?.Invoke(new CombatText(CombatTextTarget.Enemy, CombatTextKind.Ability, dmg, false));
            dealt += dmg;
        }
        return dealt;
    }

    /// <summary>
    /// A living pet absorbs <see cref="PetRegistry.DamageShare"/> of the enemy hit.
    /// Returns the share the player still takes.
    /// </summary>
    private int SplitDamageWithPet(int damage)
    {
        if (Pet is not { IsAlive: true }) return damage;
        // At least 1 damage, even if 10 % of a small hit would round to 0
        // Soul Link increases the share
        int share = Math.Max(1, (int)Math.Round(damage * (PetRegistry.DamageShare + Player.Buffs.PetDamageShareBonus)));

        Pet.TakeDamage(share);
        if (!Pet.IsAlive)
            OnPetDied();
        return Math.Max(1, damage - share);
    }

    /// <summary>Pet died: the summon goes on cooldown and is recast automatically afterwards.</summary>
    private void OnPetDied()
    {
        var pet = Pet!;
        Player.Cooldowns.StartCooldown(pet.SummonAbility.Id, PetRegistry.DeathCooldown);
        LastLog = $"✖ Your {pet.Definition.Name} has died!";
        Pet = null;
    }

    /// <summary>Removes the pet without cooldown (player death, character switch).</summary>
    private void DismissPet() => Pet = null;
}
