using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Particles;
using MonoGame.Extended.Particles.Modifiers;
using MonoGame.Extended.Particles.Modifiers.Interpolators;
using MonoGame.Extended.Particles.Profiles;
using MonoGame.Extended.TextureAtlases;
using MonoGame.Extended.Tweening;
using nkast.Aether.Physics2D.Dynamics;
using System;
using System.Collections.Generic;

namespace VeiniaFramework.Samples.BlockBreaker
{
	public class Tile : Component
	{
		protected bool hasBeenDestroyed;

		ParticleData particles;


		public override void Initialize()
		{
			body = Globals.physicsWorld.CreateRectangle(1, 1, 1, bodyType: BodyType.Kinematic);
			body.Tag = this;

			var texture = new Texture2D(Globals.graphicsDevice, 1, 1);
			texture.SetData(new[] { Color.White });

			particles = Globals.particleWorld.Add(new ParticleEffect
			{
				Position = transform.screenPos,
				Emitters = new List<ParticleEmitter>
				{
					new ParticleEmitter(new TextureRegion2D(texture), 500, TimeSpan.FromSeconds(.6f),
						Profile.BoxFill(100, 100))
					{
						AutoTrigger=false,
						Parameters = new ParticleReleaseParameters
						{
							Speed = new Range<float>(150, 300),
							Rotation = new Range<float>(-1f, 1f),
							Quantity = 20,
						},
						Modifiers =
						{
							new AgeModifier
							{
								Interpolators =
								{
									new ColorInterpolator { StartValue = new HslColor(0.33f, 0.5f, 0.5f), EndValue = new HslColor(0.5f, 0.9f, 1.0f) },
									new OpacityInterpolator{ StartValue = 1, EndValue = 0 },
									new ScaleInterpolator { StartValue = new Vector2(50,50), EndValue = Vector2.Zero },
									new HueInterpolator { StartValue = 0f, EndValue = 40 }
								}
							},
							new RotationModifier {RotationRate = -2.1f},
							new LinearGravityModifier {Direction = -Vector2.UnitY, Strength = 130},
						}
					}
				},
			}, Z: 1f); ;
		}

		public virtual void Hit()
		{
			NextFrame.actions.Add(RemovePhysics);
			void RemovePhysics() => Globals.physicsWorld.Remove(body);

			if (hasBeenDestroyed) return;
			hasBeenDestroyed = true;

			particles.effect.Trigger(transform.screenPos, 1);

			Globals.tweener.TweenTo(target: transform, expression: transform => transform.rotation, toValue: -10, duration: .2f)
				.Easing(EasingFunctions.BackIn);

			Globals.tweener.TweenTo(target: transform, expression: transform => transform.scale, toValue: Vector2.Zero, duration: .3f)
				.Easing(EasingFunctions.BackIn)
				.OnEnd((x) =>
				{
					DestroyGameObject();
					Globals.particleWorld.QueueRemove(particles.effect);

					if (FindComponentsOfType<Tile>(allowNone: true).Count == 0)
						FindComponentOfType<UI>().ResetGameWithTransition();
				});
		}
	}
}