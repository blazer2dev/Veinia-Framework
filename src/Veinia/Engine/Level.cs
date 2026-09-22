using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Myra.Graphics2D.UI;
using nkast.Aether.Physics2D.Dynamics;
using System.Collections.Generic;
using VeiniaFramework.Editor;

namespace VeiniaFramework
{
	public class Level
	{
		/// <summary>
		/// scene list made for all gameObjects
		/// </summary>
		public List<GameObject> scene = new List<GameObject>();

		/// <summary>
		/// scene list made for iterating and calling methods (Update, Draw, etc.)
		/// </summary>
		public List<GameObject> activeScene = new List<GameObject>();

		public bool firstFrameCreated { get; private set; }


		public Panel Myra = new Panel();
		public PrefabManager prefabManager { get; set; }
		public string levelName;

		public Level(string levelName) => this.levelName = levelName;
		public Level() { }

		/// <summary>
		/// Loads default level contents (GameObjects, Properties, etc.)
		/// </summary>
		public virtual void CreateScene(bool loadObjectsFromFile = true)
		{
			Globals.camera.SetPosition(Vector2.Zero);
			Globals.camera.Scale = 1f;
			Globals.camera.Rotation = 0;
			Globals.camera.ResetLookaheads();
			Globals.camera.shake.Reset();

			Globals.myraDesktop.Root = Myra;

			Globals.particleWorld.Clear();

			prefabManager?.LoadPrefabs();
			if (loadObjectsFromFile && levelName != string.Empty && levelName != null)
				LoadObjects(levelName);
		}

		/// <summary>
		/// Loads objects from a text file.
		/// </summary>
		private void LoadObjects(string editorLevelName)
		{
			var sceneFile = FileManager.Load<SceneFile>(EditorJSON.LevelsFolder, levelName);

			foreach (var item in sceneFile.objects)
			{
				var prefab = prefabManager?.Find(item.PrefabName);
				if (prefab == null) throw new System.Exception("Prefabs that got deleted and are still on " + editorLevelName);

				var sample = Instantiate(new Transform { position = item.Position, rotation = item.Rotation, scale = item.Scale, Z = item.Z }, prefab);
				sample.customData = item.customData;

				if (item.ShouldSerializeColor())
				{
					var sprite = sample.GetComponent<Sprite>();
					if (sprite != null)
						sample.GetComponent<Sprite>().color = item.Color.Value;
				}
			}
		}

		/// <summary>
		/// Creates an object and spawns it
		/// </summary>
		public GameObject Instantiate(Transform transform, List<Component> components, Body body = default, string customData = null, bool isStatic = false, bool dontDestroyOnLoad = false, List<Transform> children = null)
		{
			var sampleBody = body == null ? null : body.DeepClone();
			if (sampleBody != null) sampleBody.Enabled = true;

			GameObject sample = new GameObject(transform, components, sampleBody, customData, isStatic, dontDestroyOnLoad);
			sample.level = this;

			for (int i = 0; i < sample.components.Count; i++)
			{
				var comp = sample.components[i];

				comp.gameObject = sample;
				comp.transform = sample.transform;
				comp.level = sample.level;
			}

			if (firstFrameCreated)
			{
				sample.EarlyInitialize();
				sample.Initialize();

				sample.isInitialized = true;
			}

			transform.Children.Clear();
			if (children != null)
			{
				for (int i = 0; i < children.Count; i++)
				{
					var child = children[i];
					Instantiate(child, child.gameObject).transform.SetParent(transform, worldPositionStays: false);
				}
			}

			scene.Add(sample);
			return sample;
		}
		public GameObject Instantiate(Transform transform, GameObject prefab)
		{
			return Instantiate(transform, prefab.components.Clone(), prefab.body == null ? null : prefab.body.DeepClone(), prefab.customData, prefab.isStatic, prefab.dontDestroyOnLoad, prefab.transform.Children.Clone());
		}
		public GameObject Instantiate(GameObject prefab)
		{
			Transform newT = new Transform
			{
				position = prefab.transform.position,
				rotation = prefab.transform.rotation,
				scale = prefab.transform.scale,
				Z = prefab.transform.Z,
			};
			return Instantiate(newT, prefab.components.Clone(), prefab.body == null ? null : prefab.body.DeepClone(), prefab.customData, prefab.isStatic, prefab.dontDestroyOnLoad, prefab.transform.Children.Clone());
		}

