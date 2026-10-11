using Microsoft.Xna.Framework;
using MonoGame.Extended;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace VeiniaFramework
{
	public class Transform : Component
	{
		//pixels per unit is assigned in the veinia constructor
		public static int unitSize = 100;

		public static Transform Empty => new Transform(0, 0);

		[Browsable(false)] public Transform Parent { get; private set; }
		public List<Transform> Children { get; private set; } = new List<Transform>();

		bool posDirty;
		public Vector2 position
		{
			get
			{
				if (Parent == null)
				{
					return localPosition;
				}

				Vector2 rotatedLocal = localPosition.RotateAroundOrigin(Vector2.Zero, Parent.rotation);
				Vector2 worldPos = Parent.position + rotatedLocal * Parent.scale;

				return worldPos;
			}
			set
			{
				posDirty = true;
				if (Parent == null)
				{
					localPosition = value;
					return;
				}

				Vector2 offset = value - Parent.position / Parent.scale;
				Vector2 unrotated = offset.RotateAroundOrigin(Vector2.Zero, -Parent.rotation);
				localPosition = unrotated;
			}
		}
		[Browsable(false)] public Vector2 localPosition { get; private set; }


		bool rotDirty;
		public float rotation
		{
			get
			{
				float parentRot = (Parent != null) ? Parent.rotation : 0f;
				return localRotation + parentRot;
			}
			set
			{
				rotDirty = true;
				float parentRot = (Parent != null) ? Parent.rotation : 0f;
				localRotation = value - parentRot;
			}
		}
		[Browsable(false)] public float localRotation { get; private set; }


		public Vector2 scale
		{
			get
			{
				if (Parent == null) return localScale;
				return Parent.scale * localScale;
			}
			set
			{
				if (Parent == null) localScale = value;
				else localScale = value / Parent.scale;
			}
		}
		[Browsable(false)] public Vector2 localScale { get; private set; } = Vector2.One;

		public float Z
		{
			get
			{
				if (Parent == null) return localZ;
				return localZ + Parent.Z;
			}

			set
			{
				if (Parent == null) localZ = value;
				else localZ = value + Parent.Z;
			}
		}
		[Browsable(false)] public float localZ { get; private set; }

		public override void PreLateUpdate() // synchronise bodies 
		{
			if (body != null && Parent != null)
			{
				body.Position = position;
				body.Rotation = -MathHelper.ToRadians(rotation);
			}
			else if (body != null && Parent == null)
			{
				position = posDirty ? transform.position : body.Position;
				rotation = rotDirty ? transform.rotation : MathHelper.ToDegrees(-body.Rotation);
				body.Rotation = -MathHelper.ToRadians(rotation);
				body.Position = transform.position;

				posDirty = false;
				rotDirty = false;
			}
		}

		public Vector2 screenPos => Transform.WorldToScreenPos(position);
		public Vector2 up => new Vector2((float)MathF.Cos(MathHelper.ToRadians(rotation - 90)), -(float)MathF.Sin(MathHelper.ToRadians(rotation - 90)));
		public Vector2 right => new Vector2((float)MathF.Cos(MathHelper.ToRadians(rotation)), -(float)MathF.Sin(MathHelper.ToRadians(rotation)));

		public Transform(float worldX, float worldY, float scaleX, float scaleY)
		{
			position = new Vector2(worldX, worldY);
			scale = new Vector2(scaleX, scaleY);
		}
		public Transform(float worldX, float worldY)
		{
			position = new Vector2(worldX, worldY);
		}
		public Transform(Vector2 position, Vector2 scale)
		{
			this.position = position;
			this.scale = scale;
		}
		public Transform(Vector2 position)
		{
			this.position = position;
		}
		public Transform() { }

		public static Vector2 WorldToScreenPos(Vector2 world)
		{
			var unflippedScreen = world * unitSize;
			return new Vector2(unflippedScreen.X, -unflippedScreen.Y);
		}
		public static Vector2 WorldToScreenPos(float x, float y)
		{
			var unflippedScreen = new Vector2(x, y) * unitSize;
			return new Vector2(unflippedScreen.X, -unflippedScreen.Y);
		}
		public static Vector2 ScreenToWorldPos(Vector2 screen)
		{
			var unflippedScreen = screen / unitSize;
			return new Vector2(unflippedScreen.X, -unflippedScreen.Y);
		}
		public static Vector2 ScreenToWorldPos(float x, float y)
		{
			var unflippedScreen = new Vector2(x, y) / unitSize;
			return new Vector2(unflippedScreen.X, -unflippedScreen.Y);
		}
		public static Vector2 ToScreenUnits(Vector2 world) => world.SetY(world.Y * -1) * unitSize;
		public static Vector2 ToWorldUnits(Vector2 screen) => screen.SetY(screen.Y * -1) / unitSize;
		public static float ToScreenUnits(float world) => world * unitSize;
		public static float ToWorldUnits(float screen) => screen / unitSize;

		public void LookAt(Vector2 worldPos)
		{
			var distanceX = worldPos.X - transform.position.X;
			var distanceY = worldPos.Y - transform.position.Y;

			var result = -(float)Math.Atan2(distanceY, distanceX);
			transform.rotation = MathHelper.ToDegrees(result) + 90;
		}

		public void RotateAround(Vector2 origin, float rotation)
		{
			transform.rotation += rotation;
			transform.position = transform.position.RotateAroundOrigin(origin, rotation);
		}

		public void SetParent(Transform parent, bool worldPositionStays = true)
		{
			if (this.Parent != null) RemoveParent();

			if (worldPositionStays)
			{
				localRotation = rotation - parent.rotation;
				localPosition = (position - parent.position).RotateAroundOrigin(Vector2.Zero, -parent.rotation) / parent.scale;
				localScale = scale / parent.scale;
				localZ = Z - parent.Z;
			}

			this.Parent = parent;
			parent.Children.Add(this);
		}

		public void RemoveParent()
		{
			if (Parent == null) throw new Exception("RemoveParent() - No Parent Found!");

			Parent.Children.Remove(this);

			localRotation = rotation;
			localPosition = position;
			localScale = scale;
			localZ = Z;

			Parent = null;
		}
	}
}