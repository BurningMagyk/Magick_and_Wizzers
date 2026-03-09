using Godot;
using Main;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Match {
public class Tile : Spacial {
	public const int PARTITION_TYPE_COUNT = 3;
	public enum PartitionTypeEnum { CARUCATE, VIRGATE, BOVATE, HECTARE, ACRE }
	public static readonly PartitionTypeEnum MIN_PARTITION =
		(PartitionTypeEnum) Enum.GetValues(typeof(PartitionTypeEnum)).GetValue(0); // largest
	public static readonly PartitionTypeEnum MAX_PARTITION =
		(PartitionTypeEnum) Enum.GetValues(typeof(PartitionTypeEnum)).GetValue(PARTITION_TYPE_COUNT - 1); // smallest

	public Display.ITile DisplayTile { get; set; }
	public string Name { get; private set; }

	private Tile[] neighbors = new Tile[8];
	public Tile[] Neighbors {
		get => neighbors;
		set {
			neighbors = value;
			if (PartitionType == MAX_PARTITION) { return; }

			// 0 - North, 1 - East, 2 - South, 3 - West, 4 - NE, 5 - SE, 6 - SW, 7 - NW
			tiles[0, 0].Neighbors = [ // Top-Left
				neighbors[(int) DirectionEnum.NORTH]?.tiles[0, 1], // North
				tiles[1, 0], // East
				tiles[0, 1], // South
				neighbors[(int) DirectionEnum.WEST]?.tiles[1, 0], // West
				neighbors[(int) DirectionEnum.NORTH]?.tiles[1, 1], // NE
				tiles[1, 1], // SE
				neighbors[(int) DirectionEnum.WEST]?.tiles[1, 1], // SW
				neighbors[(int) DirectionEnum.NORTHWEST]?.tiles[1, 1] // NW
			];

			tiles[1, 0].Neighbors = [ // Top-Right
				neighbors[(int) DirectionEnum.NORTH]?.tiles[1, 1], // North
				neighbors[(int) DirectionEnum.EAST]?.tiles[0, 0], // East
				tiles[1, 1], // South
				tiles[0, 0], // West
				neighbors[(int) DirectionEnum.NORTHEAST]?.tiles[0, 1], // NE
				tiles[0, 1], // SE
				neighbors[(int) DirectionEnum.EAST]?.tiles[0, 1], // SW
				neighbors[(int) DirectionEnum.NORTHWEST]?.tiles[0, 1] // NW
			];

			tiles[0, 1].Neighbors = [ // Bottom-Left
				tiles[0, 0], // North
				tiles[1, 1], // East
				neighbors[(int) DirectionEnum.SOUTH]?.tiles[0, 0], // South
				neighbors[(int) DirectionEnum.WEST]?.tiles[1, 1], // West
				tiles[1, 0], // NE
				neighbors[(int) DirectionEnum.SOUTH]?.tiles[1, 0], // SE
				neighbors[(int) DirectionEnum.SOUTHWEST]?.tiles[1, 0], // SW
				neighbors[(int) DirectionEnum.WEST]?.tiles[1, 1] // NW
			];

			tiles[1, 1].Neighbors = [ // Bottom-Right
				tiles[1, 0], // North
				neighbors[(int) DirectionEnum.EAST]?.tiles[0, 1], // East
				neighbors[(int) DirectionEnum.SOUTH]?.tiles[1, 0], // South
				tiles[0, 1], // West
				tiles[0, 0], // NE
				neighbors[(int) DirectionEnum.SOUTHEAST]?.tiles[0, 0], // SE
				neighbors[(int) DirectionEnum.SOUTH]?.tiles[0, 0], // SW
				neighbors[(int) DirectionEnum.WEST]?.tiles[0, 0] // NW
			];
		}
	}

	public Tile[] NeighborsOrthogonal {
		get {
			return [
				neighbors[(int) DirectionEnum.NORTH],
				neighbors[(int) DirectionEnum.EAST],
				neighbors[(int) DirectionEnum.SOUTH],
				neighbors[(int) DirectionEnum.WEST]
			];
		}
	}
	public Tile[] NeighborsDiagonal {
		get {
			return [
				neighbors[(int) DirectionEnum.NORTHEAST],
				neighbors[(int) DirectionEnum.SOUTHEAST],
				neighbors[(int) DirectionEnum.SOUTHWEST],
				neighbors[(int) DirectionEnum.NORTHWEST]
			];
		}
	}

	public readonly Tile ParentTile;
	private readonly Tile[,] tiles = new Tile[2, 2];

	public readonly Vector2I Coordinate;
	public readonly PartitionTypeEnum PartitionType;

	public Tile(Vector2I coordinate, Tile parentTile) {
		Coordinate = coordinate;
		if (parentTile == null) {
			PartitionType = MIN_PARTITION;
		} else {
			PartitionType = (PartitionTypeEnum) ((int) parentTile.PartitionType + 1);
		}
		ParentTile = parentTile;
		Name = PartitionType.ToString() + " Tile [" + Coordinate.X + ", " + Coordinate.Y + "]";
	}

	public void Partition(List<Tile[,]> tilesCollection) {
		if (tilesCollection.Count == 0) { return; }

		for (int i = 0; i < 2; i++) {
			for (int j = 0; j < 2; j++) {
				tiles[i, j] = new Tile(new Vector2I(Coordinate.X * 2 + i, Coordinate.Y * 2 + j), this);
				tilesCollection[0][tiles[i, j].Coordinate.X, tiles[i, j].Coordinate.Y] = tiles[i, j];
				tiles[i, j].Partition([.. tilesCollection.Skip(1)]);
			}
		}
	}

	public Tile[] GetTilesWithPartition(PartitionTypeEnum partitionType) {
		if (PartitionType == partitionType) {
			return [this];
		} else if (PartitionType > partitionType) {
			return ParentTile.GetTilesWithPartition(partitionType);
		} else {
			List<Tile> subTiles = [];
			foreach (Tile tile in tiles) {
				subTiles.AddRange(tile.GetTilesWithPartition(partitionType));
			}
			return [.. subTiles];
		}
	}

	public void PrintNeighbors() {
		GD.Print("Neighbors of " + Name + ":");
		for (int i = 0; i < neighbors.Length; i++) {
			if (neighbors[i] != null) {
				GD.Print("  " + ((DirectionEnum) i).ToString() + ": " + neighbors[i].Name);
			} else {
				GD.Print("  " + ((DirectionEnum) i).ToString() + ": null");
			}
		}
	}

	public Tile[] GetTiles(PartitionTypeEnum partition) {
		return GetTilesWithPartition(partition);
	}

	public Tile[] GetTiles() {
		return GetTilesWithPartition(PartitionType);
	}

	public void PrintChildren() {
		GD.Print("Children of " + Name + ":");
		for (int i = 0; i < 2; i++) {
			for (int j = 0; j < 2; j++) {
				GD.Print("  " + tiles[i, j].Name);
			}
		}
	}

	public override bool Equals(object obj) {
		if (!(obj is Tile)) { // TODO - check if "is not" works instead
			return false;
		}
		Tile t = (Tile) obj;
		return Coordinate.X == t.Coordinate.X && Coordinate.Y == t.Coordinate.Y && PartitionType == t.PartitionType;
	}

	public override string ToString() {
		return $"Tile at {Coordinate} ({PartitionType})";
	}
	public override int GetHashCode() => HashCode.Combine(Coordinate.X, Coordinate.Y, PartitionType);

	public static DirectionEnum DetermineDirection(Tile from, Tile to, bool toroidal) {
		if (from.Equals(to)) {
			return DirectionEnum.NONE;
		} else if (from.Coordinate.X == to.Coordinate.X) {
			if (toroidal && from.Neighbors[(int) DirectionEnum.NORTH].Equals(to)) {
				return DirectionEnum.NORTH;
			} else if (toroidal && from.Neighbors[(int) DirectionEnum.SOUTH].Equals(to)) {
				return DirectionEnum.SOUTH;
			}
			return to.Coordinate.Y > from.Coordinate.Y ? DirectionEnum.SOUTH : DirectionEnum.NORTH;
		} else if (from.Coordinate.Y == to.Coordinate.Y) {
			if (toroidal && from.Neighbors[(int) DirectionEnum.EAST].Equals(to)) {
				return DirectionEnum.EAST;
			} else if (toroidal && from.Neighbors[(int) DirectionEnum.WEST].Equals(to)) {
				return DirectionEnum.WEST;
			}
			return to.Coordinate.X > from.Coordinate.X ? DirectionEnum.EAST : DirectionEnum.WEST;
		} else if (to.Coordinate.X > from.Coordinate.X) {
			if (toroidal) {
				if (from.Neighbors[(int) DirectionEnum.NORTHEAST].Equals(to)) {
					return DirectionEnum.NORTHEAST;
				} else if (from.Neighbors[(int) DirectionEnum.SOUTHEAST].Equals(to)) {
					return DirectionEnum.SOUTHEAST;
				}
			}
			return to.Coordinate.Y > from.Coordinate.Y ? DirectionEnum.SOUTHEAST : DirectionEnum.NORTHEAST;
		} else {
			if (toroidal) {
				if (from.Neighbors[(int) DirectionEnum.NORTHWEST].Equals(to)) {
					return DirectionEnum.NORTHWEST;
				} else if (from.Neighbors[(int) DirectionEnum.SOUTHWEST].Equals(to)) {
					return DirectionEnum.SOUTHWEST;
				}
			}
			return to.Coordinate.Y > from.Coordinate.Y ? DirectionEnum.SOUTHWEST : DirectionEnum.NORTHWEST;
		}
  }

	public static bool IsNeighbor(Tile a, Tile b) {
		if (a.Equals(b)) {
			return false;
		} else if (Math.Abs(a.Coordinate.X - b.Coordinate.X) > 1) {
			return false;
		} else if (Math.Abs(a.Coordinate.Y - b.Coordinate.Y) > 1) {
			return false;
		} else {
			return true;
		}
	}

	public static int GetPositionAtSideOf(Spacial s, DirectionEnum direction) {
		Tile[] tiles = s.GetTiles(MAX_PARTITION);
		return direction switch {
			DirectionEnum.NORTH => tiles.Min(t => t.Coordinate.Y),
			DirectionEnum.SOUTH => tiles.Max(t => t.Coordinate.Y),
			DirectionEnum.WEST => tiles.Min(t => t.Coordinate.X),
			DirectionEnum.EAST => tiles.Max(t => t.Coordinate.X),
			_ => throw new ArgumentException("Invalid direction for GetPosAtSideOf")
		};
	}

		// TODO - needs testing
	public static int CalculateOctileDistance(
		Spacial s1,
		Spacial s2,
		int boardSize,
		bool toroidal,
		out DirectionEnum directionToGo
	) {
		int horizontalDirectionToGo = -2; // -2 means undetermined.
		int horizontalDistance = 0;

		int s1Left = GetPositionAtSideOf(s1, DirectionEnum.WEST);
		int s2Right = GetPositionAtSideOf(s2, DirectionEnum.EAST);
		int s1Right = GetPositionAtSideOf(s1, DirectionEnum.EAST);
		int s2Left = GetPositionAtSideOf(s2, DirectionEnum.WEST);
		if (s1Right >= s2Left && s1Left <= s2Right) {
			// If the shapes overlap horizontally, no horizontal movement needed.
			horizontalDirectionToGo = (int) DirectionEnum.NONE;
			horizontalDistance = 0;
		} else if (s1Left > s2Right) {
			// s1 is to the right of s2.
			int nonToroidalDistance = s1Left - s2Right;
			if (toroidal) {
				// Check if it's shorter to go left (west) around the toroidal board.
				int toroidalDistance = boardSize - s1Right + s2Left - 1;
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
			if (toroidal) {
				// Check if it's shorter to go right (east) around the toroidal board.
				int toroidalDistance = boardSize - s2Right + s1Right - 1;
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

		int s1Top = GetPositionAtSideOf(s1, DirectionEnum.NORTH);
		int s2Bottom = GetPositionAtSideOf(s2, DirectionEnum.SOUTH);
		int s1Bottom = GetPositionAtSideOf(s1, DirectionEnum.SOUTH);
		int s2Top = GetPositionAtSideOf(s2, DirectionEnum.NORTH);
		if (s1Bottom >= s2Top && s1Top <= s2Bottom) {
			// If the shapes overlap vertically, no vertical movement needed.
			verticalDirectionToGo = (int) DirectionEnum.NONE;
			verticalDistance = 0;
		} else if (s1Top > s2Bottom) {
			// s1 is below s2.
			int nonToroidalDistance = s1Top - s2Bottom;
			if (toroidal) {
				// Check if it's shorter to go up (north) around the toroidal board.
				int toroidalDistance = boardSize - s1Bottom + s2Top - 1;
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
			if (toroidal) {
				// Check if it's shorter to go down (south) around the toroidal board.
				int toroidalDistance = boardSize - s2Bottom + s1Top - 1;
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
			return verticalDistance * Board.ORTHOGONAL_UNITS;
		} else if (verticalDirectionToGo == (int) DirectionEnum.NONE) {
			directionToGo = (DirectionEnum) horizontalDirectionToGo;
			return horizontalDistance * Board.ORTHOGONAL_UNITS;
		} else {
			// Both horizontal and vertical movement needed. Calculate octile distance.
			if (horizontalDistance < verticalDistance) {
				directionToGo = Util.Combine((DirectionEnum) horizontalDirectionToGo, (DirectionEnum) verticalDirectionToGo);
				return horizontalDistance * Board.DIAGONAL_UNITS +
					(verticalDistance - horizontalDistance) * Board.ORTHOGONAL_UNITS;
			} else {
				directionToGo = Util.Combine((DirectionEnum) horizontalDirectionToGo, (DirectionEnum) verticalDirectionToGo);
				return verticalDistance * Board.DIAGONAL_UNITS +
					(horizontalDistance - verticalDistance) * Board.ORTHOGONAL_UNITS;
			}
		}
	}

	public static List<Tile> BreathFrom(Tile[] start, int range, out Tile[] periphery) {
		HashSet<Tile> visited = [.. start];
		Dictionary<Tile, int> frontier = [];
		List<Tile> peripheryList = [];
		foreach (Tile tile in start) {
			frontier[tile] = range;
		}

		while (frontier.Count > 0) {
			Dictionary<Tile, int> nextFrontier = [];

			foreach (KeyValuePair<Tile, int> kvp in frontier) {
				Tile tile = kvp.Key;
				int remainingRange = kvp.Value;

				if (remainingRange >= Board.DIAGONAL_UNITS) {
					foreach (Tile neighbor in tile.NeighborsDiagonal) {
						if (neighbor != null && !visited.Contains(neighbor)) {
							visited.Add(neighbor);
							nextFrontier[neighbor] = remainingRange - Board.DIAGONAL_UNITS;
						}
					}
				}
				if (remainingRange >= Board.ORTHOGONAL_UNITS) {
					foreach (Tile neighbor in tile.NeighborsOrthogonal) {
						if (neighbor != null && !visited.Contains(neighbor)) {
							visited.Add(neighbor);
							nextFrontier[neighbor] = remainingRange - Board.ORTHOGONAL_UNITS;
						}
					}
				} else {
					foreach (Tile neighbor in tile.Neighbors) {
						if (neighbor != null && !visited.Contains(neighbor)) {
							peripheryList.Add(neighbor);
						}
					}
				}
			}

			frontier = nextFrontier;
		}

		periphery = [.. peripheryList];
		return [.. visited];
	}

	public static Tile[] AStar(
		Tile startTile,
		Tile[] impassableTiles,
		int boardSize,
		bool toroidal,
		Func<Tile, Tile, int> travelCost,
		List<Command> commands,
		int maxDistance
	) {
		HashSet<Tile> goalTiles = [];
		HashSet<Tile> avoidedTiles = [];
		HashSet<Tile> peripheralTiles = [];

		foreach (Command command in commands) {
			if (command.Type == Command.CommandType.APPROACH || command.Type == Command.CommandType.OBSTRUCT) {
				goalTiles.UnionWith(command.TotalTiles);
				peripheralTiles.UnionWith(command.PeripheralTiles);
			} else if (command.Type == Command.CommandType.AVOID) {
				avoidedTiles.UnionWith(command.TotalTiles);
			} else {
				throw new ArgumentException("Unsupported command type for A*: " + command.Type);
			}
		}

		HashSet<Tile> invalidTiles = [.. avoidedTiles];
		invalidTiles.UnionWith(impassableTiles);

		if (avoidedTiles.Contains(startTile)) {
			// Concatenate the escape path and the normal path.
			Tile[] escapePath =
				AStar(startTile, peripheralTiles, [.. impassableTiles], boardSize, toroidal, travelCost, maxDistance);
			if (escapePath.Length == 0 || escapePath.Last().Equals(startTile) || avoidedTiles.Contains(escapePath.Last())) {
				return [];
			}
			return [
				.. escapePath,
				.. AStar(escapePath.Last(), goalTiles, invalidTiles, boardSize, toroidal, travelCost, maxDistance)
			];
		} else {
			// No escape, just normal path.
			return AStar(startTile, goalTiles, invalidTiles, boardSize, toroidal, travelCost, maxDistance);
		}
	}

	private static Tile[] AStar(
		Tile startTile,
		HashSet<Tile> goalTiles,
		HashSet<Tile> invalidTiles,
		int boardSize,
		bool toroidal,
		Func<Tile, Tile, int> travelCost,
		int maxDistance
	) {
		Dictionary<Tile, int> f = [];
		Dictionary<Tile, int> g = [];
		Dictionary<Tile, int> h = [];
		Dictionary<Tile, int> distanceFromStart = [];
		f[startTile] = 0;
		g[startTile] = 0;
		distanceFromStart[startTile] = 0;
		HashSet<Tile> open = [startTile];
		HashSet<Tile> closed = [];
		Dictionary<Tile, Tile> parent = [];

		while (open.Count > 0) {
			// Find tile in open with the lowest f-score.
			int lowestF = int.MaxValue;
			Tile q = null;
			foreach (Tile openTile in open) {
				if (!f.TryGetValue(openTile, out int fValue)) {
					fValue = goalTiles.Min(
						goalTile => CalculateOctileDistance(openTile, goalTile, boardSize, toroidal, out _)
					);
					f[openTile] = fValue;
				}
				if (fValue < lowestF) {
					lowestF = fValue;
					q = openTile;
				}
			}

			open.Remove(q);
			closed.Add(q);

			// Don't consider neighbors that are too far from start tile.
			if (distanceFromStart[q] > maxDistance) {
				continue;
			}

			// Determine q's successors.
			foreach (Tile successor in q.Neighbors) {
				if (successor == null || invalidTiles.Contains(successor)) {
					continue;
				}

				parent[successor] = q;

				if (goalTiles.Contains(successor)) {
					// We're done. Reconstruct the path.
					List<Tile> path = [successor];
					while (parent.ContainsKey(path.Last())) {
						path.Add(parent[path.Last()]);
					}
					if (path.Count > 0 && path.Last().Equals(startTile)) {
						path.RemoveAt(path.Count - 1);
						path.Reverse();
						return [.. path];
					}
					throw new Exception("A* path reconstruction failed to reach start tile.");
				}

				// Compute g, h, and f scores for successor.
				int tentativeG = g[q] + travelCost(q, successor);
				h[successor] = h.TryGetValue(successor, out int value) ? value : goalTiles.Min(
					goalTile => CalculateOctileDistance(successor, goalTile, boardSize, toroidal, out _)
				);
				int tentativeF = g[successor] + h[successor];

				// Check against open and closed lists.
				if ((open.Contains(successor) || closed.Contains(successor)) && tentativeF >= f[successor]) {
					continue;
				}

				// This path to successor is better than any previous one.
				g[successor] = tentativeG;
				f[successor] = tentativeF;
				distanceFromStart[successor] = distanceFromStart[q] + 1;
				open.Add(successor);
			}
		}

		// No path found.
		return [];
	}
}
}
