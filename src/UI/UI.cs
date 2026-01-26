using Display;
using Godot;
using Match;
using System;
using System.Collections.Generic;
using System.Linq;

namespace UI {
public partial class UI : Node {
  private const float CAMERA_SPEED = 1F;
	private static readonly Dictionary<string, InputType> STRING_TO_INPUT_TYPE = new() {
		{"select", InputType.SELECT},
		{"hand", InputType.HAND},
		{"back", InputType.BACK},
		{"detail", InputType.DETAIL},
		{"pass", InputType.PASS},
		{"surrender", InputType.SURRENDER},
		{"pan_left", InputType.PAN_LEFT},
		{"pan_right", InputType.PAN_RIGHT},
		{"pan_up", InputType.PAN_UP},
		{"pan_down", InputType.PAN_DOWN},
		{"d_left", InputType.D_LEFT},
		{"d_right", InputType.D_RIGHT},
		{"d_up", InputType.D_UP},
		{"d_down", InputType.D_DOWN}
	};

  private BoardView mBoardView;
  private HandView mHandView;
	private CommandView mCommandView;
  private PassView mPassView;
	private SurrenderView mSurrenderView;
  private DetailView mDetailView;
  private Camera3D camera;

	private WizardStep mWizardStep;
  private Vector2 mHoverPoint;

	public enum InputType {
		SELECT,
		HAND,
		BACK,
		DETAIL,
		PASS,
		SURRENDER,
		PAN_LEFT,
		PAN_RIGHT,
		PAN_UP,
		PAN_DOWN,
		D_LEFT,
		D_RIGHT,
		D_UP,
		D_DOWN
	}

  // Called when the node enters the scene tree for the first time.
  public override void _Ready() {
		camera = GetNode<Camera3D>("Camera");

		mBoardView = GetNode<BoardView>("Board View");
		mHandView = GetNode<HandView>("Hand View");
		mCommandView = GetNode<CommandView>("Command View");
		mPassView = GetNode<PassView>("Pass View");
		mSurrenderView = GetNode<SurrenderView>("Surrender View");
		mDetailView = GetNode<DetailView>("Detail View");

		mBoardView.SetViewPortRect(camera.GetViewport().GetVisibleRect());
		mHandView.SetViewPortRect(camera.GetViewport().GetVisibleRect());
		mCommandView.SetViewPortRect(camera.GetViewport().GetVisibleRect());
		mPassView.SetViewPortRect(camera.GetViewport().GetVisibleRect());
		mSurrenderView.SetViewPortRect(camera.GetViewport().GetVisibleRect());
		mDetailView.SetViewPortRect(camera.GetViewport().GetVisibleRect());

		mBoardView.Select += OnSelect;
		mHandView.Select += OnSelect;
		mCommandView.Select += OnSelect;
		mPassView.Select += OnSelect;
		mSurrenderView.Select += OnSelect;
		mDetailView.Select += OnSelect;

		mBoardView.RayCast = camera.GetNode<RayCast3D>("Ray Cast");

		mWizardStep = WizardStep.ROOT;
  }

  // Called every frame. 'delta' is the elapsed time since the previous frame.
  public override void _Process(double delta) {

	}

  public override void _PhysicsProcess(double delta) {
		Vector3 oldCameraPosition = camera.Position;
		camera.Position += new Vector3(mBoardView.Joystick[0].X, 0, mBoardView.Joystick[0].Y) * CAMERA_SPEED;

		if (oldCameraPosition != camera.Position) {
				Vector3 hoverPoint3D = GetPlaneIntersection(camera.Position, camera.GlobalTransform.Basis.Z);
			Vector2 oldHoverPoint = mHoverPoint;
			mHoverPoint = new Vector2(hoverPoint3D.X, hoverPoint3D.Z);

			if (oldHoverPoint != mHoverPoint) {
				Moved?.Invoke(oldHoverPoint, mHoverPoint);
			}
		}
  }

  public override void _Input(InputEvent @event) {
		if (@event is not InputEventKey && @event is not InputEventJoypadButton && @event is not InputEventJoypadMotion) {
			return;
		}

		foreach (var entry in STRING_TO_INPUT_TYPE) {
			if (entry.Value == InputType.BACK && Input.IsActionJustPressed(entry.Key)) {
				OnSelect(null, WizardStep.SelectType.BACK);
				continue;
			}

			if (Input.IsActionJustPressed(entry.Key)) {
				ViewFrom(mWizardStep.ViewState).Input(entry.Value, true);
			} else if (Input.IsActionJustReleased(entry.Key)) {
				ViewFrom(mWizardStep.ViewState).Input(entry.Value, false);
			}
		}
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

  public void HoverTile(ITile tile) {
	if (mHandView.Showing) {
	  mBoardView.Hover(null, true); // Hovered tile doesn't show up.
	} else {
	  mBoardView.Hover(tile, false); // Hovered tile shows up as normal.
	  // mBoardView.Hover(tile, true); // Make hovered tile show up as ready to cast.
	}
  }

  public Vector2I GetHoverCoordinate() {
		return mBoardView.GetHoverCoordinate(mHoverPoint);
		}
		public Vector2I GetHoverCoordinate(Vector2 point) {
		if (mHandView.Showing) {
			return BoardView.GetHoverCoordinate(point, mHandView.HoverPartition);
		}
		return mBoardView.GetHoverCoordinate(point);
  }
  public Tile.PartitionTypeEnum GetHoverPartition() {
		if (mHandView.Showing) { return mHandView.HoverPartition; }
	  return mBoardView.HoverPartition;
  }

	public void GoToView(IView.State viewState) {
		// Hide all views except for board view that we just disable input.
		mBoardView.InputEnabled = false;
		mHandView.Hide();
		mCommandView.Hide();
		mPassView.Hide();
		mSurrenderView.Hide();
		mDetailView.Hide();

		// Show the appropriate view.
		IView view = ViewFrom(viewState);
		view.Show();
		if (view is BoardView) {
			mBoardView.InputEnabled = true;
		}
	}

	private void OnSelect(object target, WizardStep.SelectType selectType) {
		WizardStep newStep = mWizardStep.Progress(target, selectType);
		if (newStep == null) {
			// Play sound indicating blocked selection.
			GD.Print("Blocked step progression.");
		} else {
			mWizardStep = newStep;
			// Play sound indicating successful selection.
			GD.Print("Progressed to new step with view state: " + mWizardStep.ViewState.ToString() + ".");
		  GoToView(mWizardStep.ViewState);
		}
	}

	private IView ViewFrom(IView.State viewStateEnum) {
	  return viewStateEnum switch {
				IView.State.PASS => mPassView,
				IView.State.SURRENDER => mSurrenderView,
				IView.State.DETAIL => mDetailView,
		    IView.State.MEANDER_BOARD => mBoardView,
		    IView.State.MEANDER_HAND => mHandView,
				IView.State.COMMAND_LIST => mCommandView,
				IView.State.DESIGNATE_BOARD => mBoardView,
				IView.State.DESIGNATE_HAND => mHandView,
				IView.State.DESIGNATE_LIST => mDetailView, // TODO - this is wrong
				IView.State.THEATER => mBoardView,
				IView.State.CAST => mHandView,
		_ => throw new Exception("Cannot get view for ViewStateEnum: \"" + viewStateEnum.ToString() + "\"."),
	  };
	}

  private static Vector3 GetPlaneIntersection(Vector3 origin, Vector3 direction) {
		Vector3 planeNormal = new(0, 1, 0);
		float t = -planeNormal.Dot(origin) / planeNormal.Dot(direction);
		return origin + direction * t;
  }
}
}
