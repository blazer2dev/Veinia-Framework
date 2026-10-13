using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Tweening;

namespace VeiniaFramework.Samples.BlockBreaker
{
	public class Transition : Component
	{
		private Tween tween;
		Action action;
		Texture2D texture;

		public Transition(Action onTransition, string path = null)
		{
			this.action = onTransition;
			this.texture = Globals.content.Load<Texture2D>(path ?? Path.Combine("veinia_defaults", "circle"));
		}

		public override void Initialize()
		{
			var anim = level.Instantiate(new GameObject(new Transform { Z = 1000 }, new List<Component>
			{
				new Sprite(texture, Color.Black, 100)
			}));


			anim.transform.scale = Vector2.Zero;
			anim.dontDestroyOnLoad = true;

			tween = Globals.unscaledTweener.TweenTo(target: anim.transform, expression: transform => transform.scale, toValue: Globals.camera.GetUnitsInView(), duration: .5f)
			.Easing(EasingFunctions.CircleOut)
			.OnEnd((x) =>
			{
				action?.Invoke();

				tween = Globals.unscaledTweener.TweenTo(target: anim.transform, expression: transform => transform.scale, toValue: Vector2.Zero, duration: .5f, delay: .1f)
				.Easing(EasingFunctions.SineIn)
				.OnEnd((x) => { anim.DestroyGameObject(); });
			});
		}
	}
}
