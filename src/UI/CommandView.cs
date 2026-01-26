using Godot;
using Match;
using System;
using System.Collections.Generic;

namespace UI {
public partial class CommandView : CanvasLayer, IView {
	private const int MAX_COMMAND_COUNT = 5;
	private const int SPACING = 20;

  public bool Showing { get; private set; }
  public bool InputEnabled { get; set; } = true;

  private PackedScene itemBase;
	private Rect2 viewPortRect;
  private readonly Item[] items = new Item[MAX_COMMAND_COUNT];
	private readonly Dictionary<Item, Command> itemCommands = [];
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

  public void Input(UI.InputType inputType, bool press) {
	  if (!Showing || !InputEnabled) { return; }

	  if (inputType == UI.InputType.D_UP && press) {
			HoverItem(Main.DirectionEnum.NORTH);
		}
		if (inputType == UI.InputType.D_DOWN && press) {
			HoverItem(Main.DirectionEnum.SOUTH);
		}
		if (inputType == UI.InputType.SELECT && press) {
			SelectHoveredItem();
		}
		if (inputType == UI.InputType.DETAIL && press) {
			DetailHoveredItem();
		}
		if (inputType == UI.InputType.BACK && press) {
			Select?.Invoke(null, WizardStep.SelectType.BACK);
		}
  }

	public delegate void SelectDelegate(object target, WizardStep.SelectType selectType);
	public SelectDelegate Select;

  public new void Show() {
		// Hover the current item. We expect SetCommandTypes to have been called before Show.
		if (hoveredItemIndex >= currentActor.Commands.Length || hoveredItemIndex == -1) {
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
		int commandCount = actor.Commands.Length;
		if (commandCount > MAX_COMMAND_COUNT) {
			throw new Exception(
				"Amount of command types (" + commandCount + ") exceeds max amount (" + MAX_COMMAND_COUNT
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
			if (i < commandCount) {
				if (itemCommands.TryGetValue(item, out Command prevCommandType)
					&& prevCommandType != actor.Commands[i]
				) {
					// It complains if we try adding the item if it's already been added before.
					itemCommands.Remove(item);
				}
				itemCommands.Add(item, actor.Commands[i]);
				item.Available = true; // This should depend on the piece but for now, make all available.
				
				// Position them according to how many are visible.
				item.Position = new Vector2(
					(viewPortRect.Size.X - items[i].Size.X) / 2,
					(viewPortRect.Size.Y / 2) + (i - commandCount / 2F) * (items[i].Size.Y + SPACING)
				);

				item.Name = actor.Commands[i].ToString();
				item.SetText(actor.Commands[i].ToString());
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
		Select?.Invoke(itemCommands[items[hoveredItemIndex]], WizardStep.SelectType.STANDARD);
  }

	private void DetailHoveredItem() {
		// Emits signal to call OnSelectItem, defined in UI class.
		Select?.Invoke(itemCommands[items[hoveredItemIndex]], WizardStep.SelectType.DETAIL);
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
}
}
