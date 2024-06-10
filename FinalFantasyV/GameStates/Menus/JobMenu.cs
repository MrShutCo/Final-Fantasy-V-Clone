using System;
using System.Collections.Generic;
using Final_Fantasy_V.Models;
using FinalFantasyV.Content;
using FinalFantasyV.Sprites;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace FinalFantasyV.GameStates.Menus;

public class JobMenu : MenuState
{

    private Vector2 _selectedPos;
    private bool _hasHandsUp;

    private List<Vector2> _positions;
    
    public JobMenu(ContentManager cm) : base(cm)
    {
        _positions = new List<Vector2>();
        for (int i = 0; i < 7; i++)
            _positions.Add(new Vector2(8*(2+i*3.5f),8*9-4));
        for (int i = 0; i < 8; i++)
            _positions.Add(new Vector2(8*(0.5f+i*3.5f) ,8*13-4));
        for (int i = 0; i < 7; i++)
            _positions.Add(new Vector2(8*(2+i*3.5f) ,8*17-4));
        _hasHandsUp = false;
        
        menuSelectors = new[]
        {
            new CustomSelector(_positions, new List<int>{7, 8, 7}),
            new CustomSelector(new List<Vector2>{new(-40,-40)}, new List<int>(1))
        };
    }

    public override void OnEnter(PartyState ps, object _)
    {
        var h = ps.Slots[slotIndex];
        Menu.SetBox(tileData, 1,1,30,4);
        Menu.SetBox(tileData, 1,1,30,4);
        Menu.SetBox(tileData, 1, 5, 30, 16);
        Menu.SetBox(tileData, 25, 1, 6, 3);
        Menu.SetBox(tileData, 1, 21, 30, 6);
        Menu.DrawText(tileData, 26, 2, "Job");
        Menu.DrawText(tileData, 4, 2, PartyState.GetName(h.Hero));
        Menu.DrawText(tileData, 5, 3, $"LV{Menu.PadNumber(h.Level, 2)}");
        ChangeCurrentMenu(0, 0, 0);
    }

    public override void Render(SpriteBatch spriteBatch, PartyState ps)
    {
        var hero = ps.Slots[slotIndex];
        base.Render(spriteBatch, ps);
        spriteBatch.Begin();
        ps.HeroSprites[slotIndex].Draw(spriteBatch, new Vector2(8*2-4,12));

        for (int i = 0; i < 21; i++)
        {
            if (i == menuSelectors[0].GetCurrIndex())
                ps.HeroSprites[slotIndex].Draw(spriteBatch, new Rectangle(30*(_hasHandsUp ? 8 : 0),30*(i+1), 30, 24), _positions[i] + new Vector2(16, -8));
            else
                ps.HeroSprites[slotIndex].Draw(spriteBatch, new Rectangle(0,30*(i+1), 30, 24), _positions[i] + new Vector2(16, -8), Color.Gray);
        }
        
        Menu.DrawString(spriteBatch, menuSpritesheet, PartyState.GetJob(hero.Job), new Vector2(8*13, 8*2));
        RenderCursor(spriteBatch);
        
        spriteBatch.End();
    }

    public override void Update(GameTime gameTime, PartyState ps)
    {
        base.Update(gameTime, ps);
        
        if (currSelector == 0)
        {
            if (InputHandler.KeyPressed(Keys.Left)) menuSelectors[currSelector].MoveCursorLeft();
            if (InputHandler.KeyPressed(Keys.Right)) menuSelectors[currSelector].MoveCursorRight();
            if (InputHandler.KeyPressed(Keys.Up)) menuSelectors[currSelector].MoveCursorUp();
            if (InputHandler.KeyPressed(Keys.Down)) menuSelectors[currSelector].MoveCursorDown();
            if (InputHandler.KeyPressed(Keys.Back)) stateStack.Pop();
            if (InputHandler.KeyPressed(Keys.Enter))
            {
                _selectedPos = menuSelectors[currSelector].GetXyOfCursor();
                ChangeCurrentMenu(1, 0,0, ECursor.Visible);
            }
        }

        else if (currSelector == 1)
        {
            if (InputHandler.KeyPressed(Keys.Back)) ChangeCurrentMenu(0, (int)_selectedPos.X, (int)_selectedPos.Y);
            if (InputHandler.KeyPressed(Keys.Enter))
            {
                ps.Slots[slotIndex].Job = (EJob)menuSelectors[0].GetCurrIndex();
            }
            _hasHandsUp = gameTime.TotalGameTime.Milliseconds % 500 > 250;
        }
    }
}