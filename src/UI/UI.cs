using Godot;
using System;

namespace UI {
  public partial class UI : Node {
    private const float CAMERA_SPEED = 1F;

    private BoardView mBoardView;
    private HandView mHandView;
    private Camera3D camera;

    private Vector2I[] joystick = new Vector2I[] {
    new Vector2I(0, 0),
    new Vector2I(0, 0),
    };

  private Vector2 hoverPoint;

  // Called when the node enters the scene tree for the first time.
  public override void _Ready() {
    mBoardView = GetNode<BoardView>("Board View");
    mHandView = GetNode<HandView>("Hand View");
    camera = GetNode<Camera3D>("Camera");

    mBoardView.SetViewPortRect(camera.GetViewport().GetVisibleRect());
    mBoardView.Played += OnPlayed;
    mHandView.SetViewPortRect(camera.GetViewport().GetVisibleRect());
    mHandView.Selected += OnSelected;
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
        EmitSignal(SignalName.Moved, hoverPoint, oldHoverPoint);
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
      EmitSignal(SignalName.ChangedHoverType, GetHoverCoordinate(), (int) GetHoverPartition());
      }

    if (Input.IsActionJustPressed("pass_round")) {
      EmitSignal(SignalName.PassRound);
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

  [Signal]
  public delegate void ChangedHoverTypeEventHandler(
    Vector2I hoveredTileCoordinate,
    int hoveredTilePartitionType);
  [Signal]
  public delegate void MovedEventHandler(Vector2 oldPoint, Vector2 newPoint);
  [Signal]
  public delegate void SelectedEventHandler(Card card);
  [Signal]
  public delegate void PlayedEventHandler(Card card);
  [Signal]
  public delegate void PassRoundEventHandler();

  public void HoverTile(Match.Tile tile) {
    if (mHandView.Showing) {
      if (mHandView.Mode == HandView.HandViewMode.SELECTED) {
        mBoardView.Hover(tile, true); // Make hovered tile show up as ready to cast.
      } else {
        mBoardView.Hover(null, true); // Hovered tile doesn't show up.
      }
    } else {
      mBoardView.Hover(tile, false); // Hovered tile shows up as normal.
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

  private void OnSelected(Card card) {
    // Does anything need to go in here?
  }

  private void OnPlayed(Card card) {
    EmitSignal(SignalName.Played, card);
  }

  private static Vector3 GetPlaneIntersection(Vector3 origin, Vector3 direction) {
    Vector3 planeNormal = new Vector3(0, 1, 0);
    float t = -planeNormal.Dot(origin) / planeNormal.Dot(direction);
    return origin + direction * t;
  }
  }
}
