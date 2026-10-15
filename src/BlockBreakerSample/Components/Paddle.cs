using Microsoft.Xna.Framework;
using nkast.Aether.Physics2D.Dynamics;

namespace VeiniaFramework.Samples.BlockBreaker
{
	public class Paddle : Component
	{
		private const float yOffset = -4.5f;
		private const float xLimit = 8;

		Ball ball;
		Sprite sprite;

		public override void Initialize()
		{
			sprite = GetComponent<Sprite>();
			ball = FindComponentOfType<Ball>();
			body = Globals.physicsWorld.CreateRectangle(2.5f, .85f, 1, bodyType: BodyType.Kinematic);
		}

		public override void Update()
		{
			var mouseControls = new Vector2(Globals.input.GetMouseWorldPosition().X, yOffset);
			transform.position = new Vector2(MathHelper.Clamp(mouseControls.X, -xLimit, xLimit), mouseControls.Y);

			if (!ball.launched)
			{
				ball.transform.position = transform.position + Vector2.UnitY;
				FlashPaddle(1, Color.White);
			}
		}

		public void FlashPaddle(float duration, Color color)
		{
			var s = sprite.drawOptions.shader;
			s.Parameters["FlashColor"].SetValue(color.ToVector4());

			Timers.New("paddleFlash", duration, onUpdate: (t) =>
			{
				s.Parameters["FlashAmount"].SetValue(t);
			});
		}
	}
}