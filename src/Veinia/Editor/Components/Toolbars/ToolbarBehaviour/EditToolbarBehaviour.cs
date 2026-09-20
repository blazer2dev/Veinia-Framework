using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Myra.Graphics2D.TextureAtlases;
using Myra.Graphics2D.UI;
using Myra.Graphics2D.UI.ColorPicker;
using Myra.Graphics2D.UI.Properties;
using System;
using System.Collections.Generic;
using System.Linq;

namespace VeiniaFramework.Editor
{
	public class EditToolbarBehaviour : ToolbarBehaviour
	{
		EditorObjectManager editorObjectManager;
		EditorControls editorControls;

		PaintingToolbar paintingToolbar;

		public List<EditorObject> selectedObjects = new List<EditorObject>();
		public EditorObject[] clipboard;

		Window editWindow;

		bool selectDragging;
		public TextButton rotateButton; // determine if IsPressed
		public TextButton scaleButton; // determine if IsPressed
		public TextButton freeMoveButton; // determine if IsPressed

		Vector2 startSelectionPos;
		Vector2 screenMousePos;
		Vector2 worldMousePos;
		Vector2 filterSelectionPoint;

		Rectangle selectionRectangle;


		public override void OnInitialize()
		{
			editorObjectManager = gameObject.level.FindComponentOfType<EditorObjectManager>();
			editorControls = gameObject.level.FindComponentOfType<EditorControls>();

			paintingToolbar = toolbar.toolbarManager.GetToolbar<PaintingToolbar>();

			EditorLabelManager.Add("SelectedObjectCount", new Label { Text = "Selected Objects - " + selectedObjects.Count, VerticalAlignment = VerticalAlignment.Top, HorizontalAlignment = HorizontalAlignment.Center, Top = 25 });

			editorObjectManager.OnRemoveAll += () => { selectedObjects.Clear(); };
		}

		public override void OnExitTab(Toolbar newToolbar)
		{
			if (paintingToolbar != null) AssignMarkLayerPrefab();
			selectedObjects.Clear();
		}

		private void AssignMarkLayerPrefab()
		{
			if (selectedObjects.Count > 0)
			{
				var paintingToolbarBehaviour = (PaintingToolbarBehaviour)paintingToolbar.toolbarBehaviour;
				paintingToolbarBehaviour.ChangeCurrentPrefab(selectedObjects[0].PrefabName);
			}
		}

