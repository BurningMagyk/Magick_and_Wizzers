using Godot;
using System;

namespace Game {
public partial class Match {
	private readonly Board mBoard;
	private readonly UI.UI mUi;

    public Match(Display.Board displayBoard, UI.UI ui) {
		mBoard = new Board(displayBoard);

		ui.Moved += OnUIMoved;
		ui.ChangedHoverType += OnUIChangedHoverType;
		ui.PlayedFromHand += OnPlayedFromHand;
		ui.PassTurn += OnPassTurn;
		mUi = ui;
    }

    public void OnUIMoved(Vector2 newPoint, Vector2 oldPoint) {
		// Check if new position is hovering a different tile now.
		Tile newTile = mBoard.GetTileAt(mUi.GetHoverCoordinate(), mUi.GetHoverPartition()),
			oldHoveredTile = mBoard.GetTileAt(mUi.GetHoverCoordinate(oldPoint), mUi.GetHoverPartition());
		if (newTile != oldHoveredTile) {
			mUi.HoverTile(newTile);
		}
	}

	public void OnUIChangedHoverType(Vector2I hoveredTileCoordinate, int hoveredTilePartitionType) {
		mUi.HoverTile(mBoard.GetTileAt(hoveredTileCoordinate, (Tile.PartitionTypeEnum) hoveredTilePartitionType));
	}

	public void OnPlayedFromHand(UI.Card card) {
		GD.Print("Card played: " + card.Name);
		mBoard.AddPiece(
			new Piece(card.Stats), // Create a new piece.
			mBoard.GetTileAt(mUi.GetHoverCoordinate(), mUi.GetHoverPartition()),
			card.Illustration
		);
	}

	public void OnPassTurn() {
		mBoard.Resolve();
	}
}
}
