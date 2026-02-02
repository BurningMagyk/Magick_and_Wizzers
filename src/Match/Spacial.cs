using Godot;
using System;

namespace Match {
public interface Spacial {
  public Tile[] GetTiles(Tile.PartitionTypeEnum partition);
}
}
