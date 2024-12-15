using Godot;
using System;
using Main;

namespace UI {
	public partial class BoardView2D : Node {
		public bool Showing { get; private set; }
		public Tile2D.PartitionType HoverPartition;
		public Tile2D HoveredTile { get; private set;}
		
		private Sprite2D crosshair;

		// Called when the node enters the scene tree for the first time.
		public override void _Ready() {
			crosshair = GetNode<Sprite2D>("Crosshair");
			HoverPartition = Tile2D.MAX_PARTITION;
			Show();
		}

		// Called every frame. 'delta' is the elapsed time since the previous frame.
		public override void _Process(double delta) {
			
		}

		public Vector2I GetHoverPoint(Camera2D camera) {
			return GetHoverPoint(camera, HoverPartition);
		}
		public Vector2I GetHoverPoint(Camera2D camera, Tile2D.PartitionType partitionType) {
			Rect2 viewPortRect = camera.GetViewportRect();
			crosshair.Position = new Vector2(viewPortRect.Size.X / 2, viewPortRect.Size.Y / 2);

			Vector2 centerPoint = camera.GetScreenCenterPosition();
			return new Vector2I(
				(int) Math.Floor(centerPoint.X / Tile2D.TILE_SIZE * Mathf.Pow(2, (int) partitionType)),
				(int) Math.Floor(centerPoint.Y / Tile2D.TILE_SIZE * Mathf.Pow(2, (int) partitionType))
			);
		}

		public void Hover(Tile2D tile, bool showingHand) {
			Tile2D.HoverType hoverType = showingHand ? Tile2D.HoverType.CAST : Tile2D.HoverType.NORMAL;
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
