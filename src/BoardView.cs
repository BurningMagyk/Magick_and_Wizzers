using Godot;
using System;
using Main;

namespace UI {
	public partial class BoardView : Node {
		public bool Showing { get; private set; }
		public Tile.PartitionType HoverPartition;
		public Tile HoveredTile { get; private set;}
		
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

		public Vector2I GetHoverPoint(Camera2D camera) {
			return GetHoverPoint(camera, HoverPartition);
		}
		public Vector2I GetHoverPoint(Camera2D camera, Tile.PartitionType partitionType) {
			Rect2 viewPortRect = camera.GetViewportRect();
			crosshair.Position = new Vector2(viewPortRect.Size.X / 2, viewPortRect.Size.Y / 2);

			Vector2 centerPoint = camera.GetScreenCenterPosition();
			return new Vector2I(
				(int) Math.Floor(centerPoint.X / Tile.TILE_SIZE * Mathf.Pow(2, (int) partitionType)),
				(int) Math.Floor(centerPoint.Y / Tile.TILE_SIZE * Mathf.Pow(2, (int) partitionType))
			);
		}

		public void Hover(Tile tile, bool showingHand) {
			Tile.HoverType hoverType = showingHand ? Tile.HoverType.CAST : Tile.HoverType.NORMAL;
			if (HoveredTile != null) { HoveredTile.Unhover(hoverType); }
			HoveredTile = tile;
			if (HoveredTile != null) { HoveredTile.Hover(hoverType); }
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
