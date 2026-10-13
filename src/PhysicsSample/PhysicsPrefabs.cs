using Microsoft.Xna.Framework;
using nkast.Aether.Physics2D.Dynamics;
using System.Collections.Generic;
using System.IO;

namespace VeiniaFramework.Samples.Physics
{
	public class PhysicsPrefabs : PrefabManager
	{
		public override void LoadPrefabs()
		{
			base.LoadPrefabs();

			var prefab = new GameObject(new Transform { Z = 0 }, new List<Component>
			{
				new Sprite(Path.Combine("Sprites","Square"), Color.White, pixelsPerUnit: 200),
				new PhysicsRectangle(friction: 2, bodyType: BodyType.Dynamic)
			}, isStatic: false);

			Add("Block", prefab);
		}
	}
}