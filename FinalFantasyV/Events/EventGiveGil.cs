using System;
using System.Collections.Generic;
using FinalFantasyV.GameStates;
using Microsoft.Xna.Framework;

namespace FinalFantasyV.Events;

public class EventGiveGil : IGameEvent
{
    public byte HexCode { get; init; } = 0xAF;
    public Action Completed { get; set; }

    private byte _gilAmount;
    
    public EventGiveGil(List<byte> data)
    {
        _gilAmount = data[1];
    }
    
    public void OnStart(PartyState partyState, WorldState ms)
    {
        partyState.Gil += _gilAmount * 10;
        Completed?.Invoke();
    }
    
    public void Update(GameTime gameTime, WorldState ws)
    {
    }
}