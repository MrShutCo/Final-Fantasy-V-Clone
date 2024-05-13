using System;
namespace Final_Fantasy_V.Models
{
	public class DamageParameters
	{
		public int Attack;
		public int M;
		public int Defense;

        public int CalculateDamage() => Math.Min((Attack - Defense) * M, 9999);

        public DamageParameters(int attack, int m, int defense)
        {
            Attack = attack;
            M = m;
            Defense = defense;
        }
    }
}

