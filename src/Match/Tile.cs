using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Match {
public class Tile {
	public const PartitionTypeEnum MAX_PARTITION = PartitionTypeEnum.VIRGATE; // smallest
	public const PartitionTypeEnum MIN_PARTITION = PartitionTypeEnum.CARUCATE; // largest
	public enum PartitionTypeEnum { CARUCATE, VIRGATE, BOVATE, HECTARE, ACRE }

	public Display.Tile DisplayTile { get; private set; }
	public string Name { get; private set; }
	private Tile[,] tiles = new Tile[2, 2];

	public readonly Vector2I Coordinate;
	public readonly Vector3 DisplayPosition;
	public readonly float DisplaySize;
	public readonly PartitionTypeEnum PartitionType;

	public Tile(Vector2I coordinate, PartitionTypeEnum partitionType) {
		Coordinate = coordinate;
		DisplayPosition = Display.Tile.CalculatePosition(coordinate, partitionType);
		DisplaySize = Display.Tile.MESH_SIZE / (int) Math.Pow(2, (int) partitionType);
		PartitionType = partitionType;
	}

	public void Partition(List<Tile[,]> tilesCollection) {
		int partitionLevel = (int) MAX_PARTITION - tilesCollection.Count;
		if (tilesCollection.Count == 0) { return; }

		for (int i = 0; i < 2; i++) {
			for (int j = 0; j < 2; j++) {
				PartitionTypeEnum childPartitionType = (PartitionTypeEnum) partitionLevel + 1;
				tiles[i, j] = new Tile(new Vector2I(Coordinate.X * 2 + i, Coordinate.Y * 2 + j), childPartitionType);

				// tileMeshInstance.MaterialOverride = null;
				// tileMeshInstance.Mesh = null;
				// tileMeshInstance.RotationDegrees = new Vector3(0, 0, 0);
				// tileMeshInstance.Scale = new Vector3(0.5F, 0.5F, 1);
				// tileMeshInstance.Position = new Vector3(i * 0.5F - 0.25F, j * -0.5F + 0.25F, 0);

				tilesCollection[0][tiles[i, j].Coordinate.X, tiles[i, j].Coordinate.Y] = tiles[i, j];
				tiles[i, j].Partition(tilesCollection.Skip(1).ToList());
			}
		}
	}
}
}
