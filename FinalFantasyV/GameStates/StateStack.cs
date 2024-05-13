using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FinalFantasyV.GameStates
{
	public interface IState
	{
        StateStack stateStack { get; set; }
        void Update(GameTime gameTime, PartyState ps);
        void Render(SpriteBatch spriteBatch, PartyState ps);
        void OnEnter(PartyState ps);
        void OnExit();
	}

	public class StateStack
	{
        Dictionary<string, IState> mStates;
		Stack<IState> mStack;

        public StateStack()
        {
            mStates = new Dictionary<string, IState>();
            mStack = new Stack<IState>();
        }

        public void Update(GameTime gameTime, PartyState ps)
        {
            IState top = mStack.First();
            top.Update(gameTime, ps);
        }

        public IState Get(string name) => mStates[name];

        public void Add(string name, IState state)
        {
            mStates[name] = state;
            mStates[name].stateStack = this;
        }

        public void Render(SpriteBatch spriteBatch, PartyState ps)
        {
            IState top = mStack.First();
            top.Render(spriteBatch, ps);
        }

        public void Push(String name, PartyState ps)
        {
            IState state = mStates[name];
            mStack.Push(state);
            state.OnEnter(ps);
        }

        public IState Pop()
        {
            mStack.First().OnExit();
            return mStack.Pop();
        }
    }
}

