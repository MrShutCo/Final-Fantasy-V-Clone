/**
* |------------------------------------------|
* | FF5e_Map_editor                          |
* | File: MAP_Manager.cs                     |
* | v0.87, September 2016                    |
* | Author: noisecross                       |
* |------------------------------------------|
* 
* @author noisecross
* @version 0.87
* 
*/



using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Microsoft.Xna.Framework.Graphics;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.ColorSpaces;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using Path = System.IO.Path;

namespace Engine.RomReader
{
    public class MapManager
    {



        #region Attributes



        // Locations descriptors at CE/9C00. 26 bytes per location
        List<List<byte>> _mapDescriptors   = new List<List<byte>>();
        
        // Tileblocks at CF/0000. Sets of 256 tiles16x16 including graphic id, palette id and properties about the tile.
        // CF/0000 Tile blocks offsets [2 bytes * 1C]
        // CF/0038 Tile blocks [Compressed unheaded type02] [2 bytes * (256 * 4)]
        List<int> _tileBlockOffsets = new();
        List<List<byte>> _tileBlocks  = new();
        
        // Tilemaps at CB/0000 storing the tile map of one background layer in one location
        // CB/0000 Tile maps offsets [2bytes * 0x0148]
        // CB/0290 Tile maps [Compressed unheaded type02] [1byte * 64 * 64] [Until 0DFDFF]
        List<long>       _tileMapsOffsets = new List<long>();
        List<List<byte>> _tileMaps        = new List<List<byte>>();
        List<long>       _tileMapSizes    = new List<long>();
        long             _tileMapSumSizes = 0;
        List<int>        _badTilemaps     = new List<int>();
        
        // Background palettes at C3/BB00
        // 03BB00 palettes [0x2B sets of * 2bytes * 0x080 subpalettes]
        List<List<Color>> _bgPalettes = new List<List<Color>>();

        // Sprite palettes at DF/FC00
        // DFFC00 palette [0x04 sets of * 2bytes * 0x010 subpalettes]
        List<List<Color>> _spritePalettes = new List<List<Color>>();

        // Current map tiles (grid of 64 16x16 tiles meaning a list of 128 x 128 tile8x8)
        public List<List<tile8x8>> TilesBg0 = new List<List<tile8x8>>();
        public List<List<tile8x8>> TilesBg1 = new List<List<tile8x8>>();
        public List<List<tile8x8>> TilesBg2 = new List<List<tile8x8>>();

        // Current map tileBlocks (graphic) Images
        List<Image> _bgTileSet = new List<Image>();

        // Current map tileBlocks tiles16x16
        public List<tile16x16> Tiles16X16 = new List<tile16x16>();

        // Current map background (byte) tilemaps
        public List<byte> Tilemap00 = new List<byte>();
        public List<byte> Tilemap01 = new List<byte>();
        public List<byte> Tilemap02 = new List<byte>();

        // Current map tile properties
        public List<byte> TilePropertiesL = new List<byte>();
        public List<byte> TilePropertiesH = new List<byte>();
        public List<byte> TileProperties  = new List<byte>();

        public byte CurrentTransparency = 0x00;

        // Texts
        List<int> _speechPtr               = new List<int>();
        List<List<byte>> _speech           = new List<List<byte>>();
        List<string> _speechTxt            = new List<string>();
        public List<string> ItemNames     = new List<string>();
        public List<string> SpellNames    = new List<string>();
        List<string> _locationNames        = new List<string>();
        List<string> _monsterNames         = new List<string>();
        public List<string> Encounters    = new List<string>();
        public List<string> MonsterGroups = new List<string>();

        // World Maps data
        List<List<byte>> _worldMap2X2Blocks   = new List<List<byte>>();
        List<List<Color>> _worldMapBgPalettes = new List<List<Color>>();
        public Image<Rgba32> WorldmapTileset        = new Image<Rgba32>(1, 1);
        List<List<byte>> _worldmapTileMaps    = new List<List<byte>>();

        // Sprites
        List<List<List<Image>>> _sprites = new List<List<List<Image>>>();

        // Monster zones
        List<byte>             _monsterZones         = new List<byte>();
        List<List<List<byte>>> _monsterWolrdMapZones = new List<List<List<byte>>>();

        // Current map Non Playable Characters, Exits, Chests and Events
        public List<NPC>      Npcs      = new List<NPC>();
        public List<MapExit>  Exits     = new List<MapExit>();
        public List<Treasure> Treasures = new List<Treasure>();
        public List<Event>    Events    = new List<Event>();

        // Current map tileBlocks (graphic) Images
        List<Image> _charSet = new List<Image>();
        


        #endregion



        //Class constructor
        public MapManager(BinaryReader br, int headerOffset, List<string> speechTxt, List<string> itemNames, List<string> spellNames, List<string> locationNames, List<string> monsterNames)
        {
            // Load map descriptors
            br.BaseStream.Position = 0x0E9C00 + headerOffset;
            for (int i = 0; i < 0x0200; i++)
            {
                _mapDescriptors.Add(br.ReadBytes(0x1A).ToList());
            }

            //long size = 0;
            //br.BaseStream.Position = 0x1841D5 + headerOffset;
            //List<byte> bmp = Compressor.uncompressType02(br, out size, false, true);
            //Image tileset = Transformations.transform4b(bmp, 0, bmp.Count);
            //ManageBMP.exportBPM("Pollo", tileset);

            // Initialize tile blocks
            InitTileblocks(br, headerOffset);

            // Initialize tilemaps
            InitTileMaps(br, headerOffset);

            // Initialize palettes
            InitPalettes(br, headerOffset);

            // Initialize world map structures
            InitWorldMap(br, headerOffset);

            // Initialize the NPC sprites
            InitSprites(br, headerOffset);

            // Initialize text data
            LoadTextData(speechTxt, itemNames, spellNames, locationNames, monsterNames);

            // Initialize the Encounters
            InitEncounters(br, headerOffset);
        }



        #region Initializers



        /**
        * initEncounters
        * 
        * Load all the game Encounters data.
        *
        * @param br: The reader to load the data from.
        * @param headerOffset: The offset due to a header in the ROM.
        */
        private void InitEncounters(BinaryReader br, int headerOffset)
        {
            Encounters           = new List<string>();
            MonsterGroups        = new List<string>();
            _monsterZones         = new List<byte>();
            _monsterWolrdMapZones = new List<List<List<byte>>>();

            // D0/3000-D0/4FFF	Data	Monster Encounter (512*16)
            //   0-2    [...]
            //   3  	Visible monsters (bitwise)
            //   4-B	Monsters x8
            //   C-D	Palettes 
            //   E-F	Music
            br.BaseStream.Position = 0x103000 + headerOffset;
            for (int i = 0; i < 0x0200; i++)
            {
                byte[] encounter = br.ReadBytes(0x10);
                string encounterData = "";

                for (int j = 0x04; j < 0x0C ; j++)
                {
                    if (encounter[j] < 0xFF) encounterData += _monsterNames[encounter[j]] + ", ";
                }

                encounterData = encounterData.Substring(0, encounterData.Length - 2);
                Encounters.Add(encounterData);
            }


            // D0/6800-D0/6FFF	Data	Monster groups (256 * 4*2bytes indices to Monster Encounter)
            br.BaseStream.Position = 0x106800 + headerOffset;
            for (int i = 0; i < 0x0100; i++)
            {
                string groupData = "";

                int formation01 = br.ReadByte() + br.ReadByte() * 0x0100;
                int formation02 = br.ReadByte() + br.ReadByte() * 0x0100;
                int formation03 = br.ReadByte() + br.ReadByte() * 0x0100;
                int formation04 = br.ReadByte() + br.ReadByte() * 0x0100;

                groupData += "F1 (35,1%) : (" + formation01.ToString("X3") + ") " + Encounters[formation01] + "\r\n";
                groupData += "F2 (35,1%) : (" + formation02.ToString("X3") + ") " + Encounters[formation02] + "\r\n";
                groupData += "F3 (23,5%) : (" + formation03.ToString("X3") + ") " + Encounters[formation03] + "\r\n";
                groupData += "F4 (06,3%) : (" + formation04.ToString("X3") + ") " + Encounters[formation04] + "\r\n";

                MonsterGroups.Add(groupData);
            }


            // D0/8000-D0/83FF	Data	Monster zones in dungeons (512*2 indices to Monster groups)
            br.BaseStream.Position = 0x108000 + headerOffset;
            for (int i = 0; i < 0x0200; i++)
            {
                _monsterZones.Add(br.ReadByte());
                br.ReadByte();
            }


            // D0/7A00-D0/7BFF	Data	Monster zones in World 1 (4*2 * 8*8)
            // D0/7C00-D0/7DFF	Data	Monster zones in World 2 (4*2 * 8*8)
            // D0/7E00-D0/7FFF	Data	Monster zones in World 3 (4*2 * 8*8)
            //   64 World Map zones (8x8). 4 monster groups per zone.
            
            br.BaseStream.Position = 0x107A00 + headerOffset;
            for (int k = 0; k < 0x03; k++)
            {
                List<List<byte>> newWolrdMapZone = new List<List<byte>>();
                for (int j = 0; j < 0x40; j++)
                {
                    List<byte> newWolrdMapQuadrant = new List<byte>();
                    for (int i = 0; i < 0x04; i++)
                    {
                        newWolrdMapQuadrant.Add(br.ReadByte());
                        br.ReadByte();
                    }
                    newWolrdMapZone.Add(newWolrdMapQuadrant);
                }

                _monsterWolrdMapZones.Add(newWolrdMapZone);
            }


            // D0/7800-D0/797F	Data	Event encounters (0x0180)

            // D0/7980-D0/79FF	Data	Monster-in-a-box encounters (0x0080)
            //   The Monster-in-a-box Formations table is composed as records of 4 bytes each. Every record is a pair of Monster Formations ids.
            //   The first id is the "common encounter" to this box and the second id the "rare encounter" one.
            //   Index	Encounters	Monsters
            //   00		0058 005A	(Sorcerer (B), Karnak, Karnak) (Gigas)
            //   01		005A 005A	(Gigas) (Gigas)
        }



        /**
        * initSprites
        * 
        * Load all the maps sprites data.
        *
        * @param br: The reader to load the data from.
        * @param headerOffset: The offset due to a header in the ROM.
        */
        private void InitSprites(BinaryReader br, int headerOffset)
        {
            _sprites = new List<List<List<Image>>>();

            //DA/0000-DB/3A00
            br.BaseStream.Position = 0x1A0000 + headerOffset;

            //HACKME <- Where in the ROM is this information?
            int[] bytesToRead = {
                0x0200, 0x0200, 0x0200, 0x0200, 0x0200, 0x0200, 0x0200, 0x0200, 0x0200, 0x0200, 0x0200, 0x0200, 0x0200, 0x0200, 0x0200, 0x0200, 
                0x0200, 0x0200, 0x0200, 0x0200, 0x0200, 0x0200, 0x0200, 0x0200, 0x0200, 0x0200, 0x0200, 0x0200, 0x0200, 0x0200, 0x0200, 0x0200, 

                0x0200, 0x0200, 0x0200, 0x0200, 0x0200, 0x0200, 0x0200, 0x0200, 0x0200, 0x0200, 0x0200, 0x0200, 0x0200, 0x0200, 0x0200, 0x0200, 
                0x0200, 0x0200, 0x0400, 0x0400, 0x0400, 0x0400, 0x0400, 0x0400, 0x0400, 0x0400, 0x0400, 0x0400, 0x0400, 0x0400, 0x0400, 0x0400, 

                0x0400, 0x0400, 0x0400, 0x0400, 0x0400, 0x0400, 0x0400, 0x0400, 0x0400, 0x0400, 0x0400, 0x0800, 0x0800, 0x0800, 0x0800, 0x0800, 
                0x0800, 0x0800, 0x0200, 0x0200, 0x0200, 0x0200, 0x0200, 0x0200, 0x0200, 0x0200, 0x0200, 0x0200, 0x0200, 0x0200, 0x0200, 0x0200, 

                0x0200, 0x0200, 0x0200, 0x0200, 0x0200, 0x0200, 0x0200, 0x0CC0, 0x0400 };

            for (int i = 0; i < 0x69; i++)
            {
                List<byte> data4Bpp = br.ReadBytes(bytesToRead[i]).ToList();
                
                List<List<Image>> spritesI = new List<List<Image>>();
                for (int j = 0; j < 0x08; j++)
                {
                    //Load the sprite id Image.
                    Image bigBmpSet = Transformations.transform4b(data4Bpp, 0, bytesToRead[i], _spritePalettes[j].ToArray());

                    //Cut the Image in 16x16 pieces. Mirror the sprites if needed.
                    List<Image> bmpSet = ToSpriteMap(bigBmpSet, bytesToRead[i] <= 0x0200, i >= 0x67);

                    spritesI.Add(bmpSet);
                }

                _sprites.Add(spritesI);
            }
        }



        /**
        * loadTextData
        * 
        * Initialize some game texts.
        *
        * @param speechTxt: NPC dialogues.
        * @param itemNames: Names of the items.
        * @param spellNames: Names of the spells.
        * @param locationNames: Names of the Locations.
        * @param monsterNames: Names of the Monsters.
        */
        private void LoadTextData(List<string> speechTxt, List<string> itemNames, List<string> spellNames, List<string> locationNames, List<string> monsterNames)
        {
            this._speechTxt = new List<string>(speechTxt);
            this.ItemNames = new List<string>(itemNames);
            this.SpellNames = new List<string>(spellNames);
            this._locationNames = new List<string>(locationNames);
            this._monsterNames = new List<string>(monsterNames);
        }



        /**
        * initWorldMap
        * 
        * Initialize the structures related with World Maps.
        *
        * @param br: The reader to load the data from.
        * @param headerOffset: The offset due to a header in the ROM.
        */
        private void InitWorldMap(BinaryReader br, int headerOffset)
        {
            //Initialize the world map palettes
            InitWorldMapPalettes(br, headerOffset);

            //Initialize the world map tilemaps
            LoadWorldMapTilemap(br, headerOffset);
        }



        /**
        * initWorldMapPalettes
        * 
        * Aux function. Load all the World Map palettes.
        *
        * @param br: The reader to load the data from.
        * @param headerOffset: The offset due to a header in the ROM.
        */
        private void InitWorldMapPalettes(BinaryReader br, int headerOffset)
        {
            //Read World map palette
            //CF/FCC0 (+ #worldmap * 0x0100)
            br.BaseStream.Position = 0x0FFCC0 + headerOffset;

            for (int i = 0; i < 0x03; i++)
            {
                byte[] bytePal = br.ReadBytes(0x0100);
                List<Color> newPalette = new List<Color>();

                for (int j = 0; j < 0x0100; j += 2)
                {
                    int color = bytePal[j] + (bytePal[j + 1] * 0x0100);
                    int r = ((color >> 0x0) & 0x1F) << 3;
                    int g = ((color >> 0x5) & 0x1F) << 3;
                    int b = ((color >> 0xA) & 0x1F) << 3;
                    r = r + (r >> 5);
                    g = g + (g >> 5);
                    b = b + (b >> 5);

                    newPalette.Add(Color.FromRgb((byte)r, (byte)g, (byte)b));
                }

                _worldMapBgPalettes.Add(newPalette);
            }
        }



        /**
        * initTileblocks
        * 
        * Aux function. Load all the game tile blocks.
        *
        * @param br: The reader to load the data from.
        * @param headerOffset: The offset due to a header in the ROM.
        */
        private void InitTileblocks(BinaryReader br, int headerOffset)
        {
            //CF/0000 Tile block offsets [2 bytes * 1C]
            //CF/0038 Tile blocks [Compressed unheaded type02] [2 bytes * (256 * 4)]
            //CF/C502 Unused 00

            for (int i = 0; i < 0x01C; i++)
            {
                br.BaseStream.Position = 0x0F0000 + (i * 2) + headerOffset;
                _tileBlockOffsets.Add(0x0F0000 + (br.ReadByte() + (br.ReadByte() * 0x0100)));
                _tileBlocks.Add(Compressor.uncompress(_tileBlockOffsets[i], br, headerOffset, true, true));
            }
        }



        /**
        * initTileMaps
        * 
        * Aux function. Load all the game locations byte maps.
        *
        * @param br: The reader to load the data from.
        * @param headerOffset: The offset due to a header in the ROM.
        */
        private void InitTileMaps(BinaryReader br, int headerOffset)
        {
            // 0B0000 Tile maps offsets [2bytes * 0x0148]
            // 0B0290 Tile maps [Compressed unheaded type02] [1byte * 64 * 64] [Until 0DFDFF]

            int preOffset = 0;
            int newOffset = 0;
            byte  bank = 0x0B;
            long newSize = 0;
            _tileMapSizes.Clear();

            for (int i = 0; i < 0x0143; i++)
            {
                br.BaseStream.Position = 0x0B0000 + (i * 2) + headerOffset;
                newOffset = br.ReadByte() + (br.ReadByte() * 0x0100);

                if(newOffset < preOffset){
                    bank++;
                }

                _tileMapsOffsets.Add(newOffset + bank * 0x010000);
                _tileMaps.Add(Compressor.uncompress(_tileMapsOffsets[i], br, headerOffset, out newSize, true, true));
                if (_tileMaps.Last().Count != 4096)
                {
                    _badTilemaps.Add(i);
                    _tileMapSizes.Add(1);
                }
                else
                {
                    _tileMapSizes.Add(newSize);
                }

                preOffset = newOffset;
            }
            _tileMapSizes.Add(2);

            _tileMapSumSizes = _tileMapSizes.Sum();
        }



