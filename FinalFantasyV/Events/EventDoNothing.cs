using System;
using FinalFantasyV.GameStates;
using Microsoft.Xna.Framework;

namespace FinalFantasyV.Events;

public class EventDoNothing : IGameEvent
{
    public Action Completed { get; set; }
    private bool hasCompleted = false;
    public void OnStart(PartyState partyState, WorldState ms)
    {
        if (hasCompleted) return;
        Completed?.Invoke();
        hasCompleted = true;
    }
    
    public void Update(GameTime gameTime, WorldState ws)
    {
    }
}