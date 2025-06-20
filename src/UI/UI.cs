using Display;
using Godot;
using Match;
using System;
using System.Diagnostics;

namespace UI {
public partial class UI : Node {
  private const float CAMERA_SPEED = 1F;

  private ViewState mViewState = new ViewState(ViewStateEnum.MEANDER_BOARD);
  private BoardView mBoardView;
  private CommandView mCommandView;
  private HandView mHandView;
  private DetailView mDetailView;
  private Camera3D camera;

  private Vector2I[] joystick = new Vector2I[] {
    new Vector2I(0, 0),
    new Vector2I(0, 0),
  };

  	private Vector2 hoverPoint;

  // Called when the node enters the scene tree for the first time.
  public override void _Ready() {
	camera = GetNode<Camera3D>("Camera");

	mBoardView = GetNode<BoardView>("Board View");
	mHandView = GetNode<HandView>("Hand View");
	mCommandView = GetNode<CommandView>("Command View");
	mDetailView = GetNode<DetailView>("Detail View");

	mBoardView.SetViewPortRect(camera.GetViewport().GetVisibleRect());
	mHandView.SetViewPortRect(camera.GetViewport().GetVisibleRect());
	mCommandView.SetViewPortRect(camera.GetViewport().GetVisibleRect());
	mDetailView.SetViewPortRect(camera.GetViewport().GetVisibleRect());

	mBoardView.SelectPiece += OnSelectPiece;
	mBoardView.SelectTile += OnSelectTile;
	mBoardView.SelectActivity += OnSelectActivity;
	mHandView.SelectCard += OnSelectCard;
	mCommandView.SelectCommand += OnSelectCommand;
	mDetailView.SelectItem += OnSelectItem; // only for spook purposes
  }

  // Called every frame. 'delta' is the elapsed time since the previous frame.
  public override void _Process(double delta) { }

  public override void _PhysicsProcess(double delta) {
	Vector3 oldCameraPosition = camera.Position;
	camera.Position += new Vector3(joystick[0].X, 0, joystick[0].Y) * CAMERA_SPEED;

	if (oldCameraPosition != camera.Position) {
      Vector3 hoverPoint3D = GetPlaneIntersection(camera.Position, camera.GlobalTransform.Basis.Z);
	  Vector2 oldHoverPoint = hoverPoint;
	  hoverPoint = new Vector2(hoverPoint3D.X, hoverPoint3D.Z);

	  if (oldHoverPoint != hoverPoint) {
		Moved?.Invoke(oldHoverPoint, hoverPoint);
	  }
	}
  }

  public override void _Input(InputEvent @event) {
	if (Input.IsActionJustPressed("toggle_hand")) {
      if (mHandView.Showing) {
		mHandView.Hide();
	  } else {
		mHandView.Show();
	  }
	  ChangedHoverType?.Invoke(GetHoverCoordinate(), (int) GetHoverPartition());
	}

	if (Input.IsActionJustPressed("pass_round")) {
	  PassRound?.Invoke();
	}

	int horizontalPan = 0, verticalPan = 0;
	if (Input.IsActionPressed("left")) {
	  horizontalPan -= 1;
	}
	if (Input.IsActionPressed("right")) {
	  horizontalPan += 1;
	}
	if (Input.IsActionPressed("up")) {
	  verticalPan -= 1;
	}
	if (Input.IsActionPressed("down")) {
	  verticalPan += 1;
	}
	joystick[0] = new Vector2I(horizontalPan, verticalPan);

	// 	if (Input.IsKeyPressed(Key.F)) {
	// 		GD.Print("test");
	// 	}
  }

  public delegate void ChangedHoverTypeDelegate(Vector2I hoveredTileCoordinate, int hoveredTilePartitionType);
  public ChangedHoverTypeDelegate ChangedHoverType;

  public delegate void MovedDelegate(Vector2 oldPoint, Vector2 newPoint);
  public MovedDelegate Moved;

  public delegate void SelectedDelegate(Card card);
  public SelectedDelegate Selected;
	
  public delegate void PlayedDelegate(Card card);
  public PlayedDelegate Played;
	
  public delegate void PassRoundDelegate();
  public PassRoundDelegate PassRound;

  public void HoverTile(Display.ITile tile) {
    if (mHandView.Showing) {
      mBoardView.Hover(null, true); // Hovered tile doesn't show up.
	} else {
	  mBoardView.Hover(tile, false); // Hovered tile shows up as normal.
	  // mBoardView.Hover(tile, true); // Make hovered tile show up as ready to cast.
	}
  }

  public Vector2I GetHoverCoordinate() {
	return mBoardView.GetHoverCoordinate(hoverPoint);
  }
  public Vector2I GetHoverCoordinate(Vector2 point) {
	if (mHandView.Showing) {
      return mBoardView.GetHoverCoordinate(point, mHandView.HoverPartition);
	}
	return mBoardView.GetHoverCoordinate(point);
  }
  public Match.Tile.PartitionTypeEnum GetHoverPartition() {
	if (mHandView.Showing) { return mHandView.HoverPartition; }
      return mBoardView.HoverPartition;
  }

  private void OnSelectPiece(Display.Piece piece) {
		
  }
  private void OnSelectTile(Display.ITile tile) {
			
  }
  private void OnSelectActivity(Match.Activity activity) {
		
  }
  private void OnSelectCard(Card card){

  }
  private void OnSelectCommand(Command command) {
		
  }
  private void OnSelectItem() {
		
  }

  private class ViewState {
	ViewState prev;
	ViewStateEnum viewStateEnum;

	public ViewState(ViewStateEnum viewStateEnum, ViewState prev = null) {
	  this.prev = prev;
	  this.viewStateEnum = viewStateEnum;
	}
  }

  private static Vector3 GetPlaneIntersection(Vector3 origin, Vector3 direction) {
	Vector3 planeNormal = new Vector3(0, 1, 0);
	float t = -planeNormal.Dot(origin) / planeNormal.Dot(direction);
	return origin + direction * t;
  }
}
}