        /**
        * initPalettes
        * 
        * Aux function. Load all the background and sprites palettes.
        *
        * @param br: The reader to load the data from.
        * @param headerOffset: The offset due to a header in the ROM.
        */
        private void InitPalettes(BinaryReader br, int headerOffset)
        {
            // 03BB00 palettes [0x2B sets of * 2bytes * 0x080 subpalettes]

            br.BaseStream.Position = 0x03BB00 + headerOffset;

            for (int i = 0; i < 0x2B; i++)
            {
                byte[] bytePal = br.ReadBytes(0x0100);
                List<Color> newPalette = new List<Color>();

                for (int j = 0; j < 0x0100; j += 2)
                {
                    int color = bytePal[j] + (bytePal[j + 1] * 0x0100);
                    int r = ((color >> 0x0) & 0x1F) << 3;
                    int g = ((color >> 0x5) & 0x1F) << 3;
                    int b = ((color >> 0xA) & 0x1F) << 3;
                    r = r + (r >> 5);
                    g = g + (g >> 5);
                    b = b + (b >> 5);

                    newPalette.Add(Color.FromRgb((byte)r, (byte)g, (byte)b));
                }

                _bgPalettes.Add(newPalette);
            }

            // Sprite palettes at DF/FC00
            // DFFC00 palette [0x04 sets of * 2bytes * 0x010 subpalettes]
            _spritePalettes = new List<List<Color>>();

            for (int k = 0; k < 0x02; k++)
            {
                br.BaseStream.Position = 0x1FFC00 + headerOffset;

                for (int i = 0; i < 0x04; i++)
                {
                    byte[] bytePal = br.ReadBytes(0x0020);
                    List<Color> newPalette = new List<Color>();

                    for (int j = 0; j < 0x0020; j += 2)
                    {
                        int color = bytePal[j] + (bytePal[j + 1] * 0x0100);
                        int r = ((color >> 0x0) & 0x1F) << 3;
                        int g = ((color >> 0x5) & 0x1F) << 3;
                        int b = ((color >> 0xA) & 0x1F) << 3;
                        r = r + (r >> 5);
                        g = g + (g >> 5);
                        b = b + (b >> 5);

                        newPalette.Add(Color.FromRgb((byte)r, (byte)g, (byte)b));
                    }

                    _spritePalettes.Add(newPalette);
                }
            }
        }



        /**
        * loadWorldMapTilemap
        * 
        * Load all the game World Map tilemaps and store them into the attribute "worldmapTileMaps".
        *
        * @param br: The reader to load the data from.
        * @param headerOffset: The offset due to a header in the ROM.
        */
        private void LoadWorldMapTilemap(BinaryReader br, int headerOffset)
        {
            // CFE000 Offsets [2bytes * 256 pointers to each horizontal tile line * 5 worlds]
            // C7/0000-C8/221F World Map custom compressed tilemap data           
            _worldmapTileMaps.Clear();

            for (int id = 0; id < 5; id++)
            {
                List<byte> output = new List<byte>();

                for (int i = 0; i < 0x0100; i++)
                {
                    //Read 2 bytes (big endian) from (Field data offsets (2bytes * 256 vertical * 5 worlds)) and set it to offset [$23]
                    br.BaseStream.Position = 0x0FE000 + (id * 0x0200) + (i * 2) + headerOffset;

                    int offset = br.ReadByte() + br.ReadByte() * 0x0100;
                    if (id == 4 && offset < 0xF000) offset += 0x010000; //HACKME if a better compression is implemented

                    //Mete 0x0100 (256) bytes desde (C7/0000-C8/221F)[$23] (World Map Compressed data) hasta la posición de memoria (7F/7622-7F/7722)
                    br.BaseStream.Position = 0x070000 + offset + headerOffset;
                    output.AddRange(Compressor.uncompressTypeWorldMap(br));
                }

                _worldmapTileMaps.Add(output);
            }
        }



        #endregion



        #region MapDecypheringAuxiliarFunctions



        /**
        * drawBackground
        * 
        * Aux. Return a draw of one of the current background layers
        *
        * @param tileMap: The tilemap of the layer of the location to draw.
        * @param tiles16x16: The tile map of the location.
        * @param down: The priority of the layer ('up' is drawn over 'down').
        * @param background02: The layer is a 'background02' (4 color per tile instead of 16 color per tile).
        * 
        * @return: The Image picture representing the background layer.
        */
        /*private static Image<Rgba32> DrawBackground(List<byte> tileMap, List<TileTexture> tiles16X16, bool down, bool background02 = false)
        {
            var gd = tiles16X16[0].ImageP1.GraphicsDevice;
            RenderTarget2D background = new RenderTarget2D(gd, 64 * 8 * 2, 64 * 8 * 2);
            int x = 0;
            int y = 0;
            background.Set
           
                foreach (byte item in tileMap)
                {
                    if (background02)
                        if (down)
                            g.DrawImage(tiles16X16[item].ImageP1Bg02, new Point(x, y), 1);
                        else
                            g.DrawImage(tiles16X16[item].ImageP0Bg02, new Point(x, y), 1);
                    else if (down)
                        g.DrawImage(tiles16X16[item].ImageP1, new Point(x, y), 1);
                    else
                        g.DrawImage(tiles16X16[item].ImageP0, new Point(x, y), 1);
                    x += 16;
                    if (x > 16 * 63)
                    {
                        x = 0;
                        y += 16;
                    }
                }
            

            return background;
        }*/




        /**
        * isWall HACKME (It works as the C0/1661 routine in the SNES ROM (I don't understand it yet))
        * 
        * Aux. Return true if there is a wall between two tiles.
        *
        * @param obyte0: Properties byte of the origin tile.
        * @param dbyte0: Properties byte of the destination tile.
        * 
        * @return: True if there is a 'wall' between the origin tile and the destination one.
        */
        private static bool IsWall(byte obyte0, byte dbyte0)
        {
            /*
            $10D8   ???? 00:10E6 o 00:10DA (Destination byte 1 ????)
            $10F2,X (Destination byte 0)

            $10FA obyte0 (origin byte0)
            $10FB obyte1 (origin byte1)

            C4 = (Stay = 0, Up = 1, Right = 2, Down = 3, Left = 4)

            $C01720,x
            08 02 0A 0C 06 (..., 0010, 1010, 1100, 0110) {.., 2, 10, 12, 6}

            $C01725,x
            00 08 01 04 02
            */

            byte c2 = (byte)((obyte0 & 0x03) + 4);                                 // 00:0BC2
            byte c3 = ((obyte0 & 0x03) == 0x03) ? (byte)0 : (byte)(obyte0 & 0x03); // 00:0BC3

            /*
            if (((obyte1 & 0x40) == 0) && ((obyte1 & {00, 08, 01, 04, 02}[dir]) == 0)){
                // Red wall
                return true;
            */

            // $C0/1661;

            /* HACKME 10D8 se puede presuponer 0, de momento */
            /* 
            if ($10D8 != 0){
                if (dbyte0 & 0x04 == 0){ // Bit 3
                    if ((obyte0 & 0x04 == 0) || C3 != 2){
                        return true;
                    }
                }else if (C3 == 1){
                    return true; // Wall+
                }
            }*/

            if ((obyte0 & 0x04) == 0) // Bit 3
            {
                if ((dbyte0 & c2) == 0)
                    return true;
                else
                    return false;
            }

            /* Moving from under a "ceiling" tile */
            if ((dbyte0 & 0x04) != 0) // Bit 3
            {
                return false;
            }
            else if ((dbyte0 & 0x03 & c3) != 0) // Bits 1, 2
            {
                return false;
            }

            return true;
        }



        /**
        * drawWalls
        * 
        * Aux. Draw the walls in and surrounding one tile.
        *
        * @param i: X coordinate of the 16x16 tile to check.
        * @param j: Y coordinate of the 16x16 tile to check.
        * @param Image: Picture to draw the walls in.
        */
        private void DrawWalls(int i, int j, Image image)
        {
            int x = i * 16;
            int y = j * 16;

            byte item = Tilemap00[(j * 64) + i];

            byte tileProps00 = TileProperties[(item * 2) + 0];
            byte tileProps01 = TileProperties[(item * 2) + 1];

            byte wallsbyte = (byte)((tileProps01 & 0x0F));

            // Clear
            image.Mutate(g =>
            {
                g.Clip(new RectangularPolygon(x, y, 16, 16), null);
                g.Clear( Color.Transparent);
            });
            //using (var graphics = Graphics.FromImage(Image))
            //{
            //    graphics.SetClip(new RectangleF(x, y, 16, 16));
            //    graphics.Clear(Color.Transparent);
            //    graphics.ResetClip();
           //}

            // Start drawing walls
            if (wallsbyte == 0x00 && (tileProps01 & 0x40) == 0)
            {
                // 'Red' tile
                image.Mutate(g =>
                {
                    SolidBrush redBrush = new SolidBrush(Color.Red);
                    g.Fill(redBrush, new RectangleF(x,y,16,16));
                });
            }
            else
            {
                // Get normal walls
                if ((tileProps01 & 0x40) == 0)
                {
                    if (wallsbyte != 0x0F)
                    {
                        image.Mutate(g =>
                        {
                            //
                            // 0 means wall
                            // 1 means air
                            // xxxx xxxx | xxxx UDLR
                            //
                            SolidBrush redBrush = new SolidBrush(Color.Red);
                            if ((wallsbyte & 0x08) == 0) g.Fill(redBrush, new Rectangle(x, y, 16, 03));
                            if ((wallsbyte & 0x04) == 0) g.Fill(redBrush,new Rectangle(x, y + 13, 16, 03));
                            if ((wallsbyte & 0x02) == 0) g.Fill(redBrush, new Rectangle(x, y, 03, 16));
                            if ((wallsbyte & 0x01) == 0) g.Fill(redBrush, new Rectangle(x + 13, y, 03, 16));
                        });

                    }
                }

                image.Mutate(g =>
                {
                        SolidBrush greenBrush = new SolidBrush(Color.Green);
                        if (i > 0 && IsWall(tileProps00, TileProperties[Tilemap00[(j * 64) + i - 1] * 2])) g.Fill(greenBrush, new Rectangle(x, y, 3, 16));
                        if (i < 63 && IsWall(tileProps00, TileProperties[Tilemap00[(j * 64) + i + 1] * 2])) g.Fill(greenBrush, new Rectangle(x + 13, y, 3, 16));
                        if (j > 0 && IsWall(tileProps00, TileProperties[Tilemap00[((j - 1) * 64) + i] * 2])) g.Fill(greenBrush, new Rectangle(x, y, 16, 3));
                        if (j < 63 && IsWall(tileProps00, TileProperties[Tilemap00[((j + 1) * 64) + i] * 2])) g.Fill(greenBrush, new Rectangle(x, y + 13, 16, 3));
                });
                
            }
        }
         
        
        
        public struct Wall
        {
            //public byte X { get; init; }
           // public byte Y { get; init; }
            public bool PassableLeft { get; init; }
            public bool PassableRight { get; init; }
            public bool PassableUp { get; init; }
            public bool PassableDown { get; init; }
            
            public Wall(bool passableLeft, bool passableRight, bool passableUp, bool passableDown)
            {
                //X = x;
                //Y = y;
                PassableLeft = passableLeft;
                PassableRight = passableRight;
                PassableUp = passableUp;
                PassableDown = passableDown;
            }
        }
        
        public static Wall[,] GetWalls(List<byte> tileMap, List<byte> tileProperties, List<byte> tilePropertiesMask)
        {
            
            //Image<Rgba32> background = new Image<Rgba32>(64 * 8 * 2, 64 * 8 * 2);
            int x = 0;
            int y = 0;

            var walls = new Wall[64,64];
            
            byte wallsbyte = 0;

            if (tileMap.Count < 64 * 64)
                return walls;

            for (byte j = 0; j < 64; j++)
            {
                for (byte i = 0; i < 64; i++)
                {
                    byte item = tileMap[(j * 64) + i];

                    byte tileProps00 = tileProperties[(item * 2) + 0];
                    byte tileProps01 = tileProperties[(item * 2) + 1];

                    wallsbyte = (byte)((tileProps01 & 0x0F));

                    if (wallsbyte == 0x00 && (tileProps01 & 0x40) == 0)
                    {
                        // 'Red' tile
                        walls[i,j] = new Wall(false,false,false,false);
                    }
                    else
                    {
                        // Get normal walls
                        if ((tileProps01 & 0x40) == 0)
                        {
                            if (wallsbyte != 0x0F)
                            {
                                /*background.Mutate(g =>
                                {
                                    //
                                    // 0 means wall
                                    // 1 means air
                                    // xxxx xxxx | xxxx UDLR
                                    //
                                  
                                    SolidBrush redBrush = new SolidBrush(Color.Red);
                                    if ((wallsbyte & 0x08) == 0) g.Fill(redBrush, new Rectangle(x, y, 16, 03));
                                    if ((wallsbyte & 0x04) == 0) g.Fill(redBrush,new Rectangle(x, y + 13, 16, 03));
                                    if ((wallsbyte & 0x02) == 0) g.Fill(redBrush, new Rectangle(x, y, 03, 16));
                                    if ((wallsbyte & 0x01) == 0) g.Fill(redBrush, new Rectangle(x + 13, y, 03, 16));
                                    
                                });*/
                                walls[i,j] = new Wall((wallsbyte & 0x02) == 0, (wallsbyte & 0x01) == 0, (wallsbyte & 0x08) == 0, (wallsbyte & 0x04) == 0);
                            }
                        }
                        walls[i, j] = new Wall(!(i > 0 && IsWall(tileProps00, tileProperties[tileMap[(j * 64) + i - 1] * 2])),
                            !(i < 63 && IsWall(tileProps00, tileProperties[tileMap[(j * 64) + i + 1] * 2])), 
                            !(j > 0 && IsWall(tileProps00, tileProperties[tileMap[((j - 1) * 64) + i] * 2])),
                            !(j < 63 && IsWall(tileProps00, tileProperties[tileMap[((j + 1) * 64) + i] * 2])));
                        
                        
                        
                        /*background.Mutate(g =>
                        {
                            SolidBrush greenBrush = new SolidBrush(Color.Green);
                            if (i > 0 && isWall(tileProps00, tileProperties[tileMap[(j * 64) + i - 1] * 2])) g.Fill(greenBrush, new Rectangle(x, y, 3, 16));
                            if (i < 63 && isWall(tileProps00, tileProperties[tileMap[(j * 64) + i + 1] * 2])) g.Fill(greenBrush, new Rectangle(x + 13, y, 3, 16));
                            if (j > 0 && isWall(tileProps00, tileProperties[tileMap[((j - 1) * 64) + i] * 2])) g.Fill(greenBrush, new Rectangle(x, y, 16, 3));
                            if (j < 63 && isWall(tileProps00, tileProperties[tileMap[((j + 1) * 64) + i] * 2])) g.Fill(greenBrush, new Rectangle(x, y + 13, 16, 3));
                        });*/
                    }

                    x += 16;
                    if (x > 16 * 63)
                    {
                        x = 0;
                        y += 16;
                    }

                }
            }

            return walls;
        }

        /**
        * drawWalls
        * 
        * Aux. Draw the walls of the current location.
        *
        * @param tileMap: The tilemap of the background 00 of the current location.
        * @param tileProperties: The properties of the 16x16 tiles of the current location.
        * @param tilePropertiesMask: The mask properties of the 16x16 tiles of the current location.
        * 
        * @return: The Image picture representing the walls 'layer'.
        */
        private static Image<Rgba32> DrawWalls(List<byte> tileMap, List<byte> tileProperties, List<byte> tilePropertiesMask)
        {
            
            Image<Rgba32> background = new Image<Rgba32>(64 * 8 * 2, 64 * 8 * 2);
            int x = 0;
            int y = 0;

            byte wallsbyte = 0;

            if (tileMap.Count < 64 * 64)
                return background;

            for (int j = 0; j < 64; j++)
            {
                for (int i = 0; i < 64; i++)
                {
                    byte item = tileMap[(j * 64) + i];

                    byte tileProps00 = tileProperties[(item * 2) + 0];
                    byte tileProps01 = tileProperties[(item * 2) + 1];

                    wallsbyte = (byte)((tileProps01 & 0x0F));

                    if (wallsbyte == 0x00 && (tileProps01 & 0x40) == 0)
                    {
                        // 'Red' tile
                        background.Mutate(g =>
                        {
                            SolidBrush redBrush = new SolidBrush(Color.Red);
                            g.Fill(redBrush, new RectangleF(x,y,16,16));
                        });
                    }
                    else
                    {
                        // Get normal walls
                        if ((tileProps01 & 0x40) == 0)
                        {
                            if (wallsbyte != 0x0F)
                            {
                                background.Mutate(g =>
                                {
                                    //
                                    // 0 means wall
                                    // 1 means air
                                    // xxxx xxxx | xxxx UDLR
                                    //
                                    SolidBrush redBrush = new SolidBrush(Color.Red);
                                    if ((wallsbyte & 0x08) == 0) g.Fill(redBrush, new Rectangle(x, y, 16, 03));
                                    if ((wallsbyte & 0x04) == 0) g.Fill(redBrush,new Rectangle(x, y + 13, 16, 03));
                                    if ((wallsbyte & 0x02) == 0) g.Fill(redBrush, new Rectangle(x, y, 03, 16));
                                    if ((wallsbyte & 0x01) == 0) g.Fill(redBrush, new Rectangle(x + 13, y, 03, 16));
                                });
                            }
                        }

                        background.Mutate(g =>
                        {
                            SolidBrush greenBrush = new SolidBrush(Color.Green);
                            if (i > 0 && IsWall(tileProps00, tileProperties[tileMap[(j * 64) + i - 1] * 2])) g.Fill(greenBrush, new Rectangle(x, y, 3, 16));
                            if (i < 63 && IsWall(tileProps00, tileProperties[tileMap[(j * 64) + i + 1] * 2])) g.Fill(greenBrush, new Rectangle(x + 13, y, 3, 16));
                            if (j > 0 && IsWall(tileProps00, tileProperties[tileMap[((j - 1) * 64) + i] * 2])) g.Fill(greenBrush, new Rectangle(x, y, 16, 3));
                            if (j < 63 && IsWall(tileProps00, tileProperties[tileMap[((j + 1) * 64) + i] * 2])) g.Fill(greenBrush, new Rectangle(x, y + 13, 16, 3));
                        });
                    }

                    x += 16;
                    if (x > 16 * 63)
                    {
                        x = 0;
                        y += 16;
                    }

                }
            }

            return background;
        }



