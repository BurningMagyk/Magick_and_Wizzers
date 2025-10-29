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
  public Command.CommandType[] AvailableCommandTypes { get; private set; }
  public List<Command> mCommands = [];
	public bool Toroidal { get; set;}

  // Called when the node enters the scene tree for the first time.
  public Piece(Stats stats, Display.Piece displayNode) {
		Name = stats.Name + " " + sNextIdForPiece++;
		mDisplayNode = displayNode;
		displayNode.SetGamePiece(this, 0, 0); // Just one central game piece for now.

		// Command stuff should come from the stats. Use defaults for now.
		AvailableCommandTypes = [
			Command.CommandType.APPROACH,
	  	Command.CommandType.AVOID,
	  	Command.CommandType.INTERCEPT
		];
  }

	public void AddCommand(Command command) {
		mCommands.Add(command);
	}

	public bool HasCommands() {
		return mCommands.Count > 0;
	}

	public string GetCommandDescriptions() {
		string description = "";
		foreach (Command command in mCommands) {
			description += command.Describe() + "\n";
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

		// Determine move direction.
		DirectionEnum moveDirection = DetermineMoveDirection(
			board.Tiles[(int) GetPartitionType(Size)],
			[.. activeCommands]
		);
		
		// Apply over-time effects here too.
	}

  private DirectionEnum DetermineMoveDirection(Tile[,] grid, Command[] activeCommands) {
		if (activeCommands.Length == 0) {
			return DirectionEnum.NONE;
		}

		Command primaryCommand = null;
		foreach (Command command in activeCommands) {
			if (command.IsPrimary) {
				primaryCommand = command;
				break;;
			}
		}
		if (primaryCommand == null) {
			throw new Exception("No primary command found among active commands.");
		}

		// Get all target tiles.
		List<Tile> targetTiles = [];
		foreach (Target target in primaryCommand.GetTargets()) {
			Tile[] tilesFromTarget = target.GetTiles(GetPartitionType(Size));
			foreach (Tile tile in tilesFromTarget) {
				if (!targetTiles.Contains(tile)) {
					targetTiles.Add(tile);
				}
			}
		}

		List<Tile> secondaryApproachTiles = [];
		foreach (Command command in activeCommands) {
			if (command == primaryCommand || command.Type != Command.CommandType.APPROACH) {
				continue;
			}
			foreach (Target target in primaryCommand.GetTargets()) {
				Tile[] tilesFromTarget = target.GetTiles(GetPartitionType(Size));
				foreach (Tile tile in tilesFromTarget) {
					if (!secondaryApproachTiles.Contains(tile)) {
						secondaryApproachTiles.Add(tile);
					}
				}
			}
		}

		List<Tile> secondaryAvoidTiles = [];
		foreach (Command command in activeCommands) {
			if (command == primaryCommand || command.Type != Command.CommandType.AVOID) {
				continue;
			}
			foreach (Target target in primaryCommand.GetTargets()) {
				Tile[] tilesFromTarget = target.GetTiles(GetPartitionType(Size));
				foreach (Tile tile in tilesFromTarget) {
					if (!secondaryAvoidTiles.Contains(tile)) {
						secondaryAvoidTiles.Add(tile);
					}
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
		foreach (Tile tile in secondaryApproachTiles) {
			if (tile.PartitionType != requiredPartition) {
				throw new Exception($"Target tile {tile.Name} does not match piece partition type {requiredPartition}.");
			}
		}
		foreach (Tile tile in secondaryAvoidTiles) {
			if (tile.PartitionType != requiredPartition) {
				throw new Exception($"Target tile {tile.Name} does not match piece partition type {requiredPartition}.");
			}
		}

		// Determine which target is closest to this piece using A*.
		int lowestCost = int.MaxValue;
		DirectionEnum bestDirection = DirectionEnum.NONE;
		foreach (Tile targetTile in targetTiles) {
			DirectionEnum startingDirectionToTarget = AStar(
				grid,
				Tile,
				targetTile,
				[.. secondaryApproachTiles],
				[.. secondaryAvoidTiles],
				Toroidal,
				out int cost
			);
			if (cost < lowestCost) {
				lowestCost = cost;
				bestDirection = startingDirectionToTarget;
			}
		}

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