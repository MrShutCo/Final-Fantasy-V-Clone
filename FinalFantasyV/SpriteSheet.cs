using System.Reflection.Metadata;
using Engine.RomReader;
using FinalFantasyV.Sprites;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Color = Microsoft.Xna.Framework.Color;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace FinalFantasyV
{
	public class SpriteSheet
	{
		Texture2D Texture;
		public int Width { get; init; }
        public int Height { get; init; }
        Vector2 offset;
		Vector2 tileSpacing;
        Rectangle frame;

		public int X { get; private set; }
		public int Y { get; private set; }

		int tileWidth;
		int tileHeight;

        public SpriteSheet(Texture2D texture, int width, int height, Vector2 offset, Vector2 tileSpacing)
		{
			Texture = texture;
			Width = width;
			Height = height;
            this.offset = offset;
            frame = new Rectangle(0+(int)offset.X,0+(int)offset.Y,Width, Height);
			tileWidth = (Texture.Width / Width);
			tileHeight = (Texture.Height / Height);
			this.tileSpacing = tileSpacing;

        }

		public void SetTile(int x, int y)
		{
			X = x;
			Y = y;
			frame.X = (int)(x * (Width + tileSpacing.X) + offset.X);
			frame.Y = (int)(y * (Height + tileSpacing.Y) + offset.Y);
		}


		public static Texture2D ConvertToTex(GraphicsDevice device, Image<Rgba32> image)
		{
			Texture2D tex = new Texture2D(device, image.Width, image.Height);

			var data = new Color[image.Width * image.Height];
			for (var x = 0; x < image.Width; x++)
			{
				for (var y = 0; y < image.Height; y++)
				{
					var color = image[x, y];
					data[y * image.Width + x] = new Color(color.R,color.G,color.B,color.A);
				}
			}
			
			tex.SetData(data);
			return tex;
		}

        public Vector2 IndexOf(int tile)
        {
			var x = tile % tileWidth;
			var y = tile / tileWidth;
			return new Vector2(x*(Width+tileSpacing.X), y*(Height+tileSpacing.Y));
        }

        public void Draw(SpriteBatch sb, Vector2 pos, bool isFlippedH = false, bool isFlippedV = false)
		{
			var se = isFlippedH ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
			se |= isFlippedV ? SpriteEffects.FlipVertically : SpriteEffects.None; 
			sb.Draw(Texture, pos, frame, Color.White, 0, Vector2.Zero, Vector2.One, se, 0);
		}

		public void Draw(SpriteBatch sb, Rectangle mask, Vector2 pos)
		{
			sb.Draw(Texture, pos, mask, Color.White);
		}
		
		public void Draw(SpriteBatch sb, Rectangle mask, Vector2 pos, Color color)
		{
			sb.Draw(Texture, pos, mask, color);
		}

		public void Draw(SpriteBatch sb, int tilesheetX, int tilesheetY, Vector2 pos)
		{
			sb.Draw(Texture, pos, new Rectangle((int)(tilesheetX * (Width+tileSpacing.X)), (int)(tilesheetY * (Height+tileSpacing.Y)), Width, Height), Color.White);
		}
		
		public static WorldCharacter[] LoadSprites(MapManager map)
		{
			var w = new WorldCharacter[map.Npcs.Count];
			
			for (int i = 0; i < map.Npcs.Count; i++)
			{
				var gid = map.Npcs[i].graphicId;
				var pallete = map.Npcs[i].palette;
				var tex = FF5.NPCTexture;
				
				Vector2 offset = Vector2.Zero;
				if (gid == 2) offset = new Vector2(0, 785);
				if (gid == 30) offset = new Vector2(0, 752-8);
				if (gid == 36) offset = new Vector2(0, 640 + 4);			 // Barkeep
				if (gid == 37) offset = new Vector2(120, 624);				// Red Sheep
				if (gid == 52) offset = new Vector2(144-2, 592-8);
				if (gid == 66 && pallete == 0) offset = new Vector2(140, 464); // Old man green
				if (gid == 66 && pallete == 1) offset = new Vector2(0, 464); // Old man purple
				if (gid == 40) offset = new Vector2(240, 656+8);		    // Green man
				if (gid == 51) offset = new Vector2(0, 176 + 8);			// Pirate
				if (gid == 64) offset = new Vector2(272 + 8, 416 + 8);		// Blue man 
				if (gid == 65 && pallete == 0) offset = new Vector2(272 + 8, 304); // Green woman
				if (gid == 65 && pallete == 1) offset = new Vector2(272 + 8, 344); // Purple woman
				if (gid == 69) offset = new Vector2(140, 384);							// Dancer
				if (gid == 38 && pallete == 0) offset = new Vector2(120,644);			// Girl brown
				if (gid == 38 && pallete == 1) offset = new Vector2(120,664);			// Girl blue
				if (gid == 68) offset = new Vector2(280, 192 - 8);					// Blue knight
				if (gid == 70) offset = new Vector2(280, 224);				// Orange Knight
				if (gid == 57 && pallete == 1) offset = new Vector2(0, 408);				// Grey wolf
				if (gid == 56 && pallete == 1) offset = new Vector2(140, 304);	// Chancellor
				if (gid == 71) offset = new Vector2(280,256+8);				// Cid
				if (gid == 72) offset = new Vector2(280, 384);				// Mid
				if (gid == 50) offset = new Vector2(0, 304); // King
				if (gid == 67 && pallete == 0) offset = new Vector2(0, 344); // Orange Scholar
				if (gid == 67 && pallete == 1) offset = new Vector2(140, 344); // Blue Scholar
				if (gid == 61) offset = new Vector2(241, 624); // Turtle
				if (gid == 77)
				{
					offset = new Vector2(365, 452);
					tex = FF5.Bartz;
				}
				if (gid == 78)
				{
					offset = new Vector2(365 - 1, 452 - 7);
					tex = FF5.Lenna;
				}
				if (gid == 79)
				{
					offset = new Vector2(365 - 4, 452 - 13);
					tex = FF5.Galuf;
				}
				if (gid == 80)
				{
					offset = new Vector2(365, 452 - 11);
					tex = FF5.Faris;
				}
				
				Vector2 pos = new Vector2(map.Npcs[i].x * 16, map.Npcs[i].y * 16);
				
				w[i] = new WorldNPC(new SpriteSheet(tex, 16, 16, offset, new Vector2(4,4)), pos, map.Npcs[i]);

				//if (gid >= 78 && gid <= 80) w[i].IsVisible = false;
				if (!map.Npcs[i].IsVisibleOnStartup())
				{
					w[i].IsVisible = false;
				}
				
				//if (map.Npcs[i].direction == 0) w[i].Face(ECharacterMove.Up);
				//if (map.Npcs[i].direction == 1) w[i].Face(ECharacterMove.Right);
				//if (map.Npcs[i].direction == 2) w[i].Face(ECharacterMove.Down);
				//if (map.Npcs[i].direction == 3) w[i].Face(ECharacterMove.Left);
				w[i].Face((ECharacterMove)map.Npcs[i].direction);
			}

			return w;
		}
	}
}