		/// <summary>
		/// Removes the target object from the scene
		/// </summary>
		public void Remove(GameObject target) => scene.Remove(target);


		/// <summary>
		/// Initializes components in the first frame (this is used to make sure all objects are created before Initialize() as it may contain FindObjectsOfType<>)
		/// </summary>
		public void InitializeComponentsFirstFrame()
		{
			firstFrameCreated = true;

			for (int i = 0; i < scene.Count; i++)
			{
				var obj = scene[i];

				if (!obj.isEnabled || obj.isInitialized) continue;

				obj.EarlyInitialize();
				obj.Initialize();

				obj.isInitialized = true;
			}
		}

		/// <summary>
		/// Assigns which objects should be queued for method calls in a frame
		/// </summary>
		public void AssignActiveScene()
		{
			activeScene.Clear();

			for (int i = 0; i < scene.Count; i++)
			{
				var obj = scene[i];

				if (!obj.isEnabled) continue;
				if (obj.frustumCulled && obj.isStatic) continue;

				activeScene.Add(obj);
			}
		}

		/// <summary>
		/// Updates objects in the current scene.
		/// </summary>
		public virtual void Update()
		{
			for (int i = 0; i < activeScene.Count; i++)
			{
				activeScene[i].Update();
			}
		}

		/// <summary>
		/// Updates objects in the current scene after the normal update.
		/// </summary>
		public virtual void LateUpdate()
		{
			for (int i = 0; i < activeScene.Count; i++)
			{
				activeScene[i].LateUpdate();
			}
		}

