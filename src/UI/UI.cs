using Display;
using Godot;
using Match;
using System;
using System.Collections.Generic;
using System.Linq;

namespace UI {
public partial class UI : Node {
  private const float CAMERA_SPEED = 1F;

  private ViewState mViewState = new();
  private BoardView mBoardView;
  private HandView mHandView;
	private CommandView mCommandView;
  private PassView mPassView;
	private SurrenderView mSurrenderView;
  private DetailView mDetailView;
  private Camera3D camera;

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

		mBoardView.SelectPiece += OnSelectPiece;
		mBoardView.SelectTile += OnSelectTile;
		mBoardView.SelectActivity += OnSelectActivity;
		mBoardView.SelectMisc += OnSelectMisc;
		mHandView.SelectCard += OnSelectCard;
		mHandView.GoBack += RegressViewState;
		mCommandView.SelectCommand += OnSelectCommand;
		mCommandView.GoBack += RegressViewState;
		mPassView.ConfirmPass += OnConfirmPass;
		mPassView.GoBack += RegressViewState;
		mSurrenderView.SelectItem += OnSelectItem;
		mSurrenderView.GoBack += RegressViewState;
		mDetailView.SelectItem += OnSelectItem;
		mDetailView.GoBack += RegressViewState;

		mBoardView.RayCast = camera.GetNode<RayCast3D>("Ray Cast");
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

	public void ProgressToTheater() {
		ProgressViewState(ViewStateEnum.THEATER);
	}

	public void ProgressFromTheater() {
		ProgressViewState(ViewStateEnum.MEANDER_BOARD);
	}

	/// <summary> Called from BoardView using SelectPiece?.Invoke </summary>
	/// <returns> True if the selection was successful </returns>
  private SelectTypeEnum OnSelectPiece(Display.Piece piece, Command command, SelectTypeEnum selectTypeEnum) {
		if (selectTypeEnum == SelectTypeEnum.DETAIL) {
			ProgressViewState(ViewStateEnum.DETAIL);
			return SelectTypeEnum.DETAIL;
		} else if (mViewState.ViewStateEnum == ViewStateEnum.MEANDER_BOARD) {
			mCommandView.SetActor(piece.GamePiece);
			ProgressViewState(ViewStateEnum.COMMAND_LIST);
			return SelectTypeEnum.FINAL;
		} else if (mViewState.ViewStateEnum == ViewStateEnum.DESIGNATE_BOARD) {
			if ((command.FeedTarget(piece.GamePiece) && selectTypeEnum == SelectTypeEnum.SERIES)
				|| selectTypeEnum == SelectTypeEnum.FINAL
			) {
				ProgressSteppedViewState(command);
				return SelectTypeEnum.FINAL;
			}
			// Stay in DESIGNATE_BOARD if command is not done being fed.
			return selectTypeEnum;
		}
		return SelectTypeEnum.INVALID;
  }

	/// <summary> Called from BoardView using SelectTile?.Invoke </summary>
	/// <returns> True if the selection was successful </returns>
  private SelectTypeEnum OnSelectTile(ITile tile, Command command, SelectTypeEnum selectTypeEnum) {
		if (mViewState.ViewStateEnum == ViewStateEnum.MEANDER_BOARD || selectTypeEnum == SelectTypeEnum.DETAIL) {
			ProgressViewState(ViewStateEnum.DETAIL);
			return SelectTypeEnum.DETAIL;
		} else if (mViewState.ViewStateEnum == ViewStateEnum.DESIGNATE_BOARD) {
			if ((command.FeedTarget(tile.GameTile) && selectTypeEnum == SelectTypeEnum.SERIES)
				|| selectTypeEnum == SelectTypeEnum.FINAL
			) {
				ProgressSteppedViewState(command);
				return SelectTypeEnum.FINAL;
			}
			// Stay in DESIGNATE_BOARD if command is not done being fed.
			return selectTypeEnum;
		}
		return SelectTypeEnum.INVALID;
	}

