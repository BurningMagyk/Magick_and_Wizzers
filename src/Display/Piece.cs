using Godot;
using System;

namespace Display {
public partial class Piece : MeshInstance3D {
    public override void _Ready() {
        float size = Tile.MESH_SIZE / (float) Math.Pow(2, (int) Game.Tile.MAX_PARTITION);
		Scale = new Vector3(size, size, size);
    }

    public void Update() {
        
    }
}
}
