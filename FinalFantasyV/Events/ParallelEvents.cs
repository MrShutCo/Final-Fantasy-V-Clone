using System;
using System.Collections.Generic;
using FinalFantasyV.GameStates;
using Microsoft.Xna.Framework;

namespace FinalFantasyV.Events;

public class ParallelRepeatEvent : IGameEvent
{
    public Action Completed { get; set; }
    private List<IGameEvent> _parallelEvents;

    private int _completedActions;
    private int _byteCount;
    private int _repeatCount;
    private int _timesRepeated = 0;

    private PartyState _partyState;
    
    public ParallelRepeatEvent(List<IGameEvent> _events, int byteCount, int numTimes)
    {
        _parallelEvents = _events;
        _completedActions = 0;
        _byteCount = byteCount;
        _repeatCount = numTimes;
        
        foreach (var e in _parallelEvents)
        {
            e.Completed += OnComplete;
        }
    }
    
    void OnComplete()
    {
        _completedActions++;
    }
    
    void StartAllActions(PartyState partyState, WorldState ms)
    {
        foreach (var e in _parallelEvents)
        {
            e.OnStart(partyState, ms);
        }

        _partyState = partyState;
    }
    
    public void OnStart(PartyState partyState, WorldState ms)
    {
        Console.WriteLine("\n====== Parallel Repeat Event Start ======");
        Console.WriteLine($"Repeat the Next {_byteCount} byte(s) {_repeatCount} Times (Parallel)");
        StartAllActions(partyState, ms);
    }

    public void Update(GameTime gameTime, WorldState ws)
    {
        if (_completedActions >= _parallelEvents.Count)
        {
            _timesRepeated++;
            if (_timesRepeated == _repeatCount)
            {
                Completed?.Invoke();
                Console.WriteLine("====== Parallel Repeat Event Done ======\n");
                foreach (var e in _parallelEvents) e.Completed = null;
            }
            else
            {

                _completedActions = 0;
                StartAllActions(_partyState, ws);
            }
        }
        
        foreach (var e in _parallelEvents)
        {
            e.Update(gameTime, ws);
        }
    }
}

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
        Console.WriteLine("\n====== Parallel Event Start ======");
        Console.WriteLine($"Execute the Next {_byteCount} byte(s) in Parallel");
        foreach (var e in _parallelEvents)
        {
            e.OnStart(partyState, ms);
        }
    }
    
    public void Update(GameTime gameTime, WorldState ws)
    {
        if (_completedActions >= _parallelEvents.Count)
        {
            Completed?.Invoke();
            Console.WriteLine("====== Parallel Event Done ======\n");
            foreach (var e in _parallelEvents) e.Completed = null;
        }
        
        foreach (var e in _parallelEvents)
        {
            e.Update(gameTime, ws);
        }
    }
}