	/// <summary> Called from BoardView using SelectActivity?.Invoke </summary>
	/// <returns> True if the selection was successful </returns>
  private SelectTypeEnum OnSelectActivity(Match.Activity activity, Command command, SelectTypeEnum selectTypeEnum) {
		if (mViewState.ViewStateEnum == ViewStateEnum.DESIGNATE_BOARD) {
			if ((command.FeedTarget(activity) && selectTypeEnum == SelectTypeEnum.SERIES)
				|| selectTypeEnum == SelectTypeEnum.FINAL
			) {
				ProgressSteppedViewState(command);
				return SelectTypeEnum.FINAL;
			}
			// Stay in DESIGNATE_BOARD if command is not done being fed.
			return selectTypeEnum;
		}
		return SelectTypeEnum.INVALID;
  }

	/// <summary> Called from HandView using SelectCard?.Invoke </summary>
	/// <returns> True if the selection was successful </returns>
  private SelectTypeEnum OnSelectCard(Card card, Command command, SelectTypeEnum selectTypeEnum) {
		if (mViewState.ViewStateEnum == ViewStateEnum.MEANDER_HAND || selectTypeEnum == SelectTypeEnum.DETAIL) {
			ProgressViewState(ViewStateEnum.DETAIL);
			return SelectTypeEnum.DETAIL;
		} else if (mViewState.ViewStateEnum == ViewStateEnum.DESIGNATE_HAND) {
			if ((command.FeedTarget(card.GameCard) && selectTypeEnum == SelectTypeEnum.SERIES)
				|| selectTypeEnum == SelectTypeEnum.FINAL
			) {
				ProgressSteppedViewState(command);
				return SelectTypeEnum.FINAL;	
			}
			// Stay in DESIGNATE_HAND if command is not done being fed.
			return selectTypeEnum;
		}
		return SelectTypeEnum.INVALID;
  }

	/// <summary> Called from CommandView using SelectCommand?.Invoke </summary>
	/// <returns> True if the selection was successful </returns>
  private SelectTypeEnum OnSelectCommand(Command command, SelectTypeEnum selectTypeEnum) {
		if (mViewState.ViewStateEnum == ViewStateEnum.COMMAND_LIST) {
			if (selectTypeEnum == SelectTypeEnum.DETAIL) {
				ProgressViewState(ViewStateEnum.DETAIL);
			} else {
				ProgressSteppedViewState(command);
			}
			return SelectTypeEnum.FINAL;
		}
		return SelectTypeEnum.INVALID;
  }

	/// <summary> Called from DetailView using SelectItem?.Invoke </summary>
	/// <returns> True if the selection was successful </returns>
  private SelectTypeEnum OnSelectItem(string item, Command command, SelectTypeEnum selectTypeEnum) {
		if (mViewState.ViewStateEnum == ViewStateEnum.DESIGNATE_LIST) {
			if ((command.FeedSpec(item) && selectTypeEnum == SelectTypeEnum.SERIES)
				|| (selectTypeEnum == SelectTypeEnum.FINAL)
			) {
				ProgressSteppedViewState(command);
				return SelectTypeEnum.FINAL;
			}
			// Stay in DESIGNATE_LIST if command is not done being fed.
			return selectTypeEnum;
		} else return OnSelectItem(selectTypeEnum);
  }

	private SelectTypeEnum OnSelectItem(SelectTypeEnum selectTypeEnum) {
		if (mViewState.ViewStateEnum != ViewStateEnum.DETAIL) {
			return SelectTypeEnum.ALT;
		} else if (mViewState.ViewStateEnum == ViewStateEnum.SURRENDER) {
			if (selectTypeEnum == SelectTypeEnum.SURRENDER) {
				// This player loses the match.
				GD.Print("You lose!");
			}
			return selectTypeEnum;
		}
		return SelectTypeEnum.INVALID;
	}

	/// <summary> Called from anywhere using SelectMisc?.Invoke </summary>
	/// <returns> True if the selection was successful </returns>
	private SelectTypeEnum OnSelectMisc(SelectTypeEnum selectTypeEnum) {
		if (selectTypeEnum == SelectTypeEnum.ALT) {
			if (mViewState.ViewStateEnum == ViewStateEnum.MEANDER_BOARD) {
				ProgressViewState(ViewStateEnum.MEANDER_HAND);
				return SelectTypeEnum.FINAL;
			}
		} else if (selectTypeEnum == SelectTypeEnum.SURRENDER) {
			if (mViewState.ViewStateEnum == ViewStateEnum.MEANDER_BOARD) {
				ProgressViewState(ViewStateEnum.SURRENDER);
				return SelectTypeEnum.FINAL;
			}
		} else if (selectTypeEnum == SelectTypeEnum.FINAL) {
			ProgressViewState(ViewStateEnum.PASS);
			return SelectTypeEnum.FINAL;
		}
		return SelectTypeEnum.INVALID;
	}

