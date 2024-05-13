using System.Collections.Generic;
using Engine.RomReader;
using FinalFantasyV.Events;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace FinalFantasyV.GameStates;

public class MainState : IState
{
    public StateStack stateStack { get; set; }

    private List<IGameEvent> _events;
    public RomGame Rom;
    public SpriteSheet[] Objects;
    
    public MainState(ContentManager cm)
    {
        _events = new List<IGameEvent>();
        Objects = new SpriteSheet[32];
        Rom = new RomGame();
    }

    public void Update(GameTime gameTime, PartyState ps)
    {
        //if (_events.Count > 0)
            //_events[0].Update(gameTime, ps, this);
        
    }

    public void Render(SpriteBatch spriteBatch, PartyState ps)
    {
        throw new System.NotImplementedException();
    }

    public void OnEnter(PartyState ps)
    {
        var layer = Rom.GetLayers(122);
        Rom.Update(122);
    }

    public void OnExit()
    {
    }
}