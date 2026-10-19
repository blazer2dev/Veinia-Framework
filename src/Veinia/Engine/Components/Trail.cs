using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;

namespace VeiniaFramework
{
	public class Trail : Component, IDrawn, IDisposable
	{
		[ResetOnInstantiate] private DynamicVertexBuffer _vBuffer;
		[ResetOnInstantiate] private IndexBuffer _iBuffer;
		[ResetOnInstantiate] private TrailVertex[] _vertices;
		[ResetOnInstantiate] private TrailPoint[] _points;
		[ResetOnInstantiate] private float[] _pointDistances;
		[ResetOnInstantiate] private int _pointCount;
		[ResetOnInstantiate] private Vector3 _emissionPosition;
		[ResetOnInstantiate] private int _segments;

		private const float DefaultMinVertexDistance = .2f;
		private int _initialSegments;
		private float _length;
		private float _segmentLength;
		private float _width;
		private float? _lifetime;
		private float? _fadeOffLength;
		private float _z;
		private Effect _effect;
		private BasicEffect _basicEffect;
		private bool _setWorldViewProjection;
		private bool _usesDefaultEffect;
		private Vector4 _globalColor;

		private struct TrailPoint
		{
			public Vector3 Position;
			public float Age;
		}

		/// <summary>
		/// The amount of time points remain visible. Null disables time-based
		/// expiration and uses the distance-limited history instead.
		/// </summary>
		public float? Lifetime
		{
			get => _lifetime;
			set
			{
				if (value.HasValue && value.Value <= 0)
					throw new ArgumentOutOfRangeException(nameof(value), "Trail lifetime must be greater than zero.");

				_lifetime = value;
				if (_points == null)
					return;

				RemoveExpiredPoints();
				RebuildVertices();
			}
		}

		public float? FadeOffLength
		{
			get => _fadeOffLength.HasValue ? _fadeOffLength.Value / Transform.unitSize : null;
			set
			{
				if (value.HasValue && value.Value <= 0)
					throw new ArgumentOutOfRangeException(nameof(value), "Fade-off length must be greater than zero.");

				_fadeOffLength = value.HasValue ? value.Value * Transform.unitSize : null;
				if (_points != null)
					RebuildVertices();
			}
		}

		public float Length => _length / Transform.unitSize;
		public float SegmentLength => _segmentLength / Transform.unitSize;
		public float MinVertexDistance => SegmentLength;
		public float Width => _width * 2f / Transform.unitSize;

		public Trail(float length = 4f, float width = .4f, Color? color = null, Effect effect = null, float z = 0, bool setWorldViewProjection = false, float? lifetime = null, float? fadeOffLength = null)
		{
			if (length <= 0)
				throw new ArgumentOutOfRangeException(nameof(length), "Trail length must be greater than zero.");
			if (width < 0)
				throw new ArgumentOutOfRangeException(nameof(width), "Trail width cannot be negative.");
			if (lifetime.HasValue && lifetime.Value <= 0)
				throw new ArgumentOutOfRangeException(nameof(lifetime), "Trail lifetime must be greater than zero.");
			if (fadeOffLength.HasValue && fadeOffLength.Value <= 0)
				throw new ArgumentOutOfRangeException(nameof(fadeOffLength), "Fade-off length must be greater than zero.");

			_length = length * Transform.unitSize;
			_segmentLength = DefaultMinVertexDistance * Transform.unitSize;
			_initialSegments = Math.Max(2, (int)Math.Ceiling(length / DefaultMinVertexDistance) + 1);
			_width = width * Transform.unitSize / 2f;
			_lifetime = lifetime;
			_fadeOffLength = fadeOffLength.HasValue ? fadeOffLength.Value * Transform.unitSize : null;
			_z = z;
			_setWorldViewProjection = setWorldViewProjection;
			_basicEffect = Globals.shapeDrawing.basicEffect;

			if (effect == null)
			{
				_effect = Globals.content.Load<Effect>(Path.Combine("veinia_defaults", "trail_default"));
				_globalColor = (color ?? Color.White).ToVector4();
				_effect.Parameters["GlobalColor"]?.SetValue(_globalColor);
				_setWorldViewProjection = true;
				_usesDefaultEffect = true;
			}
			else
			{
				_effect = effect;
				_globalColor = Color.White.ToVector4();
				_usesDefaultEffect = false;
			}
		}

