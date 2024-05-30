namespace Engine.RomReader;

public class tile16x16
{
    public tile8x8 tile00;
    public tile8x8 tile01;
    public tile8x8 tile02;
    public tile8x8 tile03;

    public tile8x8 tile00Bg02;
    public tile8x8 tile01Bg02;
    public tile8x8 tile02Bg02;
    public tile8x8 tile03Bg02;

    public Image<Rgba32> ImageP0;
    public Image<Rgba32> ImageP1;

    public Image<Rgba32> ImageP0Bg02;
    public Image<Rgba32> ImageP1Bg02;



    /**
        * Constructor
        * 
        * @param byte00: byte 0 of the 8x8 tile 0 (upper left)
        * @param byte01: byte 1 of the 8x8 tile 0 (upper left)
        * @param byte10: byte 0 of the 8x8 tile 1 (upper right)
        * @param byte11: byte 1 of the 8x8 tile 1 (upper right)
        * @param byte20: byte 0 of the 8x8 tile 2 (bottom left)
        * @param byte21: byte 1 of the 8x8 tile 2 (bottom left)
        * @param byte30: byte 0 of the 8x8 tile 3 (bottom right)
        * @param byte41: byte 1 of the 8x8 tile 3 (bottom right)
        * @param tileset: Collection of 8x8 tile Images
        * @param inputPalette: Collection background palettes
        */
    public tile16x16(byte byte00, byte byte01, byte byte10, byte byte11,
        byte byte20, byte byte21, byte byte30, byte byte31,
        List<Image> tileset, List<Color> inputPalette)
    {
        tile00 = new tile8x8(byte00, byte01);
        tile01 = new tile8x8(byte10, byte11);
        tile02 = new tile8x8(byte20, byte21);
        tile03 = new tile8x8(byte30, byte31);

        tile00Bg02 = new tile8x8(byte00, (byte)(byte01 | 0x03));
        tile01Bg02 = new tile8x8(byte10, (byte)(byte11 | 0x03));
        tile02Bg02 = new tile8x8(byte20, (byte)(byte21 | 0x03));
        tile03Bg02 = new tile8x8(byte30, (byte)(byte31 | 0x03));

        ImageP0 = new Image<Rgba32>(16, 16);
        ImageP1 = new Image<Rgba32>(16, 16);

        ImageP0Bg02 = new Image<Rgba32>(16, 16);
        ImageP1Bg02 = new Image<Rgba32>(16, 16);

        if (!tile00.getPriority())
        {
            tile00.draw(0, 0, ImageP0, tileset, inputPalette);
            tile00Bg02.draw(0, 0, ImageP0Bg02, tileset, inputPalette, true);
        }
        else
        {
            tile00.draw(0, 0, ImageP1, tileset, inputPalette);
            tile00Bg02.draw(0, 0, ImageP1Bg02, tileset, inputPalette, true);
        }

        if (!tile01.getPriority())
        {
            tile01.draw(8, 0, ImageP0, tileset, inputPalette);
            tile01Bg02.draw(8, 0, ImageP0Bg02, tileset, inputPalette, true);
        }
        else
        {
            tile01.draw(8, 0, ImageP1, tileset, inputPalette);
            tile01Bg02.draw(8, 0, ImageP1Bg02, tileset, inputPalette, true);
        }

        if (!tile02.getPriority())
        {
            tile02.draw(0, 8, ImageP0, tileset, inputPalette);
            tile02Bg02.draw(0, 8, ImageP0Bg02, tileset, inputPalette, true);
        }
        else
        {
            tile02.draw(0, 8, ImageP1, tileset, inputPalette);
            tile02Bg02.draw(0, 8, ImageP1Bg02, tileset, inputPalette, true);
        }

        if (!tile03.getPriority())
        {
            tile03.draw(8, 8, ImageP0, tileset, inputPalette);
            tile03Bg02.draw(8, 8, ImageP0Bg02, tileset, inputPalette, true);
        }
        else
        {
            tile03.draw(8, 8, ImageP1, tileset, inputPalette);
            tile03Bg02.draw(8, 8, ImageP1Bg02, tileset, inputPalette, true);
        }
    }



    /**
        * draw
        * 
        * Draw the tile in a given Image.
        *
        * @param x: X coordinate of the 16x16 tile to draw.
        * @param y: Y coordinate of the 16x16 tile to draw.
        * @param output: The Image picture representing the tile to draw on.
        * @param priority: The priority of the layer ('up' is drawn over 'down').
        * @param background02: Is the layer a 'background02'? (4 color per tile instead of 16 color per tile).
        */
    public void Draw(int x, int y, Image<Rgba32> output, bool priority, bool background02)
    {
        output.Mutate(g =>
        {
            if (background02)
                if (priority)
                    g.DrawImage(ImageP1Bg02, new Point(x,y), 1);
                else
                    g.DrawImage(ImageP0Bg02, new Point(x,y), 1);
            else
            if (priority)
                g.DrawImage(ImageP1, new Point(x,y), 1);
            else
                g.DrawImage(ImageP0, new Point(x,y), 1);
        });
    }

}