using Godot;
using System;

namespace Display {
public partial class Piece : MeshInstance3D {
  private Match.Piece mGamePiece;

  private StandardMaterial3D[] mColorizedMaterials;

  public override void _Ready() {
	  float size = TileWithMesh.MESH_SIZE / (float) Math.Pow(2, (int) Match.Tile.MAX_PARTITION);
	  Scale = new Vector3(size, size, size);

	  // Set up colorized materials.
	  mColorizedMaterials = new StandardMaterial3D[(int) ColorizeEnum.COUNT];
	  mColorizedMaterials[(int) ColorizeEnum.HOVER] = new () {
		  AlbedoColor = new Color(1f, 0.75f, 0.75f)
	  };
	  mColorizedMaterials[(int) ColorizeEnum.SELECT] = new () {
		  AlbedoColor = new Color(1f, 1f, 0.75f)
	  };

	  // Initial colorized material.
	  Colorize(ColorizeEnum.NONE);
  }

  public void SetGamePiece(Match.Piece piece, Main.DirectionEnum horizontal, Main.DirectionEnum vertical) {
	  mGamePiece = piece;
	  Name = piece.Name + " [" + horizontal + ", " + vertical + "]";

	  // Set up regular material from piece.
	  mColorizedMaterials[(int) ColorizeEnum.NONE] = new StandardMaterial3D() {
		  AlbedoColor = new Color(0.75f, 0.75f, 0.75f)
	  };
  }

  public void Colorize(ColorizeEnum colorizeEnum) {
	  MaterialOverride = mColorizedMaterials[(int) colorizeEnum];
  }
}
}