		public override void Initialize()
		{
			_segments = _initialSegments;
			_vBuffer = new DynamicVertexBuffer(Globals.graphicsDevice, TrailVertex.VertexDeclaration, _segments * 2, BufferUsage.WriteOnly);
			_iBuffer = new IndexBuffer(Globals.graphicsDevice, IndexElementSize.SixteenBits, (_segments - 1) * 6, BufferUsage.WriteOnly);

			_vertices = new TrailVertex[_segments * 2];
			_points = new TrailPoint[_segments];
			_pointDistances = new float[_segments];
			_pointCount = 0;
			Vector3 initialPosition = GetCurrentPosition();
			_emissionPosition = initialPosition;

			FillIndexBuffer();
			AppendPoint(initialPosition);
			RebuildVertices();
		}

		private void FillIndexBuffer()
		{
			var indices = new short[(_segments - 1) * 6];
			for (int i = 0; i < _segments - 1; i++)
			{
				int vertex = i * 2;
				indices[i * 6 + 0] = (short)vertex;
				indices[i * 6 + 1] = (short)(vertex + 1);
				indices[i * 6 + 2] = (short)(vertex + 2);
				indices[i * 6 + 3] = (short)(vertex + 1);
				indices[i * 6 + 4] = (short)(vertex + 3);
				indices[i * 6 + 5] = (short)(vertex + 2);
			}

			_iBuffer.SetData(indices);
		}

		public override void Update()
		{
			if (_points == null || _vertices == null)
				return;

			float deltaTime = Math.Max(0f, Time.deltaTime);
			for (int i = 0; i < _pointCount; i++)
				_points[i].Age += deltaTime;

			RemoveExpiredPoints();

			Vector3 currentPosition = GetCurrentPosition();
			if (_pointCount == 0)
			{
				_emissionPosition = currentPosition;
				AppendPoint(currentPosition);
			}
			else
			{
				Vector3 offset = currentPosition - _emissionPosition;
				if (offset.LengthSquared() >= _segmentLength * _segmentLength)
				{
					AppendPoint(currentPosition);
					_emissionPosition = currentPosition;
				}
				else
				{
					_points[_pointCount - 1].Position = currentPosition;
				}
			}

			RebuildVertices();
		}

		private Vector3 GetCurrentPosition() => transform.screenPos.ToVector3();

		private void AppendPoint(Vector3 position)
		{
			if (_pointCount == _segments && !_lifetime.HasValue)
			{
				// With no lifetime, keep a fixed distance history like the original
				// trail and fade the points that are about to leave it.
				Array.Copy(_points, 1, _points, 0, _segments - 1);
				_pointCount--;
			}
			else
			{
				EnsureCapacity(_pointCount + 1);
			}

			_points[_pointCount++] = new TrailPoint
			{
				Position = position,
				Age = 0f,
			};
		}

		private void EnsureCapacity(int requiredPointCount)
		{
			if (requiredPointCount <= _segments)
				return;

			int newSegments = Math.Max(requiredPointCount, _segments * 2);
			if (newSegments > 32768)
				throw new InvalidOperationException("Trail exceeded the maximum point capacity for a 16-bit index buffer.");

			var oldVertexBuffer = _vBuffer;
			var oldIndexBuffer = _iBuffer;

			_segments = newSegments;
			Array.Resize(ref _points, _segments);
			Array.Resize(ref _pointDistances, _segments);
			Array.Resize(ref _vertices, _segments * 2);

			_vBuffer = new DynamicVertexBuffer(Globals.graphicsDevice, TrailVertex.VertexDeclaration, _segments * 2, BufferUsage.WriteOnly);
			_iBuffer = new IndexBuffer(Globals.graphicsDevice, IndexElementSize.SixteenBits, (_segments - 1) * 6, BufferUsage.WriteOnly);
			FillIndexBuffer();

			oldVertexBuffer?.Dispose();
			oldIndexBuffer?.Dispose();
		}

		private void RemoveExpiredPoints()
		{
			if (!_lifetime.HasValue)
				return;

			int firstLivePoint = 0;
			while (firstLivePoint < _pointCount && _points[firstLivePoint].Age >= _lifetime.Value)
				firstLivePoint++;

			if (firstLivePoint == 0)
				return;

			int livePointCount = _pointCount - firstLivePoint;
			if (livePointCount > 0)
				Array.Copy(_points, firstLivePoint, _points, 0, livePointCount);
			_pointCount = livePointCount;
		}

