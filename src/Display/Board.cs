using Godot;
using System;

namespace Display {
public partial class Board : Node3D {
  public enum HoverType { NORMAL, MOVE, INTERACT, CAST }
	
  private PackedScene mPieceScene;

  public override void _Ready() {
		mPieceScene = ResourceLoader.Load<PackedScene>("res://scenes/piece.tscn");

		// Make the base tile invisible.
		GetNode<TileWithMesh>("Tile").Visible = false;

		// Make the base piece invisible.
		Piece basePiece = GetNode<Piece>("Piece");
		basePiece.Visible = false;

		// Disable the StaticBody3D of the base piece.
	  StaticBody3D basePieceStaticBody = basePiece.GetChild<StaticBody3D>(0);
	  basePieceStaticBody.ProcessMode = ProcessModeEnum.Disabled;
	}

  public void SetRepresentedTiles(Match.Tile[,] tiles, bool useMesh) {
		PackedScene tileScene = null;
		if (useMesh) { tileScene = ResourceLoader.Load<PackedScene>("res://scenes/tile.tscn"); }
	
		for (int i = 0; i < tiles.GetLength(0); i++) {
	  	for (int j = 0; j < tiles.GetLength(1); j++) {
				ITile displayTile;

				if (tileScene != null) {
					MeshInstance3D tileMesh = tileScene.Instantiate() as MeshInstance3D;
				
				// tileMesh.Position = new Vector3(
					//   TileWithMesh.MESH_SIZE / 2 + TileWithMesh.MESH_SIZE * i,
				//   0,
				//   TileWithMesh.MESH_SIZE / 2 + TileWithMesh.MESH_SIZE * j
				// );
			
					tileMesh.Visible = true;
					tileMesh.Name = "Tile [" + i + ", " + j + "]";
					AddChild(tileMesh);

					displayTile = GetNode<TileWithMesh>(tileMesh.Name.ToString());
				
					(displayTile as TileWithMesh).UseDebugMaterial(
							(float) i / Match.Board.BOARD_SIZE,
							(float) j / Match.Board.BOARD_SIZE,
							1 - (float)(i + j) / Match.Board.BOARD_SIZE / 2
					);
				} else {
					displayTile = new TileWithoutMesh();
				}

				displayTile.Position = ITile.CalculatePosition(tiles[i, j].Coordinate, tiles[i, j].PartitionType);
				displayTile.Size = TileWithMesh.MESH_SIZE / (int) Math.Pow(2, (int) tiles[i, j].PartitionType);

				tiles[i, j].DisplayTile = displayTile;
	  	}
		}
  }

  public Piece CreatePiece(string uniqueName, Texture2D illustration) {
		MeshInstance3D pieceMesh = mPieceScene.Instantiate() as MeshInstance3D;
		StandardMaterial3D pieceMaterial = new() {
			AlbedoTexture = illustration
		};
		pieceMesh.MaterialOverride = pieceMaterial;
		pieceMesh.Name = uniqueName;
		AddChild(pieceMesh);
		return GetNode<Piece>(uniqueName);
  }
}
}