        /**
        * drawWorldMapWalls
        * 
        * Aux. Draw the walls of the current World Map.
        *
        * @param tileMap: The tilemap of the current World Map.
        * @param tileProperties: The properties of the 16x16 tiles of the current World Map.
        * 
        * @return: The Image picture representing the walls 'layer'.
        */
        /*private static Image drawWorldMapWalls(List<byte> tileMap, List<byte> tileProperties)
        {

            Image background = new Image<Rgba32>(64 * 8 * 2, 64 * 8 * 2);
            int x = 0;
            int y = 0;


            if (tileMap.Count < 64 * 64)
                return background;

            for (int j = 0; j < 64; j++)
            {
                for (int i = 0; i < 64; i++)
                {
                    byte item = tileMap[(j * 64) + i];
                    byte byte00 = (item > 0xBF) ? (byte)0x00 : tileProperties[item * 3 + 0];

                    if ((byte00 & 0x01) == 0x00)
                    {
                        // 'Red' tile
                        using (var graphics = Graphics.FromImage(background))
                        {
                            SolidBrush redBrush = new SolidBrush(Color.Red);
                            graphics.FillRectangle(redBrush, x, y, 16, 16);
                        }
                    }

                    x += 16;
                    if (x > 16 * 63)
                    {
                        x = 0;
                        y += 16;
                    }

                }
            }

            return background;
        }*/

        private static Wall[,] GetWorldMapWalls(List<byte> tileMap, List<byte> tileProperties)
        {
            int x = 0;
            int y = 0;

            var walls = new Wall[64, 64];

            if (tileMap.Count < 64 * 64)
                return walls;

            for (int j = 0; j < 64; j++)
            {
                for (int i = 0; i < 64; i++)
                {
                    byte item = tileMap[(j * 64) + i];
                    byte byte00 = (item > 0xBF) ? (byte)0x00 : tileProperties[item * 3 + 0];

                    if ((byte00 & 0x01) == 0x00)
                        walls[i, j] = new Wall(false, false, false, false);
                    else
                        walls[i, j] = new Wall(true, true, true, true);

                    x += 16;
                    if (x > 16 * 63)
                    {
                        x = 0;
                        y += 16;
                    }

                }
            }

            return walls;
        }
        
        /**
        * clearCurrentLocation
        * 
        * Aux. Clears the data lists of the current location.
        */
        private void ClearCurrentLocation()
        {
            // Clear tilemaps
            Tilemap00 = new List<byte>();
            Tilemap01 = new List<byte>();
            Tilemap02 = new List<byte>();

            // Clear tile properties
            TilePropertiesL.Clear();
            TilePropertiesH.Clear();

            for (int i = 0; i < (0x80 * 0x80); i++)
            {
                TilePropertiesL.Add(0x00);
                TilePropertiesH.Add(0x00);
            }


            // Clear tiles
            TilesBg0 = new List<List<tile8x8>>();
            TilesBg1 = new List<List<tile8x8>>();
            TilesBg2 = new List<List<tile8x8>>();

            List<tile8x8> auxList = new List<tile8x8>();
            for (int i = 0; i < 0x80; i++)
                auxList.Add(new tile8x8(0, 0));

            for (int i = 0; i < 0x80; i++)
            {
                TilesBg0.Add(auxList.ToList());
                TilesBg1.Add(auxList.ToList());
                TilesBg2.Add(auxList.ToList());
            }

            // Clear npcs, exits, chests and events
            Npcs      = new List<NPC>();
            Exits     = new List<MapExit>();
            Treasures = new List<Treasure>();
            Events    = new List<Event>();
        }



        /**
        * loadCurrentTileset
        * 
        * Aux function. Load the current location 16x16 tile set.
        *
        * @param br: The reader to load the data from.
        * @param headerOffset: The offset due to a header in the ROM.
        * @param mapDescriptors: The 26 bytes descriptor of the current location.
        */
        private void LoadCurrentTileset(BinaryReader br, int headerOffset, List<byte> mapDescriptors)
        {
            int offset = 0;

            // 08 1114 used in $C0/6B6E (CF0000,(00:1114 * 2) + CF/0000) pointer to [unheaded Type02]) Tilemap decompressed in 7F/6E22 (CF/3205 is an example)

            //br.BaseStream.Position = 0x0F0000 + headerOffset + (mapDescriptors[0x08] << 1);
            //offset = br.ReadByte() + (br.ReadByte() * 0x0100) + 0x0F0000 + headerOffset;
            //List<byte> unknown = Compressor.uncompress(offset, br, headerOffset, true, true);

            // 09 1115 used in $C0/591A (DC2D84,((00:1115 & 0x3F (6 bits)) * 4) + DC/2E24)                 First  [4bpp 8x8] Graphics load
            // .. .... used in $C0/596A (DC2D84,(Invert_bytes((00:1115..16 & 0FC0) * 4) * 4) + DC/2E24)    Second [4bpp 8x8] Graphics load
            // 0A 1116 used in $C0/59D7 (DC2D84,((00:1116..7 & 03F0)/4) + DC/2E24)                         Third  [4bpp 8x8] Graphics load
            // 0B 1117 used in $C0/5A45 (DC0000,((00:1117 & #$FC) / 2) + DC/0024)                                 [2bpp 8x8] Graphics load

            _bgTileSet = new List<Image>();
            br.BaseStream.Position = 0x1C2D84 + headerOffset + ((mapDescriptors[0x09] & 0x3F) << 2);
            offset = br.ReadByte() + (br.ReadByte() * 0x0100) + (br.ReadByte() * 0x010000) + 0x1C2E24 + headerOffset;
            br.BaseStream.Position = offset;
            _bgTileSet.AddRange(ToTileMap(Transformations.transform4b(br.ReadBytes(0x2000).ToList(), 0, 0x2000)));

            br.BaseStream.Position = 0x1C2D84 + headerOffset + ((mapDescriptors[0x09] & 0xC0) >> 4) + ((mapDescriptors[0x0A] & 0x0F) << 4);
            offset = br.ReadByte() + (br.ReadByte() * 0x0100) + (br.ReadByte() * 0x010000) + 0x1C2E24 + headerOffset;
            br.BaseStream.Position = offset;
            _bgTileSet.AddRange(ToTileMap(Transformations.transform4b(br.ReadBytes(0x2000).ToList(), 0, 0x2000)));

            br.BaseStream.Position = 0x1C2D84 + headerOffset + ((mapDescriptors[0x0A] & 0xF0) >> 2) + ((mapDescriptors[0x0B] & 0x03) << 6);
            offset = br.ReadByte() + (br.ReadByte() * 0x0100) + (br.ReadByte() * 0x010000) + 0x1C2E24 + headerOffset;
            br.BaseStream.Position = offset;
            _bgTileSet.AddRange(ToTileMap(Transformations.transform4b(br.ReadBytes(0x2000).ToList(), 0, 0x2000)));

            br.BaseStream.Position = 0x1C0000 + headerOffset + ((mapDescriptors[0x0B] & 0xFC) >> 1);
            offset = br.ReadByte() + (br.ReadByte() * 0x0100) + 0x1C0024 + headerOffset;
            br.BaseStream.Position = offset;
            _bgTileSet.AddRange(ToTileMap(Transformations.transform2bpp(br.ReadBytes(0x2000).ToList(), 0, 0x1000)));

            LoadCurrentAnimatedTileset(br, headerOffset, mapDescriptors);
        }



        /**
        * loadWorldMapTileset
        * 
        * Aux function. Load the current World Map 16x16 tile set.
        *
        * @param br: The reader to load the data from.
        * @param headerOffset: The offset due to a header in the ROM.
        * @param id: The id of the current World Map.
        */
        private Image<Rgba32> LoadWorldMapTileset(BinaryReader br, int headerOffset, int id)
        {
            // DB/8000 (+ #worldmap * 0x2000) Mode 7 tile set (each byte contains two mode 7 bytes)
            // CF/F9C0 (+ #worldmap * 0x0100) palette shift

            int mapId = 0;
            if (id == 1) mapId = 1;
            if (id > 2) mapId = 2;

            List<byte> shifts = new List<byte>();
            List<byte> romTileset = new List<byte>();
            List<byte> tileset = new List<byte>();

            //HACKME, this should be preloaded
            br.BaseStream.Position = 0x0FF9C0 + (mapId * 0x0100) + headerOffset;
            shifts.AddRange(br.ReadBytes(0x0100).ToList());

            br.BaseStream.Position = 0x1B8000 + (mapId * 0x2000) + headerOffset;
            romTileset.AddRange(br.ReadBytes(0x2000).ToList());

            for (int j = 0; j < 0x0100; j++)
            {
                for (int i = 0; i < 0x20; i++)
                {
                    int bytes = romTileset[(j * 0x20) + i];
                    tileset.Add((byte)((bytes & 0x0F) + shifts[j]));
                    tileset.Add((byte)(((bytes >> 4) & 0x0F) + shifts[j]));
                }
            }

            WorldmapTileset = Transformations.transform8bM7(tileset, 0, _worldMapBgPalettes[mapId]);

            return WorldmapTileset;
        }



        /**
        * loadCurrentAnimatedTileset
        * 
        * Aux function. Fix the current location 16x16 tile set loading the animated tiles.
        *
        * @param br: The reader to load the data from.
        * @param headerOffset: The offset due to a header in the ROM.
        * @param mapDescriptors: The 26 bytes descriptor of the current location.
        */
        private void LoadCurrentAnimatedTileset(BinaryReader br, int headerOffset, List<byte> mapDescriptors)
        {
            // The animated tiles are the ones in BG00 and BG01 setting in the VRAM $2E00 to $3000
            // There are also animated in BG02 setted in the VRAM at $4780 to $4800
            // The $C0/9A96 load these animated tiles.
            // The $C0/996D rotate the loaded tiles.

            // $C0/9A96
            //$13 = 0x24 * mapDescriptors[0x08];
            //$15 = 0;
            //while ($15 != 0x0C){
            //  X = ($15 * 2) + $13;
            //  Y = $15 * 8;
            //  $1419[Y] = $C09DA7[X];
            //  $2E = $C09DA7[X];
            //  $23 = $15 * 0x0800;
            //  X = $15 + $13;
            //  $1418[Y] = 0x00;
            //  $1417[Y] = $C09D9B[X];
            //  if ($1417[Y] & 0x0080 != 0){
            //    $1419[Y] = $23 + #$8622;
            //    JSR $9B01;
            //  }
            //  $15++;
            //}

            // $C0/996D
            // if (mapDescriptors[0x04] & 0x80 != 0x00) return;
            // Store in VRAM #$2E00 8 times 80 bytes loaded from DF/0000[141B,y]
            // Store in VRAM #$4780 4 times 40 bytes loaded from DF/0000[141B,y]

            // $C0/9A96
            int var13;
            byte[] var1417 = new byte[0x0C];
            byte[] var1418 = new byte[0x0C];
            int[]  var1419 = new  int[0x0C];
            //int var2E;

            var13 = 0x24 * mapDescriptors[0x08];

            for (int i = 0; i < 0x0C; i++)
            {
                br.BaseStream.Position = 0x009DA7 + headerOffset + (var13 + i * 2);
                var1419[i] = br.ReadByte() + br.ReadByte() * 0x0100;
                //  var2E = $C09DA7[var13 + i * 2];
                var1418[i] = 0x00;
                br.BaseStream.Position = 0x009D9B + headerOffset + (i + var13);
                var1417[i] = br.ReadByte();
                //if ((var1417[i] & 0x0080) != 0){
                    //var1419[i] = (i * 0x0800) + 0x8622;
                //}
            }


            // $C0/996D

            // 0x1000 VRAM positions are 0x0100 tiles from bg_TileSet
            // 0x2E00 VRAM is bg_TileSet[0x02E0]
            // 0x4780 VRAM is bg_TileSet[0x0378]

            // Store in VRAM #$2E00 8 times 80 bytes loaded from DF/0000[141B,y]
            // Store in VRAM #$4780 4 times 40 bytes loaded from DF/0000[141B,y]
            for (int i = 0; i < 0x08; i++)
            {
                br.BaseStream.Position = 0x1F0000 + headerOffset + ((int)(var1418[i] * 8) & 0xFF80) + var1419[i];
                List<Image> newImages = ToTileMap(Transformations.transform4b(br.ReadBytes(0x80 * 4).ToList(), 0, 0x80 * 4));

                // TODO: Tyler
                //if ((var1417[i] & 0x0080) != 0)
                //    newImages = shakeImages(newImages, 3);

                int offset = 0x02E0 + (i * 4);
                _bgTileSet[offset + 0] = newImages[0];
                _bgTileSet[offset + 1] = newImages[1];
                _bgTileSet[offset + 2] = newImages[2];
                _bgTileSet[offset + 3] = newImages[3];
            }

            for (int i = 0x08; i < 0x0C; i++)
            {
                br.BaseStream.Position = 0x1F0000 + headerOffset + ((int)(var1418[i] * 4) & 0xFFC0) + var1419[i];
                List<Image> newImages = ToTileMap(Transformations.transform2bpp(br.ReadBytes(0x40 * 8).ToList(), 0, 0x40 * 8));

                // TODO: Tyler
                //if ((var1417[i] & 0x0080) != 0)
                //   newImages = shakeImages(newImages, 1);

                int offset = 0x03F0 + ((i - 0x08) * 4);
                _bgTileSet[offset + 0] = newImages[0];
                _bgTileSet[offset + 1] = newImages[1];
                _bgTileSet[offset + 2] = newImages[2];
                _bgTileSet[offset + 3] = newImages[3];
            }
        }



        /**
        * shakeImages
        * 
        * Aux function. Take some (animated) tiles and shake them.
        * This tries to emulate the 'water effect' of the sea and the rivers.
        *
        * @param input: The list of tiles to shake.
        * @param magnitude: The magnitude of the shaking.
        */
        /*private List<Image> shakeImages(List<Image> input, int magnitude)
        {
            Image output = new Image<Rgba32>(32, 8);
            Image aux    = new Image<Rgba32>(32, 8);

            using (var g = Graphics.FromImage(aux))
            {
                g.DrawImage(input[0], 0, 0);
                g.DrawImage(input[1], 8, 0);
                g.DrawImage(input[2], 16, 0);
                g.DrawImage(input[3], 24, 0);
            }

            using (var g = Graphics.FromImage(output)){
                for(int i = 0 ; i < 8 ; i++){
                    int j = ((i % 4) * magnitude) - (2 * magnitude);
                    g.DrawImage(aux, new Rectangle(j - 32, i, 32, 1), new Rectangle(0, i, 32, 1), GraphicsUnit.Pixel);
                    g.DrawImage(aux, new Rectangle(j, i, 32, 1), new Rectangle(0, i, 32, 1), GraphicsUnit.Pixel);
                    g.DrawImage(aux, new Rectangle(j + 32, i, 32, 1), new Rectangle(0, i, 32, 1), GraphicsUnit.Pixel);
                }
            }

            aux.Dispose();

            return toTileMap(output);
        }*/



        /**
        * setCurrentTilemaps
        * 
        * Aux function. Set the current location tilemaps.
        *
        * @param br: The reader to load the data from.
        * @param headerOffset: The offset due to a header in the ROM.
        * @param mapDescriptors: The 26 bytes descriptor of the current location.
        */
        private void SetCurrentTilemaps(BinaryReader br, int headerOffset, List<byte> mapDescriptors)
        {
            // 0C 1118 used in $C0/5877 (CB0000,(((00:1118 & 0x03FF) - 1) * 2) + CB...CC.../0000???)              [unheaded Type02] (I.e. CC/344B) -> Tilemap decompressed in 7F:0000
            // 0D 1119 used in $C0/588D (if (((00:1119 & 0x0FFC) / 4) - 1) == 0xFFFF => 0-> 7F0000:7F0FFF)        [unheaded Type02] -> Tilemap ???? decompressed in 7F:1000 ????
            // 0E 111A used in $C0/58B6 (CB0000,(((00:111A & 0x3FF0)/16) - 1) * 2) + CB...CC.../0000???)          [unheaded Type02] (I.e. CC/344B) -> Tilemap ???? decompressed in 7F:2000 ????
            // 0F 111B " 
            int index = 0;

            index = ((mapDescriptors[0x0C] + (mapDescriptors[0x0D] * 0x0100)) & 0x03FF) - 1;
            if (index >= 0)
                Tilemap00 = _tileMaps[index];

            index = (((mapDescriptors[0x0D] + mapDescriptors[0x0E] * 0x0100) & 0x0FFC) >> 2) - 1;
            if (index >= 0)
                Tilemap01 = _tileMaps[index];

            index = (((mapDescriptors[0x0E] + mapDescriptors[0x0F] * 0x0100) & 0x3FF0) / 16) - 1;
            if (index >= 0)
                Tilemap02 = _tileMaps[index];
        }



