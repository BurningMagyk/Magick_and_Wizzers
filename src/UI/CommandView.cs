using Godot;
using Match;
using System;
using System.Collections.Generic;

namespace UI {
public partial class CommandView : CanvasLayer, IView {
	private const int MAX_COMMAND_COUNT = 5;
	private const int SPACING = 20;

  public delegate bool SelectCommandDelegate(Command command, SelectTypeEnum selectTypeEnum);
  public SelectCommandDelegate SelectCommand;
  public delegate bool GoBackDelegate();
  public GoBackDelegate GoBack;

  public bool Showing { get; private set; }
  public bool InputEnabled { get; set; } = true;

  private PackedScene commandItemBase;
	private Rect2 viewPortRect;
  private readonly CommandItem[] commandItems = new CommandItem[MAX_COMMAND_COUNT];
	private int commandCountSupposed;
  private readonly Dictionary<Piece, int> hoveredItemIndexPerPiece = [];
	private Piece currentActor;
	private int hoveredItemIndex;

  // Called when the node enters the scene tree for the first time.
  public override void _Ready() {
	  commandItemBase = ResourceLoader.Load<PackedScene>("res://scenes/command_item.tscn");

		// Instantiate max number of command items.
		for (int i = 0; i < MAX_COMMAND_COUNT; i++) {
			CommandItem commandItem = commandItemBase.Instantiate<CommandItem>();
			AddChild(commandItem);
			commandItems[i] = commandItem;
		}
		
	  // Make the base stuff invisible.
	  GetNode<CommandItem>("Command Item").Visible = false;
		hoveredItemIndex = -1;
	  Hide();
  }

  public override void _Input(InputEvent @event) {
	  if (!Showing) { return; }

	  if (Input.IsActionJustPressed("d_up")) {
			HoverItem(Main.DirectionEnum.UP);
		}
		if (Input.IsActionJustPressed("d_down")) {
			HoverItem(Main.DirectionEnum.DOWN);
		}
		if (Input.IsActionJustPressed("select")) {
			SelectHoveredItem();
		}
		if (Input.IsActionJustPressed("detail")) {
			DetailHoveredItem();
		}
	if (Input.IsActionJustPressed("back")) {
	  GoBack?.Invoke();
	}
  }

  public new void Show() {
		// Hover the current item. We expect SetCommandTypes to have been called before Show.
		if (hoveredItemIndex >= commandCountSupposed || hoveredItemIndex == -1) {
			HoverItem(Main.DirectionEnum.NONE);
		} else {
			commandItems[hoveredItemIndex].Hover();
		}

	  base.Show();
	  Showing = true;
	}

  public new void Hide() {
		base.Hide();
		Showing = false;

		// Remember the hovered item index for this piece as we leave.
		if (currentActor != null) {
			hoveredItemIndexPerPiece[currentActor] = hoveredItemIndex;
		}
		currentActor = null;
		Unhover();
	}

  public void SetViewPortRect(Rect2 viewPortRect) {
		this.viewPortRect = viewPortRect;
  }

	public void SetActor(Piece actor) {
		Command.CommandType[] commandTypes = actor.CommandTypes;
		commandCountSupposed = commandTypes.Length;
		if (commandCountSupposed > MAX_COMMAND_COUNT) {
			throw new Exception(
				"Amount of command types (" + commandCountSupposed + ") exceeds max amount (" + MAX_COMMAND_COUNT
				+ ") for CommandView"
			);
		}

		currentActor = actor;
		if (hoveredItemIndexPerPiece.TryGetValue(actor, out int value)) {
			hoveredItemIndex = value;
		} else {
			hoveredItemIndex = -1;
		}

		for (int i = 0; i < MAX_COMMAND_COUNT; i++) {
			CommandItem commandItem = commandItems[i];
			if (i < commandCountSupposed) {
				commandItem.CommandType = commandTypes[i];
				commandItem.Available = true; // This should depend on the piece but for now, make all available.
				
				// Position them according to how many are visible.
				commandItem.Position = new Vector2(
					(viewPortRect.Size.X - commandItems[i].Size.X) / 2,
					(viewPortRect.Size.Y / 2) + (i - commandTypes.Length / 2F) * (commandItems[i].Size.Y + SPACING)
				);

				commandItem.Name = commandTypes[i].ToString();
				commandItem.SetText(commandTypes[i].ToString());
				commandItem.Show();
			} else {
				commandItem.Hide();
			}
		}
  }

