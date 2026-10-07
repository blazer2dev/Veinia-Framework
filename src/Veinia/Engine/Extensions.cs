using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using Myra.Graphics2D.UI;
using Myra.Graphics2D.UI.Properties;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;

namespace VeiniaFramework
{
	public static class Extensions
	{
		public static List<T> Clone<T>(this List<T> listToClone) where T : ICloneable
		{
			return listToClone.Select(item => (T)item.Clone()).ToList();
		}

		public static void AddAllTo<T>(this List<T> list, List<T> destination)
		{
			foreach (var item in list)
			{
				destination.Add(item);
			}
		}
		public static Rectangle Scale(this Rectangle rect, Vector2? scaleMultiplier = default, Vector2? scaleIncrement = default)
		{
			var multi = scaleMultiplier ?? Vector2.One;
			var increment = scaleIncrement ?? Vector2.Zero;

			return new Rectangle(rect.X, rect.Y, (int)(rect.Width * multi.X + increment.X), (int)(rect.Height * multi.Y + increment.Y));
		}
		public static Rectangle OffsetNew(this Rectangle rect, Vector2 offset)
		{
			return new Rectangle(rect.X + (int)offset.X, rect.Y + (int)offset.Y, rect.Width, rect.Height);
		}
		public static Rectangle OffsetByHalf(this Rectangle rect)
		{
			rect.Offset(-rect.Width / 2, -rect.Height / 2);
			return rect;
		}
		public static Rectangle AllowNegativeSize(this Rectangle rect)
		{
			if (rect.Width < 0)
			{
				rect.X += rect.Width;
				rect.Width = Math.Abs(rect.Width);
			}
			if (rect.Height < 0)
			{
				rect.Y += rect.Height;
				rect.Height = Math.Abs(rect.Height);
			}
			return rect;
		}
		public static GameObject ExtractComponentToNewGameObject<T1>(this GameObject gameObject, Transform transform, bool isStatic = false) where T1 : Component
		{
			return new GameObject(transform, new List<Component>
			{
				(T1)gameObject.GetComponent<T1>().Clone()
			}, isStatic: isStatic);
		}
		public static Vector2 SafeNormalize(this Vector2 value)
		{
			// if the vector WOULD be normalized when its zero we would get NaN
			// so we need to only normalize the vector when its not Vector2.Zero
			if (value != Vector2.Zero)
			{
				value.Normalize();
			}

			return value;
		}
		public static Vector2 SnapToIncrements(this Vector2 value, float increments)
		{
			if (increments <= 0f)
				return value;

			return new Vector2(
				MathF.Round(value.X / increments) * increments,
				MathF.Round(value.Y / increments) * increments
			);
		}
		public static Vector2 ClampLength(this Vector2 value, float maxMagnitude)
		{
			if (value.LengthSquared() > maxMagnitude * maxMagnitude)
			{
				return value.SafeNormalize() * maxMagnitude;
			}
			return value;
		}
		public static Vector2 RotateAroundOrigin(this Vector2 vector, Vector2 origin, float rotation)
		{
			var rot = MathHelper.ToRadians(-rotation);
			float cos = MathF.Cos(rot);
			float sin = MathF.Sin(rot);
			Vector2 translated = vector - origin;

			float rotatedX = translated.X * cos - translated.Y * sin;
			float rotatedY = translated.X * sin + translated.Y * cos;

			return new Vector2(rotatedX, rotatedY) + origin;
		}
		public static Vector2 ReplaceY(this Vector2 a, float newY) => new Vector2(a.X, newY);
		public static Vector2 AddToY(this Vector2 a, float addY) => new Vector2(a.X, a.Y + addY);
		public static Vector2 GetWithoutY(this Vector2 a) => new Vector2(a.X, 0);
		public static Vector2 GetWithoutX(this Vector2 a) => new Vector2(0, a.Y);
		public static Vector3 ToVector3(this Vector2 a) => new Vector3(a.X, a.Y, 0);
		public static Vector3 ToVector3(this Vector2 a, float z) => new Vector3(a.X, a.Y, z);
		public static Vector2 ToVector2(this Vector3 a) => new Vector2(a.X, a.Y);
		public static Texture2D ChangeColor(this Texture2D texture, Color newColor, bool ignoreWhite = true)
		{
			if (newColor == Color.White && ignoreWhite) return texture;

			var colorData = new Color[texture.Width * texture.Height];
			texture.GetData(colorData);

			for (int i = 0; i < colorData.Length; i++)
			{
				var newColorHsl = newColor.ToHsl();
				var oldColorHsl = colorData[i].ToHsl();

				if (oldColorHsl.L < newColorHsl.L)
					colorData[i] = new HslColor(newColorHsl.H, newColorHsl.S, oldColorHsl.L).ToRgb();
				else
					colorData[i] = new HslColor(newColorHsl.H, newColorHsl.S, newColorHsl.L).ToRgb();
			}

			var temp = new Texture2D(texture.GraphicsDevice, texture.Width, texture.Height);
			temp.SetData(colorData);

			return temp;
		}
		public static float Clamp01(this float value)
		{
			return MathHelper.Clamp(value, 0f, 1f);
		}
		public static float NextFloat(this Random rng, float minValue, float maxValue)
		{
			return (float)(rng.NextDouble() * (maxValue - minValue) + minValue);
		}
		public static Color ToNegative(this Color color)
		{
			color = new Color(255 - color.R, 255 - color.G, 255 - color.B);
			return color;
		}
		public static Color Alpha01(this Color color, float alpha01)
		{
			byte alpha = (byte)(MathHelper.Clamp(alpha01, 0f, 1f) * 255f);
			return new Color(color.R, color.G, color.B, alpha);
		}
		public static Vector2 Round(this Vector2 vector, int decimalDigits)
		{
			return new Vector2((float)Math.Round(vector.X, decimalDigits), (float)Math.Round(vector.Y, decimalDigits));
		}
		public static Vector2 FlipY(this Vector2 vector)
		{
			return new Vector2(vector.X, -vector.Y);
		}

