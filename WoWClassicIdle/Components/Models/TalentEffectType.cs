// Models/TalentEffectType.cs
namespace Models;

public enum TalentEffectType
{
    CritChance,
    ArcaneIntellectBonus,       // +x% strength of Arcane Intellect
    ConjureWaterManaRegen,      // +x% mana regen value of Conjure Water
    AutoAttackDamage,           // +x% Auto-Angriff-Schaden
    FireballManaCost,           // -x mana cost of Fireball (flat)
    FireballDot,                // Fireball DoT damage per rank
    FireballCrit,               // +x% crit chance for Fireball
    FrostboltManaCost,          // -x mana cost of Frostbolt (flat)
    FrostboltAttackSpeedDebuff, // +x% attack speed debuff of Frostbolt
    FrostboltCrit,              // +x% crit chance for Frostbolt

    // ─── Allgemein ───
    AutoAttackSpeed,             // +x% Angriffsgeschwindigkeit
    AutoAttackCrit,              // +x% crit chance for auto-attacks only
    DamageTaken,                 // -x% damage taken
    HealingBonus,                // +x% Heilung
    RageGenBonus,                // +x% rage generation from auto-attacks

    // ─── Warrior ───
    HeroicStrikeDamage,          // +x% Heroic Strike Schaden
    HeroicStrikeManaCost,        // -x Rage-Kosten Heroic Strike (flat)
    ExecuteCooldown,             // -x Sekunden Cooldown Execute
    ThunderclapBonus,            // +x% Thunderclap damage and attack slow
    CritHeal,                    // Heals x HP on crit
    RageOnHit,                   // +x rage when attacked (flat)
    GrantBattleStance,           // Unlocks Battle Stance
    GrantBerserkerStance,        // Unlocks Berserker Stance
    GrantDefensiveStance,        // Unlocks Defensive Stance

    // ─── Priest ───
    SmiteHeal,                   // Heals x HP when Smite deals damage
    ShadowWordPainDamage,        // +x% Shadow Word Pain Schaden
    GrantDivineShield,           // Unlocks Divine Shield (Priest shield)
    SmiteDamage,                 // +x% Smite Schaden
    PowerWordFortitudeBonus,     // +x percentage points damage reduction from Power Word: Fortitude
    GrantHolyNova,               // Unlocks Holy Nova
    RenewManaCost,               // -x% Manakosten Renew
    RenewHealing,                // +x% Renew Heilung
    GrantShadowForm,             // Unlocks Shadowform
    MindBlastManaCost,           // -x% Manakosten Mind Blast
    DrainManaBonus,              // +x% mana restoration from Drain Mana

    // ─── Rogue ───
    SinisterStrikeDamage,        // +x% Sinister Strike Schaden
    DeadlyPoisonProcChance,      // +x% Proc-Chance Deadly Poison
    KidneyShotDuration,          // +x% Dauer Kidney Shot

    // ─── Paladin ───
    HolyLightManaCost,           // -x% Manakosten Holy Light
    HolyStrikeManaCost,          // -x% Manakosten Holy Strike
    DivineshieldDuration,        // +x Sekunden Dauer Divine Shield

    // ─── Hunter ───
    RaptorStrikeManaCost,        // -x Manakosten Raptor Strike (flat)
    SerpentStingManaCost,        // -x Manakosten Serpent Sting (flat)
    HuntersMarkBonus,            // +x% damage buff of Hunter's Mark

    // ─── Warlock ───
    CurseOfAgonyDuration,        // +x Sekunden Dauer Curse of Agony
    DrainLifeTickDamage,         // +x damage per Drain Life tick
    LifeTapHpCostReduction,      // -x HP-Kosten Life Tap (flat)
    ShadowBoltManaCost,          // -x Manakosten Shadow Bolt (flat)

    // ─── Shaman ───
    LightningBoltManaCost,       // -x Manakosten Lightning Bolt (flat)
    LightningBoltDamage,         // +x Schaden Lightning Bolt (flat)
    LightningShieldDamage,       // +x% retaliation damage of Lightning Shield
    GrantChainLightning,         // Unlocks Chain Lightning
    FlameShockDamage,            // +x% Flame Shock Schaden (Direkt + DoT)
    GrantWindfuryWeapon,         // Unlocks Windfury Weapon
    FlurryAttackSpeed,           // +x% attack speed for the next 3 swings after a melee crit
    GrantManaTideTotem,          // Unlocks Mana Tide Totem
    SpiritManaRegen,             // +x% mana regeneration from Spirit
    HealingWaveHealing,          // +x% Healing Wave Heilung
    EarthbindCooldown,           // -x Sekunden Cooldown Earthbind Totem

