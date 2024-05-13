using System;
using System.Collections.Generic;
using FinalFantasyV.Sprites;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FinalFantasyV.Content
{
	public enum ECursor
	{
		InActive,
		Visible,
		Selected
	}

	public class ItemSelector : MenuSelector
	{
		public ItemSelector(int widthItems, int heightItems, int startingX, int startingY, int tileWidth, int tileHeight) : base(widthItems, heightItems, startingX, startingY, tileWidth, tileHeight)
		{
		}

		public override void Draw(SpriteBatch sb, Map parentMenu, SpriteSheet menuSheet)
		{
			var pos = GetXyOfCursor();
			DrawCursor(sb, menuSheet, new Vector2((StartingX + pos.X*TileWidth)*8, StartingY*8 + pos.Y*TileHeight));
		}
	}

	public class CustomSelector : MenuSelector
	{
		private List<Vector2> _cursorPositions;
		private List<int> _numOfPositionsInRow;
		
		public CustomSelector(List<Vector2> cursorPositions, List<int> numOfPositionsInRow)
		{
			_cursorPositions = cursorPositions;
			_numOfPositionsInRow = numOfPositionsInRow;
		}

		public override void Draw(SpriteBatch spriteBatch, Map tileData, SpriteSheet menuSpritesheet)
		{
			DrawCursor(spriteBatch, menuSpritesheet, _cursorPositions[CurrentCursorPosition]);
		}

		public override void MoveCursorDown()
		{
			var (currRow, _) = RowColumn(CurrentCursorPosition);
			CurrentCursorPosition = (CurrentCursorPosition + _numOfPositionsInRow[currRow]) % _cursorPositions.Count;
		}
		
		public override void MoveCursorUp()
		{
			var (currRow, _) = RowColumn(CurrentCursorPosition);
			CurrentCursorPosition = (CurrentCursorPosition - _numOfPositionsInRow[currRow] + _cursorPositions.Count) % _cursorPositions.Count;
		}


		public override void MoveCursorLeft()
		{
			CurrentCursorPosition = (CurrentCursorPosition - 1 + _cursorPositions.Count) % _cursorPositions.Count;
		}

		public override void MoveCursorRight()
		{
			CurrentCursorPosition = (CurrentCursorPosition + 1) % _cursorPositions.Count;
		}

		private (int, int) RowColumn(int index)
		{
			for (int i = 0; i < _numOfPositionsInRow.Count; i++)
			{
				var col = index;
				index -= _numOfPositionsInRow[i];
				if (index < 0) return (i, col);
			}

			return (0,0);
		}

		public override Vector2 GetXyOfCursor()
		{
			var (row, col) = RowColumn(CurrentCursorPosition);
			return new Vector2(col, row);
		}
	}

	public class TextSelector : MenuSelector
	{
		private List<string> _options;
		
		public TextSelector(int widthItems, int heightItems, int startingX, int startingY, int tileWidth, int tileHeight, List<string> options) 
			: base(widthItems, heightItems, startingX, startingY, tileWidth, tileHeight)
		{
			_options = options;
		}
		
		public override void Draw(SpriteBatch sb, Map parentMenu, SpriteSheet menuSheet)
		{
			//Menu.SetBox(parentMenu, startingX, startingY, tileWidth, tileHeight);
			for (int x = 0; x < WidthItems; x++)
			for (int y = 0; y < HeightItems; y++)
			{
				if (x + WidthItems * y < _options.Count)
					Menu.DrawText(parentMenu, StartingX + x * TileWidth, StartingY + y * TileHeight, _options[x + WidthItems * y]);
			}

			DrawCursor(sb, menuSheet, GetPositionOfCursor());
		}
	}
	
	public class MenuSelector
	{
		public ECursor CursorState;

		protected readonly int WidthItems;
		protected readonly int HeightItems;
		protected readonly int StartingX;
		protected readonly int StartingY;
		protected readonly int TileWidth;
		protected readonly int TileHeight;
		protected int CurrentCursorPosition;
		protected int Frames;


		public MenuSelector(){}
		
        public MenuSelector(int widthItems, int heightItems, int startingX, int startingY, int tileWidth, int tileHeight)
        {
            WidthItems = widthItems;
            HeightItems = heightItems;
            StartingX = startingX;
            StartingY = startingY;
            TileWidth = tileWidth;
            TileHeight = tileHeight;
			CursorState = ECursor.InActive;
        }
        
		public int GetCurrIndex() { return CurrentCursorPosition; }

		protected Vector2 GetPositionOfCursor()
		{
			var pos = GetXyOfCursor();
			return new Vector2(8 * ((pos.X*TileWidth)+StartingX-2), 8 * ((pos.Y*TileHeight)+StartingY));
		}

		public virtual Vector2 GetXyOfCursor()
		{
			int x = CurrentCursorPosition % WidthItems;
			int y = CurrentCursorPosition / WidthItems;
            return new Vector2(x,y);
        }

		public void DrawCursor(SpriteBatch sb, SpriteSheet menuSheet, Vector2 pos)
		{
			if (CursorState == ECursor.Visible || (CursorState == ECursor.Selected && Frames % 4 > 1))
				menuSheet.Draw(sb, new Rectangle(0, 132, 16, 12), pos);
		}

		public void SetCursorTo(int x, int y) => CurrentCursorPosition = x + y * WidthItems;
        public void SetCursorTo(Vector2 v) => CurrentCursorPosition = (int)v.X + (int)v.Y * WidthItems;
        public virtual void MoveCursorUp() => CurrentCursorPosition = Math.Max((CurrentCursorPosition - WidthItems), 0);
        public virtual void MoveCursorDown() => CurrentCursorPosition = Math.Min((CurrentCursorPosition + WidthItems), WidthItems*HeightItems);
		public virtual void MoveCursorLeft() => CurrentCursorPosition = Math.Max(0, CurrentCursorPosition-1);
        public virtual void MoveCursorRight() => CurrentCursorPosition = Math.Min(WidthItems*HeightItems, CurrentCursorPosition+1);

        public void Update(GameTime _)
		{
			Frames++;
			if (CursorState != ECursor.Visible) return;
		}

        public virtual void Draw(SpriteBatch spriteBatch, Map tileData, SpriteSheet menuSpritesheet)
        {
	        
        }
        
        
        public virtual void SwapItems(int currIndex, int swappedIndex)
        {
	        
        }
	}
}

