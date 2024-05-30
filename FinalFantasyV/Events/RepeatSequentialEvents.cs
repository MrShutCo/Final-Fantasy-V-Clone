using System;
using System.Collections.Generic;
using FinalFantasyV.GameStates;
using Microsoft.Xna.Framework;

namespace FinalFantasyV.Events;

public class RepeatSequentialEvents : IGameEvent
{
    public Action Completed { get; set; }
    private List<IGameEvent> _sequentialEvents;
    private int NumTimes;
    private int _timesCompleted = 0;
    private int _currentEvent = 0;
    private int _byteCount = 0;

    private bool _isCompleteCurrentEvent;
    private PartyState _partyState;
    
    public RepeatSequentialEvents(List<IGameEvent> _events, int numTimes, int byteCount)
    {
        _sequentialEvents = _events;
        NumTimes = numTimes;
        _byteCount = byteCount;
    }
    
    public void OnStart(PartyState partyState, WorldState ms)
    {
        Console.WriteLine("\n====== Repeat Event Start ======");
        Console.WriteLine($"Repeat the next {_byteCount} byte(s) {NumTimes} times (Sequential)");
        _sequentialEvents[0].Completed += OnComplete;
        _sequentialEvents[0].OnStart(partyState, ms);
        _partyState = partyState;
    }

    public void Update(GameTime gameTime, WorldState ws)
    {
        if (_isCompleteCurrentEvent)
        {
            _sequentialEvents[_currentEvent].Completed -= OnComplete;
            _currentEvent += 1;
            if (_currentEvent == _sequentialEvents.Count)
            {
                _timesCompleted++;
                _currentEvent = 0;
            }
            
        }
        
        if (_timesCompleted == NumTimes)
        {
            _sequentialEvents[_currentEvent].Completed -= OnComplete;
            Console.WriteLine("====== Repeat Event Done ======\n");
            Completed?.Invoke();
            return;
        }
        if (_isCompleteCurrentEvent)
        {
            _sequentialEvents[_currentEvent].Completed += OnComplete;
            _sequentialEvents[_currentEvent].OnStart(_partyState, ws);
            _isCompleteCurrentEvent = false;
        }

        _sequentialEvents[_currentEvent].Update(gameTime, ws);
    }

    void OnComplete()
    {
        _isCompleteCurrentEvent = true;
    }
}