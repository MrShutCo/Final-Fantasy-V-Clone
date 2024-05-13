using Microsoft.Xna.Framework;

namespace FinalFantasyV
{
	public class Camera
	{
		public Matrix Transform { get; private set; }

		public void Follow(Vector2 position, Vector2 screensize)
		{
			var pos = Matrix.CreateTranslation(-position.X, -position.Y, 0);
			var offset = Matrix.CreateTranslation(screensize.X/2, screensize.Y/2, 0);
			Transform = pos * offset;
		}
	}
}

