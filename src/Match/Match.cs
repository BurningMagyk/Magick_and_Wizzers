using Godot;
using System;

namespace Match {
public class Match {
    private const int STARTING_LIFE_POINTS = 8000;

    private readonly Board mBoard;
    private readonly UI.UI mUi;

    private int uniqueId = 0;

    public Match(Display.Board displayBoard, UI.UI ui, Player[] players) {
      mBoard = new Board(displayBoard);

      ui.Moved += OnUIMoved;
      ui.ChangedHoverType += OnUIChangedHoverType;
      ui.PlayedFromHand += OnPlayedFromHand;
      ui.PassRound += OnPassRound;
      mUi = ui;

      // Set up players.
      for (int i = 0; i < players.Length; i++) {
          Player player = players[i];
          player.AddMaster(GenerateDefaultMaster(player.Name, player.MasterCard));
      }
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
      mBoard.AddPiece(
          card.Stats,
          mBoard.GetTileAt(mUi.GetHoverCoordinate(), mUi.GetHoverPartition()),
          card.Illustration,
          uniqueId++
      );
    }

    public void OnPassRound() {
      mBoard.Resolve();
    }

    private Piece GenerateDefaultMaster(string playerName, Main.Card card) {
        return mBoard.AddPiece(
          card.Stats,
          mBoard.GetTileAt(1, 2, Tile.MAX_PARTITION), // use default starting positions
          card.Illustration,
          uniqueId++
      );
    }
}
}
