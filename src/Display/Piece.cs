using Godot;
using System;

namespace Display {
public partial class Piece : MeshInstance3D {
    private Game.Piece gamePiece;
    public override void _Ready() {
        float size = Tile.MESH_SIZE / (float) Math.Pow(2, (int) Game.Tile.MAX_PARTITION);
		Scale = new Vector3(size, size, size);
    }

    public void SetGamePiece(
        Game.Piece piece,
		Main.DirectionEnum horizontal,
		Main.DirectionEnum vertical) {
		
		gamePiece = piece;
		Name = piece.Name + " [" + horizontal + ", " + vertical + "]";

		Update();
    }

    public void Update() {
        
    }
}
}
