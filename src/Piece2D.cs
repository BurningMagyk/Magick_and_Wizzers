using Godot;
using System;

namespace Main {
  public partial class Piece2D : Sprite2D {
    public Tile2D Tile {
      get => tile;
      set {
        tile = value;
        Position = tile.GlobalPosition;
        Scale = value.GlobalScale * value.TextureSize / Texture.GetSize();
      }
    }
    public Stats Stats { get; set; }

    private Tile2D tile;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready() {
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta) {
    }
  }
}
