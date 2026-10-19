using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace VeiniaFramework
{
	public class AnimationManager : Component
	{
		[CloneOnInstantiate]
		List<AnimData> animDatas;
		[ResetOnInstantiate]
		Sprite sprite;

		[ResetOnInstantiate]
		AnimData anim; // currentAnim
		float time;

		string startAnim;
		int tileX;
		int tileY;

		public float defaultTimeBetweenFrames;
		public float timeBetweenFramesMultiplier = 1;


		public AnimationManager(List<AnimData> animDatas, string startAnim, int tileX = 16, int tileY = 16, float timeBetweenFrames = .1f)
		{
			this.animDatas = animDatas;
			this.startAnim = startAnim;
			this.tileX = tileX;
			this.tileY = tileY;
			this.defaultTimeBetweenFrames = timeBetweenFrames;
		}

		public override void EarlyInitialize()
		{
			sprite = GetComponent<Sprite>();
			if (sprite == null) Say.Line("Animation doesn't have Sprite Component!");

			for (int row = 0; row < sprite.Texture.Height / tileY; row++)
			{
				var anim = animDatas.Find(x => x.atlasRow == row);
				if (anim == null) continue;

				for (int column = 0; column < sprite.Texture.Width / tileX; column++)
				{
					var srcRect = new Rectangle(tileX * column, tileY * row, tileX, tileY);
					anim.sourceRectangles.Add(srcRect);
				}
			}

			Play(startAnim);
		}

		public void Play(string name)
		{
			if (anim != null) // reset currentAnim Data
			{
				if (anim.name == name)
				{
					if (anim.stop) // skip a frame if animation was stopped previously
					{
						anim.frame++;
						if (anim.frame == anim.sourceRectangles.Count) anim.frame = 0;
						sprite.ChangeTexture(sprite.Texture, anim.sourceRectangles[anim.frame]);
					}
					anim.stop = false;
					return;
				}

				anim.frame = 0;
				anim.stop = false;
			}

			anim = GetAnim(name);
			sprite.ChangeTexture(sprite.Texture, anim.sourceRectangles[0]);
			time = 0;
		}

		public void Stop(int? frame = null)
		{
			if (anim != null)
			{
				anim.stop = true;
				if (frame.HasValue) anim.frame = frame.Value;
			}

			sprite.ChangeTexture(sprite.Texture, anim.sourceRectangles[anim.frame]);
			time = 0;
		}

		public override void Update()
		{
			time += Time.deltaTime;

			var timeBetweenFrames = anim.timeBetweenFrames ?? defaultTimeBetweenFrames;
			timeBetweenFrames *= timeBetweenFramesMultiplier;

			if (time > timeBetweenFrames && !anim.stop)
			{
				time = 0;

				if (anim.sourceRectangles.Count == anim.frame + 1) anim.frame = 0;
				else anim.frame++;

				sprite.ChangeTexture(sprite.Texture, anim.sourceRectangles[anim.frame]);
			}
		}

		private AnimData GetAnim(string name) => animDatas.Find(x => x.name == name);
	}

	public class AnimData
	{
		[CloneOnInstantiate]
		public List<Rectangle> sourceRectangles = new List<Rectangle>();
		public string name;
		public bool stop;

		public int atlasRow;
		public int frame = 0;

		public float? timeBetweenFrames;
	}
}
