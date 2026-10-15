using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using nkast.Aether.Physics2D.Dynamics;
using System.Collections.Generic;
using System.IO;

namespace VeiniaFramework.Samples.BlockBreaker
{
	public class LevelTemplate : Level
	{
		public LevelTemplate(string levelPath) : base(levelPath)
		{
		}

		public LevelTemplate() : base()
		{
		}

		public override void CreateScene(bool loadObjectsFromPath = true)
		{
			base.CreateScene();


			GameObject leftBorder = Instantiate(
				new Transform { position = -Vector2.UnitX * 9.7f },
				new List<Component>
				{
				}, isStatic: true);
			leftBorder.body = Globals.physicsWorld.CreateRectangle(.3f, 10, 1f, bodyType: BodyType.Static);

			GameObject rightBorder = Instantiate(
				new Transform { position = Vector2.UnitX * 9.7f },
				new List<Component>
				{
				}, isStatic: true);
			rightBorder.body = Globals.physicsWorld.CreateRectangle(.3f, 10, 1f, bodyType: BodyType.Static);

			GameObject topBorder = Instantiate(
				new Transform { position = Vector2.UnitY * 5.5f },
				new List<Component>
				{
				}, isStatic: true);
			topBorder.body = Globals.physicsWorld.CreateRectangle(19f, .3f, 1f, bodyType: BodyType.Static);

			GameObject bottomBorder = Instantiate(
				new Transform { position = Vector2.UnitY * -5.5f },
				new List<Component>
				{
					new GameOverBorder(),
				}, isStatic: true);
			bottomBorder.body = Globals.physicsWorld.CreateRectangle(19f, .3f, 1f, bodyType: BodyType.Static);



			//LEVEL
			GameObject ball = Instantiate(
				new Transform { position = -Vector2.UnitY * 10, Z = .1f },
				new List<Component>
				{
					new Sprite("Sprites/Ball", Color.Blue, pixelsPerUnit: 120),
					new Ball(),
				}, isStatic: false);
			GameObject paddle = Instantiate(
				new Transform { Z = .1f },
				new List<Component>
				{
					new Sprite("Sprites/Paddle", Color.Blue, pixelsPerUnit: 100, options: new DrawOptions{ shader = Globals.content.Load<Effect>(Path.Combine("veinia_defaults", "flash_sprite"))}),
					new Paddle(),
				}, isStatic: false);

			GameObject UI = Instantiate(
				new Transform { },
				new List<Component>
				{
					new UI()
				}, isStatic: true);
		}

		public override void Unload()
		{
			base.Unload();

			Time.stop = false;
		}
	}
}