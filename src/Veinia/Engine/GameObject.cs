using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using nkast.Aether.Physics2D.Dynamics;
using nkast.Aether.Physics2D.Dynamics.Contacts;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace VeiniaFramework
{
	public class GameObject
	{
		public List<Component> components;
		[Browsable(false)] public Level level;
		[Browsable(false)] public Transform transform;

		[Browsable(false)]
		public Body body
		{
			get { return Body; }
			set
			{
				value.OnCollision += OnCollision;
				value.OnSeparation += OnSeparation;

				value.Position = transform.position;
				if (linkPhysicsRotationToTransform) value.Rotation = MathHelper.ToRadians(-transform.rotation);

				Body = value;
			}
		}

		private Body Body;

		public bool isStatic;
		public bool dontDestroyOnLoad;
		public bool isEnabled = true;
		public bool frustumCulled;

		// customData from EditorObject
		public string customData;

		// makes body's shape rotate with transform.rotation
		// default behaviour
		public bool linkPhysicsRotationToTransform = true;

		//if your object is dontDestroyOnLoad prevent initializing it on a new scene (initialize once only)
		public bool isInitialized;

		private bool isDestroyed;


		public GameObject(Transform transform, List<Component> components, Body body = default, string customData = null, bool isStatic = false, bool dontDestroyOnLoad = false)
		{
			transform.gameObject = this;
			this.transform = transform;

			this.components = components;
			if (body != null) this.body = body;
			if (customData != null) this.customData = customData;
			this.isStatic = isStatic;
			this.dontDestroyOnLoad = dontDestroyOnLoad;

			RemoveComponent<Transform>(); // remove transform to make sure there aren't two transforms (prefab case)

			components.Add(transform); // the transform is added afterwards to components to ensure the gameobject having a transform
		}

		public void EarlyInitialize()
		{
			for (int i = 0; i < components.Count; i++)
			{
				if (!components[i].isEnabled) continue;
				components[i].EarlyInitialize();
			}
		}

		public void Initialize()
		{
			for (int i = 0; i < components.Count; i++)
			{
				if (!components[i].isEnabled) continue;
				components[i].Initialize();
			}
		}

		public void Update()
		{
			for (int i = 0; i < components.Count; i++)
			{
				if (!components[i].isEnabled) continue;
				components[i].Update();
			}
		}

		public void LateUpdate()
		{
			for (int i = 0; i < components.Count; i++)
			{
				if (!components[i].isEnabled) continue;
				components[i].LateUpdate();
			}
		}

		public void Draw(SpriteBatch sb)
		{
			for (int i = 0; i < components.Count; i++)
			{
				if (!components[i].isEnabled) continue;
				if (components[i] is IDrawn)
				{
					IDrawn drawn = (IDrawn)components[i];
					drawn.Draw(sb); // makes drawCommands
				}
			}
		}

		public List<T1> GetAllComponents<T1>() where T1 : Component
		{
			List<T1> temp = new List<T1>();

			foreach (var component in components)
			{
				if (component is T1)
				{
					temp.Add((T1)component);
				}
			}

			return temp;
		}

		public T1 GetComponent<T1>() where T1 : Component
		{
			if (isDestroyed) throw new Exception("GetComponent<T1> - The object is already destroyed!");

			List<T1> returnVal = new List<T1>();

			foreach (var item in components)
			{
				if (item is T1)
				{
					returnVal.Add((T1)item);
				}
			}

			if (returnVal.Count == 0)
				return default;

			if (returnVal.Count > 1)
				throw new Exception("GetComponent<T1> - More than one matching components! " + typeof(T1));

			else
				return returnVal[0];
		}

		public Component AddComponent(Component component, bool callInitialize = true)
		{
			var compo = (Component)component.Clone();
			components.Add(compo);

			compo.gameObject = this;
			compo.transform = transform;
			compo.level = level;

			if (level != null && level.firstFrameCreated && callInitialize)
			{
				compo.EarlyInitialize();
				compo.Initialize();

				isInitialized = true;
			}

			return compo;
		}

		private void OnSeparation(Fixture sender, Fixture other, Contact contact)
		{
			foreach (var component in components)
				component.OnSeparate(sender, other, contact);
		}

		private bool OnCollision(Fixture sender, Fixture other, Contact contact)
		{
			bool[] temp = new bool[components.Count];
			foreach (var component in components)
				temp[components.IndexOf(component)] = component.OnCollide(sender, other, contact);

			return temp.Min();
		}

		public void RemoveComponent(Component component)
		{
			NextFrame.actions.Add(RemoveNextFrame);

			void RemoveNextFrame()
			{
				if (component is IDisposable)
				{
					IDisposable destroyable = (IDisposable)component;
					destroyable.Dispose();
				}

				components.Remove(components.Find(x => x == component));
			}
		}
		public void RemoveComponent<T1>()
		{
			var c = components.FindAll(x => x is T1);
			if (c.Count > 0) c.ForEach(RemoveComponent);
		}

		public void DestroyGameObject(bool destroyChildObjects = false)
		{
			if (isDestroyed) return;

			level.Remove(this);

			NextFrame.actions.Add(delegate
			{
				level = null;

				components.Clear();
				isDestroyed = true;
				isStatic = false;
				if (body != null && Globals.physicsWorld.BodyList.Contains(body)) Globals.physicsWorld.Remove(body);
			});

			foreach (var component in components)
			{
				if (component is IDisposable)
				{
					IDisposable destroyable = (IDisposable)component;
					destroyable.Dispose();
				}
			}

			if (destroyChildObjects)
			{
				for (int i = 0; i < transform.Children.Count; i++)
				{
					transform.Children[i].DestroyGameObject(destroyChildObjects: true);
				}
			}
		}
	}
}