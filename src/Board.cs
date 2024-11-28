using Godot;
using System;

namespace Main {
  public partial class Board : Node {
	private const int BOARD_SIZE = 7;

	private Tile[,] tiles = new Tile[BOARD_SIZE, BOARD_SIZE];
	private Tile hoveredTile;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
	  PackedScene tileScene = ResourceLoader.Load<PackedScene>("res://scenes/tile.tscn");
	  for (int i = 0; i < BOARD_SIZE; i++) {
		for (int j = 0; j < BOARD_SIZE; j++) {
			Sprite2D tileSprite = tileScene.Instantiate() as Sprite2D;
			tileSprite.Position = new Vector2(
				Tile.TILE_SIZE / 2 + Tile.TILE_SIZE * i,
				Tile.TILE_SIZE / 2 + Tile.TILE_SIZE * j
		  	);
		  	tileSprite.Visible = true;
		  	tileSprite.Name = "Tile [" + i + ", " + j + "]";
		  	AddChild(tileSprite);
			tiles[i, j] = GetNode<Tile>(tileSprite.Name.ToString());
		}
	  }

	  // Make the base stuff invisible.
	  GetNode<Tile>("Tile").Visible = false;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta) {
	  
	}

	public Tile GetTileAt(Vector2I index) {
		return tiles[index.X, index.Y];
	}
  }
}
