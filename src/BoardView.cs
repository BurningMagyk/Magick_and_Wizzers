using Godot;
using Main;
using System;

namespace UI {
	public partial class BoardView : Node {
		public bool Showing { get; private set; }

		private Sprite2D crosshair;
		private Tile hoveredTile;

		// Called when the node enters the scene tree for the first time.
		public override void _Ready() {
			crosshair = GetNode<Sprite2D>("Crosshair");
			Show();
		}

		// Called every frame. 'delta' is the elapsed time since the previous frame.
		public override void _Process(double delta) {
			
		}

		public Vector2I GetHoverPoint(Camera2D camera) {
			Rect2 viewPortRect = camera.GetViewportRect();
			crosshair.Position = new Vector2(viewPortRect.Size.X / 2, viewPortRect.Size.Y / 2);

			Vector2 centerPoint = camera.GetScreenCenterPosition();
			return new Vector2I(
				(int) Math.Floor(centerPoint.X / Tile.TILE_SIZE),
				(int) Math.Floor(centerPoint.Y / Tile.TILE_SIZE)
			);
		}

		public void Hover(Tile tile, bool showingHand) {
			Tile.HoverType hoverType = showingHand ? Tile.HoverType.CAST : Tile.HoverType.NORMAL;
			if (hoveredTile != null) { hoveredTile.Unhover(hoverType); }
			hoveredTile = tile;
			if (hoveredTile != null) { hoveredTile.Hover(hoverType); }
		}

		public void Show() {
			crosshair.Visible = true;
			Showing = true;
		}

		public void Hide() {
			crosshair.Visible = false;
			Showing = false;
		}
	}
}
