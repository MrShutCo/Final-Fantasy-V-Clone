using System;
using System.Collections.Generic;
using FinalFantasyV.GameStates;
using Microsoft.Xna.Framework;

namespace FinalFantasyV.Events;

public class ParallelEvents : IGameEvent
{
    public Action Completed { get; set; }
    private List<IGameEvent> _parallelEvents;

    private int _completedActions;
    private int _byteCount;

    public ParallelEvents(List<IGameEvent> _events, int byteCount)
    {
        _parallelEvents = _events;
        foreach (var e in _events)
        {
            e.Completed += OnComplete;
        }

        _byteCount = byteCount;
    }

    void OnComplete()
    {
        _completedActions++;
    }
    
    public void OnStart(PartyState partyState, WorldState ms)
    {
        Console.WriteLine($"Execute the Next {_byteCount} byte(s) in Parallel");
        foreach (var e in _parallelEvents)
        {
            e.OnStart(partyState, ms);
        }
    }
    
    public void Update(GameTime gameTime, WorldState ws)
    {
        if (_completedActions == _parallelEvents.Count)
        {
            Completed?.Invoke();
            foreach (var e in _parallelEvents) e.Completed = null;
        }
        
        foreach (var e in _parallelEvents)
        {
            e.Update(gameTime, ws);
        }
    }
}