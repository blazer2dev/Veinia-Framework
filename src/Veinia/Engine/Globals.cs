using Apos.Camera;
using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Tweening;
using Myra.Graphics2D.UI;
using nkast.Aether.Physics2D.Dynamics;
using System;
using System.ComponentModel;

namespace VeiniaFramework
{
	public class Globals
	{
		public static bool debugDraw;

		public static Input input = new Input();
		public static Tweener tweener = new Tweener(); // stops with Time.stop
		public static Tweener unscaledTweener = new Tweener();
		public static ParticleWorld particleWorld = new ParticleWorld();
		public static Random random = new Random();
		public static FontSystem fontSystem = new FontSystem();
		public static Loader loader;
		public static GraphicsDeviceManager graphicsManager;
		public static GraphicsDevice graphicsDevice;
		public static ContentManager content;
		public static Screen screen;
		public static FPS fps;
		public static Camera camera;
		public static World physicsWorld;
		public static GameWindow window;
		public static Desktop myraDesktop;
		public static FrustumCulling frustumCulling;
		public static ShapeDrawing shapeDrawing;
	}
}