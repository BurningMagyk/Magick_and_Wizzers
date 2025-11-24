using Godot;
using System;

namespace Match {
public interface ITarget {

  public Tile[] GetTiles(Tile.PartitionTypeEnum partition);
  // {
  //   return Type switch {
  //     TargetType.PIECE => Piece.Tile.GetTilesWithPartition(partition),
  //     TargetType.TILE => Tile.GetTilesWithPartition(partition),
  //     _ => [],
  //   };
  // }
}
}
