using System;
using System.Collections.Generic;
using FinalFantasyV.GameStates;
using Microsoft.Xna.Framework;

namespace FinalFantasyV.Events;

public class EventWait : IGameEvent
{
    public byte HexCode { get; init; }
    public Action Completed { get; set; }

    private float _waitTime;
    private float currWait;
    public EventWait(List<byte> data)
    {
        var wait = data[0];
        if (wait == 0x70) _waitTime = 0.25f;
        if (wait == 0x71) _waitTime = 0.5f;
        if (wait == 0x72) _waitTime = 0.75f;
        if (wait == 0x73) _waitTime = 1f;
        if (wait == 0x74) _waitTime = 1.5f;
        if (wait == 0x75) _waitTime = 2f;
    }
    public void OnStart(PartyState partyState, WorldState ms)
    {
        Console.WriteLine($"Wait ({_waitTime}s)");
    }
    
    public void Update(GameTime gameTime, WorldState ws)
    {
        currWait += gameTime.ElapsedGameTime.Milliseconds / 1000.0f;
        if (currWait >= _waitTime)
            Completed?.Invoke();
    }
}