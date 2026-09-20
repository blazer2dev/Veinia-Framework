using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace VeiniaFramework
{
	public class Sprite : Component, IDrawn
	{
		public Color color = Color.White;
		public Vector2 DestinationSize { get; private set; }
		public Rectangle? SourceRectangle { get; private set; }
		public Effect effect;
		public DrawOptions drawOptions;
		public Texture2D Texture { get; private set; }

		private float pixelsPerUnit;


		public Sprite(Texture2D texture, Color? color = null, float? pixelsPerUnit = null, Rectangle? sourceRectangle = null, DrawOptions options = default)
		{
			this.color = color ?? Color.White;
			this.pixelsPerUnit = pixelsPerUnit ?? Transform.unitSize;
			drawOptions = options;

			ChangeTexture(texture, sourceRectangle);
		}
		public Sprite(string path, Color? color = null, float? pixelsPerUnit = null, Rectangle? sourceRectangle = null, DrawOptions options = default)
			: this(Globals.content.Load<Texture2D>(path), color, pixelsPerUnit, sourceRectangle, options)
		{
		}

		public virtual void Draw(SpriteBatch sb)
		{
			level.drawCommands.Add(new DrawCommand
			{
				command = delegate
				{
					var flipSprite = (transform.scale.X < 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None) | (transform.scale.Y < 0 ? SpriteEffects.FlipVertically : SpriteEffects.None);
					sb.Draw(Texture, rect, SourceRectangle, color,
						MathHelper.ToRadians(transform.rotation), origin: SourceRectangle.GetCenter(),
						flipSprite, layerDepth: 0);
				},
				Z = transform.Z,
				drawOptions = drawOptions
			});
		}

		public void ChangeTexture(string path, Rectangle? sourceRectangle = null) => ChangeTexture(Globals.content.Load<Texture2D>(path), sourceRectangle);
		public void ChangeTexture(Texture2D texture, Rectangle? sourceRectangle = null)
		{
			this.Texture = texture;
			this.SourceRectangle = sourceRectangle ?? texture.Bounds;
			DestinationSize = new Vector2(this.SourceRectangle.Value.Width, this.SourceRectangle.Value.Height) / (this.pixelsPerUnit / Transform.unitSize);
		}


		public Rectangle rect => new Rectangle((int)transform.screenPos.X, (int)transform.screenPos.Y,
											   (int)(DestinationSize.X * MathF.Abs(transform.scale.X)),
											   (int)(DestinationSize.Y * MathF.Abs(transform.scale.Y)));
	}
}