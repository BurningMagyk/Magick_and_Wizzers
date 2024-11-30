using Godot;
using System;

namespace Main {
	public partial class Main : Node {
		private Board board;
		private UI.UI ui;

		// Called when the node enters the scene tree for the first time.
		public override void _Ready() {
			board = GetNode<Board>("Board");
			ui = GetNode<UI.UI>("UI");
		}

		// Called every frame. 'delta' is the elapsed time since the previous frame.
		public override void _Process(double delta) {
			ui.HoverTile(board.GetTileAt(ui.GetHoverPoint(), ui.GetHoverPartition()));
		}
	}
}