		/// <summary>
		/// Draws objects in the current scene.
		/// </summary>
		public List<DrawCommand> drawCommands = new List<DrawCommand>();
		public virtual void Draw(SpriteBatch sb, DrawOptions drawOptions)
		{
			// set default transformMatrix
			if (drawOptions.virtualCamera == null)
			{
				drawOptions.virtualCamera = new VirtualCamera
				{
					transformMatrix = () => Globals.camera.GetView(),
				};
			}
			else if (drawOptions.virtualCamera.transformMatrix == null)
			{
				drawOptions.virtualCamera.transformMatrix = () => Globals.camera.GetView();
			}
			//

			for (int i = 0; i < activeScene.Count; i++) // makes drawCommands
			{
				activeScene[i].Draw(sb);
			}

			Globals.particleWorld.Draw(sb, this); // makes particle drawCommands

			for (int i = 0; i < drawCommands.Count; i++) // set default values
			{
				var c = drawCommands[i];
				c.drawOptions.blendState = c.drawOptions.blendState ?? drawOptions.blendState;
				c.drawOptions.depthStencilState = c.drawOptions.depthStencilState ?? drawOptions.depthStencilState;
				c.drawOptions.rasterizerState = c.drawOptions.rasterizerState ?? drawOptions.rasterizerState;
				c.drawOptions.samplerState = c.drawOptions.samplerState ?? drawOptions.samplerState;
				c.drawOptions.shader = c.drawOptions.shader ?? drawOptions.shader;

				c.drawOptions.virtualCamera = c.drawOptions.virtualCamera ?? drawOptions.virtualCamera;
				if (c.drawOptions.virtualCamera.transformMatrix == null) c.drawOptions.virtualCamera.transformMatrix = () => Globals.camera.GetView();

				drawCommands[i] = c;
			}

			// sorting to batch together simillar commands to reduce Begins
			drawCommands.Sort((a, b) =>
					{
						// compare by z
						int zCompare = a.Z.CompareTo(b.Z);
						if (zCompare != 0)
							return zCompare;

						// compare shader
						if (a.drawOptions.shader != b.drawOptions.shader)
						{
							if (a.drawOptions.shader == null) return -1;
							if (b.drawOptions.shader == null) return 1;
							int shaderCompare = a.drawOptions.shader.GetHashCode().CompareTo(b.drawOptions.shader.GetHashCode());
							if (shaderCompare != 0) return shaderCompare;
						}

						// compare samplerState
						if (a.drawOptions.samplerState != b.drawOptions.samplerState)
						{
							if (a.drawOptions.samplerState == null) return -1;
							if (b.drawOptions.samplerState == null) return 1;
							int samplerCompare = a.drawOptions.samplerState.GetHashCode().CompareTo(b.drawOptions.samplerState.GetHashCode());
							if (samplerCompare != 0) return samplerCompare;
						}

						// compare depthStencilState
						if (a.drawOptions.depthStencilState != b.drawOptions.depthStencilState)
						{
							if (a.drawOptions.depthStencilState == null) return -1;
							if (b.drawOptions.depthStencilState == null) return 1;
							int depthCompare = a.drawOptions.depthStencilState.GetHashCode().CompareTo(b.drawOptions.depthStencilState.GetHashCode());
							if (depthCompare != 0) return depthCompare;
						}

						// compare blendState
						if (a.drawOptions.blendState != b.drawOptions.blendState)
						{
							if (a.drawOptions.blendState == null) return -1;
							if (b.drawOptions.blendState == null) return 1;
							int blendCompare = a.drawOptions.blendState.GetHashCode().CompareTo(b.drawOptions.blendState.GetHashCode());
							if (blendCompare != 0) return blendCompare;
						}

						// compare virtualCamera
						if (a.drawOptions.virtualCamera != b.drawOptions.virtualCamera)
						{
							if (a.drawOptions.virtualCamera == null) return -1;
							if (b.drawOptions.virtualCamera == null) return 1;
							int targetCompare = a.drawOptions.virtualCamera.GetHashCode().CompareTo(b.drawOptions.virtualCamera.GetHashCode());
							if (targetCompare != 0) return targetCompare;
						}

						// All equal
						return 0;
					});

			DrawCommand prevCommand = null;
			bool beginCalled = false;
			int begins = 0;

			foreach (var cmd in drawCommands)
			{
				if (prevCommand != null && beginCalled)
				{
					if (cmd.drawOptions.shader != prevCommand.drawOptions.shader // if new Shader
					 || cmd.drawOptions.blendState != prevCommand.drawOptions.blendState // or new BlendState
					 || cmd.drawOptions.depthStencilState != prevCommand.drawOptions.depthStencilState // or new DepthStencilState
					 || cmd.drawOptions.rasterizerState != prevCommand.drawOptions.rasterizerState // or new RasterizerState
					 || cmd.drawOptions.samplerState != prevCommand.drawOptions.samplerState // or new SamplerState
					 || cmd.drawOptions.virtualCamera != prevCommand.drawOptions.virtualCamera // or new VirtualCamera
					 || cmd.drawWithoutSpriteBatch) // or using DrawUserPrimitives
					{
						sb.End();
						beginCalled = false;
					}
				}
				if (!beginCalled && !cmd.drawWithoutSpriteBatch)
				{
					begins++;
					Globals.graphicsDevice.SetRenderTarget(cmd.drawOptions.virtualCamera.renderTarget?.Invoke());
					sb.Begin(SpriteSortMode.Deferred, cmd.drawOptions.blendState, cmd.drawOptions.samplerState, cmd.drawOptions.depthStencilState, cmd.drawOptions.rasterizerState, cmd.drawOptions.shader, cmd.drawOptions.virtualCamera.transformMatrix?.Invoke());
					beginCalled = true;
				}

				if (cmd.drawWithoutSpriteBatch)
				{
					// for DrawUserPrimitives
					if (cmd.drawOptions.shader != null) cmd.drawOptions.shader.CurrentTechnique.Passes[0].Apply();
					if (cmd.drawOptions.blendState != null) Globals.graphicsDevice.BlendState = cmd.drawOptions.blendState;
					if (cmd.drawOptions.depthStencilState != null) Globals.graphicsDevice.DepthStencilState = cmd.drawOptions.depthStencilState;
					if (cmd.drawOptions.rasterizerState != null) Globals.graphicsDevice.RasterizerState = cmd.drawOptions.rasterizerState;
					if (cmd.drawOptions.samplerState != null) Globals.graphicsDevice.SamplerStates[0] = cmd.drawOptions.samplerState;
				}

				prevCommand = cmd;
				cmd.command.Invoke();
			}

			drawCommands.Clear();

			if (beginCalled)
			{
				sb.End();
				beginCalled = false;
			}
			Globals.graphicsDevice.SetRenderTarget(drawOptions.virtualCamera.renderTarget?.Invoke());

			Title.Add(begins, " - SpriteBatch Begins", 5);
		}

