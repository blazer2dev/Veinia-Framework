using System;
using System.Collections.Generic;
using FontStashSharp;
using FontStashSharp.RichText;
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


		public static void VeiniaText(this SpriteBatch sb, Level level, Vector2 position, string text, Color? color = null, float size = 32, SpriteFontBase font = null, Vector2 alignment = default, bool useRichText = false, TextStyle textStyle = TextStyle.None, FontSystemEffect fontSystemEffect = FontSystemEffect.None, int effectAmount = 0, DrawOptions drawOptions = default, float z = float.MaxValue)
		{
			color = color ?? Color.White;
			if (string.IsNullOrEmpty(text)) return;

			level.drawCommands.Add(new DrawCommand
			{
				command = delegate
				{
					var spriteFontBase = font ?? Globals.fontSystem.GetFont(size);

					if (useRichText)
					{
						var richText = new RichTextLayout
						{
							Font = spriteFontBase,
							Text = text
						};

						var textSize = richText.Size;

						var origin = new Vector2(
							textSize.X * (alignment.X + 1f) / 2f,
							textSize.Y * (alignment.Y + 1f) / 2f
						);

						richText.Draw(sb, position - origin, color.Value);
					}
					else
					{
						var textSize = spriteFontBase.MeasureString(text);

						var origin = new Vector2(
							textSize.X * (alignment.X + 1f) / 2f,
							textSize.Y * (alignment.Y + 1f) / 2f
						);

						sb.DrawString(spriteFontBase, text, position, color.Value, 0f, origin, textStyle: textStyle, effect: fontSystemEffect, effectAmount: effectAmount);
					}
				},
				Z = z,
				drawOptions = drawOptions,
			});
		}

		public static void VeiniaTextWorld(this SpriteBatch sb, Level level, Vector2 position, string text, Color? color = null, float size = 32, SpriteFontBase font = null, Vector2 alignment = default, bool useRichText = false, TextStyle textStyle = TextStyle.None, FontSystemEffect fontSystemEffect = FontSystemEffect.None, int effectAmount = 0, DrawOptions drawOptions = default, float z = float.MaxValue)
			=> VeiniaText(sb, level, Transform.WorldToScreenPos(position), text, color, size, font, alignment, useRichText, textStyle, fontSystemEffect, effectAmount, drawOptions, z);


		public static void VeiniaTextFit(this SpriteBatch sb, Level level, Vector2 position, string text, Vector2 boxSize,
			Color? color = null, float maxSize = 32f, float minSize = 8f, Vector2 alignment = default,
			Vector2 padding = default, bool useRichText = false, TextStyle textStyle = TextStyle.None, FontSystemEffect fontSystemEffect = FontSystemEffect.None,
			int effectAmount = 0, DrawOptions drawOptions = default, float z = float.MaxValue)
		{
			if (string.IsNullOrWhiteSpace(text)) return;

			Vector2 innerSize = boxSize - padding * 2f;

			List<string> Wrap(DynamicSpriteFont font)
			{
				var lines = new List<string>();

				foreach (string paragraph in text.Replace("\r", "").Split('\n'))
				{
					string line = "";

					foreach (string word in paragraph.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries))
					{
						string candidate = line.Length == 0 ? word : line + " " + word;

						if (font.MeasureString(candidate).X <= innerSize.X)
						{
							line = candidate;
							continue;
						}

						if (line.Length > 0) lines.Add(line);
						line = "";

						foreach (char c in word)
						{
							string next = line + c;

							if (line.Length > 0 && font.MeasureString(next).X > innerSize.X)
							{
								lines.Add(line);
								line = c.ToString();
							}
							else
							{
								line = next;
							}
						}
					}

					if (line.Length > 0) lines.Add(line);
					else if (paragraph.Length == 0) lines.Add("");
				}

				return lines;
			}

			float low = minSize;
			float high = maxSize;
			float best = minSize;

			for (int i = 0; i < 14; i++)
			{
				float size = (low + high) / 2f;
				var font = Globals.fontSystem.GetFont(size);
				var lines = Wrap(font);
				float lineHeight = font.MeasureString("Mg").Y * 1.1f;

				if (lines.Count * lineHeight <= innerSize.Y)
				{
					best = size;
					low = size;
				}
				else
				{
					high = size;
				}
			}

			var finalFont = Globals.fontSystem.GetFont(best);
			var finalLines = Wrap(finalFont);
			float finalLineHeight = finalFont.MeasureString("Mg").Y * 1.1f;
			float blockHeight = finalLines.Count * finalLineHeight;

			float boxLeft = position.X - boxSize.X / 2f;
			float boxTop = position.Y - boxSize.Y / 2f;

			float innerLeft = boxLeft + padding.X;
			float innerTop = boxTop + padding.Y;

			float startY = innerTop + (innerSize.Y - blockHeight) * (1f - alignment.Y) / 2f;

			for (int i = 0; i < finalLines.Count; i++)
			{
				float lineWidth = finalFont.MeasureString(finalLines[i]).X;

				float lineX = innerLeft + (innerSize.X - lineWidth) * (1f - alignment.X) / 2f;

				sb.VeiniaText(level, new Vector2(lineX, startY + i * finalLineHeight), finalLines[i],
					color, best, font: finalFont, alignment: new Vector2(-1, -1), useRichText,
					textStyle: textStyle, fontSystemEffect: fontSystemEffect,
					effectAmount: effectAmount, drawOptions: drawOptions, z: z);
			}
		}

		public static void VeiniaTextFitWorld(this SpriteBatch sb, Level level, Vector2 position, string text, Vector2 boxSize,
			Color? color = null, float maxSize = 32f, float minSize = 8f, Vector2 alignment = default, Vector2 padding = default, bool useRichText = false,
			TextStyle textStyle = TextStyle.None, FontSystemEffect fontSystemEffect = FontSystemEffect.None,
			int effectAmount = 0, DrawOptions drawOptions = default, float z = float.MaxValue)
			=> VeiniaTextFit(sb, level, Transform.WorldToScreenPos(position), text, boxSize, color, maxSize, minSize,
				alignment, padding * Transform.unitSize, useRichText, textStyle, fontSystemEffect, effectAmount, drawOptions, z);


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