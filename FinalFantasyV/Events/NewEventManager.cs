using System;
using System.Collections.Generic;
using Engine.RomReader;

namespace FinalFantasyV.Events;

public class NewEventManager
{
    public bool[] EventFlags = new bool[512];
    public bool[] NpcSwitches = new bool[921];
    
    public void SetFlag(int flag, bool status)
    {
        EventFlags[flag] = status;
    }

    public Queue<IGameEvent> CheckCollisionOnEvent(RomGame rom, byte x, byte y)
    {
        var e = rom.Map.GetEventsProperties(x,y);
        if (e == null) return null;
        return ProcessEvent(rom, e.bytes);
    }
    
    public Queue<IGameEvent> ProcessEvent(RomGame rom, List<List<byte>> bytes)
    {
        var gameEvents = new Queue<IGameEvent>();
        if (bytes is null) return new Queue<IGameEvent>();

        var ffsSeen = new List<int>();
        for (int i = 0; i < bytes.Count; i++)
        {
            if (bytes[i][0] == 0xFF)
            {
                ffsSeen.Add(i);
            }
        }
        
        Console.WriteLine(bytes);
        for (int i = 0; i < bytes.Count; i++)
        {
            var byteGrouping = bytes[i];
            var action = byteGrouping[0];

            var possibleLoop = ProcessLoops(rom, bytes, byteGrouping, i);
            if (possibleLoop.Item1 != null)
            {
                gameEvents.Enqueue(possibleLoop.Item1);
                i += possibleLoop.Item2;
            }
            
            else if (action == 0xFB ) // Continue if flag == off and flag > 0xFF
            {
                Console.WriteLine($"If Event Switch {byteGrouping[1] + 256} == Off");
                if (EventFlags[byteGrouping[1] + 256]) 
                    i += ForwardToNextFF(bytes, i);
            }
            else if (action == 0xFC) // Continue if flag == on and flag > 0xFF
            {
                Console.WriteLine($"If Event Switch {byteGrouping[1] + 256} == On");
                if (!EventFlags[byteGrouping[1] + 256])
                    i += ForwardToNextFF(bytes, i);
            }
            else if (action == 0xFD) // Continue if flag == if and flag <= 0xFF
            {
                Console.WriteLine($"If Event Switch {byteGrouping[1]} == Off");
                if (EventFlags[byteGrouping[1]])
                    i += ForwardToNextFF(bytes, i);
            }
            else if (action == 0xFE) // Continue if flag == on and flag <= 0xFF
            {
                Console.WriteLine($"If Event Switch {byteGrouping[1]} == On");
                if (!EventFlags[byteGrouping[1]])
                    i += ForwardToNextFF(bytes, i);
            }
            else if (action is 0xE0 or 0xE1 or 0xE3)
            {
                var mapIdx = CombineBytes(byteGrouping[1], byteGrouping[2]) & 0x03FF;
                var (mapX, mapY) = (byteGrouping[3] & 0x3F, byteGrouping[4] & 0x3F);
                Console.WriteLine($"Change Map: {mapIdx} at ({mapX},{mapY})");
                gameEvents.Enqueue(new EventChangeMap(byteGrouping));
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

    private (IGameEvent, int) ProcessLoops(RomGame rom, List<List<byte>> bytes, List<byte> byteGrouping, int startIndex)
    {
        var action = byteGrouping[0];
        if (action == 0xC7)
        {
            Console.WriteLine($"Execute the Next {byteGrouping[1]} byte(s) in Parallel");
            var events = ProcessNextNBytes(rom, bytes, startIndex, byteGrouping[1]);
            return (new ParallelEvents(events, byteGrouping[1]), events.Count);
            //i += events.Count;
        }
            
        if (action == 0xCF)
        {
            var (byteCount, repeatTimes) = (byteGrouping[2], byteGrouping[1]);
            Console.WriteLine($"Repeat the next {byteCount} byte(s) {repeatTimes} times (Parallel)");
            var events = ProcessNextNBytes(rom, bytes, startIndex, byteCount);
            return (new ParallelRepeatEvent(events, byteCount, repeatTimes), events.Count);
            //i += events.Count;
        }
            
        if (action == 0xCE)
        {
            var (byteCount, repeatTimes) = (byteGrouping[2], byteGrouping[1]);
            Console.WriteLine($"Repeat the next {byteCount} byte(s) {repeatTimes} times (Sequential)");
            var events = ProcessNextNBytes(rom, bytes, startIndex, byteCount);
            return (new RepeatSequentialEvents(events, repeatTimes, byteCount), events.Count);
            //i += events.Count;
        }

        return (null, 0);
    }

    List<IGameEvent> ProcessNextNBytes(RomGame rom, List<List<byte>> bytes, int startIndex, int count)
    {
        var events = new List<IGameEvent>();
        int bytesProcessed = 0;
        int offset = 1;
        while (bytesProcessed < count)
        {
            var possibleLoop = ProcessLoops(rom, bytes,  bytes[startIndex + offset], startIndex + offset);
            if (possibleLoop.Item1 != null)
            {
                events.Add(possibleLoop.Item1);
                bytesProcessed += bytes[startIndex + offset].Count;
                offset += possibleLoop.Item2;
                bytesProcessed += possibleLoop.Item2;
            }
            else
            {
                events.Add(processSingleAction(rom, bytes[startIndex+offset]));
                bytesProcessed += bytes[startIndex + offset].Count;
            }
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

        if (action is >= 0x70 and <= 0x76 or 0xB2 or 0xB3)
        {
            return new EventWait(byteGrouping);
        }

        if (action is 0xF3) return new EventCatchAll(byteGrouping);

        if (action >= 0xA2 && action <= 0xA6)
        {
            return new EventSwitch(byteGrouping);
        }

        if (action == 0xCA || action == 0xCB) return new NpcSwitch(byteGrouping);
        
        if (action == 0xD3) return new EventChangePosition(byteGrouping);

        if (action == 0xDB) 
            return new EventCatchAll(byteGrouping);
            
        if (action == 0xC8)
        {
            var dialogueOffset = CombineBytes(byteGrouping[1], byteGrouping[2]) & 0x7FFF;
            var text = rom.SpeechTxt[dialogueOffset];
            Console.WriteLine($"Display Dialogue: '{text}'");
            return new EventDialogue(text);
        }

        return new EventDoNothing();
    }

    EventDoAction ProcessAction(List<byte> data)
    {
        return new EventDoAction(data);
    }

    static int CombineBytes(byte lo, byte hi)
    {
        return (hi << 8) | lo;
    }
}