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
	public class ItemMenu : MenuState
    {
        private int _selectedItemIndex;
        
		public ItemMenu(ContentManager cm) : base(cm)
		{
            menuSelectors = new MenuSelector[] {
                new TextSelector(3, 1, 8, 1, 7, 1, new List<string> { "Use", "Sort", "Rare"}),
                new ItemSelector(0,0,0,0,0,0),
                new ItemSelector(0,0,0,0,0,0),
                new TextSelector(1, 4, 3,9,1,5, new List<string>{"","","",""}),
                //new(1, 2, 23, 1, 7, 2, new string[]{ "Job", "Ability" }),
                //new(1, 4, 2, 4, 1, 7, new string[]{"","","","" })
            };
            _selectedItemIndex = 0;
        }

        public override void OnEnter(PartyState ps, object _)
        {
            Menu.SetBox(tileData, 0, 0, 5, 3);
            Menu.SetBox(tileData, 5, 0, 27, 3);
            Menu.SetBox(tileData, 0, 3, 32, 4);
            Menu.SetBox(tileData, 0, 7, 32, 30-7);

            menuSelectors[1] = new ItemSelector(2, ps.Inventory.Count/2, 0, 9, 15, 12);
            menuSelectors[2] = new ItemSelector(2, ps.Inventory.Count/2, 0, 9, 15, 12);
    
            menuSelectors[0].CursorState = ECursor.InActive;
            menuSelectors[1].CursorState = ECursor.Visible;

            
            menuSelectors[1].CursorState = ECursor.Visible;
            menuSelectors[0].CursorState = ECursor.InActive;
            currSelector = 1;
        }

        public override void Render(SpriteBatch spriteBatch, PartyState ps)
        {
            
            base.Render(spriteBatch, ps);
            spriteBatch.Begin();
            
            if (currSelector <= 2) 
            {
                Menu.DrawManyString(spriteBatch, menuSpritesheet, ps.Inventory.Where((i, ind) => ind % 2 == 0).Select(i => i.Name).ToArray(), new Vector2(2*8,9*8), 12);
                Menu.DrawManyString(spriteBatch, menuSpritesheet, ps.Inventory.Where((i, ind) => ind % 2 == 0).Select(i => ":" + Menu.PadNumber(i.NumInInventory, 2)).ToArray(), new Vector2(12*8,9*8), 12);
                Menu.DrawManyString(spriteBatch, menuSpritesheet, ps.Inventory.Where((i, ind) => ind % 2 == 1).Select(i => i.Name).ToArray(), new Vector2(8*17,9*8), 12);
                Menu.DrawManyString(spriteBatch, menuSpritesheet, ps.Inventory.Where((i, ind) => ind % 2 == 1).Select(i => ":" + Menu.PadNumber(i.NumInInventory, 2)).ToArray(), new Vector2(28*8,9*8), 12);
            }

            if (currSelector == 3) // Consumable 
            {
                DrawParty(spriteBatch, ps);
            }
            
            
            RenderCursor(spriteBatch);

        
            
            spriteBatch.End();
        }
        
        void DrawParty(SpriteBatch spriteBatch, PartyState ps)
        {
            for (int i = 0; i < ps.Slots.Length; i++)
            {
                var startingY = 9 + i * 5;
                Menu.DrawString(spriteBatch, menuSpritesheet, PartyState.GetName(ps.Slots[i].Hero), new Vector2(7*8,startingY*8));
                Menu.DrawString(spriteBatch, menuSpritesheet, PartyState.GetJob(ps.Slots[i].Job), new Vector2(7*8,(startingY+2)*8));
                Menu.DrawString(spriteBatch, menuSpritesheet, $"LV  {ps.Slots[i].Level}", new Vector2(24*8,(startingY)*8));

                ps.HeroSprites[i].Draw(spriteBatch, new Vector2(8*3, 8 * (9 + i * 5)));

                string hp = $"{ps.Slots[i].CurrHP}".PadLeft(4);
                string maxhp = $"{ps.Slots[i].MaxHP()}".PadLeft(4);
                Menu.DrawString(spriteBatch, menuSpritesheet, $"HP {hp}/{maxhp}", new Vector2(17*8,(startingY+1)*8));

                string mp = $"{ps.Slots[i].CurrMP}".PadLeft(4);
                string maxmp = $"{ps.Slots[i].MaxMP()}".PadLeft(4);
                Menu.DrawString(spriteBatch, menuSpritesheet, $"MP {mp}/{maxmp}", new Vector2(17*8,(startingY+2)*8));
            }
        }
        
        public override void Update(GameTime gameTime, PartyState ps)
        {
            if (InputHandler.KeyPressed(Keys.Left)) menuSelectors[currSelector].MoveCursorLeft();
            if (InputHandler.KeyPressed(Keys.Right)) menuSelectors[currSelector].MoveCursorRight();

            if (currSelector == 0)
            {
                if (InputHandler.KeyPressed(Keys.Back)) stateStack.Pop();
                if (InputHandler.KeyPressed(Keys.Enter) && getCursorPos().X == 0) ChangeCurrentMenu(1, 0, 0);
                //if (inputHandler.KeyPressed(Keys.Enter) && getCursorPos().X == 2) ChangeCurrentMenu(1, 0, 0, Content.ECursor.InActive);
            }

            else if (currSelector == 1) {

                if (InputHandler.KeyPressed(Keys.Up)) menuSelectors[1].MoveCursorUp();
                if (InputHandler.KeyPressed(Keys.Down)) menuSelectors[1].MoveCursorDown();
                if (InputHandler.KeyPressed(Keys.Back)) ChangeCurrentMenu(0, 0, 0);

                if (InputHandler.KeyPressed(Keys.Enter))
                {
                    menuSelectors[1].CursorState = ECursor.Selected;
                    menuSelectors[2].CursorState = ECursor.Visible;
                    menuSelectors[2].SetCursorTo(menuSelectors[1].GetXyOfCursor());
                    currSelector = 2;
                }
            }

            else if (currSelector == 2)
            {
                if (InputHandler.KeyPressed(Keys.Up)) menuSelectors[2].MoveCursorUp();
                if (InputHandler.KeyPressed(Keys.Down)) menuSelectors[2].MoveCursorDown();
                if (InputHandler.KeyPressed(Keys.Enter))
                {
                    int prevIndex = menuSelectors[1].GetCurrIndex();
                    int currIndex = menuSelectors[2].GetCurrIndex();

                    // We've selected to use the item
                    if (prevIndex == currIndex)
                    {
                        if (ps.Inventory[currIndex] is Consumable c && c.CanDrink())
                        {
                            ChangeCurrentMenu(3, 0,0);
                            menuSelectors[1].CursorState = ECursor.InActive;
                            Menu.SetBox(tileData, 1, 8, 30, 22, 1);
                            tileData.SetLayerVisible(1, true);
                            _selectedItemIndex = currIndex;
                            return;
                        }
                    }
                    
                    (ps.Inventory[prevIndex], ps.Inventory[currIndex]) = (
                        ps.Inventory[currIndex], ps.Inventory[prevIndex]);
                    ((ItemSelector)menuSelectors[1]).SwapItems(prevIndex, currIndex);
                    menuSelectors[1].CursorState = ECursor.Visible;
                    menuSelectors[2].CursorState = ECursor.InActive;
                    menuSelectors[1].SetCursorTo(menuSelectors[2].GetXyOfCursor());
                    currSelector = 1;
                }
            }
            
            else if (currSelector == 3) // Consumable usage
            {
                if (InputHandler.KeyPressed(Keys.Up)) menuSelectors[3].MoveCursorUp();
                if (InputHandler.KeyPressed(Keys.Down)) menuSelectors[3].MoveCursorDown();
                if (InputHandler.KeyPressed(Keys.Back))
                {
                    ChangeCurrentMenu(1, 0, 0);
                    tileData.SetLayerVisible(1, false);
                }

                if (InputHandler.KeyPressed(Keys.Enter))
                {
                    var item = ps.Inventory[_selectedItemIndex];
                }
            }
            
            base.Update(gameTime, ps);
        }
    }
}

