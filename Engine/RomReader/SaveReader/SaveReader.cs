using Final_Fantasy_V.Models;

namespace Engine.RomReader.SaveReader;

public class SaveReader
{
    private BinaryReader br;

    private byte[] _eventFlags;
    private byte[] _npcFlags;
    private byte[] _items;
    private byte[] _itemCounts;

    private List<byte[]> _characters;
    
    public SaveReader()
    {
        br = new BinaryReader(File.Open("FF5.srm", FileMode.Open));
    }

    public void LoadSaveSlot(int slot)
    {
        if (slot < 0 || slot > 3)
        {
            throw new Exception("Slot invalid, must be between 0 and 3");
        }

        br.BaseStream.Position = 0x700 * slot;

        // Read characters
        _characters = [br.ReadBytes(0x50), br.ReadBytes(0x50), br.ReadBytes(0x50), br.ReadBytes(0x50)];
        
        // Read Inventory
        _items = br.ReadBytes(0x100);
        _itemCounts = br.ReadBytes(0x100);
        
        // Map Position
        
        // EVENT FLAGS POGGGGG
        br.BaseStream.Position = 0x514;
        _eventFlags = br.ReadBytes(64);
        
        // NPC flags
        br.BaseStream.Position = 0x554;
        _npcFlags = br.ReadBytes(116);
        
    }

    /*public List<Character> LoadCharacters()
    {
        foreach (var character in _characters)
        {
            Character c = new Character()
            {
                
            }
        }
    }*/

    public List<Item> LoadInventory()
    {
        List<Item> items = new();
        int i = 0;
        while (_items[i] != 0)
        {
            Item newItem;
            if (_items[i] < 128) newItem = RomData.Weapons[_items[i]];
            else if (_items[i] < 128 + 96) newItem = RomData.Gear[_items[i]-128];
            else newItem = RomData.Consumables[_items[i]-128-96];

            newItem.NumInInventory = _itemCounts[i];
            
            items.Add(newItem);
            i++;
        }

        return items;
    }

    public bool[] ParseEventFlags()
    {
        bool[] flags = new bool[512];
        for (int i = 0; i < _eventFlags.Length; i++)
        {
            for (int j = 7; j >= 0; j--)
            {
                flags[i*8+(7-j)] = ((_eventFlags[i] >> j) & 1) == 1;
            }
        }

        return flags;
    }
}