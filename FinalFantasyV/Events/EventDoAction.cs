using System;
using System.Collections.Generic;
using FinalFantasyV.GameStates;
using FinalFantasyV.Sprites;
using Microsoft.Xna.Framework;

namespace FinalFantasyV.Events;

public class EventDoAction : IGameEvent
{
    public Action Completed { get; set; }
    public bool IsParty { get; init; }
    private byte _action;
    private byte _objectId;

    private bool hasActed;
    
    Dictionary<byte, string> actions = new Dictionary<byte, string>
        {
            {0, "Do Nothing"},
            {1, "Move Up"},
            {2, "Move Right"},
            {3, "Move Down"},
            {4, "Move Left"},
            {5, "Jump 1 Tile"},
            {6, "Jump 2 Tiles"},
            {7, "Toggle Sprite Priority (Top Half)"},
            {8, "Toggle Sprite Priority (Bottom Half)"},
            {9, "Show Object"},
            {10, "Hide Object"},
            {11, "Toggle Walking Animation"},
            {16, "Set Animation Speed to Normal"},
            {17, "Set Animation Speed to Slowest"},
            {18, "Set Animation Speed to Slow"},
            {19, "Set Animation Speed to Fast"},
            {20, "Toggle Layer Priority (Top Half)"},
            {21, "Toggle Layer Priority (Bottom Half)"},
            {32, "Face Up"},
            {33, "Face Up (Walking)"},
            {34, "Face Right"},
            {35, "Face Right (Walking)"},
            {36, "Face Down"},
            {37, "Face Down (Walking)"},
            {38, "Face Left"},
            {39, "Face Left (Walking)"},
            {40, "Face Down (Alt.)"},
            {41, "Face Up (Alt.)"},
            {42, "Face Left (Alt.)"},
            {43, "Face Left (Walking, Alt.)"},
            {44, "Face Down (h-flip)"},
            {45, "Face Up (h-flip)"},
            {46, "Face Left (h-flip)"},
            {47, "Face Right (h-flip)"},
            {48, "Face Down (Right Hand <)"},
            {49, "Face Down (Right Hand -)"},
            {50, "Face Down (Right Hand >)"},
            {51, "Face Down (Left Hand >)"},
            {52, "Face Down (Left Hand -)"},
            {53, "Face Down (Left Hand <)"},
            {54, "Face Up (Right Hand >)"},
            {55, "Face Up (Right Hand <)"},
            {56, "Face Up (Left Hand <)"},
            {57, "Face Up (Left Hand >)"},
            {58, "Face Left (Arm Up)"},
            {59, "Face Left (Arm Forward)"},
            {60, "Face Right (Arm Up)"},
            {61, "Face Right (Arm Forward)"},
            {62, "Face Down (Head Down)"},
            {63, "Face Up (Head Down)"},
            {64, "Face Left (Head Down)"},
            {65, "Face Right (Head Down)"},
            {66, "Knocked Down"},
            {67, "Knocked Down (h-flip)"},
            {68, "Kick Left"},
            {69, "Kick Right"},
            {70, "Laugh 1"},
            {71, "Laugh 2"},
            {72, "Face Down (Crouching)"},
            {73, "Face Down (Jumping)"},
            {74, "Face Down (Surprised)"},
            {75, "Face Up (Alt.)"},
            {76, "Face Down (Angry)"},
            {77, "Face Up (Arms Up <>)"},
            {78, "Face Up (Arms Up ><)"},
            {79, "Face Down (Head <)"},
            {80, "Face Down (Head >)"},
            {81, "Face Up (Symmetric)"}
        };
    
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

        var exists = actions.TryGetValue(_action, out string value);
        if (exists) Console.WriteLine(s + value);
        else Console.WriteLine(s + $"Action {_action}");
        
    }

    public void OnStart(PartyState partyState, WorldState ws)
    {
        PrintAction();
        
        var charToAct = ws.WorldCharacter;
        var speed = WorldCharacter.NormalWalkingSpeed;
        if (!IsParty)
        {
            charToAct = ws.Objects[_objectId];
            speed = (charToAct as WorldNPC).Speed;
        }

        if (_action == 0x1) charToAct.Move(ECharacterMove.Up, speed);
        else if (_action == 0x2) charToAct.Move(ECharacterMove.Right, speed);
        else if (_action == 0x3) charToAct.Move(ECharacterMove.Down, speed);
        else if (_action == 0x4) charToAct.Move(ECharacterMove.Left, speed);
        

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