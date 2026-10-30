// Models/TalentDefinition.cs
namespace Models;

public class TalentDefinition
{
    public string Id { get; init; } = "";
    public string Name { get; init; } = "";
    public string Description { get; init; } = "";
    public CharacterClass Class { get; init; }
    public int TreeIndex { get; init; }   // 0, 1, 2
    public int Row { get; init; }         // 0–6
    public int Column { get; init; }      // 0–3
    public int MaxRanks { get; init; } = 1;
    public string? RequiresTalentId { get; init; }

    /// <summary>Minimum character level to invest (specialization talents: 20). 0 = row rule only.</summary>
    public int LevelRequired { get; init; }
    public TalentEffectType EffectType { get; init; }
    public double EffectPerRank { get; init; }
    public string IconUrl { get; init; } = "";
    /// <summary>ID of the ability unlocked by this talent (optional).</summary>
    public string? GrantsAbilityId { get; init; }

    /// <summary>Further unlocked abilities (e.g. Bear Form → Maul).</summary>
    public IReadOnlyList<string> AlsoGrantsAbilityIds { get; init; } = [];

    /// <summary>Name of the ability changed by <see cref="AbilityModifier"/> (<see cref="TalentEffectType.ModifyAbility"/>).</summary>
    public string? ModifiedAbilityName { get; init; }

    /// <summary>Applied to the effective copy of the ability while the talent is skilled.</summary>
    public Func<Ability, Ability>? AbilityModifier { get; init; }

    /// <summary>All abilities unlocked by this talent.</summary>
    public IEnumerable<string> GrantedAbilityIds =>
        string.IsNullOrEmpty(GrantsAbilityId)
            ? AlsoGrantsAbilityIds
            : AlsoGrantsAbilityIds.Prepend(GrantsAbilityId);

