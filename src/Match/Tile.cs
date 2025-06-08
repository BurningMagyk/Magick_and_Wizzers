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
	private Tile[,] tiles = new Tile[2, 2];

	public readonly Vector2I Coordinate;
	public readonly PartitionTypeEnum PartitionType;

	public Tile(Vector2I coordinate, PartitionTypeEnum partitionType) {
		Coordinate = coordinate;
		PartitionType = partitionType;
	}

	public void Partition(List<Tile[,]> tilesCollection) {
		int partitionLevel = (int) MAX_PARTITION - tilesCollection.Count;
		if (tilesCollection.Count == 0) { return; }

		for (int i = 0; i < 2; i++) {
			for (int j = 0; j < 2; j++) {
				PartitionTypeEnum childPartitionType = (PartitionTypeEnum) partitionLevel + 1;
				tiles[i, j] = new Tile(new Vector2I(Coordinate.X * 2 + i, Coordinate.Y * 2 + j), childPartitionType);
				tilesCollection[0][tiles[i, j].Coordinate.X, tiles[i, j].Coordinate.Y] = tiles[i, j];
				tiles[i, j].Partition(tilesCollection.Skip(1).ToList());
			}
		}
	}
}
}
