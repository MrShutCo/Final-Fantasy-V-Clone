using System;
using System.Text.Json.Serialization;

namespace Final_Fantasy_V.Models
{
    public enum EItemType
    {
        Consume,
        Sword,
        Knife,
        Shield = 8,
        Head = 4,
        Body = 2,
        Accesory = 1
    }

    public class Item
	{
        public string Name { get; set; } = "";
        public string Description { get; init; }
        public int Price { get; init; }
        public int Sell { get; init; }
        public EItemType Type { get; init; }
        //public int MaxInInventory { get; init; }
        public int NumInInventory { get; set; }

        public Item()
		{
            //MaxInInventory = 99;
            NumInInventory = 1;
		}
	}

    public class Consumable : Item
    {
	    public int Target { get; init; }
	    public byte Restrictions { get; init; }
	    public byte Properties { get; init; }
	    public byte DamageFormula { get; init; }

	    public bool CanMix() => (Restrictions & 0x02) == 0;
	    public bool CanDrink() => (Restrictions & 0x20) == 0;
	    public bool CanUseInBattle() => (Restrictions & 0x40) == 0;
	    public bool CanThrow() => (Restrictions & 0x80) == 0;

	    public byte GetDescription() => (byte)(Properties & 0x20);

	    public void UseItem(Unit[] units, int index)
	    {
		    NumInInventory--;
		    if ((Target & 0x20) == 1)
		    {
			    
		    }
	    }

	    void ApplyItem(Unit u, byte action)
	    {
		    if (action == 0x01)
		    {
			   
		    }
	    }
    }

	public class Gear : Item
	{
        public int Defense { get; init; }
        public int Evade { get; init; }
        public int Weight { get; init; }
        public int MagicDefense { get; init; }
        public int MagicEvade { get; init; }
    }

	public class Accessory : Gear
	{
        
    }

	public class Body : Gear
	{
    }

	public class Head : Gear
	{
    }

	public class Weapon : Item
	{

		public int Attack { get; init; }
        public int DamageFormula { get; init; }
        public int Hit { get; init; }
        public int ThrowAttack { get; init; }
        public int Critical { get; init; }
        public bool Throwable { get; init; }
        public string AttackCategory { get; init; }
        public string[] StrongVS { get; init; }
        public string[] MagicElementUp { get; init; }
        public string[] Special { get; init; }
        public string[] Spell { get; init; }
        public string[] StatusAdded { get; init; }
        public string UsedAsItem { get; init; }
        public string[] JobEquippable { get; init; }

        /*[JsonConstructor]
        public Weapon(int attack, int damageFormula, int hit, int throwAttack, int critical, bool throwable)
        {
            Attack = attack;
            DamageFormula = damageFormula;
            Hit = hit;
            ThrowAttack = throwAttack;
            Critical = critical;
            Throwable = throwable;
        }*/
    }
}

