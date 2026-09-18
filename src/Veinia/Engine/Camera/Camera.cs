using Microsoft.Xna.Framework;
using MonoGame.Extended;
using System;
using VeiniaFramework;

namespace Apos.Camera
{
	public class Camera
	{
		public Camera(IVirtualViewport virtualViewport)
		{
			VirtualViewport = virtualViewport;
			shake = new CameraShake();
		}

		public CameraShake shake { get; private set; }

		public float X
		{
			get => _xy.X;
			set
			{
				_xy.X = value;
				_xyz.X = value;
			}
		}
		public float Y
		{
			get => _xy.Y;
			set
			{
				_xy.Y = value;
				_xyz.Y = value;
			}
		}
		public float Z
		{
			get => _xyz.Z;
			set
			{
				_xyz.Z = value;
			}
		}

		public float FocalLength
		{
			get => _focalLength;
			set
			{
				_focalLength = value > 0.01f ? value : 0.01f;
			}
		}

		public float Rotation { get; set; } = 0f;
		public float Scale { get; set; } = 1f; // 2 = twice the size, .5f = half the size

		public Vector2 XY
		{
			get => _xy;
			set
			{
				X = value.X;
				Y = value.Y;
			}
		}
		public Vector3 XYZ
		{
			get => _xyz;
			set
			{
				X = value.X;
				Y = value.Y;
				Z = value.Z;
			}
		}

		public IVirtualViewport VirtualViewport
		{
			get;
			set;
		}

		public void SetViewport()
		{
			VirtualViewport.Set();
		}
		public void ResetViewport()
		{
			VirtualViewport.Reset();
		}

		public Matrix View => GetView(0);
		public Matrix ViewInvert => Matrix.Invert(GetView(0));

		public Matrix GetView(float z = 0, float? scaleFactor = null, float? originScale = null, Vector2? origin = null, bool useCameraScale = true, bool useCameraShake = true, bool useCameraPos = true)
		{
			float scaleZ = ZToScale(_xyz.Z, z);

			var currentScale = scaleFactor ?? 0f;
			var curentOrigin = origin ?? VirtualViewport.Origin;
			var currentOriginScale = originScale ?? 1;

			Matrix pos = useCameraPos ? Matrix.CreateTranslation(new Vector3(-XY, 0f)) : Matrix.Identity;
			Matrix scale = useCameraScale ? Matrix.CreateScale(1f / (Scale + currentScale), 1f / (Scale + currentScale), 1f) : Matrix.Identity;
			Matrix shakes = useCameraShake ? Matrix.CreateTranslation(new Vector3(shake.shakeOffset, 0f)) : Matrix.Identity;

			return VirtualViewport.Transform(
				pos * shakes *
				Matrix.CreateRotationZ(Rotation) *
				Matrix.CreateScale(scaleZ, scaleZ, 1f) * scale *
				Matrix.CreateTranslation(new Vector3(curentOrigin * currentOriginScale, 0f)));
		}
		public Matrix GetView3D()
		{
			return
				Matrix.CreateLookAt(XYZ, new Vector3(XY, Z - 1), new Vector3((float)Math.Sin(Rotation), (float)Math.Cos(Rotation), 0)) *
				Matrix.CreateScale(Scale, -Scale, 1f);
		}
		public Matrix GetProjection()
		{
			return Matrix.CreateOrthographicOffCenter(0, VirtualViewport.Width, VirtualViewport.Height, 0, 0, 1);
		}
		public Matrix GetProjection3D(float nearPlaneDistance = 0.01f, float farPlaneDistance = 100f)
		{
			var aspect = VirtualViewport.VirtualWidth / (float)VirtualViewport.VirtualHeight;
			var fov = (float)Math.Atan(VirtualViewport.VirtualHeight / 2f / FocalLength) * 2f;

			return Matrix.CreatePerspectiveFieldOfView(fov, aspect, nearPlaneDistance, farPlaneDistance);
		}

		public float ScaleToZ(float scale, float targetZ)
		{
			if (scale == 0)
			{
				return float.MaxValue;
			}
			return FocalLength / scale + targetZ;
		}
		public float ZToScale(float z, float targetZ)
		{
			if (z - targetZ == 0)
			{
				return float.MaxValue;
			}
			return FocalLength / (z - targetZ);
		}

		public float WorldToScreenScale(float z = 0f) => Vector2.Distance(WorldToScreen(0f, 0f, z), WorldToScreen(1f, 0f, z));
		public float ScreenToWorldScale(float z = 0f) => Vector2.Distance(ScreenToWorld(0f, 0f, z), ScreenToWorld(1f, 0f, z));

		public Vector2 WorldToScreen(float x, float y, float z = 0f, Matrix? customView = null) => WorldToScreen(new Vector2(x, y), z, customView);
		public Vector2 WorldToScreen(Vector2 xy, float z = 0f, Matrix? customView = null)
		{
			var view = customView ?? GetView(z);
			return Vector2.Transform(xy, view) + VirtualViewport.XY;
		}
		public Vector2 ScreenToWorld(float x, float y, float z = 0f, Matrix? customView = null) => ScreenToWorld(new Vector2(x, y), z, customView);
		public Vector2 ScreenToWorld(Vector2 xy, float z = 0f, Matrix? customView = null)
		{
			var invertedView = customView ?? GetView(z);
			invertedView = Matrix.Invert(invertedView);

			return Vector2.Transform(xy - VirtualViewport.XY, invertedView);
		}