        /**
        * loadCurrentTileProperties
        * 
        * Aux function. Set the current location tile properties.
        *
        * @param br: The reader to load the data from.
        * @param headerOffset: The offset due to a header in the ROM.
        * @param mapDescriptors: The 26 bytes descriptor of the current location.
        */
        private void LoadCurrentTileProperties(BinaryReader br, int headerOffset, List<byte> mapDescriptors)
        {
            int index = 0;
            int offset = 0;

            TileProperties.Clear();

            // 05 1111 used in C0/6B44 ((CFC540,(00:1111 * 2)) + #$CF/C540) pointer to [unheaded type02] Tilemap decompressed in 00/1186
            // 0x0200 bytes
            // CF/C540-CF/C56D  Data  Tile properties offsets [2bytes * 23]
            // CF/C56E-CF/D7FF  Data  Tile properties [Compressed unheaded type02] [2 bytes * 256 after decompressed]
            // 
            // 0 means wall
            // 1 means air
            // 
            // xxxx xxxx | xxxx UDLR
            
            index = mapDescriptors[0x05] * 2;
            br.BaseStream.Position = 0x0FC540 + index + headerOffset;
            offset = 0x0FC540 + (br.ReadByte() + (br.ReadByte() * 0x0100));
            TileProperties = Compressor.uncompress(offset, br, headerOffset, true, true);
        }



        /**
        * loadCurrentWorldMapTileProperties
        * 
        * Aux function. Set the current World Map tile properties.
        *
        * @param br: The reader to load the data from.
        * @param headerOffset: The offset due to a header in the ROM.
        * @param id: Id of the current World Map.
        */
        private void LoadCurrentWorldMapTileProperties(BinaryReader br, int headerOffset, int id)
        {
            // CF/EA00-CF/F0BF World map properties [3bytes * 192 * 3]
            TileProperties.Clear();

            int mapId = 0;
            if (id == 1) mapId = 1;
            if (id > 2) mapId = 2;

            br.BaseStream.Position = 0x0FEA00 + (mapId * 0x0240) + headerOffset;
            TileProperties.AddRange(br.ReadBytes(0x0240).ToList());
        }



        /**
        * calculateCurrentTiles16x16
        * 
        * Aux function. Calculate the current location tiles16x16 map.
        *
        * @param br: The reader to load the data from.
        * @param headerOffset: The offset due to a header in the ROM.
        * @param mapDescriptors: The 26 bytes descriptor of the current location.
        */
        private void CalculateCurrentTiles16X16(BinaryReader br, int headerOffset, List<byte> mapDescriptors)
        {
            byte id = mapDescriptors[0x08];

            // Set the current palette
            // 16 1122 Used in $C0/58DB (Sent 0x100 bytes C3BB00,([17] * 0x0100 [16]) (i.e. C3:C400) -> to 00:0C00 [Palette])
            List<Color> palette = _bgPalettes[mapDescriptors[0x16]];

            Tiles16X16 = new List<tile16x16>();
            for (int i = 0; i < 0x0100; i++)
            {
                Tiles16X16.Add(new tile16x16(_tileBlocks[id][(i * 2) + 0x0000], _tileBlocks[id][(i * 2) + 0x0001],
                                             _tileBlocks[id][(i * 2) + 0x0200], _tileBlocks[id][(i * 2) + 0x0201],
                                             _tileBlocks[id][(i * 2) + 0x0400], _tileBlocks[id][(i * 2) + 0x0401],
                                             _tileBlocks[id][(i * 2) + 0x0600], _tileBlocks[id][(i * 2) + 0x0601],
                                             _bgTileSet, palette.ToList()));
            }
        }



        /**
        * calculateCurrentWorldMapTiles16x16
        * 
        * Aux function. Calculate the current World Map tiles16x16 map.
        *
        * @param br: The reader to load the data from.
        * @param headerOffset: The offset due to a header in the ROM.
        * @param id: Id of the current World Map.
        */
        private void CalculateCurrentWorldMapTiles16X16(BinaryReader br, int headerOffset, int id)
        {
            // Set the current palette

            int mapId = 0;
            if (id == 1) mapId = 1;
            if (id > 2) mapId = 2;

            List<Color> palette = _worldMapBgPalettes[mapId];
            List<byte> tileBlocks = new List<byte>();

            //HACKME
            // Estos tileblocks deberían estar precargados. Hace un tileBlocksWorldMap y un inicializador
            br.BaseStream.Position = 0x0FF0C0 + (mapId * 0x300) + headerOffset;
            tileBlocks = br.ReadBytes(0x0300).ToList();

            Tiles16X16 = new List<tile16x16>();
            for (int i = 0; i < 0x00C0; i++)
            {
                Tiles16X16.Add(new tile16x16(tileBlocks[i + 0x0000], 0,
                                             tileBlocks[i + 0x00C0], 0,
                                             tileBlocks[i + 0x0180], 0,
                                             tileBlocks[i + 0x0240], 0,
                                             _bgTileSet, palette.ToList()));
            }

            for (int i = 0; i < 0x0040; i++)
            {
                Tiles16X16.Add(new tile16x16(0, 0, 0, 0, 0, 0, 0, 0, _bgTileSet, palette.ToList()));
            }
        }



        /**
        * calculateCurrentBackgrounds
        * 
        * Aux function. Calculate the current location tiles8x8 map using the tiles16x16 map.
        *
        * @param br: The reader to load the data from.
        * @param headerOffset: The offset due to a header in the ROM.
        * @param mapDescriptors: The 26 bytes descriptor of the current location.
        */
        private void CalculateCurrentBackgrounds(BinaryReader br, int headerOffset, List<byte> mapDescriptors, bool worldMap = false)
        {
            int index = 0;

            for (int j = 0; j < 0x80; j += 2)
            {
                for (int i = 0; i < 0x80; i += 2)
                {
                    if (Tilemap00.Count > 0)
                    {
                        TilePropertiesL[i + 0 + (j + 0) * 0x80] = TileProperties[Tilemap00[index] * 2];
                        TilePropertiesL[i + 1 + (j + 0) * 0x80] = TileProperties[Tilemap00[index] * 2];
                        TilePropertiesL[i + 0 + (j + 1) * 0x80] = TileProperties[Tilemap00[index] * 2];
                        TilePropertiesL[i + 1 + (j + 1) * 0x80] = TileProperties[Tilemap00[index] * 2];
                        TilePropertiesH[i + 0 + (j + 0) * 0x80] = TileProperties[0x1 + Tilemap00[index] * 2];
                        TilePropertiesH[i + 1 + (j + 0) * 0x80] = TileProperties[0x1 + Tilemap00[index] * 2];
                        TilePropertiesH[i + 0 + (j + 1) * 0x80] = TileProperties[0x1 + Tilemap00[index] * 2];
                        TilePropertiesH[i + 1 + (j + 1) * 0x80] = TileProperties[0x1 + Tilemap00[index] * 2];

                        TilesBg0[i + 0][j + 0] = (tile8x8)Tiles16X16[Tilemap00[index]].tile00.Clone();
                        TilesBg0[i + 1][j + 0] = (tile8x8)Tiles16X16[Tilemap00[index]].tile01.Clone();
                        TilesBg0[i + 0][j + 1] = (tile8x8)Tiles16X16[Tilemap00[index]].tile02.Clone();
                        TilesBg0[i + 1][j + 1] = (tile8x8)Tiles16X16[Tilemap00[index]].tile03.Clone();
                    }

                    if (!worldMap)
                    {
                        if (Tilemap01.Count > 4096)
                        {
                            TilesBg1[i + 0][j + 0] = (tile8x8)Tiles16X16[Tilemap01[index]].tile00.Clone();
                            TilesBg1[i + 1][j + 0] = (tile8x8)Tiles16X16[Tilemap01[index]].tile01.Clone();
                            TilesBg1[i + 0][j + 1] = (tile8x8)Tiles16X16[Tilemap01[index]].tile02.Clone();
                            TilesBg1[i + 1][j + 1] = (tile8x8)Tiles16X16[Tilemap01[index]].tile03.Clone();
                        }
                        if (Tilemap02.Count == 4096)
                        {
                            TilesBg2[i + 0][j + 0] = (tile8x8)Tiles16X16[Tilemap02[index]].tile00.Clone();
                            TilesBg2[i + 1][j + 0] = (tile8x8)Tiles16X16[Tilemap02[index]].tile01.Clone();
                            TilesBg2[i + 0][j + 1] = (tile8x8)Tiles16X16[Tilemap02[index]].tile02.Clone();
                            TilesBg2[i + 1][j + 1] = (tile8x8)Tiles16X16[Tilemap02[index]].tile03.Clone();
                        }
                    }

                    index++;
                }
            }

            // $C0/5B62 AD 10 11    LDA $1110  [$00:1110]   
            // $C0/5B65 29 3F       AND #$3F                (6 bits)
            // $C0/5B67 85 08       STA $08    [$00:0B08]   $08
            // $C0/5B69 0A          ASL A                   
            // $C0/5B6A 18          CLC                     
            // $C0/5B6B 65 08       ADC $08    [$00:0B08]   
            // $C0/5B6D AA          TAX                     (X = ($1110 & #$3F) * 3)
            // $C0/5B6E BF B8 5B C0 LDA $C05BB8,x[$C0:5BBB]
            // $C0/5B72 85 48       STA $48      [$00:0B48] (reg = C05BB8[($1110 & 0x3F) * 3)])
            // [...]
            // $C0/4A29 A5 48       LDA $48    [$00:0B48]
            // $C0/4A2B 85 47       STA $47    [$00:0B47]
            // [...]
            // $C0/02F2 A5 47       LDA $47    [$00:0B47]
            // $C0/02F4 8D 31 21    STA $2131  [$00:2131]

            // $2131
            // shbo4321
            // s    = Add/subtract select
            // 0 => Add the colors
            // 1 => Subtract the colors
            // h    = Half color math. When set, the result of the color math is
            //        divided by 2 (except when $2130 bit 1 is set and the fixed color is
            //        used, or when color is cliped).
            // 4/3/2/1/o/b = Enable color math on BG1/BG2/BG3/BG4/OBJ/Backdrop
            if (!worldMap)
            {
                br.BaseStream.Position = 0x005BB8 + headerOffset + (mapDescriptors[0x04] & 0x3F) * 3;
                long reg = br.ReadByte();
                CurrentTransparency = (byte)(reg & 0x3F);
            }
        }



        /**
        * toTileMap
        * 
        * Aux function. Chop a picture into a collection of tiles.
        *
        * @param input: The picture to chop into pieces.
        * 
        * @return: The list of Images tiles.
        */
        private static List<Image> ToTileMap(Image input)
        {
            List<Image> output = new List<Image>();

            int maxY = (input.Height / 8);

            for (int l = 0; l < maxY; l++)
            {
                for (int k = 0; k < 16; k++)
                {
                    Image newImage = input.Clone(c => c.Crop(new Rectangle(k * 8, l * 8, 8, 8)));

                    //newImage.Mutate(g =>
                    //{
                    //    g.DrawImage(input, new Point(0,0), new Rectangle(k * 8, l * 8, 8, 8), 1);
                    //});

                    output.Add(newImage);
                }
            }
            return output;
        }



        /**
        * toTileMap
        * 
        * Aux function. Chop a picture into a collection of tiles.
        *
        * @param input: The picture to chop into pieces.
        * 
        * @return: The list of Images tiles.
        */
        private static List<Image> ToSpriteMap(Image input, bool flip=true, bool px16X16=false)
        {
            List<Image> output = new List<Image>();
            List<Image> invertOutput = new List<Image>();

            int maxY = (px16X16) ? (input.Height / 16) : (input.Height / 8);
            int maxX = (px16X16) ? 8 : 4;

            for (int l = 0; l < maxY; l++)
            {
                for (int k = 0; k < maxX; k++)
                {
                    Image newImage = input.CloneAs<Rgba32>();

                    newImage.Mutate(g =>
                    {
                        if (!px16X16)
                        {
                            g.DrawImage(input, new Rectangle(k * 32, l * 8, 16, 8), 1);
                            g.DrawImage(input, new Rectangle(16 + k * 32, l * 8, 16, 8), 1);
                        }
                        else
                            g.DrawImage(input, new Rectangle(k * 16, l * 16, 16, 16), 1);
                    });

                    output.Add(newImage);

                    if (flip)
                    {
                        Image newInvImage = newImage.CloneAs<Rgba32>();
                        newInvImage.Mutate(m => m.RotateFlip(RotateMode.None, FlipMode.Vertical)); // TODO: tyler confirm
                        //newInvImage.RotateFlip(RotateFlipType.RotateNoneFlipX);
                        invertOutput.Add(newInvImage);
                    }
                }
            }

            output.AddRange(invertOutput);
            return output;
        }



        /**
        * getWorldMapTilemap
        * 
        * Get the part of the World Map tilemap which represents the given quadrant.
        *
        * @param id: World Map id.
        * @param quadrant: Quadrant to return.
        * 
        * @return: The tilemap which represents the given quadrant.
        */
        private List<byte> GetWorldMapTilemap(int id, int quadrant)
        {
            List<byte> output = new List<byte>();
            int horizontalOffset = (quadrant & 0x03) * 0x40;
            int verticalOffset = ((quadrant & 0x0C) >> 2) * 0x40;

            for (int i = 0; i < 0x40; i++)
                output.AddRange(_worldmapTileMaps[id].GetRange(horizontalOffset + (verticalOffset + i) * 0x0100, 0x40));

            return output;
        }
        

        
        #endregion



        #region MapDecypherFunctions



        /**
        * mapDecypher
        * 
        * Load all the data related with a FFV Location and draw all the
        * layers to display in the GUI.
        *
        * @param br: The reader to load the data from.
        * @param headerOffset: The offset due to a header in the ROM.
        * @param id: The id of the loacation to load.
        * @param mapBg00U: The layer background 00 (high priority) to draw on.
        * @param mapBg00D: The layer background 00 (low priority) to draw on.
        * @param mapBg01U: The layer background 01 (high priority) to draw on.
        * @param mapBg01D: The layer background 01 (low priority) to draw on.
        * @param mapBg02U: The layer background 02 (high priority) to draw on.
        * @param mapBg02D: The layer background 02 (low priority) to draw on.
        * @param walls: The fictitious layer 'walls' to draw on.
        * 
        * @return: True if the map was deciphered successfully.
        */
        public bool MapDecypher(GraphicsDevice gd, BinaryReader br, int headerOffset, int id, int quadrant,
            out BackgroundLayers map,
            out Wall[,] walls)
        {
            //mapBg00U = mapBg00D = mapBg01U = mapBg01D = mapBg02U = mapBg02D = new Texture2D(gd, 1096, 1096);
            walls = new Wall[64,64];
            bool output = true;

            // TODO: implement this fix for worldmap
            if (id < 5) return WorldMapDecypher(gd, br, headerOffset, id, quadrant, out map, out walls);
            

            Stopwatch sw = new Stopwatch();
            //string performanceMessage = ""; //DEBUG

  
                // Clear current location
                ClearCurrentLocation();


                // Set the current map descriptors values
                List<byte> mapDescriptors = this._mapDescriptors[id];


                // Load the current location tileset
                //sw.Start(); //DEBUG
                LoadCurrentTileset(br, headerOffset, mapDescriptors);
                //sw.Stop(); performanceMessage += "bg_tileset:\r\n" + sw.ElapsedMilliseconds + "ms\r\n\r\n"; sw.Reset();  //DEBUG


                // Set the current tilemaps
                SetCurrentTilemaps(br, headerOffset, mapDescriptors);


                // Load the current tile properties
                //sw.Start(); //DEBUG
                LoadCurrentTileProperties(br, headerOffset, mapDescriptors);
                //sw.Stop(); performanceMessage += "tileProperties:\r\n" + sw.ElapsedMilliseconds + "ms\r\n\r\n"; sw.Reset(); //DEBUG


                // Calculate every possible 16x16 tile in current map
                //sw.Start(); //DEBUG
                CalculateCurrentTiles16X16(br, headerOffset, mapDescriptors);
                //sw.Stop(); performanceMessage += "tiles16x16:\r\n" + sw.ElapsedMilliseconds + "ms\r\n\r\n"; sw.Reset(); //DEBUG


                // Load the actual map
                //sw.Start();
                CalculateCurrentBackgrounds(br, headerOffset, mapDescriptors);
                //sw.Stop(); performanceMessage += "Initialize tables:\r\n" + sw.ElapsedMilliseconds + "ms\r\n\r\n";sw.Reset(); //DEBUG

                List<TileTexture> tiles16x16XNA = new();
                
                foreach (var tile in Tiles16X16)
                {
                    tiles16x16XNA.Add(new TileTexture(gd, tile));
                }
                
                // Draw backgrounds
                //sw.Start(); //DEBUG

                map = new BackgroundLayers(tiles16x16XNA, Tilemap00, Tilemap01, Tilemap02);
                /*mapBg00U = new BackgroundLayer(Tilemap00, Tiles16X16, false, false);
                mapBg00D = new BackgroundLayer(Tilemap00, Tiles16X16, true, false);
                mapBg01U = new BackgroundLayer(Tilemap01, Tiles16X16, false, false);
                mapBg01D = new BackgroundLayer(Tilemap01, Tiles16X16, true, false);
                mapBg02U = new BackgroundLayer(Tilemap02, Tiles16X16, false, true);
                mapBg02D = new BackgroundLayer(Tilemap02, Tiles16X16, true, true);*/
                //sw.Stop(); performanceMessage += "Draw backgrounds:\r\n" + sw.ElapsedMilliseconds + "ms\r\n\r\n"; sw.Reset(); //DEBUG


                // Draw walls
                //sw.Start(); //DEBUG
                br.BaseStream.Position = 0x001725 + headerOffset;
                //walls = drawWalls(tilemap00, tileProperties, br.ReadBytes(0x0100).ToList());
                walls = GetWalls(Tilemap00, TileProperties, br.ReadBytes(0x0100).ToList());
                //sw.Stop();performanceMessage += "Draw walls:\r\n" + sw.ElapsedMilliseconds + "ms\r\n\r\n";sw.Reset(); //DEBUG

                //System.Windows.Forms.MessageBox.Show(performanceMessage, "Performance metrics"); //DEBUG

            return true;
        }

