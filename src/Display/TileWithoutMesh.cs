using Godot;
using System;

namespace Display {
public partial class TileWithoutMesh : ITile {
  public Match.Tile GameTile { get; set; }
  public Vector3 Position { get; set; }
  public float Size { get; set; }
}
}
