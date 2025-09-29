using Godot;
using Match;
using System;

namespace UI {
public partial class CommandView : CanvasLayer, IView {
  public delegate bool SelectCommandDelegate(Command command, SelectTypeEnum selectTypeEnum);
  public SelectCommandDelegate SelectCommand;
  public delegate bool GoBackDelegate();
  public GoBackDelegate GoBack;

  public bool Showing { get; private set; }
  public bool InputEnabled { get; set; } = true;

  private PackedScene commandItemBase;
  private CommandItem[] commandItems;
  private int hoveredItemIndex;

  // Called when the node enters the scene tree for the first time.
  public override void _Ready() {
	  commandItemBase = ResourceLoader.Load<PackedScene>("res://scenes/command_item.tscn");

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
			SelectHoveredItem(false);
		}
		if (Input.IsActionJustPressed("detail")) {
			SelectHoveredItem(true);
		}
	if (Input.IsActionJustPressed("back")) {
	  GoBack?.Invoke();
	}
  }

  public new void Show() {
	  base.Show();
	  Showing = true;
	}

  public new void Hide() {
		base.Hide();
		Showing = false;
	}

  public void SetViewPortRect(Rect2 viewPortRect) {

  }

  public void Unhover(bool forgetIndex = true) {
		if (hoveredItemIndex > -1 && commandItems[hoveredItemIndex] != null) {
			commandItems[hoveredItemIndex].Unhover();
		}
		if (forgetIndex) {
			hoveredItemIndex = -1;
		}
  }

  public void SetCommandTypes(Piece piece) {
	commandItems = new CommandItem[piece.CommandTypes.Length];
	for (int i = 0; i < piece.CommandTypes.Length; i++) {
	  Command.CommandType commandType = piece.CommandTypes[i];
	  CommandItem commandItem = commandItemBase.Instantiate<CommandItem>();
	  commandItem.CommandType = commandType;
	  commandItem.Available = true;
	  commandItem.Position = new Vector2(0, i * (commandItem.Size.Y + 5));
	  AddChild(commandItem);
	  commandItems[i] = commandItem;
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

  private void SelectHoveredItem(bool forDetail) {
	// Emits signal to call OnSelectItem, defined in UI class.
	SelectCommand?.Invoke(itemToCommand(commandItems[hoveredItemIndex]), forDetail ? SelectTypeEnum.DETAIL : SelectTypeEnum.FINAL);
  }

  private int GetUppermostItemIndex(int startIndex = 0) {
	  for (int i = startIndex; i < commandItems.Length; i++) {
		if (commandItems[i] != null) {
		  return i;
		}
	  }
	  return -1;
	}
	private int GetLowermostItemIndex(int startIndex) {
	  for (int i = startIndex; i >= 0; i--) {
		if (commandItems[i] != null) {
		  return i;
		}
	  }
	  return -1;
  }
  private int GetLowermostItemIndex() {
	  return GetLowermostItemIndex(commandItems.Length - 1);
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

  private static Command itemToCommand(CommandItem item) {
	return item.CommandType switch {
	  Command.CommandType.APPROACH => Command.Approach(null, 0, -1),
	  Command.CommandType.AVOID => Command.Avoid(null, 0, -1),
	  Command.CommandType.INTERACT => Command.Interact(null, -1),
	  _ => throw new Exception("Unknown command type " + item.CommandType + " in CommandView.itemToCommand"),
	};
  }
}
}
