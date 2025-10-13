using Display;
using Godot;
using Match;
using System;
using System.Diagnostics;

namespace UI {
public partial class UI : Node {
  private const float CAMERA_SPEED = 1F;

  private ViewState mViewState = new();
  private BoardView mBoardView;
  private CommandView mCommandView;
  private HandView mHandView;
  private DetailView mDetailView;
  private Camera3D camera;

  private Vector2 mHoverPoint;

	private Match.Piece mActor;
	private Target[] mBoardTargets;
	private Target[] mHandTargets;

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
		mBoardView.SelectMisc += OnSelectMisc;
		mHandView.SelectCard += OnSelectCard;
		mHandView.GoBack += RegressViewState;
		mCommandView.SelectCommand += OnSelectCommand;
		mCommandView.GoBack += RegressViewState;
		mDetailView.SelectItem += OnSelectItem; // doing this in DETAIL only for spook purposes
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

	/// <summary> Called from BoardView using SelectPiece?.Invoke </summary>
	/// <returns> True if the selection was successful </returns>
  private bool OnSelectPiece(Display.Piece piece, Command command, SelectTypeEnum selectTypeEnum) {
		if (mViewState.ViewStateEnum == ViewStateEnum.MEANDER_BOARD) {
			mActor = piece.GamePiece;
			ProgressViewState(ViewStateEnum.COMMAND_LIST);
			return true;
		} else if (mViewState.ViewStateEnum == ViewStateEnum.DESIGNATE_BOARD) {
			return true;
		}
		return false;
  }

	/// <summary> Called from BoardView using SelectTile?.Invoke </summary>
	/// <returns> True if the selection was successful </returns>
  private bool OnSelectTile(ITile tile, Command command, SelectTypeEnum selectTypeEnum) {
		if (mViewState.ViewStateEnum == ViewStateEnum.MEANDER_BOARD) {
			return true;
		} else if (mViewState.ViewStateEnum == ViewStateEnum.DESIGNATE_BOARD) {
			return true;
		}
		return false;
  }

	/// <summary> Called from BoardView using SelectActivity?.Invoke </summary>
	/// <returns> True if the selection was successful </returns>
  private bool OnSelectActivity(Match.Activity activity) {
		if (mViewState.ViewStateEnum == ViewStateEnum.DESIGNATE_BOARD) {
			return true;
		}
		return false;
  }

	/// <summary> Called from HandView using SelectCard?.Invoke </summary>
	/// <returns> True if the selection was successful </returns>
  private bool OnSelectCard(Card card, SelectTypeEnum selectTypeEnum) {
		if (mViewState.ViewStateEnum == ViewStateEnum.MEANDER_HAND) {
			ProgressViewState(ViewStateEnum.DETAIL);
			return true;
		} else if (mViewState.ViewStateEnum == ViewStateEnum.COMMAND_HAND) {
			if (selectTypeEnum == SelectTypeEnum.DETAIL) {
				ProgressViewState(ViewStateEnum.DETAIL);
			} else {
				// Go either to DESIGNATE_HAND or DESIGNATE_BOARD.
			}
			return true;
		}
		return false;
  }

	/// <summary> Called from CommandView using SelectCommand?.Invoke </summary>
	/// <returns> True if the selection was successful </returns>
  private bool OnSelectCommand(Command command, SelectTypeEnum selectTypeEnum) {
		if (mViewState.ViewStateEnum == ViewStateEnum.COMMAND_LIST) {
			if (selectTypeEnum == SelectTypeEnum.DETAIL) {
				ProgressViewState(ViewStateEnum.DETAIL);
			} else {
				// Go either to COMMAND_HAND or DESIGNATE_HAND or DESIGNATE_BOARD or DESIGNATE_LIST or back to MEANDER_BOARD.
				Command steppedCommand = command.StepView();
				if (steppedCommand.ViewSteps.Length == 0) {
					ProgressViewState(ViewStateEnum.MEANDER_BOARD);
				} else if (steppedCommand.ViewSteps[0] == ViewStateEnum.COMMAND_HAND) {
					mHandView.SetCommand(steppedCommand);
					ProgressViewState(ViewStateEnum.COMMAND_HAND);
				} else if (steppedCommand.ViewSteps[0] == ViewStateEnum.DESIGNATE_HAND) {
					mHandView.SetCommand(steppedCommand);
					ProgressViewState(ViewStateEnum.DESIGNATE_HAND);
				} else if (steppedCommand.ViewSteps[0] == ViewStateEnum.DESIGNATE_BOARD) {
					mBoardView.SetCommand(steppedCommand);
					ProgressViewState(ViewStateEnum.DESIGNATE_BOARD);
				} else {
					throw new Exception("Command has invalid step " + steppedCommand.ViewSteps[0].ToString() + ".");
				}
			}
			return true;
		}
		return false;
  }