		public override void OnUpdate()
		{
			screenMousePos = Globals.input.GetMouseScreenPosition();
			worldMousePos = Globals.input.GetMouseWorldPosition();


			if (Globals.input.GetMouseDown(0) && Globals.input.GetKey(Keys.LeftShift) && !Globals.myraDesktop.IsMouseOverGUI && !editorControls.isDragging && !EditorControls.disableDragMove)
			{
				selectDragging = true;
				startSelectionPos = screenMousePos;
			}

			if (Globals.input.GetKeyDown(Keys.Escape))
			{
				editWindow?.Close();
				filterSelectionWindow?.Close();
			}

			if ((Globals.input.GetMouseDown(2) || Globals.input.GetKeyDown(Keys.C)) && !EditorControls.isTextBoxFocused && !Globals.myraDesktop.IsMouseOverGUI)
			{
				if (filterSelectionWindow != null || filterSelectionPoint == default) filterSelectionPoint = screenMousePos;
				FilterSelection();
			}

			if (Globals.input.GetKeyDown(Keys.R) && !EditorControls.isTextBoxFocused)
				rotateButton.IsPressed = !rotateButton.IsPressed;

			if (Globals.input.GetKeyDown(Keys.T) && !EditorControls.isTextBoxFocused)
				scaleButton.IsPressed = !scaleButton.IsPressed;

			if (Globals.input.GetKeyDown(Keys.F) && !EditorControls.isTextBoxFocused) freeMoveButton.IsPressed = !freeMoveButton.IsPressed;
			if (freeMoveButton.IsPressed)
			{
				FreeMove();
				if (Globals.input.GetMouseUp(0)) freeMoveButton.IsPressed = false;
			}


			// selecting one thing by clicking
			if (Globals.input.GetMouseUp(0) && !selectDragging && !editorControls.isDragging && !Globals.myraDesktop.IsMouseOverGUI && !EditorControls.isMouseOverGUIPreviousFrame)
			{
				selectedObjects.Clear();
				editWindow?.Close();

				filterSelectionWindow?.Close();
				filterSelectionPoint = screenMousePos;

				EditorObject oneSelected;
				if (PaintingToolbarBehaviour.markLayer) oneSelected = editorObjectManager.PrefabOverlapsWithPoint(worldMousePos, PaintingToolbarBehaviour.currentPrefabName);
				else oneSelected = editorObjectManager.OverlapsWithPoint(worldMousePos);

				if (oneSelected != null)
				{
					selectedObjects.Add(oneSelected);
				}

				AssignMarkLayerPrefab();
			}
			// mouse up when dragging
			if (Globals.input.GetMouseUp(0) && selectDragging)
			{
				selectDragging = false;

				foreach (var item in editorObjectManager.GetInsideRectangle(selectionRectangle.AllowNegativeSize()))
				{
					if (!selectedObjects.Contains(item))
					{
						if ((PaintingToolbarBehaviour.markLayer && item.PrefabName == PaintingToolbarBehaviour.currentPrefabName))
							selectedObjects.Add(item);

						else if (!PaintingToolbarBehaviour.markLayer) selectedObjects.Add(item);

						editWindow?.Close();
						filterSelectionWindow?.Close();
					}
				}
			}

			if (Globals.input.GetKey(Keys.LeftControl) && Globals.input.GetKeyDown(Keys.D) && !EditorControls.isTextBoxFocused) Duplicate();
			if (Globals.input.GetKey(Keys.LeftAlt) && Globals.input.GetKeyDown(Keys.D) && !EditorControls.isTextBoxFocused) selectedObjects.Clear();
			if ((Globals.input.GetKeyDown(Keys.Delete) || Globals.input.GetMouseDown(1)) && !EditorControls.isTextBoxFocused && !Globals.myraDesktop.IsMouseOverGUI)
			{
				DestroySelection();
				if (filterSelectionWindow != null) filterSelectionWindow.Close();
			}

			if (!Globals.input.GetKey(Keys.LeftControl) && !EditorControls.isTextBoxFocused)
			{
				float shiftMultiplier = Globals.input.GetKey(Keys.LeftShift) ? .05f : 1f;

				if (Globals.input.GetKeyDown(Keys.W))
					foreach (var item in selectedObjects)
						item.Position += new Vector2(0, 1) * shiftMultiplier;

				if (Globals.input.GetKeyDown(Keys.S))
					foreach (var item in selectedObjects)
						item.Position += new Vector2(0, -1) * shiftMultiplier;

				if (Globals.input.GetKeyDown(Keys.A))
					foreach (var item in selectedObjects)
						item.Position += new Vector2(-1, 0) * shiftMultiplier;

				if (Globals.input.GetKeyDown(Keys.D))
					foreach (var item in selectedObjects)
						item.Position += new Vector2(1, 0) * shiftMultiplier;
			}

			if (Globals.input.GetKeyDown(Keys.E) && !EditorControls.isTextBoxFocused)
			{
				Edit();
			}

			EditorControls.disableDragMove = (rotateButton.IsPressed || scaleButton.IsPressed) && selectedObjects.Count > 0;

			if (rotateButton.IsPressed && editorControls.isDragging && Globals.input.GetMouse(0) && !Globals.input.GetKey(Keys.LeftControl))
			{
				if (Globals.input.GetKey(Keys.LeftShift) && MathF.Abs(Globals.input.mouseX) > .1f)
				{
					var amount = Globals.input.mouseX > 0 ? 22.5f : -22.5f;
					RotateSelectedAround(amount);
				}
				else if (!Globals.input.GetKey(Keys.LeftControl))
				{
					RotateSelectedAround(Globals.input.mouseX);
				}
			}
			if (Globals.input.GetKey(Keys.LeftControl) && Globals.input.GetKey(Keys.R))
			{
				ResetRotation();
			}

			if (Globals.input.GetKeyDown(Keys.Q) && selectedObjects.Count > 0)
			{
				RotateSelectedAround(-45);
			}

			if (scaleButton.IsPressed && editorControls.isDragging && Globals.input.GetMouse(0) && !Globals.input.GetKey(Keys.LeftControl))
			{
				if (Globals.input.GetKey(Keys.LeftShift) && MathF.Abs(Globals.input.mouseX) > .1f)
				{
					var amount = Globals.input.mouseX > 0 ? .5f : -.5f;
					ScaleSelectedAround(amount);
				}
				else if (!Globals.input.GetKey(Keys.LeftControl))
				{
					ScaleSelectedAround(Globals.input.mouseX);
				}
			}
			if (Globals.input.GetKey(Keys.LeftControl) && Globals.input.GetKey(Keys.T))
			{
				ResetScale();
			}

			EditorLabelManager.Add("SelectedObjectCount", new Label { Text = "Selected Objects - " + selectedObjects.Count });
		}

