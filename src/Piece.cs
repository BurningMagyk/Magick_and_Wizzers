using Godot;
using System;

namespace Main {
  public partial class Piece : MeshInstance3D
  {
	public Tile Tile {
		get => tile;
		set {
		  tile = value;
		  Position = tile.GlobalPosition;
		  // Scale = value.GlobalScale;
		}
	  }
	  public Stats Stats { get; set; }

	  private Tile tile;

	  // Called when the node enters the scene tree for the first time.
	  public override void _Ready() {
	  }

	  // Called every frame. 'delta' is the elapsed time since the previous frame.
	  public override void _Process(double delta) {
	  }
  }
}
