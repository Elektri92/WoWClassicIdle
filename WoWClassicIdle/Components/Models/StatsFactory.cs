// Models/StatsFactory.cs
namespace Models
{
    public static class StatsFactory
    {
          public static Stats GetBaseStats(CharacterClass characterClass, Race race, int level = 1, WeaponProfile? weaponOverride = null)
          {
              var stats = characterClass switch
              {
                  CharacterClass.Warrior  => new Stats { MaxHp = 120, MaxMana = 100, Attack = 6, Defense = 0, Speed = 3.6 },
                  CharacterClass.Mage     => new Stats { MaxHp = 80,  MaxMana = 450, Attack = 5, Defense = 0, Speed = 3.4 },
                  CharacterClass.Priest   => new Stats { MaxHp = 90,  MaxMana = 450, Attack = 4, Defense = 0, Speed = 3.4 },
                  CharacterClass.Rogue    => new Stats { MaxHp = 100, MaxMana = 100, Attack = 5, Defense = 0, Speed = 3.8 },
                  CharacterClass.Paladin  => new Stats { MaxHp = 110, MaxMana = 450, Attack = 5, Defense = 0, Speed = 3.6 },
                  CharacterClass.Hunter   => new Stats { MaxHp = 100, MaxMana = 450, Attack = 5, Defense = 0, Speed = 3.6 },
                  CharacterClass.Warlock  => new Stats { MaxHp = 85,  MaxMana = 450, Attack = 5, Defense = 0, Speed = 3.4 },
                  CharacterClass.Shaman   => new Stats { MaxHp = 105, MaxMana = 450, Attack = 5, Defense = 0, Speed = 3.6 },
                  CharacterClass.Druid    => new Stats { MaxHp = 100, MaxMana = 450, Attack = 4, Defense = 0, Speed = 3.6 },
                  _ => new Stats { MaxHp = 100, Attack = 5, Defense = 3, Speed = 3.6 }
            };

              var primary = ClassStatRegistry.GetPrimaryStats(characterClass, level);
              var weapon  = weaponOverride ?? StarterWeapons.For(characterClass);

              return stats with
              {
                  Level              = level,
                  Speed              = weapon.Speed,
                  WeaponMinDamage    = weapon.MinDamage,
                  WeaponMaxDamage    = weapon.MaxDamage,
                  AttackPower        = StatScaling.AttackPower(characterClass, level, primary),
                  Primary            = primary,
                  MaxHp              = stats.MaxHp   + StatScaling.BonusHp(primary),
                  MaxMana            = stats.MaxMana + StatScaling.BonusMana(characterClass, primary),
                  ManaRegenPer5      = StatScaling.ManaRegenPer5(characterClass, primary),
                  HpRegenPer5        = StatScaling.HpRegenPer5(characterClass, primary),
                    MeleeDamageMultiplier = StatScaling.MeleeDamageMultiplier(characterClass),
                    AutoAttackDamageMultiplier = StatScaling.AutoAttackDamageMultiplier(characterClass),
                  CritChance         = StatScaling.CritChance(characterClass),
                  AttackSpeedBonus   = StatScaling.AttackSpeedBonus(characterClass),
              };
        }
    }
}
