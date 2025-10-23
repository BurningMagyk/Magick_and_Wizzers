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

	private Display.Board mDisplayNode;
	public Display.Board DisplayNode {
		private get => mDisplayNode;
		set {
			mDisplayNode = value;
			value.SetRepresentedTiles(Tiles[0], true);
			for (int i = 1; i < Tiles.Count; i++) {
				value.SetRepresentedTiles(Tiles[i], false);
			}
		}
	}

	public readonly List<Tile[,]> Tiles = new List<Tile[,]>();
	private readonly HashSet<Piece> pieces = new HashSet<Piece>();
	private readonly bool mToroidal;

	public Board(bool toroidal) {
		mToroidal = toroidal;

		for (int i = 0; i <= (int) Tile.MAX_PARTITION; i++) {
			int boardLength = BOARD_SIZE * (int) Math.Pow(2, i);
			Tiles.Add(new Tile[boardLength, boardLength]);
		}

		for (int i = 0; i < BOARD_SIZE; i++) {
			for (int j = 0; j < BOARD_SIZE; j++) {
				Tile tile = new(new Vector2I(i, j), Tile.PartitionTypeEnum.CARUCATE, null);
				tile.Partition(Tiles.Skip(1).ToList());
				Tiles[0][i, j] = tile;
			}
		}

		// Link neighbors.
		for (int i = 0; i < BOARD_SIZE; i++) {
			for (int j = 0; j < BOARD_SIZE; j++) {
				// 0 - North, 1 - East, 2 - South, 3 - West, 4 - NE, 5 - SE, 6 - SW, 7 - NW
				Tile[] neighbors = [
					j == 0 ? null : Tiles[0][i, j - 1],
					i == BOARD_SIZE - 1 ? null : Tiles[0][i + 1, j],
					j == BOARD_SIZE - 1 ? null : Tiles[0][i, j + 1],
					i == 0 ? null : Tiles[0][i - 1, j],
					i == BOARD_SIZE - 1 || j == 0 ? null : Tiles[0][i + 1, j - 1],
					i == BOARD_SIZE - 1 || j == BOARD_SIZE - 1 ? null : Tiles[0][i + 1, j + 1],
					i == 0 || j == BOARD_SIZE - 1 ? null : Tiles[0][i - 1, j + 1],
					i == 0 || j == 0 ? null : Tiles[0][i - 1, j - 1]
				];
				if (mToroidal) {
					if (neighbors[0] == null) { // North
						neighbors[0] = Tiles[0][i, BOARD_SIZE - 1];
					}
					if (neighbors[1] == null) { // East
						neighbors[1] = Tiles[0][0, j];
					}
					if (neighbors[2] == null) { // South
						neighbors[2] = Tiles[0][i, 0];
					}
					if (neighbors[3] == null) { // West
						neighbors[3] = Tiles[0][BOARD_SIZE - 1, j];
					}
					if (neighbors[4] == null) { // NE
						neighbors[4] = Tiles[0][(i + 1) % BOARD_SIZE, (j - 1 + BOARD_SIZE) % BOARD_SIZE];
					}
					if (neighbors[5] == null) { // SE
						neighbors[5] = Tiles[0][(i + 1) % BOARD_SIZE, (j + 1) % BOARD_SIZE];
					}
					if (neighbors[6] == null) { // SW
						neighbors[6] = Tiles[0][(i - 1 + BOARD_SIZE) % BOARD_SIZE, (j + 1) % BOARD_SIZE];
					}
					if (neighbors[7] == null) { // NW
						neighbors[7] = Tiles[0][(i - 1 + BOARD_SIZE) % BOARD_SIZE, (j - 1 + BOARD_SIZE) % BOARD_SIZE];
					}
				}
				Tiles[0][i, j].Neighbors = neighbors;
			}
		}
	}

	public void ResolveTick() {
		// Vigil (eighth hour of night: 2 a.m.)
		// Matins (a later portion of Vigil, from 3 a.m. to dawn)
		// Lauds (dawn; approximately 5 a.m., but varies seasonally)
		// Prime (early morning, the first hour of daylight, approximately 6 a.m.)
		// Terce (third hour, 9 a.m.)
		// Sext (sixth hour, noon)
		// Nones (ninth hour, 3 p.m.)
		// Vespers (sunset, approximately 6 p.m.)
		// Compline (end of the day before retiring, approximately 7 p.m.)
		// Twilight (to make it 10 hours)
		foreach (Piece piece in pieces) {
			piece.ResolveTick();
			// Resolve activities here too.
		}
	}

	public Piece AddPiece(Stats stats, Tile targetTile, Texture2D illustration, int uniqueId) {
		Piece piece = new(stats, mDisplayNode.CreatePiece(uniqueId + '-' + stats.Name, illustration)) {
			Tile = targetTile
		};
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
	 * Free-for-all
	*/
	public Vector2I[] GetStartingPositions(Match.MatchType matchType, int playerCount) {
		int boardSizeTotal = BOARD_SIZE * (int) Math.Pow(2, (int) Tile.MAX_PARTITION);
		int boardSpace = boardSizeTotal * boardSizeTotal;

		if (playerCount > boardSpace) {
			throw new ArgumentOutOfRangeException(
				$"Player count must be less than or equal to the board space ({boardSpace})."
			);
		}

		if (matchType == Match.MatchType.FREE_FOR_ALL) {
			if (playerCount == 2) {
			return [
				new Vector2I(boardSizeTotal / 4, boardSizeTotal / 4),
				new Vector2I(boardSizeTotal * 3 / 4, boardSizeTotal * 3 / 4)
			];
			} else if (playerCount == 3) {
				return [
					new Vector2I(boardSizeTotal / 6, boardSizeTotal / 6),
					new Vector2I(boardSizeTotal / 2, boardSizeTotal / 2),
					new Vector2I(boardSizeTotal * 5 / 6, boardSizeTotal * 5 / 6)
				];
			} else if (playerCount == 4) {
				return [
					new Vector2I(boardSizeTotal / 4, boardSizeTotal / 4),
					new Vector2I(boardSizeTotal * 3 / 4, boardSizeTotal / 4),
					new Vector2I(boardSizeTotal / 4, boardSizeTotal * 3 / 4),
					new Vector2I(boardSizeTotal * 3 / 4, boardSizeTotal * 3 / 4)
				];
			} else if (playerCount == 5) {
				return [
					new Vector2I(boardSizeTotal / 6, boardSizeTotal / 6),
					new Vector2I(boardSizeTotal * 5 / 6, boardSizeTotal / 6),
					new Vector2I(boardSizeTotal / 2, boardSizeTotal / 2),
					new Vector2I(boardSizeTotal / 6, boardSizeTotal * 5 / 6),
					new Vector2I(boardSizeTotal * 5 / 6, boardSizeTotal * 5 / 6)
				];
			} else if (playerCount == 6) {
				return [
					new Vector2I(boardSizeTotal / 3, boardSizeTotal / 6),
					new Vector2I(boardSizeTotal * 2 / 3, boardSizeTotal / 6),
					new Vector2I(boardSizeTotal / 3, boardSizeTotal / 2),
					new Vector2I(boardSizeTotal * 2 / 3, boardSizeTotal / 2),
					new Vector2I(boardSizeTotal / 3, boardSizeTotal * 5 / 6),
					new Vector2I(boardSizeTotal * 2 / 3, boardSizeTotal * 5 / 6)
				];
			} else if (playerCount == 7) {
				return [
					new Vector2I(boardSizeTotal / 2, boardSizeTotal / 6),
					new Vector2I(boardSizeTotal * 5 / 6, boardSizeTotal / 6),
					new Vector2I(boardSizeTotal / 6, boardSizeTotal / 2),
					new Vector2I(boardSizeTotal / 2, boardSizeTotal / 2),
					new Vector2I(boardSizeTotal * 5 / 6, boardSizeTotal / 2),
					new Vector2I(boardSizeTotal / 6, boardSizeTotal * 5 / 6),
					new Vector2I(boardSizeTotal / 2, boardSizeTotal * 5 / 6)
				];
			} else if (playerCount == 8) {
				return [
					new Vector2I(boardSizeTotal / 6, boardSizeTotal / 6),
					new Vector2I(boardSizeTotal / 2, boardSizeTotal / 6),
					new Vector2I(boardSizeTotal * 5 / 6, boardSizeTotal / 6),
					new Vector2I(boardSizeTotal / 6, boardSizeTotal / 2),
					new Vector2I(boardSizeTotal * 5 / 6, boardSizeTotal / 2),
					new Vector2I(boardSizeTotal / 6, boardSizeTotal * 5 / 6),
					new Vector2I(boardSizeTotal / 2, boardSizeTotal * 5 / 6),
					new Vector2I(boardSizeTotal * 5 / 6, boardSizeTotal * 5 / 6)
				];
			} else if (playerCount == 9) {
				return [
					new Vector2I(boardSizeTotal / 6, boardSizeTotal / 6),
					new Vector2I(boardSizeTotal / 2, boardSizeTotal / 6),
					new Vector2I(boardSizeTotal * 5 / 6, boardSizeTotal / 6),
					new Vector2I(boardSizeTotal / 6, boardSizeTotal / 2),
					new Vector2I(boardSizeTotal / 2, boardSizeTotal / 2),
					new Vector2I(boardSizeTotal * 5 / 6, boardSizeTotal / 2),
					new Vector2I(boardSizeTotal / 6, boardSizeTotal * 5 / 6),
					new Vector2I(boardSizeTotal / 2, boardSizeTotal * 5 / 6),
					new Vector2I(boardSizeTotal * 5 / 6, boardSizeTotal * 5 / 6)
				];
			}
		}
		
		throw new ArgumentOutOfRangeException(
			$"Player count must not exceed {9}."
		);
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
