using Godot;
using System;

namespace Match {
public class Target {
    public enum TargetType {
        ACTIVITY,
        CARD,
        PIECE,
        TILE
    }
    public TargetType Type { get; private set; }
    public Activity Activity { get; private set; }
    public Piece Piece { get; private set; }
    public Tile Tile { get; private set; }

    public Target(Activity activity) {
        Type = TargetType.ACTIVITY;
        Activity = activity;
    }
    public Target(Piece piece) {
        Type = TargetType.PIECE;
        Piece = piece;
    }
    public Target(Tile tile) {
        Type = TargetType.TILE;
        Tile = tile;
    }

    public Target(TargetType type) {}
}
}
