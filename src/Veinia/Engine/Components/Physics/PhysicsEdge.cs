using Microsoft.Xna.Framework;
using nkast.Aether.Physics2D.Collision.Shapes;
using nkast.Aether.Physics2D.Dynamics;

namespace VeiniaFramework
{
	public class PhysicsEdge : PhysicsShape
	{
		Vector2 point1;
		Vector2 point2;
		float friction;
		float restitution;
		bool isSensor;
		Category category;

		[ResetOnInstantiate]
		private Shape shape;

		[ResetOnInstantiate]
		private Fixture fixture;

		public PhysicsEdge(Vector2 point1, Vector2 point2, float restitution = 0, float friction = .2f, bool isSensor = false, Category category = Category.None, BodyType bodyType = BodyType.Static, Vector2 offset = default, object tag = null, bool ignoreGravity = false, bool sleepingAllowed = true)
			: base(bodyType, offset, tag, ignoreGravity, sleepingAllowed)
		{
			this.point1 = point1;
			this.point2 = point2;
			this.restitution = restitution;
			this.friction = friction;
			this.isSensor = isSensor;
			this.category = category;
		}

		protected override void MakeShape()
		{
			shape = new EdgeShape(point1 + offset, point2 + offset);

			fixture = body.CreateFixture(shape);

			fixture.Friction = friction;
			fixture.Restitution = restitution;
			fixture.IsSensor = isSensor;
			if (category != Category.None) fixture.CollisionCategories = category;
		}
	}
}
