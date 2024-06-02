using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Color = Microsoft.Xna.Framework.Color;

namespace Engine.RomReader;

public class Monster
{
    public Monster(List<Texture2D> tiles, byte[] form, Vector2 pos, int size)
    {
        _tiles = tiles;
        _form = form;
        _pos = pos;
        _size = size;
    }

    private int _size;
    private List<Texture2D> _tiles;
    private byte[] _form;
    private Vector2 _pos;
    
    public void Draw(SpriteBatch sb, Vector2 pos)
    {
        if (_size == 64) Draw64x64(sb, pos);
        if (_size == 128) Draw128x128(sb, pos);
    }

    void Draw128x128(SpriteBatch sb, Vector2 pos)
    {
        int tileIdx = 0;
        for (int row = 0; row < 16; row++)
        {
            // Left half
            for (int col = 0; col < 16; col++)
            {
                if (HasTileAt128(col, row))
                {
                    //Console.Write(1);
                    sb.Draw(_tiles[tileIdx], pos + new Vector2(col*8,row*8), Color.White);
                    tileIdx++;
                }
                else
                {
                    //Console.Write(0);
                }
            }
            //Console.WriteLine();
        }

        int x = 0;
    }

    bool HasTileAt128(int x, int y)
    {
        // [0000_0000][1111_1111] 0  1
        // [1010_1010][0101_0101] 2  3
        // [1010_1010][0101_0101] 4  5
        // [1010_1010][0101_0101] 6  7
        // [0000_0000][1111_1111] 8  9
        // [1010_1010][0101_0101] 10 11
        // [1010_1010][0101_0101] 12 13
        // [1010_1010][0101_0101] 14 15
        // [0000_0000][1111_1111] 16 17
        // [1010_1010][0101_0101] 18 19
        // [1010_1010][0101_0101] 20 21
        // [1010_1010][0101_0101] 22 23
        // [0000_0000][1111_1111] 24 25
        // [1010_1010][0101_0101] 26 27
        // [1010_1010][0101_0101] 28 29
        // [1010_1010][0101_0101] 30 31
        

        bool isInLeftHalf = x < 8;
        if (isInLeftHalf) return ((_form[y*2] >> (7-x)) & 1) == 1;
        if (!isInLeftHalf) return ((_form[y*2 + 1] >> (15-x)) & 1) == 1;
       
        
        return false;
    }
    
    void Draw64x64(SpriteBatch sb, Vector2 pos)
    {
        int tileIdx = 0;
        for (int i = 0; i < _size/8; i++) // byte
        {
            for (int j = 7; j >= 0; j--) // bit
            {
                int hasTileHere = (_form[i] >> j) & 1;
                //Console.Write(hasTileHere);
                if (hasTileHere == 1)
                {
                    sb.Draw(_tiles[tileIdx], pos + new Vector2((7-j)*8,i*8), Color.White);
                    tileIdx++;
                }
            }
            //Console.WriteLine();
        }

        int x = 0;
    }
}