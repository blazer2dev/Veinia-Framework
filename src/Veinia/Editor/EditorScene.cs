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
		private static bool errorWindowAppeared;

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

			if (prefabManager == null) ErrorWindow("Warning", "There are no prefabs! Make a class that inherits PrefabManager and add it to Veinia.Initialize()! Check Samples For Reference.");
		}

		public static void ErrorWindow(string title, string content)
		{
			if (errorWindowAppeared == true) return;
			errorWindowAppeared = true;

			var panel = new Panel();

			var window = new Window
			{
				Title = title,
				Content = panel,
				HorizontalAlignment = HorizontalAlignment.Center,
				VerticalAlignment = VerticalAlignment.Center
			};
			window.DragDirection = DragDirection.None;
			window.CloseButton.Click += (s, e) => { errorWindowAppeared = false; };
			window.Width = 400;

			var textBox = new TextBox { Text = content, TextColor = Color.Red, Wrap = true };
			textBox.AcceptsKeyboardFocus = false;

			panel.Widgets.Add(textBox);

			window.Show(Globals.myraDesktop, Point.Zero);
		}
	}
}