  public void Unhover(bool forgetIndex = true) {
		if (hoveredItemIndex > -1 && commandItems[hoveredItemIndex] != null) {
			commandItems[hoveredItemIndex].Unhover();
		}
		if (forgetIndex) {
			hoveredItemIndex = -1;
		}
  }

  private void HoverItem(Main.DirectionEnum direction) {
		int hoveredItemIndexPrevious = hoveredItemIndex;

		if (direction == Main.DirectionEnum.NONE || GetAvailableItemsCount() == 0) {
			// Intend to unhover or no items available.
			Unhover();
			return;
		} else if (direction == Main.DirectionEnum.UP) {
			if (hoveredItemIndex == -1 || hoveredItemIndex == GetUppermostItemIndex()) {
					// Start sleeve at bottom end.
					hoveredItemIndex = GetLowermostItemIndex();
				} else {
					// Move sleeve up.
					hoveredItemIndex = GetLowermostItemIndex(hoveredItemIndex - 1);
				}
				if (hoveredItemIndex == -1) {
					Unhover();
					return;
				}
			} else if (direction == Main.DirectionEnum.DOWN) {
				if (hoveredItemIndex == -1 || hoveredItemIndex == GetLowermostItemIndex()) {
					// Start sleeve at top end.
					hoveredItemIndex = GetUppermostItemIndex();
				} else {
					// Move sleeve down.
					hoveredItemIndex = GetUppermostItemIndex(hoveredItemIndex + 1);
				}
				if (hoveredItemIndex == -1) {
					Unhover();
					return;
				}
		}

		CommandItem hoveredItem = commandItems[hoveredItemIndex];
		if (hoveredItemIndexPrevious >= 0) {
			commandItems[hoveredItemIndexPrevious].Unhover();
		}

		// Place the sleeve over the hovered item.
		hoveredItem.Hover();
  }

  private void SelectHoveredItem() {
		// Emits signal to call OnSelectItem, defined in UI class.
		SelectCommand?.Invoke(ItemToCommand(commandItems[hoveredItemIndex]), SelectTypeEnum.FINAL);
  }

	private void DetailHoveredItem() {
		// Emits signal to call OnSelectItem, defined in UI class.
		SelectCommand?.Invoke(ItemToCommand(commandItems[hoveredItemIndex]), SelectTypeEnum.DETAIL);
	}

  private int GetUppermostItemIndex(int startIndex = 0) {
	  for (int i = startIndex; i < commandItems.Length; i++) {
			if (commandItems[i].Visible) {
				return i;
			}
	  }
	  return -1;
	}
	private int GetLowermostItemIndex(int startIndex = MAX_COMMAND_COUNT - 1) {
	  for (int i = startIndex; i >= 0; i--) {
			if (commandItems[i].Visible) {
				return i;
			}
	  }
	  return -1;
  }

  private int GetAvailableItemsCount() {
	int availableCount = 0;
		for (int i = 0; i < commandItems.Length; i++) {
			if (commandItems[i].Available) {
			availableCount++;
			}
		}
	return availableCount;
  }

  private static Command ItemToCommand(CommandItem item) {
	return item.CommandType switch {
	  Command.CommandType.APPROACH => Command.Approach(null, 0, -1),
	  Command.CommandType.AVOID => Command.Avoid(null, 0, -1),
	  Command.CommandType.INTERACT => Command.Interact(null, -1),
	  _ => throw new Exception("Unknown command type " + item.CommandType + " in CommandView.itemToCommand"),
	};
  }
}
}