    /// <summary>Description of the effect for a specific rank.</summary>
    public string EffectDescription(int ranks) => EffectType switch
    {
        TalentEffectType.CritChance =>
            $"+{(EffectPerRank * ranks * 100):0.##}% Critical Strike Chance",
        TalentEffectType.ArcaneIntellectBonus =>
            $"+{(EffectPerRank * ranks * 100):0.##}% Arcane Intellect strength",
        TalentEffectType.ConjureWaterManaRegen =>
            $"+{(EffectPerRank * ranks * 100):0.##}% Conjure Water mana regen",
        TalentEffectType.AutoAttackDamage =>
            $"+{(EffectPerRank * ranks * 100):0.##}% auto-attack damage",
        TalentEffectType.FireballManaCost =>
            $"-{(EffectPerRank * ranks):0.##} Fireball mana cost",
        TalentEffectType.FireballDot =>
            $"Fireball DoT: {(EffectPerRank * ranks * 100):0.##}% of its damage over 5s",
        TalentEffectType.FireballCrit =>
            $"+{(EffectPerRank * ranks * 100):0.##}% Fireball crit chance",
        TalentEffectType.FrostboltManaCost =>
            $"-{(EffectPerRank * ranks):0.##} Frostbolt mana cost",
        TalentEffectType.FrostboltAttackSpeedDebuff =>
            $"+{(EffectPerRank * ranks * 100):0.##}% Frostbolt attack slow",
        TalentEffectType.FrostboltCrit =>
            $"+{(EffectPerRank * ranks * 100):0.##}% Frostbolt crit chance",
        TalentEffectType.AutoAttackSpeed =>
            $"+{(EffectPerRank * ranks * 100):0.##}% attack speed",
        TalentEffectType.AutoAttackCrit =>
            $"+{(EffectPerRank * ranks * 100):0.##}% auto-attack crit chance",
        TalentEffectType.DamageTaken =>
            $"-{(EffectPerRank * ranks * 100):0.##}% damage taken",
        TalentEffectType.HealingBonus =>
            $"+{(EffectPerRank * ranks * 100):0.##}% healing",
        TalentEffectType.RageGenBonus =>
            $"+{(EffectPerRank * ranks * 100):0.##}% rage from auto-attacks",
        TalentEffectType.HeroicStrikeDamage =>
            $"+{(EffectPerRank * ranks * 100):0.##}% Heroic Strike damage",
        TalentEffectType.HeroicStrikeManaCost =>
            $"-{(EffectPerRank * ranks):0.##} Heroic Strike rage cost",
        TalentEffectType.ExecuteCooldown =>
            $"-{(EffectPerRank * ranks):0.##}s Execute cooldown",
        TalentEffectType.ThunderclapBonus =>
            $"+{(EffectPerRank * ranks * 100):0.##}% Thunder Clap damage and slow",
        TalentEffectType.CritHeal =>
            $"Heal {(EffectPerRank * ranks):0.##} HP on crit",
        TalentEffectType.RageOnHit =>
            $"+{(EffectPerRank * ranks):0.##} rage when hit",
        TalentEffectType.GrantBattleStance =>
            "Grants ability: Battle Stance (+10% rage generation from auto-attacks)",
        TalentEffectType.GrantBerserkerStance =>
            "Grants ability: Berserker Stance (+5% damage, +3% crit chance)",
        TalentEffectType.GrantDefensiveStance =>
            "Grants ability: Defensive Stance (-10% damage taken)",
        TalentEffectType.SmiteHeal =>
            $"Heal {(EffectPerRank * ranks):0.##} HP when Smite deals damage",
        TalentEffectType.ShadowWordPainDamage =>
            $"+{(EffectPerRank * ranks * 100):0.##}% Shadow Word: Pain damage",
        TalentEffectType.GrantDivineShield =>
            "Grants ability: Divine Shield (absorbs 25% max HP as a shield)",
        TalentEffectType.SmiteDamage =>
            $"+{(EffectPerRank * ranks * 100):0.##}% Smite damage",
        TalentEffectType.PowerWordFortitudeBonus =>
            $"+{(EffectPerRank * ranks * 100):0.##}% damage reduction from Power Word: Fortitude",
        TalentEffectType.GrantHolyNova =>
            "Grants ability: Holy Nova (damages the enemy and heals you at the same time)",
        TalentEffectType.RenewManaCost =>
            $"-{(EffectPerRank * ranks * 100):0.##}% Renew mana cost",
        TalentEffectType.RenewHealing =>
            $"+{(EffectPerRank * ranks * 100):0.##}% Renew healing",
        TalentEffectType.GrantShadowForm =>
            "Grants ability: Shadowform (+25% Smite, Mind Blast and Shadow Word: Pain damage, -15% damage taken)",
        TalentEffectType.MindBlastManaCost =>
            $"-{(EffectPerRank * ranks * 100):0.##}% Mind Blast mana cost",
        TalentEffectType.DrainManaBonus =>
            $"+{(EffectPerRank * ranks * 100):0.##}% Drain Mana restoration",
        TalentEffectType.SinisterStrikeDamage =>
            $"+{(EffectPerRank * ranks * 100):0.##}% Sinister Strike damage",
        TalentEffectType.DeadlyPoisonProcChance =>
            $"+{(EffectPerRank * ranks * 100):0.##}% Deadly Poison proc chance",
        TalentEffectType.KidneyShotDuration =>
            $"+{(EffectPerRank * ranks * 100):0.##}% Kidney Shot duration",
        TalentEffectType.HolyLightManaCost =>
            $"-{(EffectPerRank * ranks * 100):0.##}% Holy Light mana cost",
        TalentEffectType.HolyStrikeManaCost =>
            $"-{(EffectPerRank * ranks * 100):0.##}% Holy Strike mana cost",
        TalentEffectType.DivineshieldDuration =>
            $"+{(EffectPerRank * ranks):0.##}s Divine Shield duration",
        TalentEffectType.RaptorStrikeManaCost =>
            $"-{(EffectPerRank * ranks):0.##} Raptor Strike mana cost",
        TalentEffectType.SerpentStingManaCost =>
            $"-{(EffectPerRank * ranks):0.##} Serpent Sting mana cost",
        TalentEffectType.HuntersMarkBonus =>
            $"+{(EffectPerRank * ranks * 100):0.##}% Hunter's Mark bonus",
        TalentEffectType.CurseOfAgonyDuration =>
            $"+{(EffectPerRank * ranks):0.##}s Curse of Agony duration",
        TalentEffectType.DrainLifeTickDamage =>
            $"+{(EffectPerRank * ranks):0.##} Drain Life tick damage",
        TalentEffectType.LifeTapHpCostReduction =>
            $"-{(EffectPerRank * ranks):0.##} Life Tap HP cost",
        TalentEffectType.ShadowBoltManaCost =>
            $"-{(EffectPerRank * ranks):0.##} Shadow Bolt mana cost",
        TalentEffectType.LightningBoltManaCost =>
            $"-{(EffectPerRank * ranks):0.##} Lightning Bolt mana cost",
        TalentEffectType.LightningBoltDamage =>
            $"+{(EffectPerRank * ranks):0.##} Lightning Bolt damage",
        TalentEffectType.LightningShieldDamage =>
            $"+{(EffectPerRank * ranks * 100):0.##}% Lightning Shield damage",
        TalentEffectType.GrantChainLightning =>
            "Grants ability: Chain Lightning (hits the enemy 3 times, +20% crit chance)",
        TalentEffectType.FlameShockDamage =>
            $"+{(EffectPerRank * ranks * 100):0.##}% Flame Shock damage",
        TalentEffectType.GrantWindfuryWeapon =>
            "Grants ability: Windfury Weapon (20% chance per auto attack to strike 2 extra times)",
        TalentEffectType.FlurryAttackSpeed =>
            $"+{(EffectPerRank * ranks * 100):0.##}% attack speed for 3 swings after a melee crit",
        TalentEffectType.GrantManaTideTotem =>
            "Grants ability: Mana Tide Totem (restores 24% mana over 12s)",
        TalentEffectType.SpiritManaRegen =>
            $"+{(EffectPerRank * ranks * 100):0.##}% mana regeneration from Spirit",
        TalentEffectType.HealingWaveHealing =>
            $"+{(EffectPerRank * ranks * 100):0.##}% Healing Wave healing",
        TalentEffectType.EarthbindCooldown =>
            $"-{(EffectPerRank * ranks):0.##}s Earthbind Totem cooldown",
        TalentEffectType.WrathDamage =>
            $"+{(EffectPerRank * ranks * 100):0.##}% Wrath and Starfire damage",
        TalentEffectType.GrantMoonkinForm =>
            "Grants ability: Moonkin Form (+15% Wrath, Starfire and Moonfire damage)",
        TalentEffectType.MoonfireDamage =>
            $"+{(EffectPerRank * ranks * 100):0.##}% Moonfire damage",
        TalentEffectType.GrantBearForm =>
            "Grants abilities: Bear Form (uses Rage, -6% damage taken) and Maul",
        TalentEffectType.MaulDamage =>
            $"+{(EffectPerRank * ranks * 100):0.##}% Maul damage",
        TalentEffectType.GrantCatForm =>
            "Grants abilities: Cat Form (uses Energy), Claw and Rip",
        TalentEffectType.ClawDamage =>
            $"+{(EffectPerRank * ranks * 100):0.##}% Claw damage",
        TalentEffectType.GrantTreeForm =>
            "Grants ability: Tree Form (+20% healing, +10% Wrath and Moonfire damage)",
        TalentEffectType.RegrowthHealing =>
            $"+{(EffectPerRank * ranks * 100):0.##}% Regrowth healing",
        TalentEffectType.GrantEvocation =>
            "Grants ability: Evocation (restores 80% mana and +10% damage for 8s)",
        TalentEffectType.ArcaneMissilesDamage =>
            $"+{(EffectPerRank * ranks * 100):0.##}% Arcane Missiles damage",
        TalentEffectType.GrantIgnite =>
            "Grants ability: Ignite (fire crits apply 40% damage as DoT over 3s)",
        TalentEffectType.FireBlastManaCost =>
            $"-{(EffectPerRank * ranks):0.##} Fire Blast mana cost",
        TalentEffectType.GrantIceBarrier =>
            "Grants ability: Ice Barrier (absorbs 30% max HP as a shield)",
        TalentEffectType.FrostboltDamage =>
            $"+{(EffectPerRank * ranks * 100):0.##}% Frostbolt damage",
        TalentEffectType.GrantHolyShock =>
            "Grants ability: Holy Shock (damages the enemy and heals you at the same time)",
        TalentEffectType.HolyLightShield =>
            $"Holy Light shields you for {(EffectPerRank * ranks * 100):0.##}% of your max HP",
        TalentEffectType.GrantHolyShield =>
            "Grants ability: Holy Shield (-35% damage from the next 4 attacks, damages attackers)",
        TalentEffectType.ConsecrationDamage =>
            $"+{(EffectPerRank * ranks * 100):0.##}% Consecration damage",
        TalentEffectType.GrantSealOfCommand =>
            "Grants ability: Seal of Command (+12% auto-attack damage, 25% chance for an extra attack)",
        TalentEffectType.HolyStrikeDamage =>
            $"+{(EffectPerRank * ranks * 100):0.##}% Holy Strike damage",
        TalentEffectType.GrantColdBlood =>
            "Grants ability: Cold Blood (your next damaging ability is a guaranteed critical strike)",
        TalentEffectType.GrantBladeFlurry =>
            "Grants ability: Blade Flurry (auto attacks strike twice for 10s)",
        TalentEffectType.SinisterStrikeEnergyCost =>
            $"-{(EffectPerRank * ranks):0.##} Sinister Strike energy cost",
        TalentEffectType.SliceAndDiceBonus =>
            $"+{(EffectPerRank * ranks * 100):0.##}% Slice and Dice attack speed",
        TalentEffectType.GrantGhostlyStrike =>
            "Grants ability: Ghostly Strike (weapon strike that shields you)",
        TalentEffectType.StaminaHpBonus =>
            $"+{(EffectPerRank * ranks * 100):0.##}% health gained from Stamina",
        TalentEffectType.ExposeArmorBonus =>
            $"+{(EffectPerRank * ranks * 100):0.##}% Expose Armor effect",
        TalentEffectType.GrantTamePet =>
            "Grants abilities: Call Cat (damage pet) and Call Bear (tank pet)",
        TalentEffectType.PetHealthBonus =>
            $"Pet health +{(EffectPerRank * ranks * 100):0.##}% of your max HP",
        TalentEffectType.GrantSniperShot =>
            "Grants ability: Sniper Shot (long cast, very high damage)",
        TalentEffectType.MultiShotDamage =>
            $"+{(EffectPerRank * ranks * 100):0.##}% Multi-Shot damage",
        TalentEffectType.GrantMongooseBite =>
            "Grants ability: Mongoose Bite (strike that empowers your next Raptor Strike by 45%)",
        TalentEffectType.RaptorStrikeBonus =>
            $"+{(EffectPerRank * ranks * 100):0.##}% Raptor Strike damage, -{(EffectPerRank * ranks * 100):0.##}% mana cost",
        TalentEffectType.GrantSiphonLife =>
            "Grants ability: Siphon Life (damage over time that heals you)",
        TalentEffectType.LifestealBonus =>
            $"+{(EffectPerRank * ranks * 100):0.##}% healing from lifesteal",
        TalentEffectType.GrantSummonDemon =>
            "Grants ability: Summon Voidwalker (tank demon)",
        TalentEffectType.GrantIncinerate =>
            "Grants ability: Incinerate (high fire damage, cooldown)",
        TalentEffectType.IncinerateEmpower =>
            $"Shadow Bolt increases the damage of your next Incinerate by {(EffectPerRank * ranks * 100):0.##}%",
        TalentEffectType.FireballDamage =>
            $"+{(EffectPerRank * ranks * 100):0.##}% Fireball damage",
        TalentEffectType.FireBlastDamage =>
            $"+{(EffectPerRank * ranks * 100):0.##}% Fire Blast damage",
        TalentEffectType.MindBlastDamage =>
            $"+{(EffectPerRank * ranks * 100):0.##}% Mind Blast damage",
        TalentEffectType.ThornsDamage =>
            $"+{(EffectPerRank * ranks * 100):0.##}% Thorns damage",
        TalentEffectType.InsectSwarmDamage =>
            $"+{(EffectPerRank * ranks * 100):0.##}% Insect Swarm damage",
        TalentEffectType.DeadlyPoisonStacks =>
            $"+{(EffectPerRank * ranks):0.##} maximum Deadly Poison stacks",
        TalentEffectType.DeadlyPoisonTickRate =>
            $"Deadly Poison ticks {(EffectPerRank * ranks * 100):0.##}% faster",
        TalentEffectType.PetDamageBonus =>
            $"+{(EffectPerRank * ranks * 100):0.##}% pet damage",
        TalentEffectType.DamageBonus =>
            $"+{(EffectPerRank * ranks * 100):0.##}% damage of all abilities",
        TalentEffectType.ShadowBoltDamage =>
            $"+{(EffectPerRank * ranks * 100):0.##}% Shadow Bolt damage",
        TalentEffectType.GrantSpecAbility when GrantsAbilityId is not null
                                            && ClassAbilityRegistry.FindById(GrantsAbilityId) is { } ability =>
            $"Grants ability: {ability.Name} ({AbilityText.ShortSummary(ability)})",
        TalentEffectType.BerserkerFlurry =>
            $"Passive: +{(EffectPerRank * ranks * 100):0.##}% attack speed for 3 swings after a melee crit (Berserker Stance only)",
        TalentEffectType.SpiritOfRedemption =>
            $"Passive: a killing blow turns you into an immune spirit for {IdleGameEngine.SpiritOfRedemptionBuff.BuffDuration:0} sec instead",
        TalentEffectType.Nightfall =>
            $"Passive: {(EffectPerRank * ranks * 100):0.##}% chance per damage over time tick to make your next Shadow Bolt instant",
        TalentEffectType.Eclipse =>
            $"Passive: Wrath empowers your next Starfire by {IdleGameEngine.LunarEclipseBonus * 100:0}%, Starfire your next Wrath by {IdleGameEngine.SolarEclipseBonus * 100:0}%",
        TalentEffectType.SerpentStingDamage =>
            $"+{(EffectPerRank * ranks * 100):0.##}% Serpent Sting damage",
        TalentEffectType.ModifyAbility => Description,
        _ => ""
    };
}
