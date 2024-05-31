using System;
using System.Collections.Generic;
using FinalFantasyV.GameStates;
using Microsoft.Xna.Framework;

namespace FinalFantasyV.Events;

public class EventCatchAll : IGameEvent
{
    public Action Completed { get; set; }
    private List<byte> action;

    public EventCatchAll(List<byte> data)
    {
        action = data;
    }
    
    public void OnStart(PartyState partyState, WorldState ms)
    {
        if (action[0] == 0xDB)
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

        if (action[0] == 0xF3)
        {
            var layer = (byte)((action[1] & 0xC0) >> 6);
            var xStart = action[1] & 0x3F;
            var yStart = action[2] & 0x3F;
            var width = (action[3] & 0x0F) + 1;
            var height = ((action[3] & 0xF0) >> 4) + 1;
            Console.WriteLine($"Change Map Background: {width}x{height} tiles at ({xStart},{yStart})");
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    ms.SetBackgroundTileAt(layer, xStart + x, yStart + y, action[3+y*width+x+1]);
                }
            }
            Completed?.Invoke();
        }
    }

    public void Update(GameTime gameTime, WorldState ws)
    {
        
    }
}