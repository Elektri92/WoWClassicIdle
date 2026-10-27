// Models/TalentEffectType.cs
namespace Models;

public enum TalentEffectType
{
    CritChance,
    ArcaneIntellectBonus,       // +x% Stärke von Arcane Intellect
    ConjureWaterManaRegen,      // +x% Manaregg-Wert von Conjure Water
    AutoAttackDamage,           // +x% Auto-Angriff-Schaden
    FireballManaCost,           // -x Mana Kosten von Fireball (Flat)
    FireballDot,                // Fireball-DoT-Schaden pro Rang
    FireballCrit,               // +x% Crit-Chance für Fireball
    FrostboltManaCost,          // -x Mana Kosten von Frostbolt (Flat)
    FrostboltAttackSpeedDebuff, // +x% Angriffstempo-Debuff von Frostbolt
    FrostboltCrit,              // +x% Crit-Chance für Frostbolt

    // ─── Allgemein ───
    AutoAttackSpeed,             // +x% Angriffsgeschwindigkeit
    AutoAttackCrit,              // +x% Crit-Chance nur für Auto-Attacks
    DamageTaken,                 // -x% erlittener Schaden
    HealingBonus,                // +x% Heilung
    RageGenBonus,                // +x% Rage-Generierung durch Auto-Attacks

    // ─── Warrior ───
    HeroicStrikeDamage,          // +x% Heroic Strike Schaden
    HeroicStrikeManaCost,        // -x Rage-Kosten Heroic Strike (flat)
    ExecuteCooldown,             // -x Sekunden Cooldown Execute
    ThunderclapBonus,            // +x% Thunderclap Schaden und Attack-Slow
    CritHeal,                    // Heilt x HP bei Crit
    RageOnHit,                   // +x Rage wenn Angegriffen (flat)
    GrantBattleStance,           // Schaltet Battle Stance frei
    GrantBerserkerStance,        // Schaltet Berserker Stance frei
    GrantDefensiveStance,        // Schaltet Defensive Stance frei

    // ─── Priest ───
    SmiteHeal,                   // Heilt x HP wenn Smite Schaden verursacht
    ShadowWordPainDamage,        // +x% Shadow Word Pain Schaden
    GrantDivineShield,           // Schaltet Divine Shield (Priester-Schild) frei
    SmiteDamage,                 // +x% Smite Schaden
    PowerWordFortitudeBonus,     // +x Prozentpunkte Schadensreduktion von Power Word: Fortitude
    GrantHolyNova,               // Schaltet Holy Nova frei
    RenewManaCost,               // -x% Manakosten Renew
    RenewHealing,                // +x% Renew Heilung
    GrantShadowForm,             // Schaltet Shadowform frei
    MindBlastManaCost,           // -x% Manakosten Mind Blast
    DrainManaBonus,              // +x% Mana-Wiederherstellung von Drain Mana

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
    HuntersMarkBonus,            // +x% Schadensbuff von Hunter's Mark

    // ─── Warlock ───
    CurseOfAgonyDuration,        // +x Sekunden Dauer Curse of Agony
    DrainLifeTickDamage,         // +x Schaden pro Drain Life Tick
    LifeTapHpCostReduction,      // -x HP-Kosten Life Tap (flat)
    ShadowBoltManaCost,          // -x Manakosten Shadow Bolt (flat)

    // ─── Shaman ───
    LightningBoltManaCost,       // -x Manakosten Lightning Bolt (flat)
    LightningBoltDamage,         // +x Schaden Lightning Bolt (flat)
    LightningShieldDamage,       // +x% Vergeltungsschaden von Lightning Shield
    GrantChainLightning,         // Schaltet Chain Lightning frei
    FlameShockDamage,            // +x% Flame Shock Schaden (Direkt + DoT)
    GrantWindfuryWeapon,         // Schaltet Windfury Weapon frei
    FlurryAttackSpeed,           // +x% Angriffstempo für die nächsten 3 Schläge nach einem Nahkampf-Crit
    GrantManaTideTotem,          // Schaltet Mana Tide Totem frei
    SpiritManaRegen,             // +x% Manaregeneration aus Spirit
    HealingWaveHealing,          // +x% Healing Wave Heilung
    EarthbindCooldown,           // -x Sekunden Cooldown Earthbind Totem

