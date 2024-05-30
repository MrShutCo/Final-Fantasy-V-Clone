namespace Engine.RomReader;

public class Event
{
    public long address;
    public int actionId;
    public byte x;
    public byte y;

    public List<long> addresses = new();
    public List<List<byte>> bytes = new();

    private static Dictionary<byte, int> _lengths = new()
    {
        { 0x71, 0 }
    };
    

    //string eventstring;

    public List<int> IndicesOfExtendedEvents;

    static int[] ArgumentWidths = { 0, 1, 3, 3, 3, 1, 1, 1, 1, 2 };
        
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
        IndicesOfExtendedEvents = [];

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
        var length = pointerEnd - pointerBegin;
            
        br.BaseStream.Position = 0x18E080 + headerOffset + pointerBegin;
            
        //br.BaseStream.Position = CalculatePosition(br, actionId, headerOffset, out var length);
        bytes = ReadEvent(br, headerOffset, length);

        br.BaseStream.Position = position;
    }

    private static long CalculatePosition(BinaryReader br, int actionId, int headerOffset, out long length)
    {
        long position = br.BaseStream.Position;

        //D8/E080
        br.BaseStream.Position = 0x18E080 + headerOffset + (actionId * 2);
        long pointerBegin = ((br.ReadByte() * 0x0001 + br.ReadByte() * 0x0100) + headerOffset);
        long pointerEnd   = ((br.ReadByte() * 0x0001 + br.ReadByte() * 0x0100) + headerOffset);
        length = pointerEnd - pointerBegin;
            
        br.BaseStream.Position = 0x18E080 + headerOffset + pointerBegin;
        return position;
    }

    private static int EventWidth(byte eventType)
    {
        switch (eventType)
        {
            case >= 0x80 and <= 0xA0:
                return 1;
            case <= 0x70:
                return 0;
            case 0xF3 or 0xF4 or 0xD3 or 0xD4 or 0xD5 or 0xD6:
                return 3;
            case 0xBD or 0xE2 or 0xE8 or 0xBA or 0xBB or 0xBC or 0xC8 or 0xF0 or 0xCD or 0xCA or 0xCB or 0xD8 or 0xD9 or 
                 0xCE or 0xCF or 0xD0 or 0xC9 or 0xD1 or 0xD7 or 0xFF:
                return 2;
            case 0xA6 or 0xA7 or 0xF1 or 0xB7 or 0xB8 or 0xA9 or 0xB6 or 0xC3 or 0xC4 or 0xC5 or 0xAD or 0xAF or 0xB0 or
                 0xAA or 0xAB or 0xC6 or 0xAC or 0xA0 or 0xB1 or 0xC1 or 0xDD or (>= 0xB8 and <= 0xBA) or 0xC5 or 0xC0 or 
                 0xAE or 0xBE or 0xA1 or 0xB4 or 0xB5 or 0xBF or (>= 0xA2 and <= 0xA6) or 0xDC or 0xB2 or 0xB3 or 0xFB or 
                 0xFC or 0xFD or 0xFE or 0xC7:
                return 1;
            case 0xE7 or 0x7B or 0x7D or 0x7F or 0xDA or 0xEB or 0x7E or 0xEA or 0x79 or 0x7A or 0xDB or 
                 0xE6 or 0x76 or 0x77 or 0x78 or 0x79 or 0xE4 or 0xE5 or 0xE6 or 0xC2 or (>= 0x70 and <= 0x76) :
                return 0;
            case 0xE0 or 0xE1 or 0xE3 or 0x7C:
                return 5;
            case 0xD2:
                return 4;
        }

        return 0;
    } 

    public static List<List<byte>> ReadEvent(BinaryReader br, int headerOffset, long length)
    {
        List<List<byte>> eventData = new();
        for (int i = 0; i < length ; i++)
        {
            List<byte> newbyteList = new List<byte>();

            byte nextInstruction = br.ReadByte();
            newbyteList.Add(nextInstruction);
            //int width = (nextInstruction > 0xF5) ? ArgumentWidths[nextInstruction - 0xF6] : 2;
            int width = EventWidth(nextInstruction);
            //eventstring += "D8/" + ((br.BaseStream.Position - headerOffset) & 0x00FFFF).ToString("X4") + ":\t";
            //eventstring += nextInstruction.ToString("X2") + " (";
            //addresses.Add(br.BaseStream.Position - headerOffset + 0xC00000);
            for (int j = 0; j < width; j++) { newbyteList.Add(br.ReadByte()); }
            eventData.Add(newbyteList);

            if (nextInstruction is 0xFF or 0xCD)
            {
                eventData.AddRange(addExtendedEvent(newbyteList[1] + newbyteList[2] * 0x0100, br, headerOffset, nextInstruction == 0xCD));
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

        return eventData;
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
    /*private string extendedEvent(int eventId, BinaryReader br, int headerOffset){
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
            for (int i = 0; i < EventWidth(func); i++)
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
    }*/



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
    private static List<List<byte>> addExtendedEvent(int eventId, BinaryReader br, int headerOffset, bool isSubroutine)
    {
        List<List<byte>> gameEvents = new();
        isSubroutine = false;
        long position = br.BaseStream.Position;

        // C8/3320-C8/49DE Event data offsets  [3 bytes * 1940] (Big endian)
        br.BaseStream.Position = 0x083320 + headerOffset + (eventId * 3);
        long pointerBegin = ((br.ReadByte() * 0x000001 + br.ReadByte() * 0x000100 + br.ReadByte() * 0x010000) + headerOffset) - 0xC00000;
        long pointerEnd = ((br.ReadByte() * 0x000001 + br.ReadByte() * 0x000100 + br.ReadByte() * 0x010000) + headerOffset) - 0xC00000;
        long length = pointerEnd - pointerBegin;
        if (pointerBegin < 0) return gameEvents;

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
            //addresses.Add(snesAddress);
            List<byte> newbyteList = new List<byte>();

            func = br.ReadByte();
            newbyteList.Add(func);
            var width = EventWidth(func);
            if (isSubroutine && func == 0xFF) width = 0; // Subroutines cant call other events, so 0xFF means end of subroutine
            for (int i = 0; i < width; i++)
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

            if (func == 0xCD)
            {
                gameEvents.AddRange(addExtendedEvent(newbyteList[1] + newbyteList[2] * 0x0100, br, headerOffset, false));
            }

            gameEvents.Add(newbyteList);

        } while (func != 0xFF);

        br.BaseStream.Position = position;
        return gameEvents;
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
    private static int getExtendedEventSize(byte func)
    {
        int output = 0;
        int[] sizes = { 1,1,1,1,1,1,1,1,1,1,2,2,2,1,1,1,
            1,1,1,1,1,0,1,1,2,2,2,2,0,2,2,2,
            2,3,4,3,3,3,3,3,0,0,0,0,0,0,0,0,
            5,5,0,5,0,0,0,0,0,0,0,0,0,0,0,0, // [48] here needs to be a 5 and not a 1 to read map change E0 events properly
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