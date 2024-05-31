using System.Numerics;
using Microsoft.Xna.Framework.Graphics;
using Color = Microsoft.Xna.Framework.Color;
using Vector2 = Microsoft.Xna.Framework.Vector2;


namespace Engine.RomReader;

public class BackgroundLayers
{
    private readonly List<MapManager.TileTexture> _tileTextures;
    private readonly List<byte> _tilemap00;
    private readonly List<byte>? _tilemap01;
    private readonly List<byte>? _tilemap02;

    private bool _isWorldMap;
    
    public BackgroundLayers(){}
    
    public BackgroundLayers(List<MapManager.TileTexture> tileTextures, List<byte> tilemap00, List<byte>? tilemap01, List<byte>? tilemap02, bool isWorldMap)
    {
        _isWorldMap = isWorldMap;
        _tileTextures = tileTextures;
        _tilemap00 = tilemap00;
        _tilemap01 = tilemap01;
        _tilemap02 = tilemap02;
    }
    
    public void DrawBelowCharacter(SpriteBatch sb, Vector2 playerPosition)
    {
        DrawLayer(sb, _tilemap02, false, true, playerPosition);
        DrawLayer(sb, _tilemap02, true, true, playerPosition);
        DrawLayer(sb, _tilemap01, false, false, playerPosition);
        DrawLayer(sb, _tilemap00, false, false, playerPosition);
    }
    
    public void DrawAboveCharacter(SpriteBatch sb, Vector2 playerPosition)
    {
        DrawLayer(sb, _tilemap01, true, false, playerPosition);
        DrawLayer(sb, _tilemap00, true, false, playerPosition);
    }

    void DrawLayer(SpriteBatch sb, List<byte> tilemap, bool down, bool background02, Vector2 playerPosition)
    {
        var minDraw = playerPosition - new Vector2(160, 160);
        var maxDraw = playerPosition + new Vector2(160, 160);
        
        var (x, y) = (0, 0);
        int width = _isWorldMap ? 255 : 63;
        foreach (var item in tilemap)
        {
            if (x >= minDraw.X && x <= maxDraw.X && y >= minDraw.Y && y <= maxDraw.Y)
            {
                if (background02 && down) sb.Draw(_tileTextures[item].ImageP1Bg02, new Vector2(x, y), Color.White);
                if (background02 && !down) sb.Draw(_tileTextures[item].ImageP0Bg02, new Vector2(x, y), Color.White);
                if (!background02 && down) sb.Draw(_tileTextures[item].ImageP1, new Vector2(x, y), Color.White);
                if (!background02 && !down) sb.Draw(_tileTextures[item].ImageP0, new Vector2(x, y), Color.White);
            }

            x += 16;
            if (x > 16 * width)
            {
                x = 0;
                y += 16;
            }
        }
    }

    public void SetBackgroundTileAt(int layer, int x, int y, byte tile)
    {
        if (layer == 0) _tilemap00[y * 64 + x] = tile;
        if (layer == 1) _tilemap01[y * 64 + x] = tile;
        if (layer == 2) _tilemap02[y * 64 + x] = tile;
    }
}