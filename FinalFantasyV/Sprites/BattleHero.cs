using System.Collections.Generic;
using System.Linq;
using Final_Fantasy_V.Models;
using FinalFantasyV.GameStates;
using FinalFantasyV.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
namespace FinalFantasyV.Sprites
{
	public class BattleHero : BattleUnit
	{

        InputHandler input;
        //MenuSelector menuSelector;
        private Animation _animation;
        private Animation _moveAnimation;
        private bool _isActing;
        
        private List<Vector2> walkingAnimVecs = [new(0,0), new (1,0), new Vector2(0,0)];
        private List<float> walkingTimes = [500f, 250f, 250f];

        public BattleHero(SpriteSheet enemySheet, Character character, Vector2 position) : base(enemySheet, character, position)
		{
            input = new InputHandler();
            //menuSelector = new MenuSelector();
		}

        public override void BeginAction()
        {
            _isActing = true;
            List<Vector2> walkingMoveVecs = [new(0, 0), new Vector2(-24, 0), new Vector2(-32, 0)];
            walkingMoveVecs = walkingMoveVecs.Select(vec => vec + Position).ToList();
            
            _animation = new Animation(walkingAnimVecs, walkingTimes, false);
            _moveAnimation = new Animation(walkingMoveVecs, walkingTimes, false)
            {
                IsLerpingFrames = true
            };
            _animation.StartAnimation();
            _moveAnimation.StartAnimation();
        }

        public override void Update(GameTime gameTime, PartyState ps)
        {
            _animation?.Update(gameTime);
            _moveAnimation?.Update(gameTime);

            if (_isActing && _moveAnimation is { IsActive: false })
            {
                _isActing = false;
                _animation = null;
                _moveAnimation = null;
                Position += new Vector2(32, 0);
                ResetATB();
                OnActionFinished?.Invoke();
            }
        }

        public override void Draw(SpriteBatch sb)
        {
            //menuSelector.Draw(sb);
            //sheet.SetTile(0, (int)((Character)Unit).Job);

            if (_animation != null)
            {
                var frame = _animation.GetAnimationFrame();
                sheet.SetTile((int)frame.Item1.X, (int)((Character)Unit).Job);
            }

            if (_moveAnimation != null)
            {
                var frame = _moveAnimation.GetAnimationFrame();
                Position = frame.Item1;
            }
            
            base.Draw(sb);
        }
    }
}