		private void RotateSelectedAround(float amount)
		{
			if (selectedObjects.Count == 0) return;

			var origin = new Vector2(selectedObjects.Average(x => x.Position.X), selectedObjects.Average(x => x.Position.Y));
			foreach (var item in selectedObjects)
			{
				item.Position = item.Position.RotateAroundOrigin(origin, amount);
				item.Rotation += amount;
			}
		}

		private void ScaleSelectedAround(float amount)
		{
			if (selectedObjects.Count == 0) return;

			var origin = new Vector2(
				selectedObjects.Average(x => x.Position.X),
				selectedObjects.Average(x => x.Position.Y));

			var scaleFactor = 1f + amount * 0.01f;
			scaleFactor = MathF.Max(0.01f, scaleFactor);

			foreach (var item in selectedObjects)
			{
				var offset = item.Position - origin;

				item.Position = origin + offset * scaleFactor;
				item.Scale *= scaleFactor;
			}
		}

		Window filterSelectionWindow;
		public void FilterSelection()
		{
			var overlaps = editorObjectManager.AllOverlapsWithPoint(Transform.ScreenToWorldPos(filterSelectionPoint)).ToList();

			editWindow?.Close();
			selectedObjects.Clear();

			if (filterSelectionWindow == null) // init window
			{
				filterSelectionWindow = new Window
				{
					Title = "Overlaps",
					Content = new Panel(),
				};
				filterSelectionWindow.Closed += delegate
				{
					filterSelectionWindow = null;
				};

				filterSelectionWindow.Show(Globals.myraDesktop);
			}
			else // if already created just update the content
				filterSelectionWindow.Content = new Panel();

			filterSelectionWindow.Height = 35 + 70 * overlaps.Count;
			filterSelectionWindow.Width = 100;

			var panel = (Panel)filterSelectionWindow.Content;

			int overlapButtonSize = 70;

			foreach (var overlap in overlaps)
			{
				var tex = overlap.EditorPlacedSprite.Texture;
				var overlapButton = new ImageButton
				{
					Height = overlapButtonSize,
					Width = overlapButtonSize,
					Rotation = overlap.Rotation,
					Top = overlapButtonSize * overlaps.IndexOf(overlap),
					VerticalAlignment = VerticalAlignment.Top,
					Background = new TextureRegion(tex.ChangeColor(overlap.EditorPlacedSprite.color), overlap.EditorPlacedSprite.SourceRectangle.Value),
				};

				overlapButton.MouseEntered += (s, e) => { selectedObjects.Clear(); selectedObjects.Add(overlap); };
				overlapButton.Click += (s, a) => { filterSelectionWindow.Close(); };

				panel.Widgets.Add(overlapButton);
			}
			panel.Height = overlaps.Count * overlapButtonSize;
		}

		public void DestroySelection()
		{
			foreach (var item in selectedObjects.ToArray())
			{
				selectedObjects.Remove(item);
				editorObjectManager.Remove(item);
			}
			editWindow?.Close();
			filterSelectionWindow?.Close();
		}

		public void Duplicate()
		{
			clipboard = selectedObjects.ToArray();
			selectedObjects.Clear();

			editWindow?.Close();
			filterSelectionWindow?.Close();

			foreach (var item in clipboard)
			{
				var spawnedClipboard = editorObjectManager.Spawn(item);
				selectedObjects.Add(spawnedClipboard);
			}
		}

		public void ResetRotation()
		{
			selectedObjects.ForEach(x => x.Rotation = 0);
			rotateButton.IsPressed = false;
		}

		public void ResetScale()
		{
			selectedObjects.ForEach(x => x.Scale = Vector2.One);
			scaleButton.IsPressed = false;
		}

		public void ResetCamera()
		{
			Globals.camera.SetPosition(Vector2.Zero);
			Globals.camera.Scale = 1;
		}

		public void Edit()
		{
			if (selectedObjects.Count == 1)
				EditSingle();
			else if (selectedObjects.Count > 1)
				EditMultiple();

			if (editWindow != null)
				editWindow.Closed += delegate
				{
					// destroy color picker if got invoked by editWindow
					var w = Globals.myraDesktop.GetWidgetBy(x => x is ColorPickerDialog);
					w?.RemoveFromDesktop();

					editWindow = null;
				};
		}

