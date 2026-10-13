using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Myra.Graphics2D.UI;
using System;
using System.Collections.Generic;

namespace VeiniaFramework.Editor
{
	public class EditorCheckboxes : Component, IDisposable
	{
		private static Panel panel = new Panel();
		private Window window;

		private static List<Keys> shortcuts = new List<Keys>();


		public override void Initialize()
		{
			window = new Window
			{
				Title = "Editor Options",
				Content = panel,
				HorizontalAlignment = HorizontalAlignment.Right,
				VerticalAlignment = VerticalAlignment.Top,
				CloseKey = Keys.None,
				DragDirection = DragDirection.None,
				Height = 70,
				Width = 200
			};
			window.CloseButton.RemoveFromParent();

			window.Show(Globals.myraDesktop, Point.Zero);
		}

		public override void Update()
		{
			window.Height = 30 * (shortcuts.Count + 1) - 5;

			for (int i = 0; i < panel.Widgets.Count; i++)
			{
				if (Globals.input.GetKeyDown(shortcuts[i]) && !EditorControls.isTextBoxFocused)
				{
					var checkBox = (CheckBox)panel.Widgets[i];
					checkBox.DoClick();
				}
			}
		}

		public static void Add(string text, bool defaultValue, EventHandler onEnable, EventHandler onDisable, Keys shortcut = 0)
		{
			var checkBox = new CheckBox
			{
				Text = text,
				IsChecked = defaultValue,
				Top = 30 * shortcuts.Count,
			};

			shortcuts.Add(shortcut);

			if (!defaultValue) onDisable?.Invoke(null, null);
			else onEnable?.Invoke(null, null);

			checkBox.PressedChanged += (o, e) =>
			{
				if (!checkBox.IsChecked) onDisable?.Invoke(o, e);
				if (checkBox.IsChecked) onEnable?.Invoke(o, e);
			};

			panel.Widgets.Add(checkBox);
		}

		public void Dispose()
		{
			panel.Widgets.Clear();
			shortcuts.Clear();
		}
	}
}