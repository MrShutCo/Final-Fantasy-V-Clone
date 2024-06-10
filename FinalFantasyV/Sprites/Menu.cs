using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FinalFantasyV.Sprites
{
	public class Menu
	{

		public const int NewLineIndex = 1000;
		
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


        public static void DrawString(SpriteBatch sb, SpriteSheet menuData, string text, Vector2 startingPosition, bool ltr = true)
        {
	        var tiles = GetTilesForText(text);
	        var startingX = startingPosition.X;
	        for (int i = 0; i < tiles.Count; i++)
	        {
		        var index = ltr ? i : tiles.Count - 1 - i;
		        if (tiles[index] == NewLineIndex)
		        {
			        startingPosition.X = startingX;
			        startingPosition.Y += 16;
			        continue;
		        }
		        var startingCoord = menuData.IndexOf(tiles[index]);
		        menuData.Draw(sb, new Rectangle((int)startingCoord.X, (int)startingCoord.Y, menuData.Width, menuData.Height), startingPosition);
		        startingPosition.X += ltr ? 8 : -8;
	        }
		}

		public static void DrawManyString(SpriteBatch sb, SpriteSheet menuData, string[] text, Vector2 startingPosition, int yDelta, bool ltr = true)
		{
			for (int i = 0; i < text.Length; i++)
			{
				DrawString(sb, menuData, text[i], startingPosition + new Vector2(0, yDelta * i), ltr);
			}
		}

		public static string PadNumber(int num, int pads) => num.ToString().PadLeft(pads, ' ');

		public static void DrawText(Map map, int x, int y, string text, int layer = 0, bool isLeftToRight = true)
		{
			var offset = 0;
			var tiles = GetTilesForText(text);
			if (isLeftToRight)
			{
				for (int i = 0; i < tiles.Count; i++)
				{
					map.SetTileAt(layer, x + offset, y, tiles[i] + 1);
					offset++;
				}
			}
			else
			{
				offset = tiles.Count - 1;
				for (int i = tiles.Count; i >= 0; i--)
				{
					map.SetTileAt(layer, x + offset, y, tiles[i] + 1);
					offset--;
				}
			}
		}

		public static void DrawATBBar(Map map, int x, int y, byte atb)
		{
			map.SetTileAt(0, x, y, 11);
			map.SetTileAt(0, x+5, y, 12);
			
			for (int i = 0; i < 4; i++)
			{
				if (atb < i*64) map.SetTileAt(0, x+i+1, y, 13);
				else
				{
					// atb >= i*64
					var amount = Math.Min((atb - i * 64) / 8, 7);
					map.SetTileAt(0, x+1+i, y, 216+amount);
				}
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

			text = text.Replace("\t", "    ");
			text = text.Replace("[FF]", "");
			text = text.Replace("(Bartz)", "Bartz");
			text = text.Replace("[Wait]", ""); // TODO: tyler add an actual wait in here
			
			for (; i < text.Length; i++)
			{
				int index = 0;
				int skipCount = 0;
				if (i < text.Length - 1)
					index = CheckForLetterGrouping(text[i..(i + 2)]);

				if (i < text.Length - 6 && text[i..(i + 5)] == "[EOL]")
				{
					index = NewLineIndex;
					i += 4;
				}

				if (index == 0)
					index = getSpriteSheetIndex(text[i]);
				else if (index != NewLineIndex)
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

		private static List<char> Chars = ['\'', '"', ':', ';', ',', '(',')','/','!','?','.','%','{','}'];
		private static List<int> CharIndices = [121,122,123,124,125,126,127,128,129,130,131,173, 126,127];
		
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
			var idx = Chars.IndexOf(ch);
			if (idx != -1)
				coord = CharIndices[idx];
			
			return coord;
        }
		
	}
}

