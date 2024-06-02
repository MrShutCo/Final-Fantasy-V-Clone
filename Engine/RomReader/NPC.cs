using Microsoft.Xna.Framework.Graphics;

namespace Engine.RomReader;

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
    public List<(string, List<ushort>)> dialogues;
    public Texture2D Texture;

    private static readonly HashSet<int> InvisibleActionId = [
        128, // Local control 
        0, 
        119,
        95, //Chocobo at Tycoon Meteor
        154 // Local control
    ];
    private static readonly HashSet<int> VisibleActionIds = [];
    /*[
            8, 9, 25, 26, 32, 40, 41, 42, 50, 51, 74, 86, 87, 88, 90, 95, 96, 97, 98, 99,
            100, 101, 102, 103, 104, 107, 112, 113, 114, 115, 116, 117, 118, 119, 120, 123,
            124, 125, 126, 128, 130, 131, 132, 133, 134, 135, 136, 137, 138, 139, 140, 141,
            142, 143, 144, 145, 146, 147, 148, 149, 150, 152, 154, 157, 159, 160, 161, 163,
            165, 166, 168, 169, 170, 172, 174, 175, 176, 177, 178, 181, 192, 193, 194, 195,
            196, 197, 198, 199, 200, 201, 208, 209, 210, 211, 212, 213, 223, 224, 225, 226,
            227, 228, 229, 230, 231, 232, 233, 234, 242, 243, 244, 248, 249, 250, 251, 254,
            255, 256, 257, 258, 259, 260, 261, 262, 263, 264, 265, 266, 270, 272, 273, 274,
            275, 279, 280, 281, 282, 288, 289, 292, 293, 294, 295, 296, 297, 298, 299, 300,
            301, 302, 303, 306, 307, 308, 309, 310, 311, 312, 313, 314, 318, 337, 339, 342,
            343, 344, 345, 346, 347, 348, 349, 350, 351, 352, 353, 354, 355, 356, 357, 358,
            359, 360, 361, 362, 363, 364, 365, 366, 367, 368, 369, 384, 385, 448, 562, 563,
            564, 565, 566, 567, 568, 569, 570, 571, 572, 573, 579, 639, 640, 641, 642, 643,
            644, 645, 646, 647, 648, 649, 650, 651, 652, 666, 667, 669, 672, 674, 675, 676,
            677, 678, 679, 680, 681, 682, 686, 687, 688, 689, 690, 691, 692, 693, 694, 695,
            696, 704, 714, 752, 753, 754, 757, 758, 759, 760, 761, 768, 769, 770, 771, 774,
            775, 864, 865, 866, 867, 868, 869, 870, 871, 872, 873, 874, 875, 876, 880, 889,
            917, 918, 919
        ]*/
            
        
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

        dialogues = new List<(string, List<ushort>)>();
        this.address      = address;
        actionId     = (data[0] + data[1] * 0x0100) & 0x3FFF;
        graphicId    = data[2];
        x            = data[3];
        y            = data[4];
        walkingParam = data[5];
        palette      = (byte)((data[6] & 0x03) >> 0x00);
        direction    = (byte)((data[6] & 0xE0) >> 0x05);
        unknown      = (byte)(data[6] & 0x1C);

        List<byte> actions;
        List<int> speechId = getSpeechId(br, headerOffset, actionId, out actions);
        //dialogue += "Actions:\r\n------------------------------------------\r\n";
        //dialogue += actions + "\r\n\r\n";

        if (speechId.Count > 0)
        {
            foreach(int item in speechId){
                /*dialogue += "Dialogue id: ";
                    dialogue += item.ToString("X4") + "\r\n------------------------------------------\r\n";
                    if ((item & 0x07FF) <= speechTxt.Count)
                        dialogue += speechTxt[item & 0x07FF];
                    else
                        dialogue += "<Not a dialogue>";
                    dialogue += "\r\n\r\n\r\n";*/
                if ((item & 0x07FF) <= speechTxt.Count)
                {
                    dialogues.Add((speechTxt[item & 0x07FF], new List<ushort>()));
                }
            }
        }
        else
        {
            //dialogue += "<Action>\r\n";
        }
    }

    public bool IsVisibleOnStartup()
    {
        //bool contains =  VisibleActionIds.Contains(actionId);
        bool contains = !InvisibleActionId.Contains(actionId);
        return contains;
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
    private List<int> getSpeechId(BinaryReader br, int headerOffset, int actionId, out List<byte> completeAction)
    {
        // This works as the C0/2F97 routine in the SNES ROM
        // (I don't fully understand it yet) // HACKME

        List<int> output = new List<int>();
        completeAction = [];
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
            //completeAction += "CE/" + (var23 - headerOffset).ToString("X4") + ":\t";
            //completeAction += accumulator.ToString("X2") + " (";
            completeAction.Add((byte)accumulator);
            if (accumulator == 0xF0) break;
            var23 += actionSizes[accumulator - 0xF0];
            foreach (byte item in br.ReadBytes(actionSizes[accumulator - 0xF0] - 1))
            {
                completeAction.Add(item);
            }
            //completeAction += ")\r\n";
        }
        //Event.ReadEvent(br, headerOffset, )
        var23++;
       // completeAction += ")\r\n\r\n";

        while (var23 < var26)
        {
            output.Add(br.ReadByte() + br.ReadByte() * 0x0100);
            //completeAction += "CE/" + (var23 - headerOffset).ToString("X4") + ":\t";
            //completeAction += output.Last().ToString("X4") + "\r\n";
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
        //output += dialogue.Replace("[EOL]", "\r\n").Replace("[01]", "\r\n") + "\r\n";

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