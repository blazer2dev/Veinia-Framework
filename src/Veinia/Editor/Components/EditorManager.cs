using Microsoft.Xna.Framework;
using Myra.Graphics2D.UI;

namespace VeiniaFramework.Editor
{
	public class EditorManager : Component
	{
		public override void Initialize()
		{
			var editorObjectManager = FindComponentOfType<EditorObjectManager>();
			var editorLoader = FindComponentOfType<EditorJSON>();

			var panel = new Panel();
			var window = new Window
			{
				Title = level.levelName,
				Content = panel,
				VerticalAlignment = VerticalAlignment.Bottom,
				HorizontalAlignment = HorizontalAlignment.Center,
			};
			window.CloseButton.RemoveFromParent();
			window.DragDirection = DragDirection.None;
			window.Width = 310;

			var saveButton = new TextButton { Text = "Save" };
			saveButton.Click += (s, e) => editorLoader.SaveScene();
			panel.Widgets.Add(saveButton);

			var loadButton = new TextButton { Text = "Load", Left = 50 };
			loadButton.Click += (s, e) => editorLoader.LoadScene();
			panel.Widgets.Add(loadButton);

			var removeAllButton = new TextButton { Text = "Remove All", Left = 100 };
			removeAllButton.Click += (s, e) => editorObjectManager.RemoveAll();
			panel.Widgets.Add(removeAllButton);

			var openLevelSelectButton = new TextButton { Text = "Level Select", Left = 200 };
			openLevelSelectButton.Click += (s, e) =>
			{
				if (Globals.loader.storedLevels.Count > 0 && GetComponent<LevelSelector>() == null)
				{
					AddComponent(new LevelSelector());
				}

				else if (Globals.loader.storedLevels.Count == 0)
				{
					EditorScene.ErrorWindow("Warning!", "There are no stored levels! Use Globals.loader.storedLevels.Add()! Check Samples For Reference.");
				}
			};
			panel.Widgets.Add(openLevelSelectButton);

			window.Show(Globals.myraDesktop, Point.Zero);
		}
	}
}