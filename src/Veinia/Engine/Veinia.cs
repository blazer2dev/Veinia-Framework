using Apos.Camera;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Myra;
using Myra.Graphics2D.UI;
using Newtonsoft.Json;
using nkast.Aether.Physics2D.Diagnostics;
using nkast.Aether.Physics2D.Dynamics;
using System;
using System.IO;
using VeiniaFramework.Editor;

namespace VeiniaFramework
{
	public class Veinia
	{
		Title title;
		Game game;
		DebugView debugView;

		public static bool isEditor { get; private set; }
		public static bool PausedGameWhenInactiveWindow { get; private set; }
		public bool pauseOnUnfocused;


		public Veinia(Game game, GraphicsDeviceManager graphicsManager)
		{
			//this one takes effect only in a game's constructor
			this.game = game;
			Globals.graphicsManager = graphicsManager;
			Globals.fps = new FPS(game);

			JsonConvert.DefaultSettings = () => new JsonSerializerSettings { Converters = { new Vector2JSONConverter() } };
		}

		public void Initialize(GraphicsDevice graphicsDevice, ContentManager content, GameWindow window,
							   Screen screen, int unitSize, Vector2? gravity = null, PrefabManager prefabManager = null)
		{
			#region Veinia
			Transform.unitSize = unitSize;

			Globals.loader = new Loader(prefabManager);
			Globals.graphicsDevice = graphicsDevice;
			Globals.content = content;
			Globals.screen = screen;
			Globals.camera = new Camera(new DensityViewport(graphicsDevice, window, 1920, 1080));
			Globals.physicsWorld = new World(gravity ?? new Vector2(0, -9.81f));
			Globals.shapeDrawing = new ShapeDrawing(graphicsDevice);
			Globals.frustumCulling = new FrustumCulling();


			using var stream = TitleContainer.OpenStream(Path.Combine(content.RootDirectory, "veinia_defaults/arial_rounded.ttf"));
			Globals.fontSystem.AddFont(stream);


			window.ClientSizeChanged += (s, a) => screen.ClientSizeChanged();

			// dont clear null renderTarget when switching
			graphicsDevice.PresentationParameters.RenderTargetUsage = RenderTargetUsage.PreserveContents;

			title = new Title(window);
			debugView = new DebugView(Globals.physicsWorld);

			debugView.LoadContent(graphicsDevice, content);

			#endregion

			#region Myra.UI	

			MyraEnvironment.Game = game;
			Globals.myraDesktop = new Desktop
			{
				Opacity = .95f,
				HasExternalTextInput = true,
			};

			window.TextInput += (s, a) => Globals.myraDesktop.OnChar(a.Character);
			#endregion
		}

		public void Update(GameTime gameTime)
		{
			#region Veinia

			PausedGameWhenInactiveWindow = pauseOnUnfocused && !game.IsActive;

			Globals.fps.CalculateFps(gameTime);

			NextFrame.Update();

			Timers.Update();

			Globals.unscaledTweener.Update(Time.unscaledDeltaTime);

			Time.Update(gameTime);

			if (!isEditor && !PausedGameWhenInactiveWindow && !Time.stop
			  || isEditor && !Time.stop && game.IsActive)
			{
				if (game.IsActive) Globals.input.Update();

				SingletonManager.Update();

				Globals.tweener.Update(Time.deltaTime);
				Globals.frustumCulling.Update();

				var level = Globals.loader.current;
				if (level != null)
				{
					Globals.physicsWorld.Step(Time.deltaTime);

					level.AssignActiveScene();
					level.Update();
					level.LateUpdate();
				}

				Globals.particleWorld.Update();

				Globals.camera.shake.Update();
				Globals.shapeDrawing.UpdateBasicEffect();

				SingletonManager.LateUpdate();
			}

			title.Update();
			#endregion

			#region Myra.UI
			MyraEnvironment.MouseInfoGetter = () => { return PausedGameWhenInactiveWindow ? default : MyraEnvironment.DefaultMouseInfoGetter(); };
			#endregion

			if (Globals.input.GetKeyDown(EditorScene.TriggerKey))
				ToggleEditor(Globals.loader.current);
		}

		public void ToggleEditor(Level level = null)
		{
#if RELEASE
			if (!EditorScene.AllowEditorInRelease) return;
#endif

			var targetLevel = level ?? Globals.loader.current;

			if (targetLevel is EditorScene)
			{
				isEditor = false;

				var editorScene = (EditorScene)Globals.loader.current;

				if (editorScene.editedSceneType == null)
				{
					EditorScene.ErrorWindow("Warning", "Cant play! No level type loaded! Use Globals.loader.DynamicalyLoad() after Veinia.Initialize() or Globals.loader.AddStoredLevels()");
					return;
				}

				var editedLevelInstance = (Level)Activator.CreateInstance(editorScene.editedSceneType);
				editedLevelInstance.levelName = editorScene.levelName;

				Globals.loader.Load(editedLevelInstance);
			}
			else
			{
				isEditor = true;
				if (!game.IsMouseVisible) game.IsMouseVisible = true;

				var editorScene = new EditorScene(targetLevel?.levelName, targetLevel?.GetType());
				Globals.loader.Load(editorScene);
			}
		}

		public void Draw(SpriteBatch spriteBatch, DrawOptions drawOptions = default)
		{
			DrawWorld(spriteBatch, drawOptions);
			DrawDebugPhysics();
			DrawMyra();
		}

		public void DrawWorld(SpriteBatch spriteBatch, DrawOptions drawOptions = default) => Globals.loader.current?.Draw(spriteBatch, drawOptions);
		public void DrawMyra() => Globals.myraDesktop.Render();
		public void DrawDebugPhysics(Vector2? scaleFactor = null)
		{
			if (Globals.debugDraw)
			{
				var cam = Globals.camera;
				float zScale = cam.ZToScale(cam.Z, 0);

				var currentScale = scaleFactor ?? Vector2.Zero;
				var view = cam.VirtualViewport.Transform(
					Matrix.CreateTranslation(-cam.X / Transform.unitSize, cam.Y / Transform.unitSize, 0f) *
					Matrix.CreateRotationZ(-cam.Rotation) *
					Matrix.CreateScale(1f / (cam.Scale + currentScale.X), 1f / (-cam.Scale - currentScale.Y), 1f) *
					Matrix.CreateScale(zScale, zScale, 1f) *
					Matrix.CreateTranslation(new Vector3(cam.VirtualViewport.Origin, 0f)) *
					Matrix.CreateTranslation(new Vector3(cam.shake.shakeOffset / Transform.unitSize, 0f)));

				debugView.RenderDebugData(cam.GetProjection() * Matrix.CreateScale(Transform.unitSize), view);
			}
		}
	}
}