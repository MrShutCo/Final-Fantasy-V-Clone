using System;
using System.Collections.Generic;
using Engine.RomReader;

namespace FinalFantasyV.Events;

public class NewEventManager
{
    public bool[] eventFlags = new bool[512];
    
    public void SetFlag(int flag, bool status)
    {
        eventFlags[flag] = status;
    }
    
    public Queue<IGameEvent> CheckCollisionOnEvent(RomGame rom, byte x, byte y)
    {
        var e = rom.Map.GetEventsProperties(x,y);
        var gameEvents = new Queue<IGameEvent>();
        if (e is null) return new Queue<IGameEvent>();

        var ffsSeen = new List<int>();
        for (int i = 0; i < e.bytes.Count; i++)
        {
            if (e.bytes[i][0] == 0xFF)
            {
                ffsSeen.Add(i);
            }
        }
        
        Console.WriteLine(e.bytes);
        for (int i = 0; i < e.bytes.Count; i++)
        {
            var byteGrouping = e.bytes[i];
            var action = byteGrouping[0];

            if (action == 0xFB ) // Continue if flag == off and flag > 0xFF
            {
                Console.WriteLine($"If Event Switch {byteGrouping[1] + 256} == Off");
                if (eventFlags[byteGrouping[1] + 256]) 
                    i += ForwardToNextFF(e.bytes, i);
            }
            else if (action == 0xFC) // Continue if flag == on and flag > 0xFF
            {
                Console.WriteLine($"If Event Switch {byteGrouping[1] + 256} == On");
                if (!eventFlags[byteGrouping[1] + 256])
                    i += ForwardToNextFF(e.bytes, i);
            }
            else if (action == 0xFD) // Continue if flag == if and flag <= 0xFF
            {
                Console.WriteLine($"If Event Switch {byteGrouping[1]} == Off");
                if (eventFlags[byteGrouping[1]])
                    i += ForwardToNextFF(e.bytes, i);
            }
            else if (action == 0xFE) // Continue if flag == on and flag <= 0xFF
            {
                Console.WriteLine($"If Event Switch {byteGrouping[1]} == On");
                if (!eventFlags[byteGrouping[1]])
                    i += ForwardToNextFF(e.bytes, i);
            }
            else if (action is 0xE0 or 0xE1 or 0xE3)
            {
                var mapIdx = CombineBytes(byteGrouping[1], byteGrouping[2]) & 0x03FF;
                var (mapX, mapY) = (byteGrouping[3], byteGrouping[4]);
                Console.WriteLine($"Change Map: {mapIdx} at ({mapX},{mapY})");
            }

            else if (action == 0xC7)
            {
                Console.WriteLine($"Execute the Next {byteGrouping[1]} byte(s) in Parallel");
                var events = ProcessNextNBytes(rom, e, i, byteGrouping[1]);
                gameEvents.Enqueue(new ParallelEvents(events, byteGrouping[1]));
                i += events.Count;
            }
            
            else if (action == 0xCF)
            {
                var (byteCount, repeatTimes) = (byteGrouping[2], byteGrouping[1]);
                Console.WriteLine($"Repeat the next {byteCount} byte(s) {repeatTimes} times (Parallel)");
                var events = ProcessNextNBytes(rom, e, i, byteCount);
                gameEvents.Enqueue(new ParallelRepeatEvent(events, byteCount, repeatTimes));
            }
            
            else if (action == 0xCE)
            {
                var (byteCount, repeatTimes) = (byteGrouping[2], byteGrouping[1]);
                Console.WriteLine($"Repeat the next {byteCount} byte(s) {repeatTimes} times (Sequential)");
                var events = ProcessNextNBytes(rom, e, i, byteCount);
                gameEvents.Enqueue(new RepeatSequentialEvents(events, repeatTimes, byteCount));
                i += events.Count;
            }

            else
            {
                var singleAction = processSingleAction(rom, byteGrouping);
                if (singleAction is EventDoNothing)
                {
                    Console.WriteLine($"Unhandled byte sequence {Convert.ToHexString(byteGrouping.ToArray())}");
                }
                else
                {
                    gameEvents.Enqueue(singleAction);
                }
            }
        }
        Console.WriteLine("======= Done Processing =======");
        return gameEvents;
    }

    List<IGameEvent> ProcessNextNBytes(RomGame rom, Event e, int startIndex, int count)
    {
        var events = new List<IGameEvent>();
        int bytesProcessed = 0;
        int offset = 1;
        while (bytesProcessed < count)
        {
            events.Add(processSingleAction(rom, e.bytes[startIndex+offset]));
            bytesProcessed += e.bytes[startIndex + offset].Count;
            offset++;
        }

        return events;
    }

    int ForwardToNextFF(List<List<byte>> bytes, int start)
    {
        int offset = 2;
        int numOfFFSeen = 0;
        while (numOfFFSeen < 1 && start+offset < bytes.Count)
        {
            if (bytes[start + offset][0] == 0xFF && bytes[start+offset].Count == 1)
            {
                numOfFFSeen++;
            }
            offset++;
        }

        return offset-1;
    }

    IGameEvent processSingleAction(RomGame rom, List<byte> byteGrouping)
    {
        var action = byteGrouping[0];
        if (action <= 0x40 || (action >= 0x80 && action <= 0x80+32))
        {
            return ProcessAction(byteGrouping);
        }

        if (action >= 0x70 && action <= 0x76)
        {
            var amount = action & 0xF;
            Console.WriteLine($"Wait {amount}");
            return new EventWait(byteGrouping);
        }

        if (action >= 0xA2 && action <= 0xA6)
        {
            return new EventSwitch(byteGrouping);
        }
        
        if (action == 0xD3) return new EventChangePosition(byteGrouping);

        if (action == 0xDB) 
            return new EventCatchAll(byteGrouping);
            
        if (action == 0xC8)
        {
            var dialogueOffset = CombineBytes(byteGrouping[1], byteGrouping[2]);
            var text = rom.SpeechTxt[dialogueOffset];
            Console.WriteLine($"Display Dialogue: '{text}'");
            return new EventDialogue(text);
        }

        return new EventDoNothing();
    }

    EventDoAction ProcessAction(List<byte> data)
    {
        var actor = data[0] < 0x80 ? "Party" : $"Object {data[0] & 0x7F}"; 
        var s = $"Do Action ({actor}): ";
        var action = data[0] < 0x80 ? data[0] : data[1];
        if (action == 0x01) Console.WriteLine(s + "Move Up");
        if (action == 0x02) Console.WriteLine(s + "Move Right");
        if (action == 0x03) Console.WriteLine(s + "Move Down");
        if (action == 0x04) Console.WriteLine(s + "Move Left");
        if (action == 0x09) Console.WriteLine(s + "Show Object");
        if (action == 0x10) Console.WriteLine(s + "Hide Object");
        
        return new EventDoAction(data);
    }

    static int CombineBytes(byte lo, byte hi)
    {
        return (hi << 8) | lo;
    }
}