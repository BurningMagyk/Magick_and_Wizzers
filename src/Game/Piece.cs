using Godot;
using System;

namespace Game {
public partial class Piece : MeshInstance3D
{
	public Tile Tile {
		get => tile;
		set {
			tile = value;
			Position = tile.GlobalPosition;
			// Scale = value.GlobalScale;
		}
	}
	public Main.Stats Stats { get; set; }

	private Tile tile;
	private Command command;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		float size = Tile.TILE_SIZE / (float) Math.Pow(2, (int) Tile.MAX_PARTITION);
		Scale = new Vector3(size, size, size);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta) {
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