        public class TileTexture
        {
            public Texture2D ImageP1Bg02;
            public Texture2D ImageP0Bg02;
            
            public Texture2D ImageP1;
            public Texture2D ImageP0;

            public TileTexture(GraphicsDevice gd, tile16x16 tile)
            {
                ImageP1 = ConvertToTex(gd, tile.ImageP1);
                ImageP0 = ConvertToTex(gd, tile.ImageP0);
                ImageP0Bg02 = ConvertToTex(gd, tile.ImageP0Bg02);
                ImageP1Bg02 = ConvertToTex(gd, tile.ImageP1Bg02);
            }
        }

        public static Texture2D ConvertToTex(GraphicsDevice device, Image<Rgba32> image)
        {
            Texture2D tex = new Texture2D(device, image.Width, image.Height);

            var data = new Microsoft.Xna.Framework.Color[image.Width * image.Height];
            for (var x = 0; x < image.Width; x++)
            {
                for (var y = 0; y < image.Height; y++)
                {
                    var color = image[x, y];
                    data[y * image.Width + x] = new Microsoft.Xna.Framework.Color(color.R,color.G,color.B,color.A);
                }
            }
			
            tex.SetData(data);
            return tex;
        }


        /**
        * worldMapDecypher
        * 
        * Load all the data related with a FFV World Map and draw the
        * layer to display in the GUI.
        *
        * @param br: The reader to load the data from.
        * @param headerOffset: The offset due to a header in the ROM.
        * @param id: The id of the loacation to load.
        * @param mapBg00U: The layer background 00 (high priority) to draw on.
        * @param mapBg00D: The layer background 00 (low priority) to draw on.
        * @param mapBg01U: The layer background 01 (high priority) to draw on.
        * @param mapBg01D: The layer background 01 (low priority) to draw on.
        * @param mapBg02U: The layer background 02 (high priority) to draw on.
        * @param mapBg02D: The layer background 02 (low priority) to draw on.
        * @param walls: The fictitious layer 'walls' to draw on.
        * 
        * @return: True if the map was deciphered successfully.
        */
        public bool WorldMapDecypher(GraphicsDevice gd, BinaryReader br, int headerOffset, int id, int quadrant,
            out BackgroundLayers bl, out Wall[,] walls)
        {
            walls = new Wall[1,1];
            bl = new BackgroundLayers();
            bool output = true;

            try
            {
                // Clear current location
                ClearCurrentLocation();

                // Set the current map descriptors values
                List<byte> mapDescriptors = this._mapDescriptors[id];

                // Load the current world map tileset
                _bgTileSet = ToTileMap(LoadWorldMapTileset(br, headerOffset, id));

                // Set the current tilemaps
                Tilemap00 = GetWorldMapTilemap(id, quadrant);

                // Load the current tile properties
                LoadCurrentWorldMapTileProperties(br, headerOffset, id);

                // Calculate every possible 16x16 tile in current map
                CalculateCurrentWorldMapTiles16X16(br, headerOffset, id);

                // Load the actual map
                CalculateCurrentBackgrounds(br, headerOffset, null, true);

                // Draw backgrounds
                List<TileTexture> tiles16x16XNA = new();
                
                foreach (var tile in Tiles16X16)
                {
                    tiles16x16XNA.Add(new TileTexture(gd, tile));
                }

                bl = new BackgroundLayers(tiles16x16XNA, Tilemap00, Tilemap01, Tilemap02);

                // Draw walls
                //walls = drawWorldMapWalls(tilemap00, tileProperties);
                walls = GetWorldMapWalls(Tilemap00, TileProperties);
            }
            catch (Exception e)
            {
                //System.Windows.Forms.MessageBox.Show("Error reading file: " + e.ToString(), "Error!");
                output = false;
            }

            return output;
        }



        #endregion


                
        #region ClassGetters


        
        /**
        * getBgTileset
        * 
        * Aux function. Draw all the 16x16 tiles of the current tileset into a Image.
        * 
        * @return: The Image contining the drawn tileset.
        */
        public Image GetBgTileset(){
            Image output = new Image<Rgba32>(128, 512);

            output.Mutate(g =>
            {
                for (int j = 0; j < 64; j++)
                {
                    if (j * 16 >= _bgTileSet.Count) break;
                    for (int i = 0; i < 16; i++)
                    {
                        g.DrawImage(_bgTileSet[j * 16 + i], new Point(i * 8, j * 8), 1);
                    }
                }
            });

            return output;
        }



        /**
        * displayWorlMapTileProperty
        * 
        * Check a World Map tile to get its properties.
        *
        * @param id: The id of the current World Map.
        *
        * @return: The World Map 16x16 tile properties or a blank string.
        */
        public string DisplayWorlMapTileProperty(int tileId)
        {
            string output = "";
            if (tileId >= 0xC0) return output;

            byte byte00 = TileProperties[tileId * 3 + 0];
            byte byte01 = TileProperties[tileId * 3 + 1];
            byte byte02 = TileProperties[tileId * 3 + 2];

            string[] enemyFormation = { "00: Grass", "01: Forest", "02: Desert/Swamp", "03: Sea" };

            output += tileId.ToString("X2") + " (";
            output += byte00.ToString("X2") + " ";
            output += byte01.ToString("X2") + " ";
            output += byte02.ToString("X2") + ")\r\n";

            output += "\r\nSubmarine can emerge? " + (((byte00 & 0x80) != 0x00) ? "Yes" : "No");
            output += "\r\nAirship can fly over? " + (((byte00 & 0x40) != 0x00) ? "Yes" : "No");
            output += "\r\nBoat can navigate? " + (((byte00 & 0x20) != 0x00) ? "Yes" : "No");
            output += "\r\nSubmarine can navigate? " + (((byte00 & 0x10) != 0x00) ? "Yes" : "No");
            output += "\r\nHiryuu can fly over? " + (((byte00 & 0x08) != 0x00) ? "Yes" : "No");
            output += "\r\nBlack chocobo can fly over? " + (((byte00 & 0x04) != 0x00) ? "Yes" : "No");
            output += "\r\nYellow chocobo can walk? " + (((byte00 & 0x02) != 0x00) ? "Yes" : "No");
            output += "\r\nMain character can walk? " + (((byte00 & 0x01) != 0x00) ? "Yes" : "No");

            output += "\r\nAirship can land? " + (((byte01 & 0x80) == 0x00) ? "Yes" : "No");
            output += "\r\nHiryuu can land? " + (((byte01 & 0x40) == 0x00) ? "Yes" : "No");
            output += "\r\nBlack chocobo can land? " + (((byte01 & 0x20) == 0x00) ? "Yes" : "No");

            output += "\r\n\r\nEnemy formation: " + enemyFormation[(byte02 & 0x03)];

            // | Forest    C.Green   Green     L.Mnt     H.mnt     Waterfal  River     Desert    Void      Sea      | Floor UW |
            // | 4F CF A1  4F 2F 80  4F 2F 80  44 EF 10  40 EF 10  45 EF 20  4E FF 40  4F EF 82  40 F0 83  7C 70 83 | 91 60 00 |
            // | ---------------------------------------------------------------------------------------------------|----------|
            // | 0         0         0         0         0         0         0         0         0         0        | 1        | Submarine can emerge
            // | 1         1         1         1         1         1         1         1         1         1        | 0        | Airship can fly over
            // | 0         0         0         0         0         0         0         0         0         1        | 0        | Boat can navigate
            // | 0         0         0         0         0         0         0         0         0         1        | 1        | Submarine can navigate
            // | 1         1         1         0         0         0         1         1         0         1        | 0        | Hiryuu can fly over
            // | 1         1         1         1         0         1         1         1         0         1        | 0        | Black chocobo can fly over
            // | 1         1         1         0         0         0         1         1         0         0        | 0        | Yellow chocobo can walk
            // | 1         1         1         0         0         1         0         1         0         0        | 1        | Main character can walk
            // |                                                                                                    |          |
            // | 1         0         0         1         1         1         1         1         1         0        | 0        | Airship cannot land
            // | 1         0         0         1         1         1         1         1         1         1        | 1        | Hiryuu cannot land
            // | 0         1         1         1         1         1         1         1         1         1        | 1        | Black chocobo cannot land
            // | 0         0         0         0         0         0         1         0         1         1        | 0        | ????
            // | 1111      1111      1111      1111      1111      1111      1111      1111      0000      0000     | 0000     | ????
            // |                                                                                                    |          |
            // | 1         1         1         0         0         0         0         1         1         1        | 0        | ????
            // | 0         0         0         0         0         0         1         0         0         0        | 0        | ????
            // | 1         0         0         0         0         1         0         0         0         0        | 0        | ????
            // | 0         0         0         1         1         0         0         0         0         0        | 0        | ????
            // |                                                                                                    |          |
            // | 0001      0000      0000      0000      0000      0000      0000      0010      0011      0011     | 0000     | Enemy encounter
            // | ---------------------------------------------------------------------------------------------------|----------|

            return output;
        }



        /**
        * getCurrentPalette
        * 
        * Aux function. Draw the palette of a map into a Image.
        * 
        * @param id: The id of the map which palette is going to be drawn.
        * 
        * @return: The Image contining the drawn palette.
        */
        // TODO: tyler
        /*public Image getCurrentPalette(int id){
            Image output = new Image<Rgba32>(16 * 8, 8 * 8);
            List<Color> palette;

            palette = (id > 2) ? bgPalettes[mapDescriptors[id][0x16]] : worldMapBgPalettes[id];

            output.Mutate(g =>
            {
                for (int j = 0; j < 8; j++)
                    for (int i = 0; i < 16; i++)
                    {
                        g.DrawRectangle(new Pen(palette[j * 16 + i], 8), i * 8 + 4, j * 8 + 4, 8, 8);
                        
                    }
            });

            return output;
        }*/



        /**
        * getLocationName
        * 
        * Return the name of a Location.
        *
        * @param id: The id of the map which name is going to be returned.
        *
        * @return: The Location name or a blank string if the name is empty.
        */
        public string GetLocationName(int id)
        {
            string output = "Name: ";
            int index = _mapDescriptors[id][0x02];
            output +=  (index != 0) ? _locationNames[index] : "<Empty>";
            return output;
        }



        /**
        * getMapDescriptors
        * 
        * Return the descriptors of a Location.
        *
        * @param id: The id of the map which descriptor is going to be returned.
        *
        * @return: The descriptors of the current.
        */
        public List<byte> GetMapDescriptors(int id)
        {
            return new List<byte>(_mapDescriptors[id]);
        }



        #endregion


       


        #region NPCs



        /**
        * mapGetNPCs
        *
        * Get the list of NPCs of a given map and draw them in a BPM.
        *
        * @param br: The reader to load the data from.
        * @param headerOffset: The offset due to a header in the ROM.
        * @param id: The id of the location to get the NPCs from.
        *
        * @return: The list of Images tiles.
        */
        public List<NPC> MapGetNpCs(BinaryReader br, int headerOffset, int id, Image mapNpCs)
        {
            //mapNPCs = new Image<Rgba32>(64 * 8 * 2, 64 * 8 * 2);
            string output = "";

            br.BaseStream.Position = 0x0E59C0 + headerOffset + id * 2;
            long begin = 0x0E59C0 + br.ReadByte() + br.ReadByte() * 0x0100 + headerOffset;
            long end = 0x0E59C0 + br.ReadByte() + br.ReadByte() * 0x0100 + headerOffset;

            output = "Id " + id.ToString("X3") + ": " +
                     begin.ToString("X4") + " to " + end.ToString("X4") + "\r\n\r\n";

            // Load every NPC in map 'id'
            for (long currentPos = begin; currentPos < end; currentPos += 7)
            {
                br.BaseStream.Position = currentPos;
                long address = 0xC00000 + currentPos - headerOffset;

                byte[] npc = br.ReadBytes(7);
                byte palette = (byte)(npc[6] & 0x03);

                NPC newNpc = new NPC(address, npc, br, headerOffset, _speechTxt);

                // Add the NPC to the list
                Npcs.Add(newNpc);
                output += newNpc.ToString();

                //bytes 0-1 ActionID
                //byte    2 Graphic ID
                //byte    3 X
                //byte    4 Y
                //byte    5 Walking (affects how the NPC moves)
                //byte    6 Palette (mask with 0x07)
                //byte    6 Extra parameters (mask with 0x07)(0x10 determines layer, 0x20, 0x40, 0x60 determines direction)

                // Draw this NPC in the Image
                //using (var graphics = Graphics.FromImage(mapNPCs))
                //{
                int x = npc[3] * 16;
                int y = npc[4] * 16;
                int spriteSide = (npc[2] == 0x67) ? 32 : 16;

                // Reallocate x and y for Hiryuu sprites and force the Hiryuu pallete
                if (npc[2] == 0x67)
                {
                    x -= 8;
                    y -= 16;
                    palette = 0x02;
                }

                if (npc[2] == 0x68)
                {
                    ;
                    y -= 8;
                    palette = 0x02;
                }

                // Draw red dotted shape under the NPC sprite
                //Pen redPen = new Pen(Color.Red, 1.0f);
                //redPen.DashPattern = new float[] { 4.0F, 2.0F, 1.0F, 3.0F };
                //graphics.DrawRectangle(redPen, x, y, spriteSide - 1, spriteSide - 1);

                if (npc[2] < 0xF0)
                {
                    //Take the 16x16 piece to display.
                    //Image bmp = new Image<Rgba32>(sprites[npc[2]][palette][((npc[6] & 0xE0) >> 0x05)]);
                    //Image bmp = getNPCSprite(npc[2], npc[6]);

                    //Set transparency
                    //Rectangle dstRect = new Rectangle(x, y, bmp.Width, bmp.Height);
                    //ImageAttributes attr = new Image<Rgba32>Attributes();
                    //attr.SetColorKey(spritePalettes[palette][0], spritePalettes[palette][0]);

                    //Draw image
                    //graphics.DrawImage(bmp, dstRect, 0, 0, bmp.Width, bmp.Height, GraphicsUnit.Pixel, attr);
                }
                //}

                output += "\r\n------------------------------------------\r\n";
            }

            return Npcs;
        }

        //System.Windows.Forms.MessageBox.Show(output);

            // Load map charset
            // HACKME What is 00:0ADA?
            // position = C0/1E02[00:0ADA * 2]
            /*br.BaseStream.Position = 0x001E02 + headerOffset + 4;
            int offset = br.ReadByte() + br.ReadByte() * 0x0100;
            br.BaseStream.Position = 0x1A0000 + headerOffset + offset;
            charSet = toTileMap(Transformations.transform4b(br.ReadBytes(0x0800).ToList(), 0, 0x0800));

            return npcs;
            */


            //br.BaseStream.Position = 0x0E2400 + headerOffset + ((int)numericUpDownMap.Value) * 2;
            //long begin = 0x0E2400 + br.ReadByte() + br.ReadByte() * 0x0100 + headerOffset;
            //long   end = 0x0E2400 + br.ReadByte() + br.ReadByte() * 0x0100 + headerOffset;

            //br.BaseStream.Position = 0x0E36C0 + headerOffset + ((int)numericUpDownMap.Value) * 2;
            //long begin = 0x0E36C0 + br.ReadByte() + br.ReadByte() * 0x0100 + headerOffset;
            //long   end = 0x0E36C0 + br.ReadByte() + br.ReadByte() * 0x0100 + headerOffset;

            //Origin Coordinates (2B)
            //Destiny Map ID (2B)
            //Destiny Coordinates (2B)
            //output += "Exit at tile" + npc[0] + ", " + npc[1] + "\r\n";
            //output += "To map id " + (npc[2] + npc[3] * 0x0100).ToString("X4") + "\r\n";
            //output += "to tile " + npc[4] + ", " + npc[5] + "\r\n";
        //}



        /**
        * getNPCProperties
        * 
        * Check a tile to get the properties of a possible NPC in that tile.
        *
        * @param x: X location of the possible NPC.
        * @param y: Y location of the possible NPC.
        * 
        * @return: The NPC properties or a blank string.
        */
        /*public List<string> getNPCProperties(int x, int y)
        {
            List<string> output = new List<string>();

            foreach (NPC item in npcs)
                if (item.x == x && item.y == y)
                    output.Add(item.ToString());

            return output;
        }*/



        /**
        * getNPCSprite
        * 
        * Get the sprite which corresponds to a given graphic id.
        *
        * @param graphicID: The index of the NPC sprite.
        * @param properties: Palette and direction of the sprite.
        * 
        * @return: The Image of the NPC.
        */
        /*public Image getNPCSprite(int graphicID, int properties)
        {
            Image output = new Image<Rgba32>(1, 1);

            int palette = (byte)((properties & 0x07) >> 0x00);
            int direction = (byte)((properties & 0xE0) >> 0x05);

            if (graphicID < 0x67)
            {
                // Normal NPC Sprite
                output = new Image<Rgba32>(sprites[graphicID][palette][direction]);
            }
            else if (graphicID == 0x67)
            {
                // Hiryuu body
                output = new Image<Rgba32>(32, 32);
                using (var g = Graphics.FromImage(output))
                {
                    g.DrawImage(sprites[graphicID][0x02][0x00], new Rectangle(0, 0, 16, 16), new Rectangle(0, 0, 16, 16), GraphicsUnit.Pixel);
                    g.DrawImage(sprites[graphicID][0x02][0x01], new Rectangle(0, 16, 16, 16), new Rectangle(0, 0, 16, 16), GraphicsUnit.Pixel);

                    Image newInvImage00 = new Image<Rgba32>(sprites[graphicID][0x02][0x00], 16, 16);
                    Image newInvImage01 = new Image<Rgba32>(sprites[graphicID][0x02][0x01], 16, 16);
                    newInvImage00.RotateFlip(RotateFlipType.RotateNoneFlipX);
                    newInvImage01.RotateFlip(RotateFlipType.RotateNoneFlipX);
                    g.DrawImage(newInvImage00, new Rectangle(16, 0, 16, 16), new Rectangle(0, 0, 16, 16), GraphicsUnit.Pixel);
                    g.DrawImage(newInvImage01, new Rectangle(16, 16, 16, 16), new Rectangle(0, 0, 16, 16), GraphicsUnit.Pixel);
                }
            }
            else if (graphicID == 0x68)
            {
                // Hiryuu head
                output = new Image<Rgba32>(sprites[graphicID][0x02][0x01]);
            }

            return output;
        }*/



