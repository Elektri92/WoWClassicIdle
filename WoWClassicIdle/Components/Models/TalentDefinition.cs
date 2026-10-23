// Models/TalentDefinition.cs
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
    RockbiterDmgBonus,           // +x% Autoattack-Schadensbonus von Rockbiter Weapon
    EarthbindCooldown,           // -x Sekunden Cooldown Earthbind Totem

    // ─── Druid ───
    WrathManaCost,               // -x Manakosten Wrath (flat)
}

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
    public TalentEffectType EffectType { get; init; }
    public double EffectPerRank { get; init; }
    public string IconUrl { get; init; } = "";
    /// <summary>ID der Fähigkeit die durch dieses Talent freigeschaltet wird (optional).</summary>
    public string? GrantsAbilityId { get; init; }

    /// <summary>Beschreibung des Effekts für einen bestimmten Rang.</summary>
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
            $"Fireball DoT: {(EffectPerRank * ranks):0.##} DMG over 5s",
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
        TalentEffectType.RockbiterDmgBonus =>
            $"+{(EffectPerRank * ranks * 100):0.##}% Rockbiter Weapon damage bonus",
        TalentEffectType.EarthbindCooldown =>
            $"-{(EffectPerRank * ranks):0.##}s Earthbind Totem cooldown",
        TalentEffectType.WrathManaCost =>
            $"-{(EffectPerRank * ranks):0.##} Wrath mana cost",
        _ => ""
    };
}
