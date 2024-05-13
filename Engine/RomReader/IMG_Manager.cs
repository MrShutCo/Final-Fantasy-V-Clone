using System;
namespace Engine.RomReader
{
	public class IMG_Manager
	{
		public IMG_Manager()
		{
		}
	}

    public class Transformations
    {
        /* Offset 0x03EB00 */
        /* Size   0x0012C0 */
        public static Image transform1bpp(List<byte> byteMap, int offset, int size)
        {
            int maxY = (size * 16 * 8) / 2048;
            Image<Rgba32> newImage = new Image<Rgba32>(16 * 8, maxY);

            int i = offset;
            int end = i + size;

            int x = 0;
            int y = 0;
            byte mask = 0x80;

            while (i < end) for (int yi = 0; yi < 128 / 12; yi++)
                {
                    if (y + 12 > newImage.Height)
                    {
                        i = int.MaxValue;
                        break;
                    }

                    /* Draw a line of tiles (1b) */
                    for (int xi = 0; xi < 128 / 8; xi++)
                    {

                        /* Draw a tile (1b) */
                        for (int j = 0; j < 12; j++)
                        {
                            if (i >= byteMap.Count || i >= end)
                                return newImage;

                            byte pixel1b = byteMap[i];

                            for (int k = 0; k < 8; k++)
                            {
                                newImage[x, y] = ((pixel1b & mask) == 0) ? Palettes.palette1b[0] : Palettes.palette1b[1];
                                x++;
                                mask = (byte)(mask >> 1);
                                if (mask == 0)
                                    mask = 0x80;
                            }
                            x -= 8;
                            y++;
                            i++; //Next byte
                        }
                        x += 8;
                        y -= 12;

                    }
                    x = 0;
                    y += 12;
                }

            return newImage;
        }



        /* Offset 0x11F000 */
        /* Size   0x001000 */
        public static Image transform2bpp(List<byte> byteMap, int offset, int size)
        {
            int maxY = (int)((size * 16 * 8) / 4096);
            Image<Rgba32> newImage = new Image<Rgba32>(16 * 8, maxY);

            int i = offset;
            int end = i + size;

            int x = 0;
            int y = 0;
            int cIndex = 0;

            while (i < end) for (int yi = 0; yi < 128 / 8; yi++)
                {
                    if (y + 8 > newImage.Height)
                    {
                        i = int.MaxValue;
                        break;
                    }

                    /* Draw a line of tiles (2b,8x8) */
                    for (int xi = 0; xi < 128 / 8; xi++)
                    {

                        /* Draw a tile (2b,8x8) */
                        for (int j = 0; j < 8; j++)
                        {
                            if (i >= byteMap.Count - 1 || i >= end)
                                return newImage;

                            byte pixel2b_00 = byteMap[i++];
                            byte pixel2b_01 = byteMap[i++];
                            byte mask = 0x80;

                            for (int k = 0; k < 8; k++)
                            {
                                cIndex = ((pixel2b_00 & mask) == 0) ? 0 : 1;
                                cIndex += ((pixel2b_01 & mask) == 0) ? 0 : 2;
                                newImage[x, y] = Palettes.palette2b[cIndex];
                                x++;
                                mask = (byte)(mask >> 1);
                                if (mask == 0)
                                    mask = 0x80;
                            }
                            x -= 8;
                            y++;

                        }
                        x += 8;
                        y -= 8;

                    }
                    x = 0;
                    y += 8;
                }

            return newImage;
        }



