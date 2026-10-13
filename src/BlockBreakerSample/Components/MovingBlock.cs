using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Tweening;
using System;
using VeiniaFramework.Editor;

namespace VeiniaFramework.Samples.BlockBreaker
{
	public class MovingBlock : Tile, IDrawGizmos
	{
		Tween tween;


		public override void Initialize()
		{
			base.Initialize();

			var yOffset = 1f;

			Random random = new Random();

			transform.position = new Vector2(transform.position.X, transform.position.Y - yOffset / 2);

			//random between 1f & 2f
			var duration = (float)random.Next(100, 200) / 100;
			tween = Globals.tweener.TweenTo(target: transform, expression: transform => transform.position,
											toValue: new Vector2(transform.position.X, transform.position.Y + yOffset), duration)
			.Easing(EasingFunctions.BackInOut)
			.AutoReverse()
			.RepeatForever();
		}

		public override void Update()
		{
			base.Update();

			if (hasBeenDestroyed) tween.Cancel();
		}

		public void DrawGizmos(SpriteBatch sb, Level editorScene, EditorObject editorObject, bool isSelected)
		{
			var gizmoTop = editorObject.EditorPlacedSprite.rect.OffsetByHalf();
			gizmoTop.Y += 100;
			sb.VeiniaRectangle(editorScene, gizmoTop, Color.Blue, thickness: 5);

			var gizmoBottom = editorObject.EditorPlacedSprite.rect.OffsetByHalf();
			gizmoBottom.Y -= 100;
			sb.VeiniaRectangle(editorScene, gizmoBottom, Color.Blue, thickness: 5);
		}
	}
}
