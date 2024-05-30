using System;
using System.Collections.Generic;
using FinalFantasyV.GameStates;
using Microsoft.Xna.Framework;

namespace FinalFantasyV.Events;

public class EventSwitch : IGameEvent
{
    public Action Completed { get; set; }
    private int _flag;
    private bool _status;

    public EventSwitch(List<byte> data)
    {
        if (data[0] == 0xA4)
        {
            _flag = data[1] + 256;
            _status = true;
        }
        if (data[0] == 0xA5)
        {
            _flag = data[1] + 256;
            _status = false;
        }
        if (data[0] == 0xA2)
        {
            _flag = data[1];
            _status = true;
        }
        if (data[0] == 0xA3)
        {
            _flag = data[1];
            _status = false;
        }
        Console.WriteLine($"Change Event Switch: Event Switch {_flag} = {(_status ? "On" : "Off")}");
    }
    
    
    public void OnStart(PartyState partyState, WorldState ms)
    {
        var statusText = _status ? "On" : "Off";
        Console.WriteLine($"Change Event Switch: Event Switch {_flag} = {statusText}");
        ms.SetFlag(_flag, _status);
        Completed?.Invoke();
    }
    
    public void Update(GameTime gameTime, WorldState ws)
    {
    }
}

public class NpcSwitch : IGameEvent
{
    public Action Completed { get; set; }
    private int _flag;
    private bool _status;

    public NpcSwitch(List<byte> data)
    {
        _flag = data[1] + data[2] * 0x100;
        if (data[0] == 0xCA) _status = true;
        if (data[0] == 0xCB) _status = false;
        Console.WriteLine($"Change NPC Switch: NPC Switch {_flag} = {(_status ? "On" : "Off")}");
    }
    
    
    public void OnStart(PartyState partyState, WorldState ms)
    {
        var statusText = _status ? "On" : "Off";
        Console.WriteLine($"Change NPC Switch: NPC Switch {_flag} = {statusText}");
        ms.SetFlag(_flag, _status);
        Completed?.Invoke();
    }
    
    public void Update(GameTime gameTime, WorldState ws)
    {
    }
}