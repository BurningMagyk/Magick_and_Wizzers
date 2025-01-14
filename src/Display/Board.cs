using Godot;
using System;

namespace Display {
public partial class Board : Node3D {
	public enum HoverType { NORMAL, MOVE, INTERACT, CAST }
	
	private PackedScene mPieceScene;

	public override void _Ready() {
		mPieceScene = ResourceLoader.Load<PackedScene>("res://scenes/piece.tscn");

		// Make the base stuff invisible.
		GetNode<Tile>("Tile").Visible = false;
		GetNode<Piece>("Piece").Visible = false;
	}

	public void SetRepresentedTiles(Game.Tile[,] tiles) {		
		PackedScene tileScene = ResourceLoader.Load<PackedScene>("res://scenes/tile.tscn");
		for (int i = 0; i < tiles.GetLength(0); i++) {
			for (int j = 0; j < tiles.GetLength(1); j++) {
				MeshInstance3D tileMesh = tileScene.Instantiate() as MeshInstance3D;
				tileMesh.Position = new Vector3(
					Tile.MESH_SIZE / 2 + Tile.MESH_SIZE * i,
					0,
					Tile.MESH_SIZE / 2 + Tile.MESH_SIZE * j
				);
				tileMesh.Visible = true;
				tileMesh.Name = "Tile [" + i + ", " + j + "]";
				AddChild(tileMesh);

				GetNode<Tile>(tileMesh.Name.ToString()).UseDebugMaterial(
					(float) i / Game.Board.BOARD_SIZE,
					(float) j / Game.Board.BOARD_SIZE,
					1 - (float) (i + j) / Game.Board.BOARD_SIZE / 2
				);
			}
		}
	}

	public Piece CreatePiece(String uniqueName, Texture2D illustration) {
		MeshInstance3D pieceMesh = mPieceScene.Instantiate() as MeshInstance3D;
		StandardMaterial3D pieceMaterial = new StandardMaterial3D();
		pieceMaterial.AlbedoTexture = illustration;
		pieceMesh.MaterialOverride = pieceMaterial;
		pieceMesh.Name = uniqueName;
		AddChild(pieceMesh);
		return GetNode<Piece>(uniqueName);
	}
}
}
