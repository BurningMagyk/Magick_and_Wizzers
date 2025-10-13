using Godot;
using System;

namespace Display {
public partial class TileWithMesh : MeshInstance3D, ITile {
  public const int MESH_SIZE = 32;
  // private Match.Tile mGameTile;

  public Match.Tile GameTile { get; set; }
  public new Vector3 Position { get => base.Position; set => base.Position = value; }
  public float Size { get; set; }

  public override void _Ready() {
    Scale = new Vector3(MESH_SIZE, MESH_SIZE, 1);

    PackedScene tileScene = ResourceLoader.Load<PackedScene>("res://scenes/tile.tscn");
    MeshInstance3D tileMeshInstance = tileScene.Instantiate() as MeshInstance3D;
  }

  // Called every frame. 'delta' is the elapsed time since the previous frame.
  public override void _Process(double delta) {
  }

  public void UseDebugMaterial(float red, float green, float blue) {
    StandardMaterial3D debugMaterial = new() {
      AlbedoColor = new Color(red, green, blue)
    };
    MaterialOverride = debugMaterial;
  }
}
}
