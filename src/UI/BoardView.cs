using Godot;
using System;
using Match;
using Main;

namespace UI {
public partial class BoardView : CanvasLayer, IView {
	// white, yellow, red, orange, blue, magenta
  private enum HoverType { NORMAL, TOWARD, AWAY, INTERACT, CAST, REACT }
  private const float HOVER_SPRITE_LIFT = 0.1F;

	private readonly bool[] panPressed = [false, false, false, false];

  public bool Showing { get; private set; }
	public bool InputEnabled { get; set; } = true;
  public Tile.PartitionTypeEnum HoverPartition;
  public Display.ITile HoveredTile { get; private set; }
	public Vector2I[] Joystick { get; private set; } = [new(0, 0), new(0, 0)];  
	public RayCast3D RayCast { private get; set; }

  private Sprite2D crosshair;
  private HoverType hoverType = HoverType.NORMAL;
  private Sprite3D[] hoverSprites;
	private Label commandInfoDisplay;
  private Display.Piece mHoveredPiece;

  // Called when the node enters the scene tree for the first time.
  public override void _Ready() {
		crosshair = GetNode<Sprite2D>("Crosshair");

		hoverSprites = new Sprite3D[Enum.GetNames(typeof(HoverType)).Length];
		for (int i = 0; i < hoverSprites.Length; i++) {
			string enumName = ((HoverType) i).ToString();
			hoverSprites[i] = GetNode<Sprite3D>("Hover " + enumName[0] + enumName[1..].ToLower());
		}

		foreach (Sprite3D hoverSprite in hoverSprites) {
			hoverSprite.Visible = false;
			hoverSprite.PixelSize = 1F / hoverSprite.Texture.GetSize().X;
		}

		commandInfoDisplay = GetNode<Label>("Command Info");

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
				Hover(displayPiece);
			}
		} else {
			Hover((Display.Piece) null);
		}
  }

  public void Input(UI.InputType inputType, bool press) {
		if (!Showing || !InputEnabled) { return; }

		if (inputType == UI.InputType.D_LEFT && press) {
			
		}
		if (inputType == UI.InputType.D_RIGHT && press) {
			
		}

		int horizontalPan = Joystick[0].X, verticalPan = Joystick[0].Y;
		if (inputType == UI.InputType.PAN_LEFT) {
		panPressed[(int) DirectionEnum.WEST] = press;
			horizontalPan = press ? -1 : panPressed[(int) DirectionEnum.EAST] ? 1 : 0;
		}
		if (inputType == UI.InputType.PAN_RIGHT) {
			panPressed[(int) DirectionEnum.EAST] = press;
			horizontalPan = press ? 1 : panPressed[(int) DirectionEnum.WEST] ? -1 : 0;
		}
		if (inputType == UI.InputType.PAN_UP) {
			panPressed[(int) DirectionEnum.NORTH] = press;
			verticalPan = press ? -1 : panPressed[(int) DirectionEnum.SOUTH] ? 1 : 0;
		}
		if (inputType == UI.InputType.PAN_DOWN) {
			panPressed[(int) DirectionEnum.SOUTH] = press;
			verticalPan = press ? 1 : panPressed[(int) DirectionEnum.NORTH] ? -1 : 0;
		}
		Joystick[0] = new Vector2I(horizontalPan, verticalPan);

		if (inputType == UI.InputType.PASS && press) {
			Select?.Invoke(null, WizardStep.SelectType.PASS);
		}
		if (inputType == UI.InputType.SELECT && press) {
			if (mHoveredPiece != null) {
				Select?.Invoke(mHoveredPiece, WizardStep.SelectType.STANDARD);
			} else if (HoveredTile != null) {
				Select?.Invoke(HoveredTile, WizardStep.SelectType.STANDARD);
			}
		}
		if (inputType == UI.InputType.HAND && press) {
			if (mHoveredPiece != null) {
				Select?.Invoke(mHoveredPiece, WizardStep.SelectType.HAND);
			} else if (HoveredTile != null) {
				Select?.Invoke(HoveredTile, WizardStep.SelectType.HAND);
			}
		}
		if (inputType == UI.InputType.DETAIL && press) {
			if (mHoveredPiece != null) {
				Select?.Invoke(mHoveredPiece, WizardStep.SelectType.DETAIL);
			} else if (HoveredTile != null) {
				Select?.Invoke(HoveredTile, WizardStep.SelectType.DETAIL);
			}
		}
		if (inputType == UI.InputType.SURRENDER) {
			Select?.Invoke(null, WizardStep.SelectType.SURRENDER);
		}
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

	public void Reset() {
		
	}

	public delegate void SelectDelegate(object target, WizardStep.SelectType selectType);
	public SelectDelegate Select;

  public void SetViewPortRect(Rect2 viewPortRect) {
	  crosshair.Position = new Vector2(viewPortRect.Size.X / 2, viewPortRect.Size.Y / 2);
  }

	public void SetCommand(Command command) {
		switch (command.Type) {
			case Command.CommandType.APPROACH:
			case Command.CommandType.OBSTRUCT:
				hoverType = HoverType.TOWARD;
				break;
			case Command.CommandType.AVOID:
				hoverType = HoverType.AWAY;
				break;
			case Command.CommandType.INTERACT:
				// TODO - check whether this should be CAST instead
				hoverType = HoverType.INTERACT;
				break;
			case Command.CommandType.INTERCEPT:
			case Command.CommandType.LINGER:
			case Command.CommandType.BRIDLE:
				hoverType = HoverType.REACT;
				break;
		}
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

  public void Hover(Display.ITile tile) {
		if (tile == null) {
			hoverSprites[(int) this.hoverType].Visible = false;
			return;
		}

		HoveredTile = tile;

		hoverSprites[(int) hoverType].Visible = true;
		hoverSprites[(int) hoverType].GlobalPosition = tile.Position
			+ new Vector3(0, HOVER_SPRITE_LIFT, 0);
		hoverSprites[(int) hoverType].Scale = new Vector3(tile.Size, tile.Size, 1);
  }

	private void Hover(Display.Piece piece) {
		if (piece == null) {
			mHoveredPiece?.Colorize(ColorizeEnum.NONE);
			mHoveredPiece = null;

			// Clear the command info.
			commandInfoDisplay.Text = "";
		} else if (piece != mHoveredPiece) {
			mHoveredPiece?.Colorize(ColorizeEnum.NONE);
			mHoveredPiece = piece;
			piece.Colorize(ColorizeEnum.HOVER);

			// Update the command info.
			// commandInfoDisplay.Text = piece.GamePiece.GetCommandDescriptions();
		}
	}
}
}