    // ─── Druid ───
    WrathDamage,                 // +x% Wrath- und Starfire-Schaden
    GrantMoonkinForm,            // Schaltet Moonkin Form frei
    MoonfireDamage,              // +x% Moonfire Schaden (Direkt + DoT)
    GrantBearForm,               // Schaltet Bear Form + Maul frei
    MaulDamage,                  // +x% Maul Schaden
    GrantCatForm,                // Schaltet Cat Form + Claw frei
    ClawDamage,                  // +x% Claw Schaden
    GrantTreeForm,               // Schaltet Tree Form frei
    RegrowthHealing,             // +x% Regrowth Heilung (Direkt + HoT)

    // ─── Mage (Reihe 2) ───
    GrantEvocation,             // Schaltet Evocation frei
    ArcaneMissilesDamage,       // +x% Arcane Missiles Schaden
    GrantIgnite,                // Schaltet Ignite frei
    FireBlastManaCost,          // -x Manakosten Fire Blast (Flat)
    GrantIceBarrier,            // Schaltet Ice Barrier frei
    FrostboltDamage,            // +x% Frostbolt Schaden

    // ─── Paladin (Reihe 2) ───
    GrantHolyShock,             // Schaltet Holy Shock frei
    HolyLightShield,            // Holy Light gibt Schild über x% max. HP
    GrantHolyShield,            // Schaltet Holy Shield frei
    ConsecrationDamage,         // +x% Consecration Schaden
    GrantSealOfCommand,         // Schaltet Seal of Command frei
    HolyStrikeDamage,           // +x% Holy Strike Schaden

    // ─── Rogue (Reihe 2) ───
    GrantColdBlood,             // Schaltet Cold Blood frei
    GrantBladeFlurry,           // Schaltet Blade Flurry frei
    SinisterStrikeEnergyCost,   // -x Energiekosten Sinister Strike (flat)
    SliceAndDiceBonus,          // +x Prozentpunkte Angriffstempo von Slice and Dice
    GrantGhostlyStrike,         // Schaltet Ghostly Strike frei
    StaminaHpBonus,             // +x% HP aus Stamina
    ExposeArmorBonus,           // +x% Wirkung von Expose Armor

    // ─── Hunter (Reihe 2) ───
    GrantTamePet,               // Schaltet Cat- und Bear-Pet frei
    PetHealthBonus,             // +x% der Spieler-HP als zusätzliche Pet-HP (auch Warlock)
    GrantSniperShot,            // Schaltet Sniper Shot frei
    MultiShotDamage,            // +x% Multi-Shot Schaden
    GrantMongooseBite,          // Schaltet Mongoose Bite frei
    RaptorStrikeBonus,          // +x% Raptor Strike Schaden und -x% Manakosten

    // ─── Warlock (Reihe 2) ───
    GrantSiphonLife,            // Schaltet Siphon Life frei
    LifestealBonus,             // +x% Heilung aus Lifesteal-Fähigkeiten
    GrantSummonDemon,           // Schaltet Summon Voidwalker frei
    GrantIncinerate,            // Schaltet Incinerate frei
    IncinerateEmpower,          // Shadow Bolt erhöht den Schaden des nächsten Incinerate um x%

    // ─── Balance-Runde Lv 10–20 (ersetzen wirkungslose Mana-Talente) ───
    FireballDamage,             // +x% Fireball Schaden
    FireBlastDamage,            // +x% Fire Blast Schaden
    MindBlastDamage,            // +x% Mind Blast Schaden
    ShadowBoltDamage,           // +x% Shadow Bolt Schaden
    DamageBonus,                // +x% Schaden aller Fähigkeiten (Direkt + DoT)
    ThornsDamage,               // +x% Vergeltungsschaden von Thorns
    InsectSwarmDamage,          // +x% Insect Swarm Schaden
    DeadlyPoisonStacks,         // +x maximale Stacks Deadly Poison
    DeadlyPoisonTickRate,       // Deadly Poison tickt x% schneller
    PetDamageBonus,             // +x% Schaden des Begleittiers

    // ─── Spezialisierung (Reihe 3, ab Lv 20) ───
    GrantSpecAbility,           // Schaltet die Spezialisierungs-Fähigkeit des Baums frei (GrantsAbilityId)
    BerserkerFlurry,            // Flurry wie beim Schamanen (+x% Tempo nach Crit), nur in Berserker Stance
    SpiritOfRedemption,         // Tödlicher Treffer → unverwundbarer Geist (Priester, Holy)
    Nightfall,                  // x% Chance pro DoT-Tick: nächstes Shadow Bolt sofort
    Eclipse,                    // Wrath verstärkt Starfire und umgekehrt
}
