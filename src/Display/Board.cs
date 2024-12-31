using Godot;
using System;

namespace Display {
public partial class Board : Node3D {
	public enum HoverType { NORMAL, MOVE, INTERACT, CAST }
	private HoverType currentHoverType = HoverType.NORMAL;
	private Sprite3D[] hoverSprites;
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
	}

	public void SetTiles(Game.Tile[,] tiles) {
		PackedScene tileScene = ResourceLoader.Load<PackedScene>("res://scenes/tile.tscn");
		for (int i = 0; i < tiles.GetLength(0); i++) {
			for (int j = 0; j < tiles.GetLength(1); j++) {
				MeshInstance3D tileMesh = tileScene.Instantiate() as MeshInstance3D;
				tileMesh.Position = new Vector3(
					Game.Tile.TILE_SIZE / 2 + Game.Tile.TILE_SIZE * i,
					0,
					Game.Tile.TILE_SIZE / 2 + Game.Tile.TILE_SIZE * j
				);
				tileMesh.Visible = true;
				tileMesh.Name = "Tile [" + i + ", " + j + "]";
				AddChild(tileMesh);

				GetNode<Tile>(tileMesh.Name.ToString()).UseDebugMaterial(
					(float) i / Game.Board.BOARD_SIZE,
					(float) j / Game.Board.BOARD_SIZE,
					1 - (float) (i + j) / Game.Board.BOARD_SIZE / 2
				);
			}
		}
	}

	public void Hover(Tile tile, HoverType hoverType) {
		if (currentHoverType != hoverType) {
			hoverSprites[(int) currentHoverType].Visible = false;
		}
		hoverSprites[(int) hoverType].Visible = true;
		currentHoverType = hoverType;
	}

	public void Unhover(Tile tile) {
		hoverSprites[(int) currentHoverType].Visible = false;
	}
}
}
