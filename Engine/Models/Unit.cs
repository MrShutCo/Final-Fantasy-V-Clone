using System;
namespace Final_Fantasy_V.Models
{
	public class Unit
	{
		public int Level { get; set; }
		public int Evade { get; set; }
        public int MagicDefense { get; set; }
        public int MagicEvade { get; set; }
        public int MagicMult { get; set; }
        public int Attack { get; set; }
        public int Defense { get; set; }
        public int MagicPower { get; set; }

        public bool IsBackRow { get; set; }
        public int CurrHP { get; set; }
        public int CurrMP { get; set; }
        public StatusEffects Status;

		public Unit()
		{

		}
	}
}

