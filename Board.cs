using Godot;
using System;

namespace Main {
	public partial class Board : Node
	{
		private Sprite2D _baseTile;

		// Called when the node enters the scene tree for the first time.
		public override void _Ready() {
			_baseTile = GetNode<Sprite2D>("Tile");
			_baseTile.scri
			// GD.Load()
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