		public bool IsZVisible(float z, float minDistance = 0.1f)
		{
			float scaleZ = ZToScale(Z, z);
			float maxScale = ZToScale(minDistance, 0f);

			return scaleZ > 0 && scaleZ < maxScale;
		}

		public RectangleF ViewRect => GetViewRect(0);
		public RectangleF GetViewRect(float z = 0)
		{
			var frustum = GetBoundingFrustum(z);
			var corners = frustum.GetCorners();
			var a = corners[0];
			var b = corners[1];
			var c = corners[2];
			var d = corners[3];

			var left = Math.Min(Math.Min(a.X, b.X), Math.Min(c.X, d.X));
			var right = Math.Max(Math.Max(a.X, b.X), Math.Max(c.X, d.X));

			var top = Math.Min(Math.Min(a.Y, b.Y), Math.Min(c.Y, d.Y));
			var bottom = Math.Max(Math.Max(a.Y, b.Y), Math.Max(c.Y, d.Y));

			var width = right - left;
			var height = bottom - top;

			return new RectangleF(left, top, width, height);
		}
		public BoundingFrustum GetBoundingFrustum(float z = 0, float? scaleFactor = null)
		{
			var scale = scaleFactor ?? 0f;
			Matrix view = GetView(z, scale);

			Matrix projection = GetProjection();
			return new BoundingFrustum(view * projection);
		}

		public void LinearPosition(Vector2 worldPos, float lerpT = 10)
		{
			SetPosition(Vector2.Lerp(GetPosition(), worldPos, lerpT * Time.deltaTime));
		}

		private Vector2 currentLookahead;
		public Vector2 GetLookahead(Vector2 worldPos, Vector2 lookaheadVelcity, float lookaheadLimit = 5, float lookaheadSensitivity = .5f, float lookaheadLerpT = 1)
		{
			currentLookahead = Vector2.Lerp(currentLookahead, (lookaheadVelcity * lookaheadSensitivity).ClampLength(lookaheadLimit), lookaheadLerpT * Time.deltaTime);
			return worldPos + currentLookahead;
		}
		private float currentScaleLookahead;
		public float GetScaleLookahead(Vector2 lookaheadVelcity, float lookaheadLimit = 5, float lookaheadSensitivity = .5f, float lookaheadLerpT = 1)
		{
			currentScaleLookahead = MathHelper.Lerp(currentScaleLookahead, (lookaheadVelcity * lookaheadSensitivity).ClampLength(lookaheadLimit).Length(), lookaheadLerpT * Time.deltaTime);
			return currentScaleLookahead / lookaheadLimit;
		}
		public void ResetLookaheads()
		{
			currentLookahead = Vector2.Zero;
			currentScaleLookahead = 0;
		}
		public void SetPosition(Vector2 worldPos) => XY = Transform.ToScreenUnits(worldPos);
		public Vector2 GetPosition() => Transform.ToWorldUnits(XY);

		public void SnapToPixel(int pixelsPerUnit) => SetPosition(GetPosition().SnapToIncrements(1 / (float)pixelsPerUnit));

		public Vector2 GetUnitsInView()
		{
			var a = new Vector2(ViewRect.Left, ViewRect.Bottom);
			var b = new Vector2(ViewRect.Right, ViewRect.Top);
			return Transform.ScreenToWorldPos(b - a);
		}

		public Vector2 GetCorner(CornerLocation cornerLocation, Vector2 padding = default, bool useCameraScale = true, bool useCameraShake = false, bool useCameraPos = true)
		{
			Vector2 cornerOffset = Vector2.Zero;

			switch (cornerLocation)
			{
				case CornerLocation.TopLeft:
					cornerOffset = new Vector2(VirtualViewport.TargetWidth / 2 - padding.X, VirtualViewport.TargetHeight / 2 - padding.Y);
					break;
				case CornerLocation.BottomLeft:
					cornerOffset = new Vector2(VirtualViewport.TargetWidth / 2 - padding.X, -VirtualViewport.TargetHeight / 2 + padding.Y);
					break;
				case CornerLocation.TopRight:
					cornerOffset = new Vector2(-VirtualViewport.TargetWidth / 2 + padding.X, VirtualViewport.TargetHeight / 2 - padding.Y);
					break;
				case CornerLocation.BottomRight:
					cornerOffset = new Vector2(-VirtualViewport.TargetWidth / 2 + padding.X, -VirtualViewport.TargetHeight / 2 + padding.Y);
					break;
			}

			var scale = useCameraScale ? Scale : 1;
			if (useCameraShake) cornerOffset += shake.shakeOffset / scale;
			var camPos = useCameraPos ? XY : Vector2.Zero;
			return camPos - cornerOffset * scale;
		}
		public Vector2 GetCornerWorld(CornerLocation cornerLocation, Vector2 padding = default, bool useCameraScale = true, bool useCameraShake = false, bool useCameraPos = true)
			=> Transform.ScreenToWorldPos(GetCorner(cornerLocation, padding * Transform.unitSize, useCameraScale, useCameraShake, useCameraPos));

		private Vector2 _xy = Vector2.Zero;
		private Vector3 _xyz = new Vector3(Vector2.Zero, 1f);
		private float _focalLength = 1f;
	}
}

public enum CornerLocation
{
	TopLeft,
	TopRight,
	BottomLeft,
	BottomRight,
}