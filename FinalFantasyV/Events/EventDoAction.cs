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

        if (IsParty && _action >= 16)
        {
            _action += 16;
        }
        PrintAction();
    }

    void PrintAction()
    {
        var actor = _objectId == 0 ? "Party" : $"Object {_objectId}"; 
        var s = $"Do Action ({actor}): ";
        
        if (_action == 1) Console.WriteLine(s + "Move Up");
        if (_action == 2) Console.WriteLine(s + "Move Right");
        if (_action == 3) Console.WriteLine(s + "Move Down");
        if (_action == 4) Console.WriteLine(s + "Move Left");
        if (_action == 9) Console.WriteLine(s + "Show Object");
        if (_action == 10) Console.WriteLine(s + "Hide Object");
        if (_action == 32) Console.WriteLine(s + "Face Up");
        if (_action == 34) Console.WriteLine(s + "Face Right");
        if (_action == 36) Console.WriteLine(s + "Face Down");
        if (_action == 38) Console.WriteLine(s + "Face Left");
    }

    public void OnStart(PartyState partyState, WorldState ws)
    {
        
        PrintAction();
        
        var charToAct = ws.WorldCharacter;
        if (!IsParty)
            charToAct = ws.Objects[_objectId];

        if (_action == 0x1) charToAct.Move(ECharacterMove.Up, WorldCharacter.NormalWalkingSpeed);
        else if (_action == 0x2) charToAct.Move(ECharacterMove.Right, WorldCharacter.NormalWalkingSpeed);
        else if (_action == 0x3) charToAct.Move(ECharacterMove.Down, WorldCharacter.NormalWalkingSpeed);
        else if (_action == 0x4) charToAct.Move(ECharacterMove.Left, WorldCharacter.NormalWalkingSpeed);

        else if (_action == 9) charToAct.IsVisible = true;
        else if (_action == 10) charToAct.IsVisible = false;
        
        else if (_action == 32) charToAct.Face(ECharacterMove.Up);
        else if (_action == 34) charToAct.Face(ECharacterMove.Right);
        else if (_action == 36) charToAct.Face(ECharacterMove.Down);
        else if (_action == 38) charToAct.Face(ECharacterMove.Left);
        else
        {
            charToAct.SetAction(_action);
        }
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