	/// <summary> Called from DetailView using SelectItem?.Invoke </summary>
	/// <returns> True if the selection was successful </returns>
  private bool OnSelectItem() {
		if (mViewState.ViewStateEnum == ViewStateEnum.SURRENDER) {
			return true;
		}
		return false;
  }

	/// <summary> Called from anywhere using SelectMisc?.Invoke </summary>
	/// <returns> True if the selection was successful </returns>
	private bool OnSelectMisc(SelectTypeEnum selectTypeEnum) {
		if (selectTypeEnum == SelectTypeEnum.ALT) {
			if (mViewState.ViewStateEnum == ViewStateEnum.MEANDER_BOARD) {
				ProgressViewState(ViewStateEnum.MEANDER_HAND);
				return true;
			}
		} else if (selectTypeEnum == SelectTypeEnum.SURRENDER) {
			if (mViewState.ViewStateEnum == ViewStateEnum.MEANDER_BOARD) {
				ProgressViewState(ViewStateEnum.SURRENDER);
				return true;
			}
		} else if (selectTypeEnum == SelectTypeEnum.FINAL) {
			PassRound?.Invoke();
		}
		return false;
	}

	private void ProgressViewState(ViewStateEnum viewStateEnum) {
		CallDeferred(nameof(ProgressViewStateImpl), (int) viewStateEnum);
	}