        #endregion



        #region ExitsEventsAndChests



        /**
        * mapGetExits
        * 
        * Fills the exits attribute and draw the exits as yellow squares in a BPM.
        *
        * @param br: The reader to load the data from.
        * @param headerOffset: The offset due to a header in the ROM.
        * @param id: The id of the location to get the NPCs from.
        * @param quadrant: The quadrant in case the map is a World Map.
        * @param mapNPCs: The BPM to draw in.
        */
        public void MapGetExits(BinaryReader br, int headerOffset, int id, int quadrant, Image mapNpCs)
        {
            if (id >= 0x01ff) return;

            br.BaseStream.Position = 0x0E36C0 + headerOffset + id * 2;
            long begin = 0x0E36C0 + br.ReadByte() + br.ReadByte() * 0x0100 + headerOffset;
            long end =   0x0E36C0 + br.ReadByte() + br.ReadByte() * 0x0100 + headerOffset;

            int horizontalOffset = (quadrant & 0x03) * 0x40;
            int verticalOffset = ((quadrant & 0x0C) >> 2) * 0x40;

            br.BaseStream.Position = begin + headerOffset;
            for (long currentPos = begin; currentPos < end; currentPos += 6)
            {
                //6 bytes
                //  2B: Origin Coordinates
                //  2B: Destiny Map ID????
                //  2B: Destiny Coordinates???? (Máximo 3Fx3F en mapas locales)
                long address = 0xC00000 + currentPos - headerOffset;
                byte[] data = br.ReadBytes(6);
                int originX = data[0];
                int originY = data[1];
                //int  mapId = br.ReadByte() + br.ReadByte() * 0x0100;
                //byte destinationX = br.ReadByte();
                //byte destinationY = br.ReadByte();

                Exits.Add(new MapExit(address, data));

                if (id < 5)
                {
                    originX -= horizontalOffset;
                    originY -= verticalOffset;
                }

                if (originX >= 0 && originX <= 0x40 && originY >= 0 && originY <= 0x40)
                {
                    /*using (var graphics = Graphics.FromImage(mapNPCs))
                    {
                        SolidBrush yellowBrush = new SolidBrush(Color.Yellow);
                        graphics.FillRectangle(yellowBrush, originX * 16, originY * 16, 16, 16);
                        yellowBrush.Dispose();
                    }*/
                }
            }
        }



        /**
        * getMapExitProperties
        * 
        * Return the Exit information (if exists) of a given tile.
        *
        * @param x: X location of the possible Exit.
        * @param y: Y location of the possible Exit.
        * @param isWorldMap: A flag that indicates if the current map is a World Map one.
        * 
        * @return this Exit data properly formatted or a blank string.
        */
        public string GetMapExitProperties(int x, int y, bool isWorldMap)
        {
            string output = "";
            int mask = (isWorldMap) ? 0xFF : 0x3F;

            foreach (MapExit item in Exits)
            {
                if ((item.originX & mask) == x && (item.originY & mask) == y)
                {
                    output = item.ToString();
                    break;
                }
            }

            return output;
        }



        /**
        * mapGetChests
        * 
        * Fills the chests attribute and draw them as green squares in a BPM.
        *
        * @param br: The reader to load the data from.
        * @param headerOffset: The offset due to a header in the ROM.
        * @param id: The id of the location to get the Chests from.
        * @param quadrant: The quadrant in case the map is a World Map.
        * @param mapChests: The BPM to draw in.
        */
        public void MapGetChests(BinaryReader br, int headerOffset, int id, Image mapChests)
        {
            // D1/3000-D1/3200 Treasure boxes offsets (hay que multiplicarlos por 4)
            // D1/3210-D1/35FF Treasure boxes (252 x 4 bytes)

            br.BaseStream.Position = 0x113000 + headerOffset + id;
            long begin = 0x113210 + (br.ReadByte() * 4) + headerOffset;
            long end   = 0x113210 + (br.ReadByte() * 4) + headerOffset;

            br.BaseStream.Position = begin + headerOffset;
            for (long currentPos = begin; currentPos < end; currentPos += 4)
            {
                long address    = 0xC00000 + currentPos - headerOffset;
                byte[] data       = br.ReadBytes(4);
                byte locationX    = data[0];
                byte locationY    = data[1];
                //byte properties = br.ReadByte();
                //byte priceId    = br.ReadByte();

                Treasures.Add(new Treasure(address, data));

                /*using (var graphics = Graphics.FromImage(mapChests))
                {
                    SolidBrush greenBrush = new SolidBrush(Color.Green);
                    graphics.FillRectangle(greenBrush, locationX * 16, locationY * 16, 16, 16);
                    greenBrush.Dispose();
                }*/
            }
        }



        /**
        * getEveryChestInfo
        * 
        * Aux function. Load all the game Chests.
        *
        * @param br: The reader to load the data from.
        * @param headerOffset: The offset due to a header in the ROM.
        * 
        * @return all game Chest data properly formatted.
        */
        public string GetEveryChestInfo(BinaryReader br, int headerOffset)
        {
            string message = "";
            // D1/3210-D1/35FF Treasure boxes (252 x 4 bytes)
            
            br.BaseStream.Position = 0x113210 + headerOffset;
            for (int id = 0; id < 512; id++)
            {
                // D1/3000-D1/3200 Treasure boxes offsets (hay que multiplicarlos por 4)
                // D1/3210-D1/35FF Treasure boxes (252 x 4 bytes)

                br.BaseStream.Position = 0x113000 + headerOffset + id;
                long begin = 0x113210 + (br.ReadByte() * 4) + headerOffset;
                long end = 0x113210 + (br.ReadByte() * 4) + headerOffset;

                br.BaseStream.Position = begin + headerOffset;
                for (long currentPos = begin; currentPos < end; currentPos += 4)
                {
                    //4 bytes
                    //  2B: Origin Coordinates
                    //  1B: Chest properties
                    //  1B: Price id
                    long address = 0xC00000 + currentPos - headerOffset;
                    byte[] data = br.ReadBytes(4);

                    Treasure treasure = new Treasure(address, data);

                    message += "------------------------------------------\r\n";
                    message += " Map id: " + id.ToString("X4") + "\r\n";
                    message += "------------------------------------------\r\n";
                    message += treasure.ToString(ItemNames, SpellNames) + "\r\n\r\n";
                }
            }

            return message;
        }



        /**
        * getChestsProperties
        * 
        * Return the Chest information (if exists) of a given tile.
        *
        * @param x: X location of the possible Chest.
        * @param y: Y location of the possible Chest.
        * 
        * @return this Chest data properly formatted or a blank string.
        */
        public string GetChestsProperties(int x, int y)
        {
            string output = "";

            foreach (Treasure item in Treasures)
            {
                if (item.locationX == x && item.locationY == y)
                {
                    output = item.ToString(ItemNames, SpellNames);
                    break;
                }
            }

            return output;
        }



        /**
        * mapGetEvents
        * 
        * Fills the events attribute and draw them as blue squares in a BPM.
        *
        * @param br: The reader to load the data from.
        * @param headerOffset: The offset due to a header in the ROM.
        * @param id: The id of the location to get the Events from.
        * @param quadrant: The quadrant in case the map is a World Map.
        * @param mapEvents: The BPM to draw in.
        */
        public void MapGetEvents(BinaryReader br, int headerOffset, int id, int quadrant, Image mapEvents)
        {
            // CE/2400-CE/27FF Event places offset [2 bytes * 512]
            // CE/2800-CE/365F Event places [4 bytes * 920]  (2B: Coordinates, 2B Event id (in table D8/E080))
            // D8/E080-D8/E5DF Event data offsets [2 bytes * 687 (0x2AF)]
            // D8/E5E0-D8/FFFF Event data [x bytes * 687 (0x2AF)] (Links to Extended Event data in table C8/3320)
            // C8/3320-C8/49DE Extended Event data offsets  [3 bytes * 1940] (Big endian)
            // C8/49DF-C9/FFFF Extended Event data [x bytes * 1940]
            if (id == 0x01ff) return;

            br.BaseStream.Position = 0x0E2400 + (id * 2) + headerOffset;
            int horizontalOffset = (quadrant & 0x03) * 0x40;
            int verticalOffset = ((quadrant & 0x0C) >> 2) * 0x40;

            long begin = 0x0E2400 + (br.ReadByte() + br.ReadByte() * 0x0100) + headerOffset;
            long end = 0x0E2400 + (br.ReadByte() + br.ReadByte() * 0x0100) + headerOffset;

            br.BaseStream.Position = begin + headerOffset;
            for (long currentPos = begin; currentPos < end; currentPos += 4)
            {
                //4 bytes
                //  2B: Origin Coordinates
                //  2B: Event id
                long address = 0xC00000 + currentPos - headerOffset;
                byte[] data = br.ReadBytes(4);
                int locationX = data[0];
                int locationY = data[1];
                //int properties = br.ReadByte() + br.ReadByte() * 0x0100;
                
                Events.Add(new Event(address, data, br, headerOffset));

                if (id < 5)
                {
                    locationX -= horizontalOffset;
                    locationY -= verticalOffset;
                }

                /*using (var graphics = Graphics.FromImage(mapEvents))
                {
                    SolidBrush blueBrush = new SolidBrush(Color.Blue);
                    graphics.FillRectangle(blueBrush, locationX * 16, locationY * 16, 16, 16);
                    blueBrush.Dispose();
                }*/
            }
        }



        /**
        * getEventsProperties
        * 
        * Return the Event information (if exists) of a given tile.
        *
        * @param x: X location of the possible Event.
        * @param y: Y location of the possible Event.
        * 
        * @return this Event.
        */
        public Event? GetEventsProperties(int x, int y)
        {
            Event output = null;

            foreach (Event item in Events)
            {
                if (item.x == x && item.y == y)
                {
                    output = item;
                    break;
                }
            }

            return output;
        }



        /**
        * getMapProperties
        * 
        * Return a string with the information related with the current Map Descriptors.
        *
        * @param id: The id of the location to get the properties from.
        * 
        * @return: The Map properties or a blank string.
        */
        public string GetMapProperties(int id)
        {
            string output = "";
            if (id < 5) return output;

            List<byte> mapDescriptors = this._mapDescriptors[id];

            output += "Map descriptors:\r\n";
            foreach (byte item in mapDescriptors) output += item.ToString("X2") + " ";
            output += "\r\n\r\n";

            output += "Id (bytes 00 01)\r\n";
            output += "Id " + (mapDescriptors[0x00] + mapDescriptors[0x01] * 0x0100).ToString("X4") + "\r\n";
            output += "\r\n";

            output += "Map name (byte 02)\r\n";
            output += "Index " + mapDescriptors[0x02].ToString("X2") + "\r\n";
            output += GetLocationName(mapDescriptors[0x00] + mapDescriptors[0x01] * 0x0100) + "\r\n";
            output += "\r\n";

            output += "<Unknown> (byte 03)\r\n";
            output += "\r\n";

            output += "Graphic maths (byte 04)\r\n";
            output += "Index " + (mapDescriptors[0x04]).ToString("X2") + "\r\n";
            //The higher bit is a flag which marks if the animated tiles in the VRAM still updating the animation or stops.
            //Color Math Designation ($2131) id is mapDescriptors[0x04] & 0x3F. (Read from C0/5BB8[id * 3]
            output += "\r\n";

            output += "Tile Properties (byte 05)\r\n";
            output += "Index " + (mapDescriptors[0x05]).ToString("X2") + " =>";
            output += "Read 0x0100 compressed bytes from CF/C56E[CF/" + (0xC540 * mapDescriptors[0x05] * 2).ToString("X4") + "]\r\n";
            //CF/C540-CF/C56D  Data  Tile properties offsets [2bytes * 23]
            //CF/C56E-CF/D7FF  Data  Tile properties [Compressed unheaded type02] [2 bytes * 256 after decompressed]
            output += "\r\n";

            output += "<Unknown> (bytes 06 07)\r\n";
            output += "\r\n";

            output += "Tile Blocks (byte 08)\r\n";
            output += "Index " + (mapDescriptors[0x08]).ToString("X2") + "\r\n";
            //CF/0000 to CF/0037 Tile blocks offset
            //CF/0038 to CF/C53F Tile blocks [Compressed Type02 Unheaded]
            //Every tile blocks table is contains 0x0800 bytes. The bytes corresponds to 4 sub-tables of 0x0200 bytes each.
            //Each sub-table is related with one of the 8x8 tiles the block is composed by. The tables are a sequence of 0x100 registers of two bytes each following a SNES tile standard.
            output += "\r\n";

            output += "VRAM graphics (bytes 09 0A 0B)\r\n";
            // Load the current location VRAM tileset (graphic chunk)
            output += "VRAM: 0000 -> 1000 Index (" + (mapDescriptors[0x09] & 0x3F).ToString("X2") + ")\r\n";
            output += "VRAM: 1000 -> 2000 Index (" + (((mapDescriptors[0x09] & 0xC0) >> 6) + ((mapDescriptors[0x0A] & 0x0F) << 2)).ToString("X2") + ")\r\n";
            output += "VRAM: 2000 -> 3000 Index (" + (((mapDescriptors[0x0A] & 0xF0) >> 4) + ((mapDescriptors[0x0B] & 0x03) << 4)).ToString("X2") + ")\r\n";
            output += "VRAM: 4000 -> 4400 Index (" + (mapDescriptors[0x0B] & 0xFC).ToString("X2") + ")\r\n";
            // byte leído de la tabla DC/2D84. Multiplicado por 4, Offset de 3 bytes que apunta a la tabla DC/2E24 (gráficos 4bpp para la VRAM)
            // byte leído de la tabla DC/0000. Multiplicado por 2, Offset de 2 bytes que apunta a la tabla DC/0024 (gráficos 2bpp para la VRAM)
            output += "\r\n";

            output += "Location tilemaps (bytes 0C 0D 0E 0F)\r\n";
            // Map tilemaps indexes [unheaded Type02]
            // CB/0000 Tile maps offsets [2bytes * 0x0148]
            // CB/0290 Tile maps [Compressed unheaded type02] [1byte * 64 * 64] [Until CD/FDFF]
            output += "Tilemap00 Index " + (((mapDescriptors[0x0C] + (mapDescriptors[0x0D] * 0x0100)) & 0x03FF) - 1).ToString("X4") + "\r\n";
            output += "Tilemap01 Index " + ((((mapDescriptors[0x0D] + mapDescriptors[0x0E] * 0x0100) & 0x0FFC) >> 2) - 1).ToString("X4") + "\r\n";
            output += "Tilemap02 Index " + ((((mapDescriptors[0x0E] + mapDescriptors[0x0F] * 0x0100) & 0x3FF0) / 16) - 1).ToString("X4") + "\r\n";
            output += "\r\n";

            output += "<Unknown> (bytes 10 11 12 13 14 15)\r\n";
            output += "\r\n";

            output += "Palette (byte 16)\r\n";
            //This byte is the index to the background palette.
            //C3/BB00[index * 0x0100] (0x80 registers of 2 bytes (15-bit RGB) each)
            output += mapDescriptors[0x16].ToString("X2") + "\r\n";
            output += "Read 0x0080 2 byte registers (15-bit RGB) from C3/BB00[" + (mapDescriptors[0x16] * 0x0100).ToString("X4") + "]\r\n";
            output += "\r\n";

            output += "<Unknown> (bytes 17 18)\r\n";
            output += "\r\n";

            output += "Music (byte 19)\r\n";
            output += "Index " + mapDescriptors[0x19].ToString("X2") + "\r\n";
            output += "\r\n\r\n";
            output += "------------------------------------------";
            output += "\r\n\r\n";
            output += "Number of Events: " + Events.Count.ToString() + "\r\n";
            output += "Number of NPCs: " + Npcs.Count.ToString() + "\r\n";
            output += "Number of Exits: " + Exits.Count.ToString() + "\r\n";
            output += "Number of Treasures: " + Treasures.Count.ToString() + "\r\n";

            return output;
        }



        #endregion



        #region Encounters



