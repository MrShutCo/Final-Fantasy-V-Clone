using System;
using FinalFantasyV.GameStates;
using Microsoft.Xna.Framework;

namespace FinalFantasyV.Events;

public interface IGameEvent
{
    //byte HexCode { get; init; }
    Action Completed { get; set; }
    //void Read(List<byte> data);
    void OnStart(PartyState partyState, WorldState ms);
    void Update(GameTime gameTime, WorldState ws);
}