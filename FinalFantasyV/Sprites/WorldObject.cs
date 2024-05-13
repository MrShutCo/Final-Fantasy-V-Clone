using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FinalFantasyV.Sprites;

public class WorldObject
{
    public int X;
    public int Y;
    public bool IsVisible;
    
    private readonly SpriteSheet _spriteSheet;
    
    

    public WorldObject(SpriteSheet spriteSheet)
    {
        _spriteSheet = spriteSheet;
    }

    public void Render(SpriteBatch sb)
    {
        if (IsVisible)
            _spriteSheet.Draw(sb, new Vector2(X,Y));
    }
}