        /**
        * getEncounters
        * 
        * Return information about the possible enemy encounters in a given map.
        *
        * @param mapId: The id of the map to return the Encounter data from.
        * @param quadrant: One of the 16 quadrants of the world map.
        * 
        * @return the Encounter data properly formatted or a blank string.
        */
        public string GetEncounters(int mapId, int quadrant = 0)
        {
            string output = "";

            switch (mapId)
            {
                case 0:
                    output += GetWorldMapEncounters(0, quadrant);
                    break;
                case 1:
                    output += GetWorldMapEncounters(1, quadrant);
                    break;
                case 2:
                    output += GetWorldMapEncounters(2, quadrant);
                    break;
                case 3:
                case 4:
                    break;
                default:
                    // D0/8000-D0/83FF	Data	Monster zones in dungeons (512 * 2)
                    output += "D0/8" + (mapId * 2).ToString("X3") + " : Monster group " + _monsterZones[mapId].ToString("X2") + "\r\n\r\n";
                    output += MonsterGroups[_monsterZones[mapId]];
                    break;
            };

            return output;

            // D0/3000-D0/4FFF	Data	Monster Encounter (512*16)
            // 0-2	[...]
            // 3	Visible monsters (bitwise)
            // 4-B	Monsters x8
            // C-D	Palettes 
            // E-F	Music

            // D0/7800-D0/797F	Data	Event encounters (0x0180)

            // D0/7980-D0/79FF	Data	Monster-in-a-box encounters (0x0080)
            //   The Monster-in-a-box Formations table is composed as records of 4 bytes each. Every record is a pair of Monster Formations ids.
            //   The first id is the "common encounter" to this box and the second id the "rare encounter" one.
            //   Index	Encounters	Monsters
            //   00		0058 005A	(Sorcerer (B), Karnak, Karnak) (Gigas)
            //   01		005A 005A	(Gigas) (Gigas)
            //   02		0057 005A	(Sorcerer (B), Sorcerer) (Gigas)
            //   03		005B 005A	(Gigas, Sorcerer (B), Karnak (B)) (Gigas)
            //   04		0000 0000	(Goblin) (Goblin)
            //   05		00EC 00F0	(Red Dragon) (Yellow Drgn (B), Yellow Drgn)
            //   06		0125 0125	(Cursed One, Cursed One, Cursed One (B), Cursed One (B)) (Cursed One, Cursed One, Cursed One (B), Cursed One (B))
            //   07		0110 0111	(Archaesaur) (Archaesaur, Nile (B), Nile (B))
            //   08		0110 0111	(Archaesaur) (Archaesaur, Nile (B), Nile (B))
            //   09		0157 00E8	(Fall Guard (B), Fall Guard, Fall Guard) (BandelKuar, BandelKuar, DarkWizard (B))
            //   0A		013B 013B	(Statue, Statue, Statue (B), Statue, Statue) (Statue, Statue, Statue (B), Statue, Statue)
            //   0B		01AC 01AB	(Invisible, Invisible (HB), Invisible (HB)) (Pantera, Pantera (H), Pantera (HB))
            //   0C		01DF 0071	(MachinHead) (Prototype)
            //   0D		01FF 01FF	(Magic Pot) (Magic Pot)
            //   0E		01B5 01B5	(Shinryuu) (Shinryuu)
            //   0F		0000 0000	(Goblin) (Goblin)

            // D0/7A00-D0/7BFF	Data	Monster zones in World 1 (4*2 * 8*8)
            // D0/7C00-D0/7DFF	Data	Monster zones in World 2 (4*2 * 8*8)
            // D0/7E00-D0/7FFF	Data	Monster zones in World 3 (4*2 * 8*8)
            //   64 World Map zones (8x8). 4 monster groups per zone.

            // D0/8000-D0/83FF	Data	Monster zones in dungeons (512*2)
        }



        /**
        * getWorldMapEncounters
        * 
        * Auxiliar function which returns information about the possible enemy encounters
        * in a given world map in one of its 16 quadrants.
        *
        * @param mapId: The id of the map to return the Encounter data from.
        * @param quadrant: One of the 16 quadrants of the world map.
        * 
        * @return the Encounter data properly formatted or a blank string.
        */
        private string GetWorldMapEncounters(int worldMapId, int quadrant)
        {
            string output = "";

            int[] quadrantMap = new int[]{
                0x00, 0x02, 0x04, 0x06,
                0x10, 0x12, 0x14, 0x16,
                0x20, 0x22, 0x24, 0x26,
                0x30, 0x30, 0x34, 0x36 };

            int quadrant00 = quadrantMap[quadrant];
            int quadrant01 = quadrant00 + 0x01;
            int quadrant02 = quadrant00 + 0x08;
            int quadrant03 = quadrant02 + 0x01;

            // D0/7A00-D0/7BFF	Data	Monster zones in World 1 (4*2 * 8*8)
            // D0/7C00-D0/7DFF	Data	Monster zones in World 2 (4*2 * 8*8)
            // D0/7E00-D0/7FFF	Data	Monster zones in World 3 (4*2 * 8*8)
            //   64 World Map zones (8x8). 4 monster groups per zone.

            output += "ZONE 0x" + quadrant00.ToString("X2");
            output += " ( D0/7" + (0x0A00 + worldMapId * 0x200 + quadrant00 * 0x08).ToString("X3") + " )" + "\r\n"; 
            output += "Grass\r\n" + MonsterGroups[_monsterWolrdMapZones[worldMapId][quadrant00][0]] + "\r\n";
            output += "Forest\r\n" +         MonsterGroups[_monsterWolrdMapZones[worldMapId][quadrant00][1]] + "\r\n";
            output += "Desert / Swamp\r\n" + MonsterGroups[_monsterWolrdMapZones[worldMapId][quadrant00][2]] + "\r\n";
            output += "Sea\r\n" +            MonsterGroups[_monsterWolrdMapZones[worldMapId][quadrant00][3]] + "\r\n\r\n";

            output += "ZONE 0x" + quadrant01.ToString("X2");
            output += " ( D0/7" + (0x0A00 + worldMapId * 0x200 + quadrant01 * 0x08).ToString("X3") + " )" + "\r\n"; 
            output += "Grass\r\n" +          MonsterGroups[_monsterWolrdMapZones[worldMapId][quadrant01][0]] + "\r\n";
            output += "Forest\r\n" +         MonsterGroups[_monsterWolrdMapZones[worldMapId][quadrant01][1]] + "\r\n";
            output += "Desert / Swamp\r\n" + MonsterGroups[_monsterWolrdMapZones[worldMapId][quadrant01][2]] + "\r\n";
            output += "Sea\r\n" +            MonsterGroups[_monsterWolrdMapZones[worldMapId][quadrant01][3]] + "\r\n\r\n";

            output += "ZONE 0x" + quadrant02.ToString("X2");
            output += " ( D0/7" + (0x0A00 + worldMapId * 0x200 + quadrant02 * 0x08).ToString("X3") + " )" + "\r\n"; 
            output += "Grass\r\n" +          MonsterGroups[_monsterWolrdMapZones[worldMapId][quadrant02][0]] + "\r\n";
            output += "Forest\r\n" +         MonsterGroups[_monsterWolrdMapZones[worldMapId][quadrant02][1]] + "\r\n";
            output += "Desert / Swamp\r\n" + MonsterGroups[_monsterWolrdMapZones[worldMapId][quadrant02][2]] + "\r\n";
            output += "Sea\r\n" +            MonsterGroups[_monsterWolrdMapZones[worldMapId][quadrant02][3]] + "\r\n\r\n";

            output += "ZONE 0x" + quadrant03.ToString("X2");
            output += " ( D0/7" + (0x0A00 + worldMapId * 0x200 + quadrant03 * 0x08).ToString("X3") + " )" + "\r\n"; 
            output += "Grass\r\n" +          MonsterGroups[_monsterWolrdMapZones[worldMapId][quadrant03][0]] + "\r\n";
            output += "Forest\r\n" +         MonsterGroups[_monsterWolrdMapZones[worldMapId][quadrant03][1]] + "\r\n";
            output += "Desert / Swamp\r\n" + MonsterGroups[_monsterWolrdMapZones[worldMapId][quadrant03][2]] + "\r\n";
            output += "Sea\r\n" +            MonsterGroups[_monsterWolrdMapZones[worldMapId][quadrant03][3]] + "\r\n\r\n";

            return output;
        }



        #endregion



    }



    #region AuxiliarClasses


    
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



    public class Event
    {
        public long address;
        public int actionId;
        public byte x;
        public byte y;

        public List<long> addresses = new List<long>();
        public List<List<byte>> bytes = new List<List<byte>>();

        //string eventstring;



        /**
        * Event
        * 
        * Class constructor.
        *
        * @param address: The address in the ROM where the Event is stored.
        * @param data (4 bytes)
        *   x: The initial x tile location of the Event.
        *   y: The initial y tile location of the Event.
        *   actionId: The action id of the Event.
        * @param br: The reader to load the data from.
        * @param headerOffset: The offset due to a header in the ROM.
        */
        public Event(long address, byte[] data, BinaryReader br, int headerOffset)
        {
            this.address = address;

            //4 bytes
            //  2B: Origin Coordinates
            //  2B: Event id
            x = data[0];
            y = data[1];
            actionId = data[2] + data[3] * 0x0100;
            //eventstring = "";

            // CE/2400-CE/27FF Event places offset [2 bytes * 512]
            // CE/2800-CE/365F Event places [4 bytes * 920]  (2B: Coordinates, 2B Event id (in table D8/E080))
            // D8/E080-D8/E5DF Event data offsets [2 bytes * 687 (0x2AF)]
            // D8/E5E0-D8/FFFF Event data [x bytes * 687 (0x2AF)] (Links to Extended Event data in table C8/3320)
            // C8/3320-C8/49DE Extended Event data offsets  [3 bytes * 1940] (Big endian)
            // C8/49DF-C9/FFFF Extended Event data [x bytes * 1940]

            long position = br.BaseStream.Position;

            //D8/E080
            br.BaseStream.Position = 0x18E080 + headerOffset + (actionId * 2);
            long pointerBegin = ((br.ReadByte() * 0x0001 + br.ReadByte() * 0x0100) + headerOffset);
            long pointerEnd   = ((br.ReadByte() * 0x0001 + br.ReadByte() * 0x0100) + headerOffset);
            long length = pointerEnd - pointerBegin;

            int[] argumentWidthts = { 0, 1, 3, 3, 3, 1, 1, 1, 1, 2 };

            br.BaseStream.Position = 0x18E080 + headerOffset + pointerBegin;
            for (int i = 0; i < length ; i++)
            {
                List<byte> newbyteList = new List<byte>();

                byte nextInstruction = br.ReadByte();
                newbyteList.Add(nextInstruction);
                int width = (nextInstruction > 0xF5) ? argumentWidthts[nextInstruction - 0xF6] : 2;

                //eventstring += "D8/" + ((br.BaseStream.Position - headerOffset) & 0x00FFFF).ToString("X4") + ":\t";
                //eventstring += nextInstruction.ToString("X2") + " (";
                addresses.Add(br.BaseStream.Position - headerOffset + 0xC00000);
                for (int j = 0; j < width; j++) { newbyteList.Add(br.ReadByte()); }
                bytes.Add(newbyteList);

                if (nextInstruction == 0xFF)
                {
                    addExtendedEvent(newbyteList[1] + newbyteList[2] * 0x0100, br, headerOffset);
                }

                //if (nextInstruction == 0xFF)
                //{
                //    byte lobyte = br.ReadByte();
                //    byte hibyte = br.ReadByte();
                //    eventstring += lobyte.ToString("X2") + " ";
                //    eventstring += hibyte.ToString("X2") + ") (extended Event)\r\n";
                //    eventstring += extendedEvent(lobyte + hibyte * 0x0100, br, headerOffset);
                //    eventstring += "\r\n";
                //}
                //else
                //{
                //   for (int j = 0; j < width; j++) { eventstring += br.ReadByte().ToString("X2") + " "; }
                //   eventstring += ")\r\n";
                //}

                i = i + width;
            }

            br.BaseStream.Position = position;
        }



        /**
        * extendedEvent
        * 
        * Read an extended event given its eventId.
        *
        * @param eventId: The id of the extended event to read from.
        * @param br: The reader to load the data from.
        * @param headerOffset: The offset due to a header in the ROM.
        * 
        * @return an extended Event data (not) properly formatted
        */
        private string extendedEvent(int eventId, BinaryReader br, int headerOffset){
            string output = "";
            
            long position = br.BaseStream.Position;

            // C8/3320-C8/49DE Event data offsets  [3 bytes * 1940] (Big endian)
            br.BaseStream.Position = 0x083320 + headerOffset + (eventId * 3);
            long pointerBegin = ((br.ReadByte() * 0x000001 + br.ReadByte() * 0x000100 + br.ReadByte() * 0x010000) + headerOffset) - 0xC00000;
            long pointerEnd = ((br.ReadByte() * 0x000001 + br.ReadByte() * 0x000100 + br.ReadByte() * 0x010000) + headerOffset) - 0xC00000;
            long length = pointerEnd - pointerBegin;
            if (pointerBegin < 0) return "";

            // C8/49DF-C9/FFFF Event data
            br.BaseStream.Position = pointerBegin + headerOffset;
            
            long snesAddress;
            byte bank;
            byte func;
            int offset;
            byte readenbyte = 0;

            do
            {
                snesAddress = ((br.BaseStream.Position - headerOffset) + 0xC00000);
                bank = Convert.ToByte((snesAddress & 0x00FF0000) >> 16);
                offset = Convert.ToInt32((snesAddress & 0x0000FFFF) >> 0); // TODO TYLER huh?

                output += "  " + bank.ToString("X2") + "/" + offset.ToString("X4") + ":\t  ";
                func = br.ReadByte();
                output += func.ToString("X2") + " (";
                for (int i = 0; i < getExtendedEventSize(func); i++)
                {
                    readenbyte = br.ReadByte();
                    output += readenbyte.ToString("X2") + " ";
                }
                if (func == 0xF3)
                {
                    int variableFuncSize = (readenbyte & 0x0F) + 1;
                    variableFuncSize *= ((readenbyte & 0xF0) >> 4) + 1;

                    output += "[";
                    for (int i = 0; i < variableFuncSize; i++)
                    {
                        readenbyte = br.ReadByte();
                        output += readenbyte.ToString("X2") + " ";
                    }
                    output += "]";
                }

                output += ")\r\n";

            } while (func != 0xFF);

            br.BaseStream.Position = position;

            return output;
        }



        /**
        * addExtendedEvent
        * 
        * Read an extended event given its eventId.
        *
        * @param eventId: The id of the extended event to read from.
        * @param br: The reader to load the data from.
        * @param headerOffset: The offset due to a header in the ROM.
        * 
        * @return an extended Event data (not) properly formatted
        */
        private void addExtendedEvent(int eventId, BinaryReader br, int headerOffset)
        {
            long position = br.BaseStream.Position;

            // C8/3320-C8/49DE Event data offsets  [3 bytes * 1940] (Big endian)
            br.BaseStream.Position = 0x083320 + headerOffset + (eventId * 3);
            long pointerBegin = ((br.ReadByte() * 0x000001 + br.ReadByte() * 0x000100 + br.ReadByte() * 0x010000) + headerOffset) - 0xC00000;
            long pointerEnd = ((br.ReadByte() * 0x000001 + br.ReadByte() * 0x000100 + br.ReadByte() * 0x010000) + headerOffset) - 0xC00000;
            long length = pointerEnd - pointerBegin;
            if (pointerBegin < 0) return;

            // C8/49DF-C9/FFFF Event data
            br.BaseStream.Position = pointerBegin + headerOffset;

            long snesAddress;
            byte bank;
            byte func;
            int offset;
            byte readenbyte = 0;

            do
            {
                snesAddress = ((br.BaseStream.Position - headerOffset) + 0xC00000);
                bank = Convert.ToByte((snesAddress & 0x00FF0000) >> 16);
                offset = Convert.ToInt32((snesAddress & 0x0000FFFF) >> 0);
                
                //output += "  " + bank.ToString("X2") + "/" + offset.ToString("X4") + ":\t  ";
                addresses.Add(snesAddress);
                List<byte> newbyteList = new List<byte>();

                func = br.ReadByte();
                newbyteList.Add(func);
                for (int i = 0; i < getExtendedEventSize(func); i++)
                {
                    readenbyte = br.ReadByte();
                    newbyteList.Add(readenbyte);
                }
                if (func == 0xF3)
                {
                    int variableFuncSize = (readenbyte & 0x0F) + 1;
                    variableFuncSize *= ((readenbyte & 0xF0) >> 4) + 1;

                    for (int i = 0; i < variableFuncSize; i++)
                    {
                        newbyteList.Add(br.ReadByte());
                    }
                }

                bytes.Add(newbyteList);

            } while (func != 0xFF);

            br.BaseStream.Position = position;
        }



        /**
        * getExtendedEventSize
        * 
        * Auxiliar function. Return the size of a given Event function.
        *
        * @param func: The function to get the size.
        * 
        * @return the size of the Event function
        */
        private int getExtendedEventSize(byte func)
        {
            int output = 0;
            int[] sizes = { 1,1,1,1,1,1,1,1,1,1,2,2,2,1,1,1,
                            1,1,1,1,1,0,1,1,2,2,2,2,0,2,2,2,
                            2,3,4,3,3,3,3,3,0,0,0,0,0,0,0,0,
                            0,5,0,5,0,0,0,0,0,0,0,0,0,0,0,0,
                            0,0,0,3,0,0,0,0,0,0,0,0,0,0,0,0};

            if (func < 0x80)
            {
                output = 0;
            }
            else if (func < 0xB0)
            {
                output = 1;
            }
            else
            {
                output = sizes[func - 0xB0];
            }

            return output;
        }



        /**
        * ToString
        * 
        * @return this Event data properly formatted
        */
        public string ToString()
        {
            string output = "";

            output += "Event Id  : " + actionId.ToString("X4") + "\r\n";
            output += "Coordinates: " + x.ToString("X2") + ", " + y.ToString("X2") + "\r\n";
            //output += "Data length: " + length.ToString("X4") + "\r\n";
            output += "\r\n";
            //output += eventstring;

            return output;
        }



