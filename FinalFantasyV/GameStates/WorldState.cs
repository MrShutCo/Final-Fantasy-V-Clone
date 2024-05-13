using System.Collections.Generic;
using System.Linq;
using Engine.RomReader;
using FinalFantasyV.Events;
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
        
        private Texture2D _bg1H;
        private Texture2D _bg1L;
        private Texture2D _bg2H;
        private Texture2D _bg2L;
        private Texture2D _bg3H;
        private Texture2D _bg3L;
        private MapManager.Wall[,] _walls;
        
        private readonly Camera _camera;
        private readonly RomGame _rom;
        private List<MapExit> _exits;

        private Menu _menu;

        private EventManager _eventManager;
        private NewEventManager _newEvent;
        private Queue<IGameEvent> _events;

        private PartyState _partyState;

        public WorldState(ContentManager cm)
		{
            // Bartz: new Vector2(365, 452)
            // Lenna: new Vector2(365-1, 452-7)
            // Galuf new Vector2(365-4, 452-13)
            _newEvent = new();
            var menuTex = cm.Load<Texture2D>("fontmenu");
            _menu = new();
            WorldCharacter = new WorldCharacter(new SpriteSheet(FF5.Bartz, 16, 16, new Vector2(365, 452), new Vector2(4, 4)), 
                new Vector2(33*16, 25*16));
            _camera = new Camera();
            WorldCharacter.DoneMovement += CheckNewTile;
            WorldCharacter.IsVisible = true;
            Objects = new WorldCharacter[32];
            _rom = new RomGame();
            
            _eventManager = new EventManager();
            ChangeMap(32);
        }

        private void ChangeMap(int newMapId)
        {
            GraphicsDevice gd = FF5.Graphics.GraphicsDevice;

            var img = _rom.GetLayers(newMapId);
            _bg1H = SpriteSheet.ConvertToTex(gd, img.Item1);
            _bg1L = SpriteSheet.ConvertToTex(gd, img.Item2);
            _bg2H = SpriteSheet.ConvertToTex(gd, img.Item3);
            _bg2L = SpriteSheet.ConvertToTex(gd, img.Item4);
            _bg3H = SpriteSheet.ConvertToTex(gd, img.Item5);
            _bg3L = SpriteSheet.ConvertToTex(gd, img.Item6);
            _walls = img.Item7;
            /*ImageExtensions.SaveAsPng(img.Item1, "bg1H.png");
            ImageExtensions.SaveAsPng(img.Item2, "bg1L.png");
            ImageExtensions.SaveAsPng(img.Item3, "bg2H.png");
            ImageExtensions.SaveAsPng(img.Item4, "bg2L.png");
            ImageExtensions.SaveAsPng(img.Item5, "bg3H.png");
            ImageExtensions.SaveAsPng(img.Item6, "bg3L.png");*/
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

            spriteBatch.Draw(_bg3H, new Vector2(), Color.White);
            spriteBatch.Draw(_bg3L, new Vector2(), Color.White);

            spriteBatch.Draw(_bg2H, new Vector2(), Color.White);
            spriteBatch.Draw(_bg1H, new Vector2(), Color.White);

            WorldCharacter.Draw(spriteBatch);
            foreach (var s in Objects)
                s.Draw(spriteBatch);
            
            spriteBatch.Draw(_bg2L, new Vector2(), Color.White);
            spriteBatch.Draw(_bg1L, new Vector2(), Color.White);
            spriteBatch.End();
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
                    WorldCharacter.Move(ECharacterMove.Left);
                WorldCharacter.Face(ECharacterMove.Left);
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Right))
            {
                if (_walls[(int)tilePos.X+1, (int)tilePos.Y].PassableLeft && CanWalkHere((int)tilePos.X+1, (int)tilePos.Y))
                    WorldCharacter.Move(ECharacterMove.Right);
                WorldCharacter.Face(ECharacterMove.Right);
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Up))
            {
                if (_walls[(int)tilePos.X, (int)tilePos.Y-1].PassableDown && CanWalkHere((int)tilePos.X, (int)tilePos.Y-1))
                    WorldCharacter.Move(ECharacterMove.Up);
                WorldCharacter.Face(ECharacterMove.Up);
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Down))
            {
                if (_walls[(int)tilePos.X, (int)tilePos.Y+1].PassableUp && CanWalkHere((int)tilePos.X, (int)tilePos.Y+1))
                    WorldCharacter.Move(ECharacterMove.Down);
                WorldCharacter.Face(ECharacterMove.Down);
            }

            if (InputHandler.KeyPressed(Keys.Enter))
                stateStack.Push("menu", ps);
            if (Keyboard.GetState().IsKeyDown(Keys.B)) stateStack.Push("battle", ps);
        }

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

