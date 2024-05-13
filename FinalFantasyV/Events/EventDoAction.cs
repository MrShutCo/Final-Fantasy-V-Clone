using System;
using System.Collections.Generic;
using FinalFantasyV.GameStates;
using Microsoft.Xna.Framework;

namespace FinalFantasyV.Events;

public class EventDoAction : IGameEvent
{
    public Action Completed { get; set; }
    public bool IsParty { get; init; }
    private byte _action;
    private byte _objectId;

    private bool hasActed;
    public EventDoAction(List<byte> data)
    {
        if (data.Count == 1)
        {
            IsParty = true;
            _action = data[0];
        }
        else
        {
            IsParty = false;
            _objectId = (byte)(data[0] & 0x7F);
            _action = data[1];
        }
    }

    public void OnStart(PartyState partyState, WorldState ws)
    {
        
        var actor = _objectId == 0 ? "Party" : $"Object {_objectId}"; 
        var s = $"Do Action ({actor}): ";
        if (_action == 0x01) Console.WriteLine(s + "Move Up");
        if (_action == 0x02) Console.WriteLine(s + "Move Right");
        if (_action == 0x03) Console.WriteLine(s + "Move Down");
        if (_action == 0x04) Console.WriteLine(s + "Move Left");
        if (_action == 0x09) Console.WriteLine(s + "Show Object");
        if (_action == 0x10) Console.WriteLine(s + "Hide Object");
        if (_action == 16) Console.WriteLine(s + "Face Up");
        if (_action == 18) Console.WriteLine(s + "Face Right");
        if (_action == 20) Console.WriteLine(s + "Face Down");
        if (_action == 22) Console.WriteLine(s + "Face Down");
        
        var charToAct = ws.WorldCharacter;
        if (!IsParty)
            charToAct = ws.Objects[_objectId];
        

        if (_action == 0x1) charToAct.Move(ECharacterMove.Up);
        if (_action == 0x2) charToAct.Move(ECharacterMove.Right);
        if (_action == 0x3) charToAct.Move(ECharacterMove.Down);
        if (_action == 0x4) charToAct.Move(ECharacterMove.Left);

        if (_action == 9) charToAct.IsVisible = true;
        if (_action == 10) charToAct.IsVisible = false;
        
        if (_action == 16) charToAct.Face(ECharacterMove.Up);
        if (_action == 18) charToAct.Face(ECharacterMove.Right);
        if (_action == 20) charToAct.Face(ECharacterMove.Down);
        if (_action == 22) charToAct.Face(ECharacterMove.Left);
        if (_action > 0x4)
            Completed?.Invoke();
        hasActed = true;
    }

    public void Update(GameTime gameTime, WorldState ws)
    {
        var charToAct = ws.WorldCharacter;
        if (!IsParty)
            charToAct = ws.Objects[_objectId];
        if (!charToAct.IsMoving && _action <= 0x4)
        {
            Completed?.Invoke();
        }

    }
}