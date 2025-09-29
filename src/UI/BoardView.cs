using Godot;
using System;
using Match;

namespace UI {
public partial class BoardView : CanvasLayer, IView {
  private enum HoverType { NORMAL, MOVE, INTERACT, CAST }
  private const float HOVER_SPRITE_LIFT = 0.1F;

  public bool Showing { get; private set; }
	public bool InputEnabled { get; set; } = true;
  public Tile.PartitionTypeEnum HoverPartition;
  public Display.ITile HoveredTile { get; private set; }
	public Vector2I[] Joystick { get; private set; } = [new(0, 0), new(0, 0)];  
	public RayCast3D RayCast { private get; set; }

  private Sprite2D crosshair;
  private HoverType hoverType = HoverType.NORMAL;
  private Sprite3D[] hoverSprites;
  private Display.Piece mHoveredPiece;

  // Called when the node enters the scene tree for the first time.
  public override void _Ready() {
		crosshair = GetNode<Sprite2D>("Crosshair");

		hoverSprites = new Sprite3D[Enum.GetNames(typeof(HoverType)).Length];
		hoverSprites[(int) HoverType.NORMAL] = GetNode<Sprite3D>("Hover");
		hoverSprites[(int) HoverType.MOVE] = GetNode<Sprite3D>("Hover Move");
		hoverSprites[(int) HoverType.INTERACT] = GetNode<Sprite3D>("Hover Interact");
		hoverSprites[(int) HoverType.CAST] = GetNode<Sprite3D>("Hover Cast");

		foreach (Sprite3D hoverSprite in hoverSprites) {
			hoverSprite.Visible = false;
			hoverSprite.PixelSize = 1F / hoverSprite.Texture.GetSize().X;
		}

		HoverPartition = Tile.MAX_PARTITION;
		Show();
  }

  // Called every frame. 'delta' is the elapsed time since the previous frame.
  public override void _Process(double delta) {
	
  }

  public override void _PhysicsProcess(double delta){
		if (RayCast != null && RayCast.IsColliding()) {
			MeshInstance3D collidedMesh = ((StaticBody3D) RayCast.GetCollider()).GetParent<MeshInstance3D>();
			if (collidedMesh is Display.Piece displayPiece) {
				if (displayPiece != mHoveredPiece) {
					mHoveredPiece?.Colorize(ColorizeEnum.NONE);
					mHoveredPiece = displayPiece;
					displayPiece.Colorize(ColorizeEnum.HOVER);
				}
			}
		} else {
			mHoveredPiece?.Colorize(ColorizeEnum.NONE);
		  mHoveredPiece = null;
		}
  }

  public override void _Input(InputEvent @event) {
		if (!Showing || !InputEnabled) { return; }

		if (Input.IsActionJustPressed("d_left")) {
			
		}
		if (Input.IsActionJustPressed("d_right")) {
			
		}

		int horizontalPan = 0, verticalPan = 0;
		if (Input.IsActionPressed("pan_left")) {
			horizontalPan -= 1;
		}
		if (Input.IsActionPressed("pan_right")) {
			horizontalPan += 1;
		}
		if (Input.IsActionPressed("pan_up")) {
			verticalPan -= 1;
		}
		if (Input.IsActionPressed("pan_down")) {
			verticalPan += 1;
		}
		Joystick[0] = new Vector2I(horizontalPan, verticalPan);

		if (Input.IsActionJustPressed("pass")) {
			SelectMisc?.Invoke(SelectTypeEnum.FINAL);
		}
		if (Input.IsActionJustPressed("select")) {
			if (mHoveredPiece != null) {
				SelectPiece?.Invoke(mHoveredPiece);
			}
			SelectTile?.Invoke(HoveredTile);
		}
		if (Input.IsActionJustPressed("hand")) {
			GD.Print("BoardView: Going to hand view from board view.");
			SelectMisc?.Invoke(SelectTypeEnum.PARTIAL);
		}
		if (Input.IsActionJustPressed("detail")) {
			SelectTile?.Invoke(HoveredTile);
			// TODO: Do SelectPiece instead if hovering a piece.
		}
		if (Input.IsActionJustPressed("surrender")) {
			SelectMisc?.Invoke(SelectTypeEnum.ALT);
		}
  }

  public delegate bool SelectPieceDelegate(Display.Piece piece);
  public SelectPieceDelegate SelectPiece;
  public delegate bool SelectTileDelegate(Display.ITile tile);
  public SelectTileDelegate SelectTile;
  public delegate bool SelectActivityDelegate(Activity activity);
  public SelectActivityDelegate SelectActivity;
	public delegate bool SelectMiscDelegate(SelectTypeEnum selectTypeEnum);
	public SelectMiscDelegate SelectMisc;

  public void SetViewPortRect(Rect2 viewPortRect) {
	  crosshair.Position = new Vector2(viewPortRect.Size.X / 2, viewPortRect.Size.Y / 2);
  }

  public Vector2I GetHoverCoordinate(Vector2 point) {
		return GetHoverCoordinate(point, HoverPartition);
  }
  public static Vector2I GetHoverCoordinate(Vector2 point, Tile.PartitionTypeEnum partitionType) {
		return new Vector2I(
			(int) Math.Floor(point.X / Display.TileWithMesh.MESH_SIZE * Mathf.Pow(2, (int) partitionType)),
			(int) Math.Floor(point.Y / Display.TileWithMesh.MESH_SIZE * Mathf.Pow(2, (int) partitionType))
		);
  }

  public void Hover(Display.ITile tile, bool readyToCast) {
	if (tile == null) {
	  hoverSprites[(int) this.hoverType].Visible = false;
	  return;
	}

	HoveredTile = tile;

	HoverType hoverType = readyToCast ? HoverType.CAST : HoverType.NORMAL;
	hoverSprites[(int) hoverType].Visible = true;
	if (this.hoverType != hoverType) {
	  hoverSprites[(int) this.hoverType].Visible = false;
	  this.hoverType = hoverType;
	}

	hoverSprites[(int) hoverType].GlobalPosition = tile.Position
	  + new Vector3(0, HOVER_SPRITE_LIFT, 0);
	hoverSprites[(int) hoverType].Scale = new Vector3(tile.Size, tile.Size, 1);
  }

  public new void Show() {
		base.Show();
		crosshair.Visible = true;
		Showing = true;
  }

  public new void Hide() {
		base.Hide();
		crosshair.Visible = false;
		Showing = false;
  }
}
}
