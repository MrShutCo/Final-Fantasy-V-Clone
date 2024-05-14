using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Color = Microsoft.Xna.Framework.Color;


namespace Engine.RomReader;

public class BackgroundLayers
{
    private readonly List<MapManager.TileTexture> _tileTextures;
    private readonly List<byte> _tilemap00;
    private readonly List<byte>? _tilemap01;
    private readonly List<byte>? _tilemap02;
    
    public BackgroundLayers(){}
    
    public BackgroundLayers(List<MapManager.TileTexture> tileTextures, List<byte> tilemap00, List<byte>? tilemap01, List<byte>? tilemap02)
    {
        _tileTextures = tileTextures;
        _tilemap00 = tilemap00;
        _tilemap01 = tilemap01;
        _tilemap02 = tilemap02;
    }
    
    public void DrawBelowCharacter(SpriteBatch sb)
    {
        DrawLayer(sb, _tilemap02, false, true);
        DrawLayer(sb, _tilemap02, true, true);
        DrawLayer(sb, _tilemap01, false, false);
        DrawLayer(sb, _tilemap00, false, false);
    }
    
    public void DrawAboveCharacter(SpriteBatch sb)
    {
        DrawLayer(sb, _tilemap01, true, false);
        DrawLayer(sb, _tilemap00, true, false);
    }

    void DrawLayer(SpriteBatch sb, List<byte> tilemap, bool down, bool background02)
    {
        var (x, y) = (0, 0);
        foreach (var item in tilemap)
        {
            if (background02 && down) sb.Draw(_tileTextures[item].ImageP1Bg02, new Vector2(x,y), Color.White);
            if (background02 && !down) sb.Draw(_tileTextures[item].ImageP0Bg02, new Vector2(x,y), Color.White);
            if (!background02 && down) sb.Draw(_tileTextures[item].ImageP1, new Vector2(x,y), Color.White);
            if (!background02 && !down) sb.Draw(_tileTextures[item].ImageP0, new Vector2(x,y), Color.White);
            x += 16;
            if (x > 16 * 63)
            {
                x = 0;
                y += 16;
            }
        }
    }
}