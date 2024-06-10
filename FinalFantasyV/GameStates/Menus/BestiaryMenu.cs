using System;
using System.Collections.Generic;
using Engine.RomReader;
using Final_Fantasy_V.Models;
using FinalFantasyV.Sprites;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace FinalFantasyV.GameStates.Menus;

public class BestiaryMenu : MenuState
{
    private int _monsterIndex;
    private Monster _monster;
    private Enemy _enemy;
    private RomGame _rom;

    private int _page;
    private MonsterAI _monsterAi;
    
    public BestiaryMenu(ContentManager cm, RomGame rom) : base(cm)
    {
        _rom = rom;
        _monsterIndex = 0;
        _monster = rom.GetMonster(FF5.Graphics.GraphicsDevice, _monsterIndex);
        _enemy = _rom.GetEnemy(_monsterIndex);
    }

    public override void OnEnter(PartyState ps, object data)
    {
        // Layer 0
        Menu.SetBox(tileData, 1, 0, 14, 3, 0);
        Menu.SetBox(tileData, 16, 0, 15, 30, 0);
        
        Menu.SetBox(tileData, 1, 0, 14, 3, 1);
        Menu.SetBox(tileData, 1, 14, 30, 16, 1);
        tileData.SetLayerVisible(1, false);
        //_monsterAIScript = _rom.GetMonsterAI(_monsterIndex);
        _monsterAi = _rom.GetMonsterAI(_monsterIndex);
        //_rom.CheckForOverlapping();
    }

    public override void Render(SpriteBatch spriteBatch, PartyState ps)
    {
        base.Render(spriteBatch, ps);
        spriteBatch.Begin();
        
        Menu.DrawString(spriteBatch, menuSpritesheet, _enemy.Name, new Vector2(16, 8));
        Menu.DrawString(spriteBatch, menuSpritesheet, _monsterIndex.ToString(), new Vector2(14*8,32),false);
        
        
        if (_page == 0)
        {
            Menu.DrawManyString(spriteBatch, menuSpritesheet,
                ["LV", "HP", "MP", "Str.", "Def.", "Evade", "Magic", "Mag.Def", "", "Gil", "EXP", "", "Steal", "Drops"],
                new Vector2(8 * 16 + 8, 8), 16);
            Menu.DrawManyString(spriteBatch, menuSpritesheet,
            [
                $"{_enemy.Level}", $"{_enemy.HP}", $"{_enemy.MP}", $"{_enemy.Attack}", $"{_enemy.Defense}",
                $"{_enemy.Evade}", $"{_enemy.MagicPower}", $"{_enemy.MagicDefense}", "", $"{_enemy.Gil}",
                $"{_enemy.Exp}"
            ], new Vector2(8 * 29, 8), 16, false);
            _monster.Draw(spriteBatch, new Vector2(32,128-32));
        }

        if (_page == 1)
        {
            Menu.DrawManyString(spriteBatch, menuSpritesheet, _monsterAi.AiText.ToArray(), new Vector2(2*8, 120), 12);
            _monster.Draw(spriteBatch, new Vector2(8*20,32));
        }

        spriteBatch.End();

    }

    public override void Update(GameTime gameTime, PartyState ps)
    {
        if (InputHandler.KeyPressed(Keys.Back)) stateStack.Pop();
        if (InputHandler.KeyPressed(Keys.Left))
        {
            _monsterIndex = Math.Max(_monsterIndex - 1, 0);
            UpdateMonster();
        }
        if (InputHandler.KeyPressed(Keys.Right))
        {
            _monsterIndex = Math.Max(_monsterIndex + 1, 0);
            UpdateMonster();
        }

        if (InputHandler.KeyPressed(Keys.Enter))
        {
            tileData.SetLayerVisible(_page, false);
            _page = (_page + 1) % 2;
            tileData.SetLayerVisible(_page, true);
        }
        base.Update(gameTime, ps);
    }

    private void UpdateMonster()
    {
        _monster = _rom.GetMonster(FF5.Graphics.GraphicsDevice, _monsterIndex);
        _enemy = _rom.GetEnemy(_monsterIndex);
        _monsterAi = _rom.GetMonsterAI(_monsterIndex);
    }
}
