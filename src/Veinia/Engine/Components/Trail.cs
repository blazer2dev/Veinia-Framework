using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;

namespace VeiniaFramework
{
	public class Trail : Component, IDrawn
	{
		//our buffers
		private DynamicVertexBuffer _vBuffer;
		private IndexBuffer _iBuffer;

		private TrailVertex[] vertices;

		//How many segments are already initialized?
		private int _segmentsUsed = 0;

		//How many segments does our strip have?
		private int _segments;

		//How long is each segment in world units?
		private float _segmentLength;

		//The world coordinates of our last static segment end
		private Vector3 _lastSegmentPosition;

		//If we fade out – over how many segments?
		private int fadeOutSegments = 4;

		private float _width = 1;

		private float _z = 0;

		private float _minLength = 0.01f;

		private Effect _effect;
		private BasicEffect basicEffect;

		private bool _setWorldViewProjection;

		public Trail(float segmentLength = .2f, int segments = 20, float width = .4f, Color? color = null, Effect effect = null, float z = 0, bool setWorldViewProjection = false)
		{
			_segmentLength = segmentLength * Transform.unitSize;
			_segments = segments;
			_width = width * Transform.unitSize / 2;
			_setWorldViewProjection = setWorldViewProjection;
			_z = z;

			basicEffect = Globals.shapeDrawing.basicEffect;

			if (effect == null)
			{
				_effect = Globals.content.Load<Effect>(Path.Combine("veinia_defaults", "trail_default"));
				_effect.Parameters["GlobalColor"].SetValue(color == null ? new Vector4(1, 1, 1, 1) : color.Value.ToVector4());

				_setWorldViewProjection = true;
			}
		}

		public override object Clone()
		{
			var clone = (Trail)base.Clone();
			clone._vBuffer = null;
			clone._iBuffer = null;
			clone.vertices = null;
			clone._segmentsUsed = 0;
			clone._lastSegmentPosition = Vector3.Zero;
			return clone;
		}

		public override void Initialize()
		{
			// _lastSegmentPosition = Globals.input.GetMouseScreenPosition().ToVector3();

			_vBuffer = new DynamicVertexBuffer(Globals.graphicsDevice, TrailVertex.VertexDeclaration, _segments * 2, BufferUsage.None);
			_iBuffer = new IndexBuffer(Globals.graphicsDevice, IndexElementSize.SixteenBits, (_segments - 1) * 6, BufferUsage.WriteOnly);

			vertices = new TrailVertex[_segments * 2];

			Update(); // make sure to update before draw
			FillIndexBuffer();
		}

		private void FillIndexBuffer()
		{
			short[] bufferArray = new short[(_segments - 1) * 6];
			for (var i = 0; i < _segments - 1; i++)
			{
				bufferArray[0 + i * 6] = (short)(0 + i * 2);
				bufferArray[1 + i * 6] = (short)(1 + i * 2);
				bufferArray[2 + i * 6] = (short)(2 + i * 2);
				bufferArray[3 + i * 6] = (short)(1 + i * 2);
				bufferArray[4 + i * 6] = (short)(3 + i * 2);
				bufferArray[5 + i * 6] = (short)(2 + i * 2);
			}

			_iBuffer.SetData(bufferArray);
		}

		public void Dispose()
		{
			_vBuffer?.Dispose();
			_iBuffer?.Dispose();
		}