		public void EditSingle()
		{
			if (editWindow != null || filterSelectionWindow != null) return;

			var obj = selectedObjects[0];
			if (obj == null) return;

			editWindow = Globals.myraDesktop.MakeEditWindow(obj, allowInReleaseMode: true);
		}

		public void EditMultiple()
		{
			if (editWindow != null || filterSelectionWindow != null) return;

			var data = new MultipleEditData();
			var tolerance = 0.0001f;

			var first = selectedObjects[0];

			// prefabName simillarity
			if (selectedObjects.All(o => o.PrefabName == first.PrefabName))
				data.PrefabName = first.PrefabName;

			// position simillarity
			if (selectedObjects.All(o => Math.Abs(o.Position.X - first.Position.X) < tolerance))
				data.Position.X = first.Position.X;
			if (selectedObjects.All(o => Math.Abs(o.Position.Y - first.Position.Y) < tolerance))
				data.Position.Y = first.Position.Y;

			// z simillarity
			if (selectedObjects.All(o => Math.Abs(o.Z - first.Z) < tolerance))
				data.Z = first.Z;

			// rotation simillarity
			if (selectedObjects.All(o => Math.Abs(o.Rotation - first.Rotation) < tolerance))
				data.Rotation = first.Rotation;

			// scale simillarity
			if (selectedObjects.All(o => Math.Abs(o.Scale.X - first.Scale.X) < tolerance))
				data.Scale.X = first.Scale.X;
			if (selectedObjects.All(o => Math.Abs(o.Scale.Y - first.Scale.Y) < tolerance))
				data.Scale.Y = first.Scale.Y;

			// customData simillarity
			if (selectedObjects.All(o => o.customData == first.customData))
				data.customData = first.customData;

			// color simillarity
			if (selectedObjects.All(o => o.Color == first.Color))
				data.Color = first.Color.Value;


			editWindow = Globals.myraDesktop.MakeEditWindow(data, "Multiple Object Editor", allowInReleaseMode: true);

			PropertyGrid grid = (PropertyGrid)editWindow.Content;

			grid.PropertyChanged += delegate
			{
				foreach (var obj in selectedObjects)
				{
					obj.PrefabName = data.PrefabName == "<mixed>" ? obj.PrefabName : data.PrefabName;

					obj.Position = new Vector2(float.IsNaN(data.Position.X) ? obj.Position.X : data.Position.X,
											   float.IsNaN(data.Position.Y) ? obj.Position.Y : data.Position.Y);

					obj.Z = float.IsNaN(data.Z) ? obj.Z : data.Z;

					obj.Rotation = float.IsNaN(data.Rotation) ? obj.Rotation : data.Rotation;

					obj.Scale = new Vector2(float.IsNaN(data.Scale.X) ? obj.Scale.X : data.Scale.X,
											float.IsNaN(data.Scale.Y) ? obj.Scale.Y : data.Scale.Y);

					obj.customData = data.customData == "<mixed>" ? obj.customData : data.customData;

					obj.Color = data.Color != Color.White ? data.Color : obj.Color;
				}
			};
		}

		public void FreeMove()
		{
			if (selectedObjects.Count == 1)
			{
				selectedObjects[0].Position = worldMousePos;
			}
		}

		public override void OnDraw(SpriteBatch sb)
		{
			if (filterSelectionWindow != null)
			{
				sb.VeiniaCircle(gameObject.level, filterSelectionPoint, Color.Red, radius: .2f * Globals.camera.Scale, sides: 20, thickness: 10 * Globals.camera.Scale);
			}


			for (int i = 0; i < selectedObjects.Count; i++)
			{
				sb.VeiniaRectangleRotated(gameObject.level, selectedObjects[i].EditorPlacedSprite.rect.OffsetByHalf(), Color.Blue, thickness: 4 * Globals.camera.Scale, selectedObjects[i].Rotation);
			}


			if (selectDragging)
			{
				var difference = startSelectionPos - screenMousePos;
				selectionRectangle = new Rectangle((int)startSelectionPos.X, (int)startSelectionPos.Y, (int)-difference.X, (int)-difference.Y);

				sb.VeiniaRectangle(gameObject.level, selectionRectangle.AllowNegativeSize(), Color.Red, thickness: 10);
			}
		}
	}
}

public class MultipleEditData
{
	public Vector2 Position = new Vector2(float.NaN, float.NaN);
	public Vector2 Scale = new Vector2(float.NaN, float.NaN);
	public float Rotation = float.NaN;
	public string customData = "<mixed>";
	public string PrefabName = "<mixed>";
	public float Z = float.NaN;
	public Color Color;
}