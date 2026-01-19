using Display;
using Godot;
using Match;
using System;
using System.Collections.Generic;
using System.Linq;

namespace UI {
public partial class UI : Node {
  private const float CAMERA_SPEED = 1F;

  private BoardView mBoardView;
  private HandView mHandView;
	private CommandView mCommandView;
  private PassView mPassView;
	private SurrenderView mSurrenderView;
  private DetailView mDetailView;
  private Camera3D camera;

	private WizardStep mWizardStep;
  private Vector2 mHoverPoint;

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
  public override void _Process(double delta) { }

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
		// 	if (Input.IsKeyPressed(Key.G)) {
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

	private void OnSelect(object target, WizardStep.SelectType selectType) {
		WizardStep newStep = mWizardStep.Progress(target, selectType);
		if (newStep == null) {
			// Play sound indicating blocked selection.
			GD.Print("Blocked step progression.");
		} else {
			mWizardStep = newStep;
			GD.Print("Progressed to new step with view state: " + mWizardStep.ViewState.ToString() + ".");
		}
	}

	// public static ViewStateEnum DetermineViewStateForTargetTypes(Type[] targetTypes) {
	// 	if (targetTypes.Length == 0) {
	// 		throw new Exception("No target types provided for determining ViewState.");
	// 	} else if (targetTypes.Length == 1) {
	// 		return DetermineViewStateForTargetType(targetTypes[0]);
	// 	} else {
	// 		List<ViewStateEnum> viewStates = [];
	// 		foreach (Type targetType in targetTypes) {
	// 			viewStates.Add(DetermineViewStateForTargetType(targetType));
	// 		}
	// 		if (viewStates.Distinct().Count() == 1) {
	// 			return viewStates[0];
	// 		} else {
	// 			throw new Exception(
	// 				"Multiple differing target types provided for determining ViewState: "
	// 				+ string.Join(", ", targetTypes.Select(t => t.ToString()))
	// 				+ "."
	// 			);
	// 		}
	// 	}
	// }

	// public static ViewStateEnum DetermineViewStateForTargetType(Type targetType) {
	// 	if (targetType == typeof(Match.Piece)) {
	// 		return ViewStateEnum.DESIGNATE_BOARD;
	// 	} else if (targetType == typeof(Tile)) {
	// 		return ViewStateEnum.DESIGNATE_BOARD;
	// 	} else if (targetType == typeof(Activity)) {
	// 		return ViewStateEnum.DESIGNATE_BOARD;
	// 	} else if (targetType == typeof(Card)) {
	// 		return ViewStateEnum.DESIGNATE_HAND;
	// 	} else if (targetType == typeof(string)) {
	// 		return ViewStateEnum.DESIGNATE_LIST;
	// 	} else if (targetType == typeof(Command)) {
	// 		return ViewStateEnum.COMMAND_LIST;
	// 	} else {
	// 		throw new Exception("Unknown target type " + targetType.ToString() + " for determining ViewState.");
	// 	}
	// }

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
				IView.State.DESIGNATE_LIST => mDetailView,
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
