using Godot;
using Main;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Match {
public class Tile {
	public const int STRT_COST = 5, DIAG_COST = 7;
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

	public static readonly (int dx, int dy, int cost)[] DirectionsCostMatrix = [
		( 1,  0, STRT_COST), (-1,  0, STRT_COST),
		( 0,  1, STRT_COST), ( 0, -1, STRT_COST),
		( 1,  1, DIAG_COST), ( 1, -1, DIAG_COST),
		(-1,  1, DIAG_COST), (-1, -1, DIAG_COST)
	];

	public static int Heuristic(Tile a, Tile b) {
		// Use Manhattan distance scaled to match 5/7 weighting
		int dx = Math.Abs(a.Coordinate.X - b.Coordinate.X);
		int dy = Math.Abs(a.Coordinate.Y - b.Coordinate.Y);
		int diagonal = Math.Min(dx, dy);
		int straight = Math.Abs(dx - dy);
		return diagonal * 7 + straight * 5;
	}

	public static bool IsCloserToAny(Tile candidate, Tile current, Tile[] targets) {
		foreach (var t in targets)
		{
			double distCurrent = Distance(current, t);
			double distCandidate = Distance(candidate, t);
			if (distCandidate < distCurrent)
				return true;
		}
		return false;
	}

	public static DirectionEnum DetermineDirection(Tile from, Tile to) {
		if (from.Equals(to)) {
			return DirectionEnum.NONE;
		} else if (from.Coordinate.X == to.Coordinate.X) {
			return to.Coordinate.Y > from.Coordinate.Y ? DirectionEnum.SOUTH : DirectionEnum.NORTH;
		} else if (from.Coordinate.Y == to.Coordinate.Y) {
			return to.Coordinate.X > from.Coordinate.X ? DirectionEnum.EAST : DirectionEnum.WEST;
		} else if (to.Coordinate.X > from.Coordinate.X) {
			return to.Coordinate.Y > from.Coordinate.Y ? DirectionEnum.SOUTHEAST : DirectionEnum.NORTHEAST;
		} else {
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

	private static double Distance(Tile a, Tile b) {
		int dx = a.Coordinate.X - b.Coordinate.X;
		int dy = a.Coordinate.Y - b.Coordinate.Y;
		return Math.Sqrt(dx * dx + dy * dy);
	}
}
}
