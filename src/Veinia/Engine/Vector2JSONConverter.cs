using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Globalization;

public class Vector2JSONConverter : JsonConverter
{
	public override bool CanConvert(Type objectType) => objectType == typeof(Vector2) || objectType == typeof(Vector2?);

	public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
	{
		if (value == null)
		{
			writer.WriteNull();
			return;
		}

		Vector2 vector = (Vector2)value;

		writer.WriteStartObject();

		writer.WritePropertyName("x");
		writer.WriteValue(vector.X);

		writer.WritePropertyName("y");
		writer.WriteValue(vector.Y);

		writer.WriteEndObject();
	}

	public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		if (reader.TokenType == JsonToken.Null) return null;

		if (reader.TokenType == JsonToken.StartObject)
		{
			JObject obj = JObject.Load(reader);

			return new Vector2(obj["x"]!.Value<float>(), obj["y"]!.Value<float>());
		}

		if (reader.TokenType == JsonToken.String)
		{
			string s = ((string)reader.Value).Replace(",", " ").Trim();

			string[] parts = s.Split(' ', StringSplitOptions.RemoveEmptyEntries);

			if (parts.Length == 2) return new Vector2(float.Parse(parts[0], CultureInfo.InvariantCulture), float.Parse(parts[1], CultureInfo.InvariantCulture));

			if (parts.Length == 1)
			{
				float v = float.Parse(parts[0], CultureInfo.InvariantCulture);
				return new Vector2(v);
			}
		}

		throw new InvalidOperationException("Invalid Vector2 - Can't read");
	}
}