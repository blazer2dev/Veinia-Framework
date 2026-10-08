using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Myra.Graphics2D.UI;
using System;
using System.Collections.Generic;

namespace VeiniaFramework.Editor
{
	public class EditorObjectManager : Component, IDrawn
	{
		PrefabManager prefabManager;

		public List<EditorObject> editorObjects = new List<EditorObject>();

		public Action<EditorObject> OnSpawn;
		public Action<EditorObject> OnRemove;
		public Action OnRemoveAll;

		EditToolbarBehaviour editToolbarBehaviour;

		bool drawGizmos = true;


		public EditorObjectManager(PrefabManager prefabManager) => this.prefabManager = prefabManager;

		public override void Initialize()
		{
			editToolbarBehaviour = (EditToolbarBehaviour)FindComponentOfType<ToolbarManager>().GetToolbar<EditToolbar>().toolbarBehaviour;

			EditorCheckboxes.Add("Draw Gizmos", defaultValue: true, (e, o) => { drawGizmos = true; }, (e, o) => { drawGizmos = false; });
			UpdateObjectCountLabel();
		}

		public EditorObject Spawn(string prefabName, Vector2 position = default, float rotation = default, Vector2 scale = default, float z = default, string customData = null, Color? color = null)
		{
			var prefab = prefabManager.Find(prefabName);

			IDrawGizmos drawGizmo = null;
			foreach (var component in prefab.components)
			{
				if (component is IDrawGizmos) drawGizmo = (IDrawGizmos)component;
			}

			var newT = new Transform
			{
				position = position == default ? prefab.transform.position : position,
				rotation = rotation == default ? prefab.transform.rotation : rotation,
				scale = scale == default ? prefab.transform.scale : scale,
				Z = z == default ? prefab.transform.Z : z,
			};


			Sprite editorPlacedSprite;
			if (prefab.GetComponent<Sprite>() != null)
			{
				var extractedSpriteGameObject = prefab.ExtractComponentToNewGameObject<Sprite>(newT, isStatic: true);
				editorPlacedSprite = Instantiate(extractedSpriteGameObject).GetComponent<Sprite>();
				editorPlacedSprite.drawOptions.depthStencilState = null; // ignore stencil for editor as we want to see the sprites always
			}
			else // add default prefab sprite
			{
				var extracted = prefab.ExtractComponentToNewGameObject<Transform>(Transform.Empty);
				editorPlacedSprite = (Sprite)Instantiate(extracted).AddComponent(new Sprite("veinia_defaults/prefab_default"));
			}

			// grab color from sprite if not overriden
			var newCol = color ?? Color.White;
			newCol = editorPlacedSprite.color != Color.White && color == null ? editorPlacedSprite.color : newCol;


			var newEditorObject = new EditorObject
			{
				PrefabName = prefabName,
				EditorPlacedSprite = editorPlacedSprite,

				// if new transform is default fallback to what prefab had set
				Position = newT.position,
				Rotation = newT.rotation,
				Scale = newT.scale,
				Z = newT.Z,
				customData = customData ?? prefab.customData,
				Color = newCol,
				drawGizmo = drawGizmo,
			};

			editorObjects.Add(newEditorObject);
			OnSpawn?.Invoke(newEditorObject);

			UpdateObjectCountLabel();

			return newEditorObject;
		}
		public EditorObject Spawn(EditorObject ob) => Spawn(ob.PrefabName, ob.Position, ob.Rotation, ob.Scale, ob.Z, ob.customData, ob.Color);

		public void Remove(EditorObject editorObject)
		{
			if (editorObject == null) return;

			editorObject.EditorPlacedSprite.DestroyGameObject();
			editorObjects.Remove(editorObject);

			OnRemove?.Invoke(editorObject);

			UpdateObjectCountLabel();
		}

		public void RemoveAll()
		{
			foreach (var editorObject in editorObjects.ToArray())
			{
				if (editorObject == null) return;

				editorObject.EditorPlacedSprite.DestroyGameObject();
				editorObjects.Remove(editorObject);
			}

			OnRemoveAll?.Invoke();

			UpdateObjectCountLabel();
		}

		private void UpdateObjectCountLabel() => EditorLabelManager.Add("ObjectCount", new Label { Text = "Object Count - " + editorObjects.Count, VerticalAlignment = VerticalAlignment.Top, HorizontalAlignment = HorizontalAlignment.Center });

		public EditorObject PrefabOverlapsWithPoint(Vector2 overlapPoint, string prefabName)
		{
			var overlap = editorObjects.Find(x => x.PrefabName == prefabName && x.EditorPlacedSprite.rect
												   .OffsetByHalf()
												   .Contains(Transform.WorldToScreenPos(overlapPoint)));

			return overlap;
		}

		public EditorObject[] AllOverlapsWithPoint(Vector2 overlapPoint)
		{
			return editorObjects.FindAll(x => x.EditorPlacedSprite.rect
												   .OffsetByHalf()
												   .Contains(Transform.WorldToScreenPos(overlapPoint))).ToArray();
		}

		public EditorObject OverlapsWithPoint(Vector2 overlapPoint)
		{
			return editorObjects.Find(x => x.EditorPlacedSprite.rect
												   .OffsetByHalf()
												   .Contains(Transform.WorldToScreenPos(overlapPoint)));
		}

		public List<EditorObject> GetInsideRectangle(Rectangle rect) => editorObjects.FindAll(x => rect.Intersects(x.EditorPlacedSprite.rect.OffsetByHalf()));

		public void Draw(SpriteBatch sb)
		{
			if (drawGizmos)
			{
				foreach (var obj in editorObjects)
				{
					obj.drawGizmo?.DrawGizmos(sb, gameObject.level, obj, isSelected: editToolbarBehaviour.selectedObjects.Contains(obj));
				}
			}
		}
	}
}