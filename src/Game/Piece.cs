using Godot;
using Main;
using System;

namespace Game {
public partial class Piece {
	private static int sNextIdForPiece = 0;

	public Tile Tile {
		get => tile;
		set {
			tile = value;
			// Position = tile.GlobalPosition;
			// Scale = value.GlobalScale;
		}
	}
	public Main.Stats Stats { get; set; }
	public string Name { get; private set;}

	private readonly Display.Piece mDisplayNode;
	private Tile tile;
	private Command command;

	// Called when the node enters the scene tree for the first time.
	public Piece(Stats stats) {
		Name = stats.Name + " " + sNextIdForPiece++;
	}

	public void FollowCommand() {
		switch (command.Type) {
			case Command.CommandType.MOVE:
				// Move the piece to the new tile.
				// Tile = Tile.GetNeighbor(command.Direction);
				break;
			default:
				// throw new NotImplementedException();
				break;
		}
	}
}
}
