using System;
using System.Runtime;
using System.Text;
using Final_Fantasy_V.Models;
using Microsoft.Xna.Framework.Graphics;

namespace Engine.RomReader;

public static class RomData
{
    
    public static List<Weapon> Weapons { get; private set; } = new();
    public static List<Gear> Gear { get; } = new();

    public static List<Spell> Spells { get; } = new();

    public static List<Consumable> Consumables { get; } = new();
    private static List<string> itemNames { get; set; } = new();
    

    public static Weapon? GetWeaponByName(string name)
    {
        return Weapons.FirstOrDefault(w => w.Name.Split("[FF]")[0] == name);
    }

    public static Gear? GetGearByName(string name)
    {
        return Gear.FirstOrDefault(w => w.Name.Split("[FF]")[0] == name);
    }
    
    public static Consumable? GetConsumableByName(string name)
    {
        return Consumables.FirstOrDefault(w => w.Name.Split("[FF]")[0] == name);
    }

    public static void Instantiate(BinaryReader br, int offset)
    {
        var tblManager = new TBL_Manager();
        
        ReadItemData(br, tblManager, offset);
        ReadMagicData(br, tblManager, offset);
    }
    
    static void ReadItemData(BinaryReader br, TBL_Manager tblManager, int offset)
    {
        itemNames = RomGame.AppendToExportList(br, tblManager.TBL_Reader2bpp, 0x111380, 256 /* nRegisters */, 9 /* registersSize */, offset);
        br.BaseStream.Position = 0x110000 + offset;
        
        for (int i = 0; i < 128; i++)
        {
            var data = br.ReadBytes(12);
            Weapons.Add(new Weapon()
            {
                Attack = data[0x07],
                Critical = data[0x09],
                Name = itemNames[i]
            });
        }

        for (int i = 0; i < 96; i++)
        {
            var data = br.ReadBytes(12);
            Gear.Add(new Gear()
            {
                Name = itemNames[i+128],
                Defense = data[0x07],
                Evade = data[0x06],
                Weight = data[0x01],
                MagicDefense = data[0x09],
                MagicEvade = data[0x08],
                Type = (EItemType)data[0x00],
                
            });
        }

        for (int i = 0; i < 32; i++)
        {
            var data = br.ReadBytes(8);
            Consumables.Add(new Consumable()
            {
                Name = itemNames[i+224],
                Target = data[0x00],
                Properties = data[0x03],
                Restrictions = data[0x02],
                DamageFormula = data[0x04],
                Type = EItemType.Consume
            });
        }
        
    }
    
    static void ReadMagicData(BinaryReader br, TBL_Manager tblManager, int offset)
    {
        var spellNames = RomGame.AppendToExportList(br, tblManager.TBL_Reader2bpp, 0x111C80, 87 /* nRegisters */, 6 /* registersSize */, offset);
        br.BaseStream.Position = 0x110B80 + offset;
        for (int i = 0; i < 87; i++)
        {
            
            var data = br.ReadBytes(8);
            if (i == 36)
            {
                Console.WriteLine();
            }
            Spells.Add(new Spell()
            {
                Name = spellNames[i],
                Target = data[0],
                AttackType = data[1],
                Reflectable = (data[3] & 0x80) == 1,
                MPCost = (byte)(data[3] & 0b0111_1111),
                UnAvoidable = (data[4] & 0x80) == 1,
                AttackFormula = (byte)(data[4] & 0b0111_1111),
                Attack = data[6]
                
            });
        }
    }
}



public class RomGame
{

    public readonly List<string> SpeechTxt = new();
    List<string> itemNames = new List<string>();
    List<string> spellNames = new List<string>();
    List<string> locationNames = new List<string>();
    List<string> monsterNames = new List<string>();
    private BinaryReader br;
    int offset;

    public readonly MapManager Map;

    public RomGame()
    {

        br = new BinaryReader(File.Open("FF5.sfc", FileMode.Open));

        (_, offset) = CheckSNESHeader(br);

        var tblManager = new TBL_Manager();

        itemNames = AppendToExportList(br, tblManager.TBL_Reader2bpp, 0x111380, 256 /* nRegisters */, 9 /* registersSize */, offset);
        spellNames = AppendToExportList(br, tblManager.TBL_Reader2bpp, 0x111C80, 87 /* nRegisters */, 6 /* registersSize */, offset);
        locationNames = appendToExportListSU(br, tblManager.TBL_Reader1bpp, 0x107000, 0x270000, 164, offset);
        monsterNames = AppendToExportList(br, tblManager.TBL_Reader2bpp, 0x200050, 384 /* nRegisters */, 10 /* registersSize */, offset);
        SpeechTxt = loadSpeech(tblManager, br);

        Map = new MapManager(br, offset, SpeechTxt, itemNames, spellNames, locationNames, monsterNames);
    }

    public (BackgroundLayers, MapManager.Wall[,]) GetLayers(GraphicsDevice gd, int id)
    {

        var bl = new BackgroundLayers();
        var walls = new MapManager.Wall[1,1];

        Map.MapDecypher(gd, br, offset, id, out bl, out walls);
        return (bl, walls);

    }

    public Monster GetMonster(GraphicsDevice gd, int id)
    {
        return Map.LoadMonster(gd, br, offset, id);
    }

    public BattleGroup GetBattleGroup(GraphicsDevice gd, int id)
    {
        var b= Map.BattleGroups[id];
        b.LoadMonsterData(gd, this);
        return b;
    }

