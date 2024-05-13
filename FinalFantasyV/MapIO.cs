using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace FinalFantasyV
{

    public class TiledProperty
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string Value { get; set; }
    }

    public class LayerData
    {
        public List<int> Data { get; set; }
        public int Height { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }
        public float Opacity { get; set; }
        public string Type { get; set; }
        public bool Visible { get; set; }
        public int Width { get; set; }
        public int X { get; set; }
        public int Y { get; set; }

        public List<TiledObject> Objects { get; set; }
        public List<TiledProperty> Properties { get; set; } 

        public int GetData(int x, int y) => Data[x + y * Width];
        public int GetData(Vector2 pos) => Data[(int)pos.X + (int)pos.Y * Width];
    }

    public class TiledObject
    {
        public int X { get; set; }
        public int Y { get; set; }
        public List<TiledProperty> Properties { get; set; }
    }

    public class Tileset
    {
        public int FirstGid { get; set; }
        public string Source { get; set; }
    }

    public class TilesetInfo
    {
        public SpriteSheet SpriteSheet { get; set; }
        public int Columns { get; set; }
        public string Image { get; set; }
        public int ImageHeight { get; set; }
        public int ImageWidth { get; set; }
        public int Margin { get; set; }
        public string Name { get; set; }
        public int Spacing { get; set; }
        public int TileCount { get; set; }
        public string TiledVersion { get; set; }
        public int TileHeight { get; set; }
        public int TileWidth { get; set; }
        public string Type { get; set; }
        public string Version { get; set; }
    }

    public struct MapChange
    {
        public int CurrTileX;
        public int CurrTileY;
        public int NewTileX;
        public int NewTileY;
        public string MapChangeName;
    }

    public class Map
    {
        public int CompressionLevel { get; set; }
        public int Height { get; set; }
        public bool Infinite { get; set; }
        public List<LayerData> Layers { get; set; }
        public int NextLayerId { get; set; }
        public int NextObjectId { get; set; }
        public string Orientation { get; set; }
        public string RenderOrder { get; set; }
        public string TiledVersion { get; set; }
        public int TileHeight { get; set; }
        public List<Tileset> TileSets { get; set; }
        public int TileWidth { get; set; }
        public string Type { get; set; }
        public string Version { get; set; }
        public int Width { get; set; }

        public List<MapChange> MapChanges { get; set; }

        public void SetTileAt(int layer, int x, int y, int tileID)
        {
            Layers[layer].Data[x + y * Width] = tileID;
        }

        public void SetLayerVisible(int layer, bool isVisible)
        {
            Layers[layer].Visible = isVisible;
        }

        public LayerData GetCollisionLayer() => Layers.SingleOrDefault(l => l.Name == "Collisions");
        public int GetIdOfUnwalkable()
        {
            var worldTileset = TileSets.SingleOrDefault(t => t.Source == "worldmap.tsj");
            return worldTileset.FirstGid + 23;
        }

        public void DrawTileSet(SpriteBatch sb, TilesetInfo ti)
        {
            for (int layer = 0; layer < Layers.Count; layer++)
            {
                if (!Layers[layer].Visible || Layers[layer].Type != "tilelayer" || Layers[layer].Name == "Collisions") continue;
                for (int x = 0; x < Width; x++)
                {
                    for (int y = 0; y < Height; y++)
                    {
                        var tileIndex = Layers[layer].Data[y * Width + x];
                        if (tileIndex == 0) continue;
                        tileIndex -= TileSets[0].FirstGid;
                        var tileSheetX = tileIndex % (ti.ImageWidth / ti.TileWidth);
                        var tileSheetY = tileIndex / (ti.ImageWidth / ti.TileWidth);
                        ti.SpriteSheet.Draw(sb, tileSheetX, tileSheetY, new Vector2(x * ti.TileWidth, y * ti.TileHeight));
                    }
                }
            }
        }
    }

    public class MapIO
	{
        static JsonSerializerOptions options = new() { PropertyNameCaseInsensitive = true, };

        public static TilesetInfo ReadTileSet(ContentManager cm, string file)
        {
            var text = File.ReadAllText(file);
            var tileset = JsonSerializer.Deserialize<TilesetInfo>(text, options);
            var s = tileset.Image.Split("/");
            var grpahicname = s[s.Length - 1].Split(".")[0];
            tileset.SpriteSheet = new SpriteSheet(cm.Load<Texture2D>(grpahicname), tileset.TileWidth, tileset.TileHeight, Vector2.Zero, Vector2.Zero);
            return tileset;
        }

        public static Map ReadMap(string file)
        {
            var map = JsonSerializer.Deserialize<Map>(File.ReadAllText(file), options);
            map.MapChanges = new();
            var warpLayer = map.Layers.SingleOrDefault(l => l.Name == "Warps");

            if (warpLayer is null) return map;

            foreach(var obj in warpLayer.Objects)
            {
                if (obj.Properties is null) continue;
                var mapChange = new MapChange
                {
                    CurrTileX = obj.X / 16,
                    CurrTileY = obj.Y / 16 - 1, //For some reason Tiled XY is bottom left
                    MapChangeName = obj.Properties[0].Value,
                    NewTileX = int.Parse(obj.Properties[1].Value),
                    NewTileY = int.Parse(obj.Properties[2].Value),
                };
                map.MapChanges.Add(mapChange);
            }

            return map;
        }
	}
}

