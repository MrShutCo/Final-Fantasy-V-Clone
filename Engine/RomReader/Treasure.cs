namespace Engine.RomReader;

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