		public override void Update()
		{
			Vector3 newPosition = transform.screenPos.ToVector3();
			float visibility = 1;

			//Initialize the first segment, we have no indication for the direction, so just displace the 2 vertices to the left/right
			if (_segmentsUsed == 0)
			{
				vertices[0].Position = _lastSegmentPosition + Vector3.Left;
				vertices[0].TextureCoordinate = new Vector2(0, 0);

				vertices[1].Position = _lastSegmentPosition + Vector3.Right;
				vertices[1].TextureCoordinate = new Vector2(0, 1);

				_segmentsUsed = 1;
			}

			Vector3 directionVector = newPosition - _lastSegmentPosition;
			float directionLength = directionVector.Length();

			//If the distance between our newPosition and our last segment is greater than our assigned
			// _segmentLength we have to delete the oldest segment and make a new one at the other end
			if (directionLength > _segmentLength)
			{
				Vector3 normalizedVector = directionVector / directionLength;

				//normal to the direction. In our case the trail always faces the sky so we can use the cross product
				//with (0,0,1)
				Vector3 normalVector = Vector3.Cross(Vector3.UnitZ, normalizedVector);

				//how many segments are we in?
				int currentSegment = _segmentsUsed;

				//if we are already at max #segments we need to delete the last one
				if (currentSegment >= _segments - 1)
				{
					ShiftDownSegments();
				}

				else
				{
					_segmentsUsed++;
				}

				//Update our latest segment with the new position
				vertices[currentSegment * 2].Position = newPosition + normalVector * _width;
				vertices[currentSegment * 2].TextureCoordinate = new Vector2(1, 0);
				vertices[currentSegment * 2 + 1].Position = newPosition - normalVector * _width;
				vertices[currentSegment * 2 + 1].TextureCoordinate = new Vector2(1, 1);

				//Fade out
				//We can’t have more fadeout segments than initialized segments!
				int max_fade_out_segments = Math.Min(fadeOutSegments, currentSegment);

				for (var i = 0; i < max_fade_out_segments; i++)
				{
					//Linear function y = 1/max * x – percent. Need to check with prior visibility, might be lower (if car jumps for example)
					float visibilityTerm = Math.Min(1.0f / max_fade_out_segments * i, DecodeVisibility(vertices[i * 2].Visibility));
					visibilityTerm = EncodeVisibility(visibilityTerm);

					vertices[i * 2].Visibility = visibilityTerm;
					vertices[i * 2 + 1].Visibility = visibilityTerm;
				}

				//Our last segment’s position is the current position now. Go on from there
				_lastSegmentPosition = newPosition;
			}
			//If we are not further than a segment’s length but further than the minimum distance to change something
			//(We don’t wantto recalculate everything when our target didn’t move from the last segment)
			//Alternatively we can save the last position where we calculated stuff and have a minimum distance from that, too.
			else if (directionLength > _minLength)
			{
				Vector3 normalizedVector = directionVector / directionLength;

				Vector3 normalVector = Vector3.Cross(Vector3.UnitZ, normalizedVector);

				int currentSegment = _segmentsUsed;

				vertices[currentSegment * 2].Position = newPosition + normalVector * _width;
				vertices[currentSegment * 2].TextureCoordinate = new Vector2(1, 0);
				vertices[currentSegment * 2].Visibility = EncodeVisibility(visibility);
				vertices[currentSegment * 2 + 1].Position = newPosition - normalVector * _width;
				vertices[currentSegment * 2 + 1].TextureCoordinate = new Vector2(1, 1);
				vertices[currentSegment * 2 + 1].Visibility = EncodeVisibility(visibility);

				//We have to adjust the orientation of the last vertices too, so we can have smooth curves!
				if (currentSegment >= 2)
				{
					Vector3 directionVectorOld = vertices[(currentSegment - 1) * 2].Position - vertices[(currentSegment - 2) * 2].Position;

					directionVectorOld.Normalize();
					Vector3 normalVectorOld = Vector3.Cross(Vector3.UnitZ, directionVectorOld);

					normalVectorOld = normalVectorOld + (1 - MathHelper.Clamp(Vector3.Dot(normalVectorOld, normalVector), 0, 1)) * normalVector;

					normalVectorOld.Normalize();

					vertices[(currentSegment - 1) * 2].Position = _lastSegmentPosition + normalVectorOld * _width;
					vertices[(currentSegment - 1) * 2 + 1].Position = _lastSegmentPosition - normalVectorOld * _width;
				}

				// Visibility

				//Fade out the trail to the back
				int max_fade_out_segments = Math.Min(fadeOutSegments, currentSegment);

				//Get the percentage of advance towards the next _segmentLength when we need to change vertices again
				float percent = directionLength / _segmentLength / max_fade_out_segments;

				for (var i = 0; i < max_fade_out_segments; i++)
				{
					//Linear function y = 1/max * x – percent. Need to check with prior visibility, might be lower (if car jumps for example)
					float visibilityTerm = Math.Min(1.0f / max_fade_out_segments * i - percent, DecodeVisibility(vertices[i * 2].Visibility));
					visibilityTerm = EncodeVisibility(visibilityTerm);

					vertices[i * 2].Visibility = visibilityTerm;
					vertices[i * 2 + 1].Visibility = visibilityTerm;
				}

			}
		}

		private float EncodeVisibility(float visibility) => (visibility + 1) / 3.0f;
		private float DecodeVisibility(float visibility) => (visibility * 3) - 1.0f;
		private void ShiftDownSegments()
		{
			for (var i = 0; i < _segments - 1; i++)
			{
				vertices[i * 2] = vertices[i * 2 + 2];
				vertices[i * 2 + 1] = vertices[i * 2 + 3];
			}
		}

		public void Draw(SpriteBatch sb)
		{
			level.drawCommands.Add(new DrawCommand
			{
				command = delegate
				{
					if (_setWorldViewProjection) _effect.Parameters["WorldViewProjection"].SetValue(basicEffect.View * basicEffect.Projection);

					_vBuffer.SetData(vertices);
					Globals.graphicsDevice.SetVertexBuffer(_vBuffer);
					Globals.graphicsDevice.Indices = _iBuffer;

					foreach (var pass in _effect.CurrentTechnique.Passes)
					{
						pass.Apply();
						Globals.graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, _segmentsUsed * 2);
					}
				},
				drawWithoutSpriteBatch = true,
				Z = _z,
			});
		}
	}


	public struct TrailVertex
	{
		// Stores the starting position of the particle.
		public Vector3 Position;

		// Stores TexCoords
		public Vector2 TextureCoordinate;

		// Visibility term
		public float Visibility;
		public static readonly VertexDeclaration VertexDeclaration = new VertexDeclaration(
			new VertexElement(0, VertexElementFormat.Vector3,
			VertexElementUsage.Position, 0),
			new VertexElement(12, VertexElementFormat.Vector2,
			VertexElementUsage.TextureCoordinate, 0),
			new VertexElement(20, VertexElementFormat.Single,
			VertexElementUsage.TextureCoordinate, 0));
	}
}
