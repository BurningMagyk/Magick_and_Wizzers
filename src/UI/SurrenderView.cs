using Godot;
using System;

namespace UI {
public partial class SurrenderView : CanvasLayer, IView {
  public delegate SelectTypeEnum SelectItemDelegate(SelectTypeEnum selectTypeEnum);
  public SelectItemDelegate SelectItem;
  public delegate bool GoBackDelegate();
  public GoBackDelegate GoBack;

  public bool Showing { get; private set; }
  public bool InputEnabled { get; set; } = true;

  private PackedScene itemBase;
  private Rect2 viewPortRect;
  private readonly Item[] menuItems = new Item[2];
  private int hoveredItemIndex;

  // Called when the node enters the scene tree for the first time.
  public override void _Ready() {
	  itemBase = ResourceLoader.Load<PackedScene>("res://scenes/surrender_item.tscn");

    // Instantiate items.
		Item itemYes = itemBase.Instantiate<Item>();
		AddChild(itemYes);
    menuItems[0] = itemYes;
    Item itemNo = itemBase.Instantiate<Item>();
    AddChild(itemNo);
    menuItems[1] = itemNo;

    // Set their names.
    itemYes.Name = "Yes";
		itemYes.SetText("Yes");
    itemNo.Name = "No";
    itemNo.SetText("No");

	  hoveredItemIndex = 1;
	  Hide();
  }

  public override void _Input(InputEvent @event) {
    if (!Showing || !InputEnabled) { return; }

    if (Input.IsActionJustPressed("d_left")) {
			menuItems[1].Unhover();
		  menuItems[0].Hover();
      hoveredItemIndex = 0;
		}
		if (Input.IsActionJustPressed("d_right")) {
      menuItems[0].Unhover();
		  menuItems[1].Hover();
      hoveredItemIndex = 1;
		}
    if (Input.IsActionJustPressed("select")) {
      SelectHoveredItem();
    }
    if (Input.IsActionJustPressed("back")) {
			GoBack?.Invoke();
		}
  }

  public new void Show() {
	  base.Show();

    HoverDefaultItem();

    Showing = true;
  }

  public new void Hide() {
		base.Hide();

    Showing = false;
  }

  public void SetViewPortRect(Rect2 viewPortRect) {
    this.viewPortRect = viewPortRect;

    // Position them now that we have the viewport size.
    float yPos = (viewPortRect.Size.Y - menuItems[0].Size.Y) / 2;
		menuItems[0].Position = new Vector2((viewPortRect.Size.X - menuItems[0].Size.X * 3) / 2, yPos);
		menuItems[1].Position = new Vector2((viewPortRect.Size.X + menuItems[1].Size.X) / 2, yPos);
  }

  private void SelectHoveredItem() {
		// Emits signal to call OnSelectItem, defined in UI class.
		SelectItem?.Invoke(hoveredItemIndex == 0 ? SelectTypeEnum.SURRENDER : SelectTypeEnum.ALT);
  }

  private void HoverDefaultItem() {
    hoveredItemIndex = 1;
    menuItems[1].Hover();
  }
}
}
