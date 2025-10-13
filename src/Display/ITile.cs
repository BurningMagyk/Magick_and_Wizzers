using Godot;
using System;

namespace Display {
public interface ITile {
  Match.Tile GameTile { get; set; }
  Vector3 Position { get; set; }
  float Size { get; set; }
  public static Vector3 CalculatePosition(Vector2I coordinate, Match.Tile.PartitionTypeEnum partitionType) {
    int size = TileWithMesh.MESH_SIZE / (int) Math.Pow(2, (int) partitionType);
    return new Vector3(size / 2 + size * coordinate.X, 0, size / 2 + size * coordinate.Y);
  }
}
}
