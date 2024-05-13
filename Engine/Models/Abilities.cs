using System;
namespace Final_Fantasy_V.Models
{
	public enum Ability
	{
		// Knight
		Cover,
		Guard,
		TwoHanded,
		EquipArmor,
		EquipShield,
		EquipSword,

		// Monk
		Store,
		BareFist,
		Chakra,
		Counter,
		HP10,
		HP20,
		HP30,

		// Blue Mage
		Check,
		Learning,
		Blue,
		View,

		// Thief
		Secret,
		Flee,
		Dash,
		Steal,
		Caution,
		Mug,
		Footwork,

		// Black Mage
		Black1, Black2, Black3, Black4, Black5, MP30,

		// White Mage
		White1, White2, White3, White4, White5, MP10,

		// Beserker
		Beserk, EquipAxe,

		// Sorcerer
		MagicWall, Sword1, Sword2, Sword3, Sword4, Sword5, Sword6,

		// Time Mage
		Time1, Time2, Time3, Time4, Time5, Time6, EquipRod,

		// Summoner
		Summon1, Summon2, Summon3, Summon4, Summon5, Call,

		// Red Mage
		Red1, Red2, Red3, XMagic,

		// Mimic
		Mimic,

		// Trainer
		Tame, Control, EquipWhip, Catch,

		// Geomancer
		Earth, FindHole, AntiTrap,

		// Ninja
		Smoke, Twin, FirstStrike, Throw, TwoSwords,

		// Bard
		Hide, EquipHarps, Sing,

		// Hunter
		Animals, Aim, EquipBow, XAttack,

		// Samurai TODO huh
		SSlap, GilToss, SwordGrab, EquipKatana, FDraw,

		// Lancer/Dragoon
		Jump, Lance, EquipLance,

		// Dancer
		Flirt, Dance, EquipRibbon,

		// Chemist
		Medicine, Mix, Drink, Recover, Revive
	};
}

