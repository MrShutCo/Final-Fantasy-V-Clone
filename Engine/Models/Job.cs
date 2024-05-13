using System;
namespace Final_Fantasy_V.Models
{
    public enum EJob
    {
        Knight = 0,
        Monk,
        Thief,
        Dragoon,
        Ninja,
        Samurai,
        Berserker,
        Ranger,
        MysticKnight,
        WhiteMage,
        BlackMage,
        TimeMage,
        Summoner,
        BlueMage,
        RedMage,
        Beastmaster,
        Chemist,
        Geomancer,
        Bard,
        Dancer,
        Mime,
        Freelancer,
    }

	public class Job
	{
		public string Name { get; set; }
		public  int Strength { get; set; }
        public  int Agility { get; set; }
        public  int Vitality { get; set; }
        public  int Magic { get; set; }
        public Job()
		{
		}

        public Job(int strength, int agility, int vitality, int magic)
        {
            Strength = strength;
            Agility = agility;
            Vitality = vitality;
            Magic = magic;
        }
    }
}

