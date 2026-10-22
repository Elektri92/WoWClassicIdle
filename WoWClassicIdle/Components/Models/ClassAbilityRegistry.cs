// Models/ClassAbilityRegistry.cs
namespace Models;

/// <summary>
/// Rank-1-Werte aus WoW Classic (1.12).
/// Schaden/Heilung = Durchschnitt der Tooltip-Spanne, Kosten = Mana/Rage/Energy.
/// Cooldown: echter CD falls vorhanden, sonst Cast-Zeit bzw. GCD (Idle-Taktung).
/// </summary>
public static class ClassAbilityRegistry
{
    public const int MaxSlotsPerClass = 6;

    public static IReadOnlyDictionary<CharacterClass, IReadOnlyList<Ability>> Abilities { get; }
        = new Dictionary<CharacterClass, IReadOnlyList<Ability>>
    {
        [CharacterClass.Warrior] = new[]
        {
            new Ability
            {
                Id = "Heroic_Strike_r1",
                Name = "Heroic Strike",
                IconUrl = "https://wow.zamimg.com/images/wow/icons/large/ability_rogue_ambush.jpg",
                Cooldown = 0,       // Classic: kein CD – ersetzt die nächste Auto-Attack
                ManaCost = 15,      // 15 Rage
                Damage = 10,        // +10 Waffenschaden (-10% nerf)
                AddsWeaponDamage = true,
                ReplacesAutoAttack = true,
                MinResourceToCast = 60, // erst ab 60 Rage, damit Rage für andere Fähigkeiten bleibt
                LevelRequired = 1,
                Rank = 1,
            },
            new Ability
            {
                Id = "Heroic_Strike_r2",
                Name = "Heroic Strike",
                IconUrl = "https://wow.zamimg.com/images/wow/icons/large/ability_rogue_ambush.jpg",
                Cooldown = 0,
                ManaCost = 15,
                Damage = 11,        // +10 % gegenüber Rank 1
                AddsWeaponDamage = true,
                ReplacesAutoAttack = true,
                MinResourceToCast = 60,
                LevelRequired = 10,
                Rank = 2,
            },
            new Ability
            {
                Id = "Battle_Shout_r1",
                Name = "Battle Shout",
                IconUrl = "https://wow.zamimg.com/images/wow/icons/large/ability_warrior_battleshout.jpg",
                Cooldown = 0,
                ManaCost = 10,          // 10 Rage
                BuffDuration = 30,      // 30 s
                BuffDamageBonus = 0.05, // +5 % verursachter Schaden
                LevelRequired = 2,
                Rank = 1,
            },
            new Ability
            {
                Id = "Thunderclap_r1",
                Name = "Thunder Clap",
                IconUrl = "https://wow.zamimg.com/images/wow/icons/large/spell_nature_thunderclap.jpg",
                Cooldown = 30,
                ManaCost = 20,                      // 20 Rage
                DebuffDuration = 10,
                DebuffAttackSpeedReduction = 0.20,  // −20 % Angriffstempo
                LevelRequired = 4,
                Rank = 1,
            },
            new Ability
            {
                Id = "Execute_r1",
                Name = "Execute",
                IconUrl = "https://wow.zamimg.com/images/wow/icons/large/inv_sword_48.jpg",
                Cooldown = 120,
                ManaCost = 20,                          // 20 Rage
                Damage = 50,
                RequiresEnemyBelowHpPct = 0.20,         // nur unter 20 % Gegner-HP
                LevelRequired = 8,
                Rank = 1,
            },
        },

        [CharacterClass.Mage] = new[]
        {
            new Ability
            {
                Id = "Fireball_r1",
                Name = "Fireball",
                IconUrl = "https://wow.zamimg.com/images/wow/icons/large/spell_fire_flamebolt.jpg",
                Cooldown = 1.5,
                ManaCost = 18,
                Damage = 14,        // -20% nerf (was 18)
                LevelRequired = 1,
                Rank = 1,
                ExclusiveGroup = "mage_bolt",
            },
            new Ability
            {
                Id = "Fireball_r2",
                Name = "Fireball",
                IconUrl = "https://wow.zamimg.com/images/wow/icons/large/spell_fire_flamebolt.jpg",
                Cooldown = 1.5,
                ManaCost = 18,
                Damage = 15,        // +10 % gegenüber Rank 1
                LevelRequired = 10,
                Rank = 2,
                ExclusiveGroup = "mage_bolt",
            },
            new Ability
            {
                Id = "Frostbolt_r1",
                Name = "Frostbolt",
                IconUrl = "https://wow.zamimg.com/images/wow/icons/large/spell_frost_frostbolt02.jpg",
                Cooldown = 1.5,
                ManaCost = 18,
                Damage = 12,
                DebuffDuration = 5,
                DebuffAttackSpeedReduction = 0.05,  // −5 % Angriffstempo
                LevelRequired = 1,
                Rank = 1,
                ExclusiveGroup = "mage_bolt",
            },
            new Ability
            {
                Id = "Frostbolt_r2",
                Name = "Frostbolt",
                IconUrl = "https://wow.zamimg.com/images/wow/icons/large/spell_frost_frostbolt02.jpg",
                Cooldown = 1.5,
                ManaCost = 18,
                Damage = 13,        // +10 % gegenüber Rank 1
                DebuffDuration = 5,
                DebuffAttackSpeedReduction = 0.05,
                LevelRequired = 10,
                Rank = 2,
                ExclusiveGroup = "mage_bolt",
            },
            new Ability
            {
                Id = "Arcane_Intellect_r1",
                Name = "Arcane Intellect",
                IconUrl = "https://wow.zamimg.com/images/wow/icons/large/spell_holy_magicalsentry.jpg",
                Cooldown = 0,
                ManaCost = 0,
                BuffDuration = 60,          // 60 s in idle context
                BuffIntellectBonus = 30,    // +30 Int → +450 max mana (+100% buff)
                BuffSpiritBonus = 30,       // +30 Spirit (+100% buff)
                LevelRequired = 2,
                Rank = 1,
            },
            new Ability
            {
                Id = "Conjure_Water_r1",
                Name = "Conjure Water",
                IconUrl = "https://wow.zamimg.com/images/wow/icons/large/inv_drink_10.jpg",
                Cooldown = 60,
                ManaCost = 0,
                BuffDuration = 20,              // regeneriert über 20 s
                MotManaRestorePct = 0.14,       // 12 % maximales Mana
                MotTickInterval = 5.0,          // 4 Ticks à 2,5 %
                LevelRequired = 3,
                Rank = 1,
            },
            new Ability
            {
                Id = "Conjure_Water_r2",
                Name = "Conjure Water",
                IconUrl = "https://wow.zamimg.com/images/wow/icons/large/inv_drink_10.jpg",
                Cooldown = 50,                  // 10 s weniger CD gegenüber Rank 1
                ManaCost = 0,
                BuffDuration = 20,
                MotManaRestorePct = 0.14,
                MotTickInterval = 5.0,
                LevelRequired = 11,
                Rank = 2,
            },
            new Ability
            {
                Id = "Conjure_Food_r1",
                Name = "Conjure Food",
                IconUrl = "https://wow.zamimg.com/images/wow/icons/large/inv_misc_food_30.jpg",
                Cooldown = 60,
                ManaCost = 10,
                BuffDuration = 20,              // HoT über 20 s
                HotHealPct = 0.24,              // 24 % max. HP gesamt
                HotTickInterval = 5.0,          // 4 Ticks à 6 %
                LevelRequired = 8,
                Rank = 1,
            },
            new Ability
            {
                Id = "Conjure_Food_r2",
                Name = "Conjure Food",
                IconUrl = "https://wow.zamimg.com/images/wow/icons/large/inv_misc_food_30.jpg",
                Cooldown = 50,                  // 10 s weniger CD gegenüber Rank 1
                ManaCost = 10,
                BuffDuration = 20,
                HotHealPct = 0.24,
                HotTickInterval = 5.0,
                LevelRequired = 11,
                Rank = 2,
            },
        },

        [CharacterClass.Priest] = new[]
        {
            new Ability
            {
                Id = "Smite_r1",
                Name = "Smite",
                IconUrl = "https://wow.zamimg.com/images/wow/icons/large/spell_holy_holysmite.jpg",
                Cooldown = 1.5,
                ManaCost = 18,      // -10% from 20
                Damage = 13,        // -3 (was 16)
                LevelRequired = 1,
                Rank = 1,
            },
            new Ability
            {
                Id = "Smite_r2",
                Name = "Smite",
                IconUrl = "https://wow.zamimg.com/images/wow/icons/large/spell_holy_holysmite.jpg",
                Cooldown = 1.5,
                ManaCost = 18,
                Damage = 14,        // +10 % gegenüber Rank 1
                LevelRequired = 10,
                Rank = 2,
            },
            new Ability
            {
                Id = "Renew_r1",
                Name = "Renew",
                IconUrl = "https://wow.zamimg.com/images/wow/icons/large/spell_holy_renew.jpg",
                Cooldown = 60,
                ManaCost = 30,
                BuffDuration = 20,      // HoT über 20 s
                HotTotalHeal = 40,      // 40 Heilung gesamt
                HotTickInterval = 4,    // 5 Ticks à 8
                LevelRequired = 3,
                Rank = 1,
            },
            new Ability
            {
                Id = "Shadow_Word_Pain_r1",
                Name = "Shadow Word: Pain",
                IconUrl = "https://wow.zamimg.com/images/wow/icons/large/spell_shadow_shadowwordpain.jpg",
                Cooldown = 60,
                ManaCost = 20,
                DebuffDuration = 30,        // DoT über 30 s
                DebuffDotDamage = 6,        // 6 Ticks à 6 = 36 DMG
                DebuffDotTickInterval = 5,
                LevelRequired = 5,
                Rank = 1,
            },
            new Ability
            {
                Id = "Drain_Mana_r1",
                Name = "Drain Mana",
                IconUrl = "https://wow.zamimg.com/images/wow/icons/large/spell_shadow_siphonmana.jpg",
                Cooldown = 30,
                ManaCost = 0,
                BuffDuration = 20,              // MoT über 20 s
                MotManaRestorePct = 0.08,       // 8 % maximales Mana
                MotTickInterval = 5.0,          // 4 Ticks à 2 %
                LevelRequired = 7,
                Rank = 1,
            },
        },

        [CharacterClass.Rogue] = new[]
        {
            new Ability
            {
                Id = "Sinester_Strike_r1",
                Name = "Sinister Strike",
                IconUrl = "https://wow.zamimg.com/images/wow/icons/large/spell_shadow_ritualofsacrifice.jpg",
                Cooldown = 1,
                ManaCost = 45,      // 45 Energy
                Damage = 3,
                AddsWeaponDamage = true,
                LevelRequired = 1,
                Rank = 1,
            },
            new Ability
            {
                Id = "Sinister_Strike_r2",
                Name = "Sinister Strike",
                IconUrl = "https://wow.zamimg.com/images/wow/icons/large/spell_shadow_ritualofsacrifice.jpg",
                Cooldown = 1,
                ManaCost = 45,
                Damage = 4,         // +10 % gegenüber Rank 1 (3.3 → 4)
                AddsWeaponDamage = true,
                LevelRequired = 10,
                Rank = 2,
            },
            new Ability
            {
                Id = "Slice_and_Dice_r1",
                Name = "Slice and Dice",
                IconUrl = "https://wow.zamimg.com/images/wow/icons/large/ability_rogue_slicedice.jpg",
                Cooldown = 0,
                ManaCost = 5,               // 5 Energy
                BuffDuration = 10,          // 10 s
                BuffAttackSpeedBonus = 0.1, // +10% attack speed
                LevelRequired = 3,
                Rank = 1,
            },
            new Ability
            {
                Id = "Deadly_Poison_r1",
                Name = "Deadly Poison",
                IconUrl = "https://wow.zamimg.com/images/wow/icons/large/ability_poisons.jpg",
                Cooldown = 0,
                ManaCost = 0,
                IsPassive = true,           // kein aktiver Cast – Proc bei jedem Angriff
                ProcChance = 0.15,          // 15 % Proc-Chance pro Schlag (x3 buff)
                DebuffDuration = 15,        // Debuff-Laufzeit pro Stack-Anwendung
                DebuffDotDamage = 1,        // 1 DMG / 3 s pro Stack
                DebuffDotTickInterval = 3,
                DebuffMaxStacks = 5,        // bis zu 5 Stacks
                LevelRequired = 5,
                Rank = 1,
            },
            new Ability
            {
                Id = "Kidney_Shot_r1",
                Name = "Kidney Shot",
                IconUrl = "https://wow.zamimg.com/images/wow/icons/large/ability_rogue_kidneyshot.jpg",
                Cooldown = 30,
                ManaCost = 0,
                DebuffDuration = 6,         // Betäubung für 6 s
                DebuffIsStun = true,
                LevelRequired = 7,
                Rank = 1,
            },
        },

        [CharacterClass.Paladin] = new[]
        {
            new Ability
            {
                Id = "Judgement_r1",
                Name = "Judgement",
                IconUrl = "https://wow.zamimg.com/images/wow/icons/large/spell_holy_righteousfury.jpg",
                Cooldown = 8,
                ManaCost = 20,
                Damage = 19,        // +1 (was 18)
                LevelRequired = 1,
                Rank = 1,
            },
            new Ability
            {
                Id = "Judgement_r2",
                Name = "Judgement",
                IconUrl = "https://wow.zamimg.com/images/wow/icons/large/spell_holy_righteousfury.jpg",
                Cooldown = 8,
                ManaCost = 20,
                Damage = 21,        // +10 % gegenüber Rank 1 (20.9 → 21)
                LevelRequired = 10,
                Rank = 2,
            },
            new Ability
            {
                Id = "Holy_Light_r1",
                Name = "Holy Light",
                IconUrl = "https://wow.zamimg.com/images/wow/icons/large/spell_holy_holybolt.jpg",
                Cooldown = 10,
                ManaCost = 35,
                Heal = 19,          // -5% from 20
                LevelRequired = 3,
                Rank = 1,
            },
            new Ability
            {
                Id = "Holy_Strike_r1",
                Name = "Holy Strike",
                IconUrl = "https://wow.zamimg.com/images/wow/icons/large/spell_holy_excorcism_02.jpg",
                Cooldown = 10,
                ManaCost = 15,
                Damage = 16,                // Waffenschaden + 16
                AddsWeaponDamage = true,
                ReplacesAutoAttack = true,
                LevelRequired = 4,
                Rank = 1,
            },
            new Ability
            {
                Id = "Divine_Shield_r1",
                Name = "Divine Shield",
                IconUrl = "https://wow.zamimg.com/images/wow/icons/large/spell_holy_divineshield.jpg",
                Cooldown = 60,
                ManaCost = 0,
                BuffDuration = 6,           // 6 s Immunität
                BuffGrantsImmunity = true,
                LevelRequired = 7,
                Rank = 1,
            },
        },

        [CharacterClass.Hunter] = new[]
        {
            new Ability
            {
                Id = "Raptor_Strike_r1",
                Name = "Raptor Strike",
                IconUrl = "https://wow.zamimg.com/images/wow/icons/large/ability_meleedamage.jpg",
                Cooldown = 6,
                ManaCost = 15,
                Damage = 5,
                AddsWeaponDamage = true,
                LevelRequired = 1,
                Rank = 1,
            },
            new Ability
            {
                Id = "Raptor_Strike_r2",
                Name = "Raptor Strike",
                IconUrl = "https://wow.zamimg.com/images/wow/icons/large/ability_meleedamage.jpg",
                Cooldown = 6,
                ManaCost = 15,
                Damage = 6,         // +10 % gegenüber Rank 1 (5.5 → 6)
                AddsWeaponDamage = true,
                LevelRequired = 10,
                Rank = 2,
            },
            new Ability
            {
                Id = "Hunters_Mark_r1",
                Name = "Hunter's Mark",
                IconUrl = "https://wow.zamimg.com/images/wow/icons/large/ability_hunter_snipershot.jpg",
                Cooldown = 20,
                ManaCost = 0,
                DebuffDuration = 10,
                DebuffDamageTakenBonus = 0.05, // +5% damage taken
                LevelRequired = 3,
                Rank = 1,
            },
            new Ability
            {
                Id = "Serpent_Sting_r1",
                Name = "Serpent Sting",
                IconUrl = "https://wow.zamimg.com/images/wow/icons/large/ability_poisonsting.jpg",
                Cooldown = 20,
                ManaCost = 15,
                DebuffDuration = 10,        // DoT über 10 s
                DebuffDotDamage = 10,       // 2 Ticks à 10 = 20 DMG
                DebuffDotTickInterval = 5,
                LevelRequired = 4,
                Rank = 1,
            },
            new Ability
            {
                Id = "Arcane_Shot_r1",
                Name = "Arcane Shot",
                IconUrl = "https://wow.zamimg.com/images/wow/icons/large/ability_impalingbolt.jpg",
                Cooldown = 6,
                ManaCost = 10,
                Damage = 10,
                LevelRequired = 7,
                Rank = 1,
            },
        },

        [CharacterClass.Warlock] = new[]
        {
            new Ability
            {
                Id = "Shdow_bolt_r1",
                Name = "Shadow Bolt",
                IconUrl = "https://wow.zamimg.com/images/wow/icons/large/spell_shadow_shadowbolt.jpg",
                Cooldown = 1.7,
                ManaCost = 25,
                Damage = 15,
                LevelRequired = 1,
                Rank = 1,
            },
            new Ability
            {
                Id = "Shadow_Bolt_r2",
                Name = "Shadow Bolt",
                IconUrl = "https://wow.zamimg.com/images/wow/icons/large/spell_shadow_shadowbolt.jpg",
                Cooldown = 1.7,
                ManaCost = 25,
                Damage = 17,        // +10 % gegenüber Rank 1 (16.5 → 17)
                LevelRequired = 10,
                Rank = 2,
            },
            new Ability
            {
                Id = "Curse_of_Agony_r1",
                Name = "Curse of Agony",
                IconUrl = "https://wow.zamimg.com/images/wow/icons/large/spell_shadow_curseofsargeras.jpg",
                Cooldown = 0,
                ManaCost = 10,
                DebuffDuration = 15,
                DebuffDotDamage = 4,        // -10% nerf (was 5, 4.5 → 4 rounded)
                DebuffDotTickInterval = 3,
                LevelRequired = 3,
                Rank = 1,
            },
            new Ability
            {
                Id = "Drain_Life_r1",
                Name = "Drain Life",
                IconUrl = "https://wow.zamimg.com/images/wow/icons/large/spell_shadow_lifedrain02.jpg",
                Cooldown = 60,
                ManaCost = 0,
                DebuffDuration = 12,        // DoT über 12 s
                DebuffDotDamage = 5,        // 4 Ticks à 5 = 20 DMG
                DebuffDotTickInterval = 3,
                DebuffDotLifesteal = 1.0,   // 100 % des Schadens heilt den Spieler
                LevelRequired = 5,
                Rank = 1,
            },
            new Ability
            {
                Id = "Drain_Life_r2",
                Name = "Drain Life",
                IconUrl = "https://wow.zamimg.com/images/wow/icons/large/spell_shadow_lifedrain02.jpg",
                Cooldown = 60,
                ManaCost = 0,
                DebuffDuration = 12,
                DebuffDotDamage = 10,       // doppelter Schaden gegenüber Rank 1
                DebuffDotTickInterval = 3,
                DebuffDotLifesteal = 1.0,
                LevelRequired = 11,
                Rank = 2,
            },
            new Ability
            {
                Id = "Life_Tap_r1",
                Name = "Life Tap",
                IconUrl = "https://wow.zamimg.com/images/wow/icons/large/spell_shadow_burningspirit.jpg",
                Cooldown = 30,
                ManaCost = 0,
                LifeTapHpCost = 10,         // kostet 10 HP
                LifeTapManaGain = 15,        // gibt 15 Mana
                LevelRequired = 7,
                Rank = 1,
            },
        },

        [CharacterClass.Shaman] = new[]
        {
            new Ability
            {
                Id = "Lightning_bolt_r1",
                Name = "Lightning Bolt",
                IconUrl = "https://wow.zamimg.com/images/wow/icons/large/spell_nature_lightning.jpg",
                Cooldown = 1.5,
                ManaCost = 15,
                Damage = 10,        // -1 (was 11)
                LevelRequired = 1,
                Rank = 1,
            },
            new Ability
            {
                Id = "Lightning_Bolt_r2",
                Name = "Lightning Bolt",
                IconUrl = "https://wow.zamimg.com/images/wow/icons/large/spell_nature_lightning.jpg",
                Cooldown = 1.5,
                ManaCost = 15,
                Damage = 11,        // +10 % gegenüber Rank 1
                LevelRequired = 10,
                Rank = 2,
            },
            new Ability
            {
                Id = "Rockbiter_Weapon_r1",
                Name = "Rockbiter Weapon",
                IconUrl = "https://wow.zamimg.com/images/wow/icons/large/spell_nature_rockbiter.jpg",
                Cooldown = 0,
                ManaCost = 10,
                BuffDuration = 60,
                BuffWeaponDamageBonus = 0.08, // +8% auto-attack damage
                LevelRequired = 2,
                Rank = 1,
            },
            new Ability
            {
                Id = "Earthbind_Totem_r1",
                Name = "Earthbind Totem",
                IconUrl = "https://wow.zamimg.com/images/wow/icons/large/spell_nature_earthbindtotem.jpg",
                Cooldown = 30,
                ManaCost = 5,
                DebuffDuration = 10,
                DebuffAttackSpeedReduction = 0.20,  // −20 % Angriffstempo
                LevelRequired = 4,
                Rank = 1,
            },
            new Ability
            {
                Id = "Lightning_Shield_r1",
                Name = "Lightning Shield",
                IconUrl = "https://wow.zamimg.com/images/wow/icons/large/spell_nature_lightningshield.jpg",
                Cooldown = 0,
                ManaCost = 10,
                BuffDuration = 30,              // Buff läuft 30 s
                BuffRetaliationDamage = 4,      // 4 DMG wenn der Spieler getroffen wird
                LevelRequired = 7,
                Rank = 1,
            },
        },

        [CharacterClass.Druid] = new[]
        {
            new Ability
            {
                Id = "Wrath_r1",
                Name = "Wrath",
                IconUrl = "https://wow.zamimg.com/images/wow/icons/large/spell_nature_abolishmagic.jpg",
                Cooldown = 1.5,
                ManaCost = 20,
                Damage = 14,
                LevelRequired = 1,
                Rank = 1,
            },
            new Ability
            {
                Id = "Wrath_r2",
                Name = "Wrath",
                IconUrl = "https://wow.zamimg.com/images/wow/icons/large/spell_nature_abolishmagic.jpg",
                Cooldown = 1.5,
                ManaCost = 20,
                Damage = 15,        // +10 % gegenüber Rank 1 (15.4 → 15)
                LevelRequired = 10,
                Rank = 2,
            },
            new Ability
            {
                Id = "Moonfire_r1",
                Name = "Moonfire",
                IconUrl = "https://wow.zamimg.com/images/wow/icons/large/spell_nature_starfall.jpg",
                Cooldown = 20,
                ManaCost = 5,
                Damage = 5,
                DebuffDuration = 20,
                DebuffDotDamage = 5,
                DebuffDotTickInterval = 5,
                LevelRequired = 3,
                Rank = 1,
            },
            new Ability
            {
                Id = "Rejuvenation_r1",
                Name = "Rejuvenation",
                IconUrl = "https://wow.zamimg.com/images/wow/icons/large/spell_nature_rejuvenation.jpg",
                Cooldown = 10,
                ManaCost = 35,
                BuffDuration = 10,          // HoT über 10 s
                HotTotalHeal = 20,          // 5 Ticks à 4 HP = 20 HP
                HotTickInterval = 2,
                LevelRequired = 5,
                Rank = 1,
            },
            new Ability
            {
                Id = "Thorns_r1",
                Name = "Thorns",
                IconUrl = "https://wow.zamimg.com/images/wow/icons/large/spell_nature_thorns.jpg",
                Cooldown = 0,
                ManaCost = 10,
                BuffDuration = 30,              // Buff läuft 30 s
                BuffRetaliationDamage = 5,      // 5 DMG wenn der Spieler getroffen wird
                LevelRequired = 7,
                Rank = 1,
            },
        }
    };

