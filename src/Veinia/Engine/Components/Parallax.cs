using Apos.Camera;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace VeiniaFramework
{
	public class Parallax : Sprite
	{
		public Parallax(Texture2D texture, Color color, float pixelsPerUnit,
		 float factorX = .5f, float factorY = .5f, int copiesX = 1, int copiesY = 1, Vector2 scrollSpeed = default) : base(texture, color, pixelsPerUnit)
		{
			this.factorX = factorX;
			this.factorY = factorY;
			this.copiesX = copiesX;
			this.copiesY = copiesY;
			this.scrollSpeed = scrollSpeed;
		}

		public Parallax(string path, Color color, float pixelsPerUnit,
		 float factorX = .5f, float factorY = .5f, int copiesX = 1, int copiesY = 1, Vector2 scrollSpeed = default) : base(path, color, pixelsPerUnit)
		{
			this.factorX = factorX;
			this.factorY = factorY;
			this.copiesX = copiesX;
			this.copiesY = copiesY;
			this.scrollSpeed = scrollSpeed;
		}

		float factorX;
		float factorY;
		public int copiesX; // 1 - one sprite copy for left and right
		public int copiesY; // 1 - one sprite copy for up and down

		Vector2 scrollSpeed; // auto scrolling

		Vector2 lastCameraPos;
		Camera cam;
		float textureUnitSize;


		public override void EarlyInitialize()
		{
			cam = Globals.camera;
			lastCameraPos = cam.GetPosition();
			textureUnitSize = rect.Width / Transform.unitSize;
		}

		public override void Update()
		{
			// this script runs in the editor since it inherits Sprite
			if (Veinia.isEditor) return;

			var camPos = cam.GetPosition();

			var deltaMovement = camPos - lastCameraPos;
			transform.position += new Vector2(deltaMovement.X * factorX, deltaMovement.Y * factorY) + (scrollSpeed * Time.deltaTime);

			lastCameraPos = camPos;


			if (MathF.Abs(camPos.X - transform.position.X) >= textureUnitSize)
			{
				var offsetPosX = (camPos.X - transform.position.X) % textureUnitSize;
				transform.position = new Vector2(camPos.X + offsetPosX, transform.position.Y);
			}

			if (MathF.Abs(camPos.Y - transform.position.Y) >= textureUnitSize)
			{
				var offsetPosY = (camPos.Y - transform.position.Y) % textureUnitSize;
				transform.position = new Vector2(transform.position.X, camPos.Y + offsetPosY);
			}
		}

		public override void Draw(SpriteBatch sb)
		{
			for (int x = -copiesX; x <= copiesX; x++)
			{
				for (int y = -copiesY; y <= copiesY; y++)
				{
					Vector2 offset = new Vector2(rect.Width * x, rect.Height * y);
					DrawParallax(sb, offset);
				}
			}
		}

		private void DrawParallax(SpriteBatch sb, Vector2 offset = default)
		{
			level.drawCommands.Add(new DrawCommand
			{
				command = delegate
				{
					sb.Draw(Texture, rect.OffsetNew(offset), null, color, MathHelper.ToRadians(transform.rotation),
						 new Vector2(Texture.Bounds.Width / 2, Texture.Bounds.Height / 2),
						 SpriteEffects.None, layerDepth: 0);
				},
				Z = transform.Z,
				drawOptions = drawOptions
			});
		}
	}
}
