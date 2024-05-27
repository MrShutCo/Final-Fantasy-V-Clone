using System;
using FinalFantasyV.GameStates;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace FinalFantasyV.Events;

public class EventDialogue : IGameEvent
{
    public Action Completed { get; set; }
    private string _text;
    
    public EventDialogue(string text)
    {
        _text = text;
    }
    
    public void OnStart(PartyState partyState, WorldState ms)
    {
        ms.ShowDialogue(_text);
    }

    public void Update(GameTime gameTime, WorldState ws)
    {
        if (InputHandler.KeyReleased(Keys.Space))
        {
            ws.HideDialogue();
            Completed?.Invoke();
        }
    }
}