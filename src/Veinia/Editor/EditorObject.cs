using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using System.ComponentModel;

namespace VeiniaFramework.Editor
{
	public class EditorObject
	{
		[JsonProperty("n")] public string PrefabName { get; set; }

		[JsonProperty("p")]
		public Vector2 Position
		{
			get { return position; }
			set
			{
				position = value;
				if (EditorPlacedSprite != null) EditorPlacedSprite.transform.position = value;
			}
		}
		Vector2 position;

		[JsonProperty("z", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public float Z
		{
			get { return z; }
			set
			{
				z = value;
				if (EditorPlacedSprite != null) EditorPlacedSprite.transform.Z = value;
			}
		}
		float z;

		[JsonProperty("r", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public float Rotation
		{
			get { return rotation; }
			set
			{
				rotation = value;
				if (EditorPlacedSprite != null) EditorPlacedSprite.transform.rotation = value;
			}
		}
		float rotation;

		[JsonProperty("s", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public Vector2 Scale
		{
			get { return scale; }
			set
			{
				scale = value;
				if (EditorPlacedSprite != null) EditorPlacedSprite.transform.scale = scale;
			}
		}
		Vector2 scale = Vector2.One;
		public bool ShouldSerializeScale() => scale != Vector2.One;

		[JsonProperty("d", DefaultValueHandling = DefaultValueHandling.Ignore)] public string customData;

		[JsonConverter(typeof(ColorToHexJsonConverter))]
		[JsonProperty("c", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public Color? Color
		{
			get => color;
			set
			{
				color = value;

				if (EditorPlacedSprite != null && value.HasValue) EditorPlacedSprite.color = value.Value;
			}
		}
		private Color? color;

		public bool ShouldSerializeColor() => color.HasValue && color.Value != Microsoft.Xna.Framework.Color.White;

		[Browsable(false)][JsonIgnore] public Sprite EditorPlacedSprite { get; set; }

		[Browsable(false)][JsonIgnore] public IDrawGizmos gizmo { get; set; }
	}
}