        public static Image transform3bpp(List<byte> byteMap, int offset, int size)
        {
            int maxY = (int)((size * 16 * 8) / 6144);
            Image<Rgba32> newImage = new Image<Rgba32>(16 * 8, maxY);

            int i = offset;
            int end = i + size;

            int x = 0;
            int y = 0;
            int cIndex = 0;

            while (i < end) for (int yi = 0; yi < 128 / 8; yi++)
                {
                    if (y + 8 > newImage.Height)
                    {
                        i = int.MaxValue;
                        break;
                    }

                    /* Draw a line of tiles (3b,8x8) */
                    for (int xi = 0; xi < 128 / 8; xi++)
                    {

                        /* Draw a tile (3b,8x8) */
                        int l = i + 16;

                        for (int j = 0; j < 8; j++)
                        {
                            if (i >= byteMap.Count - 1 || i >= end)
                                return newImage;

                            byte pixel3b_00 = byteMap[i++];
                            byte pixel3b_01 = byteMap[i++];
                            byte pixel3b_02 = byteMap[l++];
                            byte mask = 0x80;

                            for (int k = 0; k < 8; k++)
                            {
                                cIndex = ((pixel3b_00 & mask) == 0) ? 0 : 1;
                                cIndex += ((pixel3b_01 & mask) == 0) ? 0 : 2;
                                cIndex += ((pixel3b_02 & mask) == 0) ? 0 : 4;
                                newImage[x, y] = Palettes.palette3b[cIndex];
                                x++;
                                mask = (byte)(mask >> 1);
                                if (mask == 0)
                                    mask = 0x80;
                            }
                            x -= 8;
                            y++;

                        }
                        i += 8;
                        x += 8;
                        y -= 8;

                    }
                    x = 0;
                    y += 8;
                }

            return newImage;
        }



        public static Image transform4b(List<byte> byteMap, int offset, int size, Color[] palette = null)
        {
            int maxY = (int)((size * 16 * 8) / 8192);
            Image<Rgba32> newImage = new Image<Rgba32>(16 * 8, maxY);
            if (palette == null) palette = Palettes.palette4b;

            int i = offset;
            int end = i + size;

            int x = 0;
            int y = 0;
            int cIndex = 0;
            int bpl23;

            while (i < end) for (int yi = 0; yi < 128 / 8; yi++)
                {
                    if (y + 8 > newImage.Height)
                    {
                        i = int.MaxValue;
                        break;
                    }

                    /* Draw a line of tiles (4b,8x8) */
                    for (int xi = 0; xi < 128 / 8; xi++)
                    {

                        /* Draw a tile (4b,8x8) */
                        bpl23 = i + 8 * 2;
                        for (int j = 0; j < 8; j++)
                        {
                            if (bpl23 >= byteMap.Count - 1 || bpl23 >= end)
                                return newImage;

                            byte pixel4b_00 = byteMap[i++];
                            byte pixel4b_01 = byteMap[i++];
                            byte pixel4b_02 = byteMap[bpl23++];
                            byte pixel4b_03 = byteMap[bpl23++];
                            byte mask = 0x80;

                            for (int k = 0; k < 8; k++)
                            {
                                cIndex = (pixel4b_00 & mask) == 0 ? 0 : 1;
                                cIndex += (pixel4b_01 & mask) == 0 ? 0 : 2;
                                cIndex += (pixel4b_02 & mask) == 0 ? 0 : 4;
                                cIndex += (pixel4b_03 & mask) == 0 ? 0 : 8;
                                newImage[x, y] = palette[cIndex];
                                //outBytes[x, y] = (byte)cIndex;
                                x++;
                                mask = (byte)(mask >> 1);
                                if (mask == 0)
                                    mask = 0x80;
                            }
                            x -= 8;
                            y++;

                        }
                        x += 8;
                        y -= 8;
                        i += 8 * 2; //Discard the 16 bytes of the bpl2 & bpl3 (previously readen)

                    }
                    x = 0;
                    y += 8;
                }

            return newImage;
        }



        public static Image<Rgba32> transform8bM7(List<byte> byteMap, int offset, List<Color> palette = null)
        {
            Image<Rgba32> newImage = new Image<Rgba32>(16 * 8, 16 * 8);
            
                if (palette == null)
                {
                    Palettes.palette8b = Palettes.initializePalette8b();
                    palette = Palettes.palette8b.ToList();
                }

                int i = offset;
                int x = 0;
                int y = 0;

                for (int yi = 0; yi < 16; yi++)
                {

                    /* Draw a line of tiles (8bM7,8x8) */
                    for (int xi = 0; xi < 16; xi++)
                    {

                        /* Draw a tile (8bM7,8x8) */
                        for (int yj = 0; yj < 8; yj++)
                        {
                            for (int xj = 0; xj < 8; xj++)
                            {
                                if (i >= byteMap.Count - 1)
                                    return newImage;
                                newImage[x, y] = palette[byteMap[i++]];
                                x++;
                            }
                            x -= 8;
                            y++;
                        }
                        x += 8;
                        y -= 8;
                    }
                    x = 0;
                    y += 8;
                }

                return newImage;
            
        }
    }
}

