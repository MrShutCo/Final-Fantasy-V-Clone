using System;
using Final_Fantasy_V.Models;
using FinalFantasyV.GameStates;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FinalFantasyV
{
	public abstract class BattleUnit
    {
        public double ATB;
        public Vector2 Position;

        protected SpriteSheet sheet;
		public Unit Unit;

		public Action OnActionFinished;

		const int ATBPerSecond = 50;

		public BattleUnit(SpriteSheet sheet, Unit unit, Vector2 position)
		{
			this.sheet = sheet;
			Unit = unit;
			Position = position;
		}

		public void ResetATB()
		{
            //var N1 = Math.Max(120 - ch.Agility + ch.Weight / 8, 1);
            //if (ch.Status.Haste) N1 = Math.Max(1, N1 / 2);
            //if (ch.Status.Slow) N1 = Math.Min(N1 * 2, 255);
            //ATB = (255 - N1);
            ATB = 0;
		}

        public void AdvanceATB(GameTime gameTime)
		{
            var diff = (gameTime.ElapsedGameTime.Milliseconds / 1000.0f * ATBPerSecond);
            ATB = Math.Min(255, ATB + diff);
        }

		public abstract void BeginAction();
		public abstract void Update(GameTime gameTime, PartyState ps);

        public virtual void Draw(SpriteBatch sb)
		{
			sheet.Draw(sb, Position);
		}
	}
}