		/// <summary>
		/// Loads all assets from a content folder (WASM not supported)
		/// </summary>
		[UnsupportedOSPlatform("browser")]
		public static Dictionary<string, T1> LoadAll<T1>(this ContentManager content, string contentFolder)
		{
			DirectoryInfo dir = new DirectoryInfo(content.RootDirectory + "/" + contentFolder);

			if (!dir.Exists)
				throw new DirectoryNotFoundException($"LoadAll({dir}) doesn't exist. Check spelling and make sure the content folder is in the project directory.");

			Dictionary<string, T1> result = new();

			FileInfo[] files = dir.GetFiles("*.*");
			foreach (FileInfo file in files)
			{
				string key = Path.GetFileNameWithoutExtension(file.Name);
				result[key] = content.Load<T1>(contentFolder + "/" + key);
			}

			return result;
		}
		/// <summary>
		/// Loads specified assets from a content folder (WASM supported)
		/// </summary>
		public static Dictionary<string, T> LoadAssets<T>(this ContentManager content, string contentFolder, string[] assets)
		{
			Dictionary<string, T> result = new();

			foreach (string name in assets)
			{
				result[name] = content.Load<T>($"{contentFolder}/{name}");
			}

			return result;
		}
		public static void LoadFromTextureAtlas(this ContentManager content, string atlas, int tileX = 16, int tileY = 16, Action<string, Texture2D, Rectangle> init = null, bool ignoreTransparent = true)
		{
			var tex = content.Load<Texture2D>(atlas);
			int count = 0;

			var pixels = new Color[tex.Width * tex.Height];
			tex.GetData(pixels);

			for (int y = 0; y < tex.Height / tileY; y++)
			{
				for (int x = 0; x < tex.Width / tileX; x++)
				{
					var srcRect = new Rectangle(tileX * x, tileY * y, tileX, tileY);

					if (tex.IsTransparent(pixels, srcRect) && ignoreTransparent) continue;

					count++;
					init?.Invoke($"{atlas}:{count}", tex, srcRect);
				}
			}
		}
		public static bool IsTransparent(this Texture2D tex, Color[] pixels = null, Rectangle? sourceRectangle = null)
		{
			Color[] pixelData = pixels;

			bool isTransparent = true;

			if (pixelData == null)
			{
				pixelData = new Color[tex.Width * tex.Height];
				tex.GetData(pixelData);
			}

			var bounds = sourceRectangle ?? tex.Bounds;

			for (int y = bounds.Y; y < bounds.Y + bounds.Height; y++)
			{
				for (int x = bounds.X; x < bounds.X + bounds.Width; x++)
				{
					var p = pixelData[y * tex.Width + x];

					if (p.A != 0)
						isTransparent = false;
				}
			}
			return isTransparent;
		}
		public static Vector2 GetCenter(this Texture2D texture)
		{
			return new Vector2(texture.Width / 2f, texture.Height / 2f);
		}
		public static Vector2 GetCenter(this Rectangle rect)
		{
			return new Vector2(rect.Width / 2f, rect.Height / 2f);
		}
		public static Vector2 GetCenter(this Rectangle? rect)
		{
			return new Vector2(rect.Value.Width / 2f, rect.Value.Height / 2f);
		}

		public static Window MakeEditWindow(this Desktop myraDesktop, object Object, string title = "Object Editor", int width = 350, bool pauseGame = false, bool allowInReleaseMode = false)
		{
#if !DEBUG
			if (!allowInReleaseMode)
				return null;
#endif

			var propertyGrid = new PropertyGrid
			{
				Object = Object,
				Width = width
			};

			var editWindow = new Window
			{
				Title = title,
				Content = propertyGrid,
			};

			editWindow.Closed += (s, e) =>
			{
				if (pauseGame) Time.stop = false;
			};

			editWindow.ArrangeUpdated += (s, e) =>
			{
				if (pauseGame) Time.stop = true;
			};

			editWindow.Show(myraDesktop);

			return editWindow;
		}

		public static Window MakeEditWindowOnKeyPress(this Desktop myraDesktop, object Object, Keys key, string title = "Object Editor", int width = 350, bool pauseGame = false, bool allowInReleaseMode = false)
		{
			if (Globals.input.GetKeyDown(key))
			{
				return MakeEditWindow(myraDesktop, Object, title, width, pauseGame, allowInReleaseMode);
			}
			return null;
		}
	}
}