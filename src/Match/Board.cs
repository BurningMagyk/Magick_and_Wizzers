using Godot;
using Main;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Match {
public partial class Board {
	public const int BOARD_SIZE = 2;
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

	public readonly List<Tile[,]> Tiles = [];
	private readonly HashSet<Piece> pieces = [];
	private readonly bool mToroidal;

	public Board(bool toroidal) {
		mToroidal = toroidal;

		for (int i = 0; i <= (int) Tile.MAX_PARTITION; i++) {
			int boardLength = BOARD_SIZE * (int) Math.Pow(2, i);
			Tiles.Add(new Tile[boardLength, boardLength]);
		}

		for (int i = 0; i < BOARD_SIZE; i++) {
			for (int j = 0; j < BOARD_SIZE; j++) {
				// null parent tile (2nd arg) will make it the largest partition (MIN_PARTITION).
				Tile tile = new(new Vector2I(i, j), null);
				tile.Partition([.. Tiles.Skip(1)]);
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
			piece.ResolveTick(this);
			// Resolve activities here too.
		}
	}

	public void FinalizeRound() {
		// Resolve end-of-round effects here.
	}

	public Piece AddPiece(Stats stats, Tile targetTile, int uniqueId) {
		Piece piece = new(mDisplayNode.CreatePiece(uniqueId + '-' + stats.Name), stats) {
			Tile = targetTile
		};
		pieces.Add(piece);
		return piece;
	}

	public Piece AddMaster(Stats stats, Tile targetTile, int uniqueId) {
		Piece master = new(mDisplayNode.CreatePiece(uniqueId + '-' + stats.Name), stats, true) {
			Tile = targetTile
		};
		pieces.Add(master);
		return master;
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

	/**
	 * Free-for-all
	*/
	public static Vector2I[] GetStartingPositions(Match.MatchType matchType, int playerCount) {
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

	public bool AreInRange(Spacial s1, Spacial s2, Command.RangeEnum range) {
		if (range == Command.RangeEnum.NOT_APPLICABLE) {
			throw new ArgumentException("Range cannot be NOT_APPLICABLE for method \"AreInRange\".");
		}
		return Command.RangeEnumToUnits(range) >= CalculateOctileDistance(s1, s2, out _);
	}

	// TODO - needs testing
	public int CalculateOctileDistance(Spacial s1, Spacial s2, out DirectionEnum directionToGo) {
		int horizontalDirectionToGo = -2; // -2 means undetermined.
		int horizontalDistance = 0;

		int s1Left = Tile.GetPositionAtSideOf(s1, DirectionEnum.WEST);
		int s2Right = Tile.GetPositionAtSideOf(s2, DirectionEnum.EAST);
		int s1Right = Tile.GetPositionAtSideOf(s1, DirectionEnum.EAST);
		int s2Left = Tile.GetPositionAtSideOf(s2, DirectionEnum.WEST);
		if (s1Right >= s2Left && s1Left <= s2Right) {
			// If the shapes overlap horizontally, no horizontal movement needed.
			horizontalDirectionToGo = (int) DirectionEnum.NONE;
			horizontalDistance = 0;
		} else if (s1Left > s2Right) {
			// s1 is to the right of s2.
			int nonToroidalDistance = s1Left - s2Right;
			if (mToroidal) {
				// Check if it's shorter to go left (west) around the toroidal board.
				int toroidalDistance = GetTotalSize() - s1Right + s2Left - 1;
				if (toroidalDistance < nonToroidalDistance) {
					horizontalDirectionToGo = (int) DirectionEnum.WEST;
					horizontalDistance = toroidalDistance;
				} else {
					horizontalDirectionToGo = (int) DirectionEnum.EAST;
					horizontalDistance = nonToroidalDistance;
				}
			} else {
				horizontalDirectionToGo = (int) DirectionEnum.EAST;
				horizontalDistance = nonToroidalDistance;
			}
		} else if (s1Right < s2Left) {
			// s1 is to the left of s2.
			int nonToroidalDistance = s2Left - s1Right;
			if (mToroidal) {
				// Check if it's shorter to go right (east) around the toroidal board.
				int toroidalDistance = GetTotalSize() - s2Right + s1Right - 1;
				if (toroidalDistance < nonToroidalDistance) {
					horizontalDirectionToGo = (int) DirectionEnum.EAST;
					horizontalDistance = toroidalDistance;
				} else {
					horizontalDirectionToGo = (int) DirectionEnum.WEST;
					horizontalDistance = nonToroidalDistance;
				}
			} else {
				horizontalDirectionToGo = (int) DirectionEnum.WEST;
				horizontalDistance = nonToroidalDistance;
			}
		}

		int verticalDirectionToGo = -2; // -2 means undetermined.
		int verticalDistance = 0;

		int s1Top = Tile.GetPositionAtSideOf(s1, DirectionEnum.NORTH);
		int s2Bottom = Tile.GetPositionAtSideOf(s2, DirectionEnum.SOUTH);
		int s1Bottom = Tile.GetPositionAtSideOf(s1, DirectionEnum.SOUTH);
		int s2Top = Tile.GetPositionAtSideOf(s2, DirectionEnum.NORTH);
		if (s1Bottom >= s2Top && s1Top <= s2Bottom) {
			// If the shapes overlap vertically, no vertical movement needed.
			verticalDirectionToGo = (int) DirectionEnum.NONE;
			verticalDistance = 0;
		} else if (s1Top > s2Bottom) {
			// s1 is below s2.
			int nonToroidalDistance = s1Top - s2Bottom;
			if (mToroidal) {
				// Check if it's shorter to go up (north) around the toroidal board.
				int toroidalDistance = GetTotalSize() - s1Bottom + s2Top - 1;
				if (toroidalDistance < nonToroidalDistance) {
					verticalDirectionToGo = (int) DirectionEnum.NORTH;
					verticalDistance = toroidalDistance;
				} else {
					verticalDirectionToGo = (int) DirectionEnum.SOUTH;
					verticalDistance = nonToroidalDistance;
				}
			} else {
				verticalDirectionToGo = (int) DirectionEnum.SOUTH;
				verticalDistance = nonToroidalDistance;
			}
		} else if (s1Bottom < s2Top) {
			// s1 is above s2.
			int nonToroidalDistance = s2Top - s1Bottom;
			if (mToroidal) {
				// Check if it's shorter to go down (south) around the toroidal board.
				int toroidalDistance = GetTotalSize() - s2Bottom + s1Top - 1;
				if (toroidalDistance < nonToroidalDistance) {
					verticalDirectionToGo = (int) DirectionEnum.SOUTH;
					verticalDistance = toroidalDistance;
				} else {
					verticalDirectionToGo = (int) DirectionEnum.NORTH;
					verticalDistance = nonToroidalDistance;
				}
			} else {
				verticalDirectionToGo = (int) DirectionEnum.NORTH;
				verticalDistance = nonToroidalDistance;
			}
		}

		if (horizontalDirectionToGo == -2 || verticalDirectionToGo == -2) {
			throw new Exception("Direction to go was not determined correctly.");
		}

		if (horizontalDirectionToGo == (int) DirectionEnum.NONE) {
			directionToGo = (DirectionEnum) verticalDirectionToGo;
			return verticalDistance * STRAIGHT_UNITS;
		} else if (verticalDirectionToGo == (int) DirectionEnum.NONE) {
			directionToGo = (DirectionEnum) horizontalDirectionToGo;
			return horizontalDistance * STRAIGHT_UNITS;
		} else {
			// Both horizontal and vertical movement needed. Calculate octile distance.
			if (horizontalDistance < verticalDistance) {
				directionToGo = Util.Combine((DirectionEnum) horizontalDirectionToGo, (DirectionEnum) verticalDirectionToGo);
				return horizontalDistance * DIAGONAL_UNITS + (verticalDistance - horizontalDistance) * STRAIGHT_UNITS;
			} else {
				directionToGo = Util.Combine((DirectionEnum) horizontalDirectionToGo, (DirectionEnum) verticalDirectionToGo);
				return verticalDistance * DIAGONAL_UNITS + (horizontalDistance - verticalDistance) * STRAIGHT_UNITS;
			}
		}
	}

	public Tile[] AStar(Command[] commands, out DirectionEnum nextMoveDirection) {
		nextMoveDirection = DirectionEnum.NONE;
		return null;
	}
}
}
