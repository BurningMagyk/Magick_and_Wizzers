using Godot;
using Main;
using System;

namespace UI {
	public partial class UI : Node {
	  private const float CAMERA_SPEED = 1F;

	  private BoardView boardView;
	  private HandView handView;
	  private Camera3D camera;

	  private Vector2I[] joystick = new Vector2I[] {
	  new Vector2I(0, 0),
	  new Vector2I(0, 0),
	  };

	private Vector2 hoverPoint;

	  // Called when the node enters the scene tree for the first time.
	  public override void _Ready() {
	  boardView = GetNode<BoardView>("Board View");
		handView = GetNode<HandView>("Hand View");
		camera = GetNode<Camera3D>("Camera");

		boardView.SetViewPortRect(camera.GetViewport().GetVisibleRect());
		handView.SetViewPortRect(camera.GetViewport().GetVisibleRect());
		handView.Played += OnPlayed;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta) { }

	public override void _PhysicsProcess(double delta) {
		Vector3 oldCameraPosition = camera.Position;
		camera.Position += new Vector3(joystick[0].X, 0, joystick[0].Y) * CAMERA_SPEED;

		if (oldCameraPosition != camera.Position) {
			Vector3 hoverPoint3D = GetPlaneIntersection(camera.Position, camera.GlobalTransform.Basis.Z);
			hoverPoint = new Vector2(hoverPoint3D.X, hoverPoint3D.Z);

			EmitSignal(SignalName.Moved, camera.Position, boardView.HoveredTile);
		}
	}

	public override void _Input(InputEvent @event) {
		if (Input.IsActionJustPressed("toggle_hand")) {
			if (handView.Showing) {
				handView.Hide();
			} else {
				handView.Show();
			}
	  	}
	  	EmitSignal(SignalName.ChangedHoverType, boardView.HoveredTile);

		if (Input.IsActionJustPressed("pass_turn")) {
			EmitSignal(SignalName.PassTurn);
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
	public delegate void ChangedHoverTypeEventHandler(Game.Tile hoveredTile);
	[Signal]
	public delegate void MovedEventHandler(Vector2 newPosition, Game.Tile oldHoveredTile);
	[Signal]
	public delegate void PlayedFromHandEventHandler(Card card);
	[Signal]
	public delegate void PassTurnEventHandler();

	public void HoverTile(Game.Tile tile) {
		boardView.Hover(tile, handView.Showing);
	}

	public Vector2I GetHoverCoordinate() {
		if (handView.Showing) {
			return boardView.GetHoverCoordinate(hoverPoint, handView.HoverPartition);
		}
		return boardView.GetHoverCoordinate(hoverPoint);
	}
	public Game.Tile.PartitionType GetHoverPartition() {
		if (handView.Showing) { return handView.HoverPartition; }
		return boardView.HoverPartition;
	}

	private void OnPlayed(Card card) {
		EmitSignal(SignalName.PlayedFromHand, card);
	}

	private static Vector3 GetPlaneIntersection(Vector3 origin, Vector3 direction) {
	  Vector3 planeNormal = new Vector3(0, 1, 0);
	  float t = -planeNormal.Dot(origin) / planeNormal.Dot(direction);
	  return origin + direction * t;
	}
  }
}
