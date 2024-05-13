using FinalFantasyV.Content;
using FinalFantasyV.Sprites;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace FinalFantasyV.GameStates
{
	public abstract class MenuState : IState
	{
        public StateStack stateStack { get; set; }

        protected TilesetInfo tilesetInfo;
        protected Map tileData;
        protected Menu menu;
        protected SpriteSheet menuSpritesheet;
        protected MenuSelector[] menuSelectors;
        protected int currSelector;

        protected InputHandler inputHandler;

        protected int slotIndex;

        public MenuState(ContentManager cm)
		{
            tileData = MapIO.ReadMap("Tilemaps/mainmenu.tmj");
            tilesetInfo = MapIO.ReadTileSet(cm, "Tilemaps/Menu.tsj");
            var menuTex = cm.Load<Texture2D>("fontmenu");
            menuSpritesheet = new SpriteSheet(menuTex, 8, 8, Vector2.Zero, Vector2.Zero);
            menu = new();
            inputHandler = new InputHandler();
        }

        public abstract void OnEnter(PartyState ps);

        public void OnExit()
        {

        }

        public virtual void Render(SpriteBatch spriteBatch, PartyState ps)
        {
            spriteBatch.Begin();
            tileData.DrawTileSet(spriteBatch, tilesetInfo);
           
            spriteBatch.End();
        }

        public void RenderCursor(SpriteBatch spriteBatch)
        {
            foreach (var m in menuSelectors)
                m.Draw(spriteBatch, tileData, menuSpritesheet);
        }

        public void SetPartyState(int slotIndex) => this.slotIndex = slotIndex;

        protected void ChangeCurrentMenu(int newMenu, int startX, int startY, ECursor oldCursorState = ECursor.InActive)
        {
            menuSelectors[currSelector].CursorState = oldCursorState;
            menuSelectors[newMenu].CursorState = ECursor.Visible;
            currSelector = newMenu;
            menuSelectors[currSelector].SetCursorTo(startX, startY);
        }

        public virtual void Update(GameTime gameTime, PartyState ps)
        {
            foreach (var m in menuSelectors)
                m.Update(gameTime);


        }

        protected int getCursorX() => (int)menuSelectors[currSelector].GetXyOfCursor().X;
        protected int getCursorY() => (int)menuSelectors[currSelector].GetXyOfCursor().Y;
        protected Vector2 getCursorPos() => menuSelectors[currSelector].GetXyOfCursor();
    }
}

