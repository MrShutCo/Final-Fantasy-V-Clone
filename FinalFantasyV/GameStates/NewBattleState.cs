using System;
using System.Collections.Generic;
using Engine.RomReader;
using Final_Fantasy_V.Models;
using FinalFantasyV.Content;
using FinalFantasyV.Sprites;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.Content;

namespace FinalFantasyV.GameStates;

public class NewBattleState : IState
{
    private enum EBattleState
    {
        TimeFlowing,
        UnitActing,
        Victory,
        GameOver
    }
    
    public StateStack stateStack { get; set; }

    private BattleGroup _battleGroup;
    
    private List<SpriteSheet> heroSpriteSheets;
    private SpriteSheet _background;
    
    private TilesetInfo _tilesetInfo;
    private Map _tileData;
    private SpriteSheet menuSpritesheet;
    private List<BattleUnit> _units;
    private EBattleState _battleState;
    
    private CustomSelector? _actionSelector;
    private CustomSelector _monsterSelector;
    private CustomSelector _heroSelector;

    private Queue<BattleUnit> _queueUnits;
    
    public NewBattleState(ContentManager cm)
    {
        _background = new SpriteSheet(cm.Load<Texture2D>("backgrounds"), 240, 160, new Vector2(2, 2),
            new Vector2(4, 6));
        _tileData = MapIO.ReadMap("Tilemaps/mainmenu.tmj");
        _tilesetInfo = MapIO.ReadTileSet(cm, "Tilemaps/Menu.tsj");
        menuSpritesheet = new SpriteSheet(FF5.MenuTexture, 8, 8, Vector2.Zero, Vector2.Zero);
        heroSpriteSheets =
        [
            new SpriteSheet(FF5.Bartz, 30, 30, Vector2.Zero, Vector2.Zero),
            new SpriteSheet(FF5.Lenna, 30, 30, Vector2.Zero, Vector2.Zero),
            new SpriteSheet(FF5.Galuf, 30, 30, Vector2.Zero, Vector2.Zero),
            new SpriteSheet(FF5.Faris, 30, 30, Vector2.Zero, Vector2.Zero)
        ];
        _queueUnits = [];
        
        
        Menu.SetBox(_tileData, 1,20, 12, 10);
        Menu.SetBox(_tileData, 13,20, 18, 10);
        Menu.SetBox(_tileData, 7, 20, 9, 10, 1);
        _tileData.SetLayerVisible(1, false);
    }
    
    public void OnEnter(PartyState ps, object data)
    {
        if (data is BattleGroup group)
        {
            _battleGroup = group;
        }
        _units = new List<BattleUnit>();
        _units.Add(new BattleHero(heroSpriteSheets[0], ps.Slots[0], new Vector2(16*12.5f, 13*4)));
        _units.Add(new BattleHero(heroSpriteSheets[1], ps.Slots[1], new Vector2(16 * 12.5f, 13 * 6)));
        _units.Add(new BattleHero(heroSpriteSheets[2], ps.Slots[2], new Vector2(16 * 12.5f, 13 * 8)));
        _units.Add(new BattleHero(heroSpriteSheets[3], ps.Slots[3], new Vector2(16 * 12.5f, 13 * 10)));

        List<Vector2> heroPositions = [];
        List<int> heroRows = [];
        
        foreach (var unit in _units)
        {
            heroPositions.Add(unit.Position - new Vector2(0,0));
            heroRows.Add(1);
        }
        
        List<Vector2> monsterPositions = [];
        List<int> monsterRows = [];
        
        foreach (var unit in _battleGroup.MonsterPositions)
        {
            monsterPositions.Add(unit + new Vector2(0,0));
            monsterRows.Add(1);
        }
        
        _units[0].OnActionFinished += OnActionFinished;

        _heroSelector = new CustomSelector(heroPositions, heroRows);
        _monsterSelector = new CustomSelector(monsterPositions, monsterRows);
    }
    
    void OnActionFinished()
    {
        _battleState = EBattleState.TimeFlowing;
        _actionSelector.CursorState = ECursor.InActive;
        _monsterSelector.CursorState = ECursor.InActive;
        
    }
    