  private void ProgressViewStateImpl(int viewStateEnumInt) {
		ViewStateEnum viewStateEnum = (ViewStateEnum) viewStateEnumInt;

		mViewState = mViewState.Append(viewStateEnum);

		if (viewStateEnum == ViewStateEnum.DETAIL) {
			GetViewForState(mViewState.Prev.ViewStateEnum).InputEnabled = false;
			mDetailView.Show();
		} else if (viewStateEnum == ViewStateEnum.SURRENDER) {
			// mSurrenderView.Show();
		} else if (viewStateEnum == ViewStateEnum.MEANDER_HAND) {
			mHandView.Show();
			mBoardView.Hide();
		} else if (viewStateEnum == ViewStateEnum.COMMAND_LIST) {
			mCommandView.SetActor(mActor);
			mCommandView.Show();
			mBoardView.Hide();
		} else if (viewStateEnum == ViewStateEnum.COMMAND_HAND) {
			mCommandView.Show();
			mHandView.Hide();
		} else if (viewStateEnum == ViewStateEnum.DESIGNATE_HAND) {
			mHandView.Show();
			mCommandView.Hide();
		} else if (viewStateEnum == ViewStateEnum.DESIGNATE_BOARD) {
			mBoardView.Show();
			mCommandView.Hide();
			mHandView.Hide();
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

		if (fromViewState == ViewStateEnum.DETAIL) {
			GetViewForState(mViewState.ViewStateEnum).InputEnabled = true;
			mDetailView.Hide();
		} else if (fromViewState == ViewStateEnum.SURRENDER) {
			// mSurrenderView.Hide();
		} else if (fromViewState == ViewStateEnum.MEANDER_HAND) {
			mHandView.Hide();
			mBoardView.Show();
		} else if (fromViewState == ViewStateEnum.COMMAND_LIST) {
			mCommandView.Hide();
			mBoardView.Show();
		} else if (fromViewState == ViewStateEnum.COMMAND_HAND) {
			mHandView.Hide();
			mCommandView.Show();
		} else if (
			fromViewState == ViewStateEnum.DESIGNATE_HAND
			&& mViewState.ViewStateEnum == ViewStateEnum.COMMAND_LIST
		) {
			mHandView.Hide();
			mCommandView.Show();
		} else if (fromViewState == ViewStateEnum.DESIGNATE_BOARD) {
			mBoardView.Hide();
			if (mViewState.ViewStateEnum == ViewStateEnum.COMMAND_LIST) {
				mCommandView.Show();
			} else { // mViewState.ViewStateEnum == ViewStateEnum.DESIGNATE_HAND || ViewStateEnum.COMMAND_HAND
				mHandView.Show();
			}			
		}
	}

	private IView GetViewForState(ViewStateEnum viewStateEnum) {
		if (viewStateEnum == ViewStateEnum.DETAIL) {
			return mDetailView;
		} else if (viewStateEnum == ViewStateEnum.SURRENDER) {
			return null; // Surrender view is not implemented yet.
		} else if (viewStateEnum == ViewStateEnum.MEANDER_BOARD || viewStateEnum == ViewStateEnum.DESIGNATE_BOARD) {
			return mBoardView;
		} else if (
			viewStateEnum == ViewStateEnum.MEANDER_HAND
			|| viewStateEnum == ViewStateEnum.COMMAND_HAND
			|| viewStateEnum == ViewStateEnum.DESIGNATE_HAND
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
			GD.Print("Creating ViewState for " + viewStateEnum.ToString() + " with previous ViewState: " + prev?.ViewStateEnum.ToString());
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
						GD.PrintErr(prev + "There should be no previous ViewState " + prev + " for MEANDER_BOARD.");
						Prev = null;
					}
					break;
				case ViewStateEnum.DETAIL:
					if (prev == null) {
						GD.PrintErr("Missing previous ViewState for " + viewStateEnum.ToString() + ".");
					}
					break;
				case ViewStateEnum.SURRENDER:
				case ViewStateEnum.MEANDER_HAND:
				case ViewStateEnum.COMMAND_LIST:
					if (prev == null) {
						GD.PrintErr("Missing previous ViewState for " + viewStateEnum.ToString() + ".");
					} else if (prev.ViewStateEnum != ViewStateEnum.MEANDER_BOARD) {
						GD.PrintErr(
							"The previous ViewState should be MEANDER_BOARD, but it's " + prev.ViewStateEnum.ToString() + " instead."
						);
					}
					break;
				case ViewStateEnum.COMMAND_HAND:
					if (prev == null) {
						GD.PrintErr("Missing previous ViewState for " + viewStateEnum.ToString() + ".");
					} else if (prev.ViewStateEnum != ViewStateEnum.COMMAND_LIST) {
						GD.PrintErr(
							"The previous ViewState should be COMMAND_LIST, but it's " + prev.ViewStateEnum.ToString() + " instead."
						);
					}
					break;
				case ViewStateEnum.DESIGNATE_HAND:
					if (prev == null) {
						GD.PrintErr("Missing previous ViewState for " + viewStateEnum.ToString() + ".");
					} else if (
						prev.ViewStateEnum != ViewStateEnum.COMMAND_HAND
						&& prev.ViewStateEnum != ViewStateEnum.COMMAND_LIST
					) {
						GD.PrintErr(
							"The previous ViewState should be DESIGNATE_HAND, but it's " + prev.ViewStateEnum.ToString() + " instead."
						);
					}
					break;
				case ViewStateEnum.DESIGNATE_BOARD:
					if (prev == null) {
						GD.PrintErr("Missing previous ViewState for " + viewStateEnum.ToString() + ".");
					} else if (
						prev.ViewStateEnum != ViewStateEnum.DESIGNATE_HAND
						&& prev.ViewStateEnum != ViewStateEnum.COMMAND_HAND
						&& prev.ViewStateEnum != ViewStateEnum.COMMAND_LIST) {
						GD.PrintErr(
							"The previous ViewState should be COMMAND_HAND or COMMAND_LIST, but it's " + prev.ViewStateEnum.ToString() + " instead."
						);
					}
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

  private static Vector3 GetPlaneIntersection(Vector3 origin, Vector3 direction) {
		Vector3 planeNormal = new Vector3(0, 1, 0);
		float t = -planeNormal.Dot(origin) / planeNormal.Dot(direction);
		return origin + direction * t;
  }
}
}
