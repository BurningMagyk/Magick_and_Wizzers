using Godot;
using Main;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Match {
public partial class Board {
	public const int BOARD_SIZE = 7;
	public const int STRAIGHT_UNITS = 5;
	public const int DIAGONAL_UNITS = 7;
	public const int RANDOM_POSSIBLE_STARTING_POSITION_COUNT = 1000;
	private readonly Display.Board mDisplayNode;
	public readonly List<Tile[,]> Tiles = new List<Tile[,]>();
	private readonly HashSet<Piece> pieces = new HashSet<Piece>();

	public Board(Display.Board displayNode) {
		for (int i = 0; i <= (int) Tile.MAX_PARTITION; i++) {
			int boardLength = BOARD_SIZE * (int) Math.Pow(2, i);
			Tiles.Add(new Tile[boardLength, boardLength]);
		}

		for (int i = 0; i < BOARD_SIZE; i++) {
			for (int j = 0; j < BOARD_SIZE; j++) {
				Tile tile = new Tile(new Vector2I(i, j), Tile.PartitionTypeEnum.CARUCATE);
				tile.Partition(Tiles.Skip(1).ToList());
				Tiles[0][i, j] = tile;
			}
		}

		displayNode.SetRepresentedTiles(Tiles[0]);
		mDisplayNode = displayNode;
	}

	public void Resolve() {
		// Vigil (eighth hour of night: 2 a.m.)
		// Matins (a later portion of Vigil, from 3 a.m. to dawn)
		// Lauds (dawn; approximately 5 a.m., but varies seasonally)
		// Prime (early morning, the first hour of daylight, approximately 6 a.m.)
		// Terce (third hour, 9 a.m.)
		// Sext (sixth hour, noon)
		// Nones (ninth hour, 3 p.m.)
		// Vespers (sunset, approximately 6 p.m.)
		// Compline (end of the day before retiring, approximately 7 p.m.)
		foreach (Piece piece in pieces) {

		}
	}

	public Piece AddPiece(Stats stats, Tile targetTile, Texture2D illustration, int uniqueId) {
		Piece piece = new Piece(stats, mDisplayNode.CreatePiece(uniqueId + '-' + stats.Name, illustration));
		piece.Tile = targetTile;
		return piece;
	}

	public Tile GetTileAt(int xPos, int yPos, Tile.PartitionTypeEnum partitionType) {
		int boardSizeTotal = BOARD_SIZE * (int) Math.Pow(2, (int) partitionType);
		if (xPos < 0 || yPos < 0 || xPos >= boardSizeTotal || yPos >= boardSizeTotal) {
			return null;
		}
		return Tiles[(int) partitionType][xPos, yPos];
	}
	public Tile GetTileAt(Vector2I pos, Tile.PartitionTypeEnum partitionType) {
		return GetTileAt(pos.X, pos.Y, partitionType);
	}

	public int GetSize(Tile.PartitionTypeEnum partitionType) {
		return Tiles[(int) partitionType].GetLength(0);
	}

	public int GetTotalSize() {
		return GetSize(Tile.MAX_PARTITION);
	}

	public int GetDistance(Vector2I posA, Vector2I posB) {
		int size = GetTotalSize();
		int horizontalToroidalDistance = Math.Min(
			Math.Abs(posA.X - posB.X),
			size - Math.Abs(posA.X - posB.X)
		);
		int verticalToroidalDistance = Math.Min(
			Math.Abs(posA.Y - posB.Y),
			size - Math.Abs(posA.Y - posB.Y)
		);
		bool horizontalDistanceGreater = horizontalToroidalDistance > verticalToroidalDistance;
		int diagonalDistance, remainingParallelDistance;
		if (horizontalDistanceGreater) {
			diagonalDistance = verticalToroidalDistance;
			remainingParallelDistance = horizontalToroidalDistance - diagonalDistance;
		} else {
			diagonalDistance = horizontalToroidalDistance;
			remainingParallelDistance = verticalToroidalDistance - diagonalDistance;
		}
		return diagonalDistance * 10 + remainingParallelDistance * STRAIGHT_UNITS;
	}

	/**
	 * Does not find the optimally most spread out positions, because such an algorithm would be too inefficient.
	 * Instead, uses random sampling to approximate positions that are hopefully well spread out.
	*/
	public Vector2I[] GetStartingPositions(int playerCount) {
		if (playerCount < 2 || playerCount > Match.MAX_PLAYER_COUNT) {
			throw new ArgumentOutOfRangeException(
				$"Player count must be between 2 and {Match.MAX_PLAYER_COUNT}."
			);
		}

		int boardSizeTotal = BOARD_SIZE * (int) Math.Pow(2, (int) Tile.PartitionTypeEnum.CARUCATE);
		int boardSpace = boardSizeTotal * boardSizeTotal;

		if (playerCount > boardSpace) {
			throw new ArgumentOutOfRangeException(
				$"Player count must be less than or equal to {boardSpace}."
			);
		}
	
		Vector2I[] possibleStartingPositions;
		Random random = new Random();

		if (RANDOM_POSSIBLE_STARTING_POSITION_COUNT < boardSpace) {
			possibleStartingPositions = new Vector2I[RANDOM_POSSIBLE_STARTING_POSITION_COUNT];
			for (int i = 0; i < playerCount; i++) {
				int xPos = random.Next(0, boardSizeTotal);
				int yPos = random.Next(0, boardSizeTotal);
				possibleStartingPositions[i] = new Vector2I(xPos, yPos);
			}
		} else {
			possibleStartingPositions = new Vector2I[boardSpace];
			for (int i = 0; i < boardSizeTotal; i++) {
				for (int j = 0; j < boardSizeTotal; j++) {
					possibleStartingPositions[i * boardSizeTotal + j] = new Vector2I(i, j);
				}
			}
		}

		Vector2I[] startingPositions = new Vector2I[playerCount];
		startingPositions[0] = new Vector2I(random.Next(0, boardSizeTotal), random.Next(0, boardSizeTotal));
		
		for (int i = 1; i < playerCount; i++) {
			Vector2I[] startingPositionsSoFar = new Vector2I[i];
			Array.Copy(startingPositions, startingPositionsSoFar, i);
			startingPositions[i] = GetFarthestPositionFrom(startingPositionsSoFar, possibleStartingPositions);
		}

		return startingPositions;
	}

	public Vector2I GetFarthestPositionFrom(Vector2I[] positions, Vector2I[] choices) {
		int greatestDistance = 0;
		Vector2I farthestPosition = new Vector2I(0, 0);
		foreach (Vector2I choice in choices) {
			int distance = GetClosestDistanceFrom(positions, choice);
			if (distance > greatestDistance) {
				greatestDistance = distance;
				farthestPosition = choice;
			}
		}
		return farthestPosition;
	}

	public int GetClosestDistanceFrom(Vector2I[] positions, Vector2I choice) {
		int closestDistance = int.MaxValue;
		foreach (Vector2I position in positions) {
			int distance = GetDistance(position, choice);
			if (distance < closestDistance) {
				closestDistance = distance;
			}
		}
		return closestDistance;
	}
}
}
