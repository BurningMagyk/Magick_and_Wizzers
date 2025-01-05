using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Game {
public partial class Tile {
	public const int TILE_SIZE = 32;
	public const PartitionTypeEnum MAX_PARTITION = PartitionTypeEnum.VIRGATE;
	public enum HoverType { NORMAL, MOVE, INTERACT, CAST }
	public enum PartitionTypeEnum { CARUCATE, VIRGATE, BOVATE, HECTARE, ACRE }

	public Display.Tile DisplayTile { get; private set; }
	public string Name { get; private set; }
	public PartitionTypeEnum PartitionType { get; private set; }
	private Tile[,] tiles = new Tile[2, 2];
	// private HoverType hoverType = HoverType.NORMAL;
	// private Sprite3D[] hoverSprites;

	// public override void _Ready() {
		// hoverSprites = new Sprite3D[Enum.GetNames(typeof(HoverType)).Length];
		// hoverSprites[(int) HoverType.NORMAL] = GetNode<Sprite3D>("Hover");
		// hoverSprites[(int) HoverType.MOVE] = GetNode<Sprite3D>("Hover Move");
		// hoverSprites[(int) HoverType.INTERACT] = GetNode<Sprite3D>("Hover Interact");
		// hoverSprites[(int) HoverType.CAST] = GetNode<Sprite3D>("Hover Cast");

		// foreach (Sprite3D hoverSprite in hoverSprites) {
		// 	hoverSprite.Visible = false;
		// 	hoverSprite.PixelSize = 1F / hoverSprite.Texture.GetSize().X;
		// }
	// }

	public Vector2I Coordinate { get; private set; }

	public Tile(int x, int y) {
		Coordinate = new Vector2I(x, y);
	}
	public Tile(Vector2I coordinate) {
		Coordinate = coordinate;
	}

	public void Partition(List<Tile[,]> tilesCollection) {
		int partitionLevel = (int) MAX_PARTITION - tilesCollection.Count;
		if (tilesCollection.Count == 0) { return; }

		for (int i = 0; i < 2; i++) {
			for (int j = 0; j < 2; j++) {
			
			
			string childPartitionTypeName = Main.Util.ToTitleCase(
				Enum.GetNames(typeof(PartitionTypeEnum))[partitionLevel + 1]
			);
			tiles[i, j] = new Tile(Coordinate.X * 2 + i, Coordinate.Y * 2 + j);

			// tileMeshInstance.MaterialOverride = null;
			// tileMeshInstance.Mesh = null;
			// tileMeshInstance.RotationDegrees = new Vector3(0, 0, 0);
			// tileMeshInstance.Scale = new Vector3(0.5F, 0.5F, 1);
			// tileMeshInstance.Position = new Vector3(i * 0.5F - 0.25F, j * -0.5F + 0.25F, 0);

			tilesCollection[0][tiles[i, j].Coordinate.X, tiles[i, j].Coordinate.Y] = tiles[i, j];
			tiles[i, j].Partition(tilesCollection.Skip(1).ToList());
			}
		}
	}

	public void Hover(HoverType hoverType) {
		// if (this.hoverType != hoverType) {
		// 	hoverSprites[(int) this.hoverType].Visible = false;
		// }
		// hoverSprites[(int) hoverType].Visible = true;
		// this.hoverType = hoverType;
	}

	public void Unhover(HoverType hoverType) {
		// if (this.hoverType != hoverType) {
		// 	hoverSprites[(int) this.hoverType].Visible = false;
		// }
		// hoverSprites[(int) hoverType].Visible = false;
		// this.hoverType = hoverType;
	}
}
}
