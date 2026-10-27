using Models;

// Begleittiere (Hunter-Pets, Warlock-Dämonen): Beschwörung, Angriffe, Schadensaufteilung, Tod und Wiederbelebung.
public partial class IdleGameEngine
{
    /// <summary>Aktuell beschworenes, lebendes Pet (null = keins oder tot).</summary>
    public ActivePet? Pet { get; private set; }

    /// <summary>Beschwört das Pet der Fähigkeit mit voller HP (Anteil der Spieler-HP + Talente).</summary>
    private void SummonPet(Ability ability)
    {
        var def = PetRegistry.Get(ability.SummonsPet);
        if (def is null) return;
        int maxHp = (int)Math.Round(Player.Stats.MaxHp * (def.HpPctOfOwner + Player.PetHealthTalentBonus));
        Pet = new ActivePet(def, ability, maxHp);
        LastLog = $"✦ {def.Name} summoned ({Pet.MaxHp} HP)";
    }

    /// <summary>Pet greift im eigenen Takt an. Gibt den am Gegner verursachten Schaden zurück.</summary>
    private int UpdatePet(double dt, double enemyDmgTakenMult)
    {
        if (Pet is null) return 0;

        // Beschwörung nicht mehr ausgerüstet (Loadout-/Klassenwechsel, Talent-Reset) → Pet verschwindet
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
            double variance = 0.85 + _rng.NextDouble() * 0.30;   // 85–115 % wie bei Gegnertreffern
            int dmg = Math.Max(1, (int)Math.Round(PetRegistry.AverageHit(def, Player.Level) * variance * enemyDmgTakenMult
                                                  * (1.0 + Player.PetDamageTalentBonus + Player.Buffs.PetDamageBonus)));
            Meter.RecordAbility(Pet.MeterSource, CombatActionKind.Damage, enemy.TakeDamage(dmg));
            CombatTextRaised?.Invoke(new CombatText(CombatTextTarget.Enemy, CombatTextKind.Ability, dmg, false));
            dealt += dmg;
        }
        return dealt;
    }

    /// <summary>
    /// Ein lebendes Pet fängt <see cref="PetRegistry.DamageShare"/> des Gegnertreffers ab.
    /// Gibt den Anteil zurück, den der Spieler noch erleidet.
    /// </summary>
    private int SplitDamageWithPet(int damage)
    {
        if (Pet is not { IsAlive: true }) return damage;
        // Mindestens 1 Schaden, auch wenn 10 % eines kleinen Treffers auf 0 runden würden
        // Soul Link erhöht den Anteil
        int share = Math.Max(1, (int)Math.Round(damage * (PetRegistry.DamageShare + Player.Buffs.PetDamageShareBonus)));

        Pet.TakeDamage(share);
        if (!Pet.IsAlive)
            OnPetDied();
        return Math.Max(1, damage - share);
    }

    /// <summary>Pet gestorben: Beschwörung geht auf Cooldown und wird danach automatisch neu gewirkt.</summary>
    private void OnPetDied()
    {
        var pet = Pet!;
        Player.Cooldowns.StartCooldown(pet.SummonAbility.Id, PetRegistry.DeathCooldown);
        LastLog = $"✖ Your {pet.Definition.Name} has died!";
        Pet = null;
    }

    /// <summary>Entfernt das Pet ohne Cooldown (Spielertod, Charakterwechsel).</summary>
    private void DismissPet() => Pet = null;
}
