using System.Collections.Generic;
using Engine.RomReader;
using Final_Fantasy_V.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace FinalFantasyV.GameStates
{
	public class PartyState
	{
        public Character[] Slots;
        public SpriteSheet[] HeroSprites;
        public int Gil;
        public List<Item> Inventory { get; set; }

        public PartyState(ContentManager cm)
		{
            Slots =
            [
                new (Hero.Lenna),
                new (Hero.Butz),
                new (Hero.Galuf),
                new (Hero.Faris)
            ];
            HeroSprites =
            [
                new (cm.Load<Texture2D>("Lenna"), 30, 30, Vector2.Zero, Vector2.Zero),
                new (cm.Load<Texture2D>("Bartz"), 30, 30, Vector2.Zero, Vector2.Zero),
                new (cm.Load<Texture2D>("Galuf"), 30, 30, Vector2.Zero, Vector2.Zero),
                new (cm.Load<Texture2D>("Faris"), 30, 30, Vector2.Zero, Vector2.Zero)
            ];
            Slots[0].LeftHand = RomData.GetWeaponByName("[Swrd]Broad");
            Gil = 500;
            Slots[0].Job = EJob.Bard;
            Slots[1].Job = EJob.Knight;
            Slots[2].Job = EJob.Berserker;
            Slots[3].Job = EJob.Geomancer;
            Inventory = new List<Item>
            {
                
                /*RomData.GetWeaponByName("[Swrd]Broad"),
                RomData.GetWeaponByName("[Knif]Dagger"),
                RomData.GetWeaponByName("[Knif]Kunai"),
                RomData.GetWeaponByName("[Knif]Knife"),
                RomData.GetGearByName("[Helm]Plumed"),
                RomData.GetGearByName("[Helm]Bronze"),
                RomData.GetGearByName("[Armr]Bronze"),
                RomData.GetGearByName("[Suit]Leather"),
                RomData.GetGearByName("[Shoe]Red"),
                RomData.GetGearByName("[Ring]Wall"),*/
            };
            
            //AddItem(RomData.GetConsumableByName("Potion"), 10);
            //AddItem(RomData.GetConsumableByName("HiPotion"), 10);
        }

        public void Swap(int i, int j)
        {
            (Slots[i], Slots[j]) = (Slots[j], Slots[i]);
            (HeroSprites[i], HeroSprites[j]) = (HeroSprites[j], HeroSprites[i]);
        }

        public void AddItem(Item item, int amount = 1)
        {
            if (item == null) return;
            foreach (var i in Inventory)
            {
                if (i.Name == item.Name)
                {
                    i.NumInInventory += amount;
                    return;
                }
            }

            item.NumInInventory = amount;
            Inventory.Add(item);
        }

        public void RemoveItem(Item item)
        {
            foreach (var i in Inventory)
            {
                if (i.Name == item.Name)
                {
                    i.NumInInventory--;
                    if (i.NumInInventory == 0)
                    {
                        Inventory.Remove(item);
                        return;
                    }
                }
            }

        }
        
        public static string GetName(Hero hero)
        {
            if (hero == Hero.Butz) return "Bartz";
            if (hero == Hero.Lenna) return "Lenna";
            if (hero == Hero.Galuf) return "Galuf";
            if (hero == Hero.Faris) return "Faris";
            return "";
        }

        public static string GetJob(EJob job)
        {
            if (job == EJob.Freelancer) return "Bare";
            return "";
        }

        public void UseItem(Consumable c, int index)
        {
            c.UseItem(Slots, index);
            
        }
    }
}