    public void Update(GameTime gameTime, PartyState ps)
    {
        switch (_battleState)
        {
            case EBattleState.TimeFlowing:
                TimeFlowing(gameTime,ps);
                break;
            case EBattleState.UnitActing:
                UnitActing();
                break;
            case EBattleState.Victory:
                break;
            case EBattleState.GameOver:
                break;
        }

    }

    private void UnitActing()
    {
        var actingUnit = _queueUnits.Dequeue();
        if (actingUnit is BattleHero)
        {
            
        }
        
        var attackedEnemy = (int)_monsterSelector.GetXyOfCursor().X;
        _battleGroup.EnemyData[attackedEnemy].CurrHP -= 10;
        Console.WriteLine("Hit enemy for 10 damage");
        actingUnit.ATB = 0;
        _battleState = EBattleState.TimeFlowing;
        _actionSelector = null;
    }

    private void TimeFlowing(GameTime gameTime, PartyState ps)
    {
        _units[0].AdvanceATB(gameTime);
        _units[0].Update(gameTime, ps);

        if ((byte)_units[0].ATB == 255 && _actionSelector == null)
        {
            _queueUnits.Enqueue(_units[0]);
            _actionSelector = new CustomSelector([new Vector2(8*6, 8*22),new Vector2(8*6, 8*28)], [1,1])
            {
                CursorState = ECursor.Visible
            };
        }
        
        if (_monsterSelector.CursorState == ECursor.Visible)
        {
            if (InputHandler.KeyPressed(Keys.Enter))
            {
                _battleGroup.EnemyData[(int)_monsterSelector.GetXyOfCursor().X].CurrHP -= 10;
                _monsterSelector.CursorState = ECursor.InActive;
                _tileData.SetLayerVisible(1, false);
                _units[0].BeginAction();
                //_battleState = EBattleState.UnitActing;
                return;
            }
        }

        if (_queueUnits.TryPeek(out var unit) && unit is BattleHero)
        {
            _tileData.SetLayerVisible(1, true);
            Menu.DrawText(_tileData, 8, 22, "Fight", 1);
            Menu.DrawText(_tileData, 8, 28, "Item", 1);
            
            if (InputHandler.KeyPressed(Keys.Up))
                _actionSelector.MoveCursorUp();
            else if (InputHandler.KeyPressed(Keys.Down))
                _actionSelector.MoveCursorDown();
            else if (InputHandler.KeyPressed(Keys.Enter))
            {
                _actionSelector.CursorState = ECursor.InActive;
                _monsterSelector.CursorState = ECursor.Visible;
            }
        }
        
        
        _actionSelector?.Update(gameTime);
    }

    public void Render(SpriteBatch spriteBatch, PartyState ps)
    {
        spriteBatch.Begin();
        _background.Draw(spriteBatch, new Vector2(8,0));
        _tileData.DrawTileSet(spriteBatch, _tilesetInfo);
        _battleGroup.Draw(spriteBatch);

        
        for (int i = 0; i < ps.Slots.Length; i++)
        {
            Menu.DrawText(_tileData, 14, 22 + i * 2, ps.Slots[i].Hero.ToString());
            Menu.DrawText(_tileData, 20, 22 + i * 2, ps.Slots[i].CurrHP.ToString());
            Menu.DrawATBBar(_tileData, 25, 22 + i * 2, (byte)_units[i].ATB);
        }
        
        for (int i = 0; i < _battleGroup.EnemyData.Count; i++)
        {
            Menu.DrawText(_tileData, 2, 22+i*2, _battleGroup.EnemyData[i].Name);
        }

        foreach (var unit in _units)
        {
            unit.Draw(spriteBatch);
        }
        
        spriteBatch.DrawString(FF5.Font, _battleState.ToString(), new Vector2(0,0), Color.White);
        
        _actionSelector?.Draw(spriteBatch, _tileData, menuSpritesheet);
        _monsterSelector?.Draw(spriteBatch, _tileData, menuSpritesheet);
        
        spriteBatch.End();
    }

    

    public void OnExit()
    {
    }
}