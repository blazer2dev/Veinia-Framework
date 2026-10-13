using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using nkast.Aether.Physics2D.Dynamics;
using System.Collections.Generic;
using System.IO;

namespace VeiniaFramework.Samples.Platformer
{
	public class PlatformerPrefabs : PrefabManager
	{
		public override void LoadPrefabs()
		{
			base.LoadPrefabs();

			Add("Player", new GameObject(new Transform { Z = .5f }, new List<Component>
			{
				new Sprite(Path.Combine("Sprites", "Player"), Color.GreenYellow, pixelsPerUnit: 16),
				new Player(),
			}, isStatic: false), prefabTab: 0);


			var collectibles = Globals.content.LoadAll<Texture2D>(Path.Combine("Sprites", "Collectibles"));
			foreach (var collectible in collectibles)
			{
				var prefab = new GameObject(new Transform { Z = .3f }, new List<Component>
				{
					new Sprite(collectible.Value, Color.White, pixelsPerUnit: 16),
					new Collectible(),
					new PhysicsRectangle(.5f, .5f, bodyType: BodyType.Static, isSensor: true)
				}, isStatic: false);

				Add(collectible.Key, prefab, prefabTab: 1);
			}

			var grassTiles = Globals.content.LoadAll<Texture2D>(Path.Combine("Sprites", "Grass"));
			foreach (var grassTile in grassTiles)
			{
				var prefab = new GameObject(new Transform { Z = .6f }, new List<Component>
				{
					new Sprite(grassTile.Value, Color.White, pixelsPerUnit: 16),
					new PhysicsRectangle(1, 1, bodyType: BodyType.Static, category: Category.Cat2)
				}, isStatic: true);
				Add(grassTile.Key, prefab, prefabTab: 2);
			}

			var woodTiles = Globals.content.LoadAll<Texture2D>(Path.Combine("Sprites", "Wood"));
			foreach (var woodTile in woodTiles)
			{
				var prefab = new GameObject(new Transform { Z = .2f }, new List<Component>
				{
					new Sprite(woodTile.Value, Color.White, pixelsPerUnit: 16),
					new PhysicsRectangle(1, 1, bodyType: BodyType.Static, category: Category.Cat2)
				}, isStatic: true);

				Add(woodTile.Key, prefab, prefabTab: 2);
			}

			var bricks = Globals.content.LoadAll<Texture2D>(Path.Combine("Sprites", "Bricks"));
			foreach (var brick in bricks)
			{
				var prefab = new GameObject(new Transform { Z = .1f }, new List<Component>
				{
					new Sprite(brick.Value, Color.White, pixelsPerUnit: 16),
					new PhysicsRectangle(1, 1, bodyType: BodyType.Static, category: Category.Cat2)
				}, isStatic: true);

				Add(brick.Key, prefab, prefabTab: 3);
			}

			var decoration = Globals.content.LoadAll<Texture2D>(Path.Combine("Sprites", "Decoration"));
			foreach (var deco in decoration)
			{
				Add(deco.Key, new GameObject(new Transform { Z = .6f }, new List<Component>
				{
					new Sprite(deco.Value, Color.White, pixelsPerUnit: 16),
				}, isStatic: true), prefabTab: 4);
			}
		}
	}
}
