using Godot;
using Main;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Main {
	public partial class Tile : Sprite2D {
		public const int TILE_SIZE = 128;
		public const PartitionType MAX_PARTITION = PartitionType.VIRGATE;
		public enum HoverType { NORMAL, MOVE, INTERACT, CAST }
		public enum PartitionType { LAND, CARUCATE, VIRGATE, HECTARE, ACRE }

		private Vector2 textureSize;
		private Tile[,] tiles = new Tile[2, 2];
		private HoverType hoverType = HoverType.NORMAL;
		private Sprite2D[] hoverSprites;
		private PartitionType partitionType;

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

		public Vector2I Coordinate { get; set; }

		public void Partition(List<Tile[,]> tilesCollection) {
			int partitionLevel = (int) MAX_PARTITION - tilesCollection.Count;
			if (tilesCollection.Count == 0) { return; }

			PackedScene tileScene = ResourceLoader.Load<PackedScene>("res://scenes/tile.tscn");
			for (int i = 0; i < 2; i++) {
				for (int j = 0; j < 2; j++) {
					Sprite2D tileSprite = tileScene.Instantiate() as Sprite2D;
					tileSprite.Texture = null;
					tileSprite.Scale = new Vector2(0.5F, 0.5F);
					tileSprite.Position
						= new Vector2(i, j) * tileSprite.Scale * textureSize
						- textureSize * tileSprite.Scale / 2;
					
					string childPartitionTypeName = Util.ToTitleCase(Enum.GetNames(typeof(PartitionType))[partitionLevel + 1]);
					tileSprite.Name = childPartitionTypeName + " [" + i + ", " + j + "]";
					AddChild(tileSprite);
					tiles[i, j] = GetNode<Tile>(tileSprite.Name.ToString());
					tiles[i, j].Coordinate = new Vector2I(Coordinate.X * 2 + i, Coordinate.Y * 2 + j);
					tilesCollection[0][tiles[i, j].Coordinate.X, tiles[i, j].Coordinate.Y] = tiles[i, j];

					tiles[i, j].textureSize = textureSize;
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
	}
}
