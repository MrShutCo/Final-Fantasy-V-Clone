using System.Collections.Generic;
using Final_Fantasy_V.Models;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Tiled;
using MonoGame.Extended.Tiled.Renderers;

namespace FinalFantasyV
{
	public class Battle
	{
		List<Enemy> enemies;
		List<Character> characters;

		TiledMap tilemap;
		TiledMapRenderer tileRenderer;
        public Battle(List<Enemy> enemies, List<Character> characters)
		{
			this.enemies = enemies;
			this.characters = characters;

		}

		public void Draw(SpriteBatch sb)
		{
			tileRenderer.Draw();
		}
	}
}

