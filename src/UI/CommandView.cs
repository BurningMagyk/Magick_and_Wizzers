using Godot;
using Match;
using System;
using System.Collections.Generic;

namespace UI {
public partial class CommandView : CanvasLayer, IView {
	private const int MAX_COMMAND_COUNT = 5;
	private const int SPACING = 20;

  public delegate SelectTypeEnum SelectCommandDelegate(Command command, SelectTypeEnum selectTypeEnum);
  public SelectCommandDelegate SelectCommand;
  public delegate bool GoBackDelegate();
  public GoBackDelegate GoBack;

  public bool Showing { get; private set; }
  public bool InputEnabled { get; set; } = true;

  private PackedScene itemBase;
	private Rect2 viewPortRect;
  private readonly Item[] items = new Item[MAX_COMMAND_COUNT];
	private int commandCountSupposed;
	private readonly Dictionary<Item, Command.CommandType> itemCommandTypes = [];
  private readonly Dictionary<Piece, int> hoveredItemIndexPerPiece = [];
	private Piece currentActor;
	private int hoveredItemIndex;

  // Called when the node enters the scene tree for the first time.
  public override void _Ready() {
	  itemBase = ResourceLoader.Load<PackedScene>("res://scenes/command_item.tscn");

		// Instantiate max number of items.
		for (int i = 0; i < MAX_COMMAND_COUNT; i++) {
			Item item = itemBase.Instantiate<Item>();
			AddChild(item);
			items[i] = item;
		}
		
		hoveredItemIndex = -1;
	  Hide();
  }

  public override void _Input(InputEvent @event) {
	  if (!Showing || !InputEnabled) { return; }

	  if (Input.IsActionJustPressed("d_up")) {
			HoverItem(Main.DirectionEnum.NORTH);
		}
		if (Input.IsActionJustPressed("d_down")) {
			HoverItem(Main.DirectionEnum.SOUTH);
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
			items[hoveredItemIndex].Hover();
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
		Command.CommandType[] availableCommandTypes = actor.AvailableCommandTypes;
		commandCountSupposed = availableCommandTypes.Length;
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
			Item item = items[i];
			if (i < commandCountSupposed) {
				if (itemCommandTypes.TryGetValue(item, out Command.CommandType prevCommandType)
					&& prevCommandType != availableCommandTypes[i]
				) {
					// It complains if we try adding the item if it's already been added before.
					itemCommandTypes.Remove(item);
				}
				itemCommandTypes.Add(item, availableCommandTypes[i]);
				item.Available = true; // This should depend on the piece but for now, make all available.
				
				// Position them according to how many are visible.
				item.Position = new Vector2(
					(viewPortRect.Size.X - items[i].Size.X) / 2,
					(viewPortRect.Size.Y / 2) + (i - availableCommandTypes.Length / 2F) * (items[i].Size.Y + SPACING)
				);

				item.Name = availableCommandTypes[i].ToString();
				item.SetText(availableCommandTypes[i].ToString());
				item.Show();
			} else {
				item.Hide();
			}
		}
  }

  public void Unhover(bool forgetIndex = true) {
		if (hoveredItemIndex > -1 && items[hoveredItemIndex] != null) {
			items[hoveredItemIndex].Unhover();
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
		} else if (direction == Main.DirectionEnum.NORTH) {
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
			} else if (direction == Main.DirectionEnum.SOUTH) {
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

		Item hoveredItem = items[hoveredItemIndex];
		if (hoveredItemIndexPrevious >= 0) {
			items[hoveredItemIndexPrevious].Unhover();
		}

		// Place the sleeve over the hovered item.
		hoveredItem.Hover();
  }

  private void SelectHoveredItem() {
		// Emits signal to call OnSelectItem, defined in UI class.
		SelectCommand?.Invoke(ItemToCommand(items[hoveredItemIndex]), SelectTypeEnum.FINAL);
  }

	private void DetailHoveredItem() {
		// Emits signal to call OnSelectItem, defined in UI class.
		SelectCommand?.Invoke(ItemToCommand(items[hoveredItemIndex]), SelectTypeEnum.DETAIL);
	}

  private int GetUppermostItemIndex(int startIndex = 0) {
	  for (int i = startIndex; i < items.Length; i++) {
			if (items[i].Visible) {
				return i;
			}
	  }
	  return -1;
	}
	private int GetLowermostItemIndex(int startIndex = MAX_COMMAND_COUNT - 1) {
	  for (int i = startIndex; i >= 0; i--) {
			if (items[i].Visible) {
				return i;
			}
	  }
	  return -1;
  }

  private int GetAvailableItemsCount() {
	int availableCount = 0;
		for (int i = 0; i < items.Length; i++) {
			if (items[i].Available) {
			availableCount++;
			}
		}
	return availableCount;
  }

  private Command ItemToCommand(Item item) {
		return itemCommandTypes[item] switch {
			// These need to be using range specified by other items in this view, leave it like this for now.
			Command.CommandType.APPROACH => Command.Approach(currentActor, Command.RangeEnum.ADJACENT),
			Command.CommandType.AVOID => Command.Avoid(currentActor, Command.RangeEnum.FAR),
			Command.CommandType.INTERACT => Command.Interact(currentActor),
			_ => throw new Exception("Unknown command type " + itemCommandTypes[item] + " in CommandView.itemToCommand"),
		};
  }
}
}
