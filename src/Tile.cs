using Godot;
using System;

public partial class Tile : Sprite2D {
	public const int TILE_SIZE = 128;
	public enum HoverType { NORMAL, MOVE, INTERACT, CAST }

	private HoverType hoverType = HoverType.NORMAL;
	private Sprite2D[] hoverSprites;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		hoverSprites = new Sprite2D[Enum.GetNames(typeof(HoverType)).Length];
		hoverSprites[(int) HoverType.NORMAL] = GetNode<Sprite2D>("Hover");
		hoverSprites[(int) HoverType.MOVE] = GetNode<Sprite2D>("Hover Move");
		hoverSprites[(int) HoverType.INTERACT] = GetNode<Sprite2D>("Hover Interact");
		hoverSprites[(int) HoverType.CAST] = GetNode<Sprite2D>("Hover Cast");

		foreach (Sprite2D hoverSprite in hoverSprites) {
			hoverSprite.Visible = false;
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta) {
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
}
