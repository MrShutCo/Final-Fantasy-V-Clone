using System;
namespace Engine.RomReader
{
    public static class Palettes
    {
        public static Color[] palette1b = new Color[2]{
            Color.FromRgb(0x00,0x00,0xC1),
            Color.FromRgb(0xFF,0xFF,0xFF)
        };

        public static Color[] palette2b = new Color[4]{
            Color.FromRgb(0x00,0x00,0x00),
            Color.FromRgb(0x00,0x00,0x8B),
            Color.FromRgb(0xAD,0xD8,0xE6),
            Color.FromRgb(0xFF,0xFF,0xFF)
        };

        public static Color[] palette3b = new Color[8]{
            Color.FromRgb(0x00,0x00,0xFF),
            Color.FromRgb(0x20,0x20,0x20),
            Color.FromRgb(0x40,0x40,0x40),
            Color.FromRgb(0x60,0x60,0x60),
            Color.FromRgb(0x80,0x80,0x80),
            Color.FromRgb(0xA0,0xA0,0xA0),
            Color.FromRgb(0xC0,0xC0,0xC0),
            Color.FromRgb(0xFF,0xFF,0xFF)
        };

        public static Color[] palette4b = new Color[16]{
            Color.FromRgb(0x00,0x00,0x00),
            Color.FromRgb(0x11,0x11,0x11),
            Color.FromRgb(0x22,0x22,0x22),
            Color.FromRgb(0x33,0x33,0x33),

            Color.FromRgb(0x44,0x44,0x44),
            Color.FromRgb(0x55,0x55,0x55),
            Color.FromRgb(0x66,0x66,0x66),
            Color.FromRgb(0x77,0x77,0x77),

            Color.FromRgb(0x88,0x88,0x88),
            Color.FromRgb(0x99,0x99,0x99),
            Color.FromRgb(0xAA,0xAA,0xAA),
            Color.FromRgb(0xBB,0xBB,0xBB),

            Color.FromRgb(0xCC,0xCC,0xCC),
            Color.FromRgb(0xDD,0xDD,0xDD),
            Color.FromRgb(0xEE,0xEE,0xEE),
            Color.FromRgb(0xFF,0xFF,0xFF)
        };

        public static Color[] palette8b;

        public static Color[] initializePalette8b()
        {
            Color[] output = new Color[256];

            for (byte k = 0; k < 16; k++)
            {
                output[k + 0x00] = Color.FromRgb((byte)(k * 16), (byte)(k * 16), (byte)(k * 16));
                output[k + 0x10] = Color.FromRgb((byte)(k * 16), (byte)(k * 00), (byte)(k * 00));
                output[k + 0x20] = Color.FromRgb((byte)(k * 00), (byte)(k * 16), (byte)(k * 00));
                output[k + 0x30] = Color.FromRgb((byte)(k * 00), (byte)(k * 00), (byte)(k * 16));
                output[k + 0x40] = Color.FromRgb((byte)(k * 16), (byte)(k * 16), (byte)(k * 00));
                output[k + 0x50] = Color.FromRgb((byte)(k * 16), (byte)(k * 00), (byte)(k * 16));
                output[k + 0x60] = Color.FromRgb((byte)(k * 00), (byte)(k * 16), (byte)(k * 16));
                output[k + 0x70] = Color.FromRgb((byte)(k * 16), (byte)(k * 16), (byte)(k * 08));
                output[k + 0x80] = Color.FromRgb((byte)(k * 16), (byte)(k * 08), (byte)(k * 16));
                output[k + 0x90] = Color.FromRgb((byte)(k * 08), (byte)(k * 16), (byte)(k * 16));
                output[k + 0xA0] = Color.FromRgb((byte)(k * 16), (byte)(k * 08), (byte)(k * 08));
                output[k + 0xB0] = Color.FromRgb((byte)(k * 08), (byte)(k * 16), (byte)(k * 08));
                output[k + 0xC0] = Color.FromRgb((byte)(k * 08), (byte)(k * 08), (byte)(k * 16));
                output[k + 0xD0] = Color.FromRgb((byte)(k * 04), (byte)(k * 08), (byte)(k * 16));
                output[k + 0xE0] = Color.FromRgb((byte)(k * 16), (byte)(k * 04), (byte)(k * 08));
                output[k + 0xF0] = Color.FromRgb((byte)(k * 08), (byte)(k * 16), (byte)(k * 04));
            }

            return output;
        }
    }
}

