using Microsoft.Xna.Framework;
using nkast.Aether.Physics2D.Collision.Shapes;
using nkast.Aether.Physics2D.Common;
using nkast.Aether.Physics2D.Dynamics;
using System.Linq;

namespace VeiniaFramework
{
	public class PhysicsRectangle : PhysicsShape
	{
		float width;
		float height;
		float friction;
		float restitution;
		bool isSensor;
		Category category;

		private Vector2 oldScale;

		private Shape shape;

		private Fixture fixture;

		public PhysicsRectangle(float width = 1, float height = 1, float friction = .2f, float restitution = 0, bool isSensor = false, Category category = Category.None, BodyType bodyType = BodyType.Static, Vector2 offset = default, object tag = null, bool ignoreGravity = false, bool sleepingAllowed = true)
			: base(bodyType, offset, tag, ignoreGravity, sleepingAllowed)
		{
			this.width = width;
			this.height = height;
			this.friction = friction;
			this.restitution = restitution;
			this.isSensor = isSensor;
			this.category = category;
		}

		public override object Clone()
		{
			var clone = (PhysicsRectangle)base.Clone();
			clone.shape = null;
			clone.fixture = null;
			return clone;
		}

		public override void Update()
		{
			if (transform.scale != oldScale)
			{
				var removeFixture = body.FixtureList.FirstOrDefault(f => f == fixture);
				if (removeFixture != null)
					body.Remove(removeFixture);

				MakeShape();
				oldScale = transform.scale;
			}
		}

		protected override void MakeShape()
		{
			var v = PolygonTools.CreateRectangle(width * transform.scale.X / 2f, height * transform.scale.Y / 2f);
			v.Translate(ref offset);
			shape = new PolygonShape(v, 1f);

			fixture = body.CreateFixture(shape);

			fixture.Friction = friction;
			fixture.Restitution = restitution;
			fixture.IsSensor = isSensor;
			if (category != Category.None) fixture.CollisionCategories = category;
		}
	}
}
