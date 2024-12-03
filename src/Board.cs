using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Main {
  public partial class Board : Node {
	private const int BOARD_SIZE = 7;

	private List<Tile[,]> tiles = new List<Tile[,]>();
	private Tile hoveredTile;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		for (int i = 0; i <= (int) Tile.MAX_PARTITION; i++) {
			int boardLength = BOARD_SIZE * (int) Math.Pow(2, i);
			tiles.Add(new Tile[boardLength, boardLength]);
		}

	  	PackedScene tileScene = ResourceLoader.Load<PackedScene>("res://scenes/tile.tscn");
	  	for (int i = 0; i < BOARD_SIZE; i++) {
			for (int j = 0; j < BOARD_SIZE; j++) {
				Sprite2D tileSprite = tileScene.Instantiate() as Sprite2D;
				tileSprite.Position = new Vector2(
					Tile.TILE_SIZE / 2 + Tile.TILE_SIZE * i,
					Tile.TILE_SIZE / 2 + Tile.TILE_SIZE * j
		  		);
		  		tileSprite.Visible = true;

				string topPartitionTypeName = Util.ToTitleCase(Enum.GetNames(typeof(Tile.PartitionType))[0]);
		  		tileSprite.Name = topPartitionTypeName + " [" + i + ", " + j + "]";
			
		  		AddChild(tileSprite);
				tiles[0][i, j] = GetNode<Tile>(tileSprite.Name.ToString());
				tiles[0][i, j].Coordinate = new Vector2I(i, j);
				tiles[0][i, j].Partition(tiles.Skip(1).ToList());
			}
		}

	  // Make the base stuff invisible.
	  GetNode<Tile>("Tile").Visible = false;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta) {
	  
	}

	public Tile GetTileAt(Vector2I index, Tile.PartitionType partitionType) {
		return tiles[(int) partitionType][index.X, index.Y];
	}
  }
}
