using Godot;
using System;

namespace Game {
public partial class Match {
    public Match(Display.Board displayBoard, UI.UI ui) {
		ui.Moved += OnUIMoved;
		ui.ChangedHoverType += OnUIChangedHoverType;
		ui.PlayedFromHand += OnPlayedFromHand;
		ui.PassTurn += OnPassTurn;
    }

    	public void OnUIMoved(Vector2 newPoint, Vector2 oldPoint) {
		// Check if new position is hovering a different tile now.
		Game.Tile newTile = board.GetTileAt(ui.GetHoverCoordinate(), ui.GetHoverPartition()),
			oldHoveredTile = board.GetTileAt(ui.GetHoverCoordinate(oldPoint), ui.GetHoverPartition());
		if (newTile != oldHoveredTile) {
			ui.HoverTile(newTile);
		}
	}

	public void OnUIChangedHoverType(Vector2I hoveredTileCoordinate, int hoveredTilePartitionType) {
		ui.HoverTile(board.GetTileAt(hoveredTileCoordinate, (Game.Tile.PartitionTypeEnum) hoveredTilePartitionType));
	}

	public void OnPlayedFromHand(UI.Card card) {
		GD.Print("Card played: " + card.Name);
		displayBoard.AddPiece(
			displayBoard.GetTileAt(ui.GetHoverCoordinate(), ui.GetHoverPartition()),
			card.Stats,
			card.Illustration
		);
	}

	public void OnPassTurn() {
		board.Resolve();
	}
}
}
