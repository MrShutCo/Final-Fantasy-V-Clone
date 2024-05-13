using System;
using System.Collections.Generic;
using FinalFantasyV.GameStates;
using Microsoft.Xna.Framework;

namespace FinalFantasyV.Events;

public class EventChangePosition : IGameEvent
{
    public Action Completed { get; set; }
    private byte _x;
    private byte _y;
    private byte _objectId;
    
    public EventChangePosition(List<byte> data)
    {
        _objectId = (byte)(data[1] & 0x3F);
        _x = (byte)(data[2] & 0x3F);
        _y = (byte)(data[3] & 0x3F);
    }
    
    public void OnStart(PartyState partyState, WorldState ms)
    {
        ms.Objects[_objectId].Position = new Vector2(_x * 16, _y * 16);
        Completed?.Invoke();
    }
    
    public void Update(GameTime gameTime, WorldState ws)
    {
    }
}