		private void RebuildVertices()
		{
			float trailDistance = 0f;
			if (_pointCount > 0)
				_pointDistances[0] = 0f;

			for (int i = 1; i < _pointCount; i++)
			{
				trailDistance += Vector3.Distance(_points[i - 1].Position, _points[i].Position);
				_pointDistances[i] = trailDistance;
			}

			float fadeDistance = _fadeOffLength ?? trailDistance;
			for (int i = 0; i < _pointCount; i++)
			{
				Vector3 tangent = GetTangent(i);
				Vector3 normal = Vector3.Cross(Vector3.UnitZ, tangent);
				if (normal.LengthSquared() < 0.000001f)
					normal = Vector3.UnitY;
				else
				normal.Normalize();

				float u = _pointCount == 1 ? 1f : (float)i / (_pointCount - 1);
				float distanceVisibility = fadeDistance > 0.000001f
					? MathHelper.Clamp(_pointDistances[i] / fadeDistance, 0f, 1f)
					: 1f;
				float lifetimeVisibility = _lifetime.HasValue
					? MathHelper.Clamp(1f - _points[i].Age / _lifetime.Value, 0f, 1f)
					: 1f;

				if (_fadeOffLength.HasValue)
					// Fade lifetime influence toward full opacity at the end of the
					// fade-off region so there is no jump to 100% at its boundary.
					lifetimeVisibility = MathHelper.Lerp(lifetimeVisibility, 1f, distanceVisibility);

				float visibility = lifetimeVisibility * distanceVisibility;

				Vector3 center = _points[i].Position;

				_vertices[i * 2] = new TrailVertex
				{
					Position = center + normal * _width,
					TextureCoordinate = new Vector2(u, 0f),
					Visibility = visibility,
				};
				_vertices[i * 2 + 1] = new TrailVertex
				{
					Position = center - normal * _width,
					TextureCoordinate = new Vector2(u, 1f),
					Visibility = visibility,
				};
			}
		}

		private Vector3 GetTangent(int pointIndex)
		{
			Vector3 tangent;
			if (_pointCount < 2)
				tangent = Vector3.UnitX;
			else if (pointIndex == 0)
				tangent = _points[1].Position - _points[0].Position;
			else if (pointIndex == _pointCount - 1)
				tangent = _points[pointIndex].Position - _points[pointIndex - 1].Position;
			else
				tangent = _points[pointIndex + 1].Position - _points[pointIndex - 1].Position;

			if (tangent.LengthSquared() < 0.000001f)
			{
				for (int i = pointIndex - 1; i >= 0; i--)
				{
					tangent = _points[i + 1].Position - _points[i].Position;
					if (tangent.LengthSquared() >= 0.000001f)
						break;
				}
			}

			if (tangent.LengthSquared() < 0.000001f)
				tangent = Vector3.UnitX;
			else
				tangent.Normalize();

			return tangent;
		}

		public void Dispose()
		{
			_vBuffer?.Dispose();
			_iBuffer?.Dispose();
			_pointCount = 0;
			_vBuffer = null;
			_iBuffer = null;
		}

		public void Draw(SpriteBatch sb)
		{
			if (_pointCount < 2 || level == null || _vBuffer == null || _iBuffer == null)
				return;

			level.drawCommands.Add(new DrawCommand
			{
				command = delegate
				{
					if (_pointCount < 2 || _vBuffer == null || _iBuffer == null)
						return;

					if (_usesDefaultEffect)
						_effect.Parameters["GlobalColor"]?.SetValue(_globalColor);

					if (_setWorldViewProjection)
						_effect.Parameters["WorldViewProjection"]?.SetValue(_basicEffect.View * _basicEffect.Projection);

					_vBuffer.SetData(_vertices, 0, _pointCount * 2, SetDataOptions.Discard);
					Globals.graphicsDevice.SetVertexBuffer(_vBuffer);
					Globals.graphicsDevice.Indices = _iBuffer;

					int primitiveCount = (_pointCount - 1) * 2;
					foreach (var pass in _effect.CurrentTechnique.Passes)
					{
						pass.Apply();
						Globals.graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, primitiveCount);
					}
				},
				drawWithoutSpriteBatch = true,
				Z = _z,
				drawOptions = new DrawOptions
				{
					shader = _effect,
					blendState = BlendState.NonPremultiplied,
					rasterizerState = RasterizerState.CullNone,
				},
			});
		}
	}

	public struct TrailVertex
	{
		public Vector3 Position;
		public Vector2 TextureCoordinate;
		// Normalized opacity. The trail shader consumes this value directly.
		public float Visibility;

		public static readonly VertexDeclaration VertexDeclaration = new VertexDeclaration(new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0), new VertexElement(12, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0), new VertexElement(20, VertexElementFormat.Single, VertexElementUsage.TextureCoordinate, 1));
	}
}
