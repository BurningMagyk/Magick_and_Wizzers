using Godot;
using Main;
using System;
using System.Collections.Generic;

namespace Match {
public class Piece : Spacial {
	public enum SizeEnum { TINY, SMALL, MEDIUM, LARGE, HUGE, GARGANTUAN, COLOSSAL }
	public enum SpeedEnum { NONE, SLOW, WALK, RUN, FLY, EXTRA, SHOT }
  private static int sNextIdForPiece = 0;

	public Command[] Commands { get => mCommands; private set => mCommands = value; }
  public Stats Stats { get; set; }
  public string Name { get; private set; }
  public Player ControllingPlayer { get; set; }
	public bool IsMaster { get; private set; } = false;

	public SizeEnum Size { get => mMovement.Size; set => mMovement.Size = value; }
	public Tile Tile {
		get => mMovement.CurrTile;
		set {
			if (mMovement.CurrTile == null) {
				// Set display node position when the tile is first set for this piece.
				mDisplayNode.Position = value.DisplayTile.Position;
			}

			mMovement.CurrTile = value;
		}
	}

  private readonly Display.Piece mDisplayNode;
	private readonly Movement mMovement = new();
  private Command[] mCommands;

  // Called when the node enters the scene tree for the first time.
  public Piece(Display.Piece displayNode, Stats stats, bool isMaster = false) {
		// Set name.
		Name = stats.Name + " " + sNextIdForPiece++;

		// Set up display node.
	 	mDisplayNode = displayNode;
		displayNode.SetGamePiece(this, 0, 0); // Just one central game piece for now.

		// Set up commands.
		mCommands = new Command[stats.AvailableCommandTypes.Length];
		for (int i = 0; i < stats.AvailableCommandTypes.Length; i++) {
			mCommands[i] = Command.CreateFromType(stats.AvailableCommandTypes[i], this);
		}

		Stats = stats;
		IsMaster = isMaster;
  }

	// public string GetCommandDescriptions() {
	// 	string description = "";
	// 	foreach (Command command in mCommands) {
	// 		string commandDescription = command.Describe();
	// 		if (commandDescription == null) {
	// 			continue;
	// 		}
	// 		description += commandDescription + "\n";
	// 	}
	// 	if (description.Length == 0) {
	// 		return "You have yet to command me.";
	// 	}
	// 	return description;
	// }

	public void ResolveTick(Board board) {
		// Check reactive commands to determine triggered commands.
		List<Command> triggeredCommands = [];
		foreach (Command command in mCommands) {
			if (command.Status != Command.StatusEnum.COMPLETE) {
				continue;
			}
			if (command.IsReactive()) {
				Command triggeredCommand = command.GetTriggeredCommand(board);
				if (
					triggeredCommand != null &&
					!triggeredCommands.Contains(triggeredCommand) &&
					triggeredCommand.Status == Command.StatusEnum.READY
				) {
					triggeredCommands.Add(triggeredCommand);
				}
			}
		}

		// Get non-triggered active commands.
		List<Command> activeCommands = [];
		foreach (Command command in mCommands) {
			if (command.Status == Command.StatusEnum.COMPLETE) {
				activeCommands.Add(command);
			}
		}

		// Add triggered active commands.
		foreach (Command command in triggeredCommands) {
			activeCommands.Add(command);
		}

		// Figure out next move using A*.
		Tile[] path = Tile.AStar(Tile, [], board.GetTotalSize(), board.Toroidal, Movement.TravelCost, activeCommands, 1000); // TODO - use non-magic number
		DirectionEnum nextMoveDirection;
		if (path.Length == 0 || path[0] == Tile) {
			nextMoveDirection = DirectionEnum.NONE; // TODO - use just heuristic to determine direction here
		} else if (Tile.IsNeighbor(Tile, path[0])) {
			nextMoveDirection = Tile.DetermineDirection(Tile, path[0], board.Toroidal);	
		} else {
			throw new Exception("Invalid path returned by A*: " + path[0] + " is not a neighbor of " + Tile);
		}

		// Move piece according to its speed.
		mMovement.ResolveSpeed(nextMoveDirection);

		// Resolve overlapping pieces.
		mMovement.ResolveOverlaps();

		// Update display node position.
		mDisplayNode.Position = Tile.DisplayTile.Position;
	}

	public Tile[] GetTiles(Tile.PartitionTypeEnum partition) {
		return Tile.GetTilesWithPartition(partition);
	}

	public Tile[] GetTiles() {
		return GetTiles(GetPartitionType(Size));
	}

	public override string ToString() {
		return $"Piece: {Name}";
	}

	private class Movement {
		private enum State { DEFINITE, ORTHOGONAL, DIAGONAL }

		public SpeedEnum Speed {
			get => mSpeedEnum;
			set {
				mSpeedEnum = value;
				mSpeed = (int) Math.Pow((int) value, 2);
			}
		}

		public int Progress { get; set; } = 0;
		public SizeEnum Size { get; set; }

		public Tile CurrTile { get; set; }
		public Tile PrevTile { get; set; }

		private State mState = State.DEFINITE;

		private SpeedEnum mSpeedEnum = SpeedEnum.NONE;
		private int mSpeed = 0;
		
		public void ResolveSpeed(DirectionEnum direction) {
			if (Speed == SpeedEnum.NONE) {
				return;
			}

			if (mState == State.DEFINITE) {
				UpdateTile(direction);
			}

			// Move towards target tile.
			Progress += mSpeed;
			if (mState == State.ORTHOGONAL && Progress >= Board.ORTHOGONAL_UNITS * 10) {
				Progress -= Board.ORTHOGONAL_UNITS * 10;
				UpdateTile(direction);
			} else if (mState == State.DIAGONAL && Progress >= Board.ORTHOGONAL_UNITS * 10) {
				Progress -= Board.DIAGONAL_UNITS * 10;
				UpdateTile(direction);
			}
		}

		public void ResolveOverlaps() {
			// This should resolve pieces that share tiles and relocated them. Don't bother for now.
		}

		private void UpdateTile(DirectionEnum direction) {
			// Assume that prevTile is the current tile and that mMovementProgress is 0.
				if (direction > DirectionEnum.WEST) {
					// Diagonal move
					mState = State.DIAGONAL;
				} else if (direction != DirectionEnum.NONE) {
					// Orthogonal move
					mState = State.ORTHOGONAL;
				} else {
					// No move
					mState = State.DEFINITE;
					return;
				}

				// These signify that this piece is moving towards a new tile.
				PrevTile = CurrTile;
				CurrTile = CurrTile.Neighbors[(int) direction];
		}

		public static int TravelCost(Tile t1, Tile t2) {
			if (t1 == null || t2 == null || Tile.IsNeighbor(t1, t2)) {
				return int.MaxValue;
			}
			if (t1.Coordinate.X == t2.Coordinate.X || t1.Coordinate.Y == t2.Coordinate.Y) {
				return Board.ORTHOGONAL_UNITS;
			} else {
				return Board.DIAGONAL_UNITS;
			}
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
