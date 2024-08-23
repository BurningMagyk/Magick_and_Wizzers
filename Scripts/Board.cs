using Godot;
using System;

namespace Main {
	public partial class Board : Node
	{
		private const int BOARD_SIZE = 7, TILE_SIZE = 128;

		private Sprite2D[,] tiles = new Sprite2D[BOARD_SIZE, BOARD_SIZE];

		// Called when the node enters the scene tree for the first time.
		public override void _Ready() {
			for (int i = 0; i < BOARD_SIZE; i++) {
				for (int j = 0; j < BOARD_SIZE; j++) {
					tiles[i, j] = GetNode<Sprite2D>("Tile [" + i + ", " + j + "]");
					tiles[i, j].Position = new Vector2(
						TILE_SIZE / 2 + TILE_SIZE * i,
						TILE_SIZE / 2 + TILE_SIZE * j
					);
				}
			}
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
