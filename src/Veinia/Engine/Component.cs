using nkast.Aether.Physics2D.Dynamics;
using nkast.Aether.Physics2D.Dynamics.Contacts;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace VeiniaFramework
{
	public class Component : ICloneable
	{
		[Browsable(false)] public GameObject gameObject;
		[Browsable(false)] public Transform transform;
		[Browsable(false)] public Level level;
		[Browsable(false)] public Body body { get { return gameObject.body; } set { gameObject.body = value; } }
		[Browsable(false)] public bool isStatic { get { return gameObject.isStatic; } set { gameObject.isStatic = value; } }
		[Browsable(false)] public bool dontDestroyOnLoad { get { return gameObject.dontDestroyOnLoad; } set { gameObject.dontDestroyOnLoad = value; } }
		[Browsable(false)] public string customData { get { return gameObject.customData; } set { gameObject.customData = value; } }
		[Browsable(false)] public bool isEnabled = true;

		public object Clone() => MemberwiseClone();
		public virtual void EarlyInitialize() { }
		public virtual void Initialize() { }
		public virtual void Update() { }
		public virtual void LateUpdate() { }
		public virtual bool OnCollide(Fixture sender, Fixture other, Contact contact) => true;
		public virtual void OnSeparate(Fixture sender, Fixture other, Contact contact) { }
		public T1 FindComponentOfType<T1>() where T1 : Component => level.FindComponentOfType<T1>();
		public List<T1> FindComponentsOfType<T1>() where T1 : Component => level.FindComponentsOfType<T1>();
		public GameObject FindObjectByData(object match) => level.FindObjectByData(match);
		public T1 FindComponentByData<T1>(object match) where T1 : Component => level.FindComponentByData<T1>(match);
		public List<T1> FindComponentsByData<T1>(object match) where T1 : Component => level.FindComponentsByData<T1>(match);
		public List<GameObject> FindObjectsByData(object match) => level.FindObjectsByData(match);
		public T1 GetComponent<T1>() where T1 : Component => gameObject.GetComponent<T1>();
		public List<T1> GetAllComponents<T1>() where T1 : Component => gameObject.GetAllComponents<T1>();
		public Component AddComponent(Component component) => gameObject.AddComponent(component);
		public void RemoveComponent(Component component) => gameObject.RemoveComponent(component);
		public GameObject Instantiate(Transform transform, List<Component> components, Body body = default, string customData = null, bool isStatic = false, bool dontDestroyOnLoad = false) => level.Instantiate(transform, components, body, customData, isStatic, dontDestroyOnLoad);
		public GameObject Instantiate(Transform transform, GameObject newGameObject) => level.Instantiate(transform, newGameObject);
		public GameObject Instantiate(GameObject newGameObject) => level.Instantiate(newGameObject);
		public void DestroyGameObject(bool destroyChildObjects = false) => gameObject.DestroyGameObject(destroyChildObjects);
	}
}