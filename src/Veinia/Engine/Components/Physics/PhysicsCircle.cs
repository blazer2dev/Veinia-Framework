using Microsoft.Xna.Framework;
using nkast.Aether.Physics2D.Collision.Shapes;
using nkast.Aether.Physics2D.Dynamics;

namespace VeiniaFramework
{
	public class PhysicsCircle : PhysicsShape
	{
		float radius;
		float friction;
		float restitution;
		bool isSensor;
		Category category;

		private Shape shape;

		private Fixture fixture;

		public PhysicsCircle(float radius = 1, float friction = .2f, float restitution = 0, bool isSensor = false, Category category = Category.None, BodyType bodyType = BodyType.Static, Vector2 offset = default, object tag = null, bool ignoreGravity = false, bool sleepingAllowed = true)
			: base(bodyType, offset, tag, ignoreGravity, sleepingAllowed)
		{
			this.radius = radius;
			this.friction = friction;
			this.restitution = restitution;
			this.isSensor = isSensor;
			this.category = category;
		}

		public override object Clone()
		{
			var clone = (PhysicsCircle)base.Clone();
			clone.shape = null;
			clone.fixture = null;
			return clone;
		}

		protected override void MakeShape()
		{
			var circleShape = new CircleShape(radius, 1f);
			circleShape.Position = offset;
			shape = circleShape;

			fixture = body.CreateFixture(shape);

			fixture.Friction = friction;
			fixture.Restitution = restitution;
			fixture.IsSensor = isSensor;
			if (category != Category.None) fixture.CollisionCategories = category;
		}
	}
}