    // ─── Druid ───
    WrathDamage,                 // +x% Wrath and Starfire damage
    GrantMoonkinForm,            // Unlocks Moonkin Form
    MoonfireDamage,              // +x% Moonfire Schaden (Direkt + DoT)
    GrantBearForm,               // Unlocks Bear Form + Maul
    MaulDamage,                  // +x% Maul Schaden
    GrantCatForm,                // Unlocks Cat Form + Claw
    ClawDamage,                  // +x% Claw Schaden
    GrantTreeForm,               // Unlocks Tree Form
    RegrowthHealing,             // +x% Regrowth Heilung (Direkt + HoT)

    // ─── Mage (Reihe 2) ───
    GrantEvocation,             // Unlocks Evocation
    ArcaneMissilesDamage,       // +x% Arcane Missiles Schaden
    GrantIgnite,                // Unlocks Ignite
    FireBlastManaCost,          // -x Manakosten Fire Blast (Flat)
    GrantIceBarrier,            // Unlocks Ice Barrier
    FrostboltDamage,            // +x% Frostbolt Schaden

    // ─── Paladin (Reihe 2) ───
    GrantHolyShock,             // Unlocks Holy Shock
    HolyLightShield,            // Holy Light grants a shield of x% max HP
    GrantHolyShield,            // Unlocks Holy Shield
    ConsecrationDamage,         // +x% Consecration Schaden
    GrantSealOfCommand,         // Schaltet Seal of Command frei
    HolyStrikeDamage,           // +x% Holy Strike Schaden

    // ─── Rogue (Reihe 2) ───
    GrantColdBlood,             // Unlocks Cold Blood
    GrantBladeFlurry,           // Unlocks Blade Flurry
    SinisterStrikeEnergyCost,   // -x Energiekosten Sinister Strike (flat)
    SliceAndDiceBonus,          // +x percentage points attack speed from Slice and Dice
    GrantGhostlyStrike,         // Unlocks Ghostly Strike
    StaminaHpBonus,             // +x% HP from Stamina
    ExposeArmorBonus,           // +x% effect of Expose Armor

    // ─── Hunter (Reihe 2) ───
    GrantTamePet,               // Unlocks Cat and Bear pets
    PetHealthBonus,             // +x% of the player HP as additional pet HP (also Warlock)
    GrantSniperShot,            // Unlocks Sniper Shot
    MultiShotDamage,            // +x% Multi-Shot Schaden
    GrantMongooseBite,          // Unlocks Mongoose Bite
    RaptorStrikeBonus,          // +x% Raptor Strike damage and -x% mana cost

    // ─── Warlock (Reihe 2) ───
    GrantSiphonLife,            // Unlocks Siphon Life
    LifestealBonus,             // +x% healing from lifesteal abilities
    GrantSummonDemon,           // Unlocks Summon Voidwalker
    GrantIncinerate,            // Unlocks Incinerate
    IncinerateEmpower,          // Shadow Bolt increases the damage of the next Incinerate by x%

    // ─── Balancing round Lv 10–20 (replace ineffective mana talents) ───
    FireballDamage,             // +x% Fireball Schaden
    FireBlastDamage,            // +x% Fire Blast Schaden
    MindBlastDamage,            // +x% Mind Blast Schaden
    ShadowBoltDamage,           // +x% Shadow Bolt Schaden
    DamageBonus,                // +x% damage of all abilities (direct + DoT)
    ThornsDamage,               // +x% retaliation damage of Thorns
    InsectSwarmDamage,          // +x% Insect Swarm Schaden
    DeadlyPoisonStacks,         // +x max stacks of Deadly Poison
    DeadlyPoisonTickRate,       // Deadly Poison ticks x% faster
    PetDamageBonus,             // +x% pet damage

    // ─── Spezialisierung (Reihe 3, ab Lv 20) ───
    GrantSpecAbility,           // Unlocks the tree's specialization ability (GrantsAbilityId)
    BerserkerFlurry,            // Flurry like the Shaman's (+x% haste after crit), only in Berserker Stance
    SpiritOfRedemption,         // Lethal hit → invulnerable spirit (Priest, Holy)
    Nightfall,                  // x% chance per DoT tick: next Shadow Bolt instant
    Eclipse,                    // Wrath empowers Starfire and vice versa
    SerpentStingDamage,         // +x% Serpent Sting damage
    ModifyAbility,              // Changes ModifiedAbilityName via AbilityModifier (Lv 11–15 spec abilities)
}
