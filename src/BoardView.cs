using Godot;
using System;
using Main;

namespace UI {
	public partial class BoardView : CanvasLayer {
		public bool Showing { get; private set; }
		public Tile.PartitionType HoverPartition;
		public Tile HoveredTile { get; private set; }
		
		private Sprite2D crosshair;

		// Called when the node enters the scene tree for the first time.
		public override void _Ready() {
			crosshair = GetNode<Sprite2D>("Crosshair");
			HoverPartition = Tile.MAX_PARTITION;
			Show();
		}

		// Called every frame. 'delta' is the elapsed time since the previous frame.
		public override void _Process(double delta) {
			
		}

		public void SetViewPortRect(Rect2 viewPortRect) {
			crosshair.Position = new Vector2(viewPortRect.Size.X / 2, viewPortRect.Size.Y / 2);
		}

		public Vector2I GetHoverCoordinate(Vector2 coordinate) {
			return GetHoverCoordinate(coordinate, HoverPartition);
		}
		public Vector2I GetHoverCoordinate(Vector2 coordinate, Tile.PartitionType partitionType) {			
			return new Vector2I(
				(int) Math.Floor(coordinate.X / Tile.TILE_SIZE * Mathf.Pow(2, (int) partitionType)),
				(int) Math.Floor(coordinate.Y / Tile.TILE_SIZE * Mathf.Pow(2, (int) partitionType))
			);
		}

		public void Hover(Tile tile, bool showingHand) {
			Tile.HoverType hoverType = showingHand ? Tile.HoverType.CAST : Tile.HoverType.NORMAL;
			if (HoveredTile != null) { HoveredTile.Unhover(hoverType); }
			HoveredTile = tile;
			if (HoveredTile != null) { HoveredTile.Hover(hoverType); }
		}

		public new void Show() {
			base.Show();
			crosshair.Visible = true;
			Showing = true;
		}

		public new void Hide() {
			base.Hide();
			crosshair.Visible = false;
			Showing = false;
		}
	}
}
