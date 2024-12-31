using Godot;
using System;

namespace Display {
public partial class Tile : MeshInstance3D {
	public const int MESH_SIZE = 32;
	private Game.Tile representedTile;
	
	public override void _Ready() {
		Scale = new Vector3(MESH_SIZE, MESH_SIZE, 1);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta) {
	}

	public void SetRepresentedTile(
		Game.Tile tile,
		Main.DirectionEnum horizontal,
		Main.DirectionEnum vertical) {
		
		representedTile = tile;
		Name = tile.Name + " [" + horizontal + ", " + vertical + "]";

		Update();
	}
	public void Update() {
		
	}

	public void UseDebugMaterial(float red, float green, float blue) {
		StandardMaterial3D debugMaterial = new StandardMaterial3D();
		debugMaterial.AlbedoColor = new Color(red, green, blue);
		MaterialOverride = debugMaterial;
	}
}
}