	private void OnConfirmPass() {
		PassRound?.Invoke();
		// Don't need to call ProgressViewState because PassRound will force every user into ViewStateEnum.THEATER.
	}

	private void ProgressSteppedViewState(Command command) {
		ViewStateEnum steppedView = command.StepView();
		if (steppedView == ViewStateEnum.NONE) {
			ProgressViewState(ViewStateEnum.MEANDER_BOARD);
		} else if (steppedView == ViewStateEnum.DESIGNATE_HAND) {
			mHandView.SetCommand(command);
			ProgressViewState(ViewStateEnum.DESIGNATE_HAND);
		} else if (steppedView == ViewStateEnum.DESIGNATE_BOARD) {
			mBoardView.SetCommand(command);
			ProgressViewState(ViewStateEnum.DESIGNATE_BOARD);
		} else {
			throw new Exception("Command has invalid step " + steppedView.ToString() + ".");
		}
	}

	private void ProgressViewState(ViewStateEnum viewStateEnum) {
		CallDeferred(nameof(ProgressViewStateImpl), (int) viewStateEnum);
	}

  private void ProgressViewStateImpl(int viewStateEnumInt) {
		ViewStateEnum viewStateEnum = (ViewStateEnum) viewStateEnumInt;

		mViewState = mViewState.Append(viewStateEnum);

		if (viewStateEnum == ViewStateEnum.PASS) {
			GetViewForState(mViewState.Prev.ViewStateEnum).InputEnabled = false;
			mPassView.Show();
		} else if (viewStateEnum == ViewStateEnum.SURRENDER) {
			GetViewForState(mViewState.Prev.ViewStateEnum).InputEnabled = false;
			mSurrenderView.Show();
		} else if (viewStateEnum == ViewStateEnum.DETAIL) {
			GetViewForState(mViewState.Prev.ViewStateEnum).InputEnabled = false;
			mDetailView.Show();
		} else if (viewStateEnum == ViewStateEnum.THEATER) {
			mBoardView.Show();
			mBoardView.InputEnabled = true;
			mPassView.Hide();
		} else if (viewStateEnum == ViewStateEnum.MEANDER_HAND) {
			mHandView.Show();
			mBoardView.Hide();
			mDetailView.Hide();
		} else if (viewStateEnum == ViewStateEnum.COMMAND_LIST) {
			mCommandView.Show();
			mBoardView.Hide();
			mDetailView.Hide();
		} else if (viewStateEnum == ViewStateEnum.DESIGNATE_HAND) {
			mHandView.Show();
			mBoardView.Hide();
			mCommandView.Hide();
			mDetailView.Hide();
		} else if (viewStateEnum == ViewStateEnum.DESIGNATE_BOARD) {
			mBoardView.Show();
			mCommandView.Hide();
			mHandView.Hide();
			mDetailView.Hide();
		}
  }

	private bool RegressViewState() {
		if (mViewState.ViewStateEnum == ViewStateEnum.MEANDER_BOARD) {
			return false;
		}

		CallDeferred(nameof(RegressViewStateImpl));
		return true;
	}

