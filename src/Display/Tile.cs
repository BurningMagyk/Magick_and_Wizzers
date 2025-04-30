using Godot;
using System;

namespace Display {
public partial class Tile : MeshInstance3D {
	public const int MESH_SIZE = 32;
	private Match.Tile mGameTile;
	
	public override void _Ready() {
		Scale = new Vector3(MESH_SIZE, MESH_SIZE, 1);

		PackedScene tileScene = ResourceLoader.Load<PackedScene>("res://scenes/tile.tscn");
		MeshInstance3D tileMeshInstance = tileScene.Instantiate() as MeshInstance3D;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta) {
	}

	public void SetGameTile(Match.Tile tile, Main.DirectionEnum horizontal, Main.DirectionEnum vertical) {
		Name = tile.Name + " [" + horizontal + ", " + vertical + "]";
		Position = CalculatePosition(tile.Coordinate, tile.PartitionType);
		mGameTile = tile;
	}

	public void UseDebugMaterial(float red, float green, float blue) {
		StandardMaterial3D debugMaterial = new StandardMaterial3D();
		debugMaterial.AlbedoColor = new Color(red, green, blue);
		MaterialOverride = debugMaterial;
	}

	public static Vector3 CalculatePosition(Vector2I coordinate, Match.Tile.PartitionTypeEnum partitionType) {
		int size = MESH_SIZE / (int) Math.Pow(2, (int) partitionType);
		return new Vector3(size / 2 + size * coordinate.X, 0, size / 2 + size * coordinate.Y);
	}
}
}
