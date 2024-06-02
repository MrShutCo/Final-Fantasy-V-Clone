using System;
using System.Collections.Generic;
using Engine.RomReader;
using FinalFantasyV.Sprites;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace FinalFantasyV.GameStates
{
    public delegate void BattleCallback();

    public class BattleState : IState
    {
        private enum EBattleState
        {
            TimeFlowing,
            UnitActing,
            Victory,
            GameOver
        }

        public StateStack stateStack { get; set; }
        public event BattleCallback Ended;

        private TilesetInfo _tilesetInfo;
        private Map _tileData;
        private List<BattleUnit> _units;
        private SpriteSheet _background;
        private InputHandler _input;
        
        private RomGame _romGame;

        private EBattleState _battleState;
        
        private SpriteSheet[] _heros;
        private Menu _menu;
        private BattleUnit _actingUnit;
        private const int AtbPerSecond = 50;
        
        private BattleGroup _group;
        private int _groupId;

        public BattleState(ContentManager cm, RomGame romGame)
        {
            _romGame = romGame;
            var bartzTex = cm.Load<Texture2D>("Bartz");
            var lennaTex = cm.Load<Texture2D>("Lenna");
            var galufTex = cm.Load<Texture2D>("Galuf");
            var farisTex = cm.Load<Texture2D>("Faris");
            var menuTex = cm.Load<Texture2D>("fontmenu");
            _tileData = MapIO.ReadMap("Tilemaps/battle.tmj");
            _tilesetInfo = MapIO.ReadTileSet(cm, "Tilemaps/Menu.tsj");

            var backgroundTex = cm.Load<Texture2D>("backgrounds");
            _background = new SpriteSheet(backgroundTex, 240, 160, new Vector2(2,2), new Vector2(4, 6));
            _background.SetTile(1, 0);
            _battleState = EBattleState.TimeFlowing;

            _menu = new();

            _input = new InputHandler();
            //menuSelector = new MenuSelector(new Vector2[] { new(16*10, 16*10), new(160, 16*11)},
            //    new Action[] { });

            _heros =
            [
                new SpriteSheet(bartzTex, 30, 30, Vector2.Zero, Vector2.Zero),
                new SpriteSheet(lennaTex, 30, 30, Vector2.Zero, Vector2.Zero),
                new SpriteSheet(galufTex, 30, 30, Vector2.Zero, Vector2.Zero),
                new SpriteSheet(farisTex, 30, 30, Vector2.Zero, Vector2.Zero)
            ];
            
        }

        public void OnEnter(PartyState ps)
        {
            _tileData.SetLayerVisible(0, true);
            _tileData.SetLayerVisible(1, false);
            Ended += ActionFinished;
            _units = new List<BattleUnit>();
            _units.Add(new BattleHero(_heros[0], ps.Slots[0], new Vector2(16*13, 13*4)));
            _units.Add(new BattleHero(_heros[1], ps.Slots[1], new Vector2(16 * 13, 13 * 6)));
            _units.Add(new BattleHero(_heros[2], ps.Slots[2], new Vector2(16 * 13, 13 * 8)));
            _units.Add(new BattleHero(_heros[3], ps.Slots[3], new Vector2(16 * 13, 13 * 10)));
            _group = _romGame.GetBattleGroup(FF5.Graphics.GraphicsDevice, _groupId);
        }

        private void ActionFinished()
        {
            _battleState = EBattleState.TimeFlowing;
            _actingUnit.OnActionFinished -= ActionFinished;
            _actingUnit = null;

            _units = new();
        }

        public void OnExit()
        {
        }

        public void Render(SpriteBatch spriteBatch, PartyState ps)
        {
            spriteBatch.Begin();

            _tileData.DrawTileSet(spriteBatch, _tilesetInfo);
            _background.Draw(spriteBatch, new Vector2(8, 0));
            //units[0].Draw(spriteBatch, ps.Bartz, new Vector2(16 * 13, 26*2));
            //units[1].Draw(spriteBatch, ps.Lenna, new Vector2(16 * 13, 26*3));
            //units[2].Draw(spriteBatch, ps.Galuf, new Vector2(16 * 13, 26*4));
            //units[3].Draw(spriteBatch, ps.Faris, new Vector2(16 * 13, 26*5));
            foreach (var unit in _units)
            {
                unit.Draw(spriteBatch);
            }

            DrawEnemyNames();

            for (int i = 0; i < 4; i++)
            {
                //Menu.DrawText(_tileData, 22, 22+i*2, ps.Slots[i].CurrHP.ToString());
                DrawCharacterHealth(i, ps.Slots[i].CurrHP);
                //Menu.DrawString(spriteBatch, _background, PartyState.GetName(ps.Slots[i].Hero), new Vector2(8 * 13, 8*(22+i*2)));
            }
            
            _group.Draw(spriteBatch);

            spriteBatch.End();
        }

        private void DrawEnemyNames()
        {
            for (int i = 0; i < _group.EnemyData.Count; i++)
            {
                Menu.DrawText(_tileData, 1, 22+i*2, _group.EnemyData[i].Name);
            }
        }

        private void DrawCharacterHealth(int slot, int health)
        {
            int dig1 = health % 10;
            int dig2 = (health / 10) % 10;
            int dig3 = (health / 100) % 10;
            int dig4 = (health / 1000) % 10;
            _tileData.SetTileAt(0, 23, 22+slot*2, 52+dig1);
            _tileData.SetTileAt(0, 22, 22 + slot * 2, 52 + dig2);
            _tileData.SetTileAt(0, 21, 22 + slot * 2, 52 + dig3);
            _tileData.SetTileAt(0, 20, 22 + slot * 2, 52 + dig4);
            
        }

        private void DrawCharacterSelection(int slot)
        {
        }

        public void Update(GameTime gameTime, PartyState ps)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Back)) stateStack.Pop();

            if (InputHandler.KeyPressed(Keys.L))
            {
                _groupId++;
                for (int i = 0; i < _group.EnemyData.Count; i++)
                {
                    Menu.DrawText(_tileData, 1, 22+i*2, "          ");
                }
                _group = _romGame.GetBattleGroup(FF5.Graphics.GraphicsDevice, _groupId);
            }
            
            switch (_battleState)
            {
                case EBattleState.TimeFlowing:
                    foreach (var unit in _units)
                    {
                        unit.AdvanceATB(gameTime);
                        if (Math.Abs(unit.ATB - 255) < 0.1)
                        {
                            unit.BeginAction();
                            _battleState = EBattleState.UnitActing;
                            _actingUnit = unit;
                            _actingUnit.OnActionFinished += ActionFinished;
                            break;
                        }
                    }

                    break;
                case EBattleState.UnitActing:
                    _actingUnit.Update(gameTime, ps);
                    break;
            }
        }
    }
}