    public static IReadOnlyList<Ability> GetAbilitiesFor(CharacterClass cls)
        => Abilities.TryGetValue(cls, out var list) ? list : Array.Empty<Ability>();

    /// <summary>
    /// Wählt den optimalen Standard-Loadout für eine Klasse auf einem bestimmten Level:
    /// pro Ability-Familie (gleicher Name) nur der höchste verfügbare Rank,
    /// bei mehr Familien als Slots werden die zuletzt freigeschalteten bevorzugt.
    /// </summary>
    public static List<string> GetDefaultLoadoutIds(CharacterClass cls, int level) =>
        GetAbilitiesFor(cls)
            .Where(a => a.LevelRequired <= level)
            .GroupBy(a => a.Name)
            .Select(g => g.OrderByDescending(a => a.Rank).First())
            // Pro ExclusiveGroup nur die zuletzt freigeschaltete Ability behalten
            .GroupBy(a => string.IsNullOrEmpty(a.ExclusiveGroup) ? a.Id : a.ExclusiveGroup)
            .Select(g => g.OrderByDescending(a => a.LevelRequired).First())
            .OrderByDescending(a => a.LevelRequired)
            .Take(MaxSlotsPerClass)
            .Select(a => a.Id)
            .ToList();

    public static Ability? FindById(string id)
    {
        foreach (var list in Abilities.Values)
            if (list.Any(a => a.Id == id))
                return list.First(a => a.Id == id);
        return null;
    }

    /// <summary>Sucht eine Fähigkeit der Klasse per Anzeigename (z. B. für Damage-Meter-Zeilen).</summary>
    public static Ability? FindByName(CharacterClass cls, string name)
        => GetAbilitiesFor(cls).FirstOrDefault(a => a.Name == name);

    /// <summary>Klasse, zu der die Fähigkeit gehört.</summary>
    public static CharacterClass? ClassOf(Ability ability)
    {
        foreach (var (cls, list) in Abilities)
            if (list.Any(a => a.Id == ability.Id))
                return cls;
        return null;
    }
}
