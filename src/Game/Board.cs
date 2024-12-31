using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Game {
public partial class Board : Node3D {
	public const int BOARD_SIZE = 7;

	private readonly List<Tile[,]> tiles = new List<Tile[,]>();
	private readonly HashSet<Piece> pieces = new HashSet<Piece>();

	private PackedScene pieceScene;
	private int nextIdForPiece = 0;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		for (int i = 0; i <= (int) Tile.MAX_PARTITION; i++) {
			int boardLength = BOARD_SIZE * (int) Math.Pow(2, i);
			tiles.Add(new Tile[boardLength, boardLength]);
		}

		PackedScene tileScene = ResourceLoader.Load<PackedScene>("res://scenes/tile.tscn");
		for (int i = 0; i < BOARD_SIZE; i++) {
			for (int j = 0; j < BOARD_SIZE; j++) {
				MeshInstance3D tileSprite = tileScene.Instantiate() as MeshInstance3D;
				tileSprite.Position = new Vector3(
					Tile.TILE_SIZE / 2 + Tile.TILE_SIZE * i,
					0,
					Tile.TILE_SIZE / 2 + Tile.TILE_SIZE * j
				);
				tileSprite.Visible = true;

				string topPartitionTypeName = Main.Util.ToTitleCase(Enum.GetNames(typeof(Tile.PartitionType))[0]);
				tileSprite.Name = topPartitionTypeName + " [" + i + ", " + j + "]";
			
				AddChild(tileSprite);
				tiles[0][i, j] = GetNode<Tile>(tileSprite.Name.ToString());
				tiles[0][i, j].Coordinate = new Vector2I(i, j);
				tiles[0][i, j].Partition(tiles.Skip(1).ToList());

				tiles[0][i, j].UseDebugMaterial(
					(float) i / BOARD_SIZE,
					(float) j / BOARD_SIZE,
					1 - (float) (i + j) / BOARD_SIZE / 2
				);
			}
		}

		pieceScene = ResourceLoader.Load<PackedScene>("res://scenes/piece.tscn");

		// Make the base stuff invisible.
		GetNode<Tile>("Tile").Visible = false;
		GetNode<Piece>("Piece").Visible = false;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta) {
	
	}

	public void Resolve() {
		// Vigil (eighth hour of night: 2 a.m.)
		// Matins (a later portion of Vigil, from 3 a.m. to dawn)
		// Lauds (dawn; approximately 5 a.m., but varies seasonally)
		// Prime (early morning, the first hour of daylight, approximately 6 a.m.)
		// Terce (third hour, 9 a.m.)
		// Sext (sixth hour, noon)
		// Nones (ninth hour, 3 p.m.)
		// Vespers (sunset, approximately 6 p.m.)
		// Compline (end of the day before retiring, approximately 7 p.m.)
		foreach (Piece piece in pieces) {

		}
	}

	public void AddPiece(Tile targetTile, Main.Stats stats, Texture2D illustration) {
		// Create a new piece.
		MeshInstance3D pieceMesh = pieceScene.Instantiate() as MeshInstance3D;
		pieceMesh.Name = stats.Name + " " + nextIdForPiece++;
		StandardMaterial3D pieceMaterial = new StandardMaterial3D();
		pieceMaterial.AlbedoTexture = illustration;
		pieceMesh.MaterialOverride = pieceMaterial;
		AddChild(pieceMesh);

		// Set the piece's stats.
		Piece piece = GetNode<Piece>(pieceMesh.Name.ToString());
		piece.Stats = stats;
		piece.Tile = targetTile;
		pieces.Add(piece);
	}

	public Tile GetTileAt(Vector2I index, Tile.PartitionType partitionType) {
		int boardSizeTotal = BOARD_SIZE * (int) Math.Pow(2, (int) partitionType);
		if (index.X < 0 || index.Y < 0 || index.X >= boardSizeTotal || index.Y >= boardSizeTotal) {
			return null;
		}
		return tiles[(int) partitionType][index.X, index.Y];
	}
}
}
