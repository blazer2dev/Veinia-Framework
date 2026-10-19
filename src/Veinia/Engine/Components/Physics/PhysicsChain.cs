using Microsoft.Xna.Framework;
using nkast.Aether.Physics2D.Collision.Shapes;
using nkast.Aether.Physics2D.Common;
using nkast.Aether.Physics2D.Dynamics;

namespace VeiniaFramework
{
	public class PhysicsChain : PhysicsShape
	{
		public Vector2[] vertices
		{
			get { return verts; }
			set
			{
				verts = value;

				if (body != null)
				{
					body.Remove(fixture);
					MakeShape();
				}
			}
		}
		[CloneOnInstantiate]
		private Vector2[] verts;

		float friction;
		float restitution;
		bool isSensor;
		Category category;

		[ResetOnInstantiate]
		private Shape shape;

		[ResetOnInstantiate]
		private Fixture fixture;

		public PhysicsChain(Vector2[] vertices, float restitution = 0, float friction = .2f, bool isSensor = false, Category category = Category.None, BodyType bodyType = BodyType.Static, Vector2 offset = default, object tag = null, bool ignoreGravity = false, bool sleepingAllowed = true)
			: base(bodyType, offset, tag, ignoreGravity, sleepingAllowed)
		{
			this.verts = vertices;
			this.restitution = restitution;
			this.friction = friction;
			this.isSensor = isSensor;
			this.category = category;
		}

		protected override void MakeShape()
		{
			var v = new Vertices(vertices);
			v.Translate(ref offset);

			shape = new ChainShape(v, createLoop: true);
			fixture = body.CreateFixture(shape);

			fixture.Friction = friction;
			fixture.Restitution = restitution;
			fixture.IsSensor = isSensor;
			if (category != Category.None) fixture.CollisionCategories = category;
		}
	}
}
