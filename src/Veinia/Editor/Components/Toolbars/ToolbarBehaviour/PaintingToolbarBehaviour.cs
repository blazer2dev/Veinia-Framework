using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Myra.Graphics2D.UI;
using System;
using System.Collections.Generic;
using System.IO;

namespace VeiniaFramework.Editor
{
	public class PaintingToolbarBehaviour : ToolbarBehaviour
	{
		PrefabManager prefabManager;

		public List<EditorObject> currentObjectLayer = new List<EditorObject>();

		EditorControls editorControls;
		EditorObjectManager editorObjectManager;

		public static bool markLayer;

		public static string currentPrefabName { get; private set; }

		GameObject objectPreview;

		EditorObject lastPlacedObject;

		Vector2 mousePos;
		Vector2 mouseGridPos;


		public PaintingToolbarBehaviour(PrefabManager prefabManager) => this.prefabManager = prefabManager;

		public override void OnInitialize()
		{
			editorObjectManager = gameObject.level.FindComponentOfType<EditorObjectManager>();
			editorControls = gameObject.level.FindComponentOfType<EditorControls>();

			editorObjectManager.OnSpawn += (e) =>
			{
				if (e.PrefabName == currentPrefabName) currentObjectLayer.Add(e);
			};
			editorObjectManager.OnRemove += (e) =>
			{
				if (e.PrefabName == currentPrefabName) currentObjectLayer.Remove(e);
				if (lastPlacedObject == e) lastPlacedObject = null;
			};
			editorObjectManager.OnRemoveAll += () => { currentObjectLayer.Clear(); };

			// currentPrefabName is static so it remembers between editor sessions
			ChangeCurrentPrefab(currentPrefabName == null ? prefabManager.editorPrefabs[0].PrefabName : currentPrefabName);

			markLayer = false; // because its static we need to make sure its false by default
			EditorCheckboxes.Add("Mark Layer [Z]", defaultValue: false, (e, o) => { markLayer = true; }, (e, o) => { markLayer = false; }, Keys.Z);
		}

		public void CreateNewPreview()
		{
			if (objectPreview != null) objectPreview.DestroyGameObject();

			var prefab = prefabManager.Find(currentPrefabName);

			if (prefab.GetComponent<Sprite>() != null) objectPreview = prefab.ExtractComponentToNewGameObject<Sprite>(Transform.Empty);
			else // default prefab sprite
			{
				objectPreview = prefab.ExtractComponentToNewGameObject<Transform>(Transform.Empty);
				objectPreview.AddComponent(new Sprite(Path.Combine("veinia_defaults", "prefab_default")));
			}

			objectPreview = gameObject.level.Instantiate(objectPreview);

			var sprite = objectPreview.GetComponent<Sprite>();
			sprite.transform.Z = float.MaxValue;
			sprite.color *= .5f;

			UpdatePreview();
		}

		private void UpdatePreview()
		{
			if (objectPreview == null) return;

			if (!Globals.input.GetKey(Keys.LeftControl))
				objectPreview.transform.position = mouseGridPos;
			else
				objectPreview.transform.position = mousePos;
		}

		public void ChangeCurrentPrefab(string newPrefabName)
		{
			currentPrefabName = newPrefabName;

			currentObjectLayer = editorObjectManager.editorObjects.FindAll(x => x.PrefabName == currentPrefabName);

			EditorLabelManager.Add("currentPrefabName", new Label { Text = "Prefab - " + currentPrefabName, VerticalAlignment = VerticalAlignment.Top, HorizontalAlignment = HorizontalAlignment.Center, Top = 50 });
		}

		public override void OnExitTab(Toolbar newToolbar)
		{
			objectPreview.DestroyGameObject();

			if (lastPlacedObject != null)
			{
				var editToolbarBehaviour = (EditToolbarBehaviour)newToolbar.toolbarBehaviour;
				editToolbarBehaviour.selectedObjects.Add(lastPlacedObject);
				lastPlacedObject = null;
			}
		}
		public override void OnEnterTab() => CreateNewPreview();

		public override void OnUpdate()
		{
			mousePos = Globals.input.GetMouseWorldPosition();
			mouseGridPos = new Vector2(MathF.Round(mousePos.X), MathF.Round(mousePos.Y));

			UpdatePreview();

			var swipe = Globals.input.GetKey(Keys.LeftShift);

			if (!editorControls.isDragging && !Globals.myraDesktop.IsMouseOverGUI && !EditorControls.isMouseOverGUIPreviousFrame)
			{
				//placing
				if (Globals.input.GetMouseUp(0) || Globals.input.GetMouse(0) && swipe)
				{
					var pos = Globals.input.GetKey(Keys.LeftControl) ? mousePos : mouseGridPos;

					if (editorObjectManager.PrefabOverlapsWithPoint(pos, currentPrefabName) == null)
					{
						lastPlacedObject = editorObjectManager.Spawn(currentPrefabName, position: pos);
					}
				}
				//

				//deleting
				if (Globals.input.GetMouseUp(1) || Globals.input.GetMouse(1) && swipe)
				{
					var overlap = editorObjectManager.PrefabOverlapsWithPoint(mousePos, currentPrefabName);
					if (overlap == null) overlap = editorObjectManager.PrefabOverlapsWithPoint(mouseGridPos, currentPrefabName);
					if (overlap != null) editorObjectManager.Remove(overlap);
				}
				//
			}
		}


		public override void OnDraw(SpriteBatch sb)
		{
			if (markLayer)
				foreach (var item in currentObjectLayer)
				{
					sb.VeiniaRectangle(gameObject.level, item.EditorPlacedSprite.rect.OffsetByHalf(), Color.Green, thickness: 4 * Globals.camera.Scale);
				}
		}
	}
}
