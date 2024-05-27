﻿using System;
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

        private MapManager.Wall[,] _walls;
        
        private readonly Camera _camera;
        private readonly RomGame _rom;
        private List<MapExit> _exits;

        private Menu _menu;

        private NewEventManager _newEvent;
        private Queue<IGameEvent> _events;
        private BackgroundLayers _backgroundLayers;

        private PartyState _partyState;
        private TextPopup _textPopup;
        

        public WorldState(ContentManager cm)
		{
            // Bartz: new Vector2(365, 452)
            // Lenna: new Vector2(365-1, 452-7)
            // Galuf new Vector2(365-4, 452-13)
            _newEvent = new();
            var menuTex = cm.Load<Texture2D>("fontmenu");
            _menu = new();
            WorldCharacter = new WorldCharacter(new SpriteSheet(FF5.Bartz, 16, 16, new Vector2(365, 452), new Vector2(4, 4)), 
                new Vector2(32*16, 51*16));
            _camera = new Camera();
            WorldCharacter.DoneMovement += CheckNewTile;
            WorldCharacter.IsVisible = true;
            Objects = new WorldCharacter[32];
            _rom = new RomGame();
            _textPopup = new();
            
            ChangeMap(32);
            //ChangeMap(0);
        }

        private void ChangeMap(int newMapId)
        {
            GraphicsDevice gd = FF5.Graphics.GraphicsDevice;
            (_backgroundLayers, _walls) = _rom.GetLayers(gd, newMapId);
            _rom.Update(newMapId);
            _exits = _rom.Map.Exits;
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

            _backgroundLayers.DrawBelowCharacter(spriteBatch);
            WorldCharacter.Draw(spriteBatch);
            foreach (var s in Objects)
                s.Draw(spriteBatch);
            _backgroundLayers.DrawAboveCharacter(spriteBatch);
            spriteBatch.End();
            _textPopup.Render(spriteBatch);
        }

        void OnEventComplete()
        {
            _events.Peek().Completed = null;
            _events.Dequeue();
            if (_events.Count > 0)
            {
                _events.Peek().Completed += OnEventComplete;
                _events.Peek().OnStart(_partyState, this);
            }
        }

        public void SetFlag(int flag, bool status)
        {
            _newEvent.SetFlag(flag, status);
        }

        public void Update(GameTime gameTime, PartyState ps)
        {
            _partyState = ps; // TODO: This is a hack so events can have acccess. I dont like it 
            
            //_textPopup.Update(gameTime);
            //if (_textPopup.IsActive)
            //{
            //    return;
            //}
            
            WorldCharacter.Update(gameTime);
            foreach (var s in Objects)
                s.Update(gameTime);
            _camera.Follow(WorldCharacter.Position, new Vector2(256, 240));

            
            if (_events?.Count > 0)
            {
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

            if (InputHandler.KeyPressed(Keys.Space))
            {
                var facingTilePos = WorldCharacter.GetPositionFacing();
                foreach (var obj in Objects)
                {
                    if (obj is WorldNPC && obj.Position == new Vector2(facingTilePos.X * 16, facingTilePos.Y * 16))
                    {
                        _textPopup.ShowText("Lenna:  A cave in such a place...[EOL](Bartz):  It must have been [EOL]  formed by that earthquake...");
                    }
                }
                
            }

            if (InputHandler.KeyPressed(Keys.Enter))
                stateStack.Push("menu", ps);
            if (Keyboard.GetState().IsKeyDown(Keys.B)) stateStack.Push("battle", ps);
        }

        public void ShowDialogue(string text)
        {
            _textPopup.ShowText(text);
        }

        public void HideDialogue() => _textPopup.IsActive = false;

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
                    ChangeMap(mapChange.GetMapId());
                    WorldCharacter.Position = new Vector2(mapChange.GetDestX()*16, mapChange.GetDestY()*16);
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

