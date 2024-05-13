using System.Collections.Generic;
using System.Linq;
using Engine.RomReader;
using FinalFantasyV.GameStates;
using Microsoft.Xna.Framework;

namespace FinalFantasyV.Events;

public class EventManager
{
    private readonly Queue<List<IGameEvent>> _events = new();
    private int _eventsCompleted;
    private bool[] _eventSwitches = new bool[512];

    public bool Update(GameTime gt, PartyState ps, WorldState ws)
    {
        if (_events.Count == 0) return false;
        foreach (var gameEvent in _events.Peek())
         gameEvent.OnStart(ps, ws);
        return true;
    }

    public void CheckCollisionOnEvent(RomGame rom, byte x, byte y)
    {
        var e = rom.Map.GetEventsProperties(x,y);
        if (e is not null)
        {
            List<List<IGameEvent>> events = ProcessEventScript(e.bytes).ToList();
            foreach (var ev in events)
                RegisterNewSetEvents(ev);
        }
    }
    
    private void OnComplete()
    {
        // TODO: THIS SUCKS ASS
        _eventsCompleted++;
        if (_eventsCompleted == _events.Peek().Count)
        {
            foreach (var e in _events.Peek())
                e.Completed = null;
            _events.Dequeue();
            _eventsCompleted = 0;
            if (_events.Count > 0)
                foreach (var e in _events.Peek())
                    e.Completed += OnComplete;
        }
    }
    
    private void RegisterNewSetEvents(List<IGameEvent> events)
    {
        if (_events.Count == 0)
            foreach (var e in events)
                e.Completed += OnComplete;

        _events.Enqueue(events);
    }
    
    public IEnumerable<List<IGameEvent>> ProcessEventScript( List<List<byte>> bytes)
    {
        for (int i = 0; i < bytes.Count; i++)
        {
            var action = bytes[i];
            // TODO: both sequential and parallel dont work
            if (action[0] == 0xCF) // Repeat next several events in parallel
            {
                int timesRepeated = action[1];
                for (int j = 0; j < timesRepeated; j++)
                {
                    int totalBytes = 0;
                    var repeatedEvents = new List<IGameEvent>();
                    while (totalBytes < action[2])
                    {
                        totalBytes += bytes[i + 1].Count;
                        repeatedEvents.Add(processSingleEvent(bytes[i + 1]));
                        i++;
                    }
                    yield return repeatedEvents;
                }
            }
            else if (action[0] == 0xCE)
            {
                int timesRepeated = action[1];
                for (int j = 0; j < timesRepeated; j++)
                {
                    int totalBytes = 0;
                    
                    while (totalBytes < action[2])
                    {
                        totalBytes += bytes[i + 1].Count;
                        yield return new List<IGameEvent> {processSingleEvent(bytes[i + 1])};
                        i++;
                    }
                }
            }
            else
            {
                // TODO all these are wrong. We may still need to process events
                if (action[0] == 0xFC) // If event switch == ON && > 256
                {
                    if (!_eventSwitches[action[1]+256]) break;
                }
                if (action[0] == 0xFA) // If event switch == ON && < 256
                {
                    if (!_eventSwitches[action[1]]) break;
                }
                if (action[0] == 0xFD) // If event switch == OFF && < 256
                {
                    if (_eventSwitches[action[1]]) break;
                }
                if (action[0] == 0xFB) // If event switch == OFF && >= 256
                {
                    if (_eventSwitches[action[1] + 256]) break;
                }
                
                if (action[0] == 0xA4) // Set event on && >= 256
                    _eventSwitches[action[1]+256] = true;
                if (action[0] == 0xA2) // Set event on && < 256
                    _eventSwitches[action[1]] = true;
                yield return new List<IGameEvent> {processSingleEvent(action)};
            }
            
            
        }
    }

    static IGameEvent processSingleEvent(List<byte> action)
    {
        if (action[0] >= 0x70 && action[0] < 0x76)  return new EventWait(action);
        //if (action[0] >= 0x76 && action[1] < 0x79) yield return new EventWait(action);
        if (action[0] <= 0x40)  return new EventDoAction(action);
        if (action[0] >= 0x80 && action[0] <= 0x80+32)  return new EventDoAction(action);
        if (action[0] == 0xD3) return new EventChangePosition(action);
        return new EventDoNothing();
    }
    

}