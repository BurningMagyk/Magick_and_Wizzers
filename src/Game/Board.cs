using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Game {
public partial class Board {
	public const int BOARD_SIZE = 7;
	private readonly Display.Board displayNode;
	public readonly List<Tile[,]> Tiles = new List<Tile[,]>();
	private readonly HashSet<Piece> pieces = new HashSet<Piece>();

	public Board(Display.Board displayNode) {
		for (int i = 0; i <= (int) Tile.MAX_PARTITION; i++) {
			int boardLength = BOARD_SIZE * (int) Math.Pow(2, i);
			Tiles.Add(new Tile[boardLength, boardLength]);
		}

		for (int i = 0; i < BOARD_SIZE; i++) {
			for (int j = 0; j < BOARD_SIZE; j++) {
				Tile tile = new Tile(i, j);
				tile.Partition(Tiles.Skip(1).ToList());
				Tiles[0][i, j] = tile;
			}
		}

		this.displayNode = displayNode;
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
		Piece piece = new Piece(stats);
		piece.Tile = targetTile;
		displayNode.AddPiece(piece, illustration);
	}

	public Tile GetTileAt(Vector2I index, Tile.PartitionTypeEnum partitionType) {
		int boardSizeTotal = BOARD_SIZE * (int) Math.Pow(2, (int) partitionType);
		if (index.X < 0 || index.Y < 0 || index.X >= boardSizeTotal || index.Y >= boardSizeTotal) {
			return null;
		}
		return Tiles[(int) partitionType][index.X, index.Y];
	}
}
}
