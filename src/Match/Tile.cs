using Godot;
using Main;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Match {
public class Tile {
	public const int STRT_COST = 5, DIAG_COST = 7;
	public const int PARTITION_TYPE_COUNT = 2;
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
			tiles[0, 0].Neighbors = [
				neighbors[0]?.tiles[0, 1], // North
				tiles[0, 1], // East
				tiles[1, 0], // South
				neighbors[3]?.tiles[1, 0], // West
				neighbors[1]?.tiles[1, 1], // NE
				tiles[1, 1], // SE
				neighbors[3]?.tiles[1, 1], // SW
				neighbors[7]?.tiles[1, 1] // NW
			];

			tiles[0, 1].Neighbors = [
				neighbors[0]?.tiles[1, 1], // North
				neighbors[1]?.tiles[0, 0], // East
				tiles[1, 1], // South
				tiles[0, 0], // West
				neighbors[4]?.tiles[0, 1], // NE
				neighbors[1]?.tiles[0, 1], // SE
				tiles[0, 1], // SW
				neighbors[0]?.tiles[0, 1] // NW
			];

			tiles[1, 0].Neighbors = [
				tiles[0, 0], // North
				tiles[1, 1], // East
				neighbors[2]?.tiles[0, 0], // South
				neighbors[3]?.tiles[1, 1], // West
				tiles[1, 0], // NE
				neighbors[2]?.tiles[1, 0], // SE
				neighbors[6]?.tiles[1, 0], // SW
				neighbors[3]?.tiles[1, 0] // NW
			];

			tiles[1, 1].Neighbors = [
				tiles[1, 0], // North
				neighbors[1]?.tiles[0, 1], // East
				neighbors[2]?.tiles[1, 0], // South
				tiles[0, 1], // West
				neighbors[1]?.tiles[0, 0], // NE
				neighbors[5]?.tiles[0, 0], // SE
				neighbors[2]?.tiles[0, 0], // SW
				tiles[0, 0] // NW
			];
		}
	}

	public readonly Tile ParentTile;
	private readonly Tile[,] tiles = new Tile[2, 2];

	public readonly Vector2I Coordinate;
	public readonly PartitionTypeEnum PartitionType;

	public Tile(Vector2I coordinate, PartitionTypeEnum partitionType, Tile parentTile) {
		Coordinate = coordinate;
		PartitionType = partitionType; // TODO - can calculate this without parameter now
		ParentTile = parentTile;
	}

	public void Partition(List<Tile[,]> tilesCollection) {
		int partitionLevel = (int) MAX_PARTITION - tilesCollection.Count;
		if (tilesCollection.Count == 0) { return; }

		for (int i = 0; i < 2; i++) {
			for (int j = 0; j < 2; j++) {
				PartitionTypeEnum childPartitionType = (PartitionTypeEnum) partitionLevel + 1;
				tiles[i, j] = new Tile(new Vector2I(Coordinate.X * 2 + i, Coordinate.Y * 2 + j), childPartitionType, this);
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

	public override bool Equals(object obj) {
		if (!(obj is Tile)) { // TODO - check if "is not" works instead
			return false;
		}
		Tile t = (Tile) obj;
		return Coordinate.X == t.Coordinate.X && Coordinate.Y == t.Coordinate.Y && PartitionType == t.PartitionType;
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
