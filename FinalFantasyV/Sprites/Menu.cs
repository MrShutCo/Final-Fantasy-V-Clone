﻿using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FinalFantasyV.Sprites
{
	public class Menu
	{

		private static Dictionary<string, int> itemIcons = new()
		{
			{"[Swrd]", 195}, {"[Shld]", 209}, {"[Knif]", 199}, {"[Shoe]", 157}, {"[Suit]", 163}, {"[Armr]", 211}, {"[Helm]", 210},
			{"[Ring]", 212}
			
		};

		public void SetString(Map map, int layer, string text, int X, int Y)
		{
			int offset = 0;
			foreach (var ch in text)
			{
				map.SetTileAt(layer, X + offset, Y, getSpriteSheetIndex(ch)+1);
				offset++;
            }
		}


        public static void DrawString(SpriteBatch sb, SpriteSheet menuData, string text, Vector2 startingPosition)
        {
	        var tiles = GetTilesForText(text);
	        for (int i = 0; i < tiles.Count; i++)
	        {
		        var startingCoord = menuData.IndexOf(tiles[i]);
		        menuData.Draw(sb, new Rectangle((int)startingCoord.X, (int)startingCoord.Y, menuData.Width, menuData.Height), startingPosition);
		        startingPosition.X += 8;
	        }
		}

		public static void DrawManyString(SpriteBatch sb, SpriteSheet menuData, string[] text, Vector2 startingPosition, int yDelta)
		{
			for (int i = 0; i < text.Length; i++)
			{
				DrawString(sb, menuData, text[i], startingPosition + new Vector2(0, yDelta * i));
			}
		}

		public static string PadNumber(int num, int pads) => num.ToString().PadLeft(pads, ' ');

		public static void DrawText(Map map, int x, int y, string text)
		{
			var offset = 0;
			var tiles = GetTilesForText(text);
			for (int i = 0; i < tiles.Count; i++)
			{
				map.SetTileAt(0, x+offset, y, tiles[i]+1);
				offset++;
			}
		}

		static List<int> GetTilesForText(string text)
		{
			List<int> tiles = new List<int>();

			int i = 0;
			// Perform checks for items
			foreach (var item in itemIcons)
			{
				if (text.Contains(item.Key))
				{ 
					tiles.Add(item.Value);
					i += item.Key.Length;
				}
			}

			text = text.Replace("[FF]", "");
			
			
			for (; i < text.Length; i++)
			{
				int index = 0;
				int skipCount = 0;
				if (i < text.Length - 1)
					index = CheckForLetterGrouping(text[i..(i + 2)]);

				if (index == 0)
					index = getSpriteSheetIndex(text[i]);
				else
					skipCount = 1;
				
				tiles.Add(index);
				i += skipCount;
			}

			return tiles;
		}

		public static void DrawManyText(Map map, int x, int y, int height, string[] texts)
		{
			int offset = 0;
			foreach (var text in texts)
			{
				DrawText(map, x, y + offset, text);
				offset += height;
			}
		}

		public static void SetBox(Map map, int startingX, int startingY, int width, int height, int layer = 0)
		{
            // Set the 4 corner tiles
            map.SetTileAt(layer, startingX, startingY, 2);
            map.SetTileAt(layer, startingX+width-1, startingY, 4);
            map.SetTileAt(layer, startingX, startingY+height-1, 7);
            map.SetTileAt(layer, startingX+width-1, startingY+height-1, 9);

			for (int i = 1; i < width-1; i++)
			{
				map.SetTileAt(layer, startingX + i, startingY, 3); // Top border
				map.SetTileAt(layer, startingX + i, startingY + height-1, 8); // Bottom border
			}
            for (int i = 1; i < height - 1; i++)
            {
                map.SetTileAt(layer, startingX, startingY + i, 5); // Left border
                map.SetTileAt(layer, startingX + width-1, startingY + i, 6); // Right border
            }
			for (int x = startingX + 1; x < startingX + width - 1; x++)
				for (int y = startingY + 1; y < startingY + height - 1; y++)
					map.SetTileAt(layer, x, y, 10);
        }

		static int CheckForLetterGrouping(string s)
		{
			if (s == "il") return 116;
			if (s == "it") return 117;
			if (s == "li") return 119;
			if (s == "ll") return 120;
			if (s == "ti") return 132;
			if (s == "fi") return 133;
			if (s == "if") return 140;
			if (s == "lt") return 141;
			if (s == "tl") return 142;
			if (s == "ir") return 143;
			return 0;
		}
		
		static int getSpriteSheetIndex(char ch)
		{
            int coord = 0;
			if (char.IsLower(ch))
				coord = ch - 97 + 90;
			else if (char.IsUpper(ch))
				coord = ch - 65 + 64;
			else if (ch == ' ')
				coord = 9;
			else if (char.IsNumber(ch))
				coord = ch - 48 + 51;
			else if (ch == '/') coord = 128;
			else if (ch == '.') coord = 131;
			else if (ch == ':') coord = 123;
			else if (ch == '%') coord = 173;
			return coord;
        }
		
	}
}
