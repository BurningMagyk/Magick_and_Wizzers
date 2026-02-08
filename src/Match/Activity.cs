using Godot;
using System;

namespace Match {
public class Activity : Spacial {

  public override string ToString() {
    return "an activity";
  }

  public Tile[] GetTiles(Tile.PartitionTypeEnum partition) {
    return null;
  }
}
}
