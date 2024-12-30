using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Game {
public partial class Tile : MeshInstance3D {
	public const int TILE_SIZE = 32;
	public const PartitionType MAX_PARTITION = PartitionType.VIRGATE;
	public enum HoverType { NORMAL, MOVE, INTERACT, CAST }
	public enum PartitionType { CARUCATE, VIRGATE, BOVATE, HECTARE, ACRE }

	private Tile[,] tiles = new Tile[2, 2];
	private HoverType hoverType = HoverType.NORMAL;
	private Sprite3D[] hoverSprites;
	private PartitionType partitionType;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		hoverSprites = new Sprite3D[Enum.GetNames(typeof(HoverType)).Length];
		hoverSprites[(int) HoverType.NORMAL] = GetNode<Sprite3D>("Hover");
		hoverSprites[(int) HoverType.MOVE] = GetNode<Sprite3D>("Hover Move");
		hoverSprites[(int) HoverType.INTERACT] = GetNode<Sprite3D>("Hover Interact");
		hoverSprites[(int) HoverType.CAST] = GetNode<Sprite3D>("Hover Cast");

		foreach (Sprite3D hoverSprite in hoverSprites) {
			hoverSprite.Visible = false;
			hoverSprite.PixelSize = 1F / hoverSprite.Texture.GetSize().X;
		}

		Scale = new Vector3(TILE_SIZE, TILE_SIZE, 1);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta) {
	}

	public Vector2I Coordinate { get; set; }

	public void Partition(List<Tile[,]> tilesCollection) {
		int partitionLevel = (int) MAX_PARTITION - tilesCollection.Count;
		if (tilesCollection.Count == 0) { return; }

		PackedScene tileScene = ResourceLoader.Load<PackedScene>("res://scenes/tile.tscn");
		for (int i = 0; i < 2; i++) {
			for (int j = 0; j < 2; j++) {
			MeshInstance3D tileMeshInstance = tileScene.Instantiate() as MeshInstance3D;
			
			string childPartitionTypeName = Main.Util.ToTitleCase(
				Enum.GetNames(typeof(PartitionType))[partitionLevel + 1]
			);
			tileMeshInstance.Name = childPartitionTypeName + " [" + i + ", " + j + "]";
			AddChild(tileMeshInstance);
			tiles[i, j] = GetNode<Tile>(tileMeshInstance.Name.ToString());

			tileMeshInstance.MaterialOverride = null;
			tileMeshInstance.Mesh = null;
			tileMeshInstance.RotationDegrees = new Vector3(0, 0, 0);
			tileMeshInstance.Scale = new Vector3(0.5F, 0.5F, 1);
			tileMeshInstance.Position = new Vector3(i * 0.5F - 0.25F, j * -0.5F + 0.25F, 0);

			tiles[i, j].Coordinate = new Vector2I(Coordinate.X * 2 + i, Coordinate.Y * 2 + j);
			tilesCollection[0][tiles[i, j].Coordinate.X, tiles[i, j].Coordinate.Y] = tiles[i, j];

			tiles[i, j].Partition(tilesCollection.Skip(1).ToList());
			}
		}
	}

	public void Hover(HoverType hoverType) {
		if (this.hoverType != hoverType) {
			hoverSprites[(int) this.hoverType].Visible = false;
		}
		hoverSprites[(int) hoverType].Visible = true;
		this.hoverType = hoverType;
	}

	public void Unhover(HoverType hoverType) {
		if (this.hoverType != hoverType) {
			hoverSprites[(int) this.hoverType].Visible = false;
		}
		hoverSprites[(int) hoverType].Visible = false;
		this.hoverType = hoverType;
	}

	public void UseDebugMaterial(float red, float green, float blue) {
		StandardMaterial3D debugMaterial = new StandardMaterial3D();
		debugMaterial.AlbedoColor = new Color(red, green, blue);
		MaterialOverride = debugMaterial;
	}
}
}
