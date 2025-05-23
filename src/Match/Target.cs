using Godot;
using System;

namespace Match {
public class Target {
    public enum TargetType {
        SPELL,
        PIECE,
        TILE
    }
    public TargetType Type { get; private set; }
    public Spell Spell { get; private set; }
    public Piece Piece { get; private set; }
    public Tile Tile { get; private set; }

    public Target(Spell spell) {
        Type = TargetType.SPELL;
        Spell = spell;
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
