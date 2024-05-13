using System;
using Final_Fantasy_V.Models;

namespace Final_Fantasy_V.Algorithms
{
	public class HitDetermination
	{
		public HitDetermination()
		{
		}

		// TODO: this needs to work for monsters as well
		public static bool PhysicalHits(Unit attacker, Unit defender, Command c)
		{
			// TODO: implement image decrease
			if (defender.Status.Sleep || defender.Status.Paralyze || defender.Status.Charm)
			{

			}
			if (c == Command.Aim || c == Command.Jump || c == Command.Throw || c == Command.SwordDance || c == Command.XFight)
			{

			}
			// TODO: implement step 3-6 of the algorithm



			return true;
		}
	}
}

