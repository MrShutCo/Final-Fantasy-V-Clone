using System;
using Engine;
using Final_Fantasy_V.Models;

namespace Final_Fantasy_V.Algorithms
{
	public class Damage
	{
		static Random random = new Random();

		public Damage()
		{
			
		}

        public static DamageParameters Magic(Unit attacker, Unit defender, Spell s)
        {
            return new DamageParameters(
                s.Attack + random.Next(0, s.Attack / 8),
                (attacker.Level * attacker.MagicPower) / 256 + 4,
                defender.MagicDefense
            );
        }

        public static DamageParameters Flare(Unit attacker, Unit defender, Spell s)
		{
            return new DamageParameters(
                s.Attack + random.Next(0, s.Attack / 32),
                (attacker.Level*attacker.MagicPower) / 256 + 4,
                defender.MagicDefense/32
            );
        }

        public static DamageParameters RandomMagic(Unit attacker, Unit defender, Spell s)
        {
            return new DamageParameters(
                random.Next(50,200),
                s.Attack,
                defender.Defense
            );
        }

        public static DamageParameters PhysicalMagic(Unit attacker, Unit defender, Spell s)
        {
            return new DamageParameters(
                s.Attack + random.Next(0, s.Attack / 8),
                (attacker.Level * attacker.MagicPower) / 256 + 4,
                defender.Defense
            );
        }

        public static DamageParameters Swords(Character character, Unit defender)
        {
            return new DamageParameters(
                character.WeaponAttack() + random.Next(0, character.WeaponAttack() / 8),
                (character.Level * character.Strength) / 128 + 2,
                defender.Defense
            );
        }

        public static DamageParameters Fists(Character character, Unit defender)
        {
            var atk = 3 + random.Next(0, character.Level / 4);
            var m = 2;
            var defense = defender.Defense;

            if (character.HasAbility(Ability.BareFist))
            {
                atk = 3 + character.Level * 2 + (random.Next(0, character.Level * 2 / 8));
                m = (character.Level * character.Strength) / 256 + 2;
                if (character.Accessory == Utility.Accesories["Kaiser Knuckles"])
                    atk += 50;
            }
            return new DamageParameters(atk, m, defense);
        }

        // TODO: needs to incorporate knives bug fix
        public static DamageParameters Knives(Character character, Unit defender)
		{
			return new DamageParameters(
				character.WeaponAttack() + random.Next(0,3),
				(character.Level*character.Strength)/128 + (character.Level * character.Agility) / 128 + 2,
                defender.Defense
            );
		}

        public static DamageParameters Axes(Character character, Unit defender)
        {
            return new DamageParameters(
                (character.WeaponAttack()/2) + random.Next(0, character.WeaponAttack()),
                (character.Level * character.Strength) / 128 + 2,
                defender.Defense/4
            );
        }

        public static DamageParameters Bells(Character character, Unit defender)
        {
            return new DamageParameters(
                character.WeaponAttack()/2 + random.Next(0, character.WeaponAttack() / 2),
                (character.Level * character.Strength) / 128 + (character.Level * character.Agility) / 128 + 2,
                defender.MagicDefense
            );
        }

        public static DamageParameters Rods(Character character, Unit defender)
        {
            return new DamageParameters(
                random.Next(0, character.WeaponAttack())*2,
                (character.Level * character.MagicPower) / 256 + 2,
                defender.MagicDefense
            );
        }

        public static DamageParameters LevelBasedMagic(Character character, Unit defender)
        {
            return new DamageParameters(
                random.Next(10, 100),
                character.Level/8 + 2,
                defender.MagicDefense
            );
        }

        public static DamageParameters Monster(Character character, Enemy enemy)
        {
            return new DamageParameters(
                enemy.Attack + random.Next(0, enemy.Attack/8),
                enemy.AttMultiplier,
                character.Defense
            );
        }

        public static DamageParameters Potion(Spell s)
            => new DamageParameters(s.Attack,1,0);

        public static DamageParameters Throw(Character character, Unit defender, Weapon w)
        {
            return new DamageParameters(
                w.ThrowAttack + random.Next(0, w.ThrowAttack / 8),
                (character.Level * character.Strength) / 128 + (character.Level * character.Agility) / 128 + 2,
                defender.Defense
            );
        }

        public static DamageParameters GilToss(Character character, Unit defender)
            => new DamageParameters(character.Level + 10,150, defender.Defense);

        public static DamageParameters BraveBlade(Character character, Unit defender, int timesEscaped)
        {
            return new DamageParameters(
                Math.Max(character.WeaponAttack() - timesEscaped, 0),
                (character.Level * character.Strength) / 128 + 2,
                defender.Defense
            );
        }

        // TODO: Goblin punch

        public static DamageParameters StrongFight(Character character, Enemy enemy)
        {
            return new DamageParameters(
                enemy.Attack*8 + random.Next(0,enemy.Attack/8),
                enemy.AttMultiplier,
                character.Defense
            );
        }

        public static DamageParameters ChickenKnife(Character character, Unit defender, int timesEscaped)
        {
            return new DamageParameters(
                Math.Min(timesEscaped / 2, 127),
                (character.Level * character.Strength) / 128 + (character.Level * character.Agility) / 128 + 2,
                defender.Defense
            );
        }
    }
}

