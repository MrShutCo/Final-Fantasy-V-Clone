using System;
using System.Collections.Generic;
using FinalFantasyV.GameStates;
using Microsoft.Xna.Framework;

namespace FinalFantasyV.Events;

public class EventChangeMap : IGameEvent
{
    public byte HexCode { get; init; } = 0xE1;
    public Action Completed { get; set; }

    private byte _properties;
    private byte _x;
    private byte _y;
    private byte _mapId;
    
    public EventChangeMap(List<byte> data)
    {
        _mapId = data[1];
        _properties = data[2];
        _x = (_mapId >= 5) ? (byte)(data[3] & 0x3F) : data[3];
        _y = (_mapId >= 5) ? (byte)(data[4] & 0x3F) : data[4];
    }

    public void OnStart(PartyState partyState, WorldState ws)
    {
        ws.ChangeMap(_mapId,  new Vector2(_x, _y) * 16);
        Completed?.Invoke();
    }

    public void Update(GameTime gameTime, WorldState ws)
    {
    }
}