        /**
        * injectEvent
        * 
        * Inject a edited Event into the ROM.
        *
        * @param bw: The writer to store the data into.
        * @param headerOffset: The offset due to a header in the ROM.
        * @param address: The exact SNES address of the event to inject in.
        * @param x: The new 'x' parameter of the Event.
        * @param y: The new 'y' parameter of the Event.
        * @param actionId: The new 'actionId' parameter of the Event.
        */
        public static void injectEvent(BinaryWriter bw, int headerOffset, long address, byte x, byte y, int actionId)
        {
            //4 bytes
            //  2B: Origin Coordinates
            //  2B: Event id
            bw.BaseStream.Position = address + headerOffset - 0xC00000;
            bw.Write(x);
            bw.Write(y);
            bw.Write((byte)((actionId & 0x00FF) >> 0));
            bw.Write((byte)((actionId & 0xFF00) >> 8));
        }
    }



    public class NPC
    {
        // NPC offsets at CE/59C0-CE/5DC1 [2bytes * 512 + 2bytes eof]
        // NPC properties at CE/5DC2-CE/9BFF [7bytes each]
        // NPC actions offsets at CE/0000-CE/073F
        // NPC actions at CE/0740-CE/2293 (I don't understand them yet) HACKME

        public long address;
        public int  actionId;
        public byte graphicId;
        public byte x;
        public byte y;
        public byte walkingParam;
        public byte palette;
        public byte direction;
        public byte unknown;
        string dialogue;

        

        /**
        * NPC
        * 
        * Class constructor.
        *
        * @param address: The address in the ROM where the NPC is stored.
        * @param data
        *   actionId: The action id of the NPC.
        *   graphicId: The graphic id of the NPC.
        *   x: The initial x tile location of the NPC.
        *   y: The initial y tile location of the NPC.
        *   palette: The palette id of the NPC.
        * @param br: The reader to load the data from.
        * @param headerOffset: The offset due to a header in the ROM.
        * @param speechTxt: The list of all the 2160 script texts of the game.
        */
        public NPC(long address, byte[] data, BinaryReader br, int headerOffset, List<string> speechTxt)
        {
            //bytes 0-1 ActionID
            //byte    2 Graphic ID
            //byte    3 X
            //byte    4 Y
            //byte    5 Walking parameter
            //byte    6 Palette (mask with 0x07)
            //byte    6 Direction (mask with 0xE0)

            this.address      = address;
            this.actionId     = (data[0] + data[1] * 0x0100) & 0x3FFF;
            this.graphicId    = data[2];
            this.x            = data[3];
            this.y            = data[4];
            this.walkingParam = data[5];
            this.palette      = (byte)((data[6] & 0x03) >> 0x00);
            this.direction    = (byte)((data[6] & 0xE0) >> 0x05);
            this.unknown      = (byte)((data[6] & 0x18) >> 0x03);

            string actions;
            List<int> speechId = getSpeechId(br, headerOffset, actionId, out actions);
            dialogue += "Actions:\r\n------------------------------------------\r\n";
            dialogue += actions + "\r\n\r\n";

            if (speechId.Count > 0)
            {
                foreach(int item in speechId){
                    dialogue += "Dialogue id: ";
                    dialogue += item.ToString("X4") + "\r\n------------------------------------------\r\n";
                    if ((item & 0x07FF) <= speechTxt.Count)
                        dialogue += speechTxt[item & 0x07FF];
                    else
                        dialogue += "<Not a dialogue>";
                    dialogue += "\r\n\r\n\r\n";
                }
            }
            else
            {
                dialogue += "<Action>\r\n";
            }
        }



        /**
        * getSpeechId
        * 
        * Aux function. Return the speech id given a NPC action.
        *
        * @param br: The reader to load the data from.
        * @param headerOffset: The offset due to a header in the ROM.
        * @param actionId: The action id of the NPC.
        *
        * @return the id of the speech.
        */
        private List<int> getSpeechId(BinaryReader br, int headerOffset, int actionId, out string completeAction)
        {
            // This works as the C0/2F97 routine in the SNES ROM
            // (I don't fully understand it yet) // HACKME

            List<int> output = new List<int>();
            completeAction = "";
            //$C0/2F97

            //(A 8 bit)
            //$23 = CE0000[$147F * 2]
            //while (true){
            //    A = CE0000[$23]
            //    if(A != #$F0) break;
            //    $23 += C0316B[A - #$F0]
            //}
            //x++
            //speechId = CE0000[x]

            // C0/316B
            // 00 00 00 00  00 04 01 02
            // 04 05 04 02  02 02 02 03 
            br.BaseStream.Position = 0x00316B + headerOffset;
            byte[] actionSizes = br.ReadBytes(0x10);

            br.BaseStream.Position = 0x0E0000 + headerOffset + actionId * 2;

            int accumulator;
            int var23 = br.ReadByte() + br.ReadByte() * 0x0100;
            int var26 = br.ReadByte() + br.ReadByte() * 0x0100;
            br.BaseStream.Position = 0x0E0000 + headerOffset + var23;

            while (true)
            {
                accumulator = br.ReadByte();
                completeAction += "CE/" + (var23 - headerOffset).ToString("X4") + ":\t";
                completeAction += accumulator.ToString("X2") + " (";
                if (accumulator == 0xF0) break;
                var23 += actionSizes[accumulator - 0xF0];
                foreach (byte item in br.ReadBytes(actionSizes[accumulator - 0xF0] - 1))
                {
                    completeAction += item.ToString("X2") + " ";
                }
                completeAction += ")\r\n";
            }
            var23++;
            completeAction += ")\r\n\r\n";

            while (var23 < var26)
            {
                output.Add(br.ReadByte() + br.ReadByte() * 0x0100);
                completeAction += "CE/" + (var23 - headerOffset).ToString("X4") + ":\t";
                completeAction += output.Last().ToString("X4") + "\r\n";
                var23 += 2;
            }

            return output;
        }



        /**
        * ToString
        * 
        * @return this NPC data properly formatted
        */
        public string ToString()
        {
            //bytes 0-1 ActionID
            //byte    2 Graphic ID
            //byte    3 X
            //byte    4 Y
            //byte    5 Walking parameter
            //byte    6 Palette (mask with 0x07)
            //byte    6 Direction (mask with 0xE0)
            string output = "";

            output += "Action Id  : " + actionId.ToString("X4") + "\r\n";
            output += "Graphic Id : " + graphicId.ToString("X4") + "\r\n";
            output += "Coordinates: " + x.ToString("X2") + ", " + y.ToString("X2") + "\r\n";
            output += "Walking    : " + walkingParam.ToString("X2") + "\r\n";
            output += "Palette    : " + palette.ToString("X1") + "\r\n";
            output += "Direction  : " + direction.ToString("X1") + "\r\n";
            output += "\r\n";

            //HACKME Research NPC Actions
            output += dialogue.Replace("[EOL]", "\r\n").Replace("[01]", "\r\n") + "\r\n";

            return output;
        }



        /**
        * injectNPC
        * 
        * Inject a edited NPC into the ROM.
        *
        * @param bw: The writer to store the data into.
        * @param headerOffset: The offset due to a header in the ROM.
        * @param address: The exact SNES address of the NPC to inject in.
        * @param actionId: The new 'actionId' parameter of the NPC.
        * @param graphicId: The new 'graphicId' parameter of the NPC.
        * @param x: The new 'x' parameter of the NPC.
        * @param y: The new 'y' parameter of the NPC.
        * @param walkingParam: The new 'walkingParam' parameter of the NPC.
        * @param properties: The new 'properties' parameter of the NPC.
        */
        public static void injectNPC(BinaryWriter bw, int headerOffset, long address,
            int actionId, byte graphicId, byte x, byte y, byte walkingParam, byte properties)
        {
            //7 bytes
            //  0-1 ActionID
            //  2 Graphic ID
            //  3 X
            //  4 Y
            //  5 Walking parameter
            //  6 Palette (mask with 0x07)
            //  6 Unknown (mask with 0x18)
            //  6 Direction (mask with 0xE0)
            bw.BaseStream.Position = address + headerOffset - 0xC00000;
            bw.Write((byte)((actionId & 0x00FF) >> 0));
            bw.Write((byte)((actionId & 0xFF00) >> 8));
            bw.Write(graphicId);
            bw.Write(x);
            bw.Write(y);
            bw.Write(walkingParam);
            bw.Write(properties);
        }
    }



    public class MapExit
    {
        public long address = 0;
        public byte originX = 0;
        public byte originY = 0;
        public int mapId = 0;
        public byte destinationX = 0;
        public byte destinationY = 0;

        /**
        * MapExit
        * 
        * Class constructor.
        * 
        * @param address: The address in the ROM where the Exit is stored.
        * @param data (4 bytes)
        *   originX: The x tile location of the exit.
        *   originY: The y tile location of the exit.
        *   mapId: The destination map id of the exit.
        *   destinationX: The x tile location of the exit destination.
        *   destinationY: The y tile location of the exit destination.
        */
        public MapExit(long address, byte[] data)
        {
            //6 bytes
            //  2B: Origin Coordinates
            //  2B: Destiny Map ID????
            //  2B: Destiny Coordinates???? (Máximo 3Fx3F en mapas locales)

            this.address = address;

            originX = data[0];
            originY = data[1];
            mapId = data[2] + data[3] * 0x0100;
            destinationX = data[4];
            destinationY = data[5];
        }



        /**
        * ToString
        * 
        * @return this NPC data properly formatted
        */
        public string ToString()
        {
            string output = "";

            output += "Origin coordinates: " + originX.ToString("X2") + ", " + originY.ToString("X2") + "\r\n";
            output += "Map Id: " + (0x01FF & mapId).ToString("X4") + "\r\n";
            output += "Map properties: " + ((0xFE00 & mapId) >> 9).ToString("X2") + "\r\n";
            output += "Destination coordinates: " + (destinationX & 0x3F).ToString("X2") + ", " + (destinationY & 0x3F).ToString("X2") + "\r\n";
            output += "Destination properties: " + ((destinationX & 0xC0) >> 6).ToString("X2") + ", " + ((destinationY & 0xC0) >> 6).ToString("X2") + "\r\n";

            return output;
        }

        public int GetMapId() => (int)(0x01FF & mapId);
        public byte GetDestX() => (byte)(destinationX & 0x3F);
        public byte GetDestY() => (byte)(destinationY & 0x3F);

        /**
        * injectExit
        * 
        * Inject a edited Map Exit into the ROM.
        *
        * @param bw: The writer to store the data into.
        * @param headerOffset: The offset due to a header in the ROM.
        * @param address: The exact SNES address of the Map Exit to inject in.
        * @param originX: The new 'originX' parameter of the Map Exit.
        * @param originY: The new 'originY' parameter of the Map Exit.
        * @param mapId: The new 'mapId' parameter of the Map Exit.
        * @param destinationX: The new 'destinationX' parameter of the Map Exit.
        * @param destinationY: The new 'destinationY' parameter of the Map Exit.
        */
        public static void injectExit(BinaryWriter bw, int headerOffset, long address, byte originX, byte originY, int mapId, byte destinationX, byte destinationY)
        {
            //6 bytes
            //  2B: Origin Coordinates
            //  2B: Destiny Map ID????
            //  2B: Destiny Coordinates???? (Máximo 3Fx3F en mapas locales)

            bw.BaseStream.Position = address + headerOffset - 0xC00000;
            bw.Write(originX);
            bw.Write(originY);
            bw.Write((byte)((mapId & 0x00FF) >> 0));
            bw.Write((byte)((mapId & 0xFF00) >> 8));
            bw.Write(destinationX);
            bw.Write(destinationY);
        }



    }



    public class Treasure
    {
        public long address = 0;
        public byte locationX = 0;
        public byte locationY = 0;
        public byte itemId = 0;
        public byte properties = 0;



        /**
        * Treasure
        * 
        * Class constructor.
        * 
        * @param address: The address in the ROM where the ChestBox is stored.
        * @param data
        *   locationX: The x tile location of the chest.
        *   locationY: The y tile location of the chest.
        *   itemId: The id of the price.
        *   properties: The properties of the chest.
        */
        public Treasure(long address, byte[] data)
        {
            //4 bytes
            //  2B: Origin Coordinates
            //  1B: Chest properties
            //  1B: Content (Spell/item/money) id

            this.address = address;
            locationX    = data[0];
            locationY    = data[1];
            properties   = data[2];
            itemId       = data[3];
        }



        /**
        * ToString
        * 
        * @return this ChestBox data properly formatted
        */
        public string ToString()
        {
            string output = "";

            output += "Origin coordinates: " + locationX.ToString("X2") + ", " + locationY.ToString("X2") + "\r\n";
            output += "Item id: " + itemId.ToString("X2") + "\r\n";
            output += "chest properties: " + properties.ToString("X2") + "\r\n";

            return output;
        }



        /**
        * ToString
        * 
        * @param itemNames: A list of the current game item names.
        * @param spellNames: A list of the current game spell names
        * 
        * @return this Treasure data properly formatted
        */
        public string ToString(List<string> itemNames, List<string> spellNames)
        {
            string output = "";
            string name = "";

            int price_bhvor = (properties & 0xE0) >> 5;
            int modificator = (properties & 0x1F);

            output += "Origin coordinates: " + locationX.ToString("X2") + ", " + locationY.ToString("X2") + "\r\n";

            switch (price_bhvor)
            {
                case 0:
                    output += "Chest properties: " + properties.ToString("X2") + " (Money)\r\n";
                    output += "Item id: " + itemId.ToString("X2") + " (" + itemId * Math.Pow(10, modificator) + "GP)\r\n";
                    break;
                case 1:
                    output += "Chest properties: " + properties.ToString("X2") + " (Spell)\r\n";
                    name = (itemId < spellNames.Count) ? spellNames[itemId] : "<Error>";
                    output += "Item id: " + itemId.ToString("X2") + " (" + name + ")\r\n";
                    break;
                case 2:
                    output += "Chest properties: " + properties.ToString("X2") + " (Item)\r\n";
                    name = (itemId < itemNames.Count) ? itemNames[itemId] : "<Error>";
                    output += "Item id: " + itemId.ToString("X2") + " (" + name + ")\r\n";
                    break;
                case 3:
                    output += "Chest properties: " + properties.ToString("X2") + " (????)\r\n";
                    output += "Item id: " + itemId.ToString("X2") + "\r\n";
                    break;
                case 4:
                    output += "Chest properties: " + properties.ToString("X2") + " (????)\r\n";
                    output += "Item id: " + itemId.ToString("X2") + "\r\n";
                    break;
                case 5:
                    output += "Chest properties: " + properties.ToString("X2") + " (Item + Enemies)\r\n";
                    output += "Monster-in-a-box id: " + modificator.ToString("X2") + "\r\n";
                    name = (itemId < itemNames.Count) ? itemNames[itemId] : "<Error>";
                    output += "Item id: " + itemId.ToString("X2") + " (" + name + ")\r\n";
                    break;
                case 6:
                    output += "Chest properties: " + properties.ToString("X2") + " (????)\r\n";
                    output += "Item id: " + itemId.ToString("X2") + "\r\n";
                    break;
                case 7:
                    output += "Chest properties: " + properties.ToString("X2") + " (Spell + Enemies)\r\n";
                    output += "Monster-in-a-box id: " + modificator.ToString("X2") + "\r\n";
                    name = (itemId < spellNames.Count) ? spellNames[itemId] : "<Error>";
                    output += "Item id: " + itemId.ToString("X2") + " (" + name + ")\r\n";
                    break;
                default:
                    output += "Chest properties: " + properties.ToString("X2") + " (ERROR!)\r\n";
                    output += "Item id: " + itemId.ToString("X2") + "\r\n";
                    break;
            }

            return output;
        }



        /**
        * getPrice
        * 
        * @param itemNames: A list of the current game item names.
        * @param spellNames: A list of the current game spell names
        * 
        * @return this Treasure price properly formatted
        */
        public string getPrice(List<string> itemNames, List<string> spellNames)
        {
            string output = "";
            int price_bhvor = (properties & 0xE0) >> 5;
            int modificator = (properties & 0x1F);

            switch (price_bhvor)
            {
                case 0:
                    output += itemId * Math.Pow(10, modificator) + " Gil";
                    break;
                case 1:
                    output += (itemId < spellNames.Count) ? spellNames[itemId] : "<Error>";
                    break;
                case 2:
                    output += (itemId < itemNames.Count) ? itemNames[itemId] : "<Error>";
                    break;
                case 3:
                    output += "(????)";
                    break;
                case 4:
                    output += "(????)";
                    break;
                case 5:
                    output += (itemId < itemNames.Count) ? itemNames[itemId] : "<Error>";
                    output += "\r\nMonster-in-a-box id: " + modificator.ToString("X2");
                    break;
                case 6:
                    output += "(????)";
                    break;
                case 7:
                    output += (itemId < spellNames.Count) ? spellNames[itemId] : "<Error>";
                    output += "\r\nMonster-in-a-box id: " + modificator.ToString("X2");
                    break;
                default:
                    output += "ERROR!";
                    break;
            }

            return output;
        }



        /**
        * injectTreasure
        * 
        * Inject a edited Treasure into the ROM.
        *
        * @param bw: The writer to store the data into.
        * @param headerOffset: The offset due to a header in the ROM.
        * @param address: The exact SNES address of the Treasure to inject in.
        * @param x: The new 'x' parameter of the Treasure.
        * @param y: The new 'y' parameter of the Treasure.
        * @param properties: The new 'properties' parameter of the Treasure.
        * @param itemId: The new 'itemId' parameter of the Treasure.
        */
        public static void injectTreasure(BinaryWriter bw, int headerOffset, long address, byte x, byte y, byte properties, byte itemId)
        {
            //4 bytes
            //  2B: Origin Coordinates
            //  1B: Chest properties
            //  1B: Content (Spell/item/money) id

            bw.BaseStream.Position = address + headerOffset - 0xC00000;
            bw.Write(x);
            bw.Write(y);
            bw.Write(properties);
            bw.Write(itemId);
        }



    }



    #endregion
}

