using System;
using System.Text;

namespace Engine
{
	public class MapExtractor
	{
		public MapExtractor()
		{
		}

		static Dictionary<byte, char> mappings = new Dictionary<byte, char>()
		{
			{ 0x96, ' ' },
            { 0x97, '\'' },
            { 0x98, '\"' },
            { 0x99, ':' },
            { 0x9A, ';' },
            { 0x9B, ',' },
            { 0x9C, '(' },
            { 0x9D, ')' },
            { 0x9E, '/' },
        };

		public static void ReadMap(int mapID)
		{
			var offset = 0xC00000;

			if (mapID < 0 || mapID >= 512) return;
			var bytes = File.ReadAllBytes("FF5.sfc");


			for (int i = 0; i < 128; i++)
			{
				int startOfMap = 0xCE9C00 + i * 26 - offset;
				byte[] mapData = bytes[startOfMap..(startOfMap + 26)];

				var mapNameLoc = 0xE70000 + +(16*mapData[0x2]) - offset;
				var name = bytes[mapNameLoc..(mapNameLoc + 16)].Select(b => mapText(b)).ToArray();
				Console.WriteLine(Encoding.UTF8.GetString(name, 0, name.Length));
                Console.WriteLine(BitConverter.ToString(name));
				Console.WriteLine("=====");
            }
		}

		static byte mapText(byte b)
		{
			if (b >= 0x7A && b <= 0x93)
				return (byte)(b - 0x7A + 0x61);
			if (b >= 0x60 && b <= 0x79)
				return (byte)(b - 0x60 + 0x41);
			char ch;
			if (mappings.TryGetValue(b, out ch))
			{
				return (byte)ch;
			}
			return b;
		}
	}
}

