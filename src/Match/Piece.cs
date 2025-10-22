using Godot;
using Main;
using System;

namespace Match {
public class Piece {
  private static int sNextIdForPiece = 0;

  public Tile Tile {
	  get => tile;
	  set {
		  tile = value;
		  float size = value.DisplayTile.Size;
		  mDisplayNode.Position = value.DisplayTile.Position + new Vector3(0, size, 0);
		  mDisplayNode.Scale = new Vector3(size, size, size);
	  }
  }
  public Main.Stats Stats { get; set; }
  public string Name { get; private set;}
  public Player MasteringPlayer { get; set; }

  private readonly Display.Piece mDisplayNode;
  private Tile tile;
  public Command.CommandType[] CommandTypes { get; private set; }
  public Command Command { get; set; }

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

	public void ResolveTick() {
		FollowCommand();
		// Apply over-time effects here too.
	}

  private void FollowCommand() {
	// switch (command.Type) {
	// 	case Command.CommandType.MOVE:
	// 		// Move the piece to the new tile.
	// 		// Tile = Tile.GetNeighbor(command.Direction);
	// 		break;
	// 	default:
	// 		// throw new NotImplementedException();
	// 		break;
	// }
  }
}
}
