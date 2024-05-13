using Final_Fantasy_V.Models;
using FinalFantasyV.GameStates;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FinalFantasyV.Sprites
{
	public class BattleHero : BattleUnit
	{

        InputHandler input;
        //MenuSelector menuSelector;

        public BattleHero(SpriteSheet enemySheet, Character character, Vector2 position) : base(enemySheet, character, position)
		{
            input = new InputHandler();
            //menuSelector = new MenuSelector();
		}

        public override void BeginAction()
        {
            //input.RegisterKey(Keys.Up, () => menuSelector.MoveMenu(-1));
            //input.RegisterKey(Keys.Down, () => menuSelector.MoveMenu(1));
        }

        public override void Update(GameTime gameTime, PartyState ps)
        {

        }

        public override void Draw(SpriteBatch sb)
        {
            //menuSelector.Draw(sb);
            sheet.SetTile(0, (int)((Character)Unit).Job);
            base.Draw(sb);
        }
    }
}

