using Godot;
using System;

namespace Display {
public partial class Piece : MeshInstance3D {
	private Match.Piece mGamePiece;
	public override void _Ready() {
		float size = Tile.MESH_SIZE / (float) Math.Pow(2, (int) Match.Tile.MAX_PARTITION);
		Scale = new Vector3(size, size, size);
	}

	public void SetGamePiece(Match.Piece piece, Main.DirectionEnum horizontal, Main.DirectionEnum vertical) {
		mGamePiece = piece;
		Name = piece.Name + " [" + horizontal + ", " + vertical + "]";
	}
}
}