	private void RegressViewStateImpl() {
		ViewStateEnum fromViewState = mViewState.ViewStateEnum;
		mViewState = mViewState.Revert();
		GD.Print(
			"Reverted to ViewState: " + mViewState.ViewStateEnum.ToString() + " from " + fromViewState.ToString() + "."
		);

		if (fromViewState == ViewStateEnum.PASS) {
			GetViewForState(mViewState.ViewStateEnum).InputEnabled = true;
			mPassView.Hide();
		} else if (fromViewState == ViewStateEnum.SURRENDER) {
			GetViewForState(mViewState.ViewStateEnum).InputEnabled = true;
			mSurrenderView.Hide();
		} else if (fromViewState == ViewStateEnum.DETAIL) {
			GetViewForState(mViewState.ViewStateEnum).InputEnabled = true;
			mDetailView.Hide();
		} else if (fromViewState == ViewStateEnum.MEANDER_HAND) {
			mHandView.Hide();
			mBoardView.Show();
		} else if (fromViewState == ViewStateEnum.COMMAND_LIST) {
			mCommandView.Hide();
			mBoardView.Show();
		} else if (fromViewState == ViewStateEnum.DESIGNATE_HAND) {
			mHandView.Hide();
			GetViewForState(mViewState.Prev.ViewStateEnum).Show();
		} else if (fromViewState == ViewStateEnum.DESIGNATE_BOARD) {
			mBoardView.Hide();
			GetViewForState(mViewState.Prev.ViewStateEnum).Show();	
		}
	}

	private IView GetViewForState(ViewStateEnum viewStateEnum) {
		if (viewStateEnum == ViewStateEnum.PASS) {
			return mPassView;
		} else if (viewStateEnum == ViewStateEnum.SURRENDER) {
			return mSurrenderView;
		} else if (viewStateEnum == ViewStateEnum.DETAIL) {
			return mDetailView;
		} else if (
			viewStateEnum == ViewStateEnum.MEANDER_BOARD
			|| viewStateEnum == ViewStateEnum.DESIGNATE_BOARD
			|| viewStateEnum == ViewStateEnum.THEATER
		) {
			return mBoardView;
		} else if (
			viewStateEnum == ViewStateEnum.MEANDER_HAND
			|| viewStateEnum == ViewStateEnum.DESIGNATE_HAND
			|| viewStateEnum == ViewStateEnum.CAST_SPELL
		) {
			return mHandView;
		} else if (viewStateEnum == ViewStateEnum.COMMAND_LIST) {
			return mCommandView;
		} else {
			GD.PrintErr("Unknown ViewStateEnum: " + viewStateEnum.ToString() + ".");
			return null;
		}
	}

  class ViewState {
		public readonly ViewState Prev;
		public readonly ViewStateEnum ViewStateEnum;

		private ViewState(ViewStateEnum viewStateEnum, ViewState prev) {
			GD.Print(
				"Creating ViewState for " + viewStateEnum.ToString() + " with previous ViewState: "
				+ prev?.ViewStateEnum.ToString()
			);
			ViewStateEnum = viewStateEnum;
			Prev = prev;

			if (prev.ViewStateEnum == viewStateEnum) {
				GD.PrintErr("The ViewState " + viewStateEnum.ToString() + " is already the current ViewState.");
			}
			if (prev.ViewStateEnum == ViewStateEnum.SURRENDER || prev.ViewStateEnum == ViewStateEnum.DETAIL) {
				GD.PrintErr(
					"The previous ViewState " + prev.ViewStateEnum.ToString() + " is not a valid previous ViewState for "
					+ viewStateEnum.ToString() + "."
				);
				Prev = null;
			}

			switch (viewStateEnum) {
				case ViewStateEnum.MEANDER_BOARD:
					if (prev != null) {
						Prev = null; // should let player undo his last command?
					}
					break;
				case ViewStateEnum.DETAIL:
					if (prev == null) {
						GD.PrintErr("Missing previous ViewState for " + viewStateEnum.ToString() + ".");
					}
					break;
				case ViewStateEnum.PASS:
				case ViewStateEnum.SURRENDER:
				case ViewStateEnum.MEANDER_HAND:
				case ViewStateEnum.COMMAND_LIST:
					if (prev == null) {
						GD.PrintErr("Missing previous ViewState for " + viewStateEnum.ToString() + ".");
					} else if (prev.ViewStateEnum != ViewStateEnum.MEANDER_BOARD) {
						GD.PrintErr(
							"The previous ViewState should be MEANDER_BOARD, but it's " + prev.ViewStateEnum.ToString()
							+ " instead."
						);
					}
					break;
				case ViewStateEnum.DESIGNATE_HAND:
					if (prev == null) {
						GD.PrintErr("Missing previous ViewState for " + viewStateEnum.ToString() + ".");
					} else if (
						prev.ViewStateEnum != ViewStateEnum.COMMAND_LIST
						&& prev.ViewStateEnum != ViewStateEnum.DESIGNATE_BOARD
						&& prev.ViewStateEnum != ViewStateEnum.DESIGNATE_LIST
					) {
						GD.PrintErr(
							"The previous ViewState should be for commanding, but it's " + prev.ViewStateEnum.ToString()
							+ " instead."
						);
					}
					break;
				case ViewStateEnum.DESIGNATE_BOARD:
					if (prev == null) {
						GD.PrintErr("Missing previous ViewState for " + viewStateEnum.ToString() + ".");
					} else if (
						prev.ViewStateEnum != ViewStateEnum.COMMAND_LIST
						&& prev.ViewStateEnum != ViewStateEnum.DESIGNATE_HAND
						&& prev.ViewStateEnum != ViewStateEnum.DESIGNATE_LIST
					) {
						GD.PrintErr(
							"The previous ViewState should be for commanding, but it's " + prev.ViewStateEnum.ToString()
							+ " instead."
						);
					}
					break;
				case ViewStateEnum.DESIGNATE_LIST:
					if (prev == null) {
						GD.PrintErr("Missing previous ViewState for " + viewStateEnum.ToString() + ".");
					} else if (
						prev.ViewStateEnum != ViewStateEnum.COMMAND_LIST
						&& prev.ViewStateEnum != ViewStateEnum.DESIGNATE_HAND
						&& prev.ViewStateEnum != ViewStateEnum.DESIGNATE_BOARD
					) {
						GD.PrintErr("The previous ViewState should be for commanding, but it's " + prev.ViewStateEnum.ToString()
							+ " instead."
						);
					}
					break;
				case ViewStateEnum.THEATER:
					// No checks.
					break;
				default:
					GD.PrintErr("Unknown ViewStateEnum: " + viewStateEnum.ToString() + ".");
					break;
			}
		}

