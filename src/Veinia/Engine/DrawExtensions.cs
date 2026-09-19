using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;

namespace VeiniaFramework
{
	public static class DrawExtensions
	{
		public static void VeiniaPoint(this SpriteBatch sb, Level level, Vector2 position, Color? color = null, float size = 10, DrawOptions drawOptions = default, float z = float.MaxValue)
		{
			color = color ?? Color.White;

			level.drawCommands.Add(new DrawCommand
			{
				command = delegate
				{
					sb.DrawPoint(position, color.Value, size, 0);
				},
				Z = z,
				drawOptions = drawOptions,
			});
		}
		public static void VeiniaPointWorld(this SpriteBatch sb, Level level, Vector2 position, Color? color = null, float size = 10, DrawOptions drawOptions = default, float z = float.MaxValue)
			=> VeiniaPoint(sb, level, Transform.WorldToScreenPos(position), color, size, drawOptions, z);

		public static void VeiniaText(this SpriteBatch sb, Level level, Vector2 position, string text, Color? color = null, float size = 32, SpriteFontBase font = null, Vector2 alignment = default, TextStyle textStyle = TextStyle.None, FontSystemEffect fontSystemEffect = FontSystemEffect.None, int effectAmount = 0, DrawOptions drawOptions = default, float z = float.MaxValue)
		{
			color = color ?? Color.White;
			if (text == null || text == string.Empty) return;

			level.drawCommands.Add(new DrawCommand
			{
				command = delegate
				{
					var spriteFontBase = font ?? Globals.fontSystem.GetFont(size);
					var textSize = spriteFontBase.MeasureString(text);

					var origin = new Vector2(
						textSize.X * (alignment.X + 1f) / 2f,
						textSize.Y * (alignment.Y + 1f) / 2f
					);

					sb.DrawString(spriteFontBase, text, position, color.Value, 0f, origin, textStyle: textStyle, effect: fontSystemEffect, effectAmount: effectAmount);
				},
				Z = z,
				drawOptions = drawOptions,
			});
		}
		public static void VeiniaTextWorld(this SpriteBatch sb, Level level, Vector2 position, string text, Color? color = null, float size = 32, SpriteFontBase font = null, Vector2 alignment = default, TextStyle textStyle = TextStyle.None, FontSystemEffect fontSystemEffect = FontSystemEffect.None, int effectAmount = 0, DrawOptions drawOptions = default, float z = float.MaxValue)
			=> VeiniaText(sb, level, Transform.WorldToScreenPos(position), text, color, size, font, alignment, textStyle, fontSystemEffect, effectAmount, drawOptions, z);

		public static void VeiniaLine(this SpriteBatch sb, Level level, Vector2 point1, Vector2 point2, Color? color = null, float thickness = 10, DrawOptions drawOptions = default, float z = float.MaxValue)
		{
			color = color ?? Color.White;

			level.drawCommands.Add(new DrawCommand
			{
				command = delegate
				{
					sb.DrawLine(point1, point2, color.Value, thickness);
				},
				Z = z,
				drawOptions = drawOptions,
			});
		}
		public static void VeiniaLineWorld(this SpriteBatch sb, Level level, Vector2 point1, Vector2 point2, Color? color = null, float thickness = 10, DrawOptions drawOptions = default, float z = float.MaxValue)
			=> VeiniaLine(sb, level, Transform.WorldToScreenPos(point1), Transform.WorldToScreenPos(point2), color, thickness, drawOptions, z);

		public static void VeiniaCircle(this SpriteBatch sb, Level level, Vector2 position, Color? color = null, float radius = 1, int sides = 10, float thickness = 1, DrawOptions drawOptions = default, float z = float.MaxValue)
		{
			color = color ?? Color.White;

			level.drawCommands.Add(new DrawCommand
			{
				command = delegate
				{
					sb.DrawCircle(new CircleF(position.ToPoint(), radius / 2), sides, color.Value, thickness);
				},
				Z = z,
				drawOptions = drawOptions,
			});
		}
		public static void VeiniaCircleWorld(this SpriteBatch sb, Level level, Vector2 position, Color? color = null, float radius = 1, int sides = 10, float thickness = 1, DrawOptions drawOptions = default, float z = float.MaxValue)
			=> VeiniaCircle(sb, level, Transform.WorldToScreenPos(position), color, Transform.ToScreenUnits(radius), sides, thickness, drawOptions, z);

		public static void VeiniaRectangle(this SpriteBatch sb, Level level, RectangleF rectangle, Color? color = null, float thickness = 1, DrawOptions drawOptions = default, float z = float.MaxValue)
		{
			color = color ?? Color.White;

			level.drawCommands.Add(new DrawCommand
			{
				command = delegate
				{
					sb.DrawRectangle(rectangle, color.Value, thickness);
				},
				Z = z,
				drawOptions = drawOptions,
			});
		}

		public static void VeiniaRectangleRotated(this SpriteBatch sb, Level level, RectangleF rectangle, Color? color = null, float thickness = 1, float rotation = 0, DrawOptions drawOptions = default, float z = float.MaxValue)
		{
			color = color ?? Color.White;

			Vector2 center = new Vector2(rectangle.X + rectangle.Width / 2f, rectangle.Y + rectangle.Height / 2f);

			Vector2 topLeft = new Vector2(rectangle.Left, rectangle.Top);
			Vector2 topRight = new Vector2(rectangle.Right, rectangle.Top);
			Vector2 bottomRight = new Vector2(rectangle.Right, rectangle.Bottom);
			Vector2 bottomLeft = new Vector2(rectangle.Left, rectangle.Bottom);

			topLeft = topLeft.RotateAroundOrigin(center, -rotation);
			topRight = topRight.RotateAroundOrigin(center, -rotation);
			bottomRight = bottomRight.RotateAroundOrigin(center, -rotation);
			bottomLeft = bottomLeft.RotateAroundOrigin(center, -rotation);

			level.drawCommands.Add(new DrawCommand
			{
				command = delegate
				{
					sb.DrawLine(topLeft.X, topLeft.Y, topRight.X, topRight.Y, color.Value, thickness);
					sb.DrawLine(topRight.X, topRight.Y, bottomRight.X, bottomRight.Y, color.Value, thickness);
					sb.DrawLine(bottomRight.X, bottomRight.Y, bottomLeft.X, bottomLeft.Y, color.Value, thickness);
					sb.DrawLine(bottomLeft.X, bottomLeft.Y, topLeft.X, topLeft.Y, color.Value, thickness);
				},
				Z = z,
				drawOptions = drawOptions
			});
		}
	}
}