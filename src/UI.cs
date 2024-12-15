using Godot;
using Main;
using System;

namespace UI {
  public partial class UI : Node {
    private const float CAMERA_SPEED = 5F;

    private BoardView boardView;
    private HandView handView;
    private Camera3D camera;

    private Vector2[] joystick = new Vector2[] {
      new Vector2(0, 0),
      new Vector2(0, 0),
    };

    // Called when the node enters the scene tree for the first time.
    public override void _Ready() {
      boardView = GetNode<BoardView>("Board View");
      handView = GetNode<HandView>("Hand View");
      camera = GetNode<Camera3D>("Camera");

      handView.SetViewPortRect(camera.GetViewport().GetVisibleRect());
      handView.Played += OnPlayed;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta) {
      
    }

    public override void _PhysicsProcess(double delta) {
      Vector3 oldCameraPosition = camera.Position;
      camera.Position += new Vector3(joystick[0].X, joystick[0].Y, 10) * CAMERA_SPEED;
      // It seems that calling Align repositions the camera.
      // camera.Align();

      if (oldCameraPosition != camera.Position) {
        EmitSignal(SignalName.Moved, camera.Position, boardView.HoveredTile);
      }
    }

    public override void _Input(InputEvent @event) {
      if (Input.IsActionJustPressed("ui_hand")) {
        if (handView.Showing) {
          handView.Hide();
        }
        else {
          handView.Show();
        }
        EmitSignal(SignalName.ChangedHoverType, boardView.HoveredTile);
      }

      float horizontalPan = 0, verticalPan = 0;
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
      joystick[0] = new Vector2(horizontalPan, verticalPan);

    // 	if (Input.IsKeyPressed(Key.F)) {
    // 		GD.Print("test");
    // 	}
    }

    [Signal]
    public delegate void ChangedHoverTypeEventHandler(Tile hoveredTile);
    [Signal]
    public delegate void MovedEventHandler(Vector2 newPosition, Tile oldHoveredTile);
    [Signal]
    public delegate void PlayedFromHandEventHandler(Card card);

    public void HoverTile(Tile tile) {
      boardView.Hover(tile, handView.Showing);
    }

    public Vector2I GetHoverPoint() {
      if (handView.Showing) {
        return boardView.GetHoverPoint(camera, handView.HoverPartition);
      }
      return boardView.GetHoverPoint(camera);
    }
    public Main.Tile.PartitionType GetHoverPartition() {
      if (handView.Showing) { return handView.HoverPartition; }
      return boardView.HoverPartition;
    }

    private void OnPlayed(Card card) {
      EmitSignal(SignalName.PlayedFromHand, card);
    }
  }
}
