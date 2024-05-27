using FinalFantasyV.Sprites;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace FinalFantasyV.Menus;

public class TextPopup
{
    private Menu _menu;
    private TilesetInfo _tilesetInfo;
    private Map tileData;
    public bool IsActive;

    private SpriteSheet menuSpritesheet;
    private string text;
    
    public TextPopup()
    {
        tileData = MapIO.ReadMap("Tilemaps/mainmenu.tmj");
        menuSpritesheet = new SpriteSheet(FF5.MenuTexture, 8, 8, Vector2.Zero, Vector2.Zero);
        _tilesetInfo = FF5.MenuTilesetInfo;
        Menu.SetBox(tileData, 1, 1, 30, 8);
    }
    
    public void ShowText(string text)
    {
        IsActive = true;
        this.text = text;
    }

    public void Render(SpriteBatch sb)
    {
        if (IsActive)
        {
            sb.Begin();
            tileData.DrawTileSet(sb, _tilesetInfo);
            sb.End();
            
            sb.Begin(transformMatrix: Matrix.CreateScale(0.75f,1f,1));
            Menu.DrawString(sb, menuSpritesheet, text, new Vector2(20,16));
            sb.End();
        }
    }

    public void Update(GameTime gt)
    {
    }
}