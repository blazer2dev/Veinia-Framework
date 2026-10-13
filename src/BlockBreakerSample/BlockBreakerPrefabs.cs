using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.IO;

namespace VeiniaFramework.Samples.BlockBreaker
{
	public class BlockBreakerPrefabs : PrefabManager
	{
		public override void LoadPrefabs()
		{
			base.LoadPrefabs();

			Add("Grass Tile", new GameObject(new Transform { Z = .1f }, new List<Component>
			{
				new Sprite(Path.Combine("Sprites","Grass Tile"), Color.White, pixelsPerUnit: 200),
				new GrassTile(),
			}, isStatic: true));

			Add("Moving Tile", new GameObject(new Transform { Z = .1f }, new List<Component>
			{
				new Sprite(Path.Combine("Sprites","Moving Tile"), Color.White, pixelsPerUnit: 200),
				new MovingBlock(),
			}, isStatic: true));

			Add("Metal Tile", new GameObject(new Transform { Z = .1f }, new List<Component>
			{
				new Sprite(Path.Combine("Sprites","Metal Tile"), Color.White, pixelsPerUnit: 200),
				new MetalBlock(),
			}, isStatic: true));

			Add("Background", new GameObject(new Transform { Z = 0 }, new List<Component>
			{
				new Sprite(Path.Combine("Sprites","Background"), Color.White, pixelsPerUnit: 200),
			}, isStatic: true));
		}
	}
}