using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Match {
public class Tile {
	public const PartitionTypeEnum MAX_PARTITION = PartitionTypeEnum.VIRGATE; // smallest
	public const PartitionTypeEnum MIN_PARTITION = PartitionTypeEnum.CARUCATE; // largest
	public enum PartitionTypeEnum { CARUCATE, VIRGATE, BOVATE, HECTARE, ACRE }

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
}
}
