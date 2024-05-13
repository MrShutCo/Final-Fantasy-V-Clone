using System.Collections.Generic;
using System.Linq;
using Final_Fantasy_V.Models;
using FinalFantasyV.Content;
using FinalFantasyV.Sprites;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace FinalFantasyV.GameStates.Menus
{
	public class EquipmentMenu : MenuState
	{
        int equipTypeChosen;
        List<Item> possibleItems;

        public EquipmentMenu(ContentManager cm) : base(cm)
        {
            menuSelectors = new MenuSelector[] {
                new TextSelector(5, 1, 1, 1, 6, 1, new List<string> { "Eqp.", "Optimum", " Rmv", "Empty", " End" }),
                new ItemSelector(1, 5, 8, 3, 7, 1),
                new ItemSelector(1, 12, 2, 15, 12, 1)
            };
            possibleItems = new();
        }


        public override void OnEnter(PartyState ps)
        {
            Menu.SetBox(tileData, 0, 0, 32, 13);
            Menu.SetBox(tileData, 0, 0, 32, 3);
            Menu.SetBox(tileData, 1, 13, 14, 16);
            Menu.SetBox(tileData, 15, 13, 16, 16);

            currSelector = 0;
            menuSelectors[0].CursorState = ECursor.Visible;
            menuSelectors[1] = new ItemSelector(1, 5, 6, 4, 7, 14);
            menuSelectors[2] = new ItemSelector(1, 12, 0, 14, 12, 14);
        }

        public override void Render(SpriteBatch spriteBatch, PartyState ps)
        {
            base.Render(spriteBatch, ps);
            spriteBatch.Begin();
            var h = ps.Slots[slotIndex];

            Menu.DrawManyString(spriteBatch, menuSpritesheet, new []{ h.RightHand?.Name, h.LeftHand?.Name, h.Head?.Name, h.Body?.Name, h.Accessory?.Name }, new Vector2(8*8, 8*4), 14);
            
            if (currSelector == 2)
                DrawStats(spriteBatch, h);

            Menu.DrawManyString(spriteBatch, menuSpritesheet, new[] { "R.Hand", "L.Hand","Head", "Body", "Relic" }, new Vector2(8 * 1, 8 * 4), 14);
            Menu.DrawManyString(spriteBatch, menuSpritesheet, new []{"Strgth.", "Agility", "Vitality", "Mag.Pwr", "Attack", "Defense", "Evade %", "Mag.Def", "EqpWgt"}, new Vector2(8*16,8*14-4),14);
            
            Menu.DrawManyString(spriteBatch, menuSpritesheet, 
                new []{h.Strength, h.Agility, h.Vitality, h.MagicPower, h.Attack, h.Defense, h.Evade, h.MagicDefense, h.Weight}.Select(s => Menu.PadNumber(s, 3)).ToArray(),
                new Vector2(8 * 23, 8 * 14 - 4), 14);

            Menu.DrawManyString(spriteBatch, menuSpritesheet, new[] { PartyState.GetName(ps.Slots[slotIndex].Hero), PartyState.GetJob(ps.Slots[slotIndex].Job) }, new Vector2(22*8, 3*8+4), 12);
            ps.HeroSprites[slotIndex].Draw(spriteBatch, new Vector2(8*19+4,8*3));

            RenderCursor(spriteBatch);

            spriteBatch.End();
        }

        void DrawStats(SpriteBatch spriteBatch, Character h)
        {
            Menu.DrawManyString(spriteBatch, menuSpritesheet, possibleItems.Select(i => i.Name).ToArray(), new Vector2(8*2, 8*14), 14);
            Menu.DrawManyString(spriteBatch, menuSpritesheet, possibleItems.Select(i => ":" + Menu.PadNumber(i.NumInInventory, 2)).ToArray(), new Vector2(8*11, 8*14), 14);

            // TODO: make it show the new stats with new equipment, as well as show yellow or red text based on if its better or not
            
            int newAttack = 0;
            

            if (equipTypeChosen == 0)
                newAttack = h.LeftHand.Attack + ((Weapon)possibleItems[getCursorY()]).Attack;
                
            if (equipTypeChosen == 1)
                newAttack = h.RightHand.Attack + ((Weapon)possibleItems[getCursorY()]).Attack;
                    
            Menu.DrawManyString(spriteBatch, menuSpritesheet, 
                new []{h.Strength, h.Agility, h.Vitality, h.MagicPower, newAttack, h.Defense, h.Evade, h.MagicDefense, h.Weight}.Select(s => ":" + Menu.PadNumber(s, 3)).ToArray(),
                new Vector2(8 * 26, 8 * 14 - 4), 14);
        }

        public override void Update(GameTime gameTime, PartyState ps)
        {
            if (currSelector == 0)
            {
                if (InputHandler.KeyPressed(Keys.Back)) stateStack.Pop();
                if (InputHandler.KeyPressed(Keys.Enter))
                {
                    var x = getCursorX();
                    if (x == 0) ChangeCurrentMenu(1, 0, 0);
                    if (x == 2)
                    {
                        ChangeCurrentMenu(1, 0, 0);
                        // TODO: add remove flag to change behaviour
                    }

                    if (x == 3)
                    {
                        UnEquipItem(ps, 0);
                        UnEquipItem(ps, 1);
                        UnEquipItem(ps, 2);
                        UnEquipItem(ps, 3);
                        UnEquipItem(ps, 4);
                    }

                    if (x == 4) stateStack.Pop();
                    // TODO: hide first box
                }
                if (InputHandler.KeyPressed(Keys.Right)) menuSelectors[currSelector].MoveCursorRight();
                if (InputHandler.KeyPressed(Keys.Left)) menuSelectors[currSelector].MoveCursorLeft();
            }

            else if (currSelector == 1)
            {
                if (InputHandler.KeyPressed(Keys.Back))
                    ChangeCurrentMenu(0, 0, 0);
                if (InputHandler.KeyPressed(Keys.Up))
                {
                    if (getCursorPos().Y == 0) menuSelectors[currSelector].SetCursorTo(0, 4);
                    else menuSelectors[currSelector].MoveCursorUp();
                }
                if (InputHandler.KeyPressed(Keys.Down))
                {
                    if (getCursorPos().Y == 4) menuSelectors[currSelector].SetCursorTo(0, 0);
                    else menuSelectors[currSelector].MoveCursorDown();
                }
                if (InputHandler.KeyPressed(Keys.Enter))
                {
                    equipTypeChosen = getCursorY();
                    ChangeCurrentMenu(2, 0, 0, ECursor.Visible);
                    if (equipTypeChosen < 2) possibleItems = ps.Inventory.Where(i => i is Weapon).ToList();
                    if (equipTypeChosen == 2) possibleItems = ps.Inventory.Where(i => i.Type == EItemType.Head).ToList();
                    if (equipTypeChosen == 3) possibleItems = ps.Inventory.Where(i => i.Type == EItemType.Body).ToList();
                    if (equipTypeChosen == 4) possibleItems = ps.Inventory.Where(i => i.Type == EItemType.Accesory).ToList();
                    
                }

            }

            else if (currSelector == 2)
            {
                if (InputHandler.KeyPressed(Keys.Up))
                {
                    if (getCursorPos().Y == 0) menuSelectors[currSelector].SetCursorTo(0, 10);
                    else menuSelectors[currSelector].MoveCursorUp();
                }
                if (InputHandler.KeyPressed(Keys.Down))
                {
                    if (getCursorPos().Y == 10) menuSelectors[currSelector].SetCursorTo(0, 0);
                    else menuSelectors[currSelector].MoveCursorDown();
                }
                if (InputHandler.KeyPressed(Keys.Left))
                    menuSelectors[currSelector].MoveCursorLeft();
                if (InputHandler.KeyPressed(Keys.Right))
                    menuSelectors[currSelector].MoveCursorLeft();
                if (InputHandler.KeyPressed(Keys.Back))
                    ChangeCurrentMenu(1, 0, equipTypeChosen);
                if (InputHandler.KeyPressed(Keys.Enter))
                {
                    // Equip the item if it isnt null
                    if (getCursorY() < possibleItems.Count)
                    {
                        UnEquipItem(ps, equipTypeChosen);
                        EquipItem(ps);
                    }
                    ChangeCurrentMenu(1, 0, equipTypeChosen);
                }
            }

            base.Update(gameTime, ps);
        }

        void UnEquipItem(PartyState ps, int index)
        {
            if (index == 0)
            {
                ps.AddItem(ps.Slots[slotIndex].RightHand);
                ps.Slots[slotIndex].RightHand = null;
            }
            if (index == 1)
            {
                ps.AddItem(ps.Slots[slotIndex].LeftHand);
                ps.Slots[slotIndex].LeftHand = null;
            }
            if (index == 2)
            {
                ps.AddItem(ps.Slots[slotIndex].Head);
                ps.Slots[slotIndex].Head = null;
            }
            if (index == 3)
            {
                ps.AddItem(ps.Slots[slotIndex].Body);
                ps.Slots[slotIndex].Body = null;
            }
            if (index == 4)
            {
                ps.AddItem(ps.Slots[slotIndex].Accessory);
                ps.Slots[slotIndex].Accessory = null;
            }
        }

        void EquipItem(PartyState ps)
        {
            if (equipTypeChosen == 0)
            {
                ps.Slots[slotIndex].RightHand = (Weapon)possibleItems[getCursorY()];
                ps.RemoveItem(ps.Slots[slotIndex].RightHand);
            }

            if (equipTypeChosen == 1)
            {
                ps.Slots[slotIndex].LeftHand = (Weapon)possibleItems[getCursorY()];
                ps.RemoveItem(ps.Slots[slotIndex].LeftHand);
            }

            if (equipTypeChosen == 2)
            {
                ps.Slots[slotIndex].Head = (Gear)possibleItems[getCursorY()];
                ps.RemoveItem(ps.Slots[slotIndex].Head);
            }

            if (equipTypeChosen == 3)
            {
                ps.Slots[slotIndex].Body = (Gear)possibleItems[getCursorY()];
                ps.RemoveItem(ps.Slots[slotIndex].Body);
            }

            if (equipTypeChosen == 4)
            {
                ps.Slots[slotIndex].Accessory = (Gear)possibleItems[getCursorY()];
                ps.RemoveItem(ps.Slots[slotIndex].Accessory);                
            }
        }
    }
}

