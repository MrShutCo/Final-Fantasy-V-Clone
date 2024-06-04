using System;
using Engine;
namespace Final_Fantasy_V.Models
{
    public enum Hero
    {
        Butz,
        Lenna,
        Galuf,
        Faris,
        Cara
    }

	public class Character : Unit
	{
        public int Strength { get { return CurrentJob.Strength + HeroStrength();  }}
        public int Agility { get { return CurrentJob.Agility + HeroAgility(); }}
        public int Vitality { get { return CurrentJob.Vitality + HeroVitality(); }}
        public new int MagicPower { get { return CurrentJob.Magic + HeroMagic(); } }
        public new int Attack { get { return WeaponAttack(); }}
        public new int Defense { get { return Head.Defense + Body.Defense + Accessory.Defense; }}
        public int Evasion { get; private set; }
        public int Weight { get; private set; }
        public int MagicMultiplier { get; private set; }
        public int Exp { get; private set; }


        public Weapon RightHand;
        public Weapon LeftHand;
        public Gear Head;
        public Gear Body;
        public Gear Accessory;

        public readonly Hero Hero;
        public Job CurrentJob;
        public EJob Job;
        public Ability[] Abilities;

        public Character(Hero hero)
		{
            Hero = hero;
            CurrentJob = Utility.Jobs["Normal"];
            Status = new();
            Level = 1;
            RightHand = Utility.Weapons["Empty"];
            LeftHand = Utility.Weapons["Empty"];
            Head = Utility.HeadGear["None"];
            Body = Utility.BodyWear["None"];
            Accessory = Utility.Accesories["None"];
            Abilities = [];
            Job = EJob.Freelancer;
            CurrHP = MaxHP();
            CurrMP = MaxMP();
		}

        public int MaxHP() => Utility.GetLevel(Level).Hp * (Vitality + 32) / 32;
        public int MaxMP() => Utility.GetLevel(Level).Mp * (MagicPower + 32) / 32;
        public int WeaponAttack() => RightHand.Attack + LeftHand.Attack;
        public bool HasAbility(Ability ability) => Abilities.Contains(ability);

        int HeroStrength()
        {
            if (Hero == Hero.Butz) return 4;
            if (Hero == Hero.Lenna) return 1;
            if (Hero == Hero.Galuf) return 3;
            if (Hero == Hero.Faris) return 3;
            if (Hero == Hero.Cara) return 1;
            return 0;
        }
        int HeroAgility()
        {
            if (Hero == Hero.Butz) return 1;
            if (Hero == Hero.Lenna) return 2;
            if (Hero == Hero.Galuf) return 0;
            if (Hero == Hero.Faris) return 3;
            if (Hero == Hero.Cara) return 4;
            return 0;
        }
        int HeroVitality()
        {
            if (Hero == Hero.Butz) return 3;
            if (Hero == Hero.Lenna) return 1;
            if (Hero == Hero.Galuf) return 4;
            if (Hero == Hero.Faris) return 2;
            if (Hero == Hero.Cara) return 0;
            return 0;
        }
        int HeroMagic()
        {
            if (Hero == Hero.Butz) return 1;
            if (Hero == Hero.Lenna) return 4;
            if (Hero == Hero.Galuf) return 0;
            if (Hero == Hero.Faris) return 2;
            if (Hero == Hero.Cara) return 3;
            return 0;
        }
    }
}

