using Godot;
using System;

namespace Match {
public class Match {
	public enum MatchType {
	  FREE_FOR_ALL
	}
	private const int STARTING_LIFE_POINTS = 8000;

	public const int MAX_PLAYER_COUNT = 16;

	private readonly Board mBoard;
	private readonly UI.UI mUi;
	private readonly Player[] mPlayers;

	private int uniqueId = 0;

	public Match(Display.Board displayBoard, UI.UI ui, Player[] players) {
	  mBoard = new Board(displayBoard);

	  ui.Moved += OnUIMoved;
	  ui.ChangedHoverType += OnUIChangedHoverType;
	  ui.Played += OnPlayed;
	  ui.PassRound += OnPassRound;
	  mUi = ui;

	  mPlayers = players;
	
	  // Set up players.
	  Vector2I[] startingPositions = mBoard.GetStartingPositions(MatchType.FREE_FOR_ALL, mPlayers.Length);
	  for (int i = 0; i < mPlayers.Length; i++) {
		Player player = mPlayers[i];
		player.AddMaster(GenerateDefaultMaster(player.Name, player.MasterCard, startingPositions[i]));
	  }
	}

	public void OnUIMoved(Vector2 newPoint, Vector2 oldPoint) {
	  // I don't know why all three Tile objects here are sometimes different. Hacky way to make it work right.
	  Tile hoveredTile = mBoard.GetTileAt(mUi.GetHoverCoordinate(), mUi.GetHoverPartition()),
		newHoveredTile = mBoard.GetTileAt(mUi.GetHoverCoordinate(newPoint), mUi.GetHoverPartition()),
		oldHoveredTile = mBoard.GetTileAt(mUi.GetHoverCoordinate(oldPoint), mUi.GetHoverPartition());
	  if (newHoveredTile == null && hoveredTile == null) {
		mUi.HoverTile(null);
	  // Check if new position is hovering a different tile now.
	  } else if (hoveredTile != null && newHoveredTile != oldHoveredTile) {
		mUi.HoverTile(hoveredTile.DisplayTile);
	  }
	}

	public void OnUIChangedHoverType(Vector2I hoveredTileCoordinate, int hoveredTilePartitionType) {
	  mUi.HoverTile(
			mBoard.GetTileAt(hoveredTileCoordinate,
			(Tile.PartitionTypeEnum) hoveredTilePartitionType).DisplayTile
	  );
	}

	public void OnPlayed(UI.Card card) {
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

	private Piece GenerateDefaultMaster(string playerName, Main.Card card, Vector2I position) {
	  return mBoard.AddPiece(
		card.Stats,
		mBoard.GetTileAt(position, Tile.MAX_PARTITION), // use default starting positions
		card.Illustration,
		uniqueId++
	  );
	}
}
}
