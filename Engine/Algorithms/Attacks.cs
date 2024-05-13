using System;
using Final_Fantasy_V.Models;
namespace Final_Fantasy_V.Algorithms
{
	public class Attacks
	{
		public Attacks()
		{
		}

		public static int DetermineAttack(Unit attacker, Unit defender, int attackType, Command command, Spell s = null)
		{
			if (attackType == 0x06) return MagicAttack(attacker, defender, s);
			return 0;
		}

		public static int KnivesAttack(Character c, Enemy e, Command command)
		{
			if (!HitDetermination.PhysicalHits(c, e, command)) return 0;
			DamageParameters d = Damage.Knives(c, e);
			d = Modifiers(c, e, command, d);
            return d.CalculateDamage();
		}
		
		
		public static int SwordsAttack(Character c, Enemy e, Command command)
		{
            if (!HitDetermination.PhysicalHits(c, e, command)) return 0;
            DamageParameters d = Damage.Swords(c, e);
            d = Modifiers(c, e, command, d);
            return d.CalculateDamage();
        }

		public static int MagicAttack(Unit attacker, Unit defender, Spell s)
		{
			// Aegis shield check
			DamageParameters d = Damage.Magic(attacker, defender, s);
			//if (s) d.M /= 2;

            return d.CalculateDamage();
        }

		static DamageParameters Modifiers(Unit attacker, Unit defender, Command command, DamageParameters d)
		{
            d = RowModifier(attacker, defender, d);
            d = CommandModifiers(command, d);
            // Apply target status effector modifiers to Defense and M
            d = TargetPhysicalStatusModifiers(defender, d);
			// Apply Magic Sword modifiers
			return d;
        }

        static DamageParameters RowModifier(Unit attacker, Unit defender, DamageParameters d)
		{
			if (attacker.IsBackRow) d.M /= 2;
			if (defender.IsBackRow) d.M /= 2;
			return d;
		}

		static DamageParameters DoubleGrip(Character c, DamageParameters d)
		{
			if (c.HasAbility(Ability.TwoHanded)) d.M *= 2;
			return d;
		}
			

		static DamageParameters CommandModifiers(Command c, DamageParameters d)
		{
			switch (c) {
				case Command.SwordDance:
                    d.Attack *= 2;
                    d.M *= 2;
                    break;
				case Command.Throw:
					d.Attack *= 2;
					break;
				case Command.BuildUp:
					d.M *= 2;
					break;
				case Command.XFight:
					d.M /= 2;
					d.Defense = 0;
					break;
				case Command.Defend:
					d.M /= 2;
					break;
				// The same as setting Damage = 0
				case Command.Guarding:
					d.Attack = 0;
					break;
			}
			return d;
		}

		static DamageParameters TargetPhysicalStatusModifiers(Unit defender, DamageParameters d)
		{
			if (defender.Status.Protect) d.M /= 2;
			if (defender.Status.Toad) d.Defense = 0;
			return d;
		}

        static DamageParameters TargetMagicalStatusModifiers(Unit defender, DamageParameters d)
        {
            if (defender.Status.Shell) d.M /= 2;
            if (defender.Status.Toad) d.Defense = 0;
            return d;
        }
    }
}

