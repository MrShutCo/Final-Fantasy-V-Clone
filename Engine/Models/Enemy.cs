using System;
namespace Final_Fantasy_V.Models
{
	public class Enemy : Unit
	{
		public string Name { get; set; }
        public int HP { get; init; }
        public int MP { get; init; }
        public int AttMultiplier { get; init; }
        public int Exp { get; init; }
        public int Gil { get; init; }
        public int Speed { get; init; }

        public Enemy()
		{
			Status = new();
            CurrHP = HP;
            CurrMP = MP;
		}
	}
}

