using Godot;
using System;

namespace Match {
public class Target {
  public enum TargetType {
    ACTIVITY,
    CARD,
    PIECE,
    TILE,
    ITEM
  }
  public TargetType Type { get; private set; }
  public Activity Activity { get; private set; }
  public Main.Card Card { get; private set; }
  public Piece Piece { get; private set; }
  public Tile Tile { get; private set; }
  public string Item { get; private set; } // Placeholder for now.

  public Target(Activity activity) {
    Type = TargetType.ACTIVITY;
    Activity = activity;
  }
  public Target(Main.Card card) {
    Type = TargetType.CARD;
    Card = card;
  }
  public Target(Piece piece) {
    Type = TargetType.PIECE;
    Piece = piece;
  }
  public Target(Tile tile) {
    Type = TargetType.TILE;
    Tile = tile;
  }
  public Target(string item) {
    Type = TargetType.ITEM;
    Item = item;
  }

  public Tile[] GetTiles(Tile.PartitionTypeEnum partition) {
    return Type switch {
      TargetType.PIECE => Piece.Tile.GetTilesWithPartition(partition),
      TargetType.TILE => Tile.GetTilesWithPartition(partition),
      _ => [],
    };
  }

  public override string ToString() {
    if (Type == TargetType.ACTIVITY) {
      return "an activity";
    } else if (Type == TargetType.CARD) {
      return $"Card: {Card.Name}";
    } else if (Type == TargetType.PIECE) {
      return $"Piece: {Piece.Name}";
    } else if (Type == TargetType.TILE) {
      return $"Tile at {Tile.Coordinate}";
    } else if (Type == TargetType.ITEM) {
      return $"Item: {Item}";
    } else {
      return "Unknown Target";
    }
  }
}
}
