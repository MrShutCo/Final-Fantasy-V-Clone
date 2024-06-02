using System;
using System.Collections.Generic;
using System.Linq;
using Engine.RomReader;
using FinalFantasyV.Events;
using FinalFantasyV.Menus;
using FinalFantasyV.Sprites;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace FinalFantasyV.GameStates
{
	public class WorldState : IState
    {
        
        public StateStack stateStack { get; set; }
        public readonly WorldCharacter WorldCharacter;
        public WorldCharacter[] Objects;
        private PartyState _partyState;
        public readonly TextPopup TextPopup;

        private MapManager.Wall[,] _walls;
        
        private readonly Camera _camera;
        private readonly RomGame _rom;
        private List<MapExit> _exits;

        private readonly NewEventManager _newEvent;
        private Queue<IGameEvent> _events;
        private BackgroundLayers _backgroundLayers;
        
        private Menu _menu;
        private bool _isEventComplete;
        private bool _isPaused = false;

        private Monster enemy;
        private int _enemyId = 1;

        public WorldState(ContentManager cm, RomGame romGame)
		{
            // Bartz: new Vector2(365, 452)
            // Lenna: new Vector2(365-1, 452-7)
            // Galuf new Vector2(365-4, 452-13)
            _newEvent = new();
            _menu = new();
            WorldCharacter = new WorldCharacter(FF5.BartzSprite, new Vector2(32*16, 51*16));
            _camera = new Camera();
            WorldCharacter.DoneMovement += CheckNewTile;
            WorldCharacter.IsVisible = true;
            Objects = new WorldCharacter[32];
            _rom = romGame;
            TextPopup = new();
            
            // World Map
            //ChangeMap(0, new Vector2(156 * 16, 150 * 16));
            ChangeMap(17, new Vector2(12 *16, 20*16));
            
            // Tule
            //ChangeMap(32, new Vector2(32 * 16, 45 * 16));
            
        }

        public void ChangeMap(int newMapId, Vector2 newPos)
        {
            GraphicsDevice gd = FF5.Graphics.GraphicsDevice;
            (_backgroundLayers, _walls) = _rom.GetLayers(gd, newMapId);
            _rom.Update(newMapId);
            _exits = _rom.Map.Exits;
            WorldCharacter.Position = newPos;
            //enemy = _rom.GetMonster(gd, _enemyId);
            Objects = SpriteSheet.LoadSprites(_rom.Map);

        }

        
        public void OnEnter(PartyState ps)
        {
            
        }

        public void OnExit()
        {
            
        }

        public void Render(SpriteBatch spriteBatch, PartyState ps)
        {
            spriteBatch.Begin(transformMatrix: _camera.Transform);

            _backgroundLayers.DrawBelowCharacter(spriteBatch, WorldCharacter.Position);
            WorldCharacter.Draw(spriteBatch);
            foreach (var s in Objects)
                s.Draw(spriteBatch);
            _backgroundLayers.DrawAboveCharacter(spriteBatch, WorldCharacter.Position);

            //spriteBatch.Draw(enemy, WorldCharacter.Position - new Vector2(64,64), Color.White);
            //enemy.Draw(spriteBatch, WorldCharacter.Position - new Vector2(64,64));
            spriteBatch.End();
            TextPopup.Render(spriteBatch);
        }

        private void OnEventComplete()
        {
            _isEventComplete = true;
        }

        public void SetBackgroundTileAt(int layer, int x, int y, byte tile)
        {
            _backgroundLayers.SetBackgroundTileAt(layer, x, y, tile);
        }

        public void SetFlag(int flag, bool status)
        {
            _newEvent.SetFlag(flag, status);
        }
        
        public void Update(GameTime gameTime, PartyState ps)
        {
            _partyState = ps; // TODO: This is a hack so events can have acccess. I dont like it 

            if (_isEventComplete)
            {
                _isEventComplete = false;
                _events.Peek().Completed = null;
                _events.Dequeue();
                if (_events.Count > 0)
                {
                    _events.Peek().Completed += OnEventComplete;
                    _events.Peek().OnStart(_partyState, this);
                }
            }
            
            TextPopup.Update(gameTime);
            
            WorldCharacter.Update(gameTime);
            foreach (var s in Objects)
                s.Update(gameTime);
            _camera.Follow(WorldCharacter.Position + new Vector2(8,8), new Vector2(256, 240));

            
            if (_events?.Count > 0)
            {
                if (InputHandler.KeyPressed(Keys.Q) && _isPaused == false)
                {
                    _isPaused = true;
                }
                else if (InputHandler.KeyPressed(Keys.Q) && _isPaused)
                {
                    _isPaused = false;
                }

                if (_isPaused) return;
                _events.Peek().Update(gameTime, this);
                return;
            }

            var tilePos = WorldCharacter.GetTilePosition();
            if (Keyboard.GetState().IsKeyDown(Keys.Left))
            {
                if (_walls[(int)tilePos.X-1, (int)tilePos.Y].PassableRight && CanWalkHere((int)tilePos.X-1, (int)tilePos.Y))
                    WorldCharacter.Move(ECharacterMove.Left, WorldCharacter.FastWalkingSpeed);
                WorldCharacter.Face(ECharacterMove.Left);
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Right))
            {
                if (_walls[(int)tilePos.X+1, (int)tilePos.Y].PassableLeft && CanWalkHere((int)tilePos.X+1, (int)tilePos.Y))
                    WorldCharacter.Move(ECharacterMove.Right,WorldCharacter.FastWalkingSpeed);
                WorldCharacter.Face(ECharacterMove.Right);
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Up))
            {
                if (_walls[(int)tilePos.X, (int)tilePos.Y-1].PassableDown && CanWalkHere((int)tilePos.X, (int)tilePos.Y-1))
                    WorldCharacter.Move(ECharacterMove.Up,WorldCharacter.FastWalkingSpeed);
                WorldCharacter.Face(ECharacterMove.Up);
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Down))
            {
                if (_walls[(int)tilePos.X, (int)tilePos.Y+1].PassableUp && CanWalkHere((int)tilePos.X, (int)tilePos.Y+1))
                    WorldCharacter.Move(ECharacterMove.Down,WorldCharacter.FastWalkingSpeed);
                WorldCharacter.Face(ECharacterMove.Down);
            }

            if (Keyboard.GetState().IsKeyDown(Keys.P))
            {
                /*var startEvent = _rom.Map.Events.Find(e => e.actionId == 38);
                
                _events = _newEvent.ProcessEvent(_rom, startEvent);
                if (_events?.Count > 0)
                {
                    _events.Peek().Completed += OnEventComplete;
                    _events.Peek().OnStart(_partyState, this);
                }*/
                var events = _rom.GetStart();
                _events = _newEvent.ProcessEvent(_rom, events);
                _events.Peek().Completed += OnEventComplete;
                _events.Peek().OnStart(_partyState, this);
            }

            if (InputHandler.KeyPressed(Keys.L))
            {
                _enemyId++;
                //enemy = _rom.GetMonster(FF5.Graphics.GraphicsDevice, _enemyId);
                //group = _rom.GetBattleGroup(FF5.Graphics.GraphicsDevice, _enemyId);
            }

            if (InputHandler.KeyPressed(Keys.Space))
            {
                var facingTilePos = WorldCharacter.GetPositionFacing();
                foreach (var obj in Objects)
                {
                    if (obj is WorldNPC && obj.Position == new Vector2(facingTilePos.X * 16, facingTilePos.Y * 16))
                    {
                        //obj = (obj as WorldNPC).;
                        //_textPopup.ShowText();
                    }
                }
                
            }

            if (InputHandler.KeyPressed(Keys.Enter))
                stateStack.Push("menu", ps);
            if (Keyboard.GetState().IsKeyDown(Keys.B)) stateStack.Push("battle", ps);
        }

        public void ShowDialogue(string text)
        {
            TextPopup.ShowText(text);
        }

        public void HideDialogue() => TextPopup.IsActive = false;

        private void CheckNewTile()
        {
            var pos = WorldCharacter.GetTilePosition();
            if (_events?.Count > 0) return; // Dont check new events on new tile
            //_eventManager.CheckCollisionOnEvent(_rom, (byte)pos.X, (byte)pos.Y);
            _events = _newEvent.CheckCollisionOnEvent(_rom, (byte)pos.X, (byte)pos.Y);
            if (_events?.Count > 0)
            {
                _events.Peek().Completed += OnEventComplete;
                _events.Peek().OnStart(_partyState, this);
            }
            
            foreach (var mapChange in _exits)
            {
                var (x, y) = ((byte)pos.X, (byte)pos.Y);
                if (x == mapChange.originX && y == mapChange.originY)
                {
                    ChangeMap(mapChange.GetMapId(),new Vector2(mapChange.GetDestX()*16, mapChange.GetDestY()*16));
                }
            }
        }

        private bool CanWalkHere(int x, int y)
        {
            foreach (var obj in Objects)
            {
                if (obj.Position == new Vector2(x*16, y*16) && obj.IsVisible) return false;
            }

            return true;
        }
            
        
        
    }
}