		public ViewState() {
			ViewStateEnum = ViewStateEnum.MEANDER_BOARD;
			GD.Print("Creating initial ViewState for " + ViewStateEnum.ToString() + ".");
			Prev = null;
		}

		public ViewState Append(ViewStateEnum viewStateEnum) {
			return new ViewState(viewStateEnum, this);
		}

		public ViewState Revert() {
			if (Prev == null) {
				GD.PrintErr("Cannot revert to a previous ViewState, as there is no previous ViewState.");
				return this;
			}
			return Prev;
		}
	}

	public static ViewStateEnum DetermineViewStateForTargetTypes(Type[] targetTypes) {
		if (targetTypes.Length == 0) {
			throw new Exception("No target types provided for determining ViewState.");
		} else if (targetTypes.Length == 1) {
			return DetermineViewStateForTargetType(targetTypes[0]);
		} else {
			List<ViewStateEnum> viewStates = [];
			foreach (Type targetType in targetTypes) {
				viewStates.Add(DetermineViewStateForTargetType(targetType));
			}
			if (viewStates.Distinct().Count() == 1) {
				return viewStates[0];
			} else {
				throw new Exception(
					"Multiple differing target types provided for determining ViewState: "
					+ string.Join(", ", targetTypes.Select(t => t.ToString()))
					+ "."
				);
			}
		}
	}

	public static ViewStateEnum DetermineViewStateForTargetType(Type targetType) {
		if (targetType == typeof(Match.Piece)) {
			return ViewStateEnum.DESIGNATE_BOARD;
		} else if (targetType == typeof(Tile)) {
			return ViewStateEnum.DESIGNATE_BOARD;
		} else if (targetType == typeof(Activity)) {
			return ViewStateEnum.DESIGNATE_BOARD;
		} else if (targetType == typeof(Card)) {
			return ViewStateEnum.DESIGNATE_HAND;
		} else if (targetType == typeof(string)) {
			return ViewStateEnum.DESIGNATE_LIST;
		} else if (targetType == typeof(Command)) {
			return ViewStateEnum.COMMAND_LIST;
		} else {
			throw new Exception("Unknown target type " + targetType.ToString() + " for determining ViewState.");
		}
	}

  private static Vector3 GetPlaneIntersection(Vector3 origin, Vector3 direction) {
		Vector3 planeNormal = new(0, 1, 0);
		float t = -planeNormal.Dot(origin) / planeNormal.Dot(direction);
		return origin + direction * t;
  }
}
}
