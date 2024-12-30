using Godot;
using System;
using UI;

namespace Main {
public partial class Main : Node {
	private Game.Board board;
	private UI.UI ui;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		board = GetNode<Game.Board>("Board");
		ui = GetNode<UI.UI>("UI");

		ui.Moved += OnUIMoved;
		ui.ChangedHoverType += OnUIChangedHoverType;
		ui.PlayedFromHand += OnPlayedFromHand;
		ui.PassTurn += OnPassTurn;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta) {

	}

	public void OnUIMoved(Vector2 newPosition, Game.Tile oldHoveredTile) {
		// Check if new position is hovering a different tile now.
		Game.Tile newTile = board.GetTileAt(ui.GetHoverCoordinate(), ui.GetHoverPartition());
		if (newTile != oldHoveredTile) {
			ui.HoverTile(newTile);
		}
	}

	public void OnUIChangedHoverType(Game.Tile hoveredTile) {
		ui.HoverTile(hoveredTile);
	}

	public void OnPlayedFromHand(Card card) {
		GD.Print("Card played: " + card.Name);
		board.AddPiece(
			board.GetTileAt(ui.GetHoverCoordinate(), ui.GetHoverPartition()),
			card.Stats,
			card.Illustration
		);
	}

	public void OnPassTurn() {
		board.Resolve();
	}
}
}
