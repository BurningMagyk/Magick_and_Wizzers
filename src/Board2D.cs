using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Main {
  public partial class Board2D : Node {
	private const int BOARD_SIZE = 7;

	private readonly List<Tile2D[,]> tiles = new List<Tile2D[,]>();
	private readonly HashSet<Piece2D> pieces = new HashSet<Piece2D>();

	private PackedScene pieceScene;
	private Tile2D hoveredTile;
	private int pieceId = 0;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		for (int i = 0; i <= (int) Tile2D.MAX_PARTITION; i++) {
			int boardLength = BOARD_SIZE * (int) Math.Pow(2, i);
			tiles.Add(new Tile2D[boardLength, boardLength]);
		}

	  	PackedScene tileScene = ResourceLoader.Load<PackedScene>("res://scenes/tile2d.tscn");
	  	for (int i = 0; i < BOARD_SIZE; i++) {
			for (int j = 0; j < BOARD_SIZE; j++) {
				Sprite2D tileSprite = tileScene.Instantiate() as Sprite2D;
				tileSprite.Position = new Vector2(
					Tile2D.TILE_SIZE / 2 + Tile2D.TILE_SIZE * i,
					Tile2D.TILE_SIZE / 2 + Tile2D.TILE_SIZE * j
		  		);
		  		tileSprite.Visible = true;

				string topPartitionTypeName = Util.ToTitleCase(Enum.GetNames(typeof(Tile2D.PartitionType))[0]);
		  		tileSprite.Name = topPartitionTypeName + " [" + i + ", " + j + "]";
			
		  		AddChild(tileSprite);
				tiles[0][i, j] = GetNode<Tile2D>(tileSprite.Name.ToString());
				tiles[0][i, j].Coordinate = new Vector2I(i, j);
				tiles[0][i, j].Partition(tiles.Skip(1).ToList());
			}
		}

		pieceScene = ResourceLoader.Load<PackedScene>("res://scenes/piece.tscn");

		// Make the base stuff invisible.
		GetNode<Tile2D>("Tile").Visible = false;
		GetNode<Piece2D>("Piece").Visible = false;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta) {
	  
	}

	public void AddPiece(Tile2D targetTile, Stats stats, Texture2D illustration) {
		// Create a new piece.
		Sprite2D pieceSprite = pieceScene.Instantiate() as Sprite2D;
		pieceSprite.Name = stats.Name + " " + pieceId++;
		pieceSprite.Texture = illustration;
		AddChild(pieceSprite);

		// Set the piece's stats.
		Piece2D piece = GetNode<Piece2D>(pieceSprite.Name.ToString());
		piece.Stats = stats;
		piece.Tile = targetTile;
		pieces.Add(piece);
	}

	public Tile2D GetTileAt(Vector2I index, Tile2D.PartitionType partitionType) {
		return tiles[(int) partitionType][index.X, index.Y];
	}
  }
}
