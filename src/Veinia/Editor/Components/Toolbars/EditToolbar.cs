using Myra.Graphics2D.UI;

namespace VeiniaFramework.Editor
{
	public class EditToolbar : Toolbar
	{
		public EditToolbar(string toolbarName, ToolbarBehaviour toolbarBehaviour) : base(toolbarName, toolbarBehaviour)
		{
		}

		public override void OnInitialize(GameObject gameObject)
		{
			var editToolbarBehaviour = (EditToolbarBehaviour)toolbarBehaviour;

			var panel = new Panel { Height = 280 };
			displayedToolbarContent = panel;

			var destroySelectedButton = new TextButton { Text = "Destroy Selected [RMB]" };
			destroySelectedButton.Click += (o, e) =>
			{
				editToolbarBehaviour.DestroySelection();
			};
			panel.Widgets.Add(destroySelectedButton);

			var deselectButton = new TextButton { Text = "Deselect [LAlt+D]", Top = 25 };
			deselectButton.Click += (o, e) =>
			{
				editToolbarBehaviour.selectedObjects.Clear();
			};
			panel.Widgets.Add(deselectButton);

			var duplicateButton = new TextButton { Text = "Duplicate [Ctrl+D]", Top = 50 };
			duplicateButton.Click += (o, e) =>
			{
				editToolbarBehaviour.Duplicate();
			};
			panel.Widgets.Add(duplicateButton);

			var filterSelectionButton = new TextButton { Text = "Filter Selection [C]", Top = 75 };
			filterSelectionButton.Click += (o, e) =>
			{
				editToolbarBehaviour.FilterSelection();
			};
			panel.Widgets.Add(filterSelectionButton);

			var freeMoveButton = new TextButton { Text = "Free Move [F]", Top = 100, Toggleable = true };
			editToolbarBehaviour.freeMoveButton = freeMoveButton;
			panel.Widgets.Add(freeMoveButton);

			var rotateButton = new TextButton { Text = "Rotate [R]", Top = 125, Toggleable = true };
			editToolbarBehaviour.rotateButton = rotateButton;
			panel.Widgets.Add(rotateButton);

			var resetRotationButton = new TextButton { Text = "Reset Rotation [Ctrl+R]", Top = 150 };
			resetRotationButton.Click += (o, e) =>
			{
				editToolbarBehaviour.ResetRotation();
			};
			panel.Widgets.Add(resetRotationButton);

			var scaleButton = new TextButton { Text = "Scale [T]", Top = 175, Toggleable = true };
			editToolbarBehaviour.scaleButton = scaleButton;
			panel.Widgets.Add(scaleButton);

			var resetScaleButton = new TextButton { Text = "Reset Scale [Ctrl+T]", Top = 200 };
			resetScaleButton.Click += (o, e) =>
			{
				editToolbarBehaviour.ResetScale();
			};
			panel.Widgets.Add(resetScaleButton);

			var editButton = new TextButton { Text = "Edit [E]", Top = 225 };
			editButton.Click += (o, e) =>
			{
				editToolbarBehaviour.Edit();
			};
			panel.Widgets.Add(editButton);

			//
			panel.Widgets.Add(new HorizontalSeparator { Top = 250, VerticalAlignment = VerticalAlignment.Top });
			//

			var resetCamera = new TextButton { Text = "Reset Camera", Top = 260 };
			resetCamera.Click += (o, e) =>
			{
				editToolbarBehaviour.ResetCamera();
			};
			panel.Widgets.Add(resetCamera);
		}
	}
}