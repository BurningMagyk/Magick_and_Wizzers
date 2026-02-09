using Godot;
using Main;
using System;
using System.Collections.Generic;

namespace Match {
public class Piece : Spacial {
	public enum SizeEnum { TINY, SMALL, MEDIUM, LARGE, HUGE, GARGANTUAN, COLOSSAL }
  private static int sNextIdForPiece = 0;

	public Command[] Commands { get => mCommands; private set => mCommands = value; }
  public Stats Stats { get; set; }
  public string Name { get; private set; }
  public Player ControllingPlayer { get; set; }
	public bool IsMaster { get; private set; } = false;

	public SizeEnum Size { get => mMovement.Size; set => mMovement.Size = value; }
	public Tile Tile { get => mMovement.CurrTile; set => mMovement.CurrTile = value; }

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
		// Check reactive commands.
		List<Command> triggeredCommands = [];
		foreach (Command command in mCommands) {
			if (command.Status != Command.StatusEnum.COMPLETE) {
				continue;
			}
			if (command.IsReactive()) {
				
			}
		}

		// Get active commands.
		List<Command> activeCommands = [];
		foreach (Command command in mCommands) {
			if (command.Status == Command.StatusEnum.COMPLETE) {
				activeCommands.Add(command);
			}
			// TODO - check if any reactive commands trigger any ready commands to add to the list of active commands
		}

		// Move piece.
		mMovement.ResolveSpeed(board, [.. activeCommands]);
		
		// Apply over-time effects here too.
	}

	public Tile[] GetTiles(Tile.PartitionTypeEnum partition) {
		return Tile.GetTilesWithPartition(partition);
	}

	public override string ToString() {
		return $"Piece: {Name}";
	}

	private class Movement {
		private enum State { NOT_BETWEEN, BETWEEN_STRAIGHT, BETWEEN_DIAGONAL }

		public int Speed { get; set; }
		public int Progress { get; set; } = 0;
		public SizeEnum Size { get; set; }

		public Tile CurrTile { get; set; }
		public Tile PrevTile { get; set; }

		private State mState = State.NOT_BETWEEN;
		

		public void ResolveSpeed(Board board, Command[] activeCommands) {
			if (Speed == 0) {
				return;
			}

			if (mState == State.NOT_BETWEEN) {
				// Not between tiles. Determine move direction.
				DirectionEnum moveDirection = DetermineMoveDirection(
					board.Tiles[(int) GetPartitionType(Size)],
					activeCommands
				);
				// Assume that prevTile is the current tile and that mMovementProgress is 0.
				if (moveDirection > DirectionEnum.WEST) {
					// Diagonal move
					mState = State.BETWEEN_DIAGONAL;
				} else if (moveDirection != DirectionEnum.NONE) {
					// Straight move
					mState = State.BETWEEN_STRAIGHT;
				} else {
					// No move
					mState = State.NOT_BETWEEN;
					return;
				}
				CurrTile = CurrTile.Neighbors[(int) moveDirection]; // Don't move the piece's display yet.
			}

			// Move towards target tile.
			Progress += Speed;
			if (mState == State.BETWEEN_STRAIGHT && Progress >= Board.STRAIGHT_UNITS) {
				mState = State.NOT_BETWEEN;
				prevTile = tile;
				ResolveMovement(mMovementProgress - Board.STRAIGHT_UNITS, board, activeCommands);
			} else if (mMovementState == MovementState.DIAGONAL && mMovementProgress >= Tile.DIAG_COST) {
				mMovementState = MovementState.DONE;
				prevTile = tile;
				ResolveMovement(mMovementProgress - Tile.DIAG_COST, board, activeCommands);
			}

			// Done moving during this tick.
			Tile tileBeingMovedTo = CurrTile;
			// Relocate display node.
			CurrTile = PrevTile;
			// Make the tile being moved to the current tile for next tick.
			CurrTile = tileBeingMovedTo;
		}

		public void ResolveIncompletes() {
			// This should resolve pieces that share tiles and relocated them. Don't bother for now.
			if (mState != State.NOT_BETWEEN) {
				// Relocate display node.
				Tile = PrevTile;
				Progress = 0;
				mState = State.NOT_BETWEEN;
			}
		}

		public void ResolveOverlaps() {
		}

		private DirectionEnum DetermineMoveDirection(Tile[,] grid, Command[] activeCommands) {
			if (activeCommands.Length == 0) {
				return DirectionEnum.NONE;
			}

			List<Tile> tilesToApproach = [];
			// Get the APPROACH command.
			Command approachCommand = null;
			foreach (Command command in activeCommands) {
				if (command.Type == Command.CommandType.APPROACH) {
					approachCommand = command;
					break;
				}
			}

			if (approachCommand != null) {
				foreach (ITarget target in approachCommand.GetTargets()) {
					Tile[] tilesFromTarget = target.GetTiles(GetPartitionType(Size));
					foreach (Tile tile in tilesFromTarget) {
						if (!tilesToApproach.Contains(tile)) {
							tilesToApproach.Add(tile);
						}
					}
				}
			}

			List<Tile> tilesToAvoid = [];
			// Get the AVOID command.
			Command avoidCommand = null;
			foreach (Command command in activeCommands) {
				if (command.Type == Command.CommandType.AVOID) {
					avoidCommand = command;
					break;
				}
			}

			if (avoidCommand != null) {
				foreach (ITarget target in avoidCommand.GetTargets()) {
					Tile[] tilesFromTarget = target.GetTiles(GetPartitionType(Size));
					foreach (Tile tile in tilesFromTarget) {
						if (!tilesToAvoid.Contains(tile)) {
							tilesToAvoid.Add(tile);
						}
					}
				}
			}

			// Check that all target tiles are the correct partition type.
			Tile.PartitionTypeEnum requiredPartition = GetPartitionType(Size);
			foreach (Tile tile in tilesToApproach) {
				if (tile.PartitionType != requiredPartition) {
					throw new Exception($"Target tile {tile.Name} does not match piece partition type {requiredPartition}.");
				}
			}
			foreach (Tile tile in tilesToAvoid) {
				if (tile.PartitionType != requiredPartition) {
					throw new Exception($"Target tile {tile.Name} does not match piece partition type {requiredPartition}.");
				}
			}

			// TODO - rewrite A* to handle multiple target tiles.
			int lowestCost = int.MaxValue;
			DirectionEnum bestDirection = DirectionEnum.NONE;
			// GD.Print("targetTiles count: " + targetTiles.Count);
			// foreach (Tile targetTile in targetTiles) {
			// 	DirectionEnum startingDirectionToTarget = AStar(
			// 		grid,
			// 		Tile,
			// 		targetTile,
			// 		[.. secondaryApproachTiles],
			// 		[.. secondaryAvoidTiles],
			// 		Toroidal,
			// 		out int cost
			// 	);
			// 	if (cost < lowestCost) {
			// 		lowestCost = cost;
			// 		bestDirection = startingDirectionToTarget;
			// 	}
			// }

			return bestDirection;
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
