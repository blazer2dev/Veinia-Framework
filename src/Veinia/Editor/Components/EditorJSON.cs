using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;

namespace VeiniaFramework.Editor
{
	public class EditorJSON : Component
	{
		EditorObjectManager editorObjectManager;

		private string editedLevelName;

		public static string LevelsFolder = "LevelData";

		public SceneFile sceneFile;


		public EditorJSON(string editedLevelName) => this.editedLevelName = editedLevelName;

		public override void Initialize()
		{
			editorObjectManager = FindComponentOfType<EditorObjectManager>();

			sceneFile = new SceneFile();

			LoadScene();
		}

		public void SaveScene()
		{
			if (editedLevelName == null || editedLevelName == string.Empty)
			{
				EditorScene.ErrorWindow("Warning", "The edited level has no name therefore we dont know how to save it! Add a name in the level constructor!");
				return;
			}

			sceneFile.objects = editorObjectManager.editorObjects;
			sceneFile.editorCamPosition = Globals.camera.GetPosition();
			sceneFile.editorCamScale = Globals.camera.Scale;

			var savedData = FileManager.Save(sceneFile, LevelsFolder, editedLevelName, saveToDevWorkspace: true);

			if (OperatingSystem.IsBrowser())
			{
				EditorScene.ErrorWindow("Run Console", "Level Printed In Console: " + editedLevelName);
				Say.Line(savedData);
				return;
			}
		}

		public void LoadScene()
		{
			if (editedLevelName == null || editedLevelName == string.Empty)
			{
				EditorScene.ErrorWindow("Warning", "The edited level has no name therefore we dont know how to load it! Add a name in the level constructor!");
				return;
			}

			editorObjectManager.RemoveAll(); // when changing levels in editor

			sceneFile = FileManager.Load<SceneFile>(LevelsFolder, editedLevelName);

			foreach (var item in sceneFile.objects)
				editorObjectManager.Spawn(item);

			Globals.camera.SetPosition(sceneFile.editorCamPosition ?? Vector2.Zero);
			Globals.camera.Scale = sceneFile.editorCamScale ?? 1;
		}

		public override void Update()
		{
			if (Globals.input.GetKey(Keys.LeftControl) && Globals.input.GetKeyDown(Keys.S)) SaveScene();
		}
	}
}