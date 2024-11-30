using Godot;
using Main;
using System;

public partial class Tile : Sprite2D {
	public const int TILE_SIZE = 128;
	public const PartitionType MAX_PARTITION = PartitionType.VIRGATE;
	public enum HoverType { NORMAL, MOVE, INTERACT, CAST }
	public enum PartitionType { LAND, CARUCATE, VIRGATE, HECTARE, ACRE }

	private Vector2 textureSize;
	private Tile[,] tiles = new Tile[2, 2];
	private HoverType hoverType = HoverType.NORMAL;
	private Sprite2D[] hoverSprites;
	private int partitionLevel = 0;

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

		// Saving texture size because texture can become null later.
		if (Texture != null) {
			textureSize = Texture.GetSize();
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta) {
	}

	public void Partition() { Partition(1); }
	private void Partition(int level) {
		partitionLevel = level - 1;
		if (partitionLevel >= (int) MAX_PARTITION) { return; }

		PackedScene tileScene = ResourceLoader.Load<PackedScene>("res://scenes/tile.tscn");
		for (int i = 0; i < 2; i++) {
			for (int j = 0; j < 2; j++) {
				Sprite2D tileSprite = tileScene.Instantiate() as Sprite2D;
				tileSprite.Texture = null;
				tileSprite.Scale = new Vector2(0.5F, 0.5F);
				tileSprite.Position
					= new Vector2(i, j) * tileSprite.Scale * textureSize
					- textureSize * tileSprite.Scale / 2;
				
				string partitionTypeName = Util.ToTitleCase(Enum.GetNames(typeof(PartitionType))[level]);
		  		tileSprite.Name = partitionTypeName + " [" + i + ", " + j + "]";
		  		AddChild(tileSprite);
				tiles[i, j] = GetNode<Tile>(tileSprite.Name.ToString());

				tiles[i, j].textureSize = textureSize;
				tiles[i, j].Partition(level + 1);
			}
		}
	}

	public Tile GetTileAt(Vector2I index, PartitionType partitionType) {
		if ((int) partitionType <= partitionLevel) { return this; }

		Vector2I partitionIndex = new Vector2I(
			(int) Math.Floor(index.X / Math.Pow(2, (int) partitionType - partitionLevel - 1)),
			(int) Math.Floor(index.Y / Math.Pow(2, (int) partitionType - partitionLevel - 1))
		);
		GD.Print(partitionIndex);
		Vector2I subIndex = new Vector2I(
			index.X - partitionIndex.X * (int) Math.Pow(2, (int) partitionType),
			index.Y - partitionIndex.Y * (int) Math.Pow(2, (int) partitionType));
		
		return tiles[partitionIndex.X, partitionIndex.Y].GetTileAt(subIndex, partitionType);
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
