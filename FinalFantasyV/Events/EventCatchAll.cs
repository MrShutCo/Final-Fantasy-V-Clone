using System;
using System.Collections.Generic;
using FinalFantasyV.GameStates;
using Microsoft.Xna.Framework;

namespace FinalFantasyV.Events;

public class EventCatchAll : IGameEvent
{
    public Action Completed { get; set; }
    private byte action;

    public EventCatchAll(List<byte> data)
    {
        action = data[0];
    }
    
    public void OnStart(PartyState partyState, WorldState ms)
    {
        if (action == 0xDB)
        {
            Console.WriteLine("Update Party State");
            // Update Party Sprite
            /*foreach (var obj in ms.Objects)
            {
                if (obj.Position == ms.WorldCharacter.Position)
                {
                    obj.IsVisible = false;
                }
            }*/
            Completed?.Invoke();
        }
    }

    public void Update(GameTime gameTime, WorldState ws)
    {
        
    }
}