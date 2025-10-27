using Godot;
using Main;
using System;
using System.Collections.Generic;

namespace Match {
public class Piece {
	public enum SizeEnum { TINY, SMALL, MEDIUM, LARGE, HUGE, GARGANTUAN, COLOSSAL }
  private static int sNextIdForPiece = 0;

  public Tile Tile {
	  get => tile;
	  set {
		  tile = value;
			prevTile = value;
		  float size = value.DisplayTile.Size;
		  mDisplayNode.Position = value.DisplayTile.Position + new Vector3(0, size, 0);
		  mDisplayNode.Scale = new Vector3(size, size, size);
	  }
  }
  public Main.Stats Stats { get; set; }
  public string Name { get; private set;}
  public Player MasteringPlayer { get; set; }
	public SizeEnum Size { get; set; }

  private readonly Display.Piece mDisplayNode;
  private Tile tile, prevTile;
  public Command.CommandType[] CommandTypes { get; private set; }
  public Command Command { get; set; }
	public bool Toroidal { get; set;}

  // Called when the node enters the scene tree for the first time.
  public Piece(Stats stats, Display.Piece displayNode) {
		Name = stats.Name + " " + sNextIdForPiece++;
		mDisplayNode = displayNode;
		displayNode.SetGamePiece(this, 0, 0); // Just one central game piece for now.

		// Command stuff should come from the stats. Use defaults for now.
		CommandTypes = [
			Command.CommandType.APPROACH,
	  	Command.CommandType.AVOID,
	  	Command.CommandType.INTERCEPT
		];
  }

	public void ResolveTick(Board board) {
		FollowCommand(board.Tiles[(int) GetPartitionType(Size)]);
		// Apply over-time effects here too.
	}

  private void FollowCommand(Tile[,] grid) {
		if (Command == null) {
			return;
		}

		if (Command.Type == Command.CommandType.APPROACH) {
			if (Command.GetTargets().Length == 0) {
				return;
			}

			// Get all target tiles.
			List<Tile> targetTiles = [];
			foreach (Target target in Command.GetTargets()) {
				Tile[] tilesFromTarget = target.GetTiles(GetPartitionType(Size));
				foreach (Tile tile in tilesFromTarget) {
					if (!targetTiles.Contains(tile)) {
						targetTiles.Add(tile);
					}
				}
			}

			// Check that all target tiles are the correct partition type.
			Tile.PartitionTypeEnum requiredPartition = GetPartitionType(Size);
			foreach (Tile tile in targetTiles) {
				if (tile.PartitionType != requiredPartition) {
					throw new Exception($"Target tile {tile.Name} does not match piece partition type {requiredPartition}.");
				}
			}

			// Determine which target is closest to this piece using A*.
			

		}
  }

	public static Tile.PartitionTypeEnum GetPartitionType(SizeEnum size) {
		switch (size) {
			case SizeEnum.TINY:
			case SizeEnum.SMALL:
			case SizeEnum.MEDIUM:
				return Tile.MAX_PARTITION;
			case SizeEnum.LARGE:
			case SizeEnum.HUGE:
			case SizeEnum.GARGANTUAN:
			case SizeEnum.COLOSSAL:
				int sizeDown = Math.Min(SizeEnum.COLOSSAL - size, Tile.PARTITION_TYPE_COUNT - 1);
				return (Tile.PartitionTypeEnum) ((int) Tile.MIN_PARTITION + sizeDown);
			default:
				throw new ArgumentOutOfRangeException(nameof(size), size, null);
		};
	}
}
}
