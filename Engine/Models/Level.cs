using System;
namespace Final_Fantasy_V.Models
{
	public class Level
	{
		public int Exp { get; set; }
		public int Hp { get; set; }
        public int Mp { get; set; }
        public Level(int exp, int hp, int mp)
		{
			Exp = exp;
			Hp = hp;
			Mp = mp;
		}
	}
}

