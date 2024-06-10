using System;
using System.Collections.Generic;
using FinalFantasyV.Content;
using FinalFantasyV.Sprites;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace FinalFantasyV.GameStates.Menus
{
	public class CharacterMenu : MenuState
	{
        const int UnitMenu = 0;
        const int LeftMenu = 2;

        int _previousActiveSelector;
        string _possibleNewState;
        Vector2 _previousActiveItem;

		public CharacterMenu(ContentManager cm) : base(cm)
		{
            //menuSelectors = new Content.MenuSelector[] { new(
            //   new Vector2[] { new(8 * 22, 8 * 7), new(8 * 22, 8 * 9), new(8 * 22, 8 * 11), new(8 * 22, 8 * 13), new(8 * 22, 8 * 15), new(8 * 22, 8 * 17), new(8 * 21, 8 * 2), new(8 * 21, 8 * 4), },
            //   new Action[] { () => Console.WriteLine("Item"), () => Console.WriteLine("Magic") }
            //)};
            var pp = new List<Vector2>()
            {
                new(8 * 21, 8 * 1), new(8 * 21, 8 * 3), new(8 * 22, 8 * 7), new(8 * 22, 8 * 9), new(8 * 22, 8 * 11),
                new(8 * 22, 8 * 13), new(8 * 22, 8 * 15), new(8 * 22, 8 * 17)
            };
            
            menuSelectors = new MenuSelector[] {
                new CustomSelector(pp, new List<int>{1,1,1,1,1,1,1,1}),
                //new TextSelector(1, 6, 24, 7, 6, 2, new List<string> { "Item", "Magic", "Equip", "Status", "Config", "Save" }),
                //new TextSelector(1, 2, 23, 1, 7, 2, new List<string> { "Job", "Ability" }),
                new TextSelector(1, 4, 2, 4, 1, 7, new List<string> {"","","","" }),
                new TextSelector(1, 4, 2, 4, 1, 7, new List<string> {"","","","" }) // Meant for swapping characters around
            };
            menuSelectors[0].CursorState = ECursor.Visible;

            inputHandler = new();
            inputHandler.RegisterKey(Keys.Up, () => Console.WriteLine());
            _possibleNewState = "";
        }

		public override void OnEnter(PartyState ps, object? _)
		{
            currSelector = 0;
            menuSelectors[currSelector].SetCursorTo(0, 0);
            menuSelectors[0].CursorState = ECursor.Visible;
            menuSelectors[1].CursorState = ECursor.InActive;
            menuSelectors[2].CursorState = ECursor.InActive;
            Menu.SetBox(tileData, 0, 0, 32, 30);
            Menu.SetBox(tileData, 22, 25, 10, 5);
            Menu.SetBox(tileData, 23, 20, 9, 5);
            Menu.SetBox(tileData, 23, 5, 9, 15);
            Menu.SetBox(tileData, 22, 0, 10, 6);
            Menu.DrawText(tileData, 23, 26, Menu.PadNumber(ps.Gil, 8));
            Menu.DrawText(tileData, 28, 28, "Gil");
            Menu.DrawText(tileData, 24, 21, "TIME");

            UpdateParty(ps);
        }

        void UpdateParty(PartyState ps)
        {
            for (int i = 0; i < ps.Slots.Length; i++)
            {
                var startingY = 2 + i * 7;
                menu.SetString(tileData, 0, $"{PartyState.GetName(ps.Slots[i].Hero)}  {PartyState.GetJob(ps.Slots[i].Job)}", 1, startingY);
                menu.SetString(tileData, 0, $"LV  {ps.Slots[i].Level}", 2, startingY + 1);


                string hp = $"{ps.Slots[i].CurrHP}".PadLeft(4);
                string maxhp = $"{ps.Slots[i].MaxHP()}".PadLeft(4);
                menu.SetString(tileData, 0, $"HP {hp}/{maxhp}", 10, startingY + 3);

                string mp = $"{ps.Slots[i].CurrMP}".PadLeft(4);
                string maxmp = $"{ps.Slots[i].MaxMP()}".PadLeft(4);
                menu.SetString(tileData, 0, $"MP {mp}/{maxmp}", 10, startingY + 4);
            }
        }

        public override void Render(SpriteBatch spriteBatch, PartyState ps)
        {

            base.Render(spriteBatch, ps);
            spriteBatch.Begin();
            for (int i = 0; i < ps.Slots.Length; i++)
            {
                int isBackRow = ps.Slots[i].IsBackRow ? 3 : 0;
                ps.HeroSprites[i].Draw(spriteBatch, new Vector2(8 * (2 + isBackRow), 8 * (4 + i * 7)));
            }
            
            Menu.DrawManyString(spriteBatch, menuSpritesheet, new []{"Job", "Ability"}, new Vector2(8*23, 8*1), 16);
            Menu.DrawManyString(spriteBatch, menuSpritesheet, new []{"Item", "Magic", "Equip", "Status", "Config", "Save"}, new Vector2(8*24, 8*7), 16);
            
            RenderCursor(spriteBatch);
            
            spriteBatch.End();

        }

        public override void Update(GameTime gameTime, PartyState ps)
        {
            base.Update(gameTime, ps);


            if (currSelector == UnitMenu)
            {

                // Up / Down movement
                if (InputHandler.KeyPressed(Keys.Up))
                    menuSelectors[currSelector].MoveCursorUp();
                else if (InputHandler.KeyPressed(Keys.Down))
                    menuSelectors[currSelector].MoveCursorDown();

                // Back to World
                if (InputHandler.KeyPressed(Keys.Back)) stateStack.Pop();

                // Into submenu
                if (InputHandler.KeyPressed(Keys.Enter)  && getCursorPos().Y == 2) stateStack.Push("menu.item", ps);
                else if (InputHandler.KeyPressed(Keys.Enter) && getCursorPos().Y == 6) stateStack.Push("menu.bestiary", ps);
                else if (InputHandler.KeyPressed(Keys.Enter) && getCursorPos().Y == 7) stateStack.Push("menu.save", ps);
                else if (InputHandler.KeyPressed(Keys.Enter))
                {
                    _possibleNewState = GetNextMenuState((int)menuSelectors[currSelector].GetXyOfCursor().Y);
                    ChangeCurrentMenu(LeftMenu, 0, 0, ECursor.Selected);
                }
            }

            else if (currSelector == LeftMenu)
            {
                if (InputHandler.KeyPressed(Keys.Up) && menuSelectors[LeftMenu].GetXyOfCursor().Y == 0)
                    ChangeCurrentMenu(LeftMenu, 0, 3);
                else if (InputHandler.KeyPressed(Keys.Up))
                    menuSelectors[currSelector].MoveCursorUp();
                if (InputHandler.KeyPressed(Keys.Down) && menuSelectors[LeftMenu].GetXyOfCursor().Y == 3)
                    ChangeCurrentMenu(LeftMenu, 0, 0);
                else if (InputHandler.KeyPressed(Keys.Down))
                    menuSelectors[currSelector].MoveCursorDown();


                if (InputHandler.KeyPressed(Keys.Back))
                    ChangeCurrentMenu(_previousActiveSelector, (int)_previousActiveItem.X, (int)_previousActiveItem.Y);

                if (InputHandler.KeyPressed(Keys.Enter))
                {
                    if (_possibleNewState != "")
                    {
                        ((MenuState)stateStack.Get(_possibleNewState)).SetPartyState((int)menuSelectors[currSelector].GetXyOfCursor().Y);
                        stateStack.Push(_possibleNewState, ps);
                        ChangeCurrentMenu(0, 0, 0);
                        _possibleNewState = "";
                    } else
                    {
                        ChangeCurrentMenu(2, 0, (int)menuSelectors[currSelector].GetXyOfCursor().Y, ECursor.Selected);
                    }
                }
            }

            else if (currSelector == 3)
            {
                if (InputHandler.KeyPressed(Keys.Up) && menuSelectors[currSelector].GetXyOfCursor().Y == 0)
                    ChangeCurrentMenu(currSelector, 0, 3);
                else if (InputHandler.KeyPressed(Keys.Up))
                    menuSelectors[currSelector].MoveCursorUp();
                if (InputHandler.KeyPressed(Keys.Down) && menuSelectors[currSelector].GetXyOfCursor().Y == 3)
                    ChangeCurrentMenu(currSelector, 0, 0);
                else if (InputHandler.KeyPressed(Keys.Down))
                    menuSelectors[currSelector].MoveCursorDown();

                if (InputHandler.KeyPressed(Keys.Back))
                    ChangeCurrentMenu(2, (int)getCursorPos().X, (int)getCursorPos().Y);
                if (InputHandler.KeyPressed(Keys.Enter))
                {
                    var (first, second) = (menuSelectors[LeftMenu].GetCurrIndex(), menuSelectors[currSelector].GetCurrIndex());
                    if (first != second)
                    {
                        ps.Swap(first, second);
                        UpdateParty(ps);
                    } else
                    {
                        ps.Slots[first].IsBackRow = !ps.Slots[first].IsBackRow;
                    }
                    ChangeCurrentMenu(2, (int)getCursorPos().X, (int)getCursorPos().Y);
                }
            }

            // Left-Right movement
            if (InputHandler.KeyPressed(Keys.Left))
            {
                if (currSelector != LeftMenu)
                {
                    _previousActiveItem = menuSelectors[currSelector].GetXyOfCursor();
                    _previousActiveSelector = currSelector;
                    ChangeCurrentMenu(LeftMenu, 0, 0);
                }
            }
            if (InputHandler.KeyPressed(Keys.Right))
            {
                if (currSelector == LeftMenu)
                    ChangeCurrentMenu(_previousActiveSelector, (int)_previousActiveItem.X, (int)_previousActiveItem.Y);
            }

        }

        string GetNextMenuState(int index)
        {
            return index switch
            {
                0 => "menu.job",
                3 => "menu.magic",
                4 => "menu.equipment",
                5 => "menu.status",
                6 => "menu.bestiary",
                7 => "menu.save",
                _ => ""
            };
        }
    }
}

