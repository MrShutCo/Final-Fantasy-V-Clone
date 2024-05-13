using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FinalFantasyV
{
	public enum ECharacterMove
	{
		Down,Left,Right,Up
	}

	public class Tween
	{
		Vector2 _starting;
		Vector2 _end;
		double _timeMilliseconds;
		double _timeElapsed;
		Vector2[] _tileSheetSprites;

        public Tween(Vector2 starting, Vector2 end, double timeMilliseconds, Vector2[] tileSheetSprites)
        {
            _starting = starting;
            _end = end;
            _timeMilliseconds = timeMilliseconds;
			_tileSheetSprites = tileSheetSprites;
        }

		public (Vector2, Vector2) Lerp(GameTime gt)
		{
			_timeElapsed += gt.ElapsedGameTime.Milliseconds;
			_timeElapsed = Math.Min(_timeElapsed, _timeMilliseconds);
			var lerp = Vector2.Lerp(_starting, _end, (float)(_timeElapsed / _timeMilliseconds));
			lerp.X = (int)lerp.X;
			lerp.Y = (int)lerp.Y;
			int tileIndex = (int)(_timeElapsed) / (int)(_timeMilliseconds/(_tileSheetSprites.Length-1));
            return (lerp, _tileSheetSprites[tileIndex]);
		}

		public bool IsDone(Vector2 pos) => pos == _end;  
    }

	public class WorldCharacter
	{
		private Random r = new();
		Tween _tween;
		public bool IsMoving { get; set; }
		public bool IsVisible { get; set; }
		bool _isOddStepCount = true;
		bool _isMirrored;
		private int _walkingType;

		public event Action DoneMovement;

        public Vector2 Position;
        private SpriteSheet _character;
		public WorldCharacter(SpriteSheet spriteSheet, Vector2 pos, int walkingType = -1)
		{
			_character = spriteSheet;
			Position = pos;
			_walkingType = walkingType;
			IsVisible = true;
		}

		public Vector2 GetTilePosition() => Position / 16;

		public void Update(GameTime gt)
		{
			if (_walkingType != -1)
			{
				//Move((ECharacterMove)r.Next(0,3));
			}
			
			if (IsMoving)
			{
				var animation = Vector2.Zero;
				(Position, animation) = _tween.Lerp(gt);
                if (_tween.IsDone(Position))
                {
                    IsMoving = false;
                    DoneMovement?.Invoke();
                }
                _character.SetTile((int)animation.X, (int)animation.Y);
            }
		}

        public void Draw(SpriteBatch sb)
        {
	        if (IsVisible)
				_character.Draw(sb, Position, _isMirrored);
        }

		public void Face(ECharacterMove move)
		{
			if (IsMoving) return;
			if (move == ECharacterMove.Left)
			{
				_isMirrored = false;
				_character.SetTile(4, 0);
			}
            if (move == ECharacterMove.Right)
            {
                _isMirrored = true;
                _character.SetTile(4, 0);
            }
            if (move == ECharacterMove.Up)
            {
                _character.SetTile(2, 0);
            }
            if (move == ECharacterMove.Down)
            {
                _character.SetTile(0, 0);
            }
        }

		public void Move(ECharacterMove move)
		{
			if (move == ECharacterMove.Left && !IsMoving)
			{
                IsMoving = true;
                _isMirrored = false;
                _tween = new Tween(Position, Position + new Vector2(-16,0), 125, new Vector2[] {new(4,0), new(5,0), new(4,0) });
            }
			if (move == ECharacterMove.Right && !IsMoving)
			{
                IsMoving = true;
				_isMirrored = true;
                _tween = new Tween(Position, Position + new Vector2(16, 0), 125, new Vector2[] { new(4, 0), new(5, 0), new(4, 0) });
            }
            if (move == ECharacterMove.Up && !IsMoving)
			{
                IsMoving = true;
                _tween = new Tween(Position, Position + new Vector2(0, -16), 125, new Vector2[] { new(2,0), new(3, 0), new(2, 0) });
            }
			
			if (move == ECharacterMove.Down && !IsMoving)
			{
                IsMoving = true;
                _tween = new Tween(Position, Position + new Vector2(0, 16), 125, new Vector2[] { new(0, 0), new(1, 0), new(0,0) });
            }
        }
    }
}

