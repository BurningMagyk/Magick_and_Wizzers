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
			mDisplayNode.Position = value.DisplayPosition;
			mDisplayNode.Scale = new Vector3(value.DisplaySize, value.DisplaySize, value.DisplaySize);
		}
	}
	public Main.Stats Stats { get; set; }
	public string Name { get; private set;}
	public Player MasteringPlayer { get; set; }

	private readonly Display.Piece mDisplayNode;
	private Tile tile;
	public Command Command { get; private set; }

	// Called when the node enters the scene tree for the first time.
	public Piece(Stats stats, Display.Piece displayNode) {
		Name = stats.Name + " " + sNextIdForPiece++;
		mDisplayNode = displayNode;
	}

	public void FollowCommand() {
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
