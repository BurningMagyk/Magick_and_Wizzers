using Godot;
using Main;
using System;
using System.Collections.Generic;

namespace Match {
public class Piece : ITarget {
	public enum SizeEnum { TINY, SMALL, MEDIUM, LARGE, HUGE, GARGANTUAN, COLOSSAL }
	private enum MovementState { DONE, STRAIGHT, DIAGONAL }
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
	public Command[] Commands { get => mCommands; private set => mCommands = value; }
  public Stats Stats { get; set; }
  public string Name { get; private set; }
  public Player MasteringPlayer { get; set; }
	public SizeEnum Size { get; set; }

  private readonly Display.Piece mDisplayNode;
  private Tile tile, prevTile;
	private MovementState mMovementState = MovementState.DONE;
	private int mMovementProgress = 0;
	private int mSpeed = 1; // Tiles per tick. Should be set from stats.
	private int maxCommandTargets = 2; // Should be set from stats.
  private Command[] mCommands;
	public bool Toroidal { get; set;}

  // Called when the node enters the scene tree for the first time.
  public Piece(Stats stats, Display.Piece displayNode) {
		Name = stats.Name + " " + sNextIdForPiece++;
		mDisplayNode = displayNode;
		displayNode.SetGamePiece(this, 0, 0); // Just one central game piece for now.
		mCommands = new Command[stats.AvailableCommandTypes.Length];
		for (int i = 0; i < stats.AvailableCommandTypes.Length; i++) {
			mCommands[i] = new Command(
				stats.AvailableCommandTypes[i],
				maxCommandTargets
			);
		}
		Stats = stats;
  }

	public string GetCommandDescriptions() {
		string description = "";
		foreach (Command command in mCommands) {
			string commandDescription = command.Describe();
			if (commandDescription == null) {
				continue;
			}
			description += commandDescription + "\n";
		}
		if (description.Length == 0) {
			return "You have yet to command me.";
		}
		return description;
	}

	public void ResolveTick(Board board) {
		// Get active commands.
		List<Command> activeCommands = [];
		foreach (Command command in mCommands) {
			if (command.IsActive()) {
				activeCommands.Add(command);
			}
		}		

		// Move piece.
		ResolveMovement(mSpeed, board, [.. activeCommands]);
		
		// Apply over-time effects here too.
	}

	public void ResolveMovement(int speed, Board board, Command[] activeCommands) {
		if (speed == 0) {
			return;
		}

		if (mMovementState == MovementState.DONE) {
			// Not between tiles. Determine move direction.
			DirectionEnum moveDirection = DetermineMoveDirection(
				board.Tiles[(int) GetPartitionType(Size)],
				activeCommands
			);
			// Assume that prevTile is the current tile and that mMovementProgress is 0.
			if (moveDirection > DirectionEnum.WEST) {
				// Diagonal move
				mMovementState = MovementState.DIAGONAL;
			} else if (moveDirection != DirectionEnum.NONE) {
				// Straight move
				mMovementState = MovementState.STRAIGHT;
			} else {
				// No move
				mMovementState = MovementState.DONE;
				return;
			}
			tile = Tile.Neighbors[(int) moveDirection]; // Don't move the piece's display yet.
		}

		// Move towards target tile.
		mMovementProgress += mSpeed;
		if (mMovementState == MovementState.STRAIGHT && mMovementProgress >= Tile.STRT_COST) {
			mMovementState = MovementState.DONE;
			prevTile = tile;
			ResolveMovement(mMovementProgress - Tile.STRT_COST, board, activeCommands);
		} else if (mMovementState == MovementState.DIAGONAL && mMovementProgress >= Tile.DIAG_COST) {
			mMovementState = MovementState.DONE;
			prevTile = tile;
			ResolveMovement(mMovementProgress - Tile.DIAG_COST, board, activeCommands);
		}

		// Done moving during this tick.
		Tile tileBeingMovedTo = tile;
		// Relocate display node.
		Tile = prevTile;
		// Make the tile being moved to the current tile for next tick.
		tile = tileBeingMovedTo;
	}

	public void FinalizePosition() {
		// This should resolve pieces that share tiles and relocated them. Don't bother for now.
		if (mMovementState != MovementState.DONE) {
			// Relocate display node.
			Tile = prevTile;
			mMovementProgress = 0;
			mMovementState = MovementState.DONE;
		}
	}

	public Tile[] GetTiles(Tile.PartitionTypeEnum partition) {
		return Tile.GetTilesWithPartition(partition);
	}

	public override string ToString() {
		return $"Piece: {Name}";
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

	private static DirectionEnum AStar(
		Tile[,] grid,
		Tile start,
		Tile goal,
		Tile[] approachTiles,
		Tile[] avoidTiles,
		bool toroidal,
		out int cost
	) {
		var openSet = new PriorityQueue<Tile, int>();
		var cameFrom = new Dictionary<Tile, Tile>();
		var gScore = new Dictionary<Tile, int> { [start] = 0 };

		openSet.Enqueue(start, Tile.Heuristic(start, goal));

		while (openSet.Count > 0) {
			var current = openSet.Dequeue();

			if (current.Equals(goal)) {
				cost = gScore[current];
				List<Tile> path = ReconstructPath(cameFrom, current);
				// Print the path:
				GD.Print("========== Path Found ==========");
				foreach (Tile tile in path) {
					GD.Print(tile.Name);
				}
				GD.Print("================================");

				if (path.Count < 2) {
					return DirectionEnum.NONE;
				}
				Tile nextStep = path[1];
				if (nextStep.Equals(start)) {
					return DirectionEnum.NONE;
				} else if (!Tile.IsNeighbor(start, nextStep)) {
					throw new Exception("Next step in path is not a neighbor of start tile.");
				} else {
					return Tile.DetermineDirection(start, nextStep);
				}
			}

			foreach (var (dx, dy, baseCost) in Tile.DirectionsCostMatrix) {
				var neighbor = grid[current.Coordinate.X + dx, current.Coordinate.Y + dy];

				// Skip if outside board bounds.
				if (neighbor.Coordinate.X < 0 ||
						neighbor.Coordinate.Y < 0 ||
						neighbor.Coordinate.X >= grid.GetLength(0) ||
						neighbor.Coordinate.Y >= grid.GetLength(1)
				) {
					continue;
				}

				// Compute cost modifier based on approach/avoid tiles.
				int moveCost = baseCost;
				bool closerToAvoid = Tile.IsCloserToAny(neighbor, current, avoidTiles);
				bool closerToApproach = Tile.IsCloserToAny(neighbor, current, approachTiles);

				if (closerToAvoid && !closerToApproach) {
					moveCost *= 2;
				} else if (closerToApproach && !closerToAvoid) {
					moveCost /= 2;
				}

				int tentativeG = gScore[current] + moveCost;

				if (!gScore.TryGetValue(neighbor, out int existingG) || tentativeG < existingG) {
					cameFrom[neighbor] = current;
					gScore[neighbor] = tentativeG;
					int fScore = tentativeG + Tile.Heuristic(neighbor, goal);
					openSet.Enqueue(neighbor, fScore);
				}
			}
		}

		// No path found
		cost = int.MaxValue;
		return DirectionEnum.NONE;
	}

	private static List<Tile> ReconstructPath(Dictionary<Tile, Tile> cameFrom, Tile current) {
		var path = new List<Tile> { current };
		while (cameFrom.ContainsKey(current)) {
			current = cameFrom[current];
			path.Insert(0, current);
		}
		return path;
	}
}
}