    public void Update(int id)
    {
        int quadrantsToGoTo = 1;
        if (id < 5)
        {
            quadrantsToGoTo = 16;
        }

        for (int i = 0; i < quadrantsToGoTo; i++)
        {
            Map.MapGetExits(br, offset, id, i, null);
            Map.MapGetEvents(br, offset, id, i, null);
        }
        
        Map.MapGetNpCs(br, offset, id, null);
        
        //Map.MapGetChests(br, offset, id, null);
    }

    public void FindMapOfActionId(int actionId)
    {
        Map.Events.Clear();
        for (int i = 5; i < 511; i++)
        {
            Map.MapGetEvents(br, offset, i, 0, null);                    
        }
    }

    public List<List<byte>> GetStart()
    {
        return Map.StartGameEvent(br, offset);
    }
  
    public static (bool, int) CheckSNESHeader(BinaryReader br)
    {
        int headerOffset;
        /* Too small size */
        if (br.BaseStream.Length < 0x200000)
            return (false, 0);

        if (br.BaseStream.Length % 1024 == 0)
        {
            headerOffset = 0x0000;
            br.BaseStream.Position = 0xFFC0;
            byte[] headerName = br.ReadBytes(21);
            return (System.Text.Encoding.UTF8.GetString(headerName) == "FINAL FANTASY 5      ", headerOffset);
        }
        else if (br.BaseStream.Length % 1024 == 512)
        {
            headerOffset = 0x0200;
            br.BaseStream.Position = (0xFFB0 + 0x0200);
            byte[] headerName = br.ReadBytes(21);
            return (Encoding.UTF8.GetString(headerName) == "FINAL FANTASY 5      ", headerOffset);
        }
            return (false, 0);
    }

    public static List<string> AppendToExportList(BinaryReader br, Dictionary<byte, string> inputTBL, int address, int nRegisters, int registersSize, int headerOffset)
    {
        List<string> output = new List<string>();

        try
        {
            br.BaseStream.Position = address + headerOffset;

            for (int i = 0; i < nRegisters; i++)
            {
                string newLine = "";

                for (int j = 0; j < registersSize; j++)
                {
                    newLine += inputTBL[br.ReadByte()];
                }

                output.Add(newLine);
            }

        }
        catch (Exception e)
        {
            // MessageBox.Show("Error reading the file: " + e.Tostring(), "Error");
        }

        return output;
    }

    List<string> appendToExportListSU(BinaryReader br, Dictionary<byte, string> inputTBL, int offsetsAdress, int address, int nRegisters, int headerOffset)
    {
        List<string> output = new List<string>();
        List<int> offsets = new List<int>();

        try
        {

            br.BaseStream.Position = offsetsAdress + headerOffset;
            for (int i = 0; i < nRegisters; i++)
            {
                offsets.Add(br.ReadByte() + 0x0100 * br.ReadByte());
            }

            offsets.Add(2304);

            for (int i = 0; i < nRegisters; i++)
            {
                br.BaseStream.Position = address + headerOffset + offsets[i];
                string newLine = "";

                byte newbyte = br.ReadByte();
                for (int j = 0; j < offsets[i + 1] - offsets[i]; j++)
                {
                    newLine += inputTBL[newbyte];
                    newbyte = br.ReadByte();
                }

                output.Add(newLine);
            }

        }
        catch (Exception e)
        {
            //MessageBox.Show("Error reading the file: " + e.Tostring(), "Error");
        }

        return output;
    }


    
    public List<String> loadSpeech(TBL_Manager tblManager, BinaryReader br)
        {
            List<String> speechTxt = new List<String>();

            Byte newChar = 0x00;

            /* Const (Bartz) */
            List<Byte> bartz = new List<Byte>();
            if (tblManager.TBL_Injector1bpp.TryGetValue("(", out newChar)) bartz.Add(newChar);
            if (tblManager.TBL_Injector1bpp.TryGetValue("B", out newChar)) bartz.Add(newChar);
            if (tblManager.TBL_Injector1bpp.TryGetValue("a", out newChar)) bartz.Add(newChar);
            if (tblManager.TBL_Injector1bpp.TryGetValue("r", out newChar)) bartz.Add(newChar);
            if (tblManager.TBL_Injector1bpp.TryGetValue("t", out newChar)) bartz.Add(newChar);
            if (tblManager.TBL_Injector1bpp.TryGetValue("z", out newChar)) bartz.Add(newChar);
            if (tblManager.TBL_Injector1bpp.TryGetValue(")", out newChar)) bartz.Add(newChar);

            /* Reset speech */
            List<int> speechPtr = new List<int>();
            List<List<Byte>> speech = new List<List<byte>>();


            /* Read Speech */
            br.BaseStream.Position = 0x2013F0;

            for (int i = 0; i < 2160; i++)
            {
                int newPtr = br.ReadByte() + br.ReadByte() * 0x0100 + (br.ReadByte() - 0xC0) * 0x010000;
                speechPtr.Add(newPtr);
            }

            foreach (int item in speechPtr)
            {
                List<Byte> newRegister = new List<byte>();
                br.BaseStream.Position = item;

                Byte newByte = br.ReadByte();
                while (newByte != 0)
                {
                    if (newByte != 0x02)
                    {
                        newRegister.Add(newByte);
                    }
                    else
                    {
                        /* (Bartz)*/
                        newRegister.AddRange(bartz);
                    }
                    newByte = br.ReadByte();
                }

                String newLine = "";
                foreach(Byte subitem in newRegister)
                {
                    newLine += tblManager.TBL_Reader1bpp[subitem];
                }

                speechTxt.Add(newLine);
                speech.Add(newRegister);

            }

            return speechTxt;
        }
}