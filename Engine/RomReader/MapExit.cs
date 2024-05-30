namespace Engine.RomReader;

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