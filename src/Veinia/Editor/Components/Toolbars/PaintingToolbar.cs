using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Myra.Graphics2D;
using Myra.Graphics2D.TextureAtlases;
using Myra.Graphics2D.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace VeiniaFramework.Editor
{
	public class PaintingToolbar : Toolbar
	{
		public static int columns = 1;
		public static int padding = 0;

		PrefabManager prefabManager;
		PaintingToolbarBehaviour paintingToolbarBehaviour;

		List<PaintingToolbarTab> paintingToolbarTabs = new List<PaintingToolbarTab>();


		public PaintingToolbar(string toolbarName, ToolbarBehaviour toolbarBehaviour, PrefabManager prefabManager) : base(toolbarName, toolbarBehaviour) => this.prefabManager = prefabManager;

		public override void OnInitialize(GameObject gameObject)
		{
			paintingToolbarBehaviour = (PaintingToolbarBehaviour)toolbarBehaviour;

			//create needed tabs
			for (int i = 0; i < prefabManager.editorPrefabs.Max(x => x.PaintingToolbarTab) + 1; i++)
			{
				var newTab = new PaintingToolbarTab();
				newTab.Scroll.Content = newTab.Panel;
				paintingToolbarTabs.Add(newTab);
			}
			//

			FeedToolbarWithPrefabs();

			var tabControl = new TabControl { TabSelectorPosition = TabSelectorPosition.Right };
			for (int i = 0; i < paintingToolbarTabs.Count; i++)
				tabControl.Items.Add(new TabItem { Content = paintingToolbarTabs[i].Scroll, Text = i.ToString() });

			displayedToolbarContent = tabControl;

			ShowPrefabsInToolbars();
		}

		private void OnClickPrefab(Prefab prefab)
		{
			paintingToolbarBehaviour.ChangeCurrentPrefab(prefab.PrefabName);
			paintingToolbarBehaviour.CreateNewPreview();
		}


		private void FeedToolbarWithPrefabs()
		{
			for (int i = 0; i < prefabManager.editorPrefabs.Count; i++)
			{
				var prefab = prefabManager.editorPrefabs[i];
				paintingToolbarTabs[prefab.PaintingToolbarTab].Prefabs.Add(prefab);
			}
		}

		private void ShowPrefabsInToolbars()
		{
			int prefabButtonSize = 70;

			foreach (var tab in paintingToolbarTabs)
			{
				foreach (var prefab in tab.Prefabs)
				{
					var sprite = prefab.PrefabGameObject.GetComponent<Sprite>();
					if (sprite == null)
					{
						var texture = Globals.content.Load<Texture2D>(Path.Combine("veinia_defaults", "prefab_default"));
						sprite = new Sprite(texture);
					}

					var index = tab.Prefabs.IndexOf(prefab);
					var top = prefabButtonSize * (index / columns);
					var left = prefabButtonSize * (index % columns);

					var prefabButton = new ImageButton
					{
						Height = prefabButtonSize,
						Width = prefabButtonSize,
						Top = top,
						Left = left,
						Padding = new Thickness(-padding),
						VerticalAlignment = VerticalAlignment.Top,
						Background = new TextureRegion(sprite.Texture.ChangeColor(sprite.color), sprite.SourceRectangle.Value),
					};
					prefabButton.Click += (s, a) => OnClickPrefab(prefab);
					tab.Panel.Widgets.Add(prefabButton);

					if (prefab.ShowLabel)
					{
						var prefabTextOutline = new Label
						{
							Text = prefab.PrefabName,
							Top = top,
							Left = left,
							TextColor = Color.Black,
							MaxWidth = prefabButtonSize,
							Wrap = true
						};
						prefabTextOutline.TouchDown += (s, a) => OnClickPrefab(prefab);
						tab.Panel.Widgets.Add(prefabTextOutline);

						var prefabText = new Label
						{
							Text = prefab.PrefabName,
							Top = top - 1,
							Left = left - 1,
							MaxWidth = prefabButtonSize,
							Wrap = true
						};
						prefabText.TouchDown += (s, a) => OnClickPrefab(prefab);
						tab.Panel.Widgets.Add(prefabText);
					}
				}
				tab.Panel.Height = prefabButtonSize * ((tab.Prefabs.Count + columns - 1) / columns);
				tab.Panel.Width = prefabButtonSize * Math.Min(tab.Prefabs.Count, columns);
			}
		}
	}

	public class PaintingToolbarPrefab
	{
		public string PrefabName { get; set; }
		public Texture2D Texture { get; set; }
		public Color Color { get; set; }
	}

	public class PaintingToolbarTab
	{
		public List<Prefab> Prefabs = new List<Prefab>();
		public Panel Panel { get; set; } = new Panel();
		public ScrollViewer Scroll { get; set; } = new ScrollViewer();
	}
}
