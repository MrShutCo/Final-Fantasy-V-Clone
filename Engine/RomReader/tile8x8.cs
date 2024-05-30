namespace Engine.RomReader;

public class tile8x8 : ICloneable
{
    /** 
        
        2 bytes
        ------------------------------------------ ------------------------------------------
        I7 I6 I5 I4 I3 I2 I1 I0 | V0 H0 R0 P2 P1 P0 I9 I8

        I = Background tile id.
        V = Vertical flip.
        H = Horizontal flip.
        R = Tile priority.
        P = Tile palette.

        */

    int id = 0;
    bool hFlip = false;
    bool vFlip = false;
    bool priority = false;
    int palette = 0;
    byte byte00 = 0;
    byte byte01 = 0;



    /**
        * Constructor
        * 
        * @param byte00: Properties byte 0
        * @param byte01: Properties byte 1
        */
    public tile8x8(byte byte00, byte byte01)
    {
        this.byte00 = byte00;
        this.byte01 = byte01;
        id = byte00 + ((byte01 & 0x03) * 0x0100);
        vFlip = (byte01 & 0x80) != 0;
        hFlip = (byte01 & 0x40) != 0;
        priority = (byte01 & 0x20) != 0;
        palette = (byte01 & 0x1C) >> 2;
    }



    /**
        * ToString
        * 
        * @return this 8x8 tile data properly formatted
        */
    public string ToString()
    {
        string output = "";

        output += "id      = " + id.ToString("X4") + "\r\n";
        output += "hFlip   = " + (hFlip ? "true" : "false") + "\r\n";
        output += "vFlip   = " + (vFlip ? "true" : "false") + "\r\n";
        output += "prior   = " + (priority ? "true" : "false") + "\r\n";
        output += "palette = " + palette.ToString("X2") + "\r\n";

        return output;
    }

    /**
        * calcGraph
        * 
        * Returns the picture representing this 8x8 tile.
        * 
        * @param tileset: Current tileset.
        * @param inputPalette: Current background palette.
        * @param background02: The layer is a 'background02' (4 color per tile instead of 16 color per tile).
        * 
        * @return this 8x8 tile Image
        */
    public Image calcGraph(List<Image> tileset, List<Color> inputPalette, bool background02 = false)
    {
        List<Color> palette = inputPalette;
        for (int i = 0; i < 0x080; i += 0x10)
        {
            palette[i] = Color.FromRgba(0xFF, 0x00, 0x00, 0x00);
        }

        Image<Rgba32> auxiliar = new Image<Rgba32>(8, 8);

        int index = 0;

        Dictionary<Rgba32, Rgba32> colorMap = new();

        if (!background02)
        {
            foreach (Color item in Palettes.palette4b)
            {
                colorMap[item] = palette[(this.palette * 16) + index];
                index++;
            }
        }
        else
        {
            foreach (Color item in Palettes.palette2b)
            {
                colorMap[item] = palette[this.palette * 4 + index];
                index++;
            }

            colorMap[Palettes.palette2b[0]] = Color.FromRgba(0xFF, 0x00, 0x00, 0x00);
        }

        //ImageAttributes imageAttributes = new ImageAttributes();
        //imageAttributes.SetRemapTable(colorMap, ColorAdjustType.Image);

        if (id < tileset.Count)
        {
            //Graphics g = Graphics.FromImage(auxiliar);
            auxiliar.Mutate(g =>
            {
                g.DrawImage(tileset[id], new Rectangle(0, 0, 8, 8), 1);
                for (int x = 0; x < auxiliar.Width; x++)
                {
                    for (int y = 0; y < auxiliar.Width; y++)
                    {
                        Rgba32 val;
                        if (colorMap.TryGetValue(auxiliar[x, y], out val))
                            auxiliar[x, y] = colorMap[auxiliar[x, y]];

                    }
                }
            });
        }

        auxiliar.Mutate(g =>
        {
            if (hFlip & !vFlip)
            {
                g.RotateFlip(RotateMode.None, FlipMode.Horizontal);
            }
            else if (!hFlip & vFlip)
            {
                g.RotateFlip(RotateMode.None, FlipMode.Vertical);
            }
            else if (hFlip & vFlip)
            {
                g.RotateFlip(RotateMode.None, FlipMode.Vertical);
                g.RotateFlip(RotateMode.None, FlipMode.Horizontal);
            }
        });
        // Copy here, taking flips into account
            

        return auxiliar;
    }



    /**
        * draw
        * 
        * Draw the tile in a given Image.
        *
        * @param x: X coordinate of the 16x16 tile to draw.
        * @param y: Y coordinate of the 16x16 tile to draw.
        * @param output: The Image picture representing the tile to draw on.
        * @param tileset: Current tileset.
        * @param inputPalette: Current background palette.
        * @param background02: Is the layer to draw a 'background02'? (4 color per tile instead of 16 color per tile).
        */
    public void draw(int x, int y, Image output, List<Image> tileset, List<Color> inputPalette, bool background02 = false)
    {
        Image tileImage = calcGraph(tileset, inputPalette, background02);

        output.Mutate(g =>
        {
            g.DrawImage(tileImage, new Point(x,y), 1);
        });
    }



    // Getter
    public bool getPriority() { return priority; }



    // Overrides
    public object Clone()
    {
        return new tile8x8(byte00, byte01);
    }
}