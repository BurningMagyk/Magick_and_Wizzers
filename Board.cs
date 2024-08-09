using Godot;
using System;

namespace Main {
	public partial class Board : Node
	{
		private PackedScene _baseTile;
		private Sprite2D[,] tiles;

		// Called when the node enters the scene tree for the first time.
		public override void _Ready() {
			_baseTile = ResourceLoader.Load<PackedScene>("res://Tile.tscn");
			
			tiles = new Sprite2D[7, 7];
			for (int i = 0; i < tiles.GetLength(0); i++) {
				for (int j = 0; j < tiles.GetLength(1); j++) {
					Sprite2D tile = (Sprite2D) _baseTile.Instantiate();
					tile.Position = new Vector2(i * tile.Transform.Scale.X, j * tile.Transform.Scale.Y);
					tiles[i, j] = tile;
				}
			}

			// _baseTile.visible = false;
		}

		// Called every frame. 'delta' is the elapsed time since the previous frame.
		public override void _Process(double delta) {
		}

		public override void _UnhandledInput(InputEvent @event) {
			if (@event is InputEventMouseButton mouseEvent) {

			}
		}
	}
}
