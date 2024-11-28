using Godot;
using System;

namespace UI {
	public partial class BoardView : Node {
		public bool Showing { get; private set; }

		private Sprite2D crosshair;

		// Called when the node enters the scene tree for the first time.
		public override void _Ready() {
			crosshair = GetNode<Sprite2D>("Crosshair");
			Show();
		}

		// Called every frame. 'delta' is the elapsed time since the previous frame.
		public override void _Process(double delta) {
		}

		public void Show() {
			crosshair.Visible = true;
			Showing = true;
		}

		public void Hide() {
			crosshair.Visible = false;
			Showing = false;
		}

		public void SetViewPortRect(Rect2 viewPortRect) {
			crosshair.Position = new Vector2(viewPortRect.Size.X / 2, viewPortRect.Size.Y / 2);
		}
	}
}