		/// <summary>
		/// Unloads current level
		/// </summary>
		public virtual void Unload()
		{
			firstFrameCreated = false;

			foreach (var obj in scene.ToArray())
			{
				if (!obj.dontDestroyOnLoad)
					obj.DestroyGameObject();
			}
			Globals.physicsWorld.Clear();
		}

		/// <summary>
		/// Finds a component in the scene.
		/// </summary>
		public T1 FindComponentOfType<T1>() where T1 : Component
		{
			var returnVal = new List<T1>();

			foreach (var item in scene)
			{
				T1 currentItem = item.GetComponent<T1>();
				if (currentItem == null) continue;
				else returnVal.Add(currentItem);
			}

			if (returnVal.Count == 0)
			{
				Say.Line("FindComponentOfType<T1> - Found no components matching requirements! " + typeof(T1));
				return default;
			}

			if (returnVal.Count > 1)
				Say.Line("FindComponentOfType<T1> - Found more than one component matching the requirements! " + typeof(T1));

			return returnVal[0];
		}

		/// <summary>
		/// Finds multiple components in a scene.
		/// </summary>
		public List<T1> FindComponentsOfType<T1>() where T1 : Component
		{
			var returnVal = new List<T1>();

			foreach (var item in scene)
			{
				var matchingComponents = item.GetAllComponents<T1>();

				matchingComponents.AddAllTo(returnVal);
			}
			if (returnVal.Count == 0)
				Say.Line("FindComponentsOfType<T1> - Found no components matching requirements! " + typeof(T1));

			return returnVal;
		}

		/// <summary>
		/// Finds an object in the scene by customData.
		/// </summary>
		public GameObject FindObjectByData(object match)
		{
			var returnVal = new List<GameObject>();

			foreach (var item in scene)
			{
				GameObject currentItem = null;
				if (item.customData != null && item.customData.Equals(match))
					currentItem = item;
				if (currentItem == null) continue;
				else returnVal.Add(currentItem);
			}

			if (returnVal.Count == 0)
			{
				Say.Line("FindObjectByData - Found no object matching requirements! Query: " + (string)match);
				return default;
			}

			if (returnVal.Count > 1)
				Say.Line("FindObjectByData - Found more than one object matching the requirements! Query:" + (string)match);

			return returnVal[0];
		}

		/// <summary>
		/// Finds objects in the scene by customData.
		/// </summary>
		public List<GameObject> FindObjectsByData(object match)
		{
			var returnVal = new List<GameObject>();

			foreach (var item in scene)
			{
				GameObject currentItem = null;
				if (item.customData != null && item.customData.Equals(match))
					currentItem = item;
				if (currentItem == null) continue;
				else returnVal.Add(currentItem);
			}
			if (returnVal.Count == 0)
				Say.Line("FindObjectsByData - Found no object matching requirements! Query: " + (string)match);

			return returnVal;
		}

		/// <summary>
		/// Finds component in the scene by customData
		/// </summary>
		public T1 FindComponentByData<T1>(object match) where T1 : Component => FindObjectByData(match).GetComponent<T1>();

		/// <summary>
		/// Finds components in the scene by customData
		/// </summary>
		public List<T1> FindComponentsByData<T1>(object match) where T1 : Component
		{
			var objects = FindObjectsByData(match);

			var returnVal = new List<T1>();

			foreach (var item in objects)
			{
				var currentItem = item.GetComponent<T1>();
				if (currentItem != null)
				{
					returnVal.Add(currentItem);
				}
			}

			return returnVal;
		}
	}
}