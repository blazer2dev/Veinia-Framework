using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Myra.Graphics2D.UI;
using System;
using System.Collections.Generic;

namespace VeiniaFramework.Editor
{
	public class EditorScene : Level
	{
		public Type editedSceneType;
		private static bool windowAppeared;

		public static Keys TriggerKey = Keys.Tab;
		public static bool AllowEditorInRelease = false;

		public EditorScene(string levelPath, Type editedSceneType) : base(levelPath) => this.editedSceneType = editedSceneType;

		public override void CreateScene(bool loadObjectsFromPath)
		{
			// the gameObjects shouldn't be loaded as usual because later we only load their sprites
			base.CreateScene(loadObjectsFromFile: false);

			/* KEYBOARD BINDINGS
			 * 
			 * Toolbar Swap - 1-10
			 * Drag - LMB 
			 * Swipe Motion - LShift + LMB
			 * Debug Draw - F
			 * Show GUID - Y
			 * Hide Grid - G
			 * Mark Layer - Z 
			 * Save - LCtrl + S
			 * Duplicate Selection - LCtrl + D
			 * Rotate - R
			 * Incremented Rotation - LShift (While Rotating)
			 * Rotate By 45 - Q
			 * Reset Rotation - LCtrl + R
			 * Scale - T
			 * Normalized Scale - LShift (While Scaling)
			 * Reset Scale - LCtrl + T
			 * Move Selection - WSAD
			 * Move Selection Slower - WSAD + LShift
			 * Edit Selected - E
			 * Destroy Selection - RMB
			 * Deselect - LAlt + D
			 * Selection Overlap Menu - Middle Mouse Button / C
			 */

			Globals.tweener.CancelAll();
			Globals.unscaledTweener.CancelAll();

			EditorCheckboxes.Add("Debug Draw [X]", defaultValue: Globals.debugDraw, (e, o) => { Globals.debugDraw = true; }, (e, o) => { Globals.debugDraw = false; }, Keys.X);
			EditorCheckboxes.Add("Use File Encryption", FileManager.UseEncryption, (e, o) => { FileManager.UseEncryption = true; }, (e, o) => { FileManager.UseEncryption = false; });
			EditorCheckboxes.Add("Show GUID [Y]", defaultValue: Globals.showGUID, (e, o) => { Globals.showGUID = true; }, (e, o) => { Globals.showGUID = false; }, Keys.Y);

			GameObject systems = Instantiate(Transform.Empty, new List<Component>
			{
				new EditorControls(),
				new EditorGrid(),
				new EditorJSON(levelName),
				new EditorObjectManager(prefabManager),
			});

			var toolbarManager = new ToolbarManager();
			if (prefabManager != null && prefabManager.editorPrefabs.Count > 0)
				toolbarManager.toolbars.Add(new PaintingToolbar("Painting", new PaintingToolbarBehaviour(prefabManager), prefabManager));
			toolbarManager.toolbars.Add(new EditToolbar("Edit", new EditToolbarBehaviour()));

			GameObject UI = Instantiate(Transform.Empty, new List<Component>
			{
				new EditorLabelManager(),
				new EditorCheckboxes(),
				new FPSWindow(),
			});
			UI.AddComponent(toolbarManager);
			UI.AddComponent(new EditorManager());

			if (prefabManager == null) PopupWindow("Warning", "There are no prefabs! Make a class that inherits PrefabManager and add it to Veinia.Initialize()! Check Samples For Reference.");
		}

		public static void PopupWindow(string title, string content, int? width = null, int? height = null, Color? textColor = null)
		{
			if (windowAppeared == true) return;
			windowAppeared = true;

			var panel = new Panel();

			var window = new Window
			{
				Title = title,
				Content = panel,
				HorizontalAlignment = HorizontalAlignment.Center,
				VerticalAlignment = VerticalAlignment.Center,
				DragDirection = DragDirection.None,
				Width = width ?? 400,
			};
			if (height != null) window.Height = height.Value;
			window.CloseButton.Click += (s, e) => { windowAppeared = false; };

			var textBox = new TextBox
			{
				Text = content,
				TextColor = textColor ?? Color.White,
				Wrap = true,
				AcceptsKeyboardFocus = false
			};

			panel.Widgets.Add(textBox);

			window.Show(Globals.myraDesktop, Point.Zero);
		}
	}
}