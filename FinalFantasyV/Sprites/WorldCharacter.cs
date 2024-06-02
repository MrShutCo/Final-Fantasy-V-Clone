﻿using System;
using System.Collections.Generic;
using System.Numerics;
using Engine.RomReader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace FinalFantasyV
{
	public enum ECharacterMove
	{
		Down,Left,Right,Up,Special1,Special2,Special3,Special4
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
			if (_tileSheetSprites != null)
			{
				lerp.X = (int)lerp.X;
				lerp.Y = (int)lerp.Y;
				int tileIndex = (int)(_timeElapsed) / (int)(_timeMilliseconds / (_tileSheetSprites.Length - 1));
				return (lerp, _tileSheetSprites[tileIndex]);
			}

			return (lerp, lerp);
		}

		public bool IsDone(Vector2 pos) => pos == _end;  
    }

	public class WorldCharacter
	{
		protected Random r = new();
		Tween _tween;
		public bool IsMoving { get; set; }
		public bool IsVisible { get; set; }
		bool _isOddStepCount = true;
		bool _isMirrored;

		public const int FastWalkingSpeed = 125;
		public const int NormalWalkingSpeed = 250;
		public const int SlowWalkingSpeed = 500;
		public const int SlowestWalkingSpeed = 1000;

		public event Action DoneMovement;
		protected ECharacterMove Facing;

        public Vector2 Position;
        protected SpriteSheet _character;
		public WorldCharacter(SpriteSheet spriteSheet, Vector2 pos)
		{
			_character = spriteSheet;
			Position = pos;
			IsVisible = true;
		}

		public Vector2 GetTilePosition() => Position / 16;

		public virtual void Update(GameTime gt)
		{
			
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
			switch (move)
			{
				case ECharacterMove.Left:
					_isMirrored = false;
					_character.SetTile(4, 0);
					break;
				case ECharacterMove.Right:
					_isMirrored = true;
					_character.SetTile(4, 0);
					break;
				case ECharacterMove.Up:
					_character.SetTile(2, 0);
					break;
				case ECharacterMove.Down:
					_character.SetTile(0, 0);
					break;
				case ECharacterMove.Special1:
					_character.SetTile(6, 0);
					break;
			}


			Facing = move;
		}

		public Vector2 GetPositionFacing()
		{
			var tilePos = GetTilePosition();
			return Facing switch
			{
				ECharacterMove.Down => tilePos + new Vector2(0, 1),
				ECharacterMove.Left => tilePos + new Vector2(-1, 0),
				ECharacterMove.Right => tilePos + new Vector2(1,0),
				ECharacterMove.Up => tilePos + new Vector2(0, -1),
				_ => throw new ArgumentOutOfRangeException()
			};
		}

		public void Move(ECharacterMove move, int milliseconds)
		{
			if (move == ECharacterMove.Left && !IsMoving)
			{
                IsMoving = true;
                _isMirrored = false;
                _tween = new Tween(Position, Position + new Vector2(-16,0), milliseconds, new Vector2[] {new(4,0), new(5,0), new(4,0) });
            }
			if (move == ECharacterMove.Right && !IsMoving)
			{
                IsMoving = true;
				_isMirrored = true;
                _tween = new Tween(Position, Position + new Vector2(16, 0), milliseconds, new Vector2[] { new(4, 0), new(5, 0), new(4, 0) });
            }
            if (move == ECharacterMove.Up && !IsMoving)
			{
                IsMoving = true;
                _tween = new Tween(Position, Position + new Vector2(0, -16), milliseconds, new Vector2[] { new(2,0), new(3, 0), new(2, 0) });
            }
			
			if (move == ECharacterMove.Down && !IsMoving)
			{
                IsMoving = true;
                _tween = new Tween(Position, Position + new Vector2(0, 16), milliseconds, new Vector2[] { new(0, 0), new(1, 0), new(0,0) });
            }
        }

		public void SetAction(int action)
		{
			if (action == 48)
				SetCharacterSprite(0, 3, true);
			else if (action == 49)
				SetCharacterSprite(1, 3, true);
			else if (action == 50)
				SetCharacterSprite(5, 3, true);
			else if (action == 51)
				SetCharacterSprite(0, 3, false);
			else if (action == 52)
				SetCharacterSprite(1, 3, false);
			else if (action == 53)
				SetCharacterSprite(5, 3, false);
			// Face up (Right Hand)
			else if (action == 54)
				SetCharacterSprite(4,1,false);
			else if (action == 55)
				SetCharacterSprite(5,1, false);
			// Face up (Left Hand)
			else if (action == 56)
				SetCharacterSprite(4,1,true);
			else if (action == 57)
				SetCharacterSprite(5,1,true);
			// Face left Arm
			else if (action == 58)
				SetCharacterSprite(5, 2, false);
			else if (action == 59)
				SetCharacterSprite(4,2, false);
			// Face right arm
			else if (action == 60)
				SetCharacterSprite(5, 2, true);
			else if (action == 61)
				SetCharacterSprite(4,2, true);
			else if (action == 62)
				SetCharacterSprite(0,1, false);
			else if (action == 63)
				SetCharacterSprite(2, 1, false);
			else if (action == 64)
				SetCharacterSprite(1,1, false);
			else if (action == 65)
				SetCharacterSprite(1,1,true);
			else if (action == 66)
				SetCharacterSprite(6,0,false);
			else if (action == 67)
				SetCharacterSprite(6,0,true);
			else if (action == 68)
				SetCharacterSprite(3,2,false);
			else if (action == 69)
				SetCharacterSprite(3,2,true);
			else if (action == 70)
				SetCharacterSprite(0,2,false);
			else if (action == 71)
				SetCharacterSprite(1,2,false);
			else if (action == 72)
				SetCharacterSprite(4,3,false);
			else if (action == 73)
				SetCharacterSprite(3,3,false);
			else if (action == 74)
				SetCharacterSprite(2,3,false);
			//else if (action == 75)
			//	SetCharacter();
			else if (action == 76)
				SetCharacterSprite(2,2,false);
			else if (action == 79)
				SetCharacterSprite(2,2,false);	
			else if (action == 80)
				SetCharacterSprite(2,2, true);
			else
				SetCharacterSprite(0, 0, false);
		}

		public void SetCharacterSprite(int x, int y, bool isMirrored)
		{
			_isMirrored = isMirrored;
			_character.SetTile(x,y);
		}

		public void SetCharacter(SpriteSheet newCharacter)
		{
			var (x, y) = (_character.X, _character.Y);
			_character = newCharacter;
			_character.SetTile(x,y);
		}

		public bool CanWalkHere(ECharacterMove move, List<WorldCharacter> objects, MapManager.Wall[,] walls)
		{
			var tilePos = GetTilePosition();
			if (move == ECharacterMove.Left && walls[(int)tilePos.X - 1, (int)tilePos.Y].PassableRight)
			{
				return true;
			}
			if (move == ECharacterMove.Right && walls[(int)tilePos.X + 1, (int)tilePos.Y].PassableLeft)
			{
				return true;
			}
			if (move == ECharacterMove.Up && walls[(int)tilePos.X, (int)tilePos.Y-1].PassableDown)
			{
				return true;
			}
			if (move == ECharacterMove.Down && walls[(int)tilePos.X, (int)tilePos.Y+1].PassableUp)
			{
				return true;
			}

			return false;
		}
		
		private bool IsObjectHere(int x, int y, List<WorldCharacter> objects)
		{
			foreach (var obj in objects)
			{
				if (obj.Position == new Vector2(x*16, y*16) && obj.IsVisible) return false;
			}

			return true;
		}
    }
}

